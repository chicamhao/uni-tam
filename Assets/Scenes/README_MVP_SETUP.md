# MVP Setup Guide — Tam

## What's Already Created

All data .asset files have been written. After Unity reimports, you'll see:

| Asset | Path | Purpose |
|---|---|---|
| `ChapterSettings.asset` | `Assets/Settings/` | 4 chapter entries (player + npc_guard, chapters 1-2) |
| `CardData_ReturnCard.asset` | `Assets/Settings/` | "Return to Bed" card (triggers chapter advance via Bed) |
| `CardData_Investigate.asset` | `Assets/Settings/` | "Investigate" card (usable on npc_guard) |
| `CardData_Gossip.asset` | `Assets/Settings/` | "Gossip" card (usable on npc_guard) |
| `FacialExpressionSet_Guard.asset` | `Assets/Settings/` | 8 expression slots for NPC (empty morphs — need 3D model) |
| `Dialogue_Guard_Investigate.asset` | `Assets/Resources/Dialogues/` | 3-line dialogue, card_investigate → npc_guard |
| `Dialogue_Guard_Gossip.asset` | `Assets/Resources/Dialogues/` | 3-line dialogue, card_gossip → npc_guard |

## Option A: One-Click Scene Setup (Recommended)

1. Open the project in Unity Editor
2. Wait for import to complete
3. Go to **Tam → Setup MVP Scene** in the top menu bar
4. The script creates a fully wired scene at `Assets/Scenes/MVP.unity`
5. Open `Assets/Scenes/MVP.unity`
6. Press **Play**

### What the script creates:
- **Player** — Capsule with CharacterController, InputHandle, ActionControl, CrosshairInteractor
- **PlayerCamera** — Child of Player, first-person perspective
- **Directional Light** — Basic sun
- **Ground** — 20x20 plane
- **NPC_Guard** — Capsule placeholder with SkinnedMeshRenderer, conversation camera, expression set assigned
- **Bed** — Simple cube with Bed component, BoxCollider trigger, highlight renderer
- **Spawn Points** — 4 empty transforms (Spawn_Player_Start, Spawn_Guard, Spawn_Player_Ch2, Spawn_Guard_Ch2)
- **UI Canvas** — Fullscreen with: FadeOverlay, ToastText, DialoguePanel (NPC name + line text), CardSelectionPanel (with vertical list), CardButtonPrefab
- **GameDriver** — All scene references wired, ChapterSettings + CardData assets assigned

## Option B: Manual Scene Setup

If the one-click script doesn't work, set up the scene manually:

### 1. Scene Objects Hierarchy
```
MVP Scene/
├── Directional Light
├── Ground (Plane)
├── Player
│   └── PlayerCamera (Camera, AudioListener, URP component)
├── NPC_Guard
│   ├── Visual (Capsule)
│   └── ConversationCamera (Camera, disabled by default)
├── Bed
│   └── Visual (Cube)
├── UI Canvas (Canvas, CanvasScaler, GraphicRaycaster)
│   ├── FadeOverlay (Image — fullscreen, black, alpha=0)
│   ├── ToastText (TextMeshProUGUI — bottom-center)
│   ├── DialoguePanel (Image — bottom 20% of screen)
│   │   ├── NPCNameText (TextMeshProUGUI — top-left)
│   │   └── LineText (TextMeshProUGUI — fill panel)
│   ├── CardSelectionPanel (Image — center 40%)
│   │   └── CardListContainer (RectTransform + VerticalLayoutGroup)
│   └── CardButtonPrefab (disabled prefab, see below)
├── Spawn_Player_Start (empty at 0,0,0)
├── Spawn_Guard (empty at 5,0,3)
├── Spawn_Player_Ch2 (empty at 0,0,10)
├── Spawn_Guard_Ch2 (empty at 5,0,13)
└── EventSystem
```

### 2. GameDriver Inspector Wiring

| Section | Field | Assign |
|---|---|---|
| ProgressionManager | spawnPoints | Array size 4: Spawn_Player_Start, Spawn_Guard, Spawn_Player_Ch2, Spawn_Guard_Ch2 |
| ProgressionManager | chapterSettings | `Assets/Settings/ChapterSettings.asset` |
| PlayerState | cardReturn | `Assets/Settings/CardData_ReturnCard.asset` |
| PlayerState | defaultNPCCards | Size 2: CardData_Investigate, CardData_Gossip |
| UIManager | fadeOverlay | UI Canvas / FadeOverlay (Image) |
| UIManager | toastText | UI Canvas / ToastText |
| UIManager | dialoguePanel | UI Canvas / DialoguePanel |
| UIManager | npcNameText | UI Canvas / DialoguePanel / NPCNameText |
| UIManager | lineText | UI Canvas / DialoguePanel / LineText |
| UIManager | cardSelectionPanel | UI Canvas / CardSelectionPanel |
| UIManager | cardListContainer | UI Canvas / CardSelectionPanel / CardListContainer |
| UIManager | cardButtonPrefab | CardButtonPrefab (see below) |
| GameplayScene | playerCamera | Player / PlayerCamera |
| Action System | playerActionControl | Player (ActionControl component) |
| Action System | actionSettingsAsset | `Assets/Settings/ActionSettings.asset` |

### 3. CardButtonPrefab Setup

Create a UI Button prefab with:
- **RectTransform**: width=300, height=80
- **Image**: Background (dark gray)
- **Button component**: normal=gray, highlighted=lighter blue
- **CardSelectionButton component** (from Core namespace)
- Child **CardName** (TextMeshProUGUI): font-size 18, bold, centered, top-anchored
- Child **CardDescription** (TextMeshProUGUI): font-size 14, gray, centered, bottom-anchored

Then assign the CardSelectionButton's serialized fields:
- `_nameText` → CardName
- `_descText` → CardDescription
- `_iconImage` → (optional, leave null if no icons)
- `_button` → Button component

### 4. NPC Inspector Wiring

| Field | Assign |
|---|---|
| NPCID | "npc_guard" |
| DisplayName | "Gate Guard" |
| conversationCamera | ConversationCamera child |
| expressionSet | `Assets/Settings/FacialExpressionSet_Guard.asset` |
| skinnedMeshRenderer | SkinnedMeshRenderer on NPC |

### 5. NPC Conversation Camera Setup

- Position: (0, 1.6, 2) relative to NPC
- Field of View: 30 (close-up)
- Depth: 1 (renders on top of player camera when active)
- **Ensure it's disabled by default**

### 6. Bed Inspector Wiring

| Field | Assign |
|---|---|
| highlightRenderer | Bed/Visual (MeshRenderer) |

## Running the MVP

1. Press **Play**
2. WASD to move, mouse to look
3. Walk up to the blue capsule (NPC_Guard) — crosshair turns green
4. Press **E** (Interact) — card selection panel appears
5. Click "Investigate" or "Gossip" — dialogue starts
6. Dialogue auto-advances on timer, press **Space** to skip
7. Walk up to the brown cube (Bed) — press E
8. Bed says "Not the right time" (chapter 1), then (after chapter 2 via code) triggers sleep→advance

## Chapter Progression via Code

The Bed only works in Chapter 2. To test chapter progression:
- Set `GameplayScene.Instance.CurrentChapter = 2` in a test script or via Debug Inspector
- Or add a debug trigger that calls `GameplayScene.Instance.AdvanceChapter()`

## Future Improvements

1. **3D NPC Model** — Replace the placeholder capsule with a proper character model (with blend shapes for facial expressions)
2. **Card Icons** — Assign Texture2D icons to each CardData asset
3. **NPC Animations** — Assign AnimationClips to ChapterSettings entries
4. **Puzzle Objects** — Create PuzzleObject prefabs for puzzle scenes
5. **Footprint/Shadow** — Set up FootprintSpawner waypoints and shadow materials
6. **Sound Effects** — Add AudioSources to NPC, Bed, and UI