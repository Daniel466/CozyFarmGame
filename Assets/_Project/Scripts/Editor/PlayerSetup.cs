using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Tools > CozyFarm > Setup Player
///
/// EditorWindow — drag in a character model from Characters.fbx, then click Setup.
///
/// Does automatically:
///   - Instantiates the chosen model as a child of the Player GameObject
///   - Assigns Player_AC.controller to the model's Animator
///   - Wires PlayerInteraction.playerController reference
///   - Sets model scale and offset on PlayerController
///
/// Prerequisites:
///   - Run Tools > CozyFarm > Setup Player Animator first (creates Player_AC.controller)
///   - Player GameObject must have PlayerController + PlayerInteraction components
///   - FBX Rig must be set to Humanoid on Characters.fbx and all Mixamo FBX files
/// </summary>
public class PlayerSetup : EditorWindow
{
    private const string ControllerPath = "Assets/_Project/Animations/Player/Player_AC.controller";

    private GameObject characterModelPrefab;
    private float      modelScale  = 1f;
    private Vector3    modelOffset = new Vector3(0f, -1f, 0f); // matches PlayerController default

    [MenuItem("Tools/CozyFarm/Setup Player")]
    public static void Open() => GetWindow<PlayerSetup>("Player Setup").Show();

    private void OnGUI()
    {
        GUILayout.Label("Player Setup", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        EditorGUILayout.HelpBox(
            "1. Run 'Setup Player Animator' first.\n" +
            "2. Set Characters.fbx Rig to Humanoid.\n" +
            "3. Drag a character from Characters.fbx into the slot below.\n" +
            "4. Click Setup Player.",
            MessageType.Info);

        EditorGUILayout.Space(8);

        characterModelPrefab = (GameObject)EditorGUILayout.ObjectField(
            "Character Model", characterModelPrefab, typeof(GameObject), false);

        modelScale  = EditorGUILayout.FloatField("Model Scale",  modelScale);
        modelOffset = EditorGUILayout.Vector3Field("Model Offset", modelOffset);

        EditorGUILayout.Space(8);

        GUI.enabled = characterModelPrefab != null;
        if (GUILayout.Button("Setup Player", GUILayout.Height(36)))
            RunSetup();
        GUI.enabled = true;

        EditorGUILayout.Space(4);
        if (GUILayout.Button("List Characters.fbx Objects"))
            ListCharacterObjects();
    }

    private void RunSetup()
    {
        // Find player in scene
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController == null)
        {
            EditorUtility.DisplayDialog("Setup Player", "No PlayerController found in scene. Open the main scene first.", "OK");
            return;
        }
        GameObject player = playerController.gameObject;

        // Load Player_AC controller
        var ac = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (ac == null)
        {
            EditorUtility.DisplayDialog("Setup Player",
                "Player_AC.controller not found at:\n" + ControllerPath +
                "\n\nRun Tools > CozyFarm > Setup Player Animator first.", "OK");
            return;
        }

        // Remove any existing model child tagged as the player model
        var existing = player.transform.Find("PlayerModel");
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
            Debug.Log("[PlayerSetup] Removed existing PlayerModel child.");
        }

        // Instantiate model as child
        GameObject model = (GameObject)PrefabUtility.InstantiatePrefab(characterModelPrefab, player.transform);
        model.name = "PlayerModel";
        model.transform.localPosition = modelOffset;
        model.transform.localRotation = Quaternion.identity;
        model.transform.localScale    = Vector3.one * modelScale;
        Undo.RegisterCreatedObjectUndo(model, "Setup Player Model");

        // Find Animator on model (may be on a child)
        var animator = model.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            animator = model.AddComponent<Animator>();
            Debug.Log("[PlayerSetup] Added Animator to model root.");
        }
        animator.runtimeAnimatorController = ac;
        animator.applyRootMotion           = false;

        // Assign animator to PlayerController via SerializedObject
        var so = new SerializedObject(playerController);
        so.FindProperty("animator").objectReferenceValue      = animator;
        so.FindProperty("characterModel").objectReferenceValue = model.transform;
        so.FindProperty("modelScale").floatValue              = modelScale;
        so.FindProperty("modelOffset").vector3Value           = modelOffset;
        so.ApplyModifiedProperties();

        // Wire PlayerInteraction.playerController
        var interaction = player.GetComponent<PlayerInteraction>();
        if (interaction != null)
        {
            var soI = new SerializedObject(interaction);
            soI.FindProperty("playerController").objectReferenceValue = playerController;
            soI.ApplyModifiedProperties();
            Debug.Log("[PlayerSetup] Wired PlayerInteraction.playerController.");
        }

        EditorUtility.SetDirty(player);
        Debug.Log($"[PlayerSetup] Done! Model: {model.name}, Controller: {ac.name}, Scale: {modelScale}");
        EditorUtility.DisplayDialog("Setup Player",
            "Player setup complete!\n\n" +
            "If the character looks wrong:\n" +
            "- Check Rig > Humanoid on Characters.fbx\n" +
            "- Adjust Model Scale / Offset and re-run\n" +
            "- Make sure applyRootMotion = false on Animator", "OK");
    }

    private void ListCharacterObjects()
    {
        const string fbxPath = "Assets/PaidAssets/Synty/Hybrid approach/Models/Characters.fbx";
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        if (assets == null || assets.Length == 0)
        {
            Debug.Log("[PlayerSetup] Characters.fbx not found at: " + fbxPath);
            return;
        }
        foreach (var a in assets)
        {
            if (a is GameObject)
                Debug.Log($"[PlayerSetup] GameObject in Characters.fbx: \"{a.name}\"");
        }
    }

    // ---------------------------------------------------------------

    [MenuItem("Tools/CozyFarm/Fix Player FBX Rigs (Set Humanoid)")]
    public static void FixRigs()
    {
        string[] paths = new[]
        {
            "Assets/PaidAssets/Synty/Hybrid approach/Models/Characters.fbx",
            "Assets/_Project/Animations/Player/Characters@Idle.fbx",
            "Assets/_Project/Animations/Player/Characters@Walking.fbx",
            "Assets/_Project/Animations/Player/Characters@Plant A Plant.fbx",
            "Assets/_Project/Animations/Player/Characters@Watering.fbx",
            "Assets/_Project/Animations/Player/Characters@Picking Up.fbx",
        };

        int updatedCount = 0;
        foreach (string path in paths)
        {
            var importer = AssetImporter.GetAtPath(path) as ModelImporter;
            if (importer == null)
            {
                Debug.LogWarning($"[PlayerSetup] Not found or not a model: {path}");
                continue;
            }

            bool changed = false;

            if (importer.animationType != ModelImporterAnimationType.Human)
            {
                importer.animationType = ModelImporterAnimationType.Human;
                changed = true;
            }

            // For animation-only FBX (@ clips) — import animations, no mesh needed
            if (path.Contains("@"))
            {
                if (!importer.importAnimation) { importer.importAnimation = true; changed = true; }

                bool isLoop = path.Contains("Idle") || path.Contains("Walking");

                // Get existing clips or build a default one from the FBX source clip
                var clips = importer.clipAnimations;
                if (clips == null || clips.Length == 0)
                {
                    // No explicit clips defined — create one covering the full take
                    var defaultClip = new ModelImporterClipAnimation
                    {
                        name               = System.IO.Path.GetFileNameWithoutExtension(path).Replace("Characters@", ""),
                        takeName           = "mixamo.com",
                        firstFrame         = 0,
                        lastFrame          = 999, // Unity clamps to actual length
                        loopTime           = isLoop,
                        lockRootHeightY    = true,
                        lockRootRotation   = !isLoop, // lock rotation on action clips; locomotion clips rotate freely
                        lockRootPositionXZ = !isLoop, // lock XZ on action clips; locomotion clips stay in place anyway
                        heightFromFeet     = false,
                    };
                    importer.clipAnimations = new[] { defaultClip };
                    changed = true;
                }
                else
                {
                    bool clipsChanged = false;
                    foreach (var clip in clips)
                    {
                        if (!clip.lockRootHeightY)  { clip.lockRootHeightY  = true;   clipsChanged = true; }
                        if (clip.heightFromFeet)    { clip.heightFromFeet   = false;  clipsChanged = true; }
                        if (isLoop && !clip.loopTime) { clip.loopTime = true;          clipsChanged = true; }
                        // Action clips (non-looping) must lock rotation and XZ so the skeleton
                        // stays upright in place — Mixamo bakes root rotation into the take by default
                        if (!isLoop && !clip.lockRootRotation)   { clip.lockRootRotation   = true; clipsChanged = true; }
                        if (!isLoop && !clip.lockRootPositionXZ) { clip.lockRootPositionXZ = true; clipsChanged = true; }
                    }
                    if (clipsChanged) { importer.clipAnimations = clips; changed = true; }
                }
            }

            if (changed)
            {
                importer.SaveAndReimport();
                Debug.Log($"[PlayerSetup] Set Humanoid + fixed settings: {path}");
                updatedCount++;
            }
            else
            {
                Debug.Log($"[PlayerSetup] Already correct: {path}");
            }
        }

        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Fix Player FBX Rigs",
            $"Done! {updatedCount} file(s) updated to Humanoid.\nCheck Console for details.", "OK");
    }
}
