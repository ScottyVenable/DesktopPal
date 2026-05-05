# Simulation Runtime and Service Boundaries

Status: Draft
Owner: Sol (implementation), Jesse (doc steward)
Related: TRITIUM-PLAN §9 "Core architecture and simulation", Issue #14
Milestone: Tritium-0 Structural Reset

## Goals

- Decouple simulation logic from WPF rendering and window code-behind.
- Establish a deterministic, fixed-step simulation clock that does not vary
  with monitor refresh rate or window focus.
- Define explicit service boundaries between simulation, persisted state,
  AI conversation, system integration, and UI presentation.
- Make core state transitions (decay, hatch, mood, action selection) testable
  in plain unit tests with no WPF dependency.
- Provide a migration path that does not require a full rewrite of
  `MainWindow.xaml.cs` in a single change.

## Non-Goals

- Full ECS or game-engine architecture. DesktopPal is a small companion app,
  not an engine project.
- Multi-threaded simulation. The fixed-step loop runs on a single thread,
  marshalled to the UI thread via `Dispatcher` for visual updates.
- Replacing WPF as the rendering layer. WPF stays; the simulation simply
  stops being driven by it.
- Multi-pet support. The runtime should not preclude it, but no scheduling,
  identity, or routing for multiple pets is in scope here.
- Replay/rewind systems. Determinism is a means to testability, not a
  user-facing time-travel feature.

## Current State

Today the simulation is driven from the UI render callback in
`MainWindow.xaml.cs`:

```csharp
CompositionTarget.Rendering += GameLoop;
...
private void GameLoop(object sender, EventArgs e)
{
    Pet.State.Tick();
    Pet.UpdateVisuals();
    ...
}
```

Concrete consequences:

- `CompositionTarget.Rendering` fires at the compositor's cadence (typically
  60 Hz, but variable, paused during minimization, throttled on battery).
  Decay rates in `PetState.Tick()` are tuned to "assuming 1 tick per second
  or so" while in practice the method is invoked tens of times per second.
- Movement, wandering, target-following, drag handling, vision dispatch,
  and tray menu wiring all live in `MainWindow.xaml.cs` (~430 lines).
  There is no seam between "what the pet is doing" and "how the window
  draws it".
- `_saveTimer` (one-minute `DispatcherTimer`) and `_visionTimer`
  (five-minute `DispatcherTimer`) are independent UI timers with no
  shared scheduler.
- `PetState` mixes persisted profile fields, runtime mood, hotkey
  preferences, and decay rules in one class.
- There is no way to run a single tick from a test, because
  `PetState.UpdateRealTime()` reads `DateTime.Now` directly and `Tick()`
  is intended to be called at an undefined real frequency.

This works for a prototype. It blocks every Foundation-tier exit criterion:
no determinism, no testability, no clear seam for AI, world state, or
diagnostics.

## Proposed Design

### Tick rate

The simulation runs at a **fixed 10 Hz logical tick** (100 ms per tick).

Rationale:

- DesktopPal stats change on the order of seconds-to-hours; sub-second
  granularity is wasteful and produces noisy decay math.
- 10 Hz is fast enough to drive smooth wander targets, intent changes,
  and event evaluation, while slow enough to be cheap on battery.
- Animation interpolation runs separately on the WPF compositor at
  display rate; it reads simulation state but does not advance it.

The tick rate is a single named constant (`SimulationTickHz = 10`) so it
can be tuned without spreading magic numbers.

### Service shape

Introduce a `SimulationEngine` (working name) that owns the loop:

```
SimulationEngine
├── Start(IClock clock)
├── Stop()
├── event TickElapsed(SimulationTick tick)
└── internal accumulator-based update driven by a System.Threading.Timer
```

The engine does not know about WPF. It receives an `IClock` abstraction so
tests can drive a deterministic clock and step time forward by exact
amounts.

`SimulationTick` carries:

- monotonic tick number,
- elapsed simulated time since start,
- wall-clock at tick start (for events that need real time, e.g. offline
  decay reconciliation).

### Service boundaries

```
┌──────────────────────────────────────────────────────────────┐
│                         UI layer (WPF)                       │
│  MainWindow / WorldWindow / PetControl / PalDrawer           │
│  - subscribes to SimulationEngine.TickElapsed                │
│  - renders, animates, handles input                          │
│  - dispatches user intents into the command bus              │
└──────────────────────────────────────────────────────────────┘
                 ▲ render reads             ▼ intents
┌──────────────────────────────────────────────────────────────┐
│                      Simulation layer                        │
│  SimulationEngine    - fixed-step loop                       │
│  PetSimulation       - decay, mood, action selection         │
│  WorldSimulation     - decorations, spawn rules (later)      │
│  CommandBus          - feed/pet/clean/call/letter/chat       │
└──────────────────────────────────────────────────────────────┘
                 ▲ load/save               ▼ events
┌──────────────────────────────────────────────────────────────┐
│                         State layer                          │
│  PetProfile (persisted: name, birth, hotkey, model, vision)  │
│  PetStats   (runtime: hunger, hygiene, happiness, energy)    │
│  PetMood    (runtime, derived)                               │
│  WorldState (persisted decorations, environment)             │
│  SaveService (see persistence.md)                            │
└──────────────────────────────────────────────────────────────┘
                 ▲ async results          ▼ requests
┌──────────────────────────────────────────────────────────────┐
│                   Integration services                       │
│  AIService        (see ai-memory.md)                         │
│  SystemIntegration (FileSystemWatcher, vision, screenshot)   │
│  Diagnostics      (logging, status reporting)                │
└──────────────────────────────────────────────────────────────┘
```

Rules:

- The UI layer never mutates `PetStats` directly. It dispatches commands.
- The simulation layer never calls `Dispatcher.Invoke` or touches WPF
  types. Cross-thread marshalling happens at the UI subscription edge.
- Integration services are async and report results back through the
  command bus or domain events, never by reaching into UI windows.
- Save/load is initiated by the simulation layer on its own cadence,
  not by UI timers.

### Tick contract

Each tick performs, in order:

1. Drain pending commands from the command bus.
2. Apply integration events queued since last tick (letter received,
   AI reply, vision result).
3. Advance `PetSimulation` (decay, mood recompute, action selection).
4. Advance `WorldSimulation` (when introduced).
5. Raise `TickElapsed` so the UI can read state and render.
6. If a save interval has elapsed, request `SaveService.Save()`.

Steps 1–4 are pure: same input state plus same commands plus same elapsed
time produces the same output state. This is the property unit tests
exercise.

### Pause and background rules

- When the host process is suspended (laptop sleep, OS-level pause),
  the engine does not attempt to "catch up" tick-by-tick. On resume it
  computes elapsed wall-clock time and applies a single coarse offline
  decay step (the existing `UpdateRealTime` logic, moved into
  `PetSimulation.ReconcileOffline(TimeSpan)`).
- A configurable cap (e.g. 24 h) prevents pathological catch-up after
  long offline periods.
- "Quiet mode" / DND pauses *expressive* output (chat bubbles, emotes)
  but does not pause decay. Decay is part of the world; quiet mode is a
  presentation choice.

### Determinism

- All randomness (wander targets, idle action choice, dialogue selection
  on the simulation side) goes through a single seeded `IRandomSource`
  owned by the engine. Tests inject a deterministic source.
- `DateTime.Now` is forbidden in the simulation layer. Use `IClock.Now`.
- Floating-point order of operations is fixed by the tick contract.

## Migration Plan

This is a four-step migration. Each step ships independently and leaves
the app in a working state.

### Step 1 — Extract simulation primitives (no behavior change)

- Create `Sim/IClock.cs`, `Sim/IRandomSource.cs`, `Sim/SimulationTick.cs`.
- Split `PetState` into `PetProfile` (persisted identity/preferences) and
  `PetStats` (mutable runtime values). Keep a `PetState` facade for
  compatibility during migration.
- Move decay math (`UpdateRealTime`, `Tick`) into a new `PetSimulation`
  class that takes `PetStats` and an elapsed `TimeSpan`. Old `Tick()`
  delegates to it.
- Acceptance: app behavior unchanged, but `PetSimulation` is unit-testable
  with a fake clock.

### Step 2 — Introduce SimulationEngine alongside the render loop

- Add `SimulationEngine` driven by a `System.Threading.Timer` at 10 Hz.
- Subscribe `MainWindow` to `TickElapsed` and call `Pet.State.Tick()`
  from there instead of from `CompositionTarget.Rendering`.
- Rendering callback shrinks to "read state, update visuals, animate".
- `_saveTimer` and `_visionTimer` continue to exist for now.
- Acceptance: removing the `CompositionTarget.Rendering` simulation tick
  line does not change observable decay behavior; rendering stays smooth.

### Step 3 — Move movement and action selection into the simulation

- Pull wander target selection, target-following arithmetic, and idle
  state transitions out of `MainWindow.xaml.cs` into `PetSimulation`.
- The UI reads "current intent" + "current position" from state and
  draws it. Drag remains UI-side because it is a direct input gesture.
- Vision and save scheduling move to engine-driven cadences.
- Acceptance: `MainWindow.xaml.cs` shrinks substantially; movement and
  decay can be exercised in headless tests.

### Step 4 — Command bus and event surface

- Introduce `CommandBus` with typed commands (Feed, Pet, Clean, CallToMouse,
  ReceiveLetter, ApplyAIReply, SpawnDecoration).
- UI input handlers and integration services dispatch commands instead
  of mutating state directly.
- Add a small set of domain events (StatChanged, HatchOccurred, MoodShifted)
  the UI subscribes to for reactive presentation.
- Acceptance: there are no direct writes to `PetStats` from outside
  `PetSimulation`.

Each step is one or two issues under the runtime-foundation epic and
should be merge-ready independently.

## Risks

- **Smoothness regressions.** Moving simulation off the render callback
  is correct, but naive interpolation can make wander motion look choppy.
  Mitigation: animate position visually on the render callback by
  interpolating between the last two simulated positions.
- **DispatcherTimer vs Threading.Timer drift.** A `System.Threading.Timer`
  ticks off the UI thread; events must be marshalled. Mitigation: the
  engine raises events on its own thread; UI subscribers wrap handlers
  in `Dispatcher.BeginInvoke` at the subscription site.
- **Hidden coupling discovered late.** `MainWindow.xaml.cs` has grown
  organically; some "simulation" behavior may turn out to depend on UI
  state (e.g. Canvas dimensions). Mitigation: Step 1 explicitly
  inventories every read/write before refactoring.
- **Save thrash.** Engine-driven save at every tick is wrong; once-per-N-ticks
  saving with a dirty flag is required. Persistence design (see
  `persistence.md`) handles atomic writes.
- **Test gap.** Determinism is only valuable if there are tests. Step 1
  must land with at least decay/hatch unit tests or the migration
  regresses to "looks fine, no proof".

## Open Questions

- Should the simulation tick rate be user-configurable for performance
  (e.g. 5 Hz on battery)? Default no, revisit if telemetry shows cost.
- Does `WorldWindow` survive the migration as a separate window, or does
  it become a layer inside a single shell window? Out of scope here;
  raised as a follow-up to the UX workstream.
- Where does animation state live? Proposed: UI-side, derived from
  simulation events. Needs confirmation once Step 3 lands.
- Should the command bus be sync (queued, drained at tick start) or
  async? Proposed sync-with-queue. Confirm during Step 4.
- Is there value in recording a tick log for debugging? Possibly during
  Tritium-1 Trustworthy Prototype, not now.

— Jesse
