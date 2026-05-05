# DesktopPal Roadmap Execution Guide

This document is the short-form execution companion to `TRITIUM-PLAN.md`. It exists so contributors can understand what is active now without reading the full strategic plan first.

## Current reality

DesktopPal has a compelling prototype, but it is not yet a production-ready desktop companion.

Today the repository has:

- a working WPF desktop overlay prototype
- local LM Studio chat support
- persistent pet stats and offline decay
- desktop letter handling
- tray, settings, and hotkey controls

It does **not** yet have:

- a real vision pipeline
- production-ready animation/content systems
- automated tests
- CI workflows
- packaging and release discipline

## Execution priorities

The roadmap is sequenced intentionally:

1. stabilize architecture and simulation
2. harden integrations and trustworthiness
3. improve daily usability and onboarding
4. add delivery discipline
5. deepen memory, progression, and world systems
6. prepare for extensibility and long-term maintenance

## Active milestone sequence

| Milestone | Focus | Typical work |
| --- | --- | --- |
| Tritium-0 Structural Reset | runtime foundation | simulation loop, service boundaries, save strategy |
| Tritium-1 Trustworthy Prototype | reliability | diagnostics, watcher lifetime, degraded mode, tests |
| Tritium-2 Daily Driver Alpha | usability | onboarding, UX consolidation, polish |
| Tritium-3 Packaged Companion | delivery | CI, packaging, release checklist |
| Tritium-4 Memory and Growth | AI depth | memory model, progression, authored unlocks |
| Tritium-5 Living Desktop | world systems | gardening, persistent world state, safe vision |
| Tritium-6 Companion Platform | maturity | multi-pet groundwork, migrations, extensibility |

## Epic structure

Roadmap work should be grouped into parent issues with linked sub-issues. Preferred epic breakdown:

- runtime foundation
- trustworthy integrations
- daily-driver UX
- delivery baseline
- memory and progression
- living desktop systems
- platform foundations

Each epic should own:

- a concise objective
- exit criteria
- linked sub-issues with acceptance criteria

## Immediate queue

The next planning-ready work items are:

1. design a fixed-step simulation runtime
2. define save-path and schema versioning strategy
3. harden system integration service lifetime and diagnostics
4. implement AI degraded mode and LM Studio status reporting
5. add an initial restore/build/test CI workflow
6. choose a packaging path and release-readiness checklist

## How to use this with the GitHub Project

For roadmap issues in the project:

- use milestones to indicate the target phase
- use labels for type, area, and priority
- use `Ready` status only after acceptance criteria are written
- keep estimate and target date realistic and small enough to be actionable

If an issue is still exploratory, label it `status:needs-design` and keep it in backlog until scope is concrete.

## Design Docs

Design docs live under `docs/design/`. They are drafted before
implementation begins and updated as decisions are made. Each one is
attached to a planning issue and referenced from the relevant epic.

| Doc | Scope | Issue | Milestone |
| --- | --- | --- | --- |
| [simulation-runtime.md](design/simulation-runtime.md) | Fixed-step simulation loop, service boundaries, migration off `CompositionTarget.Rendering` | #14 | Tritium-0 |
| [persistence.md](design/persistence.md) | Save path under `%LOCALAPPDATA%\DesktopPal\`, schema version envelope, atomic write, backup and recovery | #15 | Tritium-0 |
| [ai-memory.md](design/ai-memory.md) | Layered memory model (persona / long-term / mid-term / short-term), versioned prompt templates, eval seams, degraded-mode interplay with #18 | #25 | Tritium-0 design / Tritium-4 implementation |

When a design doc lands, the linked issue moves from `status:needs-design`
to `Ready` and acquires its acceptance criteria from the doc's Migration
Plan.

— Jesse
