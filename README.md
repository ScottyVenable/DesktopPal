# 🐾 DesktopPal

![Platform](https://img.shields.io/badge/platform-Windows%2010%2F11-blue)
![.NET](https://img.shields.io/badge/.NET-10.0-purple)
![License](https://img.shields.io/badge/license-MIT-green)
![Build](https://img.shields.io/badge/build-passing-brightgreen)

**DesktopPal** is an advanced, AI-driven virtual pet and desktop companion that lives directly on your Windows desktop. Powered by local LLMs via **LM Studio**, your pal grows in real-time, learns from your interactions, and observes your digital life to become a unique, lifelong companion.

---

## ✨ Features

| Feature | Description |
|---------|-------------|
| 🏠 **Desktop Overlay** | Transparent, borderless window — your pet wanders across your wallpaper without interfering with your work |
| 🧠 **Local AI Brain** | Full integration with LM Studio; personality, memory, and complex conversations |
| 🐣 **Real-Time Growth** | Starts as an egg and evolves based on care — time passes even when your PC is off |
| ✉️ **Desktop Correspondence** | Drop a `.txt` file on your desktop and your pal will read and reply to it |
| 👀 **Screen Awareness** | Periodically detects your active window and comments on what you are doing |
| 🛠️ **Deep Integration** | Interaction with desktop icons, procedural decorations (trees, flowers), and system-level awareness |
| 🖱️ **Interactive** | Drag and drop your pal, right-click for stats, or watch it wander in its 3/4-perspective world |
| 🔔 **System Tray** | Do-Not-Disturb toggle and settings accessible from the Windows taskbar |
| 📋 **Debug Logging** | Rolling log file (`desktoppal.log`) with structured levels for easy troubleshooting |

---

## 🚀 Getting Started

### Prerequisites

- **Windows 10 / 11**
- **.NET 10 SDK** — [download](https://dotnet.microsoft.com/download)
- **[LM Studio](https://lmstudio.ai/)** — running with a model loaded and the local server started on port `1234`

### Installation

```bash
# 1. Clone the repository
git clone https://github.com/ScottyVenable/DesktopPal.git
cd DesktopPal

# 2. Build the solution
dotnet build DesktopPal.slnx

# 3. Run the application
dotnet run --project DesktopPal/DesktopPal.csproj
```

---

## 🎮 How to Play

| Action | Result |
|--------|--------|
| **Left-Click & Drag** | Pick up your pal and move it around |
| **Right-Click** | Toggle the stats / status bubble |
| **Feed button** | Increase hunger by 20% |
| **Clean button** | Restore hygiene to 100% |
| **Chat input** | Talk to your pal (requires LM Studio) |
| **Write a letter** | Save a `.txt` file on your Desktop — your pal will reply with a `Reply_` file |
| **Tray icon → Settings** | Change the pet's name, AI model, and vision settings |
| **Tray icon → Do Not Disturb** | Hide the pet without closing the app |

---

## 🛠️ Technical Architecture

DesktopPal is built with **C# .NET 10 WPF**, utilising:

| Component | Technology |
|-----------|------------|
| Desktop overlay | WPF `AllowsTransparency` + `WindowStyle="None"` |
| Window management | Win32 API (`user32.dll`) |
| Letter system | `FileSystemWatcher` on `%USERPROFILE%\Desktop` |
| AI communication | `HttpClient` → LM Studio OpenAI-compatible endpoint |
| State persistence | JSON (`pet_state.json`) via `System.Text.Json` |
| Screen capture | `System.Drawing.Common` + `Graphics.CopyFromScreen` |
| Debug logging | Built-in `DebugLogger` (file + Debug output) |

### Key files

```
DesktopPal/
├── AIService.cs              # LM Studio HTTP client with retry/timeout
├── App.xaml.cs               # Global exception handlers
├── DebugLogger.cs            # Structured logging (Debug/Info/Warning/Error)
├── MainWindow.xaml.cs        # Game loop, vision system, tray icon
├── PetControl.xaml.cs        # Pet UI, chat, feeding, cleaning
├── PetState.cs               # Data model, persistence, stat decay, levelling
└── SystemIntegrationService.cs  # File watcher, screen capture, letter replies
```

---

## 🧪 Testing

```bash
# Build and run all unit tests (Windows required)
dotnet test DesktopPal.Tests/DesktopPal.Tests.csproj

# Build only (works cross-platform)
dotnet build DesktopPal.slnx
```

The test project (`DesktopPal.Tests`) uses **xUnit** and covers:
- `PetState` — stat clamping, decay, level progression, save/load
- `AIService` — mocked HTTP responses, error handling, history management
- `SystemIntegrationService` — file I/O helpers, dispose lifecycle
- `DebugLogger` — log levels, enable/disable, file output

---

## 🗺️ Roadmap

- [ ] **Vision System** — AI-powered analysis of what is on screen (Moondream / LLaVA)
- [ ] **Sprite Animations** — replace Ellipse placeholders with Lottie or sprite sheets
- [ ] **Gardening System** — interactive farm plots that grow over days
- [ ] **Desktop Icon Manipulation** — move icons via `SysListView32`
- [ ] **Multi-Pet Support** — have a whole family of pals!

See [CONTRIBUTING.md](CONTRIBUTING.md) for how to pick up a roadmap item.

---

## 🤝 Contributing

Contributions are welcome! Please read [CONTRIBUTING.md](CONTRIBUTING.md) for:
- Development setup and workflow
- Coding standards and naming conventions
- How to write tests and use the debug logger
- PR and issue templates

---

## 📜 License

This project is licensed under the **MIT License** — see the [LICENSE](LICENSE) file for details.

