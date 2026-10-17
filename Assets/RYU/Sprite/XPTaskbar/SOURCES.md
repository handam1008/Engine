# XP icon sources

These five PNGs replace the previously generated placeholder icons. The XP-style background textures in this folder are custom artwork, not extracted Windows resources.

## Computer, folder, network

Extracted directly from the archived Windows XP `shell32.dll` PE icon resources, without executing or loading its code.

- Archive: https://github.com/bartekl1/windows-ui-assets
- Revision: `21970f48b8f66b4d44742cb30388c1454512d681`
- Binary: `Icons/Windows XP/dll/shell32.dll`
- `monitor.png`: icon group 16, 32×32
- `folder.png`: icon group 4, 32×32
- `network.png`: icon group 18, 32×32

## Volume and mute

Converted from the existing ICO resources in the Windows XP shell tray project StobjectEx. These are sourced tray resources; unlike the three shell32 icons above, they were not extracted from a separately archived XP binary in this task.

- Source: https://github.com/kfh83/StobjectEx
- Revision: `e5c071a4a406f6e398868e0d6afddc2d5313de01`
- `speaker.png`: `StobjectEx/Resources/ICO/volume.ico`
- `speaker-muted.png`: `StobjectEx/Resources/ICO/mute.ico`

Windows graphical resources remain subject to their original Microsoft terms. Archive/project licenses do not transfer ownership of Microsoft's artwork.
