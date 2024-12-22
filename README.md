# MiSide SpeedrunMod
This mod is a tool intended to be used for testing MiSide speedrun strategies.

# Features
This mod has multiple features that can be useful for messing around with the game or improving the quality of it.  

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
    <td>Toggle Place Stops (Experimental)</td>
    <td>Toggle the place stop display. Place stops are used by the game to prevent the player from going in a certain direction. This feature is still somewhat experimental since there are places where they aren't properly detected yet.</td>
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
This repository isn't really setup for contributing, but if you do want to contribute in some way feel free to do so.  
Just make sure that your project is setup with a correct references to the dll files and to not add these to your PR.