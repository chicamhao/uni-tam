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
Assets/
├── Resources/
│   └── Dialogues/           # DialogueEntry ScriptableObjects (auto-registered at startup)
├── Scenes/
│   └── SampleScene.unity    # Main game scene
├── Scripts/
│   ├── Actions/             # First-person action system (Move, Jump, Crouch, Interact, Skip)
│   ├── Chapters/            # Chapter-specific objects (Bed, FootprintSpawner, HumanShadow, ShadowTrigger, SoundSource)
│   ├── Core/                # Core game systems
│   │   ├── Bootstrapper.cs        # DI registration before scene load
│   │   ├── CardSelectionButton.cs # UI button for card selection
│   │   ├── Dialogue.cs            # Dialogue engine (lines, timers, events)
│   │   ├── GameDriver.cs          # MonoBehaviour bridge (Awake → Init, Update → Tick)
│   │   ├── GameScene.cs           # Scene manager (camera swap, chapter advance)
│   │   ├── PlayerState.cs         # Owned card inventory
│   │   ├── ProgressionManager.cs  # Chapter state management for IPositionable objects
│   │   └── UIManager.cs           # HUD, toasts, fades, dialogue UI, card selection UI
│   ├── Input/               # Input handling
│   │   ├── CrosshairInteractor.cs # Crosshair raycast for interactable detection
│   │   └── InputHandle.cs        # Input action bindings (new Input System)
│   ├── Interfaces/          # Shared interfaces
│   │   ├── IClickable.cs
│   │   ├── IInteractable.cs
│   │   └── IPositionable.cs
│   ├── NPC/                 # NPC behaviour
│   │   └── NPC.cs           # Identity, expressions, chapter positioning, interaction
│   ├── Puzzle/              # Swap puzzle system
│   │   ├── Puzzle.cs              # Puzzle state machine (selection, swapping, camera)
│   │   ├── PuzzleObject.cs        # Clickable puzzle piece
│   │   └── PuzzleTrigger.cs       # Trigger volume to enter/exit puzzle mode
│   ├── Settings/            # ScriptableObject data definitions
│   │   ├── ActionSettings.cs
│   │   ├── CardData.cs
│   │   ├── ChapterSettings.cs
│   │   ├── DialogueEntry.cs
│   │   └── FacialExpressionSet.cs
│   └── Utility/             # Shared utilities
│       ├── Calculator.cs
│       ├── DIContainer.cs         # Generic DI container for plain C# singletons
│       └── Timer.cs               # Reusable timer
├── Settings/                # URP & game settings assets
└── TutorialInfo/
```

---

## Architecture

### Plain C# Singletons + DI Container

All core systems (`Dialogue`, `Puzzle`, `UIManager`, `PlayerState`, `ProgressionManager`, `GameplayScene`) are plain C# classes — not MonoBehaviours. They are registered at startup by `Bootstrapper`:

```
[RuntimeInitializeOnLoadMethod(BeforeSceneLoad)]
Bootstrapper.Initialize()
    └── DIContainer.Inject(new Dialogue())
    └── DIContainer.Inject(new ProgressionManager())
    └── DIContainer.Inject(new PlayerState())
    └── DIContainer.Inject(new UIManager())
    └── DIContainer.Inject(new GameplayScene())
    └── DIContainer.Inject(new Puzzle())
```

Then `GameDriver` (a MonoBehaviour placed in each scene) calls `Init(...)` on each system with scene references (cameras, UI panels, spawn points), and drives their `Tick(...)` methods from `Update()`.

### Chapter System

`ProgressionManager` holds a database of `ChapterEntry` records keyed by `"{ActorID}_{ChapterNumber}"`. Each `IPositionable` (NPCs, player) can be relocated, hidden, or given a new animation per chapter. Calling `AdvanceChapter()` on `GameplayScene` triggers a repositioning of all registered actors.

### Dialogue System

Dialogue is triggered by using a **CardData** item on an **NPC**. The `Dialogue` system looks up a `DialogueEntry` by `{CardID}_{NPCID}`. Dialogue lines auto-advance on a timer and can be skipped. NPCs display facial expressions via blend-shape morphs during lines.

### Puzzle System

The swap puzzle activates when the player enters a trigger volume. The player camera is swapped for a puzzle camera, and the player can click puzzle objects to select and swap them. Exiting returns control to the player.

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
| Classes, structs, enums, methods, properties, public fields | PascalCase | `GameDriver`, `Dialogue`, `ApplyExpression()` |
| Private fields (including serialized) | `_camelCase` prefix — no exceptions (every private field, static/readonly/serialized included) | `_lineTimer`, `_context`, `_fadeState` |
| Private readonly fields | `_camelCase` prefix | `_settings`, `_controller`, `_input` |
| Static readonly fields | `_camelCase` prefix | `_groundDistance`, `_jumpGroundingPreventionTime` |
| Local variables | camelCase | `elapsed`, `targetVelocity`, `worldSpaceMoveInput` |
| Interfaces | `I` prefix | `IInteractable`, `IPositionable`, `IClickable` |
| Boolean fields (public) | PascalCase, no prefixes | IsVisible, IsDialogueActive, IsActive |
| Boolean fields (private) | `_camelCase` | `_isActive`, `_initialized`, `_hovering` |
| Events | `On` prefix | `OnDialogueStarted`, `OnPuzzleExited`, `OnLineChanged` |
| Inspector-exposed fields | PascalCase, `public` or `[SerializeField] private` | `NPCID`, `DisplayName`, `puzzleCamera` |

### Namespaces

| Folder | Namespace |
|---|---|
| Scripts/Actions | Actions |
| Scripts/Chapters | Chapters |
| Scripts/Core | Core (Bootstrapper, CardSelectionButton); GameScene.cs → Manager.Scene |
| Scripts/Input | Input |
| Scripts/Interfaces | Interfaces |
| Scripts/NPC | NPCs (plural — NPC is the class name) |
| Scripts/Puzzle | Puzzle (PuzzleObject, PuzzleTrigger); Puzzle.cs class itself un-namespaced |
| Scripts/Settings | Settings |
| Scripts/Utility | Utility |

- Top-level domain singletons (`Dialogue`, `Puzzle`, `PlayerState`, `UIManager`, `GameplayScene`, `ProgressionManager`, `GameDriver`) are intentionally un-namespaced for cross-module access; `.editorconfig` suppresses IDE0130 for these.

### File & Class Organization

- **One class per file** (exceptions: closely related types like `DialogueEntry` + `DialogueLine` + `FacialExpression` + `MorphTargetValue`)
- File name matches the primary class name
- `using` statements grouped: `System.*` first, then Unity (`UnityEngine`, `TMPro`, etc.), then local project namespaces — alphabetically within each group
- Classes are `sealed` unless designed for inheritance
- Static utility classes are `static`

### Plain C# Singleton Pattern

Core systems are plain C# classes (not MonoBehaviours), registered via `DIContainer`:

```csharp
public sealed class Dialogue
{
    public static Dialogue Instance => DIContainer.Get<Dialogue>();
    // ...
}
```

Registration happens in `Bootstrapper.Initialize()` (runs `BeforeSceneLoad`). Scene references are passed later via `Init(...)` methods called by `GameDriver.Awake()`. Update logic is driven via `Tick(...)` methods called by `GameDriver.Update()`.

### MonoBehaviour Conventions

- Use `[RequireComponent(...)]` for mandatory dependencies
- Use `[Header("...")]` for inspector section grouping
- Use `[Tooltip("...")]` for serialized field documentation
- Use `[SerializeField] private` over `public` for serialized fields that don't need external writes
- Prefer `FindAnyObjectByType<T>()` over slow singleton lookups in MonoBehaviours
- Subscribe/unsubscribe events in `Start()` / `OnDestroy()` (or `OnEnable()` / `OnDisable()`)

> ⚠️ **Serialized-field renames break inspector bindings.** Renaming a `[SerializeField]` field (e.g. `CardSelectionButton`'s `_nameText`, `_descText`, `_iconImage`, `_button`, `_cachedCard`) causes Unity to lose the serialized reference — the field shows up as `None` in the inspector. **Re-assign all renamed serialized references in the Unity inspector after this refactor** (private-field `_camelCase` convention has no exceptions).

### Input & Actions

- The **Action pattern** encapsulates each player action in its own class (`MoveAction`, `JumpAction`, `CrouchAction`, `InteractAction`, `SkipLineAction`)
- All actions share an `ActionContext` that holds the `CharacterController`, `InputHandle`, and state flags
- `InputHandle` wraps the new Unity Input System and exposes semantic getters (`GetMoveInput()`, `GetJumpInputDown()`, etc.)
- Input is disabled via `InputHandle.DisableInput()` during dialogue, puzzle, and cutscenes

### Event-Driven Communication

Core systems communicate via C# events, not tight coupling:

```csharp
public event Action<DialogueEntry, NPC> OnDialogueStarted;
public event Action OnDialogueEnded;
public event Action<DialogueLine> OnLineChanged;
```

Subscribers wire up in `GameDriver.Start()` and unsubscribe in `GameDriver.OnDestroy()`.

### Interfaces

| Interface | Purpose | Implemented By |
|---|---|---|
| `IInteractable` | Player crosshair interaction | `NPC`, `Bed`, `SoundSource` |
| `IPositionable` | Per-chapter repositioning | `NPC`, `ActionControl` (player) |
| `IClickable` | Puzzle object click | `PuzzleObject` |

### Comments & Documentation

- `/// <summary>` XML doc comments on **all public classes, methods, and properties**
- `// ─── Section headers ────────────────────────────────────────────` for grouping within large files
- Inline `//` comments for non-obvious logic
- `#if UNITY_EDITOR` for editor-only code (debug helpers, `ContextMenu` methods)

### ScriptableObject Settings

- Settings data lives in `Assets/Scripts/Settings/` as `ScriptableObject` assets
- Each settings class has a `[CreateAssetMenu(...)]` attribute for easy creation
- Simple structs (`ChapterEntry`, `DialogueLine`, `MorphTargetValue`) use `[System.Serializable]`
- Settings fields use `[Tooltip(...)]` for Unity inspector tooltips

### Coroutines & Timing

- Use `Timer` (from `Utility`) for non-MonoBehaviour timed operations (line auto-advance in `Dialogue`)
- Use `StartCoroutine` in MonoBehaviours for sequenced animations (fades, spawning, blending)
- Prefer `Mathf.Lerp` / `Mathf.SmoothStep` for smooth transitions

### MaterialPropertyBlock

Use `MaterialPropertyBlock` (not `material` property) for per-instance visual changes to avoid breaking instancing and creating material copies:

```csharp
private MaterialPropertyBlock mpb;
private void Awake()
{
    mpb = new MaterialPropertyBlock();
    renderer.GetPropertyBlock(mpb);
}
```

### Error Handling

- `DIContainer.Get<T>()` throws `InvalidOperationException` if the type is not registered (fail fast)
- Use `DIContainer.TryGet<T>()` for optional dependencies
- Null-check with `?.` and `??` operators; guard with `if (x == null) return`
- `Debug.LogWarning` for recoverable issues (re-registration, missing references)

### Project Structure

```
Assets/Scripts/
├── Actions/          # First-person action classes (Move, Jump, Crouch, Interact, Skip)
├── Chapters/         # Chapter-specific MonoBehaviours (Bed, FootprintSpawner, HumanShadow, etc.)
├── Core/             # Plain C# singletons (Dialogue, Puzzle, UIManager, PlayerState, etc.)
├── Input/            # Input handling (InputHandle, CrosshairInteractor)
├── Interfaces/       # Shared interfaces (IInteractable, IPositionable, IClickable)
├── NPC/              # NPC behaviour and expressions
├── Puzzle/           # Swap puzzle system
├── Settings/         # ScriptableObject data definitions
└── Utility/          # Shared utilities (DIContainer, Timer, Calculator)
```

### EditorConfig

- `.editorconfig` suppresses IDE0130 (namespace-folder mismatch) for `.cs` files — some top-level classes intentionally omit namespaces

---

## Built With

- [Unity](https://unity.com/) — Game engine
- [Universal Render Pipeline](https://unity.com/srp/universal-render-pipeline) — Rendering
- [Unity Input System](https://docs.unity3d.com/Packages/com.unity.inputsystem@latest) — Input handling
- [TextMeshPro](https://docs.unity3d.com/Manual/com.unity.textmeshpro.html) — UI text rendering