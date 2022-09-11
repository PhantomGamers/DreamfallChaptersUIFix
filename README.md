# DreamfallChaptersUIFix

Limits problematic UI elements to 16:9 and fixes cutscene FOV in Dreamfall Chapters

## Fixes

- Makes cutscenes hor+ FOV (horizontal FOV will be increased for widescreen resolutions)
- Limits inventory UI to 16:9
- Limits subtitles to 16:9
- Limits main menu to 16:9
- Limits chapter summary screen to 16:9

## Known issues

There are a few things untouched by the fix but these will have no impact on the ability to play the game:

- Consequence notification is stretched and might have text cut off at very wide resolutions
- Chapter title screens will be stretched and will have text cut off in some chapters even at 21:9

## Installation

- Download the [latest version](https://github.com/PhantomGamers/DreamfallChaptersUIFix/releases/latest) of this fix.
- Extract it the folder that the game is installed to (where `Dreamfall Chapters.exe` is)
- Run the game
  
## Configuration

After launching the game with the mod installed, edit `BepInEx\config\DreamfallChaptersUIFix.cfg` with your text editor of choice.

## Changelog

1.0.0:

- Initial release
