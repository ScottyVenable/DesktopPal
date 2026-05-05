# TRITIUM Plan

## 1. Title and purpose

**TRITIUM-PLAN.md** is the broad product and engineering roadmap for DesktopPal. It translates the current prototype into a sequenced program of work: what the project is today, what it should become, what must be fixed first, and how to move from a promising desktop toy to a stable companion platform.

This document is grounded in the current repository state. It acknowledges prior planning in `plans/phase-8-ui-overhaul.md`, but supersedes that narrower UI-focused plan with a wider product, architecture, release, and repository strategy.

## 2. Executive summary

DesktopPal already proves the core fantasy: a small companion lives on the Windows desktop, wanders around, reacts to the user, supports local AI chat through LM Studio, responds to text-file letters, exposes tray/settings controls, and creates small moments of delight through emotes and cleanup interactions.

The project is still a prototype. Core game logic is tied directly to WPF rendering and window code-behind. Persistence is a single JSON file beside the executable. Desktop/system integration is partially stubbed. Vision is suggestive rather than real. The repository has no automated tests, no CI workflows, and no packaging or release pipeline.

The right strategy is not to chase every ambitious idea immediately. The next stage should focus on four moves:

1. Stabilize the architecture around a real simulation/runtime model.
2. Productize the existing experience so it can be configured, tested, packaged, and trusted.
3. Expand into richer memory, content, environment, and desktop interactions only after the foundation is solid.
4. Mature the repository and release process so feature growth does not collapse under technical debt.

This roadmap proposes a phased plan across four horizons:

- **Foundation:** untangle architecture, make simulation deterministic, harden persistence and services.
- **Productization:** improve UX, settings, onboarding, quality, packaging, and release readiness.
- **Expansion:** add deeper AI memory, progression, world systems, and more credible desktop features.
- **Platform/Maturity:** support extensibility, multi-pet systems, analytics-lite local diagnostics, and long-term maintainability.

## 3. Current project snapshot

### Product state

DesktopPal is currently a **.NET 10 WPF desktop overlay application** built around:

- `MainWindow`
- `WorldWindow`
- `PetControl`
- `PetState`
- `AIService`
- `SettingsWindow`
- `SystemIntegrationService`
- `Decoration`
- `PalDrawer`

### Architecture state

The app currently uses a **two-window overlay model**:

- `MainWindow` hosts the pet and interaction surface.
- `WorldWindow` sits behind and holds world objects such as decorations.

That model works as a prototype, but the implementation is heavily coupled through code-behind and direct cross-window/control references.

### Runtime and simulation state

- `PetState` persists to `pet_state.json` beside the executable.
- `UpdateRealTime()` applies offline decay on load.
- `Tick()` is invoked from `CompositionTarget.Rendering`, so simulation is effectively tied to frame rate rather than a fixed simulation clock.
- Movement, wandering, drag behavior, basic hygiene events, and some layer management live inside `MainWindow.xaml.cs`.

### AI state

- `AIService` targets LM Studio on `http://localhost:1234/v1/chat/completions`.
- It keeps a short message list rather than a structured memory system.
- On failure it returns fallback error text to the user rather than handling degraded states cleanly.

### System integration state

- `SystemIntegrationService` watches the desktop for `.txt` files.
- `FileSystemWatcher` is created as a local variable rather than owned lifetime-managed state.
- Screenshot capture exists as a stub-level building block, not a completed vision feature.
- Several methods swallow exceptions, limiting debuggability and trustworthiness.

### Repository and delivery state

- README and HANDOFF describe an ambitious long-term vision.
- Several major items remain aspirational or partial: true vision integration, desktop icon manipulation, farming, rich animation, multi-pet support, packaging, CI, and tests.
- There is **no `.github/workflows/` folder** and **no automated test suite** in the repo.

## 4. What already works well

DesktopPal has real strengths worth preserving.

### Experience strengths

- **Strong core fantasy:** a desktop companion that feels present rather than hidden in a separate app window.
- **Local AI chat:** LM Studio integration gives the product immediate personality and a privacy-friendly pitch.
- **Letters:** the text-file correspondence loop is unusual, legible, and memorable.
- **Simple delight systems:** emotes, wandering, cleanup, and lightweight reactions already create attachment.
- **Low-friction control surface:** tray integration, settings, and hotkey support make the prototype usable.

### Product strengths

- The project already has a distinct identity: not just a chatbot, not just a Tamagotchi, and not just a novelty overlay.
- The local-first framing is meaningful. Users can understand why this exists.
- The concept supports both cozy interaction and deeper systems over time.

### Engineering strengths

- The current codebase is small enough to restructure without a full rewrite.
- Core object boundaries already hint at future modules: state, AI, integrations, windows, controls, decorations.
- Offline decay and persistent state show the right instinct: the pet should feel continuous across sessions.

## 5. Key gaps and technical debt

### Architecture and runtime

- Heavy code-behind coupling across windows and controls.
- Direct UI ownership of simulation and game rules.
- Frame-tied simulation via `CompositionTarget.Rendering`.
- No clear separation between simulation state, interaction orchestration, and rendering.
- No event bus, command model, or domain-layer interfaces.

### Persistence and data integrity

- Single JSON save beside the executable is fragile for packaging and permissions.
- No versioned save schema or migration strategy.
- No corruption recovery, backup, or transactional write pattern.

### AI and memory

- Short chat list is not a real memory model.
- No separation between chat context, long-term memory, mood, and authored personality.
- No robust degraded-mode behavior when LM Studio is unavailable.
- No prompt versioning or test harness for prompts.

### UX and content

- UI is functional but still prototype-grade.
- Rich animation, stronger visual identity, and onboarding are missing.
- Existing interactions are charming but shallow.
- Progression is light and does not yet create durable long-term goals.

### System integrations

- Vision is not truly implemented.
- Desktop icon manipulation is not implemented.
- Screenshot capture is not connected to a safe, user-controlled pipeline.
- Integration error handling and diagnostics are weak.

### Quality and delivery

- No automated tests.
- No CI workflows.
- No packaging/install/update path.
- No release checklist, compatibility matrix, or crash logging discipline.

### Repository/process

- README vision is ahead of shipped reality.
- Planning exists, but it is fragmented.
- There is no issue taxonomy or milestone structure reflected in this repo snapshot.

## 6. Product vision and design pillars

### Product vision

DesktopPal should become a **local-first desktop companion simulation** that blends:

- cozy presence,
- responsive AI conversation,
- persistent growth,
- desktop-aware interactions,
- and a believable sense of living alongside the user over time.

It should feel less like a floating widget and more like a small resident of the user's machine.

### Design pillars

#### 1. Presence over noise

The pet should feel alive and nearby, not disruptive. It should react with restraint, respect focus, and avoid becoming a notification engine.

#### 2. Local-first trust

Core value comes from running locally. AI, memory, and system interactions should be privacy-conscious, transparent, and user-controlled.

#### 3. Small delight, long arc

Every session should have quick wins, but the product also needs medium- and long-term progression. Users should form attachment over days and weeks, not just minutes.

#### 4. System magic with clear consent

Desktop-aware features are a differentiator, but they must be gated, understandable, reversible, and safe.

#### 5. Deterministic core, expressive surface

The simulation must be stable, testable, and deterministic. Personality, dialogue, animation, and decoration can be expressive on top of that foundation.

#### 6. Mobile-first discipline applied to desktop complexity

Even though this is a Windows desktop product, the interaction model should stay compact, simple, and readable. Small surfaces first. Progressive disclosure second.

## 7. Strategic themes

### Theme A: Stabilize the pet as a system

Build a simulation/runtime architecture that is not dependent on frame rate, window code-behind, or uncontrolled side effects.

### Theme B: Ship a trustworthy desktop product

Move from prototype behavior to packaging, settings clarity, error visibility, diagnostics, and predictable persistence.

### Theme C: Deepen attachment

Expand memory, progression, authored personality, letters, and world interactions so the pal feels more personal over time.

### Theme D: Earn advanced integrations

Approach vision, desktop manipulation, and richer OS hooks gradually, behind clear permissions and technical safeguards.

### Theme E: Build the repo for sustained development

Introduce issue structure, CI, tests, docs, and release discipline so the project can scale without becoming brittle.

## 8. Roadmap by horizon

## Foundation

**Goal:** convert the prototype into a maintainable product core.

Primary outcomes:

- Fixed-step simulation loop
- Service and state boundaries
- safer save system
- basic logging and diagnostics
- clearer UI architecture
- initial test harness

Exit criteria:

- Simulation behavior is no longer frame-tied.
- App can recover from LM Studio unavailability and save issues.
- Core state transitions are testable outside WPF.

## Productization

**Goal:** make DesktopPal something that can be installed, configured, used daily, and supported.

Primary outcomes:

- onboarding and first-run flow
- stronger settings and permissions model
- better drawer/menu UX
- packaging and release flow
- CI and basic regression coverage

Exit criteria:

- A new user can install, configure, and understand the app without reading source.
- Core flows are covered by automated validation.
- A repeatable build/package process exists.

## Expansion

**Goal:** deepen the product with richer simulation, AI memory, and environmental systems.

Primary outcomes:

- memory layers and relationship tracking
- meaningful progression/content loops
- world objects and decoration systems
- early vision pipeline
- more desktop-aware interactions

Exit criteria:

- The pal changes in meaningful ways across sessions.
- Users have reasons to return beyond novelty.
- At least one advanced desktop-aware feature is credible and safe.

## Platform/Maturity

**Goal:** prepare DesktopPal for broader scale, extensibility, and long-term stewardship.

Primary outcomes:

- multi-pet foundations
- plugin/content architecture where appropriate
- migration/versioning discipline
- stronger observability and support tools
- release governance

Exit criteria:

- The project can absorb feature growth without another architectural reset.
- Content, AI tuning, and system integrations can evolve independently.

## 9. Detailed workstreams

### Core architecture and simulation

#### Objectives

- Remove frame-tied logic.
- Isolate simulation from WPF.
- Replace direct control/window coupling with clear orchestration boundaries.

#### Major initiatives

- Introduce a `SimulationEngine` or equivalent runtime service with:
  - fixed timestep updates,
  - accumulated delta handling,
  - pause/background rules,
  - deterministic state transitions.
- Split `PetState` responsibilities into:
  - persisted profile/state,
  - runtime stats/mood,
  - simulation rules,
  - save/load service.
- Move movement, wandering, target-following, decay, and event triggering out of `MainWindow.xaml.cs`.
- Introduce domain events or commands for actions such as:
  - feed,
  - pet,
  - clean,
  - call-to-mouse,
  - receive-letter,
  - complete-chat,
  - spawn-decoration.
- Refactor the two-window model so ownership is explicit:
  - UI composition/root shell,
  - world rendering surface,
  - overlay interaction surface.
- Decide whether `MainWindow` and `WorldWindow` stay as separate windows long term or become a shell strategy with interchangeable presentation modes.

#### Deliverables

- Simulation design doc
- refactored runtime layer
- save service with version field
- deterministic test cases for decay/progression

#### Priority

**P0.** This is the foundation of everything else.

### UX/UI and interaction model

#### Objectives

- Preserve charm while making the UI coherent, consistent, and legible.
- Turn prototype controls into a compact product surface.

#### Major initiatives

- Consolidate interaction patterns across:
  - right-click pet,
  - tray menu,
  - settings window,
  - drawer interactions,
  - chat input/status surfaces.
- Revisit `PalDrawer` as the primary command surface:
  - stats,
  - care,
  - social/chat,
  - world,
  - settings.
- Improve onboarding and first-run education:
  - what the pet can do,
  - what requires LM Studio,
  - what permissions/features are optional,
  - how letters and hotkeys work.
- Define focus-aware behavior:
  - quiet mode,
  - DND,
  - presentation/fullscreen behavior,
  - accessibility and reduced-motion options.
- Evolve visuals from placeholder shapes toward a coherent art and animation system.
- Keep `plans/phase-8-ui-overhaul.md` as historical input, but treat its proposals as candidates within this broader roadmap rather than the main plan itself.

#### Deliverables

- interaction map
- UI information architecture
- first-run experience
- updated settings and drawer spec
- visual style guide for cozy modern desktop overlay presentation

#### Priority

**P1.** Important once architecture stabilization is underway.

### AI systems and memory

#### Objectives

- Move from single-call chat novelty to a believable companion intelligence layer.

#### Major initiatives

- Separate AI responsibilities into:
  - conversation service,
  - memory service,
  - prompt builder,
  - mood/personality state,
  - fallback/degraded mode.
- Replace the current short `_messages` list with layered memory:
  - session memory,
  - rolling relationship summary,
  - durable facts/preferences,
  - authored personality traits.
- Add explicit behavior when LM Studio is unavailable:
  - offline canned reactions,
  - clear status in settings,
  - retry/backoff behavior,
  - no raw error-text dumped as the main user experience.
- Make vision a separate capability, not a hidden side effect of chat.
- Add prompt/config versioning and fixtures for repeatable prompt validation.

#### Deliverables

- AI architecture spec
- memory schema
- degraded-mode UX
- prompt test cases

#### Priority

**P1-P2.** Start design in Foundation, deepen in Expansion.

### Content/progression/personality

#### Objectives

- Give users reasons to care for the pal over time.

#### Major initiatives

- Define progression layers:
  - hatch/early growth,
  - relationship milestones,
  - unlockable behaviors,
  - earned decorations/items,
  - letter/story beats.
- Expand authored personality content:
  - mood-driven reactions,
  - contextual dialogue pools,
  - relationship-specific lines,
  - milestone letters.
- Introduce simple currencies or reward loops only if they reinforce care rather than grind.
- Clarify stat semantics and balance:
  - hunger,
  - hygiene,
  - happiness,
  - energy,
  - experience,
  - level.
- Build a lightweight content pipeline so authored content is not trapped in hardcoded lists.

#### Deliverables

- progression design
- content schema
- milestone/reward map
- authored dialogue/content backlog

#### Priority

**P2.** Begins after core runtime is stable enough to support it.

### World systems/decorations/environment

#### Objectives

- Turn the world from a few spawned objects into a lightweight persistent environment.

#### Major initiatives

- Persist world objects separately from pet state.
- Expand `Decoration` into a typed world-object system with:
  - metadata,
  - growth stages,
  - interaction rules,
  - placement logic,
  - cleanup/expiry.
- Introduce environment zones and spawn rules rather than ad hoc object placement.
- Revisit farming/gardening as a medium-term system:
  - seeds,
  - growth timers,
  - watering/care,
  - decoration rewards.
- Support delight systems such as emotes, naps, idle activities, and ambient interactions without overwhelming the desktop.

#### Deliverables

- world-state schema
- decoration object model
- persistent environment save strategy
- gardening prototype plan

#### Priority

**P2.** Valuable expansion area after runtime and UI cleanup.

### System integrations and desktop features

#### Objectives

- Make desktop-aware features real, safe, and understandable.

#### Major initiatives

- Refactor `SystemIntegrationService` into owned, disposable services with logging and error reporting.
- Fix watcher lifetime by holding `FileSystemWatcher` as managed state rather than a local variable.
- Rebuild screenshot capture and vision as a consented flow:
  - explicit enablement,
  - capture scope,
  - retention rules,
  - model routing,
  - visible status.
- Treat desktop icon manipulation as a high-risk, later-phase capability:
  - prototype only after stronger trust/permission UX exists,
  - allow full disable,
  - always restore/undo where possible.
- Expand integrations incrementally:
  - active window awareness,
  - idle/lock state awareness,
  - notifications or reminders only if user-configured,
  - richer hotkey and tray behaviors.

#### Deliverables

- integration safety model
- service lifecycle refactor
- letter system hardening
- vision proof-of-concept plan

#### Priority

**P1-P3.** Hardening first, advanced features later.

### Quality/testing/CI/release

#### Objectives

- Add minimum professional engineering discipline.

#### Major initiatives

- Introduce automated tests in layers:
  - pure simulation/unit tests,
  - persistence tests,
  - prompt builder tests,
  - selective UI smoke tests where practical.
- Add `.github/workflows/` with CI for:
  - restore,
  - build,
  - test,
  - packaging validation.
- Add code quality gates:
  - nullable warnings reviewed,
  - analyzer adoption,
  - consistent formatting/linting.
- Define packaging strategy:
  - self-contained vs framework-dependent,
  - installer choice,
  - save-path implications,
  - update model.
- Add release readiness checklist:
  - LM Studio dependency messaging,
  - Windows compatibility,
  - known issues,
  - rollback plan.

#### Deliverables

- test project structure
- first CI workflow
- packaging recommendation
- release checklist

#### Priority

**P0-P1.** Build/test discipline should start immediately.

### Repository/docs/community/process

#### Objectives

- Align the repository with reality while preserving ambition.

#### Major initiatives

- Update docs to distinguish:
  - shipped features,
  - partial/prototype features,
  - future goals.
- Add architecture docs for the runtime, AI, integrations, and save system.
- Establish issue taxonomy, epics, and milestone naming.
- Add contribution guidance for:
  - code changes,
  - content additions,
  - prompt updates,
  - bug reports.
- Maintain a changelog and roadmap discipline so future plans do not fragment into isolated notes.

#### Deliverables

- docs refresh plan
- contribution/process guide
- epic list and milestone conventions
- roadmap maintenance cadence

#### Priority

**P1.** Important for execution clarity even before expansion work.

## 10. A milestone matrix / phased release outline

| Milestone | Horizon | Theme | Core outcome |
| --- | --- | --- | --- |
| Tritium 0 - Structural Reset | Foundation | Architecture | Fixed-step simulation, service boundaries, save/version groundwork |
| Tritium 1 - Trustworthy Prototype | Foundation | Quality | Logging, watcher lifecycle fixes, AI degraded mode, first tests |
| Tritium 2 - Daily Driver Alpha | Productization | UX/Product | Onboarding, cleaner drawer/settings, improved persistence and permissions |
| Tritium 3 - Packaged Companion | Productization | Delivery | CI, packaging, release checklist, installable preview build |
| Tritium 4 - Memory and Growth | Expansion | AI/Content | Layered memory, progression milestones, richer authored personality |
| Tritium 5 - Living Desktop | Expansion | World/Integrations | Persistent environment, gardening/decorations, early safe vision pipeline |
| Tritium 6 - Companion Platform | Platform/Maturity | Scalability | Multi-pet groundwork, migrations, extensibility, mature support tooling |

### Release phase outline

#### Phase 1: Tritium 0 - Structural Reset

- Extract simulation/runtime loop from rendering.
- Refactor `PetState` responsibilities.
- Introduce persistence versioning and safer save location strategy.
- Define service boundaries for AI and system integrations.

#### Phase 2: Tritium 1 - Trustworthy Prototype

- Fix `FileSystemWatcher` lifetime.
- Replace swallowed exceptions with logging and surfaced diagnostics.
- Implement AI unavailable/offline mode.
- Add first unit tests around simulation and persistence.

#### Phase 3: Tritium 2 - Daily Driver Alpha

- Ship onboarding.
- Rework settings and drawer as coherent product surfaces.
- Add clearer DND/focus behavior.
- Tune existing loops: wandering, chat, letters, cleanup, care.

#### Phase 4: Tritium 3 - Packaged Companion

- Add CI workflows.
- Choose packaging/distribution path.
- Validate install/update/save-path behavior.
- Publish an alpha-quality packaged build internally or to early users.

#### Phase 5: Tritium 4 - Memory and Growth

- Add layered memory.
- Implement progression milestones and authored unlocks.
- Expand letters into a progression/content channel.
- Add prompt/version test coverage.

#### Phase 6: Tritium 5 - Living Desktop

- Persist world state and decorations.
- Prototype gardening/farming loop.
- Introduce safe, opt-in vision pipeline with clear controls.
- Explore one advanced desktop feature beyond active-window awareness.

#### Phase 7: Tritium 6 - Companion Platform

- Prepare for multi-pet support.
- Add migration discipline for saves/content/prompts.
- Formalize extensibility boundaries.
- Strengthen release/support process.

## 11. Risks, constraints, and dependencies

### Major risks

- **Architecture drag:** adding features before untangling simulation and UI coupling will compound rework.
- **Desktop integration risk:** system-level features can feel magical, but they can also be fragile or intrusive.
- **AI dependency risk:** LM Studio availability, model quality, and latency directly affect perceived intelligence.
- **Scope spread:** the concept naturally expands into many directions. Without sequencing, the project becomes a wishlist.
- **WPF overlay complexity:** transparent multi-window behavior, z-order, focus, and click-through interactions are easy to regress.

### Constraints

- Windows desktop environment only, at least for the near term.
- Current implementation is a prototype and should be treated as such.
- No existing CI/test baseline means quality work starts near zero.
- Packaging decisions affect persistence pathing, permissions, upgrades, and support burden.

### Dependencies

- .NET 10 and WPF platform support
- LM Studio local server on `localhost:1234`
- Windows shell/Win32 behaviors for advanced integrations
- art/content bandwidth for animation and personality expansion

## 12. Recommended immediate next actions

### Next 1-2 weeks

1. Create epics and issues from this roadmap.
2. Design the runtime refactor before adding new features.
3. Decide the target save location and save-versioning strategy.
4. Add a minimal test project covering `PetState`-style simulation and persistence rules.
5. Add the first CI workflow for restore, build, and test.

### Next 2-4 weeks

6. Refactor frame-tied `Tick()` logic into a fixed-step simulation service.
7. Harden `SystemIntegrationService`:
   - owned watcher lifetime,
   - logging,
   - explicit exception handling,
   - clearer screenshot/vision boundaries.
8. Implement AI degraded mode and LM Studio status reporting in settings.
9. Produce a UI architecture pass for drawer, tray, settings, and first-run onboarding.

### Next 1-2 milestones

10. Package an alpha-quality build.
11. Update docs so README and HANDOFF clearly separate current reality from planned direction.
12. Start the memory/progression design track in parallel with productization work, but do not ship it on top of unstable architecture.

## 13. Appendix: suggested issue taxonomy / epics list

### Epic taxonomy

- `epic:architecture`
- `epic:simulation`
- `epic:persistence`
- `epic:ui-ux`
- `epic:ai`
- `epic:memory`
- `epic:content`
- `epic:world`
- `epic:integrations`
- `epic:quality`
- `epic:ci-release`
- `epic:docs-process`

### Suggested label taxonomy

#### Type labels

- `type:feature`
- `type:bug`
- `type:tech-debt`
- `type:design`
- `type:docs`
- `type:test`
- `type:release`

#### Priority labels

- `priority:p0`
- `priority:p1`
- `priority:p2`
- `priority:p3`

#### Area labels

- `area:main-window`
- `area:world-window`
- `area:pet-control`
- `area:pet-state`
- `area:ai-service`
- `area:system-integration`
- `area:settings`
- `area:drawer`
- `area:decorations`
- `area:docs`
- `area:repo`

#### Status labels

- `status:ready`
- `status:blocked`
- `status:in-progress`
- `status:needs-design`
- `status:needs-validation`

### Suggested first-wave epics

#### Epic 1: Prototype to runtime foundation

- fixed-step simulation loop
- state/service boundary refactor
- save versioning and safer storage location
- deterministic simulation tests

#### Epic 2: Trustworthy integrations

- watcher lifetime fix
- logging and diagnostics
- AI degraded mode
- settings visibility for integration state

#### Epic 3: Product UX consolidation

- drawer/settings redesign
- onboarding
- DND/focus behavior
- interaction consistency pass

#### Epic 4: Delivery baseline

- test project setup
- CI workflow setup
- packaging path decision
- alpha release checklist

#### Epic 5: Companion depth

- memory layers
- progression milestones
- authored dialogue pipeline
- letters expansion

#### Epic 6: Living world

- persistent world state
- decorations framework
- gardening prototype
- safe vision proof of concept

### Suggested milestone naming

- `Tritium-0 Structural Reset`
- `Tritium-1 Trustworthy Prototype`
- `Tritium-2 Daily Driver Alpha`
- `Tritium-3 Packaged Companion`
- `Tritium-4 Memory and Growth`
- `Tritium-5 Living Desktop`
- `Tritium-6 Companion Platform`

---

This plan is intentionally ambitious. It is also intentionally sequenced. DesktopPal already has the spark. The next step is to protect that spark with stronger architecture, clearer scope, and disciplined productization.
