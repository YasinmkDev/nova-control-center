# Nova Control Center — WinUI 3 + C# (.NET 8)

## Part 1 — Architecture + Shell (this delivery)
Parts 2-5 follow on `next`: Dashboard charts, Data page, Visual page, Settings + Framework Report.

## Why this animation system
WinUI 3 has **two** native animation layers, and Nova uses both where each wins:
- **Windows.UI.Composition** (GPU compositor, 60fps, runs off UI thread) — sidebar indicator slide, toast slide/fade, theme crossfade, card hover lift. No XAML layout pass, no jank.
- **XAML Storyboards / NavigationTransitionInfo** — page transitions, staggered entrance, skeleton shimmer. Declarative, keyboard/focus-safe.
- **Win2D (Microsoft.Graphics.Win2D)** — Visual page particles on a GPU canvas. Native, not a GIF/fake.
- No third-party animation lib is needed for motion. CommunityToolkit is only for DataGrid.

## Project structure
```
NovaControlCenter/
  NovaControlCenter.csproj
  Package.appxmanifest / app.manifest
  App.xaml / App.xaml.cs
  MainWindow.xaml / .cs        # frameless title bar, Mica, sidebar, toasts, modal host
  Styles/DesignTokens.xaml     # colours, spacing 4/8/12/16/24/32, type, radius
  Models/Models.cs             # NavItem, ServerRow, SampleData (60 rows)
  Services/ThemeService.cs     # crossfade theme (see limitation note inside)
  Services/ToastService.cs     # toast queue API
  Controls/  (Part 2) StatCard, LineChart, DonutChart
  Views/     (Parts 2-4) DashboardPage, DataPage, VisualPage, SettingsPage
```

## Setup — Windows only
```powershell
# 1. Visual Studio 2022 17.10+ with ".NET desktop development" + "Windows App SDK C# Templates"
# 2. Check SDK:
winget install Microsoft.WindowsAppSDK --version 1.6.250205002
dotnet --version  # needs 8.0.x

# 3. Build & run (from this folder):
dotnet restore
dotnet build -c Release
dotnet run -c Release
# Or open NovaControlCenter.csproj in VS 2022 and press F5 (unpackaged run works; MSIX packaging needs a cert)
```

## Files in Part 1
- `NovaControlCenter.csproj` — WinAppSDK 1.6, Toolkit DataGrid 8.1, Win2D 1.3.1
- `App.xaml/.cs` — theme restored before window shows
- `MainWindow.xaml/.cs` — ExtendsContentIntoTitleBar + SetTitleBar, MicaBackdrop BaseAlt, AppWindow Min/Max/Close, sidebar 240↔72px easeOutCubic lerp, sliding 3px indicator (Composition cubic-bezier), EntranceNavigationTransitionInfo, toast stack (max 3, slide-in, 4s auto-dismiss)
- `Styles/DesignTokens.xaml` — Nova palette #6C5CE7 / #00D2FF, spacing + type + radius tokens
- `Services/ThemeService.cs`, `ToastService.cs`, `Models/Models.cs`

## Honest notes (Part 1)
- Sidebar width: WinUI has no layout-property Composition animation, so width is a 280ms easeOutCubic timer lerp — smooth for one column, not a compositor animation.
- Theme: no native circular-reveal on Window root in WinUI 3 — shipped crossfade (180ms out / 220ms in) is the native idiom.
- ContentDialog spring: no public spring API — built-in EntranceThemeTransition + Acrylic smoke is the closest native.

Say `next` for Part 2 (Dashboard).

---

## Part 5 — Framework Report (WinUI 3 + C#)

### What was easy vs hard
- **Easy:** Mica/Acrylic, custom title bar + AppWindow, NavigationView/Frame transitions, ToggleSwitch/Slider/ComboBox animations (built-in), DataGrid sorting/virtualisation, Toasts via InfoBar, Composition scale/opacity.
- **Hard:** No native chart (built custom Canvas Line/Donut), no native 3D/shader (Win2D particles is the closest native), no layout-property Composition animation (sidebar width = timer lerp), no circular-reveal or spring dialog API, ContentDialog blur needs Acrylic workaround.

### Native vs third-party
- Native: Composition, Mica, Acrylic, Storyboards, AppWindow, Win2D (Microsoft, GPU canvas).
- Third-party: only CommunityToolkit DataGrid. No animation library needed.

### Estimated performance
- Startup: ~0.8–1.5s cold (unpackaged), ~300–500ms warm; memory ~120–180MB idle.
- Animation: 60fps for Composition transforms + Win2D particles (120 pts); XAML layout animations (sidebar width) are the only non-compositor path and are kept to one column.

### Ratings (1-10)
| | Score |
|---|---|
| Animation power | 8 — Composition is excellent; missing spring/reveal/layout anims |
| UI flexibility | 8 — XAML + templating is deep; charts/3D need custom work |
| Performance | 9 — native, GPU compositor, virtualised lists |
| Ecosystem | 7 — Windows App SDK + Toolkit are solid; smaller than web/Electron |
| Scalability for large industrial software | 8 — strong for Windows-only desktop; weak if you need macOS/web too |

### Limitations before choosing it
1. **Windows-only.** No macOS/Linux/web without a rewrite (Tauri/Flutter win there).
2. **No native charts / 3D.** Budget custom Canvas/Win2D work.
3. **Packaging friction:** MSIX + signing for Store; unpackaged run is fine for demos.
4. **Smaller community** than Electron/React — fewer copy-paste UI kits.
5. **WinUI 3 ≠ UWP/WinUI 2:** some UWP APIs/behaviours differ; check Windows App SDK 1.6 release notes.

### Checklist vs brief
- [x] Frameless title bar + drag + hover window controls
- [x] Collapsible sidebar, sliding indicator
- [x] Dashboard: 4 count-up cards, live line chart (2s), donut hover-expand, staggered entrance
- [x] Data: 60 rows, sortable, live search, skeleton shimmer, add/delete
- [x] Visual: Win2D particles + gradient mesh, mouse repulsion
- [x] Settings: toggles, sliders, dropdown, dark/light crossfade theme
- [x] Page transitions, micro-interactions, toasts (stack, max 3), modal with Acrylic blur, drag-drop reorder (Dashboard Priority queue), Mica/Acrylic, keyboard/focus visuals, design tokens, 60fps target
