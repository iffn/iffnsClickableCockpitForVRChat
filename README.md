# iffnsClickableCockpitForVRChat
Implements a system for clicking buttons even at high speed. Works for Desktop and VR.

Requires UdonSharp 1.0 or higher.

Created as an addon for SaccFlightAndVehicles (Brake currently not implemented):
https://github.com/Sacchan-VRC/SaccFlightAndVehicles

Check the ClickableSF1 prefab for the implementation. Everything is inside the PilotSeat.

### Integration without Submodules
To maintain compatability with other projects, please put everything into ```/Assets/iffnsStuff/iffnsClickableCockpitForVRChat``` 

### Git Submodule integration
Add this submodule with the following git command (Assuming the root of your Git project is the Unity project folder)
```
git submodule add https://github.com/iffn/iffnsClickableCockpitForVRChat.git Assets/iffnsStuff/iffnsClickableCockpitForVRChat
```

If you have manually added it, use this one first. (I recommend to close the Unity project first)
```
git rm Assets/iffnsStuff/iffnsClickableCockpitForVRChat -r
```

