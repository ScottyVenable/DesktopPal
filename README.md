# DesktopPal

[![CI](https://github.com/ScottyVenable/DesktopPal/actions/workflows/ci.yml/badge.svg)](https://github.com/ScottyVenable/DesktopPal/actions/workflows/ci.yml)

DesktopPal is a Windows-only WPF desktop companion prototype.It places a small pet on the desktop, supports local AI chat through LM Studio, and experiments with lightweight world and desktop interactions.

## Project status

DesktopPal is currently in **prototype / pre-release** state.

What is working today:

- transparent desktop overlay with a wandering pet
- local AI chat through LM Studio
- text-file "letter" reading and reply writing on the desktop
- persistent pet stats with offline decay
- settings, system tray, and global hotkey support
- simple world objects and cleanup interactions

What is still partial or planned:

- true vision is **not** implemented yet; the repo only has groundwork
- pet visuals and animation are still prototype-grade
- gardening, richer world systems, and multi-pet support are roadmap items
- there is currently no automated test suite, CI workflow, or packaging pipeline in the repository

If you want the execution plan rather than the product pitch, start with:

- [`TRITIUM-PLAN.md`](TRITIUM-PLAN.md) for the full roadmap
- [`docs/ROADMAP.md`](docs/ROADMAP.md) for the contributor-facing execution summary
- [`CONTRIBUTING.md`](CONTRIBUTING.md) for issue, milestone, and triage conventions
- [`HANDOFF.md`](HANDOFF.md) for architecture and implementation notes

## Prerequisites

- Windows 10 or Windows 11
- .NET 10 SDK
- LM Studio running locally on `http://localhost:1234` for AI chat features

DesktopPal can still launch without LM Studio, but AI-dependent features will be limited.

## Run locally

From the repository root:

```bash
dotnet build DesktopPal/DesktopPal.csproj
dotnet run --project DesktopPal/DesktopPal.csproj
```

From the `DesktopPal/` subdirectory:

```bash
dotnet run
```

## Repository map

| Path | Purpose |
| --- | --- |
| `DesktopPal/` | WPF application source |
| `TRITIUM-PLAN.md` | Long-form product and engineering roadmap |
| `docs/ROADMAP.md` | Execution summary, milestones, and issue structure |
| `CONTRIBUTING.md` | Contribution, triage, and GitHub project workflow |
| `HANDOFF.md` | Technical architecture handoff for developers/agents |
| `plans/` | Historical planning notes retained for reference |

## Current repository gaps

The repo still needs:

- automated tests
- CI validation
- packaging and release discipline
- clearer separation between prototype behavior and shipped capability

Those gaps are tracked in the Tritium roadmap and GitHub project rather than being treated as already complete.

## Contributing

Please read [`CONTRIBUTING.md`](CONTRIBUTING.md) before opening issues or pull requests. New work should align to the Tritium milestones and use the repository label taxonomy.

## License status

No repository license file is currently present. Until a license is added, treat reuse and redistribution as unspecified.
