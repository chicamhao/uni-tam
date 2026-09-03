# DI Refactor Plan — TAM Unity Project

## Overview

Replace the static Service Locator (`DIContainer.Get<T>()` + `XXX.Instance` properties) with explicit constructor injection via `Init()` methods. Introduce interfaces for all 6 core services. Keep the game compilable at every step via a parallel-compat strategy.

## Current Dependency Graph (as-is)

```
Bootstrapper
  └── DIContainer.Inject(...)  ← 6 plain-C# singletons, eager-registered

GameDriver (MonoBehaviour)
  ├── Progression.Instance.Init(...)
  ├── Dialogue.Instance.Init(...)
  ├── PlayerState.Instance.Init(...)
  ├── GuiGameDriver.Init() → Gui.Instance.Init(...)
  ├── GameplayScene.Instance.Init(...)
  ├── Puzzle.Instance.Init(...)
  ├── subscribes Dialogue events → GameplayScene handlers (in Start)
  └── Tick loop: GameplayScene.Instance.Tick(), Puzzle.Instance.Tick()

GuiGameDriver (MonoBehaviour)
  ├── Gui.Instance.Init(...)
  └── subscribes Dialogue events → Gui handlers (in Start)

GameplayScene
  ├── Dialogue.Instance.Update()
  └── Progression.Instance?.ApplyChapter(...)

Dialogue
  └── PlayerState.Instance.OwnedCards (in OpenCardSelectionForNPC)

PlayerState
  └── Gui.Instance.ShowToast(...) (in GrantCard)

NPC (MonoBehaviour)
  └── Dialogue.Instance.OpenCardSelectionForNPC(...)

Bed (MonoBehaviour)
  ├── GameplayScene.Instance.CurrentChapter
  └── Gui.Instance.ShowToast(...)

PuzzleTrigger (MonoBehaviour)
  └── Puzzle.Instance (subscribes events)

SkipLineAction
  └── Dialogue.Instance.RequestSkip()
```

## Service Dependency Topology (ordered by dependency depth)

| Level | Service       | Depends On                          | Used By                            |
|-------|---------------|--------------------------------------|-------------------------------------|
| 0     | Gui           | _nothing (8 scene refs via Init)_    | PlayerState, Bed, GuiGameDriver    |
| 0     | Progression   | _nothing (scene refs + ChapterSettings)_ | GameplayScene                    |
| 0     | Puzzle        | _nothing (camera ref via Init)_      | PuzzleTrigger, GameDriver          |
| 0     | Dialogue      | _nothing (DialogSettings via Init)_  | GameplayScene, NPC, SkipLineAction, GuiGameDriver, GameDriver |
| 1     | PlayerState   | → Gui                                 | Dialogue                           |
| 1     | GameplayScene | → Dialogue, → Progression             | Bed, GameDriver                    |

---

## Two-Phase Strategy

### Phase A — Parallel Interfaces + Compat (Phases 0–3, 17 steps, each compiles)

---

## Phase 0 — Interface Definitions

Create `Assets/Scripts/Interfaces/` directory with 6 files.

### IGui (file: `Assets/Scripts/Interfaces/IGui.cs`)

```csharp
using System;
using System.Collections.Generic;
using Assets.Scripts.Interaction;
using Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IGui
    {
        void Tick(float dt);
        void FadeToBlack(float duration = -1f);
        void FadeFromBlack(float duration = -1f);
        void ShowToast(string message);
        void ShowCardSelection(List<CardData> availableCards, Action<CardData> onCardSelected);
        void HideCardSelection();
        void HandleDialogueStarted(DialogueEntry entry, NPC _);
        void HandleDialogueEnded();
        void HandleLineChanged(DialogueLine line);
    }
}
```

### IDialogue (file: `Assets/Scripts/Interfaces/IDialogue.cs`)

```csharp
using System;
using Assets.Scripts.Interaction;
using Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IDialogue
    {
        bool IsDialogueActive { get; }
        NPC CurrentNPC { get; }
        DialogueEntry CurrentEntry { get; }
        int CurrentLineIndex { get; }
        bool SkipCurrentLine { get; set; }
        event Action<DialogueEntry, NPC> OnDialogueStarted;
        event Action OnDialogueEnded;
        event Action<DialogueLine> OnLineChanged;
        event Action<NPC> OnCardSelectionRequested;
        void Init(DialogSettings settings);
        void RegisterDialogue(string cardID, string npcID, DialogueEntry entry);
        bool TryGetDialogue(string cardID, string npcID, out DialogueEntry entry);
        void StartDialogue(DialogueEntry entry, NPC npc);
        void EndDialogue();
        void Update();
        void OpenCardSelectionForNPC(NPC npc);
        void OnCardSelected(CardData selectedCard, NPC npc);
        void RequestSkip();
    }
}
```

### IPlayerState (file: `Assets/Scripts/Interfaces/IPlayerState.cs`)

```csharp
using System.Collections.Generic;
using Settings;

namespace Assets.Scripts.Interfaces
{
    public interface IPlayerState
    {
        IReadOnlyList<CardData> OwnedCards { get; }
        void Init(CardData cardReturn, List<CardData> defaultNPCCards, IGui gui);
        void GrantCard(CardData card);
        bool HasCard(CardData card);
    }
}
```

### IGameplayScene (file: `Assets/Scripts/Interfaces/IGameplayScene.cs`)

```csharp
using Assets.Scripts.Interaction;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IGameplayScene
    {
        int CurrentChapter { get; set; }
        void Init(Camera playerCamera, IDialogue dialogue, IProgression progression);
        void Tick();
        void HandleDialogueStarted(DialogueEntry entry, NPC npc);
        void HandleDialogueEnded();
        void AdvanceChapter();
    }
}
```

### IProgression (file: `Assets/Scripts/Interfaces/IProgression.cs`)

```csharp
using Assets.Scripts.Interaction.Interfaces;
using Assets.Scripts.Settings;
using UnityEngine;

namespace Assets.Scripts.Interfaces
{
    public interface IProgression
    {
        void Init(Transform[] spawnPoints, ChapterSettings chapters);
        void RegisterPositionable(IPositionable obj);
        void ApplyChapter(int chapter);
    }
}
```

### IPuzzle (file: `Assets/Scripts/Interfaces/IPuzzle.cs`)

```csharp
using System;

namespace Assets.Scripts.Interfaces
{
    public interface IPuzzle
    {
        event Action OnPuzzleStarted;
        event Action OnPuzzleExited;
        string ClickActionName { get; set; }
        string ExitActionName { get; set; }
        void Init(UnityEngine.Camera puzzleCamera);
        void EnableInputActions();
        void DisableInputActions();
        void Tick();
        void SetActive(bool active);
    }
}
```

---

## Phase 1 — Leaf Services (Level 0) — Add interface implementation

Each service implements its interface AND keeps the static `Instance` property backed by `DIContainer.Get<T>()` for backward compat. All existing callers still compile against the static property.

### Step 1a — Gui → `: IGui`
- File: `Assets/Scripts/UI/Gui.cs`
- Add `: IGui` to class declaration
- Add `using Assets.Scripts.Interfaces;`
- Keep `public static Gui Instance => DIContainer.Get<Gui>();`
- No Init() signature change (Gui depends on nothing)

### Step 1b — Dialogue → `: IDialogue`
- File: `Assets/Scripts/Progressions/Dialogue.cs`
- Add `: IDialogue`, same pattern
- Flag the `PlayerState.Instance.OwnedCards` call inside `OpenCardSelectionForNPC` — will be handled later (remains as static call during Phase 1)

### Step 1c — Progression → `: IProgression`
- File: `Assets/Scripts/Progressions/Progression.cs`
- Add `: IProgression`, same pattern

### Step 1d — Puzzle → `: IPuzzle`
- File: `Assets/Scripts/Interaction/Puzzle/Puzzle.cs` (must be created first — currently missing)
- Add `: IPuzzle`, same pattern

**Compiles after Step 1:** All existing static Instance callers unchanged. New interfaces exist but unused by consumers. ✅

---

## Phase 2 — Dependent Services (Level 1) — Add interface params to Init()

### Step 2a — PlayerState → `: IPlayerState`, receive IGui in Init()

**File:** `Assets/Scripts/Progressions/PlayerState.cs`

**Init() signature change:**
```
FROM: public void Init(CardData cardReturn, List<CardData> defaultNPCCards)
TO:   public void Init(CardData cardReturn, List<CardData> defaultNPCCards, IGui gui)
```

**Internal changes:**
- Add `private IGui _gui;` field
- Replace `Gui.Instance.ShowToast(...)` → `_gui.ShowToast(...)`

**Compat:** Keep `static Instance => DIContainer.Get<PlayerState>()`

### Step 2b — GameplayScene → `: IGameplayScene`, receive IDialogue + IProgression in Init()

**File:** `Assets/Scripts/Core/GameScene.cs`

**Init() signature change:**
```
FROM: public void Init(Camera playerCamera)
TO:   public void Init(Camera playerCamera, IDialogue dialogue, IProgression progression)
```

**Internal changes:**
- Add `private IDialogue _dialogue; private IProgression _progression;` fields
- Replace `Dialogue.Instance.Update()` → `_dialogue.Update()`
- Replace `Progression.Instance?.ApplyChapter(...)` → `_progression.ApplyChapter(...)`

**Compat:** Keep static Instance for Bed.cs which calls `GameplayScene.Instance.CurrentChapter`

**Compiles after Step 2:** PlayerState and GameplayScene use interface references internally but external callers still go through static Instance. ✅

---

## Phase 3 — Introduce ServiceHub & Wire Up Consumers

### Step 3a — Create `Assets/Scripts/Core/ServiceHub.cs`

```csharp
namespace Assets.Scripts.Core
{
    /// <summary>
    /// Temporary named-field container for all service interfaces.
    /// Replaces DIContainer during migration. Phase 4 will remove it.
    /// </summary>
    public static class ServiceHub
    {
        public static IGui Gui { get; set; }
        public static IDialogue Dialogue { get; set; }
        public static IPlayerState PlayerState { get; set; }
        public static IGameplayScene GameplayScene { get; set; }
        public static IProgression Progression { get; set; }
        public static IPuzzle Puzzle { get; set; }
    }
}
```

### Step 3b — Refactor GameDriver.cs (composite root)

GameDriver becomes the single place where services are constructed and wired:

```
Awake():
  - Remove all Instance.Init() calls
  - Construct services: new Gui(), new Dialogue(), new Progression(), etc.
  - Call Init() with explicit dependencies:
      playerState.Init(cardReturn, defaultNPCCards, gui)
      gameplayScene.Init(playerCamera, dialogue, progression)
  - Populate ServiceHub:
      ServiceHub.Gui = gui; ServiceHub.Dialogue = dialogue; ...
```

**Subscriptions:** Move event subscriptions from Start() to after Init() in Awake(),
ensuring services are ready before any frame runs.

### Step 3c — Refactor GuiGameDriver.cs (scene-ref provider)

Receives `IGui` and `IDialogue` references through ServiceHub or via the gameDriver:

```
Init():
  ServiceHub.Gui.Init(fadeOverlay, toastText, ...)

Start():
  ServiceHub.Dialogue.OnDialogueStarted += ServiceHub.Gui.HandleDialogueStarted
  ...
```

### Step 3d — NPC.cs (MonoBehaviour consumer)

Replace `Dialogue.Instance.OpenCardSelectionForNPC(this)` → `ServiceHub.Dialogue.OpenCardSelectionForNPC(this)`

### Step 3e — PuzzleTrigger.cs (MonoBehaviour consumer)

Replace `Puzzle.Instance` → `ServiceHub.Puzzle`

### Step 3f — Bed.cs (MonoBehaviour consumer)

Replace `GameplayScene.Instance.CurrentChapter` → `ServiceHub.GameplayScene.CurrentChapter`
Replace `Gui.Instance.ShowToast(...)` → `ServiceHub.Gui.ShowToast(...)`

### Step 3g — SkipLineAction.cs (plain C# consumer)

Replace `Dialogue.Instance.RequestSkip()` → `ServiceHub.Dialogue.RequestSkip()`

**Compiles after Step 3:** No more static `Instance` references from consumers. Only the old `Instance` properties and `DIContainer` are still compiled but unreferenced. ✅

---

## Phase B — Tear Down (Phase 4, 5 steps)

### Step 4a — Delete Bootstrapper.cs
- No longer needed — GameDriver constructs services explicitly
- Remove `[RuntimeInitializeOnLoadMethod]` registration

### Step 4b — Delete DIContainer.cs
- Remove `using Assets.Scripts.Utility;` from all files
- Remove `DIContainer` references from Bootstrapper (already deleted)

### Step 4c — Remove all 6 static `Instance` properties
- From Gui, Dialogue, Progression, Puzzle, PlayerState, GameplayScene
- Each was the last consumer of DIContainer

### Step 4d — GameDriver uses `new T()` instead of DIContainer
- GameDriver already constructs services directly in Step 3b
- Verify no remaining DIContainer references

### Step 4e — Delete ServiceHub.cs
- MonoBehaviours (NPC, PuzzleTrigger, Bed) receive service references via GameDriver setting public fields or via Init()
- GameDriver passes references explicitly on Awake()

**Final state:** Every dependency is explicit. GameDriver is the composite root. Zero static globals.

---

## Key Design Decisions

1. **`Init()` not constructor injection** — services need scene refs (Camera, Transform[]) not available at construction time.
2. **ServiceHub over keeping DIContainer** — named fields (not a dict), trivial to `grep` and delete, clearly temporary.
3. **GameDriver as sole composite root** — after Phase 4, it's the only place services are created and wired.
4. **No DI framework** — 6 services don't warrant Zenject/VContainer. Manual DI is proportional. A framework can be added later.

---

## Edge Cases

- **Dialogue → PlayerState** — `Dialogue.OpenCardSelectionForNPC` calls `PlayerState.Instance.OwnedCards`. This cross-dependency is unusual (Level 0 → Level 1). The plan defers this: Dialogue keeps the `ServiceHub.PlayerState` call during migration; final cleanup injects `IPlayerState` into Dialogue's Init if still needed.
- **PuzzleTrigger + NPC are MonoBehaviours** — can't receive constructor injection. ServiceHub (Phase A) then public-field-set-by-GameDriver (Phase B) handles this.
- **SkipLineAction is plain C#** — created by ActionControl. Receives `IDialogue` via its constructor in Phase B.
- **Missing Puzzle.cs** — must be created before Phase 1d can compile. The Puzzle class definition is needed.
- **namespace inconsistency** — Plan uses `Assets.Scripts.Interfaces` for interface namespace to match project convention.