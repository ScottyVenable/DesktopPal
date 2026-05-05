# World State: Persistent Decorations and Living Desktop

Status: Draft
Owner: Sol (implementation), Jesse (doc steward)
Related: TRITIUM-PLAN §"Living Desktop", Issue #28
Milestone: Tritium-5 Living Desktop (design now, implementation later)
Depends on: `docs/design/persistence.md` (#15), `docs/design/simulation-runtime.md` (#14)

## Goals

- Make the desktop world persistent. Trees, flowers, poop, and any future
  prop survive app restart, machine reboot, and graceful shutdown.
- Define a single, versioned, serializable shape for world state that can
  evolve without breaking existing saves.
- Support time-based lifecycle rules so the world stays alive between
  sessions: things spawn, age, and clean themselves up using offline time
  the same way `PetState` already decays stats.
- Keep decoration definitions data-driven so new prop types can be added
  without touching the simulation core or the serializer.
- Coexist cleanly with `pet_state.json` under the save path defined in
  `docs/design/persistence.md` (atomic write, schema envelope, backup).
- Stay small (< 100 KB projected at horizon) and load in well under
  50 ms, matching persistence targets.

## Non-Goals

- Authored level design. The world is procedurally populated by the pet's
  behavior, not hand-placed by a designer.
- Multi-monitor coordinate normalization in v1. World coordinates are
  expressed against the primary work area and re-clamped on load.
- Networked or shared world state. The world is per-user, local-first.
- Physics or collision simulation. Decorations are static visuals with
  simple z-order and bounds.
- Multi-pet ownership semantics. Multi-pet boundaries are covered in
  `docs/design/multi-pet.md` (#30); world state only carries an owning
  pet id where relevant.

## Current State

`DesktopPal/WorldWindow.xaml.cs` exposes a bare canvas API:

```csharp
public void AddObject(UIElement element, double x, double y) { ... }
public void RemoveObject(UIElement element) { ... }
```

`DesktopPal/Decoration.cs` defines three hardcoded prop types — `Tree`,
`Flower`, `Poop` — each constructed in code as a `UserControl` containing
WPF primitives. Spawn and removal live inline in callers:

- `Poop` decorations register a `MouseDown` handler that calls
  `WorldWindow.RemoveObject` and bumps `PetState.Hygiene` by 5.
- Trees and flowers are added by gameplay code and never removed.
- World position is held only in `Canvas.Left` / `Canvas.Top` on the live
  WPF element. There is no model behind it.

Concrete problems:

- **No persistence.** Closing the app discards every decoration. The
  "garden" never actually grows.
- **No model.** `Decoration` is a view, not data. There is no list of
  decorations to query, save, age, or migrate.
- **String-typed kinds.** `DecorationType` is a `string` switched on in
  the constructor. Adding a kind means editing a `if/else` chain.
- **Side effects in view.** `Poop.MouseDown` reaches into
  `Application.Current.MainWindow` to mutate `PetState.Hygiene`. The
  rule belongs in the simulation, not the visual.
- **No lifecycle.** Nothing ages, expires, or cleans up. Offline time
  has no effect on the world.
- **No spawn rules.** Spawning is whatever the caller decides at the
  call site, with no central policy.

## Proposed Design

### Data model

A new `WorldState` record sits next to `PetState` in the save layer:

```csharp
public sealed class WorldState
{
    public int SchemaVersion { get; set; } = 1;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public List<DecorationRecord> Decorations { get; set; } = new();
}

public sealed class DecorationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string TypeId { get; set; } = "";   // e.g. "tree", "flower", "poop"
    public double X { get; set; }
    public double Y { get; set; }
    public DateTime SpawnedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }   // null = no auto-expiry
    public Guid? OwnerPetId { get; set; }      // see multi-pet.md
    public Dictionary<string, string> Props { get; set; } = new(); // small, type-specific
}
```

Notes:

- `TypeId` is a string key, not an enum, so new types can ship without
  schema bumps.
- `Props` is intentionally a flat string map. Anything richer goes
  through a real schema bump.
- `OwnerPetId` is nullable so single-pet saves and "neutral" props
  (placed flowers, weather effects later) work the same way.

### Decoration type registry

Decoration kinds become data, registered at startup:

```csharp
public sealed record DecorationType(
    string Id,
    string DisplayName,
    Func<DecorationRecord, UIElement> CreateView,
    DecorationLifecycle Lifecycle
);

public sealed record DecorationLifecycle(
    TimeSpan? OfflineExpiry,        // poop: 6 h offline → cleared
    bool RemovableByClick,          // poop: yes; tree: no
    bool AffectsPetStats            // poop click: +Hygiene
);
```

A `DecorationCatalog` service holds the registry. The simulation talks
to records; the view layer asks the catalog for a `UIElement` for a
given record. This kills the `if (type == "Tree")` chain and isolates
WPF construction from save logic.

### Storage

Per `docs/design/persistence.md`, world state lives at:

```
%LOCALAPPDATA%\DesktopPal\world_state.json
```

A separate file (not embedded in `pet_state.json`) because:

- World state churn is independent — decorations age every tick, pet
  stats change less often. Splitting reduces unnecessary rewrite cost.
- A corrupted world file should not lose pet history, and vice versa.
- Multi-pet (#30) makes pets a list under `WorldState`, but stats and
  authored pet content can still live per-pet without the world being
  forced into the same file.

Both files share the persistence envelope: schema version, atomic
temp-file-then-rename, rolling `.bak`, observable failure model.

### Lifecycle and tick integration

The simulation runtime (#14) gains a `WorldSystem` tick. On each fixed
step it:

1. Iterates `WorldState.Decorations`.
2. Applies type-specific aging via `DecorationLifecycle.OfflineExpiry`.
3. Removes records whose `ExpiresAt` has passed.
4. Emits spawn requests to a `WorldSpawner` strategy (per type).

Offline catch-up runs once at load: compute elapsed time from
`WorldState.LastSeen`, advance lifecycles, drop expired records before
the view ever sees them.

### Spawn rules (initial policy, not committed schema)

| Type    | Trigger                                      | Cap         | Offline expiry |
| ------- | -------------------------------------------- | ----------- | -------------- |
| Poop    | Pet hygiene tick + random chance per N min   | 6 active    | 6 h            |
| Flower  | Pet "happy" milestone, weighted by location  | 20 active   | none           |
| Tree    | Pet level-up reward, manual unlock           | 5 active    | none           |

Spawn rules are policy, not schema. They live in `WorldSpawner`
implementations and can change without migrating saves.

### View layer

`WorldWindow` becomes a thin projection of `WorldState`:

- On load, ask the catalog to materialize a `UIElement` for each record,
  attach it to the canvas at `(X, Y)`, tag it with `Id`.
- On record removal, remove the element with the matching `Id`.
- User interactions (clicking poop) raise events into the simulation;
  the simulation mutates state and stat effects; the view reacts.

`AddObject` / `RemoveObject` are kept as internal plumbing only.

## Migration Plan

The current world is fully ephemeral, so this is a forward migration,
not a data migration.

1. **Land the model.** Add `WorldState`, `DecorationRecord`,
   `DecorationCatalog` with the existing three types registered.
   No persistence yet.
2. **Route through the model.** Replace direct
   `WorldWindow.AddObject(new Decoration(...))` calls with
   `WorldSystem.Spawn(typeId, x, y)`. View becomes a projection.
   Behavior visibly identical.
3. **Persist.** Wire `world_state.json` into the persistence layer
   alongside `pet_state.json`. First boot writes an empty
   `WorldState { SchemaVersion = 1 }`.
4. **Lifecycle.** Add `OfflineExpiry` for `poop` only. Trees and
   flowers persist indefinitely. Offline catch-up runs on load.
5. **Decouple stat effects.** Move the `MouseDown → Hygiene += 5`
   rule out of `Decoration` into a `WorldInteraction` handler that the
   simulation owns.
6. **Open the catalog.** Document how to register a new decoration
   type. Foundation for Tritium-5 garden expansion.

Each step is independently shippable and reversible.

## Risks

- **Coordinate drift across resolutions.** A user changes monitor
  resolution; saved `(X, Y)` points off-screen. Mitigation: clamp on
  load to the current work area, log a warning, never silently lose
  records.
- **DPI / multi-monitor.** Saved coordinates are in WPF device-
  independent pixels of the primary work area at save time. Multi-
  monitor users may see decorations land on a different monitor. v1
  documents the limitation; full multi-monitor support is deferred.
- **Save churn.** Every spawn/expiry mutates `world_state.json`. We
  must debounce writes (e.g., flush on tick boundary or every N
  seconds) and never write per-frame.
- **Schema creep via `Props` map.** The string-map escape hatch will
  be abused for "just one field" until it becomes a de-facto schema.
  Mitigation: document that anything stored more than once or read by
  more than one site must be promoted to a typed property and a schema
  bump.
- **Visual desync.** A record exists with no view, or a view exists
  with no record. Mitigation: a single reconciliation pass after every
  tick that compares record ids to canvas children.
- **Offline catch-up storms.** A user returns after a month; tens of
  thousands of expired-poop ticks must not stall startup. Mitigation:
  collapse to a single delta computation, not a per-second loop.
- **Conflict with multi-pet (#30).** Ownership semantics, per-pet
  spawn budgets, and "whose poop is this" must be settled there before
  this doc commits to per-pet caps.

## Open Questions

- Does `world_state.json` belong inside the same directory as
  `pet_state.json`, or under a `world/` subfolder once multi-pet lands?
- Should `DecorationType` definitions be embedded in code or loaded
  from a JSON manifest under `content/decorations/`? Vex will likely
  want the manifest path; Sol will likely want the code path until the
  catalog stabilizes.
- Do flowers and trees ever expire? A 30-day garden that never decays
  is fine for v1, but a "neglected garden wilts" rule would tie back
  to pet hygiene/care.
- How does the world interact with the AI memory layer (#25)? A pet
  who "remembers planting a tree" wants a record id reference, not a
  duplicate of the data.
- Multi-pet ownership: if Pet A spawns poop, can Pet B clean it for a
  hygiene boost? Decision deferred to #30.
- Is there a hard cap on total decoration count for performance, and
  what happens when it is hit (refuse spawn vs. evict oldest)?

## Sub-issues this spawns

- Implement `WorldState` / `DecorationRecord` model (no persistence).
- Implement `DecorationCatalog` and migrate `Decoration.cs` callers
  through the catalog.
- Add `world_state.json` under the persistence envelope.
- Implement offline lifecycle catch-up for `poop`.
- Move poop-click hygiene effect out of the view into the simulation.
- Document decoration-type authoring for contributors.

— Jesse
