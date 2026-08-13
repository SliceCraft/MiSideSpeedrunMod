# MiSide SpeedrunMod
This mod is a tool intended to be used for practicing speedruns or testing theories.

# Features
This mod has multiple features that can be useful for messing around with the game or giving you the chance to practice individual parts.  

## Practice modes
In the main menu you can go to `Settings->Mods->Speedrun Mod->Practice` to play specific parts of the game to practice them.  
These parts will automatically restart when you reach the end.

## Refresh rate settings
In the main menu you can go to `Settings->Mods->Speedrun Mod->Refresh Rate` to configure a reported refresh rate target.  
Using refresh rate above `540 Hz` will invalidate the run.  
The mod also shows periodic refresh-rate notifications in-game.

## Overlay
In the main menu you can go to `Settings->Mods->Speedrun Mod->Overlay` to configure and inspect the debug overlay.  
By default the overlay toggle hotkey is `F4` (configurable in the overlay menu).

## Fixes
- When pressing the start with a clean slate button the achievements will also be reset if this mod is enabled.
- When toggling `skipdialogue` the mod will remember the state of this toggle. Meaning when you restart the game you will keep this toggle in the state that you had before you quit the game.

### Softlocks
In the main menu you can go to `Settings->Mods->Speedrun Mod->Softlocks` to toggle Softlock Fixes.  
Softlock Fixes are **on by default**. Use **All Softlock Fixes** as a master gate, or turn individual Softlock Fixes off to bisect Softlock vs Softlock Fix without restarting.  
When a Softlock Fix applies, a notification is shown.

Covered Softlock Fixes (dialogue-skip / stuck progression):

- **Sleepy** — fast dialogue skip during her sit
- **Core** — skip during or right after the robot Mita throw
- **Creepy** — choosing the dialogue option and mashing skip
- **Baseball (Be Candid)** — skip-spam during bat take / hit animations
- **Kind Mita's note (Reboot)** — taking the note and skipping dialogue
- **Cappie** — Kind Mita interactions before/after the ring, and fast skip during the entrance
- **Ghostly puzzle** — sit-to-glue picture pieces are always interactable
- **Ghostly chapter load** — main-menu load or restart into Ghost Mita no longer spawns you out of bounds in the void

## Toggles
 <table>
  <tr>
    <th>Shortcut</th>
    <th>Name</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>Alt + O</td>
    <td>Toggle Triggers</td>
    <td>This toggle will change whether or not you can see in game triggers.</td>
  </tr>
  <tr>
    <td>Alt + L</td>
    <td>Toggle Running</td>
    <td>Toggle whether or not you are allowed to run.</td>
  </tr>
  <tr>
    <td>F1 (configurable)</td>
    <td>Target FPS toggle</td>
    <td>In-game: switch between your configured target FPS and your previous FPS. Rebind under FPS settings.</td>
  </tr>
  <tr>
    <td>F2 (configurable)</td>
    <td>Uncap FPS toggle</td>
    <td>In-game: switch between uncapped FPS with VSync disabled and your previous FPS/VSync settings. Rebind under FPS settings.</td>
  </tr>
  <tr>
    <td>F4 (configurable)</td>
    <td>Overlay toggle</td>
    <td>In-game: toggle overlay on/off. Rebind under Overlay settings.</td>
  </tr>
</table> 

# Installing
Before you can install this mod you need to have BepInEx with Il2Cpp support installed, this can be downloaded on their [Bleeding Edge download page](https://builds.bepinex.dev/projects/bepinex_be).  
You then need to extract the zip file to your game directory.  
Then you can download the most recent version on github through the releases section and put this dll file in the plugin folder.  
You can find this folder at `MiSide/BepInEx/plugins`.

# Contributing
Thanks for being interested in contributing to this mod!  
To setup your dev environment make sure to clone this repository and copy the interop files from your BepInEx folder (should be in `MiSide/BepInEx/interop`) to the `Dependencies` folder.  
Then download the most recent version from the [MenuLib](https://github.com/SliceCraft/MiSideMenuLib/releases) and also place this dll file in the Dependencies folder.  
You can find some good first issues over [here](https://github.com/SliceCraft/MiSideSpeedrunMod/issues?q=is%3Aissue%20state%3Aopen%20label%3A%22good%20first%20issue%22), feel free to ask for help although I won't teach you how to code.

## Build And Deploy
You can clean, build, and copy mod DLLs to your game/plugin folder with `dotnet` commands. Copy step runs only when you pass `DeployOutputDir`.

Default deploy flow:

```sh
dotnet clean -c Release
dotnet build -c Release -p:DeployOutputDir=/path/to/MiSide/BepInEx/plugins
```

By default this deploys:

- `SliceCraft.SpeedrunMod.dll`
- `SliceCraft.MenuLib.dll`

To deploy different files, pass `DeployAssemblies` as comma-separated list:

```sh
dotnet clean -c Release
dotnet build -c Release \
  -p:DeployOutputDir=/path/to/MiSide/BepInEx/plugins \
  -p:DeployAssemblies="SliceCraft.SpeedrunMod.dll,SliceCraft.MenuLib.dll"
```