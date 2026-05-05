# Packaging & Distribution

**Status:** Draft
**Owner:** Rook (QA & Release Engineer)
**Issue:** #23 — Choose packaging path and publish release readiness checklist
**Milestone:** Tritium-0
**Last updated:** 2025

---

## Abstract

DesktopPal currently has no packaging story. Users can only run the app from
source via `dotnet run`. This document evaluates five distribution options for
a Windows-only WPF/WinForms desktop pet that depends on the .NET 10 desktop
runtime, native Win32 hooks, and (eventually) a local AI model. It recommends
a **phased approach starting with self-contained single-file publish**,
graduating to a signed installer (Inno Setup or WiX) once a code-signing
certificate is available, and reserving Velopack for the auto-update phase.

---

## Goals

- Let a non-technical Windows user install DesktopPal in under one minute
  without touching a terminal, the .NET SDK, or a `git clone`.
- Produce a single GitHub Release artifact per tag that QA can smoke-test on
  a clean Windows VM.
- Keep the packaging step reproducible from CI on a stock `windows-latest`
  runner with no per-developer machine state.
- Preserve the app's ability to install global Win32 mouse/keyboard hooks,
  read input device state, and write to `%LOCALAPPDATA%\DesktopPal\`.
- Make version metadata (FileVersion, ProductVersion, Release tag, CHANGELOG
  heading) line up exactly across binary, artifact filename, and Release page.

## Non-Goals

- Cross-platform builds. DesktopPal is Windows-only by design
  (`net10.0-windows`, WPF, WinForms, Win32 hooks).
- Microsoft Store distribution at v1.x. The sandbox model is incompatible
  with global input hooks and free-roaming overlay windows (see Option 3).
- Enterprise mass deployment (Group Policy, MSI ADMX). Out of scope until
  there is demand.
- Auto-update infrastructure at v0.x. Manual download from GitHub Releases is
  acceptable for the alpha audience.

---

## Current State

- Project type: `WinExe`, `TargetFramework=net10.0-windows`,
  `UseWPF=true`, `UseWindowsForms=true`.
- No `dotnet publish` step exists in CI or in any local script.
- No build configuration files are checked in (no `electron-builder.yml`
  equivalent, no `.iss`, no `.wxs`, no `Package.appxmanifest`).
- No code-signing certificate is owned by the project.
- The repo's only "build" is `dotnet build` and `dotnet run` from the
  `DesktopPal/` folder, both of which require the .NET 10 SDK.
- Effect on users: zero non-developers can run the app today.

---

## Options Analysis

### 1. Self-contained single-file publish

```
dotnet publish -c Release -r win-x64 --self-contained true \
  /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true
```

Produces a single `DesktopPal.exe` (~70-150 MB) that bundles the .NET 10
desktop runtime. User double-clicks the EXE; nothing else to install.

**Pros**
- Lowest friction install for non-technical users — one file, no
  prerequisites, no installer UAC prompt.
- Reproducible from any `windows-latest` runner with the .NET 10 SDK; no
  third-party tooling.
- Win32 hooks work normally (no sandbox).
- Trivial rollback: keep the previous `.exe`.

**Cons**
- Large download (~70-150 MB) because the runtime travels with each release.
- No Start menu entry, no uninstaller, no file association registration —
  users have to manage the EXE themselves.
- Unsigned single-file EXEs are **strong SmartScreen and antivirus magnets**
  (Option 4 doesn't fix this without a cert, but installers are slightly
  better-tolerated).
- `%LOCALAPPDATA%\DesktopPal\` is created on first run, but there is no
  uninstall path that cleans it up.

### 2. Framework-dependent publish + .NET 10 prerequisite

```
dotnet publish -c Release -r win-x64 --self-contained false \
  /p:PublishSingleFile=true
```

Produces a small (~5-15 MB) EXE that requires the user to have the .NET 10
Desktop Runtime installed.

**Pros**
- Tiny artifact; fast download; trivial CDN cost.
- Runtime updates (security patches) come from Microsoft, not from us.

**Cons**
- **Hard blocker for the non-technical audience.** First-launch failure with
  a "this app requires .NET 10" dialog is a 100% drop-off for the target
  user base.
- .NET 10 is brand-new. Most consumer machines do not have it preinstalled
  and won't for a long time.
- Every issue report becomes "did you install the runtime?" triage noise.

### 3. MSIX package

Wrap the publish output in an `.msix` (or `.msixbundle`) package, optionally
listed on the Microsoft Store.

**Pros**
- Clean install/uninstall, automatic updates if shipped via Store.
- Per-user install with no admin required.
- Store listing gives discoverability we cannot otherwise buy.

**Cons**
- **MSIX containerization breaks low-level Win32 input hooks** that
  DesktopPal needs (e.g. `SetWindowsHookEx(WH_MOUSE_LL/WH_KEYBOARD_LL)`,
  raw input across other apps' windows). Sandboxed apps can hook their own
  process but cannot install system-wide hooks without `runFullTrust`,
  which the Store rejects for consumer apps.
- Overlay/topmost free-roaming windows that interact with other desktops are
  restricted under Store policy.
- Local AI model files would have to live in package-relative storage with
  size limits and update-package re-download semantics.
- Sideloaded MSIX still needs a code-signing cert, same as Option 4.

**Verdict:** MSIX is incompatible with the product's core behaviour.
Defer indefinitely.

### 4. Inno Setup or WiX MSI installer wrapping a self-contained publish

Take the Option 1 output and wrap it in an installer (Inno Setup `.iss` is
simpler; WiX `.wxs` is more enterprise-friendly and produces a true MSI).

**Pros**
- Real Windows install experience: Start menu shortcut, desktop shortcut,
  Add/Remove Programs entry, clean uninstall, file/protocol associations.
- A signed installer dramatically reduces SmartScreen friction even though
  the inner EXE is the same size.
- Inno Setup is free, scriptable, and runs on `windows-latest` runners.
- WiX integrates with MSI infrastructure if enterprise deployment ever
  matters.

**Cons**
- Requires authoring and maintaining an `.iss` (or `.wxs`) script.
- Without a code-signing certificate, the installer itself trips SmartScreen
  the first time. Cert cost: ~$200-500/year (OV) or ~$300-600/year (EV with
  hardware token). EV gives instant SmartScreen reputation; OV builds
  reputation over time.
- Adds a CI step and a build dependency (Inno Setup or WiX toolset on the
  runner).

### 5. Velopack / Squirrel auto-update flow

Velopack (the actively maintained successor to Squirrel.Windows) builds
delta-update-capable installers and a release feed.

**Pros**
- Background auto-update is the right long-term answer for a desktop pet
  that ships often.
- Delta updates keep download size reasonable even with a self-contained
  runtime.
- First-run experience is as smooth as Option 4.

**Cons**
- Requires hosting a release feed (GitHub Releases works, but the schema is
  Velopack-specific) and a stable update cadence to be worth the setup
  cost.
- Adds a runtime dependency on Velopack's update bootstrapper; failures in
  the updater become our support burden.
- Premature at v0.x. We do not yet have a release cadence to update against.

---

## Recommendation

**Adopt Option 1 (self-contained single-file publish) immediately. Plan to
graduate to Option 4 (Inno Setup installer wrapping the same publish output)
once a code-signing certificate is funded. Defer Option 5 (Velopack) until
the project has a steady release cadence and a signed installer baseline.
Reject Options 2 and 3 outright.**

Justification, weighted to DesktopPal's specific constraints:

1. **Windows-only, native hooks required.** Eliminates MSIX (Option 3).
2. **Non-technical audience.** Eliminates "install the .NET runtime first"
   (Option 2). The target user does not know what a runtime is.
3. **Local AI model on-disk.** Tolerates a large artifact. The marginal cost
   of bundling the .NET runtime (~70 MB) is small next to a model file.
4. **No code-signing cert today.** Eliminates "ship a polished installer
   immediately" — an unsigned MSI is no better than an unsigned EXE for
   SmartScreen, and the installer-authoring cost is wasted until we sign.
5. **Velocity.** Option 1 is implementable in one CI job and one publish
   command. Ship it now; iterate.

---

## Release Readiness Checklist

A release is **not ready** until every box is checked. Owner is the agent
listed in brackets; Rook signs off the final go/no-go.

- [ ] `dotnet build -c Release` is green on the source branch tip [Sol]
- [ ] `dotnet test` is green (when a test project exists) [Sol]
- [ ] Version bumped in `DesktopPal/DesktopPal.csproj`
      (`<Version>`, `<FileVersion>`, `<AssemblyVersion>`) [Sol]
- [ ] `CHANGELOG.md` has a section for the new version with `Added`,
      `Changed`, `Fixed` subsections and the release date [Sol / Vex]
- [ ] `dotnet publish -c Release -r win-x64 --self-contained true
      /p:PublishSingleFile=true` produces a working
      `DesktopPal.exe` [Rook]
- [ ] Artifact filename matches `DesktopPal-vX.Y.Z-win-x64.exe` [Rook]
- [ ] `DesktopPal.exe` Properties → Details shows the bumped FileVersion and
      ProductVersion [Rook]
- [ ] Smoke test on a clean Windows 10/11 VM with **no .NET SDK installed**:
      app launches, pet appears on screen, save state writes to
      `%LOCALAPPDATA%\DesktopPal\`, app exits cleanly [Rook]
- [ ] Screenshots captured at 1280x720 and saved to
      `screenshots/release-vX.Y.Z/` [Rook]
- [ ] Release notes drafted from the CHANGELOG entry, with known issues
      called out [Sol / Vex]
- [ ] Code-signing applied to `DesktopPal.exe`
      (defer with `N/A — unsigned alpha` until cert is acquired) [Rook]
- [ ] GitHub Release created against the tag, artifact attached, notes
      pasted, marked Pre-release for non-`main` tags [Sol]
- [ ] No open `priority:p0` or `priority:p1` issues remain in the milestone
      [Jesse / Rook]

---

## Migration Path

**Phase A — Single-file publish (Tritium-0 / Tritium-1).**
Add a `publish` job to CI that runs the Option 1 command on push to
`alpha`. Upload the resulting EXE as an artifact. Manually attach to the
GitHub Release for tagged builds. Document the SmartScreen warning in the
README so users know what to expect.

**Phase B — Inno Setup installer, still unsigned (Tritium-2).**
Add an `.iss` script that wraps the Phase A publish output. Adds Start menu
entry, uninstaller, and Add/Remove Programs presence. Still unsigned;
SmartScreen still fires on first install.

**Phase C — Code signing (when budget allows).**
Acquire an OV or EV code-signing certificate. Sign both the inner EXE and
the installer. SmartScreen reputation builds (OV) or is instant (EV).
This is the point where we can credibly market to non-technical users.

**Phase D — Velopack auto-update (post-1.0).**
Adopt Velopack only after Phase C is stable and we have a predictable
release cadence (e.g. monthly). Migrate the release feed to Velopack's
schema and ship a self-updating installer.

---

## Risks

| Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- |
| Code-signing cert cost blocks Phase C indefinitely | Medium | High (user trust) | Document SmartScreen workaround in README; revisit budget each milestone; consider sponsorship/grants |
| SmartScreen / Defender flags unsigned single-file EXE as suspicious | High | High (user drop-off) | Clear README guidance with screenshots; submit binary to Microsoft for false-positive review after each release; consider Phase B installer sooner if reports spike |
| .NET 10 SDK not yet on `windows-latest` runners or has breaking changes | Medium | Medium (CI breakage) | Pin SDK version in a `global.json`; have Rook verify the runner image supports `net10.0-windows` before each release |
| Win32 API permissions denied on locked-down corporate machines (no global hooks, no `%LOCALAPPDATA%` write) | Medium | Medium (silent feature failure) | Detect hook installation failure at startup; fall back to a degraded mode with a clear message; log to `%LOCALAPPDATA%\DesktopPal\logs\` |
| Single-file EXE first-launch extraction is slow on HDDs / scanned by AV | Medium | Low | Document expected ~5-15s first-launch delay; consider `IncludeAllContentForSelfExtract` tradeoffs |
| Artifact size (~150 MB) when local AI model ships in-package | Medium | Medium (download abandonment) | Ship model as separate optional download fetched on first run; keep base EXE under 100 MB |
| No uninstall path in Phase A leaves orphaned `%LOCALAPPDATA%\DesktopPal\` | Low | Low | Phase B installer adds clean uninstall; document manual cleanup in README until then |

---

## Follow-up Issues to Consider

The following are candidates for Jesse to triage into the backlog. **Rook is
not creating these — this is a recommendation list only.**

- Add `dotnet publish` step to CI producing `DesktopPal-vX.Y.Z-win-x64.exe`
  on tag push (Phase A implementation).
- Add `global.json` pinning the .NET 10 SDK version used by CI.
- Add a startup self-check that verifies hook installation and writable
  `%LOCALAPPDATA%\DesktopPal\` and surfaces a degraded-mode banner.
- Author Inno Setup `.iss` wrapping the publish output (Phase B).
- Research and budget an OV or EV code-signing certificate (Phase C).
- Document the SmartScreen first-launch flow with screenshots in the README.

— Rook
