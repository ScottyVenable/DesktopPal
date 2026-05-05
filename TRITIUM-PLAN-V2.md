# TRITIUM Plan — V2

> Active strategic and execution plan for DesktopPal, superseding
> [`TRITIUM-PLAN.md`](./TRITIUM-PLAN.md). V1 remains in the repository
> as historical reference for how we framed the prototype-to-product
> transition; V2 reflects the post-Wave-3 reality and resets the
> roadmap around turning shipped design docs into shipped code.

---

## 1. Title and purpose

**TRITIUM-PLAN-V2.md** is the current product and engineering roadmap
for DesktopPal. It reflects work that has actually landed on `main`
across Wave 1 (runtime hardening), Wave 2 (UX consolidation and
content), and Wave 3 (design depth + first-playable gardening), and it
sequences the next horizon of work.

V1 was written when DesktopPal was a promising prototype with no CI,
no design docs, no degraded-mode AI behavior, and a paw-drawer UI. V2
is written from a different starting position:

- the runtime is logged, watched, and shutdown-safe,
- the AI service has a real status model and graceful fallback,
- the pet visual identity has stabilized (icon-scale blue bear),
- the primary control surface is a hotkey-driven companion panel,
- a CI workflow exists,
- six design docs cover simulation, persistence, AI memory, world
  state, multi-pet, and packaging,
- an authored content library covers personality, voice, offline
  phrases, letter beats, progression, achievements, and onboarding,
- and a first-playable gardening loop ships in the build.

The strategic question has shifted from "how do we untangle the
prototype?" to "how do we turn the design corpus we just authored
into the runtime systems users feel?".

---

## 2. What changed since V1

The headline is that V1's *Foundation* horizon is largely **designed
and partially shipped**, and the *Productization* horizon has its
first concrete deliverables on disk. Specifically:

### Runtime and integration hardening (Wave 1)

- **Logging.** `Logging` writes to
  `%LOCALAPPDATA%\DesktopPal\logs\desktoppal.log` with rotation.
  Replaces silent `catch` blocks across system integration and AI
  paths. (#17, #18)
- **`SystemIntegrationService` lifecycle.** Now `IDisposable`,
  retains and disposes the `FileSystemWatcher`, detaches handlers,
  surfaces watcher errors via the logger, and is disposed during
  shutdown alongside tray icon, hotkey, world window, and companion
  window. (#17)
- **AI degraded mode.** `AIService` exposes an `AIServiceStatus`
  (Unknown / Available / Unavailable / Error) with a `StatusChanged`
  event, applies a 10 s timeout via `CancellationToken`, distinguishes
  timeouts / unreachable hosts / malformed responses, and falls back
  to `OfflineBrain` phrases instead of returning raw error strings.
  (#18)
- **Status surface.** Companion panel header shows an LM Studio
  status dot and label (green / amber / red / grey) with a tooltip
  describing the last failure. (#18)
- **Build hygiene.** Cleared remaining build warnings, nullability
  fixes in `AIService` and `SystemIntegrationService`, corrected
  `GameLoop` delegate signature, removed redundant
  `System.Drawing.Common` reference. (#33)

### UX consolidation (Wave 2)

- **Companion panel replaces paw drawer.** The drawer surface is
  retired; primary control surface is now a hotkey-driven companion
  panel with a tray fallback. (#31)
- **Legacy drawer code removed.** `PalDrawer` user control and
  paw-toggle / drawer-button styles deleted; `OnDrawerResized` shim
  removed from `MainWindow`. (#31)
- **Blue-bear visual pass.** Refined proportions so the pet reads
  clearly at desktop-icon scale and aligns with
  `reference_buddy_blue_bear.png`.
- **Taskbar-safe boundaries.** Pet movement, initial spawn, and poop
  decorations now respect `SystemParameters.WorkArea` on all four
  edges. Multi-monitor scope documented as primary-display-only for
  now. (#12)
- **First-run onboarding window.** Introduces the pet, surfaces the
  companion-panel hotkey live from `PetState`, explains tray
  fallback and feed/clean basics. Persisted via
  `HasCompletedOnboarding` on `PetState`. (#20)
- **Animation-ready groundwork.** Gentle ~3 px / 1.6 s idle bob and
  randomised 3–6 s blink (~120 ms hide of active face). Idle bob
  auto-pauses while dragging. Scaffolding only — full state-machine
  animation is deferred. (#2)
- **Authored offline phrases ported.** `OfflineBrain.cs` now reflects
  `docs/content/offline-phrases.md` as source of truth, with new
  Sleepy / Excited / Curious / Encouraging categories wired into
  `GetRandomPhrase`. (#35 — partially shipped under Unreleased)

### Design depth (Wave 3)

Six design docs landed under `docs/design/` and are referenced from
the relevant epics:

- `simulation-runtime.md` — fixed-step loop, service boundaries,
  migration off `CompositionTarget.Rendering`. (#14)
- `persistence.md` — `%LOCALAPPDATA%\DesktopPal\` save path, schema
  version envelope, atomic write, backup and recovery. (#15)
- `ai-memory.md` — layered memory (persona / long-term / mid-term /
  short-term), versioned prompts, eval seams, degraded-mode interplay.
  (#25)
- `world-state.md` — persistent world state, decoration model, type
  catalog, offline lifecycle, `world_state.json`. (#28)
- `multi-pet.md` — multi-pet runtime topology, `WorldState.Pets`
  collection, species/theme manifest seam, v1→v2 migrator, per-pet AI
  memory isolation. (#30)
- `packaging.md` — self-contained single-file publish → signed Inno
  Setup installer → Velopack progression, release readiness checklist,
  code-signing risks. (#23)

### Authored content library (Wave 3)

Authored content under `docs/content/` is now the source of truth for
the pet's voice and progression:

- `personality.md`
- `voice-guide.md`
- `offline-phrases.md`
- `letter-beats.md`
- `progression.md`
- `achievements.md`
- `onboarding.md`

### First-playable gardening (Wave 3)

A real gameplay loop now exists end-to-end:

- `GardenPlot` decoration with `Empty → Seeded → Sprout → Bloom`
  lifecycle.
- "Plant" action in the companion panel that spawns a plot near the
  pet.
- Persistence via a new `World` field on `PetState` (back-compatible
  — older saves load with an empty world).
- Offline catch-up that advances plot states using the same
  elapsed-real-time pattern as stat decay.
- Bloom plots are clickable for a small Happiness + Experience
  reward and reset to Empty for re-planting.
- `CleanAll` preserves garden plots while still wiping ephemeral
  decorations.
- Transition delays are short, configurable consts (MVP visibility).
  (#3)

### Repository and CI

- **Restore/build/test CI workflow.** `.github/workflows/` populated;
  initial workflow restores, builds, and runs tests on push/PR.
  (#22)
- **Issue templates and labels.** Templates added; labels manifest
  committed.
- **Project board fields.** Issues now carry type / priority / status
  / area labels and are assigned to Tritium milestones.

### Net result

Of the V1 *Immediate queue* (six items), **five are closed** (#14,
#15, #17, #18, #22, #23) and the sixth (offline-phrase port, #35) is
shipped under Unreleased pending issue close. Tritium-0 design work
is complete; Tritium-1 has shipped its key runtime tickets;
Tritium-2 has shipped onboarding, companion panel, and the
animation-ready groundwork.

---

## 3. Current snapshot

### What is working in the build

- Pet overlay, drag, wander, taskbar-safe movement.
- Companion panel as primary control surface, with hotkey + tray
  fallback.
- LM Studio chat with timeout, status reporting, and offline-brain
  fallback.
- Letter handling via `SystemIntegrationService` with disposable
  watcher.
- First-run onboarding window with live hotkey display.
- Idle bob and blink animation scaffolding.
- Gardening MVP loop (plant → grow → harvest → reward).
- Persistence to `pet_state.json` with `World` field, back-compatible
  load.
- Logging with rotation under `%LOCALAPPDATA%\DesktopPal\logs\`.
- CI restore/build/test on push and PR.

### What is still raw

- Save file is still alongside the executable, not yet under the
  `%LOCALAPPDATA%\DesktopPal\` envelope described in
  `persistence.md`. No schema versioning, atomic write, or backup.
- Simulation tick is still driven from `CompositionTarget.Rendering`
  rather than a fixed-step service. `SimulationEngine` does not yet
  exist as a real type.
- Animations are storyboards on the existing visual, not a state
  machine. No sprite atlas or Lottie path is wired.
- AI memory is still a short chat list. Layered memory model from
  `ai-memory.md` is not implemented.
- World state is in-process inside `PetState.World`. There is no
  `world_state.json`, no decoration type catalog, no offline
  lifecycle hooks beyond garden-plot ticks.
- Multi-pet runtime does not exist; `WorldState.Pets` is a future
  contract.
- Packaging is still `dotnet build`. No single-file publish, no
  installer, no release artifact.
- Vision pipeline (#1) is unimplemented.
- Multi-monitor support is intentionally limited to the primary
  display.
- Accessibility, telemetry-lite diagnostics, and extensibility hooks
  are not started.

### What is documented but not implemented

The design corpus is now ahead of the runtime. Each of these has a
design doc and an issue waiting to consume it:

| Design doc | Implementation status | Tracking issue |
| --- | --- | --- |
| `simulation-runtime.md` | not implemented | #14 (closed as design-only) |
| `persistence.md` | not implemented | #15 (closed as design-only) |
| `ai-memory.md` | not implemented | #25 (closed as design-only) |
| `world-state.md` | partially shadowed by `PetState.World` | #38 |
| `multi-pet.md` | not implemented | #39 |
| `packaging.md` | not implemented | #37 |
| Animation state machine | scaffolding only | #2, #36 |
| Garden v2 (multi-seed, watering, economy) | MVP only | #40 |

The next push must convert these from documents into systems.

---

## 4. Strategic posture for the next phase

Wave 1–3 closed the "is this thing trustworthy and is its voice
defined?" question. Wave 4 needs to close the "does its runtime
actually match its design?" question.

The strategic posture for the next phase is therefore:

1. **Implement the design corpus.** Persistence, simulation runtime,
   AI memory, world state, animation state machine, and multi-pet
   topology should stop being documents and start being code. The
   design docs are intentionally implementation-shaped — they include
   migration plans, schema envelopes, and service boundaries. Use
   them as specs.
2. **Promote `PetState.World` to a real `WorldState`.** The gardening
   MVP proved that a persistent world matters. The next move is to
   formalize it: split world state from pet state, add the type
   catalog, give it its own JSON file under the persistence envelope.
3. **Make the simulation deterministic.** A fixed-step
   `SimulationService` is the gating dependency for animation state
   machines, multi-pet ticking, and meaningful AI memory windows.
   Build it before adding more behaviors on top of the render-tick.
4. **Get the product installable.** Wave 4 should produce a signed
   single-file artifact and a GitHub Release pipeline. Without that,
   none of the polish is reachable to anyone outside the repo.
5. **Earn vision and OS magic last.** The vision pipeline (#1)
   remains a differentiator, but it should sit behind a stable
   simulation, a real persistence envelope, and a packaging path.
   Do groundwork now (capture pipeline, consent UI, off-by-default
   plumbing) but defer the model integration.
6. **Keep the voice intact.** Vex's authored content is now canonical.
   Every new system that emits a string — phrases, letter beats,
   onboarding copy, achievement text, error toasts — must read from
   `docs/content/` rather than reinventing tone.

---

## 5. Roadmap by horizon

### Now — next push (Wave 4: turn docs into code)

Goal: collapse the gap between the design corpus and the runtime.

- **Persistence migration.** Implement
  `%LOCALAPPDATA%\DesktopPal\` save root, schema version envelope,
  atomic write (`*.tmp` + rename), backup-on-load, recovery on
  corruption. Migrator from existing `pet_state.json` beside the
  executable. Per `persistence.md`. (consumes design from #15)
- **Fixed-step simulation service.** Introduce `SimulationService`
  with deterministic tick, accumulated delta, pause-on-background
  rules. Migrate movement, wander, decay, garden ticks, and
  letter-event handling off `CompositionTarget.Rendering`. Per
  `simulation-runtime.md`. (consumes design from #14)
- **Animation state machine.** Replace ad-hoc storyboards with a
  small state machine (`Idle`, `Walk`, `Drag`, `Sleep`, `React`,
  `Bloom-Reward`) driven by simulation events. Keep current visual
  but route bob/blink through the state machine. (#36, with #2 as
  parent)
- **AI memory layers (v1).** Implement persona / long-term /
  mid-term / short-term as separate stores, with versioned prompt
  template, summarization seam, and degraded-mode interplay. Per
  `ai-memory.md`. (consumes design from #25)
- **Garden v2.** Multiple seed types from a manifest, watering action,
  harvest economy feeding the progression doc. (#40)

### Next — Wave 5 (delivery + platform)

Goal: make the product installable and prepare for multi-pet and
world depth.

- **Packaging.** Self-contained single-file publish, GitHub Release
  artifact upload from CI, signed Inno Setup installer (with the
  code-signing risks from `packaging.md` flagged). (#37, consumes
  design from #23)
- **Persistent world state.** Promote `PetState.World` to a real
  `WorldState` with its own `world_state.json` under the persistence
  envelope, decoration type catalog, offline lifecycle. (#38)
- **Multi-pet runtime.** `WorldState.Pets` collection,
  species/theme manifest seam, v1→v2 save migrator, per-pet AI memory
  isolation. (#39)
- **Vision pipeline groundwork (#1).** Off-by-default capture
  pipeline, explicit consent UI, redaction options, no model wired
  yet — only plumbing.
- **Manual QA pass.** Companion panel placement, hotkey
  discoverability, multi-monitor behavior. (#32)

### Later — Wave 6+ (extensibility and polish)

Goal: long-term stewardship.

- Mod / extension hooks for content (seeds, decorations, phrases,
  letter beats) reading from manifests under `docs/content/`.
- Multi-monitor improvements past primary-display-only.
- Accessibility pass (keyboard navigation in companion panel, screen
  reader names, contrast review of status dot).
- Telemetry-lite local diagnostics: opt-in local-only counters and a
  "copy diagnostics" button in settings. No remote telemetry.
- Letter beat content campaign expansion.
- Achievements surface in the companion panel.
- Vision pipeline model integration (gated behind everything above).

---

## 6. Detailed workstreams

These workstreams mirror V1 but are rewritten for current reality.
Each lists current state, next deliverables, and the issues / docs
that own them.

### 6.1 Core architecture and simulation

**Current state.** `MainWindow.xaml.cs` still owns wander, drag,
hygiene, and tick orchestration. `Tick()` runs from
`CompositionTarget.Rendering`. `PetState` carries both persisted
identity and runtime stats, plus the new `World` field. There is no
`SimulationService` type yet.

**Next deliverables.**

- `SimulationService` with fixed-step accumulator, pause on
  background, deterministic update order.
- Move wander / decay / garden ticks / decoration ticks into the
  service.
- Split `PetState` into persisted profile, runtime stats, and a save
  service consumer.
- Domain events for `Feed`, `Pet`, `Clean`, `Plant`, `Harvest`,
  `Call`, `LetterReceived`.

**Owners.** Sol. **Inputs.** `simulation-runtime.md`. **Issues.** new
implementation issues to be opened under Tritium-0 epic (#13).

### 6.2 Persistence and data integrity

**Current state.** Single `pet_state.json` beside the executable.
`PetState.World` is a back-compatible additive field. No schema
version, no atomic write, no backup.

**Next deliverables.**

- `Save` directory under `%LOCALAPPDATA%\DesktopPal\` per
  `persistence.md`.
- Schema version envelope + migrator for the in-place `pet_state.json`.
- Atomic write (`*.tmp` then rename) and rolling backup.
- Recovery path that loads from the most recent valid backup on a
  parse failure, with logger trail.

**Owners.** Sol. **Inputs.** `persistence.md`. **Issues.** new
implementation issue under Tritium-0 epic (#13).

### 6.3 AI service and memory

**Current state.** `AIService` has degraded-mode behavior (#18) and a
status surface in the companion panel. Memory is a short chat list.
Offline phrases come from `OfflineBrain.cs` mirroring
`docs/content/offline-phrases.md`.

**Next deliverables.**

- Layered memory store (persona / long-term / mid-term / short-term).
- Versioned prompt template loader.
- Summarization seam to compress mid-term into long-term over time.
- Degraded-mode interplay: choose phrase categories from
  `OfflineBrain` based on simulation state, not random.
- Eval seam (capture prompt + response pairs locally for review).

**Owners.** Sol (runtime) + Vex (prompt template wording).
**Inputs.** `ai-memory.md`, `personality.md`, `voice-guide.md`.
**Issues.** new implementation issue under Tritium-4 epic (#24).

### 6.4 World state and decorations

**Current state.** `PetState.World` carries garden plots only. Other
decorations (poop, ephemera) are tracked elsewhere and not part of
the world state. No type catalog. Garden plot lifecycle is hardcoded.

**Next deliverables.**

- Promote `WorldState` to a top-level persisted document
  (`world_state.json`) under the persistence envelope.
- Decoration type catalog (id, lifecycle, persistence rule,
  offline-tick rule).
- Offline catch-up generalised to walk every decoration of a type
  and apply its rule.
- Garden v2: multi-seed manifest, watering action, harvest economy.

**Owners.** Sol (runtime) + Vex (seed/decoration content).
**Inputs.** `world-state.md`, `progression.md`. **Issues.** #38, #40.

### 6.5 Animation and visual identity

**Current state.** Blue-bear visual pass shipped at icon scale. Bob
and blink storyboards exist as one-off animations. No state machine.
No sprite atlas. No Lottie pipeline.

**Next deliverables.**

- Animation state machine driven by simulation events, with
  transitions for `Idle`, `Walk`, `Drag`, `React`, `Sleep`,
  `Bloom-Reward`, `Letter`.
- Decision: keep XAML-driven visual or migrate to sprite-atlas /
  Lottie. Capture in a short ADR before implementation. (#36)
- Reaction animations tied to garden v2 harvest, letter receipt, and
  feed/clean events.

**Owners.** Sol. **Inputs.** `personality.md` for what reactions
should *feel* like. **Issues.** #2 (parent), #36 (state machine).

### 6.6 System integration and vision

**Current state.** `SystemIntegrationService` is disposable, logged,
and watches the desktop for `.txt` files. Screenshot capture exists
as a stub. Vision is not implemented.

**Next deliverables (Wave 5 groundwork only).**

- Off-by-default capture pipeline with a settings toggle and a
  visible "Vision is enabled" status indicator.
- Region selection / window-only capture options before any model
  hook.
- Redaction-friendly capture settings (resolution cap, monochrome
  option, cadence cap).
- No LM Studio vision call until packaging ships.

**Owners.** Sol. **Inputs.** none yet (a vision design doc should be
authored before #1 implementation begins). **Issues.** #1.

### 6.7 UX, onboarding, and content surfacing

**Current state.** Companion panel is the primary surface. First-run
onboarding window exists with placeholder copy. Achievements,
progression, and letter beats are authored but not yet surfaced in
the UI.

**Next deliverables.**

- Replace onboarding placeholder copy with `docs/content/onboarding.md`
  (Vex pass per #20).
- Achievements list view in the companion panel reading from
  `docs/content/achievements.md`.
- Progression indicator (level / stage) in the companion panel
  reading from `docs/content/progression.md`.
- Letter-beat scheduler that consumes `docs/content/letter-beats.md`
  and writes letters into the configured drop folder.

**Owners.** Sol (UI) + Vex (copy). **Issues.** #2, #20 (closed but
copy still pending), new issues for achievements / progression /
letter scheduler.

### 6.8 Quality, CI, and release

**Current state.** Restore/build/test CI workflow exists (#22).
There is no test suite of substance. No packaging. No release
artifact.

**Next deliverables.**

- A small unit-test project for `OfflineBrain`, garden lifecycle, and
  persistence migrator.
- Single-file publish profile.
- GitHub Release artifact upload from CI on tagged commits.
- Inno Setup installer with explicit code-signing TODO from
  `packaging.md`.
- Manual QA pass per #32.

**Owners.** Sol + Rook. **Inputs.** `packaging.md`. **Issues.** #37,
#32.

### 6.9 Repository and process

**Current state.** Issue templates, labels manifest, project fields,
and milestones exist. CHANGELOG discipline is in place under
`## [Unreleased]`. CONTRIBUTING.md and HANDOFF.md exist.

**Next deliverables.**

- Open implementation-side issues for the closed design tickets
  (#14, #15, #25 are closed as design-only; their runtime work needs
  fresh issues).
- Cut a `0.2.0` tag once persistence migration + simulation service
  ship, to anchor the first packaged release.
- Wave-cadence release notes based on the CHANGELOG.

**Owners.** Jesse (issues, board) + Sol (changelog, tags).

---

## 7. Milestone matrix

Rough current progress per Tritium milestone. Percentages are
calibrated against the milestone's exit criteria, not against issue
count.

| Milestone | Theme | Progress | Notes |
| --- | --- | --- | --- |
| **Tritium-0** Structural Reset | runtime foundation | ~40% | Design complete (#14, #15). Runtime not migrated yet — `SimulationService` and `%LOCALAPPDATA%` envelope are the gating deliverables. |
| **Tritium-1** Trustworthy Prototype | reliability | ~75% | Logging, watcher lifecycle, AI degraded mode, status dot, build hygiene all shipped (#17, #18, #33). Remaining: unit tests, manual QA pass (#32). |
| **Tritium-2** Daily Driver UX | usability | ~60% | Companion panel, drawer retirement, taskbar-safe boundaries, onboarding window, animation scaffolding shipped (#12, #20, #31). Remaining: animation state machine (#36), achievements/progression surfacing, real onboarding copy. |
| **Tritium-3** Delivery Baseline | release readiness | ~25% | CI restore/build/test shipped (#22). Packaging design shipped (#23). Implementation pending: single-file publish, installer, release artifact (#37). |
| **Tritium-4** Memory and Progression | AI depth | ~30% | Memory design shipped (#25). Progression and letter-beat content authored. Runtime memory layers not implemented. |
| **Tritium-5** Living Desktop | world systems | ~25% | Garden MVP shipped (#3). World-state design shipped (#28). World-state runtime, decoration catalog, garden v2 (#40), persistent world (#38) pending. Vision pipeline (#1) untouched. |
| **Tritium-6** Companion Platform | platform | ~15% | Multi-pet design shipped (#30). Runtime not started (#39). Extensibility hooks not started. |

Open epic parents: #13, #16, #19, #21, #24, #27, #29.

---

## 8. Risks and dependencies

### Architecture risk

**Risk:** the longer simulation stays on
`CompositionTarget.Rendering`, the harder it gets to add multi-pet
ticking, deterministic animation, and meaningful AI memory windows.

**Mitigation:** make `SimulationService` the first deliverable in
Wave 4. Block animation state machine and multi-pet runtime on it
explicitly.

### Persistence risk

**Risk:** garden state and onboarding flag now live in
`pet_state.json` beside the executable. A bad release that breaks
load loses real player progress.

**Mitigation:** ship persistence migration *before* the first packaged
release. Atomic write + backup are non-negotiable for any v0.2.0 tag.

### Voice drift risk

**Risk:** new UI surfaces (achievements, progression, vision consent,
error toasts) keep being written ad-hoc and drift from Vex's
authored voice.

**Mitigation:** every new string-emitting system loads from
`docs/content/` or routes through a content service that does. No
hardcoded user-facing copy.

### Vision risk

**Risk:** vision integration is the single most user-trust-sensitive
feature. Shipping it before packaging, persistence, and consent UX
mature would burn user trust.

**Mitigation:** keep #1 in Later. Land capture-pipeline plumbing
off-by-default in Wave 5. Defer model integration until Wave 6+.

### Code-signing risk

**Risk:** unsigned installer triggers SmartScreen and blocks early
adopters. Acquiring a code-signing certificate has cost and lead time.

**Mitigation:** flagged in `packaging.md`. Treat as a Wave 5 blocker
to schedule, not a Wave 4 surprise.

### Single-developer risk

**Risk:** the project moves at the pace of its lead developer; bus
factor is one.

**Mitigation:** keep design docs authoritative, keep changelog
discipline, keep the issue board honest. Anyone who picks up the
repo should be able to find the next thing to do from
`TRITIUM-PLAN-V2.md` + `docs/ROADMAP.md` + the open issue list.

### Multi-monitor risk

**Risk:** primary-display-only is documented but is a real ceiling on
real-world usability.

**Mitigation:** acceptable for Wave 4; revisit in Wave 6 with a
proper screen-aware design.

---

## 9. Recommended immediate next actions

In order. Each item is a fresh issue or a follow-up to an existing
one. Top of the queue first.

1. **Open Wave-4 implementation issues for the design tickets that
   were closed as design-only.** Specifically: persistence migration
   (consumes #15), simulation service (consumes #14), AI memory
   layers (consumes #25). Each new issue should reference the design
   doc and inherit its acceptance criteria. — *Jesse*
2. **Implement persistence migration to `%LOCALAPPDATA%\DesktopPal\`.**
   Schema envelope, atomic write, backup, migrator. This is the
   gating dependency for the first packaged release. — *Sol*
3. **Implement `SimulationService` with fixed-step tick.** Migrate
   wander, decay, garden ticks, decoration ticks. Remove
   `CompositionTarget.Rendering` for simulation. — *Sol*
4. **Promote `PetState.World` to a real `WorldState` document.**
   Split out into `world_state.json`, add decoration type catalog,
   generalize offline catch-up. (#38) — *Sol*
5. **Build the animation state machine on top of the simulation
   service.** Route existing bob/blink through it. (#36) — *Sol*
6. **Implement AI memory layers v1.** Persona / long-term / mid-term
   / short-term stores, versioned prompt template, summarization
   seam. — *Sol* (with *Vex* on prompt wording)
7. **Replace onboarding placeholder copy with
   `docs/content/onboarding.md`.** Close out the copy debt from #20.
   — *Vex* + *Sol*
8. **Garden v2: multi-seed manifest, watering action, harvest
   economy.** (#40) — *Sol* + *Vex*
9. **Set up single-file publish + GitHub Release artifact.** First
   draft of the packaging pipeline; signing can follow. (#37) —
   *Sol* + *Rook*
10. **Run the manual QA pass on companion panel placement, hotkey
    discoverability, multi-monitor behavior.** (#32) — *Rook*

---

## 10. Appendix — open issues snapshot

Snapshot taken from `gh issue list --state open --limit 100` at the
time of writing. Grouped by milestone where one is assigned, by epic
otherwise.

### Tritium-0 — Structural Reset (epic #13)

- *(none open — runtime implementation issues to be opened per
  action item 1 above)*

### Tritium-1 — Trustworthy Prototype (epic #16)

- **#32** Run manual QA on companion panel placement, hotkey
  discoverability, and multi-monitor behavior — `type:test`,
  `priority:p1`, `status:ready`.

### Tritium-2 — Daily Driver UX (epic #19)

- **#2** Replace placeholder pet visuals with an animation-ready
  system — `type:feature`, `priority:p2`, `status:in-progress`.
- **#12** Complete scale, taskbar-safe layout, and cleanup polish —
  `type:feature`, `priority:p1`, `status:ready`.
- **#31** Remove remaining legacy drawer code and references after
  companion-panel migration — `type:tech-debt`, `priority:p2`,
  `status:ready`.
- **#36** Build state-machine driven animation system —
  `type:feature`, `priority:p2`, `status:needs-design`.

### Tritium-3 — Delivery Baseline (epic #21)

- **#37** Implement self-contained single-file publish and GitHub
  Release artifact — `type:release`, `priority:p1`, `status:ready`.

### Tritium-4 — Memory and Progression (epic #24)

- **#35** Port authored offline phrases from
  `docs/content/offline-phrases.md` into `OfflineBrain.cs` —
  `type:tech-debt`, `priority:p2`, `status:ready`. (Note: shipped
  under Unreleased; issue still open.)

### Tritium-5 — Living Desktop (epic #27)

- **#1** Implement opt-in vision pipeline via LM Studio —
  `type:feature`, `priority:p2`, `status:needs-design`.
- **#38** Implement persistent world state and decoration data
  model — `type:feature`, `priority:p1`, `status:ready`.
- **#40** Garden v2 — multiple seed types, watering action, harvest
  economy — `type:feature`, `priority:p2`, `status:needs-design`.

### Tritium-6 — Companion Platform (epic #29)

- **#39** Implement multi-pet runtime support — `type:feature`,
  `priority:p2`, `status:ready`.

### Repo / docs

- **#34** [M1] docs todo — create a todo list and format GitHub
  Issues, create issue templates — `area:docs`, `area:repo`.

### Open epic parents (for cross-reference)

- **#13** Epic: Tritium-0 structural reset and runtime foundation.
- **#16** Epic: Tritium-1 trustworthy prototype and integrations.
- **#19** Epic: Tritium-2 daily driver UX consolidation.
- **#21** Epic: Tritium-3 delivery baseline and release readiness.
- **#24** Epic: Tritium-4 memory and progression depth.
- **#27** Epic: Tritium-5 living desktop systems.
- **#29** Epic: Tritium-6 companion platform foundations.

---

*Authored by Sol, Co-Creative Director and Lead Programmer.
Supersedes `TRITIUM-PLAN.md`. Pair with `docs/ROADMAP.md` for the
short-form execution view and with `docs/design/` for implementation
specs.*
