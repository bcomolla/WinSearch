# WinSearch

Global hotkey search overlay for Windows — search settings, apps, and files instantly.

## Run

```powershell
dotnet run --project src/WinSearch/WinSearch.csproj
```

Once running, press **Win+Space** to open the search overlay. Press **Esc** to dismiss it.

## Build

```powershell
dotnet build
```

## Publish (single exe)

```powershell
dotnet publish src/WinSearch/WinSearch.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

Output: `publish\WinSearch.exe`

## Run compiled exe directly

```
src\WinSearch\bin\Debug\net8.0-windows\WinSearch.exe
```
