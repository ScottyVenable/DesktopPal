# Contributing to DesktopPal

Thank you for your interest in contributing! This document explains how to set up your development environment, follow project conventions, and submit changes.

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [Getting Started](#getting-started)
3. [Project Structure](#project-structure)
4. [Development Workflow](#development-workflow)
5. [Coding Standards](#coding-standards)
6. [Testing](#testing)
7. [Logging & Debugging](#logging--debugging)
8. [Submitting Changes](#submitting-changes)
9. [Issue & PR Conventions](#issue--pr-conventions)
10. [Roadmap Items](#roadmap-items)

---

## Code of Conduct

All contributors are expected to be respectful and professional. Harassment, discrimination, and disruptive behaviour will not be tolerated. Please follow the [Contributor Covenant v2.1](https://www.contributor-covenant.org/version/2/1/code_of_conduct/).

---

## Getting Started

### Prerequisites

| Tool | Minimum Version | Purpose |
|------|----------------|---------|
| **Windows 10 / 11** | – | Runtime target; WPF requires Windows |
| **.NET SDK** | 10.0 | Build, test, publish |
| **Visual Studio 2022** or **VS Code + C# Dev Kit** | – | IDE |
| **LM Studio** | Latest | Local AI server (port `1234`) |

### Clone and build

```bash
git clone https://github.com/ScottyVenable/DesktopPal.git
cd DesktopPal

# Build entire solution
dotnet build DesktopPal.slnx

# Run the application
dotnet run --project DesktopPal/DesktopPal.csproj
```

---

## Project Structure

```
DesktopPal/
├── DesktopPal/                  # Main WPF application
│   ├── AIService.cs             # LM Studio (OpenAI-compatible) HTTP client
│   ├── App.xaml / App.xaml.cs  # Application entry point & global exception handling
│   ├── DebugLogger.cs           # Structured debug logging utility
│   ├── Decoration.cs            # Procedural world objects (Tree, Flower, Poop)
│   ├── MainWindow.xaml.cs       # Game loop, drag/drop, tray icon, vision system
│   ├── PetControl.xaml.cs       # Pet UI, chat bubble, stat interaction buttons
│   ├── PetState.cs              # Pet data model, persistence, stat decay
│   ├── SettingsWindow.xaml.cs   # Settings dialog
│   ├── SystemIntegrationService.cs  # Desktop file watcher, screen capture
│   └── WorldWindow.xaml.cs      # Background world canvas
│
├── DesktopPal.Tests/            # xUnit test project
│   ├── PetStateTests.cs         # Unit tests for PetState
│   ├── AIServiceTests.cs        # Unit tests for AIService (mocked HTTP)
│   ├── SystemIntegrationServiceTests.cs
│   └── DebugLoggerTests.cs
│
├── DesktopPal.slnx              # Solution file
├── CONTRIBUTING.md              # This file
├── HANDOFF.md                   # Technical handoff for AI agents / new developers
└── README.md                    # User-facing documentation
```

---

## Development Workflow

1. **Fork** the repository and create a feature branch:
   ```bash
   git checkout -b feat/my-feature
   ```

2. **Make your changes** – keep commits small and focused.

3. **Write or update tests** in `DesktopPal.Tests/`.

4. **Build and test** before pushing:
   ```bash
   dotnet build DesktopPal.slnx
   dotnet test DesktopPal.Tests/DesktopPal.Tests.csproj
   ```

5. **Push** and open a **Pull Request** against `main`.

---

## Coding Standards

### Language & framework
- **C# 12** with nullable reference types enabled (`<Nullable>enable</Nullable>`).
- **WPF on .NET 10** – avoid platform-agnostic alternatives that break the Windows overlay behaviour.
- Use `async`/`await` correctly: do not block the UI thread.

### Style
- Follow the [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions).
- Use `var` only when the type is obvious from the right-hand side.
- Prefer expression-bodied members for simple one-liners.
- XML doc comments (`///`) on all `public` and `internal` types and members.

### Naming
| Symbol | Convention | Example |
|--------|------------|---------|
| Classes, interfaces | PascalCase | `PetState`, `IAIService` |
| Methods | PascalCase | `ChatAsync`, `UpdateRealTime` |
| Private fields | _camelCase | `_velocity`, `_random` |
| Constants | PascalCase | `StatMax`, `BaseUrl` |
| Local variables | camelCase | `elapsed`, `reply` |

### Error handling
- **Never use bare `catch { }` or `catch (Exception) { /* ignore */ }`** – always log the exception with `DebugLogger.Error(...)`.
- Use specific exception types where possible (`IOException`, `HttpRequestException`, etc.).
- Return meaningful fallback values instead of `null` where the caller cannot handle `null` gracefully.

---

## Testing

Tests live in `DesktopPal.Tests/` and use **xUnit**.

```bash
# Run all tests (requires Windows runtime)
dotnet test DesktopPal.Tests/DesktopPal.Tests.csproj

# Run with coverage
dotnet test DesktopPal.Tests/DesktopPal.Tests.csproj --collect:"XPlat Code Coverage"
```

> **Note:** Because the project targets `net10.0-windows` (WPF), tests must be run on Windows. The build succeeds cross-platform, but `dotnet test` will fail on Linux/macOS.

### What to test

| Area | Strategy |
|------|----------|
| `PetState` | Pure logic – no mocks needed |
| `AIService` | Inject a custom `HttpMessageHandler` to avoid a live LM Studio server |
| `SystemIntegrationService` | Use a temporary directory; skip tests that need `%USERPROFILE%\Desktop` when the folder is absent |
| `DebugLogger` | Redirect `LogFilePath` via reflection and assert file contents |
| WPF UI | Out-of-scope for unit tests; use manual smoke testing or add UI automation separately |

### Writing new tests

1. Mirror the production file name: `Foo.cs` → `FooTests.cs`.
2. Use the Arrange-Act-Assert (AAA) pattern.
3. Give each test a descriptive name: `MethodName_Condition_ExpectedResult`.
4. Clean up any temp files / state in `IDisposable.Dispose()` or an `[AfterEach]` hook.

---

## Logging & Debugging

DesktopPal uses a lightweight built-in logger (`DebugLogger`) that writes to:

- **Visual Studio / Debug Output** window (`System.Diagnostics.Debug.WriteLine`)
- **`desktoppal.log`** in the application's base directory (rolling, trimmed at ~500 KB)

### Log levels

| Level | When to use |
|-------|-------------|
| `Debug` | High-frequency events (game-loop steps, minor state changes) |
| `Info` | Significant lifecycle events (startup, save, level-up, letter received) |
| `Warning` | Non-fatal unexpected states (empty file, missing desktop path) |
| `Error` | Caught exceptions, failed I/O, unexpected errors |

### Usage

```csharp
DebugLogger.Info("Pet state loaded successfully.");
DebugLogger.Warning("Desktop path not found – watcher not started.");
DebugLogger.Error("Failed to write reply.", ex);
DebugLogger.Debug($"Velocity: ({_velocity.X:F2}, {_velocity.Y:F2})");
```

Caller information (`[CallerMemberName]`, `[CallerFilePath]`) is captured automatically; no need to include the method name in the message string.

---

## Submitting Changes

### Pull Request checklist

Before marking a PR as **Ready for Review**:

- [ ] All new/changed code includes XML doc comments for public/internal members
- [ ] Error handling uses `DebugLogger.Error(...)` — no silent catches
- [ ] New logic has corresponding unit tests in `DesktopPal.Tests/`
- [ ] `dotnet build DesktopPal.slnx` succeeds with **0 errors**
- [ ] `dotnet test` passes on a Windows machine
- [ ] The PR description follows the template below

### PR description template

```markdown
## Summary
<!-- One paragraph explaining what this PR does and why. -->

## Changes
<!-- Bullet list of the most important code changes. -->

## Testing
<!-- Describe how you verified the change works correctly. -->

## Screenshots / Demo
<!-- Attach if the change affects the UI. -->

## Related Issues
<!-- e.g. Closes #42 -->
```

---

## Issue & PR Conventions

### Issue labels

| Label | Meaning |
|-------|---------|
| `bug` | Something is broken |
| `enhancement` | New feature or improvement |
| `documentation` | Docs-only change |
| `good first issue` | Suitable for newcomers |
| `help wanted` | Community assistance welcome |
| `needs-triage` | Newly opened, not yet reviewed |
| `wontfix` | Intentionally out of scope |

### Issue template fields (required)

When filing a bug:
- **Title** – concise description of the problem
- **Steps to reproduce** – numbered list
- **Expected behaviour**
- **Actual behaviour**
- **Environment** (OS version, .NET version, LM Studio version)
- **Logs** – paste the relevant section of `desktoppal.log`

When requesting a feature:
- **Title** – what you want to add
- **Motivation** – why it should be added
- **Proposed solution** – high-level description
- **Alternatives considered**

### Sub-issues

Break large features into sub-issues using GitHub's **sub-issues** feature (or a task list in the parent issue body). Each sub-issue must have:
- A clear, atomic scope
- The parent issue linked in the description
- Appropriate labels

---

## Roadmap Items

Priority items from `HANDOFF.md` that are open for contributions:

| # | Feature | Difficulty |
|---|---------|------------|
| 1 | **Vision Integration** – pass screenshots to a local vision model (Moondream/LLaVA) | Hard |
| 2 | **Animation Overhaul** – replace Ellipse placeholders with Lottie / sprite sheets | Medium |
| 3 | **Desktop Icon Manipulation** – move icons via `SysListView32` | Hard |
| 4 | **Farming/Gardening System** – interactive plots in `Decoration.cs` | Medium |
| 5 | **System Tray "Do Not Disturb"** – persist DND state across sessions | Easy |

Pick an item, open an issue, and mention `@ScottyVenable` to coordinate before starting large changes.

---

*Thank you for helping make DesktopPal better! 🐾*
