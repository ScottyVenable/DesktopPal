# Multi-Pet, Migration, and Extensibility Boundaries

Status: Draft
Owner: Sol (implementation), Jesse (doc steward)
Related: TRITIUM-PLAN §"Companion Platform", Issue #30
Milestone: Tritium-6 Companion Platform (design now, implementation later)
Depends on: `docs/design/persistence.md` (#15),
            `docs/design/world-state.md` (#28),
            `docs/design/ai-memory.md` (#25)

## Goals

- Establish a data shape and runtime topology that supports more than
  one pet on the desktop without rewriting the simulation later.
- Define a clean migration path from today's single-pet save to a
  multi-pet save, with no user-visible data loss.
- Carve out an extensibility boundary for "species" or "theme" variants
  (bear, cat, fox, blob) so authored content can ship without code
  changes to the simulation core.
- Keep AI memory and stat decay isolated per pet — one pet's
  experience must not bleed into another's persona or memories.
- Land the multi-pet shape early enough that other systems
  (world state, persistence envelope, companion UI) can target it as
  the canonical model, even while only one pet is exposed in v1.

## Non-Goals

- Shipping multi-pet UI in v1. The work here is foundation; the user-
  facing "add a second pet" flow is a later milestone.
- Pet-to-pet AI dialogue or relationship modeling. Pets coexist; they
  do not converse.
- A full content/modding pipeline. We define the data seam for species
  definitions; the loader, validation, and authoring tools come later.
- Cloud sync of pet rosters. Local-first.
- Cross-pet shared memory. Each pet has its own memory layers (#25).

## Current State

`DesktopPal/PetState.cs` defines a single `PetState` class. It is
loaded as a singleton, hardcoded into `MainWindow`:

- `MainWindow` constructs one `PetControl` and one `PetState`.
- `CompanionWindow` reads the single `PetState`.
- `Decoration` (poop click) reaches into
  `Application.Current.MainWindow.Pet.State.Hygiene` to mutate the one
  pet's stats.
- The save file `pet_state.json` is a flat object with no `pets[]`
  collection and no pet identity beyond `Name`.
- There is no concept of a "species" or theme. The pet's appearance is
  hardcoded in `PetControl.xaml` and code-behind.

Concrete problems:

- **Singleton assumptions everywhere.** Every callsite that says
  `Application.Current.MainWindow.Pet` is structurally locked to one
  pet.
- **No pet identity.** Without a stable id, world-state ownership
  (#28), AI memory routing (#25), and UI selection cannot disambiguate
  pets.
- **Schema is flat.** Adding a `pets[]` array is a breaking change
  with no migration story today (no schema version, no migrator).
- **Appearance is code, not data.** "Buddy the blue bear" is the
  reference design; the running pet is a generic shape. There is no
  seam for authored species variants.

## Proposed Design

### Top-level shape

A new `WorldState` envelope (also referenced by `world-state.md`)
becomes the root persisted document for the simulation, with `pets`
as a first-class collection:

```csharp
public sealed class WorldState
{
    public int SchemaVersion { get; set; } = 2;
    public DateTime LastSeen { get; set; } = DateTime.Now;
    public List<PetRecord> Pets { get; set; } = new();
    public List<DecorationRecord> Decorations { get; set; } = new();
}
```

`PetRecord` is the renamed, multi-pet-shaped successor to today's
`PetState`:

```csharp
public sealed class PetRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Buddy";
    public string SpeciesId { get; set; } = "buddy_blue_bear";
    public DateTime BirthTime { get; set; } = DateTime.Now;

    public PetStats Stats { get; set; } = new();          // hunger, hygiene, ...
    public PetProgression Progression { get; set; } = new(); // level, xp
    public PetPresentation Presentation { get; set; } = new(); // hotkey, vision, model
    public PetWorldPresence Presence { get; set; } = new();    // last x/y, action state

    // Memory layers live in their own files keyed by Id (see ai-memory.md).
}
```

The single global `PetState` class is retired. Where today's code
reads `pet.State.Hunger`, multi-pet code reads
`pet.Record.Stats.Hunger`. Each pet ticks its own stats; the
simulation runtime (#14) iterates the roster.

### Species / theme definitions

Species are data, not code:

```csharp
public sealed record SpeciesDefinition(
    string Id,                       // "buddy_blue_bear", "cat_basic", ...
    string DisplayName,
    string SpriteSheetPath,          // resolved by content layer
    PetStatTuning StatTuning,        // optional per-species decay rates
    string DefaultPersonaPromptId    // ties to ai-memory.md persona layer
);
```

A `SpeciesCatalog` is loaded at startup from a manifest under
`content/species/` (path TBD with Vex). Adding a new species means
shipping a manifest entry + assets, not editing simulation code.

v1 ships a single registered species: `buddy_blue_bear`, matching
the existing reference design (`reference_buddy_blue_bear.png`).

### Runtime topology

- `MainWindow` no longer owns "the pet." It owns a `PetRoster`
  service backed by `WorldState.Pets`.
- One `PetControl` instance per active `PetRecord`. Each gets its own
  position state, animation state, and click target.
- `CompanionWindow` gains a tab strip (or list) keyed by `PetRecord.Id`,
  with the active pet selected. v1 shows a single tab; multi-pet just
  adds tabs.
- AI calls are scoped per pet: prompt building, memory retrieval, and
  response routing all carry a `PetId`.
- World decorations link to pets via `DecorationRecord.OwnerPetId`
  (see `world-state.md`). A null owner means "neutral / world-owned."

### Coexistence rules on the desktop

- **No collision physics.** Pets may visually overlap. Z-order is
  stable per pet to avoid flicker.
- **Independent navigation.** Each pet runs its own movement loop. No
  pathing avoidance in v1.
- **Independent attention.** Each pet has its own AI memory and its
  own short-term context. They do not share what they "saw."
- **Shared world.** Decorations are shared scenery. A poop spawned by
  Pet A can be cleaned by Pet B (or by the user). Whether Pet B gets
  the hygiene credit is a Tritium-6 policy decision (see Open
  Questions).
- **Hotkeys.** A global hotkey targets the "active" pet (selected in
  the companion window). Per-pet hotkeys are deferred.

### AI memory isolation

Per `docs/design/ai-memory.md`, each pet owns its own:

- persona layer (species default, plus per-pet drift)
- long-term memory store
- mid-term and short-term context

Memory files are keyed by `PetRecord.Id`, e.g.:

```
%LOCALAPPDATA%\DesktopPal\memory\<pet-id>\long_term.json
%LOCALAPPDATA%\DesktopPal\memory\<pet-id>\mid_term.json
```

A pet deletion removes its memory directory. A pet rename does not.

## Migration Plan

We have one schema today (call it v1: a flat `pet_state.json`). The
target is v2: `world_state.json` with a `Pets` list.

1. **Introduce schema version.** Land `docs/design/persistence.md`
   first. `pet_state.json` gains `SchemaVersion = 1` at rest.
2. **Define v2 shape.** Add `WorldState`, `PetRecord`, `SpeciesCatalog`
   in code. v1 is still the on-disk format.
3. **Write the migrator.** On load:
   - If `world_state.json` exists with `SchemaVersion >= 2`, use it.
   - Else if `pet_state.json` (v1) exists, read it, wrap it into a
     `WorldState` with one `PetRecord`:
     - `Id = new Guid()`
     - `SpeciesId = "buddy_blue_bear"`
     - Copy `Name`, `BirthTime`, `LastSeen`, stats, progression,
       hotkey, model, vision flag.
   - Write a fresh `world_state.json` (v2). Keep `pet_state.json` as
     a `.legacy.bak` per the persistence backup policy.
   - Else: brand new install, create `WorldState` with no pets and
     fall through to onboarding (which creates the first pet).
4. **Route runtime through the roster.** Replace
   `Application.Current.MainWindow.Pet.State.X` patterns with calls
   that take an explicit `PetId`. Where the call site genuinely means
   "the active pet," resolve through `PetRoster.Active`.
5. **Lock v2.** Once v2 reads and writes cleanly, refuse to write v1.
   v1 read remains supported indefinitely for upgrade.
6. **Expose multi-pet.** Add a "new pet" flow in the companion
   window. Off by default behind a setting until UX is ready.

The migrator is unit-testable: feed it sample v1 JSON, assert the
resulting `WorldState`. This becomes a fixture suite that protects
every future schema bump.

## Risks

- **Singleton callsites are everywhere.** Every
  `Application.Current.MainWindow.Pet.State` is a structural bug
  against multi-pet. The migration is mostly a refactor risk, not a
  data risk. Mitigation: stage the `PetRoster` service first and
  forbid new `MainWindow.Pet.State` references in code review.
- **Memory cross-contamination.** A bug in memory routing leaks
  Pet A's memories into Pet B's prompt. Mitigation: every memory
  read/write takes a `PetId`; assert it matches at the storage
  boundary.
- **Species manifest as a content trap.** If species definitions
  drift from the simulation's expected shape, pets fail to load.
  Mitigation: validate manifests at startup; a malformed species
  falls back to the default with a logged warning, never crashes.
- **Migration loses data.** A field rename or split during the v1→v2
  migration silently drops state. Mitigation: fixture tests for
  every known v1 shape, including partial / edge-case files.
- **Companion UI explodes.** A naive tab strip with N pets and per-
  pet panels gets unwieldy fast. Mitigation: v1 ships single-pet UI;
  multi-pet UI is its own design pass.
- **Performance under N pets.** Each pet adds a tick load, an AI
  context, a window child. Mitigation: define a soft cap (e.g., 4
  pets) before opening the door; revisit when actually needed.
- **Scope creep into modding.** "Species are data" invites a full
  modding API. Mitigation: this doc commits only to a manifest seam.
  A modding API is a separate doc when there is demand.

## Open Questions

- Where does the species manifest live, and who owns it? Likely
  `content/species/<id>/species.json` with Vex as steward, but the
  path is not committed.
- Does a pet have a stable `Id` from creation, or only after first
  save? (Recommendation: assign at creation, persist immediately.)
- When Pet B cleans poop spawned by Pet A, does Pet B get the
  hygiene credit, Pet A, both, or neither? Default proposal:
  **the cleaning pet gets the credit**, because the action is theirs.
- Should each pet have its own hotkey, or does the global hotkey
  always target the active pet? v1 keeps the global single-target
  hotkey.
- How are pets destroyed? Soft-archive vs. hard-delete? Memory
  retention rules differ between the two and need a UX decision.
- Do species variants share progression curves, or can a species
  override `PetStatTuning`? The shape supports override; whether v1
  uses it is a balance call.
- Multi-monitor: do pets remember which monitor they were on, or
  re-clamp like decorations? Decision deferred to a multi-monitor
  pass.
- How do pets co-author memories of shared events ("we both saw the
  user open the inbox")? Deferred to #25 expansion.

## Sub-issues this spawns

- Introduce `PetRoster` service and route the existing single pet
  through it (no behavior change).
- Define `SpeciesCatalog` and register `buddy_blue_bear` as the
  default. Move appearance constants out of `PetControl` code-behind
  into the species definition.
- Implement v1→v2 migrator with fixture-based tests.
- Add `PetId` plumbing to AI prompt building and memory storage
  paths (depends on #25).
- Add `OwnerPetId` enforcement on world decorations spawned by pets
  (depends on #28).
- Companion window: refactor to take a `PetId` and render a single
  pet's panel; tab strip is a follow-up.
- Forbid `Application.Current.MainWindow.Pet.State` access pattern
  in contributor guide; document the roster API.

— Jesse
