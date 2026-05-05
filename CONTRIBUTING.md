# Contributing to DesktopPal

## Scope

DesktopPal is still in prototype stage. Contributions should improve the current product honestly and incrementally. Do not describe roadmap items as already shipped.

Before starting work, read:

- `README.md`
- `HANDOFF.md`
- `TRITIUM-PLAN.md`
- `docs/ROADMAP.md`

## Working agreements

- Keep issues small enough to finish within a milestone.
- Use milestones for phase planning and labels for type, area, and priority.
- Prefer updating an existing roadmap issue over creating duplicate feature requests.
- Capture acceptance criteria before work starts.
- Treat advanced desktop integrations as opt-in and safety-sensitive.

## Issue standards

Every actionable issue should include:

1. a clear problem statement
2. why the work matters
3. scope and constraints
4. acceptance criteria / definition of done
5. milestone, labels, and project placement

Recommended title style:

- `Implement opt-in vision pipeline via LM Studio`
- `Define save-path and schema versioning strategy`
- `Add initial restore/build/test CI workflow`

## Label taxonomy

### Type

- `type:epic`
- `type:feature`
- `type:bug`
- `type:tech-debt`
- `type:docs`
- `type:test`
- `type:release`

### Priority

- `priority:p0`
- `priority:p1`
- `priority:p2`
- `priority:p3`

### Area

- `area:architecture`
- `area:ui-ux`
- `area:ai`
- `area:content`
- `area:integrations`
- `area:world`
- `area:quality`
- `area:docs`
- `area:repo`
- `area:platform`

### Workflow status

- `status:in-progress`
- `status:ready`
- `status:blocked`
- `status:needs-design`
- `status:needs-validation`

## Milestone model

DesktopPal execution is organized around these milestones:

1. `Tritium-0 Structural Reset`
2. `Tritium-1 Trustworthy Prototype`
3. `Tritium-2 Daily Driver Alpha`
4. `Tritium-3 Packaged Companion`
5. `Tritium-4 Memory and Growth`
6. `Tritium-5 Living Desktop`
7. `Tritium-6 Companion Platform`

Use the earliest milestone that can realistically absorb the work without forcing speculative design.

## GitHub Project workflow

The active planning board is the DesktopPal roadmap project. For issues added to that board, keep these fields current:

- `Status`
- `Priority`
- `Size`
- `Estimate`
- `Start date`
- `Target date`

Use:

- `Backlog` for sequenced but not yet ready work
- `Ready` for well-scoped work with clear acceptance criteria
- `In progress` only when implementation has actually started
- `In review` when the work is awaiting validation or merge
- `Done` when the issue is complete and reflected in the repository

## Reporting bugs and requests

Use the issue templates in `.github/ISSUE_TEMPLATE/`.

- Bug reports should include environment, reproduction steps, expected behavior, and actual behavior.
- Feature requests should explain the user outcome, constraints, and definition of done.

## Documentation changes

Contributor-facing docs should stay aligned with shipped reality:

- `README.md` for repo overview and current status
- `HANDOFF.md` for architecture notes
- `TRITIUM-PLAN.md` for long-form strategy
- `docs/ROADMAP.md` for milestone execution guidance

If a roadmap or milestone decision changes, update the relevant doc in the same pass.
