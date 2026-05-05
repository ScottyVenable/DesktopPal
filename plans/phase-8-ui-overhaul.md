# DesktopPal Phase 8: Interaction & UI Overhaul Plan

## 1. Interaction Redesign
### 1.1. Contextual Right-Click
- **Goal:** Integrate DesktopPal commands into the Windows Desktop right-click menu or simulate a global right-click hook.
- **Approach:** Since modifying the actual Windows Shell menu is high-risk, we will implement a "Global Click Listener". When the user right-clicks on the empty desktop, a custom, stylized WPF context menu will appear at the mouse position.
- **Commands:** Call Buddy, Clean All Poops, Plant Seed, Toggle DND.

### 1.2. Pet Interaction
- **Right-Click Buddy:** Triggers a "Speak" action (AI response) and a "Pet" animation.
- **Left-Click Buddy:** Selects him/picks him up.
- **Interaction Modes:** A small floating toggle to change what clicks do (e.g., "Inspect Mode" vs "Play Mode").

## 2. Perspective Correction
- **Current Issue:** Buddy gets smaller as he moves "up" (simulating a horizon line).
- **Correction:** Switch to "Top-Down 3/4 RPG Perspective". The scale remains consistent (1.0) regardless of Y position, as the camera is angled from above looking down on a flat plane. Vertical movement is now just "walking north" rather than "walking away".

## 3. The "Pal Drawer" (Main UI)
- **The Trigger:** A small, cute, semi-transparent button anchored just above the Taskbar (Bottom-Right).
- **The Drawer:** Clicking the trigger slides out a horizontal "Drawer" (Dock) containing icons for primary categories.
- **The Full Menu:** One icon in the drawer expands into a full-screen or centered stylized window with tabs:
    - **📊 Stats:** Detailed view of Level, EXP, Hunger, Hygiene, Happiness, and Ability Scores.
    - **📦 Store:** Buy decorations, food, and toys using "Buddy Points" earned by interactions.
    - **🛠️ Tools:** Access to "Call to Mouse", "Clean Screen", and "Screenshot Vision".
    - **⚙️ Settings:** All existing settings migrated here.

## 4. New Mechanics
- **Call to Mouse:** Buddy stops wandering and pathfinds directly to your current cursor position.
- **Cleaning:** Clicking a "Poop" decoration in the `WorldWindow` removes it and grants hygiene points.
- **Petting:** Right-clicking Buddy repeatedly increases Happiness and triggers a unique blush emote.

## 5. Technical Implementation Steps
1.  **Perspective Update:** Remove depth-based scaling from `MainWindow.xaml.cs`.
2.  **Drawer Component:** Create `PalDrawer.xaml` with slide animations using `DoubleAnimation`.
3.  **Command Hook:** Implement a global mouse hook (via `SetWindowsHookEx` or a hidden transparent overlay) to catch desktop right-clicks.
4.  **Action Logic:** Add `CallToMouse()` and `CleanAll()` methods to `MainWindow`.

## 6. Visual Style
- **Theme:** "Cozy Modern". Rounded corners (CornerRadius="20"), Pastel colors, glassmorphism (Opacity/Blur), and playful animations.
