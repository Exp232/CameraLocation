# Camera Location

Shows where the camera is, in a corner of the screen: the map position (X and Z) and the height, plus which camera is active. Handy for placing things in map mods, reporting a spot, or finding your way around a large map.

```
Strategy camera
X 1234   Z -567   Height 89
```

## Settings
Open the **Camera Location** page in the [Mod Settings Hub](https://www.nexusmods.com/railroader/mods/1977) (top-right menu button or Ctrl+F7). Without the hub, Ctrl+F8 opens the mod's own window. Everything is saved to `Settings.xml` in the mod folder as you change it.

- **Show camera location**: on/off.
- **Corner**: top left, top right, bottom left or bottom right.
- **Show height / Show which camera**: leave out the parts you don't need.
- **Decimals**: 0 to 2.
- **Text size, distance from the edges, background**: so it sits where it doesn't cover the game's own interface.

The coordinates are the game's own (the ones the camera and jump commands use). The mod only draws text on the screen; it never changes the game, and the text never takes a click.

## Install
Unzip into `Documents\Railroader\Mods` so you have `Mods\CameraLocation\CameraLocation.dll`. Needs Railroader 0.6.0 or newer.

## About the code
I'm a pilot by profession, not a coder by day. I use AI as an accessibility tool that lets me write Railroader mods and learn along the way; this mod's code was written with Claude.
