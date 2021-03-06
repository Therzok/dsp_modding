﻿# What The Break mod

A [Dyson Sphere Program](https://store.steampowered.com/app/1366540/Dyson_Sphere_Program/) mod which improves the workflow of reporting errors 🔍 in mods.

![File](https://raw.githubusercontent.com/Therzok/dsp_modding/54e198691f8b55258ba4f864dd924bd5fca3f242/src/WhatTheBreak/images/file.png)

The button will copy a nice error summary for the mod developers, giving them a bit more info about what's probably causing it (except a game update).

```
Exception hit 1 times: System.Exception: Exception of type 'System.Exception' was thrown.

at TestThrow.TestThrowPlugin.OnListRefreshed (System.Collections.Generic.List`1<UIGameSaveEntry>) <0x00031>
at TestThrow.TestThrowPlugin/Patches.RefreshList (System.Collections.Generic.List`1<UIGameSaveEntry>) <0x00037>
at (wrapper dynamic-method) UILoadGameWindow.DMD<UILoadGameWindow..RefreshList> (UILoadGameWindow) <0x0033b>
at UILoadGameWindow._OnOpen () <0x0001c>
at ManualBehaviour._Open () <0x000b5>

Target methods matching by name:
System.Void UILoadGameWindow::RefreshList()

Relevant plugins:
1. org.Therzok.dsp.TestThrow1.0.3.3 - Therzok-TestThrow\TestThrow.dll
    Prefix[index=0]: static System.Void TestThrow.Patches::RefreshList(System.Collections.Generic.List`1<UIGameSaveEntry> ___entries)

2. org.Therzok.dsp.IThrowToo0.1.2.2 - Therzok-IThrowToo\IThrowToo.dll
    Prefix[index=1]: static System.Void IThrowToo.Patches::RefreshList(System.Collections.Generic.List`1<UIGameSaveEntry> ___entries)

==================
```

## Mod developer information

The plugin scans the stacktraces for the Dynamic Method Definition entries, and tries to resolve them.

Currently there's a caveat where it is not doing any parameter parsing, but it's planned. There are a lot of exceptions (anything in `ManualBehaviour`) that won't appear in the normal logs, so this might help in those cases.

## Installing

🛠 Just install via [Thunderstore](https://dsp.thunderstore.io), it will automatically download everything needed.

To manually install, ensure [BepInEx](https://github.com/BepInEx/BepInEx) is installed first and then drop the `WhatTheBreak.dll` file into `$DSP_GAME\BepInEx\Plugins\WhatTheBreak\WhatTheBreak.dll`.

For any 🐛 or issues with this mod, please report them over at [GitHub](https://github.com/Therzok/dsp_modding/issues/new).

## Changelog

### v1.1

* Always show the button, not just when able to resolve the plugin.

### v1.0.0

* Improve error report to include the plugin directory and assembly.

### v0.1.0

* Initial release of the mod.
