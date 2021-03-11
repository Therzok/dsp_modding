# dsp_modding

Collection of mods and research done for DSP.

# Pre-requisites

Visual Studio or VS Code, `dotnet` and `dotnet tool install -g nbgv` (NerdBank.GitVersioning for release builds).

## Mod list:

* [OrderSaves](src/OrderSaves/README.md) ([Thunderstore](https://dsp.thunderstore.io/package/Therzok/OrderSaves/)) - sorts the savegames in the save/load windows
* [WhatTheBreak](src/WhatTheBreak/README.md) ([Thunderstore](https://dsp.thunderstore.io/package/Therzok/WhatTheBreak/)) - orovides a copy to clipboard button for exceptions and does analysis on the stacktrace for better mod error reports

### WIP List
* [PrefabBlocks](src/PrefabBlocks/README.md) - Unity prefab basic objects
* [SaveTheWindows](src/SaveTheWindows/README.md) - Storage of window positions across sessions
* [YouNameIt](src/YouNameIt/README.md) - Showing the name of the interplanetary logistics stations on top of them in planet view
* [Sortee](src/YouNameIt/README.md) - Automatically sort inventory and containers on open/close.

## Extras:

Fully automated resolution of Dyson Sphere Program install on Steam via MSBuild targets.

Automated package zip generation on release builds.

Plugin dependency resolution via `<PluginReference>`.