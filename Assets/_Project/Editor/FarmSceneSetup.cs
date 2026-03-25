using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;

/// <summary>
/// One-click Farm scene setup tool.
/// Run via: Tools > CozyFarm > Setup Farm Scene
///
/// Creates and wires up ALL required GameObjects and components.
/// Safe to re-run — existing objects are updated, not duplicated.
/// </summary>
public class FarmSceneSetup : Editor
{
    // ── Asset paths ───────────────────────────────────────────────────────────

    private const string CropDBPath     = "Assets/_Project/ScriptableObjects/Crops/CropDatabase.asset";
    private const string BuildingDBPath = "Assets/_Project/ScriptableObjects/Buildings/BuildingDatabase.asset";
    private const string FontPath       = "Assets/_Project/Art/Fonts/Kenney Future SDF.asset";

    // ── Menu entry ────────────────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Full Setup")]
    public static void SetupFarmScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Farm Scene",
            "This will create a basic playable scene:\n" +
            "  - Ground plane (40x40 units)\n" +
            "  - Directional light\n" +
            "  - GameManager + all core systems\n" +
            "  - Camera (if none exists)\n" +
            "  - AudioManager\n\n" +
            "Existing objects are updated, not duplicated.\n\nContinue?", "Yes", "Cancel"))
            return;

        Undo.SetCurrentGroupName("Full Farm Scene Setup");
        int undoGroup = Undo.GetCurrentGroup();

        int created = 0;

        // 1. Ground plane
        SetupGround(ref created);

        // 2. Lighting
        SetupLighting(ref created);

        // 3. GameManager (all core systems)
        var gmGO = SetupGameManager(ref created);

        // 4. Player
        var playerGO = SetupPlayer(ref created);

        // 5. Camera
        SetupCamera(playerGO, ref created);

        // 6. Audio
        SetupAudioManager(ref created);

        // 7. Wire cross-references
        WireReferences(gmGO, playerGO);

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorSceneManager.SaveOpenScenes();

        EditorUtility.DisplayDialog("Setup Complete!",
            $"Farm scene ready!\n\nCreated/updated: {created} object(s)\n\n" +
            "Next: name your player root GameObject 'Player' and re-run if needed.\n\nHit Play!",
            "Let's Farm!");

        Debug.Log($"[FarmSceneSetup] Done! Created/updated {created}.");
    }

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Cleanup Only (remove deactivated)")]
    public static void CleanupOnly()
    {
        int removed = CleanupDeactivatedObjects();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Cleanup Done", $"Removed {removed} deactivated root object(s).", "OK");
    }

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Clear Farm Grid Area")]
    public static void ClearGridAreaOnly()
    {
        var gmGO = GameObject.Find("GameManager");
        if (gmGO == null) { EditorUtility.DisplayDialog("Error", "GameManager not found.", "OK"); return; }
        int cleared = ClearFarmGridArea(gmGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Grid Cleared", $"Removed {cleared} object(s) from the farm grid area.", "OK");
    }

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Fix Decorative Layers")]
    public static void FixDecorativeLayersOnly()
    {
        var gmGO = GameObject.Find("GameManager");
        if (gmGO == null) { EditorUtility.DisplayDialog("Error", "GameManager not found.", "OK"); return; }
        int fixed_ = SetDecorativesToIgnoreRaycast(gmGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Layers Fixed", $"Set {fixed_} decorative object(s) to Ignore Raycast.", "OK");
    }

    // ── Ground plane ──────────────────────────────────────────────────────────

    private static void SetupGround(ref int created)
    {
        if (GameObject.Find("FarmGround") != null) return;

        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "FarmGround";
        go.transform.position = Vector3.zero;
        go.transform.localScale = new Vector3(4f, 1f, 4f); // 40x40 units — larger than the 20x20 grid
        Undo.RegisterCreatedObjectUndo(go, "Create FarmGround");
        created++;
        Debug.Log("[FarmSceneSetup] Created FarmGround plane.");
    }

    // ── Game Manager ──────────────────────────────────────────────────────────

    private static GameObject SetupGameManager(ref int created)
    {
        var go = GameObject.Find("GameManager");
        bool isNew = go == null;

        if (isNew)
        {
            go = new GameObject("GameManager");
            Undo.RegisterCreatedObjectUndo(go, "Create GameManager");
            created++;
        }

        // Core systems
        EnsureComponent<FarmGrid>(go);
        EnsureComponent<FarmingManager>(go);
        EnsureComponent<InventoryManager>(go);
        EnsureComponent<EconomyManager>(go);
        EnsureComponent<ProgressionManager>(go);
        EnsureComponent<SaveManager>(go);
        EnsureComponent<BuildingManager>(go);
        EnsureComponent<RealTimeManager>(go);
        EnsureComponent<GameManager>(go); // always last — reads other components

        // FarmGrid values
        var grid   = go.GetComponent<FarmGrid>();
        var gridSO = new SerializedObject(grid);
        gridSO.FindProperty("gridWidth").intValue  = 20;
        gridSO.FindProperty("gridHeight").intValue = 20;
        gridSO.FindProperty("tileSize").floatValue = 1f;
        gridSO.ApplyModifiedProperties();

        go.transform.position = Vector3.zero;

        // Wire databases
        var saveMgr = go.GetComponent<SaveManager>();
        var saveSO  = new SerializedObject(saveMgr);
        var cropDB  = AssetDatabase.LoadAssetAtPath<CropDatabase>(CropDBPath);
        var bldgDB  = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(BuildingDBPath);
        if (cropDB != null)  saveSO.FindProperty("cropDatabase").objectReferenceValue     = cropDB;
        if (bldgDB != null)  saveSO.FindProperty("buildingDatabase").objectReferenceValue = bldgDB;
        saveSO.ApplyModifiedProperties();

        Debug.Log($"[FarmSceneSetup] {(isNew ? "Created" : "Updated")} GameManager.");
        return go;
    }

    // ── Player ────────────────────────────────────────────────────────────────

    private static GameObject SetupPlayer(ref int created)
    {
        var go = GameObject.Find("Player");
        bool isNew = go == null;

        if (isNew)
        {
            // No placeholder — Synty character must be in the scene named "Player"
            Debug.LogWarning("[FarmSceneSetup] No 'Player' GameObject found. " +
                "Place your Synty character in the scene, name the root 'Player', then re-run.");
            return null;
        }

        // All 5 player modules + coordinator + interaction
        EnsureComponent<PlayerMotor>(go);
        EnsureComponent<PlayerAnimationDriver>(go);
        EnsureComponent<PlayerInputReader>(go);
        EnsureComponent<PlayerAutoMoveAgent>(go);
        EnsureComponent<PlayerActionLock>(go);
        EnsureComponent<PlayerController>(go);
        EnsureComponent<PlayerInteraction>(go);

        // Auto-wire Animator → PlayerAnimationDriver
        var anim = go.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            var animDriver = go.GetComponent<PlayerAnimationDriver>();
            var animSO = new SerializedObject(animDriver);
            animSO.FindProperty("animator").objectReferenceValue = anim;
            animSO.ApplyModifiedProperties();
            Debug.Log($"[FarmSceneSetup] PlayerAnimationDriver wired to Animator on '{anim.gameObject.name}'.");
        }
        else
        {
            Debug.LogWarning("[FarmSceneSetup] No Animator found in Player children — assign it manually on PlayerAnimationDriver.");
        }

        // Auto-size CharacterController from mesh bounds
        AutoSizeCharacterController(go);

        // Wire ground layer to FarmInteract
        var pi    = go.GetComponent<PlayerInteraction>();
        var piSO  = new SerializedObject(pi);
        int layer = LayerMask.NameToLayer("FarmInteract");
        if (layer >= 0)
        {
            piSO.FindProperty("groundLayer").intValue = 1 << layer;
            piSO.ApplyModifiedProperties();
            Debug.Log("[FarmSceneSetup] PlayerInteraction groundLayer set to FarmInteract.");
        }
        else
        {
            Debug.LogWarning("[FarmSceneSetup] FarmInteract layer not found! Create it in Project Settings > Tags and Layers.");
        }

        // Wire PlayerController reference on PlayerInteraction
        var pc = go.GetComponent<PlayerController>();
        piSO.FindProperty("playerController").objectReferenceValue = pc;
        piSO.ApplyModifiedProperties();

        Debug.Log($"[FarmSceneSetup] {(isNew ? "Created" : "Updated")} Player.");
        return go;
    }

    // ── Character Controller auto-sizing ──────────────────────────────────────

    private static void AutoSizeCharacterController(GameObject player)
    {
        var cc = EnsureComponent<CharacterController>(player);

        // SkinnedMeshRenderer.bounds is unreliable in Edit mode (often returns zero).
        // Use fixed Synty-friendly values: 1.8m tall humanoid, feet at local Y=0.
        // If your character is a different scale, adjust Height and Center Y in the Inspector.
        const float height  = 1.8f;
        const float radius  = 0.3f;
        const float centerY = 0.9f; // must be height/2 so capsule bottom sits at Y=0 (feet)

        cc.height     = height;
        cc.radius     = radius;
        cc.center     = new Vector3(0f, centerY, 0f);
        cc.skinWidth  = 0.02f;
        cc.slopeLimit = 45f;
        cc.stepOffset = 0.3f;

        EditorUtility.SetDirty(cc);
        Debug.Log("[FarmSceneSetup] CharacterController set: height=1.8, radius=0.3, center Y=0.9 (feet at Y=0). Adjust in Inspector if character is a different scale.");
    }

    // ── Camera ────────────────────────────────────────────────────────────────

    private static void SetupCamera(GameObject player, ref int created)
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            cam = camGO.AddComponent<Camera>();
            created++;
        }

        var farmCam = EnsureComponent<FarmCamera>(cam.gameObject);

        if (player != null)
        {
            var so = new SerializedObject(farmCam);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.ApplyModifiedProperties();
        }

        cam.transform.position = new Vector3(0f, 15f, -12f);
        cam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);
        Debug.Log("[FarmSceneSetup] Camera configured.");
    }

    // ── Audio ─────────────────────────────────────────────────────────────────

    private static void SetupAudioManager(ref int created)
    {
        if (Object.FindFirstObjectByType<AudioManager>() != null) return;
        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        go.AddComponent<AmbienceManager>();
        created++;
        Debug.Log("[FarmSceneSetup] Created AudioManager.");
    }

    // ── Lighting ──────────────────────────────────────────────────────────────

    private static void SetupLighting(ref int created)
    {
        // If a directional light already exists, leave it alone
        var existingLight = Object.FindFirstObjectByType<Light>();
        if (existingLight != null && existingLight.type == LightType.Directional) return;

        var go    = new GameObject("Directional Light");
        var light = go.AddComponent<Light>();
        light.type      = LightType.Directional;
        light.intensity = 1.2f;
        light.color     = new Color(1f, 0.96f, 0.84f); // warm sunlight
        go.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        Undo.RegisterCreatedObjectUndo(go, "Create Directional Light");
        created++;
        Debug.Log("[FarmSceneSetup] Created Directional Light.");
    }

    // ── Cross-reference wiring ────────────────────────────────────────────────

    private static void WireReferences(GameObject gmGO, GameObject playerGO)
    {
        if (gmGO == null || playerGO == null) return;

        // Wire BuildingManager ground layer
        var bm   = gmGO.GetComponent<BuildingManager>();
        var bmSO = new SerializedObject(bm);
        int layer = LayerMask.NameToLayer("FarmInteract");
        if (layer >= 0)
        {
            bmSO.FindProperty("groundLayer").intValue = 1 << layer;
            bmSO.ApplyModifiedProperties();
        }
    }

    // ── Fix layers helper ─────────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Game Systems Only (no environment)")]
    public static void SetupSystemsOnly()
    {
        int created  = 0;
        var gmGO     = SetupGameManager(ref created);
        var playerGO = SetupPlayer(ref created);
        SetupCamera(playerGO, ref created);
        SetupAudioManager(ref created);
        SetupLighting(ref created);
        WireReferences(gmGO, playerGO);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Done", $"{created} object(s) created/updated.\nEnvironment left untouched.", "OK");
    }

    [MenuItem("Tools/CozyFarm/Setup Farm Scene/Fix FarmGrid Values Only")]
    public static void FixFarmGridOnly()
    {
        var gmGO = GameObject.Find("GameManager");
        if (gmGO == null) { EditorUtility.DisplayDialog("Error", "GameManager not found in scene.", "OK"); return; }

        var grid   = gmGO.GetComponent<FarmGrid>();
        if (grid == null) { EditorUtility.DisplayDialog("Error", "FarmGrid component not found on GameManager.", "OK"); return; }

        var so = new SerializedObject(grid);
        so.FindProperty("gridWidth").intValue  = 20;
        so.FindProperty("gridHeight").intValue = 20;
        so.FindProperty("tileSize").floatValue = 1f;
        so.ApplyModifiedProperties();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Done", "FarmGrid set to 20x20, tileSize 1.", "OK");
        Debug.Log("[FarmSceneSetup] FarmGrid updated: 20x20, tileSize 1.");
    }

    /// <summary>
    /// Fixes flower bed layers so clicks pass through to farm tiles.
    /// Called from CozyFarmToolkit.
    /// </summary>
    public static void FixFlowerBedLayers() => FixSceneLayers();

    [MenuItem("Tools/CozyFarm/Fix Scene Layers")]
    public static void FixSceneLayers()
    {
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast < 0) { EditorUtility.DisplayDialog("Error", "Could not find Ignore Raycast layer.", "OK"); return; }

        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var go in allObjects)
        {
            string lower = go.name.ToLower();
            if ((lower.Contains("flower") || lower.Contains("flowerbed")) && go.layer != ignoreRaycast)
            {
                Undo.RecordObject(go, "Fix Layer");
                go.layer = ignoreRaycast;
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        EditorUtility.DisplayDialog("Fix Scene Layers", $"Fixed {count} object(s).", "OK");
    }

    // ── Grid Area Clearing ──────────────────────────────────────────────────

    /// <summary>
    /// Names of GameObjects that must never be removed from the grid area.
    /// </summary>
    private static readonly System.Collections.Generic.HashSet<string> ProtectedNames =
        new(System.StringComparer.OrdinalIgnoreCase)
    {
        "GameManager", "Player", "Main Camera", "AudioManager",
        "FarmGridCollider", "FarmGround", "HUD Canvas", "PanelsCanvas",
        "EventSystem", "LightingManager"
    };

    /// <summary>
    /// Component types that mark an object as essential (never remove).
    /// </summary>
    private static readonly System.Type[] ProtectedComponents =
    {
        typeof(Camera), typeof(UnityEngine.EventSystems.EventSystem),
        typeof(Light), typeof(Canvas), typeof(FarmGrid)
    };

    /// <summary>
    /// Removes any GameObjects with colliders inside the 20x20 farm grid bounds.
    /// Grid is centered on the GameManager's position, spanning +-halfSize on X and Z.
    /// Protected objects (player, camera, systems) are never touched.
    /// </summary>
    private static int ClearFarmGridArea(GameObject gmGO)
    {
        if (gmGO == null) return 0;

        var grid = gmGO.GetComponent<FarmGrid>();
        if (grid == null) return 0;

        Vector3 center = gmGO.transform.position;
        var gridSO = new SerializedObject(grid);
        float width  = gridSO.FindProperty("gridWidth").intValue;
        float height = gridSO.FindProperty("gridHeight").intValue;
        float tileSize = gridSO.FindProperty("tileSize").floatValue;
        float halfX = width  * tileSize * 0.5f;
        float halfZ = height * tileSize * 0.5f;

        Bounds gridBounds = new Bounds(center, new Vector3(halfX * 2f, 100f, halfZ * 2f));

        var allColliders = Object.FindObjectsByType<Collider>(FindObjectsSortMode.None);
        var toRemove = new System.Collections.Generic.List<GameObject>();

        foreach (var col in allColliders)
        {
            if (col == null || col.gameObject == null) continue;

            GameObject go = col.gameObject;

            // Skip protected objects
            if (IsProtected(go)) continue;

            // Check if this collider is inside the grid bounds
            if (gridBounds.Intersects(col.bounds))
            {
                // Only remove if this isn't a child of a protected root
                Transform root = go.transform.root;
                if (!IsProtected(root.gameObject) && !toRemove.Contains(go))
                {
                    toRemove.Add(go);
                }
            }
        }

        int removed = 0;
        foreach (var go in toRemove)
        {
            if (go == null) continue;
            Debug.Log($"[FarmSceneSetup] Clearing from grid: {GetFullPath(go)}");
            Undo.DestroyObjectImmediate(go);
            removed++;
        }

        if (removed > 0)
            Debug.Log($"[FarmSceneSetup] Cleared {removed} object(s) from farm grid area.");

        return removed;
    }

    /// <summary>
    /// Checks if a GameObject is protected from removal.
    /// </summary>
    private static bool IsProtected(GameObject go)
    {
        if (go == null) return true;
        if (ProtectedNames.Contains(go.name)) return true;

        foreach (var compType in ProtectedComponents)
            if (go.GetComponent(compType) != null) return true;

        return false;
    }

    // ── Decorative Layer Fix ─────────────────────────────────────────────────

    /// <summary>
    /// Name fragments that identify decorative crop/plant objects.
    /// </summary>
    private static readonly string[] DecorativeKeywords =
    {
        "wheat-plant", "carrot", "corn-plant", "plant-salad", "cotton",
        "pumkin-leaves", "pumpkin", "flower", "flowerbed",
        "bush-berries", "hay-bale", "scarecrow"
    };

    /// <summary>
    /// Sets any decorative crop/plant GameObjects outside the grid to Ignore Raycast.
    /// Objects inside the grid should have already been removed by ClearFarmGridArea.
    /// </summary>
    private static int SetDecorativesToIgnoreRaycast(GameObject gmGO)
    {
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast < 0) return 0;

        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;

        foreach (var go in allObjects)
        {
            if (go == null || go.layer == ignoreRaycast) continue;
            if (IsProtected(go)) continue;

            string lower = go.name.ToLower();
            bool isDecorative = false;

            foreach (var keyword in DecorativeKeywords)
            {
                if (lower.Contains(keyword))
                {
                    isDecorative = true;
                    break;
                }
            }

            if (isDecorative)
            {
                Undo.RecordObject(go, "Set Decorative to Ignore Raycast");
                go.layer = ignoreRaycast;
                count++;
            }
        }

        if (count > 0)
            Debug.Log($"[FarmSceneSetup] Set {count} decorative object(s) to Ignore Raycast layer.");

        return count;
    }

    /// <summary>
    /// Returns the full hierarchy path of a GameObject for logging.
    /// </summary>
    private static string GetFullPath(GameObject go)
    {
        string path = go.name;
        Transform parent = go.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    // ── Cleanup ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Removes deactivated root GameObjects and known stale groups from the scene.
    /// </summary>
    private static int CleanupDeactivatedObjects()
    {
        var scene = EditorSceneManager.GetActiveScene();
        var roots = scene.GetRootGameObjects();
        int removed = 0;

        // Known stale groups from previous layouts
        var staleNames = new System.Collections.Generic.HashSet<string>(System.StringComparer.OrdinalIgnoreCase)
        {
            "F A R M", "A N I M A L S", "F L O W E R S", "F E N C E",
            "G R A S S", "P R O P S", "P E O P L E", "V E H I C L E S",
            "N A T U R E", "T E R R A I N", "building-house-big", "FARM LAYOUT"
        };

        foreach (var root in roots)
        {
            if (root == null) continue;

            bool shouldRemove = !root.activeSelf || staleNames.Contains(root.name);

            // Never remove essential objects
            string lower = root.name.ToLower();
            if (lower == "gamemanager" || lower == "player" || lower == "main camera" ||
                lower == "audiomanager" || lower == "hud canvas" ||
                lower == "--- environment ---" || lower.Contains("directional light") ||
                lower.Contains("global volume") || lower.Contains("event"))
                shouldRemove = false;

            if (shouldRemove)
            {
                Debug.Log($"[FarmSceneSetup] Removing: {root.name} (active={root.activeSelf})");
                Undo.DestroyObjectImmediate(root);
                removed++;
            }
        }

        return removed;
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var c = go.GetComponent<T>();
        if (c == null) c = go.AddComponent<T>();
        return c;
    }
}
