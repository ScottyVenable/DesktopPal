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
- [x] Transparent Overlay Foundation
- [x] 3/4 Perspective Movement
- [x] Drag & Drop Mechanics
- [x] Stats & Persistence (Hunger, Happiness, etc.)
- [x] LM Studio Chat Integration
- [x] Letter Reading/Writing
- [x] Basic Screen Awareness (Screenshot logic)
- [x] Procedural Decorations (Planting trees/flowers)

## 6. Future Roadmap (Recommended for Next Agent)
1. **Vision Integration:** Pass the screenshots from `CaptureScreen()` to a local Vision model (like Moondream or Llava) via LM Studio to let the pet "see" specific apps.
2. **Animation Overhaul:** Replace the `Ellipse` placeholders with `Lottie` animations or Sprite sheets for walking, sleeping, and eating.
3. **Desktop Icon Manipulation:** Use `User32.dll` and `FindWindowEx` to locate `SysListView32` and programmatically move icons.
4. **Farming/Gardening System:** Expand `Decoration.cs` to include interactive plots that the user can water.
5. **System Tray Integration:** Add a "Do Not Disturb" toggle and settings menu in the Windows System Tray.

## 7. How to Develop
1. Ensure .NET 10 SDK is installed.
2. Run `dotnet build` to verify namespaces.
3. Ensure LM Studio is active on port 1234 before running.
