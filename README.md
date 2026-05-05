# 🐾 DesktopPal

**DesktopPal** is an advanced, AI-driven virtual pet and desktop companion that lives directly on your Windows desktop. Powered by local LLMs via **LM Studio**, your pal grows in real-time, learns from your interactions, and observes your digital life to become a unique, lifelong companion.

---

## ✨ Features

- **🏠 Lives on the Desktop:** A transparent, borderless overlay that lets your pet wander across your wallpaper without interfering with your work.
- **🧠 Local AI Brain:** Full integration with LM Studio. Your pet isn't just a script; it has a personality, memory, and can hold complex conversations.
- **🐣 Real-Time Growth:** Your pet starts as an egg and evolves based on how you care for it. Time passes even when your PC is off!
- **✉️ Desktop Correspondence:** Write a `.txt` file on your desktop and your pal will "read" it and write a reply back to you.
- **👀 Screen Awareness:** Your pal periodically "looks" at what you're doing and might offer comments or encouragement.
- **🛠️ Deep Integration:** Interaction with desktop icons, procedural decorations (trees, flowers), and system-level awareness.
- **🖱️ Interactive:** Drag and drop your pal, right-click to check stats, or just watch it wander in its 3/4 perspective world.

---

## 🚀 Getting Started

### Prerequisites
- **Windows 10/11**
- **.NET 10 SDK**
- **[LM Studio](https://lmstudio.ai/)**: Running with a model loaded and the local server started on port `1234`.

### Installation
1. Clone the repository:
   ```bash
   git clone https://github.com/YOUR_USERNAME/DesktopPal.git
   ```
2. Navigate to the project folder:
   ```bash
   cd DesktopPal/DesktopPal
   ```
3. Run the application:
   ```bash
   dotnet run
   ```

---

## 🎮 How to Play
- **Left-Click & Drag:** Pick up your pal and move it around.
- **Right-Click:** Open the interaction menu to feed, clean, or chat.
- **Write Letters:** Save a `.txt` file on your desktop. Your pal will respond with a `Reply_` file.
- **Care:** Keep an eye on Hunger and Happiness. Neglected pets grow slower and might become mischievous!

---

## 🛠️ Technical Architecture
DesktopPal is built with **C# .NET WPF**, utilizing:
- **WPF Transparency:** For the seamless desktop overlay.
- **Win32 API (User32.dll):** For advanced window management and desktop icon interaction.
- **FileSystemWatcher:** For the real-time "Letter" system.
- **JSON Persistence:** For secure, real-time state management.

---

## 🗺️ Roadmap
- [ ] **Vision System:** AI-powered analysis of what's actually on the screen.
- [ ] **Sprite Animations:** Moving from placeholders to high-quality 2D animations.
- [ ] **Gardening System:** Interactive farm plots that grow over days.
- [ ] **Multi-Pet Support:** Have a whole family of pals!

---

## 📜 License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
