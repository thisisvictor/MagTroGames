# MagTroGames
Source code for a Space Invader-ish game that uses magnets to control.

![alt text](https://github.com/thisisvictor/MagTroGames/blob/master/InputMethods_zoom.png "3 different ways to play")

## Intro
Hello thanks for your interest in this project. The code you see here is the source code of the app I used for my study on Tangible Around-Device Interaction using a magnetic ring. It is made available because 1) a reviewer of the corresponding paper requested it, and 2) I do want to share it so that you can build your own app using this neat idea.

Details of the study can be found in the paper I'll be publishing (link available later).

## Interface
Since the app was designed to be used in a study, which was a 3 x 2 factorial design, when you start the app you'll see 3 buttons (Touch, Tilt, Magnetism), and an extra one (Linear/Angular). They are for different forms of inputs and mapping schemes.

There is also one hidden button at the top right corner. It is for me to quickly check the number of shots fired and the completion time, which will be shown when you tap it (and tapping it again switches back to the instruction).

## Gameplay
The app is a simple game based on Space Invader, where the player moves a canon horizontally to shoot down descending aliens with lasers. The player wins by defeating all the aliens, and loses by not able to do so before they reach the bottom of the screen. 

To make it work for a formal study, I've made several modifications to the game:

* Smaller number of aliens so it takes less time to finish;
* Absence of elements such as defense bunkers and the "mysterious ship" to remove any randomization of game objects for consistent measurements across sessions;
* Inputs are for moving the canon only, which automatically shoots the lasers instead of requiring a separate input;
* Vertical movement of the aliens switches between upward and downward within 75% the screen, instead of always downward, so they are always in range and the player will always be able to defeat all of them without a time limit.

So don't feel strange if it doesn't play the same way you expected :)

## Details
The app is developed using Unity 2017 with Android build settings. It's fairly straight-forward and I've left some comments in the code to explain what each section does.

The most interesting part is where there app detects and transforms changes in magnetic fields into user input (in the GameManager.cs file):

```csharp
magValue = new Vector3(Input.compass.rawVector.x, Input.compass.rawVector.y, Input.compass.rawVector.z);
angle = Mathf.Atan2 (magValue.y, magValue.x) * Mathf.Rad2Deg; //[-180, 180]
```

Note how I only need to use the x- and y-axis values to calculate the angle. This corresponds to the way the game is played using the magnetic ring: you place the ring (diametrically magnetized) on the same plane as the mobile device, and rotate it.

![alt text](https://github.com/thisisvictor/MagTroGames/blob/master/MagnetFieldXYZ.png "placement of the ring and the device")

