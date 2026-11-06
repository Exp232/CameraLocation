# Building
1. Install the .NET SDK. Edit `RailroaderDir` in the .csproj (or pass `/p:RailroaderDir="D:\...\Railroader"`).
2. `dotnet build -c Release` builds and copies into `Documents\Railroader\Mods\CameraLocation` (override with `/p:ModsDir=...`).
3. `dotnet build -c Release /p:Package=true /p:ReleaseVersion=1.0.0` also writes `dist\CameraLocation-1.0.0.zip`
   (contains the `CameraLocation` folder with the DLL and Info.json; unzip into `Documents\Railroader\Mods`).
Keep `version` in Info.json in step with `ReleaseVersion`.
