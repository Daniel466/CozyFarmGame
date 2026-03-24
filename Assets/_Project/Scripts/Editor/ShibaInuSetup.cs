using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

/// <summary>
/// One-shot editor tool — run via Tools > CozyFarm > Setup ShibaInu Dog.
///
/// Creates:
///   Assets/_Project/Animations/Animals/ShibaInu_AC.controller
///     - Float  "Speed"  — 0=Idle, 1=Walk, 2=Gallop
///     - Trigger "Eat"   — plays Eating, exits back to locomotion
///     - Trigger "Pet"   — plays Idle_2_HeadLow, exits back to locomotion
///
/// Then builds or refreshes:
///   Assets/_Project/Prefabs/Animals/ShibaInu_Dog.prefab
///     - Root with NavMeshAgent + DogController + CapsuleCollider
///     - ShibaInu FBX model as a child with Animator assigned
///     - InteractionPrompt world-space label child
/// </summary>
public static class ShibaInuSetup
{
    private const string FbxPath        = "Assets/PaidAssets/Ultimate Animated Animals - July 2021/FBX/ShibaInu.fbx";
    private const string ControllerPath = "Assets/_Project/Animations/Animals/ShibaInu_AC.controller";
    private const string PrefabPath     = "Assets/_Project/Prefabs/Animals/ShibaInu_Dog.prefab";
    private const string FontPath       = "Assets/_Project/Art/Fonts/Kenney Future SDF.asset";

    [MenuItem("Tools/CozyFarm/List ShibaInu Clip Names")]
    public static void ListClips()
    {
        var all = AssetDatabase.LoadAllAssetsAtPath(FbxPath);
        if (all == null || all.Length == 0)
        {
            EditorUtility.DisplayDialog("Not found", "FBX not found at:\n" + FbxPath, "OK");
            return;
        }
        var sb = new System.Text.StringBuilder();
        foreach (var obj in all)
            if (obj is AnimationClip c && !c.name.StartsWith("__"))
                sb.AppendLine(c.name);
        string result = sb.Length > 0 ? sb.ToString() : "No AnimationClips found in FBX.";
        Debug.Log("[ShibaInuSetup] Clip names:\n" + result);
        EditorUtility.DisplayDialog("ShibaInu Clip Names", result, "OK");
    }

    [MenuItem("Tools/CozyFarm/Setup ShibaInu Dog")]
    public static void Run()
    {
        EnsureDirectories();
        var controller = BuildAnimatorController();
        BuildPrefab(controller);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[ShibaInuSetup] Done. Prefab saved to: " + PrefabPath);
        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath));
    }

    // -------------------------------------------------------------------------
    // Directories
    // -------------------------------------------------------------------------

    private static void EnsureDirectories()
    {
        CreateFolderIfMissing("Assets/_Project/Animations");
        CreateFolderIfMissing("Assets/_Project/Animations/Animals");
        CreateFolderIfMissing("Assets/_Project/Prefabs/Animals");
    }

    private static void CreateFolderIfMissing(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            int last = path.LastIndexOf('/');
            AssetDatabase.CreateFolder(path[..last], path[(last + 1)..]);
        }
    }

    // -------------------------------------------------------------------------
    // AnimatorController
    // -------------------------------------------------------------------------

    private static AnimatorController BuildAnimatorController()
    {
        // Always delete and recreate for a clean build
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        var ac = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        // Parameters
        ac.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ac.AddParameter("Eat",   AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Pet",   AnimatorControllerParameterType.Trigger);

        var layer     = ac.layers[0];
        var stateMachine = layer.stateMachine;

        // Load clips from FBX
        var idle    = LoadClip("AnimalArmature|Idle");
        var walk    = LoadClip("AnimalArmature|Walk");
        var gallop  = LoadClip("AnimalArmature|Gallop");
        var eating  = LoadClip("AnimalArmature|Eating");
        var petClip = LoadClip("AnimalArmature|Idle_2_HeadLow");

        // Enable looping on locomotion clips via SerializedObject
        SetLooping(idle,   true);
        SetLooping(walk,   true);
        SetLooping(gallop, true);
        SetLooping(eating, false);
        SetLooping(petClip, false);

        // States
        var stIdle   = stateMachine.AddState("Idle");
        var stWalk   = stateMachine.AddState("Walk");
        var stGallop = stateMachine.AddState("Gallop");
        var stEat    = stateMachine.AddState("Eat");
        var stPet    = stateMachine.AddState("Pet");

        stIdle.motion   = idle;
        stWalk.motion   = walk;
        stGallop.motion = gallop;
        stEat.motion    = eating;
        stPet.motion    = petClip;

        stateMachine.defaultState = stIdle;

        // ── Locomotion transitions driven by Speed ─────────────────────────────

        // Idle -> Walk (Speed >= 0.5)
        AddTransition(stIdle, stWalk, "Speed", AnimatorConditionMode.Greater, 0.5f);
        // Walk -> Idle (Speed < 0.5)
        AddTransition(stWalk, stIdle, "Speed", AnimatorConditionMode.Less, 0.5f);
        // Walk -> Gallop (Speed >= 1.5)
        AddTransition(stWalk, stGallop, "Speed", AnimatorConditionMode.Greater, 1.5f);
        // Gallop -> Walk (Speed < 1.5)
        AddTransition(stGallop, stWalk, "Speed", AnimatorConditionMode.Less, 1.5f);
        // Idle -> Gallop (Speed >= 1.5) — safety shortcut when catching up fast
        AddTransition(stIdle, stGallop, "Speed", AnimatorConditionMode.Greater, 1.5f);

        // ── Eat trigger — from any locomotion state ────────────────────────────
        AddTriggerTransition(stIdle,   stEat, "Eat");
        AddTriggerTransition(stWalk,   stEat, "Eat");
        AddTriggerTransition(stGallop, stEat, "Eat");
        // Eat -> Idle on finish
        var eatExit = stEat.AddTransition(stIdle);
        eatExit.hasExitTime    = true;
        eatExit.exitTime       = 1f;
        eatExit.hasFixedDuration = false;
        eatExit.duration       = 0.15f;

        // ── Pet trigger — from any locomotion state ────────────────────────────
        AddTriggerTransition(stIdle,   stPet, "Pet");
        AddTriggerTransition(stWalk,   stPet, "Pet");
        AddTriggerTransition(stGallop, stPet, "Pet");
        // Pet -> Idle on finish
        var petExit = stPet.AddTransition(stIdle);
        petExit.hasExitTime      = true;
        petExit.exitTime         = 1f;
        petExit.hasFixedDuration = false;
        petExit.duration         = 0.15f;

        EditorUtility.SetDirty(ac);
        return ac;
    }

    private static AnimationClip LoadClip(string name)
    {
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(FbxPath))
            if (obj is AnimationClip clip && clip.name == name)
                return clip;

        Debug.LogWarning($"[ShibaInuSetup] Clip not found in FBX: {name}");
        return null;
    }

    private static void SetLooping(AnimationClip clip, bool loop)
    {
        if (clip == null) return;
        var so    = new SerializedObject(clip);
        var settings = so.FindProperty("m_AnimationClipSettings");
        settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    private static AnimatorStateTransition AddTransition(
        AnimatorState from, AnimatorState to,
        string param, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime      = false;
        t.hasFixedDuration = true;
        t.duration         = 0.15f;
        t.AddCondition(mode, threshold, param);
        return t;
    }

    private static AnimatorStateTransition AddTriggerTransition(
        AnimatorState from, AnimatorState to, string trigger)
    {
        var t = from.AddTransition(to);
        t.hasExitTime      = false;
        t.hasFixedDuration = true;
        t.duration         = 0.10f;
        t.AddCondition(AnimatorConditionMode.If, 0f, trigger);
        return t;
    }

    // -------------------------------------------------------------------------
    // Prefab
    // -------------------------------------------------------------------------

    private static void BuildPrefab(AnimatorController controller)
    {
        // Root GameObject
        var root = new GameObject("ShibaInu_Dog");

        // ── NavMeshAgent ───────────────────────────────────────────────────────
        var agent           = root.AddComponent<UnityEngine.AI.NavMeshAgent>();
        agent.radius        = 0.28f;
        agent.height        = 0.55f;
        agent.baseOffset    = 0f;
        agent.speed         = 2.0f;
        agent.angularSpeed  = 300f;
        agent.acceleration  = 14f;
        agent.stoppingDistance = 1.6f;
        agent.autoBraking   = true;
        agent.autoRepath    = true;
        agent.obstacleAvoidanceType = UnityEngine.AI.ObstacleAvoidanceType.MedQualityObstacleAvoidance;

        // ── CapsuleCollider — tuned to ShibaInu mesh at scale 1 ───────────────
        var capsule       = root.AddComponent<CapsuleCollider>();
        capsule.direction = 2;                               // Z — front-to-back
        capsule.center    = new Vector3(0f, 0.28f, 0.05f);
        capsule.radius    = 0.18f;
        capsule.height    = 0.72f;

        // ── DogController ─────────────────────────────────────────────────────
        root.AddComponent<DogController>();

        // ── Model child (ShibaInu FBX instance) ───────────────────────────────
        var fbxRoot = AssetDatabase.LoadAssetAtPath<GameObject>(FbxPath);
        if (fbxRoot == null)
        {
            Debug.LogError("[ShibaInuSetup] ShibaInu.fbx not found at: " + FbxPath);
            Object.DestroyImmediate(root);
            return;
        }

        var model         = (GameObject)PrefabUtility.InstantiatePrefab(fbxRoot);
        model.name        = "ShibaInu_Model";
        model.transform.SetParent(root.transform, false);
        // FBX mesh is authored upright. Scale chosen so the dog stands ~0.55m tall
        // (comparable to a medium Shiba Inu in a cozy game world).
        model.transform.localScale    = Vector3.one * 0.165f;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // Unpack the FBX prefab instance so we can freely add/remove components
        PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

        // Strip any MonoBehaviours from the asset pack that may disable the model at runtime
        foreach (var mono in model.GetComponentsInChildren<MonoBehaviour>(true))
        {
            Debug.Log($"[ShibaInuSetup] Removing pack script: {mono.GetType().Name}");
            Object.DestroyImmediate(mono);
        }

        // Ensure model and all children are active
        model.SetActive(true);
        foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            t.gameObject.SetActive(true);

        // Get existing Animator (on root or any child) or add one to the root
        var animator = model.GetComponentInChildren<Animator>(true) ?? model.AddComponent<Animator>();
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion           = false;
        animator.updateMode                = AnimatorUpdateMode.Normal;
        animator.cullingMode               = AnimatorCullingMode.CullUpdateTransforms;

        // ── InteractionPrompt child (world-space TextMesh) ─────────────────────
        var prompt         = new GameObject("InteractionPrompt");
        prompt.transform.SetParent(root.transform, false);
        prompt.transform.localPosition = new Vector3(0f, 1.1f, 0f);
        prompt.transform.localScale    = Vector3.one * 0.006f;
        prompt.SetActive(false);

        // TextMeshPro 3D — requires TMPro assembly
        var tmp = prompt.AddComponent<TMPro.TextMeshPro>();
        tmp.text      = "[E] Pet";
        tmp.fontSize  = 18f;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.color     = new Color(1f, 0.95f, 0.6f, 1f);
        tmp.outlineWidth = 0.25f;
        tmp.outlineColor = new Color32(0, 0, 0, 220);
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        tmp.overflowMode     = TMPro.TextOverflowModes.Overflow;
        tmp.isOrthographic   = false;

        var font = AssetDatabase.LoadAssetAtPath<TMPro.TMP_FontAsset>(FontPath);
        if (font != null) tmp.font = font;

        // ── Wire DogController references ──────────────────────────────────────
        var dc = root.GetComponent<DogController>();
        // dogAnimator — set via SerializedObject so the private field is reached
        var so   = new SerializedObject(dc);
        so.FindProperty("dogAnimator").objectReferenceValue              = animator;
        so.FindProperty("interactionPromptRoot").objectReferenceValue    = prompt;
        so.ApplyModifiedPropertiesWithoutUndo();

        // ── Save as prefab ─────────────────────────────────────────────────────
        bool success;
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out success);
        Object.DestroyImmediate(root);

        if (success)
            Debug.Log("[ShibaInuSetup] Prefab created: " + PrefabPath);
        else
            Debug.LogError("[ShibaInuSetup] Failed to save prefab.");
    }
}
