# ButterBoard

## Description

ButterBoard is a logical Breadboard circuit simulation built in Unity. <br>
You can place various sizes of breadboards and connect components on to them. <br>
Among these components is a processor containing a basic assembly interpreter. <br>

If stuck, press the help button in the top right of the viewport, or the top right of a relevant window (if present) to be taken to the assocciated help window.

Built with Unity 2021.3.18f1

## Controls

### Camera

| Action   |         Binding |
|:---------|----------------:|
| Move     | WASD/Arrow Keys |
| Zoom     |    Scroll Wheel |

### Building

| Action              |                        Binding |
|:--------------------|-------------------------------:|
| Place               |                     Left Click |
| Remove              |                    Right Click |
| Pick up             |                     Left Click |
| Copy placed object  | Left Shift (Hold) + Left Click |
| Cancel Placement    |                         Escape |
| Place Grid Freeform |               Left Ctrl (Hold) |
| Place multiple      |              Left Shift (Hold) |
| Rotate left         |                              Q |
| Rotate Right        |                              E |

### Misc
| Action     |                  Binding |
|:-----------|-------------------------:|
| Undo       |              Control + Z |
| Redo       |              Control + Y |
| Redo       | Control + Left Shift + Z |
| Quick Save |         Left Control + S |

## Known Issues
Undoing the removal of a Processor element does not restore the program

## Misc Info
The default location for saves is `UserProfile/Documents/My Games/ButterBoard` <br>
Example saves can be found in `Assets/Examples/Saves` <br>
Any runtime exceptions will be displayed in the action history. if one occurs, please save and send the file for testing. <br>