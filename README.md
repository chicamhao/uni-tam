# TAM — Narrative First-Person Adventure

A chapter-driven, first-person narrative adventure game built with Unity (URP). In **TAM**, you explore atmospheric scenes, collect cards, use them to converse with NPCs, solve puzzles, and advance through a story that reshapes the world around you.

> **Current status**: Active development — core systems are in place but content is still being built.

---

## Overview

**TAM** is a story-first game where the world changes as you progress. Use **cards** (dialogue keys) to talk to NPCs, solve a **swap puzzle** to unlock new areas, and interact with environmental objects to advance chapters. Each chapter repositions characters and objects, revealing new story beats.

### Core Loop

1. **Explore** a first-person 3D environment
2. **Collect cards** that unlock dialogue with NPCs
3. **Talk to NPCs** via the card-based dialogue system (facial expressions, timed lines, conversation cameras)
4. **Solve the puzzle** to progress
5. **Sleep** or trigger chapter transitions to advance the story
6. **Watch the world change** — NPCs, objects, and shadows shift per chapter

---

## Getting Started

### Prerequisites

- **Unity 6000.0.23f1** (or compatible 6.x version)
- URP (Universal Render Pipeline) package
- Input System package
- TextMeshPro package

### Opening the Project

1. Clone the repository
2. Open the project in Unity Hub
3. Open `Assets/Scenes/SampleScene.unity`
4. Press Play

### Creating a New Dialogue Entry

1. Right-click in the Project window → **Create → ScriptableObjects → DialogueEntry**
2. Set `CardID` and `NPCID` to match the card and NPC you want to connect
3. Add dialogue lines with display durations and facial expressions
4. Place the asset in `Assets/Resources/Dialogues/` (auto-registered at startup)

### Adding a Card

1. Right-click → **Create → Game → Card Data**
2. Set `CardID`, `DisplayName`, `Description`, and optional `TargetNPCIDs`
3. Assign an icon texture

### Defining a Chapter

1. Create or edit the `ChapterSettings` asset (Create → ScriptableObjects → ChapterSettings)
2. Add entries with `ActorID`, `Chapter` number, `SpawnPointID` (matching a Transform name in the scene), optional animation clip, and visibility

---

## Controls

| Action | Input |
|---|---|
| Move | WASD |
| Look | Mouse |
| Jump | Space |
| Crouch | Ctrl |
| Sprint | Shift |
| Interact | E |
| Skip dialogue line | F |
| Quit | Hold Escape |

---

## Development Status

- [x] Core architecture (DI, Bootstrapper, GameDriver)
- [x] First-person action system (move, jump, crouch, sprint)
- [x] Card-based dialogue system with facial expressions
- [x] Chapter progression system
- [x] Swap puzzle system
- [x] NPC interaction with crosshair targeting
- [x] Atmospheric effects (footprints, shadows, sound sources)
- [x] UI (fade, toast, dialogue panel, card selection)
- [ ] Full chapter content and narrative
- [ ] Audio implementation
- [ ] Build configuration
- [ ] Save/load system

---

## Built With

- [Unity](https://unity.com/) — Game engine
- [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline) — Rendering
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) — Input handling
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) — UI text rendering
