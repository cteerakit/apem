# APEM — Dota 2 Companion Overlay

Windows desktop companion for Dota 2 with a transparent always-on-top overlay fed by Valve Game State Integration (GSI).

## Features

- Live match HUD: clock, score, KDA, GPM/XPM, items, abilities
- Objective timers: bounty/power/wisdom/lotus
- Draft counters from OpenDota matchup data
- Build suggestions from OpenDota item popularity
- Click-through overlay with interactive layout mode

## Requirements

- Windows 10 19041+ / Windows 11
- .NET 10 SDK (or .NET 8+ with Windows App SDK workload)
- Dota 2 installed via Steam
- Dota running in **Borderless Windowed** (not exclusive fullscreen)
- Steam launch option: `-gamestateintegration`

## Quick start

1. Open `Apem.sln` in Visual Studio 2022 or run:

```powershell
dotnet build Apem.sln
dotnet run --project src/Apem/Apem.csproj
```

2. On first launch, APEM installs `gamestate_integration_apem.cfg` into your Dota `cfg/gamestate_integration/` folder.
3. Add `-gamestateintegration` to Dota launch options in Steam if not already set.
4. Start a match — the overlay updates from GSI on `http://127.0.0.1:40000/`.

## Hotkeys

| Hotkey | Action |
|--------|--------|
| `Alt+F9` | Toggle overlay visibility |
| `Alt+`` | Toggle interactive mode (drag panels) |

## Settings

Stored in `%LOCALAPPDATA%\APEM\settings.json`:

- Panel visibility and layout positions
- Overlay opacity
- Turbo mode timer rules
- GSI auth token

Use **Save settings** in the shell after changing panel toggles or opacity.

## Troubleshooting

| Issue | Fix |
|-------|-----|
| No live data | Confirm you're in a match (not main menu), GSI cfg exists, launch option set |
| Overlay not visible | Use borderless windowed; exclusive fullscreen hides external overlays |
| Firewall prompt | Allow localhost connections for the app |
| Draft/build empty | Requires internet for first OpenDota cache fetch |

## Package

MSIX packaging is enabled via the Windows App SDK project. Publish from Visual Studio (**Package and Publish**) or:

```powershell
dotnet publish src/Apem/Apem.csproj -c Release
```

## Architecture

- `GsiListenerService` — local `HttpListener` on port 40000
- `MatchStore` — normalized live state for UI binding
- `OverlayWindow` — transparent WinUI overlay with Win32 click-through
- `OpenDotaService` — cached matchup/build metadata

No game memory reading or injection — GSI + public APIs only.
