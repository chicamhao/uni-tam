using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Settings;
using NPCs;
using Input;
using Actions;
using System.Linq;
using Assets.Scripts.Settings;

/// <summary>
/// One-click MVP scene builder.
/// Run via Tam → Setup MVP Scene in the Unity Editor toolbar.
/// Creates a fully wired playable scene with player, NPC, UI, and chapter spawn points.
/// </summary>
public static class SetupMVPScene
{
    private const string ScenePath = "Assets/Scenes/MVP.unity";
    private static GameObject _cardButtonPrefab;

    [MenuItem("Tam/Setup MVP Scene", priority = 10)]
    public static void Setup()
    {
        // Save current scene if dirty
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.isDirty)
            EditorSceneManager.SaveScene(currentScene);

        // Create or open the MVP scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        scene.name = "MVP";
        EditorSceneManager.SetActiveScene(scene);

        // Remove default objects (new scene creates Main Camera, Directional Light)
        DestroyAllDefaultObjects();

        // ── Build the scene bottom-up ─────────────────────────────────────────

        // 1. Directional Light
        var sun = CreateDirectionalLight();

        // 2. Ground
        var ground = CreateGround();

        // 3. Player + Camera
        var playerCamera = CreatePlayerCamera();
        var player = CreatePlayer();
        playerCamera.transform.SetParent(player.transform, false);
        playerCamera.transform.localPosition = new Vector3(0, 0.7f, 0);

        // 4. Spawn Points
        var spawnStart = CreateSpawnPoint("Spawn_Player_Start", new Vector3(0, 0, 0));
        var spawnGuard = CreateSpawnPoint("Spawn_Guard", new Vector3(5, 0, 3));
        var spawnCh2 = CreateSpawnPoint("Spawn_Player_Ch2", new Vector3(0, 0, 10));
        var spawnGuardCh2 = CreateSpawnPoint("Spawn_Guard_Ch2", new Vector3(5, 0, 13));

        // 5. NPC (placeholder capsule with conversation camera)
        var npc = CreateNPC();
        npc.transform.position = new Vector3(5, 0, 3);

        // 6. Bed (interactable for chapter progression)
        var bed = CreateBed();
        bed.transform.position = new Vector3(-4, 0, 0);

        // 7. UI Canvas
        var canvas = CreateUICanvas();
        var (fadeOverlay, toastText, dialoguePanel, npcNameText, lineText,
             cardSelectionPanel, cardListContainer, cardButtonPrefab) = SetupUI(canvas);
        _cardButtonPrefab = cardButtonPrefab;

        // 8. GameDriver (wires everything)
        var gameDriver = CreateGameDriver(
            new Transform[] { spawnStart.transform, spawnGuard.transform, spawnCh2.transform, spawnGuardCh2.transform },
            playerCamera, player, fadeOverlay, toastText, dialoguePanel, npcNameText, lineText,
            cardSelectionPanel, cardListContainer, cardButtonPrefab);

        // ── Save the scene ────────────────────────────────────────────────────
        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("MVP Scene Ready",
            "Scene saved to Assets/Scenes/MVP.unity\n\n" +
            "Next steps:\n" +
            "1. Open the scene\n" +
            "2. Assign ChapterSettings asset to GameDriver.chapterSettings\n" +
            "3. Assign CardData assets to GameDriver.cardReturn / defaultNPCCards\n" +
            "4. Press Play!\n\n" +
            "See README_MVP_SETUP.md for full details.",
            "OK");
    }

    private static void DestroyAllDefaultObjects()
    {
        foreach (var go in Object.FindObjectsByType<GameObject>())
        {
            if (go.scene.isLoaded && go.transform.parent == null)
                Object.DestroyImmediate(go);
        }
    }

    private static GameObject CreateDirectionalLight()
    {
        var go = new GameObject("Directional Light", typeof(Light));
        go.GetComponent<Light>().type = LightType.Directional;
        go.GetComponent<Light>().intensity = 1.5f;
        go.GetComponent<Light>().shadowStrength = 0.8f;
        go.transform.rotation = Quaternion.Euler(50, -30, 0);
        return go;
    }

    private static GameObject CreateGround()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(10, 1, 10);
        return ground;
    }

    private static Camera CreatePlayerCamera()
    {
        var camGO = new GameObject("PlayerCamera", typeof(Camera), typeof(AudioListener));
        camGO.tag = "MainCamera";
        var cam = camGO.GetComponent<Camera>();
        cam.nearClipPlane = 0.1f;
        cam.fieldOfView = 60;
        cam.depth = 0;
        cam.clearFlags = CameraClearFlags.Skybox;
        return cam;
    }

    private static GameObject CreatePlayer()
    {
        var player = new GameObject("Player");
        player.tag = "Player";
        player.layer = LayerMask.NameToLayer("Default");

        var cc = player.AddComponent<CharacterController>();
        cc.height = 1.8f;
        cc.radius = 0.5f;
        cc.center = new Vector3(0, 0.9f, 0);

        // Capsule visual (child)
        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        visual.transform.SetParent(player.transform);
        visual.transform.localPosition = new Vector3(0, 0.9f, 0);
        visual.transform.localScale = new Vector3(0.5f, 0.9f, 0.5f);
        Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

        player.AddComponent<InputHandle>();
        player.AddComponent<CrosshairInteractor>();
        player.AddComponent<ActionControl>();

        player.transform.position = new Vector3(0, 0, 0);

        return player;
    }

    private static GameObject CreateNPC()
    {
        var npc = new GameObject("NPC_Guard", typeof(NPC));
        npc.tag = "Untagged";

        var npcComp = npc.GetComponent<NPC>();
        npcComp.NPCID = "npc_guard";
        npcComp.DisplayName = "Gate Guard";

        // Placeholder capsule visual (instead of SkinnedMeshRenderer without a model)
        var visual = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        visual.name = "Visual";
        visual.transform.SetParent(npc.transform);
        visual.transform.localPosition = new Vector3(0, 1, 0);
        visual.transform.localScale = new Vector3(0.5f, 1, 0.5f);
        Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());

        // Add a SkinnedMeshRenderer placeholder for the NPC component to reference
        var smr = npc.AddComponent<SkinnedMeshRenderer>();
        var capsuleMesh = Resources.GetBuiltinResource<Mesh>("Capsule.fbx");
        smr.sharedMesh = capsuleMesh;
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.2f, 0.4f, 0.8f); // Blue
        smr.sharedMaterial = mat;
        npcComp.skinnedMeshRenderer = smr;

        // Load and assign FacialExpressionSet
        var expressionSet = AssetDatabase.LoadAssetAtPath<FacialExpressionSet>(
            "Assets/Settings/FacialExpressionSet_Guard.asset");
        if (expressionSet != null)
            npcComp.expressionSet = expressionSet;

        // Conversation camera (child)
        var camGO = new GameObject("ConversationCamera", typeof(Camera));
        camGO.transform.SetParent(npc.transform);
        camGO.transform.localPosition = new Vector3(0, 1.6f, 2f);
        var convCam = camGO.GetComponent<Camera>();
        convCam.nearClipPlane = 0.1f;
        convCam.fieldOfView = 30;
        convCam.depth = 1;
        convCam.enabled = false;
        npcComp.conversationCamera = convCam;

        return npc;
    }

    private static GameObject CreateBed()
    {
        var bed = new GameObject("Bed", typeof(BoxCollider));
        bed.tag = "Untagged";

        var bc = bed.GetComponent<BoxCollider>();
        bc.isTrigger = true;
        bc.size = new Vector3(2, 1, 3);

        // Visual: simple box
        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(bed.transform);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localScale = new Vector3(2, 0.3f, 3);
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.6f, 0.3f, 0.1f); // Brown
        visual.GetComponent<MeshRenderer>().sharedMaterial = mat;

        // Add Bed component
        var bedComponent = bed.AddComponent<Assets.Scripts.Chapters.Bed>();
        bedComponent.highlightRenderer = visual.GetComponent<MeshRenderer>();

        // Collider for interaction
        bed.AddComponent<SphereCollider>();
        bed.GetComponent<SphereCollider>().radius = 2;
        bed.GetComponent<SphereCollider>().isTrigger = true;

        return bed;
    }

    private static GameObject CreateSpawnPoint(string name, Vector3 position)
    {
        var sp = new GameObject(name);
        sp.transform.position = position;
        return sp;
    }

    private static GameObject CreateUICanvas()
    {
        var canvasGO = new GameObject("UI Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        // Add EventSystem if not present
        if (Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));
        }

        return canvasGO;
    }

    private static (
        Image fadeOverlay,
        TextMeshProUGUI toastText,
        GameObject dialoguePanel,
        TextMeshProUGUI npcNameText,
        TextMeshProUGUI lineText,
        GameObject cardSelectionPanel,
        Transform cardListContainer,
        GameObject cardButtonPrefab
    ) SetupUI(GameObject canvasGO)
    {
        var canvasTransform = canvasGO.transform;

        // ── Fade Overlay (full-screen black image, starts invisible) ─────────
        var fadeGO = new GameObject("FadeOverlay", typeof(Image));
        fadeGO.transform.SetParent(canvasTransform, false);
        var fadeRect = fadeGO.GetComponent<RectTransform>();
        fadeRect.anchorMin = Vector2.zero;
        fadeRect.anchorMax = Vector2.one;
        fadeRect.sizeDelta = Vector2.zero;
        var fadeImage = fadeGO.GetComponent<Image>();
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.raycastTarget = false;

        // ── Toast Text (bottom-center) ───────────────────────────────────────
        var toastGO = new GameObject("ToastText", typeof(TextMeshProUGUI));
        toastGO.transform.SetParent(canvasTransform, false);
        var toastRect = toastGO.GetComponent<RectTransform>();
        toastRect.anchorMin = new Vector2(0.5f, 0);
        toastRect.anchorMax = new Vector2(0.5f, 0);
        toastRect.pivot = new Vector2(0.5f, 0);
        toastRect.anchoredPosition = new Vector2(0, 50);
        toastRect.sizeDelta = new Vector2(600, 60);
        var toastText = toastGO.GetComponent<TextMeshProUGUI>();
        toastText.text = "";
        toastText.fontSize = 28;
        toastText.alignment = TextAlignmentOptions.Center;
        toastText.color = Color.white;
        toastGO.SetActive(false);

        // ── Dialogue Panel (bottom third of screen) ──────────────────────────
        var dialoguePanelGO = new GameObject("DialoguePanel", typeof(Image));
        dialoguePanelGO.transform.SetParent(canvasTransform, false);
        var dpRect = dialoguePanelGO.GetComponent<RectTransform>();
        dpRect.anchorMin = new Vector2(0.1f, 0.1f);
        dpRect.anchorMax = new Vector2(0.9f, 0.3f);
        dpRect.sizeDelta = Vector2.zero;
        var dpImage = dialoguePanelGO.GetComponent<Image>();
        dpImage.color = new Color(0, 0, 0, 0.7f);
        dialoguePanelGO.SetActive(false);

        // NPC Name (inside dialogue panel, top-left)
        var nameGO = new GameObject("NPCNameText", typeof(TextMeshProUGUI));
        nameGO.transform.SetParent(dialoguePanelGO.transform, false);
        var nameRect = nameGO.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0, 1);
        nameRect.anchorMax = new Vector2(0, 1);
        nameRect.pivot = new Vector2(0, 1);
        nameRect.anchoredPosition = new Vector2(20, -10);
        nameRect.sizeDelta = new Vector2(400, 40);
        var nameText = nameGO.GetComponent<TextMeshProUGUI>();
        nameText.text = "";
        nameText.fontSize = 24;
        nameText.fontStyle = FontStyles.Bold;
        nameText.color = Color.white;

        // Line Text (inside dialogue panel, center-left)
        var lineGO = new GameObject("LineText", typeof(TextMeshProUGUI));
        lineGO.transform.SetParent(dialoguePanelGO.transform, false);
        var lineRect = lineGO.GetComponent<RectTransform>();
        lineRect.anchorMin = Vector2.zero;
        lineRect.anchorMax = Vector2.one;
        lineRect.pivot = new Vector2(0.5f, 0.5f);
        lineRect.offsetMin = new Vector2(20, 10);
        lineRect.offsetMax = new Vector2(-20, -50);
        var lineText = lineGO.GetComponent<TextMeshProUGUI>();
        lineText.text = "";
        lineText.fontSize = 20;
        lineText.color = Color.white;
        lineText.alignment = TextAlignmentOptions.TopLeft;

        // ── Card Selection Panel (center of screen) ──────────────────────────
        var cardPanelGO = new GameObject("CardSelectionPanel", typeof(Image));
        cardPanelGO.transform.SetParent(canvasTransform, false);
        var cpRect = cardPanelGO.GetComponent<RectTransform>();
        cpRect.anchorMin = new Vector2(0.3f, 0.3f);
        cpRect.anchorMax = new Vector2(0.7f, 0.7f);
        cpRect.sizeDelta = Vector2.zero;
        var cpImage = cardPanelGO.GetComponent<Image>();
        cpImage.color = new Color(0, 0, 0, 0.85f);
        cardPanelGO.SetActive(false);

        // Card list container (inside card panel)
        var listGO = new GameObject("CardListContainer");
        listGO.transform.SetParent(cardPanelGO.transform, false);
        var listRect = listGO.AddComponent<RectTransform>();
        listRect.anchorMin = Vector2.zero;
        listRect.anchorMax = Vector2.one;
        listRect.sizeDelta = new Vector2(-40, -40);
        listRect.anchoredPosition = Vector2.zero;
        var listLayout = listGO.AddComponent<VerticalLayoutGroup>();
        listLayout.spacing = 10;
        listLayout.padding = new RectOffset(10, 10, 10, 10);
        listLayout.childAlignment = TextAnchor.MiddleCenter;
        listLayout.childControlWidth = true;
        listLayout.childControlHeight = true;

        // Card button prefab (single button, will be instantiated at runtime)
        var btnPrefab = new GameObject("CardButtonPrefab", typeof(Image));
        btnPrefab.SetActive(false);
        var btnRect = btnPrefab.GetComponent<RectTransform>();
        btnRect.sizeDelta = new Vector2(300, 80);
        var btnImage = btnPrefab.GetComponent<Image>();
        btnImage.color = new Color(0.2f, 0.2f, 0.3f, 1);

        // Button component
        var btn = btnPrefab.AddComponent<Button>();
        btn.targetGraphic = btnImage;
        var colors = btn.colors;
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.5f);
        btn.colors = colors;

        // CardSelectionButton component
        var csBtn = btnPrefab.AddComponent<Core.CardSelectionButton>();

        // Card name text (inside button)
        var btnNameGO = new GameObject("CardName", typeof(TextMeshProUGUI));
        btnNameGO.transform.SetParent(btnPrefab.transform, false);
        var btnNameRect = btnNameGO.GetComponent<RectTransform>();
        btnNameRect.anchorMin = new Vector2(0, 1);
        btnNameRect.anchorMax = new Vector2(1, 1);
        btnNameRect.pivot = new Vector2(0.5f, 1);
        btnNameRect.anchoredPosition = new Vector2(0, -15);
        btnNameRect.sizeDelta = new Vector2(-20, 30);
        var btnNameText = btnNameGO.GetComponent<TextMeshProUGUI>();
        btnNameText.fontSize = 18;
        btnNameText.fontStyle = FontStyles.Bold;
        btnNameText.alignment = TextAlignmentOptions.Center;

        // Card description text (inside button)
        var btnDescGO = new GameObject("CardDescription", typeof(TextMeshProUGUI));
        btnDescGO.transform.SetParent(btnPrefab.transform, false);
        var btnDescRect = btnDescGO.GetComponent<RectTransform>();
        btnDescRect.anchorMin = new Vector2(0, 0);
        btnDescRect.anchorMax = new Vector2(1, 0);
        btnDescRect.pivot = new Vector2(0.5f, 0);
        btnDescRect.anchoredPosition = new Vector2(0, 5);
        btnDescRect.sizeDelta = new Vector2(-20, 25);
        var btnDescText = btnDescGO.GetComponent<TextMeshProUGUI>();
        btnDescText.fontSize = 14;
        btnDescText.color = new Color(0.8f, 0.8f, 0.8f);
        btnDescText.alignment = TextAlignmentOptions.Center;

        // Wire up CardSelectionButton serialized fields
        // (We'll set these via reflection or the user sets them in the Inspector)
        // CardSelectionButton has [SerializeField] private fields, so we can't set them directly.
        // The user will need to assign them in the prefab Inspector.
        // For now, the fallback is that the prefab works with default TMP text.

        return (
            fadeImage,
            toastText,
            dialoguePanelGO,
            nameText,
            lineText,
            cardPanelGO,
            listRect,
            btnPrefab
        );
    }

    private static GameObject CreateGameDriver(
        Transform[] spawnPoints,
        Camera playerCamera,
        GameObject player,
        Image fadeOverlay,
        TextMeshProUGUI toastText,
        GameObject dialoguePanel,
        TextMeshProUGUI npcNameText,
        TextMeshProUGUI lineText,
        GameObject cardSelectionPanel,
        Transform cardListContainer,
        GameObject cardButtonPrefab)
    {
        var go = new GameObject("GameDriver", typeof(GameDriver));
        var gd = go.GetComponent<GameDriver>();

        // Serialized fields on GameDriver
        gd.spawnPoints = spawnPoints;
        gd.playerCamera = playerCamera;

        // ChapterSettings asset
        var chapterSettings = AssetDatabase.LoadAssetAtPath<ChapterSettings>(
            "Assets/Settings/ChapterSettings.asset");
        gd.chapterSettings = chapterSettings;

        // CardData assets
        gd.cardReturn = AssetDatabase.LoadAssetAtPath<CardData>(
            "Assets/Settings/CardData_ReturnCard.asset");
        gd.defaultNPCCards = new System.Collections.Generic.List<CardData>
        {
            AssetDatabase.LoadAssetAtPath<CardData>("Assets/Settings/CardData_Investigate.asset"),
            AssetDatabase.LoadAssetAtPath<CardData>("Assets/Settings/CardData_Gossip.asset"),
        };

        // UI references
        gd.fadeOverlay = fadeOverlay;
        gd.toastText = toastText;
        gd.dialoguePanel = dialoguePanel;
        gd.npcNameText = npcNameText;
        gd.lineText = lineText;
        gd.cardSelectionPanel = cardSelectionPanel;
        gd.cardListContainer = cardListContainer;
        gd.cardButtonPrefab = cardButtonPrefab;

        // ActionControl
        var actionControl = player.GetComponent<ActionControl>();
        gd.playerActionControl = actionControl;

        // ActionSettings asset
        gd.actionSettingsAsset = AssetDatabase.LoadAssetAtPath<ActionSettings>(
            "Assets/Settings/ActionSettings.asset");

        return go;
    }
}