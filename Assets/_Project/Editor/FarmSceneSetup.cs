using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;

/// <summary>
/// One-click Farm scene setup tool.
/// Run via: Tools > CozyFarm > Setup Farm Scene
/// Creates and wires up ALL required GameObjects and components.
/// </summary>
public class FarmSceneSetup : Editor
{
    [MenuItem("Tools/CozyFarm/Setup Farm Scene")]
    public static void SetupFarmScene()
    {
        if (!EditorUtility.DisplayDialog("Setup Farm Scene",
            "This will add all required game system GameObjects to the current scene.\n\n" +
            "Existing objects with the same names will be skipped.\n\nContinue?", "Yes", "Cancel"))
            return;

        // Load font
        var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/_Project/Art/Fonts/Kenney Future SDF.asset");
        if (font == null)
            font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");

        // Load databases
        var cropDB = AssetDatabase.LoadAssetAtPath<CropDatabase>(
            "Assets/_Project/ScriptableObjects/Crops/CropDatabase.asset");
        var buildingDB = AssetDatabase.LoadAssetAtPath<BuildingDatabase>(
            "Assets/_Project/ScriptableObjects/Buildings/BuildingDatabase.asset");

        int created = 0;

        // 1. GAME MANAGER
        var gmGO = SetupGameManager(ref created);

        // 2. PLAYER
        var playerGO = SetupPlayer(ref created);

        // 3. CAMERA
        SetupCamera(playerGO, ref created);

        // 4. HUD
        SetupHUD(font, cropDB, buildingDB, ref created);

        // 5. AUDIO MANAGER
        SetupAudioManager(ref created);

        // 6. PAUSE MENU
        SetupPauseMenu(ref created);

        // 7. LIGHTING
        SetupLighting(ref created);

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Setup Complete!",
            $"Farm scene setup complete!\n\n" +
            $"Created {created} new GameObjects.\n\n" +
            $"Next steps:\n" +
            $"1. Position the Player on the ground\n" +
            $"2. Assign Ground Layer to PlayerInteraction\n" +
            $"3. Assign CropVisual prefab to FarmingManager\n" +
            $"4. Hit Play!", "Let's Farm!");

        Debug.Log($"[FarmSceneSetup] Done! {created} objects created.");
    }

    // ─── Game Manager ──────────────────────────────────────────────────────────

    private static GameObject SetupGameManager(ref int created)
    {
        var existing = GameObject.Find("GameManager");
        if (existing != null)
        {
            Debug.Log("[FarmSceneSetup] GameManager already exists — skipping.");
            EnsureComponent<FarmGrid>(existing);
            EnsureComponent<FarmingManager>(existing);
            EnsureComponent<InventoryManager>(existing);
            EnsureComponent<EconomyManager>(existing);
            EnsureComponent<ProgressionManager>(existing);
            EnsureComponent<SaveManager>(existing);
            EnsureComponent<BuildingManager>(existing);
            EnsureComponent<GameManager>(existing);
            return existing;
        }

        var go = new GameObject("GameManager");
        go.AddComponent<FarmGrid>();
        go.AddComponent<FarmingManager>();
        go.AddComponent<InventoryManager>();
        go.AddComponent<EconomyManager>();
        go.AddComponent<ProgressionManager>();
        go.AddComponent<SaveManager>();
        go.AddComponent<BuildingManager>();
        go.AddComponent<GameManager>();
        created++;
        Debug.Log("[FarmSceneSetup] Created GameManager.");
        return go;
    }

    // ─── Player ────────────────────────────────────────────────────────────────

    private static GameObject SetupPlayer(ref int created)
    {
        var existing = GameObject.Find("Player");
        if (existing != null)
        {
            Debug.Log("[FarmSceneSetup] Player already exists — skipping.");
            EnsureComponent<CharacterController>(existing);
            EnsureComponent<PlayerController>(existing);
            EnsureComponent<PlayerInteraction>(existing);
            EnsureComponent<BuildModeController>(existing);
            return existing;
        }

        var go = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        go.name = "Player";
        go.transform.position = new Vector3(10f, 1f, 10f);

        // Blue URP material
        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.SetColor("_BaseColor", new Color(0.3f, 0.55f, 0.9f));
        go.GetComponent<Renderer>().material = mat;

        // Replace capsule collider with character controller
        Object.DestroyImmediate(go.GetComponent<CapsuleCollider>());
        var cc = go.AddComponent<CharacterController>();
        cc.height = 2f;
        cc.radius = 0.4f;

        go.AddComponent<PlayerController>();
        go.AddComponent<PlayerInteraction>();
        go.AddComponent<BuildModeController>();

        // Hat
        var hat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hat.name = "Hat";
        hat.transform.SetParent(go.transform);
        hat.transform.localPosition = new Vector3(0, 0.75f, 0);
        hat.transform.localScale = new Vector3(0.6f, 0.25f, 0.6f);
        Object.DestroyImmediate(hat.GetComponent<Collider>());
        var hatMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        hatMat.SetColor("_BaseColor", new Color(0.4f, 0.25f, 0.1f));
        hat.GetComponent<Renderer>().material = hatMat;

        created++;
        Debug.Log("[FarmSceneSetup] Created Player.");
        return go;
    }

    // ─── Camera ────────────────────────────────────────────────────────────────

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

        // Wire target via SerializedObject
        if (player != null)
        {
            var so = new SerializedObject(farmCam);
            so.FindProperty("target").objectReferenceValue = player.transform;
            so.ApplyModifiedProperties();
        }

        cam.transform.position = new Vector3(10f, 12f, -2f);
        cam.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
        Debug.Log("[FarmSceneSetup] Camera configured.");
    }

    // ─── HUD ───────────────────────────────────────────────────────────────────

    private static void SetupHUD(TMP_FontAsset font, CropDatabase cropDB,
        BuildingDatabase buildingDB, ref int created)
    {
        // Remove old HUD Canvas if exists
        var existing = GameObject.Find("HUD Canvas");

        // Check if HUDBootstrapper exists
        var bootstrapper = Object.FindFirstObjectByType<HUDBootstrapper>();
        if (bootstrapper == null && existing == null)
        {
            // Create HUD bootstrapper object
            var hudGO = new GameObject("HUD");
            var hb = hudGO.AddComponent<HUDBootstrapper>();

            // Wire databases and font via SerializedObject
            var so = new SerializedObject(hb);
            if (font != null) so.FindProperty("fontAsset").objectReferenceValue = font;
            if (cropDB != null) so.FindProperty("cropDatabase").objectReferenceValue = cropDB;
            if (buildingDB != null) so.FindProperty("buildingDatabase").objectReferenceValue = buildingDB;
            so.ApplyModifiedProperties();

            created++;
            Debug.Log("[FarmSceneSetup] Created HUD bootstrapper.");
        }
        else
        {
            Debug.Log("[FarmSceneSetup] HUD already exists — skipping.");
        }
    }

    // ─── Audio Manager ─────────────────────────────────────────────────────────

    private static void SetupAudioManager(ref int created)
    {
        if (Object.FindFirstObjectByType<AudioManager>() != null)
        {
            Debug.Log("[FarmSceneSetup] AudioManager already exists — skipping.");
            return;
        }

        var go = new GameObject("AudioManager");
        go.AddComponent<AudioManager>();
        go.AddComponent<AmbienceManager>();
        created++;
        Debug.Log("[FarmSceneSetup] Created AudioManager.");
    }

    // ─── Pause Menu ────────────────────────────────────────────────────────────

    private static void SetupPauseMenu(ref int created)
    {
        if (Object.FindFirstObjectByType<PauseMenuUI>() != null)
        {
            Debug.Log("[FarmSceneSetup] PauseMenuUI already exists — skipping.");
            return;
        }

        var go = new GameObject("PauseMenu");
        go.AddComponent<PauseMenuUI>();
        created++;
        Debug.Log("[FarmSceneSetup] Created PauseMenu.");
    }

    // ─── Lighting ──────────────────────────────────────────────────────────────

    private static void SetupLighting(ref int created)
    {
        if (Object.FindFirstObjectByType<CozyLightingSetup>() != null)
        {
            Debug.Log("[FarmSceneSetup] Lighting already configured — skipping.");
            return;
        }

        var go = new GameObject("LightingManager");
        var lighting = go.AddComponent<CozyLightingSetup>();

        // Find directional light
        var sun = Object.FindFirstObjectByType<Light>();
        if (sun != null)
        {
            var so = new SerializedObject(lighting);
            so.FindProperty("sunLight").objectReferenceValue = sun;
            so.ApplyModifiedProperties();
        }

        created++;
        Debug.Log("[FarmSceneSetup] Created LightingManager.");
    }

    // ─── Fix Flower Bed Layers ─────────────────────────────────────────────────

    [MenuItem("Tools/CozyFarm/Fix Flower Bed Layers")]
    public static void FixFlowerBedLayers()
    {
        // Layer 2 = "Ignore Raycast" — built-in Unity layer
        int ignoreRaycast = LayerMask.NameToLayer("Ignore Raycast");
        if (ignoreRaycast < 0)
        {
            EditorUtility.DisplayDialog("Error", "Could not find 'Ignore Raycast' layer.", "OK");
            return;
        }

        // Find all scene GameObjects whose name contains "flower" or "bed" (case-insensitive)
        // so that polyperfect flower bed meshes don't intercept farm tile raycasts
        var allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int count = 0;
        foreach (var go in allObjects)
        {
            string lower = go.name.ToLower();
            if ((lower.Contains("flower") || lower.Contains("flowerbed") || lower.Contains("flower_bed"))
                && go.layer != ignoreRaycast)
            {
                Undo.RecordObject(go, "Set Flower Bed to Ignore Raycast");
                go.layer = ignoreRaycast;
                count++;
            }
        }

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

        EditorUtility.DisplayDialog("Fix Flower Bed Layers",
            count > 0
                ? $"Set {count} flower bed object(s) to 'Ignore Raycast' layer.\n\nSave the scene to persist this change."
                : "No flower bed objects found.\n\nIf clicking is still unreliable, manually set the flower bed GameObjects to layer 'Ignore Raycast' in the Inspector.",
            "OK");

        Debug.Log($"[FarmSceneSetup] Fixed {count} flower bed objects → Ignore Raycast layer.");
    }

    // ─── Helper ────────────────────────────────────────────────────────────────

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }
}
