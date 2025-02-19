# MiSide SpeedrunMod
This mod is a tool intended to be used for practicing speedruns or testing theories.

# Features
This mod has multiple features that can be useful for messing around with the game or giving you the chance to practice individual parts.  

## Fixes
When pressing the start with a clean slate button the achievements will also be reset if this mod is enabled.

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
    <td>Alt + P</td>
    <td>Toggle Place Stops (Deprecated)</td>
    <td>Toggle the place stop display. Place stops are used by the game to prevent the player from going in a certain direction. This feature has been deprecated and will be removed soon.</td>
  </tr>
  <tr>
    <td>Alt + L</td>
    <td>Toggle Running</td>
    <td>Toggle whether or not you are allowed to run.</td>
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