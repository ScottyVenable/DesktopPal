# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Build
dotnet build DesktopPal/DesktopPal.csproj

# Run
dotnet run --project DesktopPal/DesktopPal.csproj

# Run (from within the DesktopPal/ subdirectory)
dotnet run
```

**Prerequisite:** LM Studio must be running with a model loaded and the local server active on `http://localhost:1234` before launching. The app falls back gracefully with an error message in chat bubbles if LM Studio is unavailable.

## Architecture

This is a **C# .NET 10 WPF** desktop overlay application. There are no tests. The project file is at `DesktopPal/DesktopPal.csproj`.

### Two-Window Model
The app runs two transparent, full-screen WPF windows simultaneously:

- **`WorldWindow`** — pinned to the bottom of the Z-order via `SetWindowPos(HWND_BOTTOM)`. Acts as the "ground layer" canvas where world objects (decorations, poop) are placed. Never comes to the foreground.
- **`MainWindow`** — `Topmost` by default, hosts the `PetControl` on a `Canvas`. Dynamically drops to non-topmost when the pet wanders under a foreground application window (smart layering).

Both windows use `AllowsTransparency="True"` with transparent backgrounds so the desktop and wallpaper show through.

### Core Components

| File | Role |
|------|------|
| `PetState.cs` | Data model; JSON persistence to `pet_state.json` next to the exe; offline time-decay via `UpdateRealTime()` on load |
| `PetControl.xaml/.cs` | The pet `UserControl`; owns `PetState` and `AIService` instances; handles chat bubbles, stat bars, feeding, petting |
| `MainWindow.xaml.cs` | Game loop (`CompositionTarget.Rendering`); movement/wandering/dragging; global hotkey via `RegisterHotKey`; system tray; letter dispatch |
| `AIService.cs` | POSTs to LM Studio OpenAI-compatible endpoint; maintains a rolling 10-message chat history |
| `OfflineBrain.cs` | Static phrase lists for when LM Studio is unavailable (Idle, Hungry, Petting, Calling categories) |
| `SystemIntegrationService.cs` | `FileSystemWatcher` on the Desktop for `.txt` letters; screen capture stub |
| `Decoration.cs` | Procedural WPF `UserControl`s rendered inline: Tree, Flower, Poop. Poop is clickable to clean up |
| `SettingsWindow.xaml.cs` | Modal dialog for pet name, LM Studio model name, vision toggle, global hotkey key |

### State & Persistence
`PetState` serializes to `pet_state.json` in the exe's base directory. On startup, `UpdateRealTime()` computes elapsed hours since `LastSeen` and decays stats (Hunger −5/hr, Hygiene −3/hr, Happiness −2/hr, Energy −4/hr). The pet hatches automatically after 10 minutes offline or 1 minute in-app (the in-app threshold exists for fast testing). The save timer fires every minute via a `DispatcherTimer`.

### Namespace Disambiguation
Both WPF (`System.Windows`) and WinForms/System.Drawing are referenced in the same project (`UseWPF` and `UseWindowsForms` both true). Ambiguous types like `Point`, `Rectangle`, and `UserControl` must be fully qualified — always use `System.Windows.Point`, `System.Windows.Controls.UserControl`, etc.

### Global Hotkey
The default hotkey is **Ctrl+Alt+B** (modifier `3`, key `0x42`). Hotkey registration happens in `OnSourceInitialized` via Win32 `RegisterHotKey`. After changing the hotkey in Settings, `MainWindow.RegisterGlobalHotkey()` must be called to re-register it — `SettingsWindow` does this automatically on save.

### Letter System
`SystemIntegrationService.WatchDesktop()` monitors `%USERPROFILE%\Desktop` for new `.txt` files. On creation, `MainWindow.HandleLetterReceived` passes the content to `AIService.ChatAsync` and writes the reply as `Reply_<original filename>` on the Desktop.

### Vision System
`HandleVision()` fires every 5 minutes via timer. It reads the active foreground window title via `GetWindowText` and sends a contextual prompt to LM Studio. Full screenshot capture is stubbed in `SystemIntegrationService` but not yet wired to a vision model.
