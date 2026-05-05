# Changelog

## [Unreleased]

- Mirrored the expanded `.tritium` team world folder into the workspace so Sol, Jesse, Vex, Rook, and Bridge share a versioned source-of-truth for journals, memories, and shared spaces alongside the code (#45).
- First-playable gardening loop(#3): added a `GardenPlot` decoration with an `Empty -> Seeded -> Sprout -> Bloom` lifecycle, a "Plant" action in the companion panel that spawns a plot near the pet, persistence via a new `World` field on `PetState` (back-compatible — older saves load with an empty world), and offline catch-up that advances plot states using the same elapsed-real-time pattern as stat decay. Bloom plots are clickable for a small Happiness + Experience reward and reset to Empty for re-planting. Transition delays are short, configurable consts (`SeededToSproutDelay`, `SproutToBloomDelay`) for MVP visibility. `CleanAll` now preserves garden plots while still wiping ephemeral decorations.
- Added a first-run onboarding window that introduces the pet, surfaces the companion-panel hotkey (read live from `PetState`), and explains the tray fallback plus feed/clean basics. Persisted via a new `HasCompletedOnboarding` flag on `PetState` so it appears once per installation; placeholder copy uses the blue-bear voice and will be replaced from `docs/content/onboarding.md` once Vex's pass lands (#20).
- Pet visuals now have animation-ready groundwork: a gentle ~3px / 1.6s idle bob storyboard and a randomised 3–6s blink that briefly hides the active face for ~120ms. Idle bob auto-pauses while the pet is being dragged and resumes on release. This is scaffolding only — the full state-machine-driven animation system (sprite atlas / Lottie option) is intentionally deferred (#2).
- Removed the legacy `PalDrawer` user control and its supporting paw-toggle / drawer-button styles, fully retiring the drawer surface in favor of the companion panel and tray fallback (#31).
- Cleaned up an empty `OnDrawerResized` shim from `MainWindow` left over from the drawer migration (#31).
- Pet movement and initial spawn now respect `SystemParameters.WorkArea` on all four edges, so the pet no longer wanders under or onto the taskbar regardless of taskbar position (#12).
- Poop decorations are now clamped to the working area so droppings cannot spawn under the taskbar (#12).
- Documented multi-monitor scope in `MainWindow` comments: pet stays inside the primary display's WorkArea for now (#12).
- Ported the authored offline phrase library from `docs/content/offline-phrases.md` into `OfflineBrain.cs`. Idle / Hungry / Petting / Calling lists expanded to match the source of truth, and four new categories — Sleepy, Excited, Curious, Encouraging — are now wired into `GetRandomPhrase`.

- Replaced the paw drawer as the primary control surface with a hotkey-driven companion panel, while keeping the desktop pet overlay and a minimal tray fallback.
- Refined the blue bear visual pass so the pet reads more clearly at desktop-icon scale and aligns more closely with the repository reference art.
- Added `Logging` writing to `%LOCALAPPDATA%\DesktopPal\logs\desktoppal.log` with rotation; replaces silent `catch` blocks across system integration and AI paths (#17, #18).
- Hardened `SystemIntegrationService`: now `IDisposable`, retains and disposes the `FileSystemWatcher` cleanly, detaches handlers, surfaces watcher errors via the logger, and is disposed during shutdown alongside the tray icon, hotkey, world window, and companion window (#17).
- AI degraded mode: `AIService` now exposes an `AIServiceStatus` (Unknown / Available / Unavailable / Error) with a `StatusChanged` event, applies a 10 s timeout via `CancellationToken`, distinguishes timeouts / unreachable hosts / malformed responses, and falls back to `OfflineBrain` phrases instead of returning raw error strings (#18).
- Companion panel header now shows an LM Studio status dot and label (green / amber / red / grey) with a tooltip describing the last failure (#18).
- Cleared remaining build warnings: nullability fixes in `AIService` and `SystemIntegrationService`, corrected `GameLoop` delegate signature, and removed the explicit `System.Drawing.Common` PackageReference (provided implicitly by the Windows Desktop runtime) to silence NU1510 (#33).

