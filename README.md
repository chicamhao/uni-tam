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

## Key Features

| Feature | Description |
|---|---|
| **Card-based Dialogue** | Collect cards and use them on NPCs to trigger dialogue trees. Cards can target specific NPCs or be usable on all. |
| **Chapter Progression** | The story advances through chapters. Each chapter repositions actors (NPCs, player, objects) via `IPositionable`. |
| **Swap Puzzle** | A simple click-to-select, click-to-swap puzzle with a dedicated camera and input mode. |
| **First-Person Action** | Full movement (walk, sprint, jump, crouch) with the new Unity Input System. |
| **NPC Facial Expressions** | NPCs blend between blend-shape morph targets (Happy, Sad, Angry, Surprised, etc.) during dialogue. |
| **Atmospheric FX** | Footprint decals along spline paths, pulsing human shadows, looping sound sources that grow in volume. |
| **Fade / Toast UI** | Scene transitions with fade-to-black, toast notifications for game events. |
| **Plain C# Architecture** | Core systems are plain C# singletons wired via a DI container, with a thin `GameDriver` MonoBehaviour bridging Unity's lifecycle. |

---

## Project Structure

```
Assets/Scripts/
├── Actions/            # First-person actions (Move, Jump, Crouch, Interact, Skip)
├── Chapters/           # Chapter-driven repositioning (Bed)
├── Core/               # Plain C# singletons wired by GameDriver
│   ├── GameDriver.cs           # Composite root — creates all services (Awake) & drives ticks (Update)
│   ├── GameplayScene.cs        # Scene manager (camera swap, chapter progression, events)
│   ├── GuiGameDriver.cs        # MonoBehaviour glue for UI panel references
│   └── ...
├── FX/                 # Visual effects (Footprints, Shadows, SoundSource)
├── Input/              # Input handling (InputHandle, CrosshairInteractor)
├── Interaction/        # Card-based interactions
│   ├── Actions/        # Action classes for player input
│   ├── Input/          # Input controllers
│   ├── Interfaces/     # IClickable, IInteractable, IPositionable
│   └── Puzzle/         # Swap puzzle system (Puzzle, PuzzleObject, PuzzleTrigger)
├── Interfaces/         # Shared service interfaces (IDialogue, IGui, IPlayerState, etc.)
├── NPC/                # NPC behavior, expressions, positioning
├── Progressions/       # Core services (Dialogue, Gui, PlayerState, Progression)
├── Settings/           # ScriptableObject configs (ActionSettings, DialogueSettings, etc.)
├── UI/                 # UI system (Gui, Card)
└── Utility/            # Helpers (Calculator, Timer)
```

---

## Architecture

### Composite Root Pattern: GameDriver

**GameDriver** is a **MonoBehaviour composite root** that:
1. **Creates** all plain C# singletons (Dialogue, Gui, PlayerState, Progression, Puzzle, GameplayScene)
2. **Injects** constructor dependencies (IPlayerState, etc.)
3. **Wires** scene references via explicit `Init(...)` methods
4. **Drives** ticks via `Update()` → calls `Tick()` on each service

No static service locators, no singleton Instance properties. All services are field-private in GameDriver.

### Plain C# Services

Core systems are plain C# classes, **not MonoBehaviours**:
- **Dialogue**: Card-triggered NPC conversations; line auto-advance via Timer
- **Gui**: Fade overlay, toast notifications, dialogue panel, card selection UI
- **PlayerState**: Card inventory & owned cards
- **Progression**: Chapter database; repositions `IPositionable` actors per chapter
- **Puzzle**: Swap-puzzle state machine; camera swap on enter/exit
- **GameplayScene**: Scene manager; drives progression & manages camera/input modes

### Communication: Events

Services communicate via **C# events**, not tight coupling:
```csharp
Dialogue.OnDialogueStarted    → Gui shows panel, GameplayScene swaps camera
Dialogue.OnLineChanged        → Gui updates NPC name & line text
Puzzle.OnPuzzleEntered        → GameplayScene swaps camera & disables player input
```

### Interfaces

| Interface | Purpose | Implemented By |
|---|---|---|
| `IDialogue` | Card-based dialogue engine | `Dialogue` |
| `IGui` | UI system | `Gui` |
| `IPlayerState` | Card inventory | `PlayerState` |
| `IProgression` | Chapter progression | `Progression` |
| `IPuzzle` | Swap puzzle logic | `Puzzle` |
| `IGameplayScene` | Scene manager | `GameplayScene` |
| `IInteractable` | Player crosshair targets (NPCs, beds) | `NPC`, `Bed` |
| `IPositionable` | Per-chapter repositioning | `NPC`, `ActionControl` |
| `IClickable` | Puzzle object click | `PuzzleObject` |

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

## Code Conventions

### Naming

| Element | Convention | Example |
|---|---|---|
| Classes, structs, enums | PascalCase | `GameDriver`, `Dialogue`, `DialogueEntry` |
| Methods, properties, public fields | PascalCase | `Init()`, `ApplyExpression()`, `IsDialogueActive` |
| Private fields (all types) | `_camelCase` — **no exceptions** | `_lineTimer`, `_fadeState`, `_settings` |
| Local variables | camelCase | `elapsed`, `targetVelocity` |
| Interfaces | `I` prefix | `IDialogue`, `IInteractable`, `IPositionable` |
| Events | `On` prefix | `OnDialogueStarted`, `OnLineChanged`, `OnPuzzleExited` |

### Class Structure

- **Sealed by default** — only unseal for inheritance
- **One file per class** (exceptions: related data types like DialogueLine, MorphTargetValue)
- **Using groups**: System.* → UnityEngine → local namespaces (alphabetical within each)
- **XML doc comments** on all public types, methods, and properties
- **Inline comments** for non-obvious logic
- **Section headers** (`// ── State ────`) for organization in large files

### Dependency Injection

**Constructor injection** for dependencies:
```csharp
public sealed class Dialogue : IDialogue
{
    private readonly IPlayerState _playerState;
    
    public Dialogue(IPlayerState playerState) => _playerState = playerState;
    public void Init(DialogueSettings settings) { ... }  // Scene refs passed here
}
```

**GameDriver wires everything**:
```csharp
_dialogue = new Dialogue(_playerState);           // Constructor deps
_dialogue.Init(_dialogSettings);                  // Scene refs & config
_gameplayScene.Init(playerCamera, _dialogue);     // More scene refs
```

### MonoBehaviour Best Practices

- **Composite root only** — GameDriver is the **only** MonoBehaviour that creates services
- Other MonoBehaviours receive service refs via `ref` fields (e.g., `DialogueRef`, `GuiRef`)
- **No FindObjectOfType** for core systems — inject refs instead
- Use `[SerializeField] private` for internal state; `public` only for direct assignment by GameDriver

### Event-Driven Communication

Services communicate via **C# events** and **ref field injection**:

```csharp
// GameDriver injects service refs into MonoBehaviours
npc.DialogueRef = _dialogue;
npc.GuiRef = _gui;

// Services subscribe to events
_dialogue.OnDialogueStarted += _gameplayScene.HandleDialogueStarted;
_dialogue.OnDialogueEnded += _gameplayScene.HandleDialogueEnded;
```

### Action Pattern

Each player action is a separate class that shares an `ActionContext`:
```csharp
public sealed class MoveAction
{
    private readonly ActionContext _context;
    private readonly MoveSettings _moveSettings;
    
    public MoveAction(ActionContext context, MoveSettings moveSettings) { ... }
    public void Move() { ... }
}
```

### Timing

- **Non-MonoBehaviour**: Use `Timer` (Utility) for line auto-advance in Dialogue
- **MonoBehaviour**: Use `StartCoroutine` for sequenced effects (fades, spawning)
- Prefer `Mathf.Lerp` for smooth transitions

### ScriptableObject Settings

- Settings live in `Assets/Scripts/Settings/` with `[CreateAssetMenu(...)]` attributes
- Data types (ChapterEntry, DialogueLine, MorphTargetValue) use `[System.Serializable]`
- Include `[Tooltip(...)]` for inspector documentation

---

## Built With

- [Unity](https://unity.com/) — Game engine
- [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline) — Rendering
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) — Input handling
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) — UI text rendering