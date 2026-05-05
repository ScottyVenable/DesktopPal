# DesktopPal: Technical Handoff Document

This document provides a comprehensive overview of the **DesktopPal** project for future developers or AI agents (e.g., Claude Code).

## 1. Project Overview
**DesktopPal** is a C# .NET WPF application that acts as a transparent desktop overlay companion. It features virtual pet mechanics, local AI integration, and deep system hooks.

## 2. Technical Stack
- **Framework:** .NET 10.0 (WPF)
- **Language:** C#
- **Dependencies:** 
  - `System.Drawing.Common`: Used for screen capture/vision logic.
  - `LM Studio`: External local LLM server (expected on `http://localhost:1234`).
- **Namespace Handling:** Note that both WPF and WinForms/System.Drawing are used. High-level classes (like `Point`, `Rectangle`, `UserControl`) are disambiguated with full namespaces (e.g., `System.Windows.Point`).

## 3. Core Architecture
- **`MainWindow.xaml`**: A borderless, transparent, full-screen `Topmost` window. It acts as the "world" canvas.
- **`PetControl.xaml`**: The primary UI component for the pet. Handles the visual representation, chat bubbles, and the interaction menu.
- **`PetState.cs`**: The data model. Handles JSON serialization (`pet_state.json`), stat decay (hunger, hygiene, etc.), and real-time growth calculations based on timestamps.
- **`AIService.cs`**: Orchestrates communication with LM Studio. Maintains a short-term chat history and handles system prompts.
- **`SystemIntegrationService.cs`**:
  - `FileSystemWatcher`: Monitors the Desktop for `.txt` files.
  - `ScreenCapture`: Logic for taking snapshots of the desktop (for future Vision integration).
- **`Decoration.cs`**: Procedural UI elements (Trees, Flowers) that the pet can "plant" on the canvas.

## 4. Key Systems & Workflows
### 4.1. Real-Time Growth
The `UpdateRealTime()` method in `PetState.cs` calculates the delta between `LastSeen` and `DateTime.Now` on startup. This ensures the pet "lives" even when the app is closed.

### 4.2. Letter System
The `FileSystemWatcher` triggers `HandleLetterReceived` in `MainWindow`. The AI processes the content and uses `SystemIntegrationService` to write a `Reply_` file back to the Desktop.

### 4.3. Transparency & Interaction
The `MainWindow` uses `AllowsTransparency="True"`. WPF handles per-pixel alpha, meaning the background is click-through to the desktop icons, but the Pet's sprite remains clickable for dragging/context menus.

## 5. Current Feature Status

### Implemented prototype capabilities
- [x] Transparent overlay foundation
- [x] Two-window desktop presentation model
- [x] Drag and drop pet interaction
- [x] Persistent pet stats and offline decay
- [x] LM Studio chat integration
- [x] Letter reading and reply writing through desktop `.txt` files
- [x] Basic tray/settings/hotkey controls
- [x] Simple world objects and cleanup interactions

### Partial or prototype-only capabilities
- [~] Screen awareness groundwork exists, but there is no finished opt-in vision pipeline
- [~] Visual presentation and animation are functional but still placeholder/prototype grade
- [~] Decorations exist, but a persistent world system does not

### Not yet implemented
- [ ] Automated tests
- [ ] CI workflows
- [ ] Packaging and release pipeline
- [ ] Multi-pet support
- [ ] Production-ready memory system

## 6. Repository and delivery status

- The roadmap source of truth is `TRITIUM-PLAN.md`.
- Contributor and triage guidance lives in `CONTRIBUTING.md` and `docs/ROADMAP.md`.
- Historical notes in `plans/` should be treated as reference input, not the current execution plan.
- The repository is still pre-release and should be described as a prototype unless a milestone explicitly upgrades that status.

## 7. Recommended next work

Prioritize the near-term Tritium work instead of adding new speculative features:

1. Extract simulation/runtime behavior from frame-tied UI code.
2. Define save-path, versioning, and recovery strategy.
3. Harden system integrations and degraded-mode AI behavior.
4. Add a first test baseline and CI workflow.
5. Consolidate onboarding, settings, and drawer UX once the runtime direction is clearer.

## 8. How to Develop
1. Ensure .NET 10 SDK is installed.
2. Run `dotnet build DesktopPal/DesktopPal.csproj`.
3. Start LM Studio on port 1234 if you need AI chat features.
4. Review `TRITIUM-PLAN.md`, `docs/ROADMAP.md`, and `CONTRIBUTING.md` before planning new work.
