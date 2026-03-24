using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// Editor window to build ShibaInu_AC.controller from manually assigned clips.
/// Open via: Tools > CozyFarm > Dog Animator Setup
/// </summary>
public class DogAnimatorGenerator : EditorWindow
{
    private const string OutputPath = "Assets/_Project/Animations/Dog/ShibaInu_AC.controller";

    private AnimationClip clipIdle;
    private AnimationClip clipIdle2;
    private AnimationClip clipWalk;
    private AnimationClip clipGallop;
    private AnimationClip clipPet;    // Idle_2_HeadLow or equivalent
    private AnimationClip clipEat;    // Eating or equivalent

    [MenuItem("Tools/CozyFarm/Dog Animator Setup")]
    public static void Open() => GetWindow<DogAnimatorGenerator>("Dog Animator Setup");

    private void OnGUI()
    {
        EditorGUILayout.LabelField("ShibaInu Animator Controller", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Drag animation clips from the Project window into the slots below, then click Generate.\n\n" +
            "Speed float: 0=Idle  1=Walk  2=Gallop\n" +
            "Pet trigger  →  Pet clip  →  back to Idle\n" +
            "Eat trigger  →  Eat clip  →  back to Idle",
            MessageType.Info);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Locomotion", EditorStyles.boldLabel);
        clipIdle   = ClipField("Idle",   clipIdle);
        clipIdle2  = ClipField("Idle 2 (optional)", clipIdle2);
        clipWalk   = ClipField("Walk",   clipWalk);
        clipGallop = ClipField("Gallop", clipGallop);

        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Interactions", EditorStyles.boldLabel);
        clipPet = ClipField("Pet  (e.g. Idle_2_HeadLow)", clipPet);
        clipEat = ClipField("Eat  (e.g. Eating)",         clipEat);

        EditorGUILayout.Space(10);

        bool canGenerate = clipIdle != null && clipWalk != null && clipGallop != null;
        GUI.enabled = canGenerate;
        if (GUILayout.Button("Generate Controller", GUILayout.Height(36)))
            Generate();
        GUI.enabled = true;

        if (!canGenerate)
            EditorGUILayout.HelpBox("Idle, Walk, and Gallop are required.", MessageType.Warning);
    }

    private static AnimationClip ClipField(string label, AnimationClip clip) =>
        (AnimationClip)EditorGUILayout.ObjectField(label, clip, typeof(AnimationClip), false);

    private void Generate()
    {
        // Ensure output folder exists
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations"))
            AssetDatabase.CreateFolder("Assets/_Project", "Animations");
        if (!AssetDatabase.IsValidFolder("Assets/_Project/Animations/Dog"))
            AssetDatabase.CreateFolder("Assets/_Project/Animations", "Dog");

        // Remove existing controller so we start clean
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(OutputPath) != null)
            AssetDatabase.DeleteAsset(OutputPath);

        var ctrl = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);

        // ── Parameters ───────────────────────────────────────────────────────
        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Pet",   AnimatorControllerParameterType.Trigger);
        ctrl.AddParameter("Eat",   AnimatorControllerParameterType.Trigger);

        var layer = ctrl.layers[0];
        var sm    = layer.stateMachine;
        sm.entryPosition    = new Vector3(-200,   0, 0);
        sm.anyStatePosition = new Vector3(-200,  90, 0);
        sm.exitPosition     = new Vector3( 600,   0, 0);

        // ── States ───────────────────────────────────────────────────────────
        var stIdle   = AddState(sm, "Idle",   clipIdle,   new Vector2(  0,   0));
        var stWalk   = AddState(sm, "Walk",   clipWalk,   new Vector2(220,   0));
        var stGallop = AddState(sm, "Gallop", clipGallop, new Vector2(440,   0));
        var stPet    = AddState(sm, "Pet",    clipPet,    new Vector2(220,  90));
        var stEat    = AddState(sm, "Eat",    clipEat,    new Vector2(440,  90));

        AnimatorState stIdle2 = null;
        if (clipIdle2 != null)
        {
            stIdle2 = AddState(sm, "Idle2", clipIdle2, new Vector2(0, 90));

            // Idle → Idle2 after exit time, only when Speed ~ 0
            var t = stIdle.AddTransition(stIdle2);
            t.hasExitTime = true; t.exitTime = 1f;
            t.hasFixedDuration = true; t.duration = 0.1f;
            t.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            // Idle2 → Idle after exit time
            var t2 = stIdle2.AddTransition(stIdle);
            t2.hasExitTime = true; t2.exitTime = 1f;
            t2.hasFixedDuration = true; t2.duration = 0.1f;
        }

        sm.defaultState = stIdle;

        // ── Locomotion transitions ────────────────────────────────────────────
        AddSpeedTransition(stIdle,   stWalk,   AnimatorConditionMode.Greater, 0.5f);
        AddSpeedTransition(stWalk,   stIdle,   AnimatorConditionMode.Less,    0.5f);
        AddSpeedTransition(stWalk,   stGallop, AnimatorConditionMode.Greater, 1.5f);
        AddSpeedTransition(stGallop, stWalk,   AnimatorConditionMode.Less,    1.5f);

        // ── Pet trigger ───────────────────────────────────────────────────────
        if (clipPet != null)
        {
            var t = sm.AddAnyStateTransition(stPet);
            t.hasExitTime = false; t.hasFixedDuration = true; t.duration = 0.15f;
            t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.If, 0, "Pet");

            var tb = stPet.AddTransition(stIdle);
            tb.hasExitTime = true; tb.exitTime = 0.9f;
            tb.hasFixedDuration = true; tb.duration = 0.15f;
        }

        // ── Eat trigger ───────────────────────────────────────────────────────
        if (clipEat != null)
        {
            var t = sm.AddAnyStateTransition(stEat);
            t.hasExitTime = false; t.hasFixedDuration = true; t.duration = 0.15f;
            t.canTransitionToSelf = false;
            t.AddCondition(AnimatorConditionMode.If, 0, "Eat");

            var tb = stEat.AddTransition(stIdle);
            tb.hasExitTime = true; tb.exitTime = 0.9f;
            tb.hasFixedDuration = true; tb.duration = 0.15f;
        }

        // Persist layer changes
        var layers = ctrl.layers;
        layers[0] = layer;
        ctrl.layers = layers;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<AnimatorController>(OutputPath));
        EditorUtility.DisplayDialog("Done", $"Controller saved to:\n{OutputPath}", "OK");
        Debug.Log($"[DogAnimatorGenerator] Controller saved to {OutputPath}");
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static AnimatorState AddState(AnimatorStateMachine sm, string name, Motion clip, Vector2 pos)
    {
        var state = sm.AddState(name, pos);
        state.motion = clip;
        return state;
    }

    private static void AddSpeedTransition(AnimatorState from, AnimatorState to,
        AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = false;
        t.hasFixedDuration = true;
        t.duration = 0.15f;
        t.AddCondition(mode, threshold, "Speed");
    }
}
