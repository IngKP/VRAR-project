# VRAR-project

## Pre-Requisites

* **Unity Version**: 2021.3.45f2
* **Packages**: XR Interaction Toolkit, XR Transitions. It should be automatically downloaded after opening unity.

Clone the repo using:

```bash
git clone https://github.com/IngKP/VRAR-project.git
```

## Scene

Locate the main game scene at:
`Assets` -> `Samples` -> `XRTransitions` -> `0.0.1` -> `Example Scenes` -> **Fade**

## How to play

- To move, use `shift + WASD` to replicate handset movement.
- To select a start button, aim the beam and use `shift + Left mouse`.
- To select the objects, aim the beam and use `shift + G`


## Development Checklist

### Completed

- [x] **Context Transitions**: Implemented core logic to switch between VR contexts using Fade effects.
- [x] **Scavenger Hunt Logic**: Created `ScavengerHuntManager` to enforce a sequential search (Item B only appears after Item A is found).
- [x] **Object Interaction**: Added `MisplacedObject` script to handle "Select/Grab" events and trigger particle effects upon discovery.
- [x] **User Interface**: 
  - Created **World Space UI** compatible with VR headsets.
  - Added **Start/Rule Screen** overlay.
  - Added **Victory Screen** that spawns in front of the player upon completion.

### To Do
- [ ] Make sure it works with the head set (highest priority).
- [ ] Changing the first context to spaceship or somehting.
- [ ] Changing the items for more immersion.
- [ ] Time the experiment.
- [ ] Adding sound effect.
- [ ] Implement one more game.
- [ ] Try new transition.

### Known Bugs
- Cannot move and transition at the same time.
- Restart works but the objects are gone and cannot replay.