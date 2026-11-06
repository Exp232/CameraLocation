# Changelog

## 1.0.0

First release.

- Shows the camera's map position (X and Z, plus Height) in a corner of the screen, with the name of the active camera (first-person, strategy, dispatcher, fly).
- Corner (top left, top right, bottom left, bottom right), text size, distance from the edges, background opacity, decimals, and whether height and camera name are shown are all settings.
- Settings page in the Mod Settings Hub when it is installed (found by reflection, so the hub is not required); otherwise Ctrl+F8 opens the mod's own settings window. Values are kept in `Settings.xml`.
