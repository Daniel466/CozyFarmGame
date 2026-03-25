using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.IO;

/// <summary>
/// Tools > CozyFarm > Setup Player Animator
/// Scans Assets/_Project/Animations/Player/ for Mixamo FBX clips and builds
/// a single AnimatorController with Idle/Walk/Plant/Water/Harvest states.
/// </summary>
public static class PlayerAnimatorSetup
{
    private const string AnimFolder   = "Assets/_Project/Animations/Player";
    private const string OutputPath   = "Assets/_Project/Animations/Player/Player_AC.controller";

    [MenuItem("Tools/CozyFarm/Setup Player Animator")]
    public static void BuildController()
    {
        AnimationClip idle    = FindClip("Idle");
        AnimationClip walk    = FindClip("Walking");
        AnimationClip plant   = FindClip("Plant");
        AnimationClip water   = FindClip("Watering");
        AnimationClip harvest = FindClip("Picking");

        if (idle == null || walk == null)
        {
            Debug.LogError("[PlayerAnimatorSetup] Could not find Idle or Walking clip. " +
                           "Make sure FBX files are in Assets/_Project/Animations/Player/");
            return;
        }

        // Delete existing controller
        if (File.Exists(Path.GetFullPath(OutputPath)))
            AssetDatabase.DeleteAsset(OutputPath);

        var ac = AnimatorController.CreateAnimatorControllerAtPath(OutputPath);

        // Parameters
        ac.AddParameter("Speed",   AnimatorControllerParameterType.Float);
        ac.AddParameter("Plant",   AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Water",   AnimatorControllerParameterType.Trigger);
        ac.AddParameter("Harvest", AnimatorControllerParameterType.Trigger);

        var root = ac.layers[0].stateMachine;

        // States
        var stateIdle    = root.AddState("Idle",    new Vector3(250, 0));
        var stateWalk    = root.AddState("Walk",    new Vector3(250, 80));
        var statePlant   = root.AddState("Plant",   new Vector3(500, 0));
        var stateWater   = root.AddState("Water",   new Vector3(500, 80));
        var stateHarvest = root.AddState("Harvest", new Vector3(500, 160));

        stateIdle.motion    = idle;
        stateWalk.motion    = walk;
        statePlant.motion   = plant   ?? idle;
        stateWater.motion   = water   ?? idle;
        stateHarvest.motion = harvest ?? idle;

        root.defaultState = stateIdle;

        // Idle <-> Walk
        var toWalk = stateIdle.AddTransition(stateWalk);
        toWalk.hasExitTime = false;
        toWalk.duration    = 0.1f;
        toWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");

        var toIdle = stateWalk.AddTransition(stateIdle);
        toIdle.hasExitTime = false;
        toIdle.duration    = 0.1f;
        toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

        // Any State -> action states (interrupt locomotion)
        AddAnyStateTransition(root, statePlant,   "Plant",   ac);
        AddAnyStateTransition(root, stateWater,   "Water",   ac);
        AddAnyStateTransition(root, stateHarvest, "Harvest", ac);

        // Action states -> Idle on exit
        AddExitTransition(statePlant,   stateIdle);
        AddExitTransition(stateWater,   stateIdle);
        AddExitTransition(stateHarvest, stateIdle);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PlayerAnimatorSetup] Player_AC.controller created at {OutputPath}");
        Debug.Log($"  Idle:    {(idle    != null ? idle.name    : "MISSING")}");
        Debug.Log($"  Walk:    {(walk    != null ? walk.name    : "MISSING")}");
        Debug.Log($"  Plant:   {(plant   != null ? plant.name   : "not found - using Idle placeholder")}");
        Debug.Log($"  Water:   {(water   != null ? water.name   : "not found - using Idle placeholder")}");
        Debug.Log($"  Harvest: {(harvest != null ? harvest.name : "not found - using Idle placeholder")}");

        var missing = new System.Collections.Generic.List<string>();
        if (plant   == null) missing.Add("Plant   (needs FBX containing 'Plant')");
        if (water   == null) missing.Add("Water   (needs FBX containing 'Watering')");
        if (harvest == null) missing.Add("Harvest (needs FBX containing 'Picking')");

        string missingMsg = missing.Count > 0
            ? "\n\n⚠ Missing clips (Idle used as placeholder):\n- " + string.Join("\n- ", missing) +
              "\n\nAdd the Mixamo FBX files to:\nAssets/_Project/Animations/Player/\nthen re-run Setup Player Animator."
            : "";

        EditorUtility.DisplayDialog("Player Animator Setup",
            "Player_AC.controller created!" + missingMsg +
            "\n\nNext:\n1. Assign it to the player Animator in Inspector\n" +
            "2. Assign the Synty character model as the player model", "OK");
    }

    [MenuItem("Tools/CozyFarm/List Player Clip Names")]
    public static void ListClips()
    {
        string[] fbxPaths = Directory.GetFiles(AnimFolder, "*.fbx", SearchOption.TopDirectoryOnly);
        if (fbxPaths.Length == 0) { Debug.Log("[PlayerAnimatorSetup] No FBX files found in " + AnimFolder); return; }

        foreach (string path in fbxPaths)
        {
            string assetPath = AnimFolder + "/" + Path.GetFileName(path);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__preview"))
                    Debug.Log($"[PlayerAnimatorSetup] {Path.GetFileName(path)} => clip: \"{clip.name}\"");
            }
        }
    }

    // ---- Helpers ----

    private static AnimationClip FindClip(string keyword)
    {
        string[] fbxPaths = Directory.GetFiles(AnimFolder, "*.fbx", SearchOption.TopDirectoryOnly);
        foreach (string fullPath in fbxPaths)
        {
            if (!Path.GetFileName(fullPath).ToLower().Contains(keyword.ToLower())) continue;

            string assetPath = AnimFolder + "/" + Path.GetFileName(fullPath);
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(assetPath);
            foreach (var a in assets)
            {
                if (a is AnimationClip clip && !clip.name.StartsWith("__preview"))
                    return clip;
            }
        }
        return null;
    }

    private static void AddAnyStateTransition(AnimatorStateMachine sm, AnimatorState target,
                                              string triggerParam, AnimatorController ac)
    {
        var t = sm.AddAnyStateTransition(target);
        t.hasExitTime   = false;
        t.duration      = 0.1f;
        t.canTransitionToSelf = false;
        t.AddCondition(AnimatorConditionMode.If, 0, triggerParam);
    }

    private static void AddExitTransition(AnimatorState from, AnimatorState to)
    {
        var t = from.AddTransition(to);
        t.hasExitTime = true;
        t.exitTime    = 0.9f;
        t.duration    = 0.15f;
    }
}
