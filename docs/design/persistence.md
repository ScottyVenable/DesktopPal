# Persistence: Save Path, Schema Versioning, and Recovery

Status: Draft
Owner: Sol (implementation), Jesse (doc steward)
Related: TRITIUM-PLAN §5 "Persistence and data integrity", Issue #15
Milestone: Tritium-0 Structural Reset

## Goals

- Move persisted state out of the install directory to a per-user,
  writable location appropriate for a packaged Windows application.
- Introduce an explicit schema version on every persisted file so future
  shape changes can migrate forward without data loss.
- Make every save atomic: a crash, power loss, or write error must never
  leave a corrupted save in place.
- Keep at least one rolling backup so a corrupt or rejected save can be
  recovered without losing the entire pet history.
- Define an explicit, observable failure model so the app and the user
  both know when persistence has degraded.
- Keep load and save fast enough to be invisible (target < 50 ms on
  typical hardware).

## Non-Goals

- Cloud sync. DesktopPal is local-first by pillar.
- Encryption-at-rest. The save contains no secrets and no PII beyond the
  pet name and chosen hotkey. File-system permissions are sufficient.
- A general-purpose database. JSON files remain the storage format.
- Multi-process safety. Only one DesktopPal instance is expected to run
  per user session; we detect and refuse a second instance instead of
  coordinating writes.
- Save compression. Files are small (< 50 KB projected even at horizon).

## Current State

`DesktopPal/PetState.cs`:

```csharp
private static string SavePath =>
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "pet_state.json");

public static PetState Load()
{
    if (File.Exists(SavePath))
    {
        string json = File.ReadAllText(SavePath);
        var state = JsonSerializer.Deserialize<PetState>(json);
        ...
    }
    return new PetState();
}

public void Save()
{
    try
    {
        LastSeen = DateTime.Now;
        string json = JsonSerializer.Serialize(this, ...);
        File.WriteAllText(SavePath, json);
    }
    catch { /* Ignore for now */ }
}
```

Concrete problems:

- **Save location is the install directory.** Once the app is packaged
  (MSIX, installer, Program Files), the install directory is read-only
  for normal users. Saves will silently fail.
- **No schema version field.** Adding a new property today is tolerated
  (default value), but any rename, retype, or split is a breaking change
  with no migration story.
- **Non-atomic write.** `File.WriteAllText` truncates then writes. A
  crash mid-write produces a zero-byte or partially-written file, and
  next launch deserializes garbage or throws.
- **Silent catch.** `catch { }` hides every error, including disk-full,
  permission-denied, antivirus interference, and JSON serialization bugs.
  The user has no signal that their pet state is no longer being saved.
- **No backup.** A single corrupt write loses everything from birth.
- **Load throws on bad JSON.** `JsonSerializer.Deserialize` propagates
  to the caller, which treats it as a fatal startup error instead of
  falling back to a recovery path.

## Proposed Design

### Save path

Primary save root:

```
%LOCALAPPDATA%\DesktopPal\
```

Resolved via `Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)`
plus `"DesktopPal"`.

Why `LOCALAPPDATA` over `APPDATA` (Roaming):

- DesktopPal is a Windows-specific desktop overlay tied to a particular
  machine's screen layout, hotkey conflicts, and decoration positions.
  Roaming this state to other machines is misleading.
- Save sizes are small but write frequency is non-trivial; roaming
  profile sync is unnecessary network and disk cost.
- Standard guidance for non-roaming local app state.

Layout under the root:

```
%LOCALAPPDATA%\DesktopPal\
├── saves\
│   ├── pet.json              ← canonical save
│   ├── pet.json.bak          ← previous good save (rolling)
│   └── pet.json.tmp          ← in-flight write target (transient)
├── logs\
│   └── desktoppal.log        ← diagnostics (out of scope here)
└── settings\
    └── ui.json               ← UI prefs that aren't pet state (future)
```

The `saves\` directory is created on first launch with default ACLs
(user-writable). The app refuses to start if it can't create or write to
this directory and surfaces a first-run error UI.

Migration of legacy saves: on first launch after this change, if a
`pet_state.json` exists next to the executable and no `saves\pet.json`
exists, copy it forward and rename the legacy file to
`pet_state.json.migrated`. The legacy file is never read again.

**Implementation status (path-only migration):** a first-pass version of
the legacy migration shipped in PR #48 (branch
`feat/sol-persistence-localappdata`, closes #44). It copies legacy
`pet_state.json` from next to the .exe to`%LOCALAPPDATA%\DesktopPal\pet_state.json` (the
flat path used by the current schema-1 save) and best-effort deletes the
legacy file on success. Migration is wrapped in try/catch and logs every
outcome via `Logging`. The full design above — `saves\pet.json` plus
`.bak` / `.tmp` / `quarantine\` layout, schema-version wrapper, atomic
write, and recovery flow — is still pending and tracked in the
persistence epic; this PR is a path-only move so packaged installs can
write at all.

### File format and schema version

Every persisted JSON file gains a wrapper:

```json
{
  "schema": "desktoppal.pet/1",
  "schemaVersion": 1,
  "savedAtUtc": "2025-01-15T03:42:11Z",
  "appVersion": "0.5.0",
  "data": {
    "name": "Buddy",
    "birthTime": "...",
    "hunger": 87.4,
    ...
  }
}
```

Rules:

- `schema` is a stable identifier per file kind (`desktoppal.pet`,
  `desktoppal.world`, etc.). It never changes meaning.
- `schemaVersion` is a monotonically increasing integer per `schema`.
- Loaders dispatch on `schemaVersion` and run a chain of pure migration
  functions (`v1 → v2 → v3`) before deserializing into the current
  domain types.
- Unknown future versions are not silently accepted: a forward-version
  save triggers a "this save is from a newer DesktopPal" recovery flow
  rather than a deserialization error.

The current code shape (a flat `PetState` object) becomes
`schemaVersion: 1`. No data migration is required for the first cut;
future versions wrap accordingly.

### Atomic write

```
1. Serialize state to bytes.
2. Write bytes to saves\pet.json.tmp.
3. Flush + fsync (FileStream.Flush(true)).
4. If saves\pet.json exists, rename it to saves\pet.json.bak
   (atomic same-volume rename, replacing any prior .bak).
5. Rename saves\pet.json.tmp to saves\pet.json.
6. Delete any older orphan .tmp files.
```

Steps 4 and 5 use `File.Replace` where possible (single-call replace +
backup), falling back to `File.Move(..., overwrite: true)` for clarity.
On the same volume these renames are atomic at the filesystem level.

Properties this guarantees:

- A crash before step 5 leaves the previous good file in place.
- A crash between steps 4 and 5 leaves `.bak` as the previous file and
  no canonical file; load uses recovery path A (see below).
- A power loss after step 3 but before step 4 leaves a complete `.tmp`
  alongside the old canonical; load ignores `.tmp` files.

### Save cadence

- Driven by the simulation engine, not a UI timer (see
  `simulation-runtime.md`).
- A "dirty" flag is raised whenever `PetStats` or persisted profile
  fields change.
- Once per minute, if dirty, the engine requests `SaveService.Save()`.
- An explicit save fires on app shutdown and on hatch/level-up
  milestones regardless of cadence.
- Saves are coalesced: if a save is already in flight, additional
  requests mark "save again when done" rather than queueing.

### Recovery model

Load flow:

1. Try to load `saves\pet.json`.
   - If file missing → go to step 2.
   - If parse OK and `schemaVersion` known → migrate forward, return
     state. **Happy path.**
   - If parse fails or schema unknown → go to step 2 with a recorded
     `LoadFailureReason`.

2. Try to load `saves\pet.json.bak`.
   - If parse OK → return state. Mark the session as "recovered from
     backup" and surface a non-blocking notice in the UI.
   - If missing or parse fails → go to step 3.

3. Quarantine and start fresh.
   - Move any unparseable file to
     `saves\quarantine\pet.json.<UTC-timestamp>` for later inspection.
   - Return a brand-new `PetState` with `IsHatched = false`.
   - Surface a clear, blocking first-run-style dialog: "DesktopPal could
     not load your previous save and started a new pet. Your old save
     was preserved in <path>."

Forward-version case (save's `schemaVersion` is higher than supported):

- Do **not** quarantine. Refuse to write. Show a "this save is from a
  newer version of DesktopPal; please update" message and exit cleanly.
  Overwriting would destroy the user's real save.

### Failure modes and observability

| Failure                              | Behavior                                      | User signal                          |
| ------------------------------------ | --------------------------------------------- | ------------------------------------ |
| Save dir not creatable               | Refuse to start                               | First-run error dialog               |
| Save dir not writable mid-session    | Switch to "persistence degraded" mode         | Drawer status badge + log entry      |
| Atomic rename fails                  | Retry once; on second failure, degrade        | Same as above                        |
| Disk full                            | Degrade; do not crash                         | Same as above                        |
| Antivirus locks the file             | Retry with backoff; degrade after N failures  | Same as above                        |
| Save parse fails on load             | Fall back to .bak; if also fails, quarantine  | Recovery notice                      |
| Forward-version save                 | Refuse to write, exit                         | Update-required dialog               |

"Persistence degraded" mode keeps the app running so the user does not
lose their session, but every state change happens in memory only. The
UI shows a persistent badge on the drawer until the next successful
save.

All persistence outcomes — success, retry, degrade, recover, quarantine —
emit structured log lines via the diagnostics service so they are
visible in `logs\desktoppal.log` and to support requests.

### API shape

```csharp
public interface ISaveService
{
    LoadResult<PetState> LoadPet();
    SaveResult SavePet(PetState state);
    bool IsDegraded { get; }
    event EventHandler<PersistenceStatusChanged> StatusChanged;
}
```

`LoadResult` carries `State`, `Source` (canonical / backup / fresh /
quarantined), and `Reason`. `SaveResult` carries `Success`, `Attempts`,
and `Error`. The UI reads these to render the status badge and to log
recovery notices.

## Migration Plan

Three steps, each shippable independently.

### Step 1 — Path move and legacy import

- Add `SaveService` that resolves and ensures
  `%LOCALAPPDATA%\DesktopPal\saves\`.
- Update `PetState.Load`/`Save` to use `SaveService` (or the equivalent
  injected dependency once the runtime refactor lands).
- On first launch, import any `pet_state.json` next to the executable
  into the new location; rename the legacy file to
  `pet_state.json.migrated`.
- Keep the existing flat JSON format for now.
- Acceptance: a freshly built MSIX install can save and reload without
  permission errors. A user with an existing prototype save retains
  their pet on first launch of the new build.

### Step 2 — Atomic write + backup

- Implement the tmp/rename/.bak sequence in `SaveService.Save`.
- On load, fall through to `.bak` if the canonical file is missing or
  unparseable.
- Add quarantine for unparseable saves.
- Acceptance: forced corruption of `saves\pet.json` (truncate to zero
  bytes) on launch results in recovery from `.bak`, a non-blocking UI
  notice, and the corrupt file moved to `saves\quarantine\`. Forced
  corruption of both files results in a fresh pet plus quarantine
  preservation, no crash.

### Step 3 — Versioned envelope

- Wrap the persisted JSON in the schema envelope
  (`schema`, `schemaVersion`, `savedAtUtc`, `appVersion`, `data`).
- Implement a migration registry; the v1 case is "unwrap and
  deserialize". Add a stub v2 to validate the framework.
- Implement forward-version refusal.
- Acceptance: handcrafted v0 (legacy flat) saves load via a one-shot
  upgrade to v1; handcrafted v999 saves are refused with the
  update-required dialog.

Future schema-version changes (e.g. world objects, AI memory) reuse this
envelope and migration registry rather than reinventing it per file.

## Risks

- **Permission edge cases.** Some corporate environments redirect
  `LOCALAPPDATA` or apply restrictive ACLs. Mitigation: test on a
  standard user account (not just the dev's admin account) and surface
  a clear error rather than crashing.
- **Antivirus latency.** Real-time scanners can transiently lock newly
  written files, breaking the rename step. Mitigation: bounded retry
  loop with backoff; the degraded-mode signal communicates the issue
  to the user instead of failing silently.
- **Atomic-rename across volumes.** If `LOCALAPPDATA` is on a different
  volume than `tmp`, rename is non-atomic. Mitigation: keep `.tmp`
  inside `saves\`, same volume by construction.
- **Migration regressions.** A bad migration silently corrupts data.
  Mitigation: each migration is a pure function with unit tests over
  representative payloads, and `.bak` retention provides one rollback
  step.
- **Save thrash.** Naive dirty-flag implementations can save several
  times per second. Mitigation: cadence is enforced by the simulation
  engine, not by event handlers.
- **Quarantine growth.** A persistent corruption bug could fill the
  quarantine directory. Mitigation: cap quarantine to N files (e.g.
  10), evicting oldest.

## Open Questions

- Do we ever offer "export pet" / "import pet" as a user-facing feature?
  If yes, the envelope already supports it; UI is out of scope here.
- Should `settings\ui.json` (window placement, drawer prefs) be a
  separate file kind from `pet.json`? Proposed yes, separate schema id,
  same envelope. Confirm during Tritium-2.
- Where does AI memory persist? Proposed: separate file under `saves\`
  with its own `schema` id, so memory schema can evolve independently
  of pet stats. Detailed in `ai-memory.md`.
- Do we need a checksum field in the envelope? Probably not at v1;
  filesystem-level integrity plus parse validation has been sufficient
  for similar apps. Revisit if recovery telemetry says otherwise.
- How do we handle the case where the user moves `%LOCALAPPDATA%` (rare
  but supported)? Proposed: re-resolve on every launch, do not cache
  the path. No special handling needed.

— Jesse
