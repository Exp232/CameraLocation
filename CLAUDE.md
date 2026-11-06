# CameraLocation - notes for Claude Code

A mod for Railroader's built-in mod loader (Unity/Mono, game 0.6.x; entry point `ModEntryPoint` in Main.cs, asset-pack Info.json, settings in Settings.xml). It draws the camera's game-space position (`CameraSelector.shared.CurrentCameraPosition`, plus `CurrentCameraIdentifier`) in a screen corner with Unity's IMGUI (`CameraOverlay.OnGUI`). Namespace `CameraLocation`, author Exp232.

## Build
- `dotnet build -c Release /p:DeployToGame=false` compiles against the game's DLLs (`RailroaderDir` in the csproj, or `/p:RailroaderDir=...`).
- Do not deploy into the game's Mods folder unless asked.
- Your usual job here: build, read compile errors, fix them minimally without changing behaviour, repeat until clean.

## How it works
- No Harmony. `CameraOverlay` (CameraOverlay.cs) rebuilds the text every 0.1 s in `Update` and draws it in `OnGUI` (Repaint only). An exception switches the overlay off with one log line.
- `CurrentCameraPosition` is already in game coordinates (`WorldTransformer.WorldToGame`) and is `Vector3.zero` while no camera is selected, which the overlay treats as "nothing to show".

## Conventions
- When changing behaviour or a setting, bump the version in `Info.json`, `BUILDING.md` and the csproj together and add a `CHANGELOG.md` entry (newest first, under `# Changelog`). Update `README.md` for user-visible changes.
- Never use a version that also appears in the csproj as a package version (the `Microsoft.NETFramework.ReferenceAssemblies` reference is 1.0.3), or BuildWatcher mis-reads the csproj change.
- Settings window: `SettingsUi.cs` (Ctrl+F8). `BuildPage` draws the page without a scroll view so the Mod Settings Hub can host it; `HubLink.cs` registers the page with the hub by reflection (copy of the hub's `HubLinkTemplate.cs`; do not list the hub in Info.json).
