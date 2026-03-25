using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Tools > CozyFarm > Migrate To Farm Demo Scene
///
/// Moves all game-system GameObjects from Farm.unity into DEMO_07_Farm.unity
/// and saves the result as Assets/_Project/Scenes/Farm.unity.
///
/// What migrates:  GameManager, Player, HUD Canvas, PanelsCanvas, Main Camera,
///                 DogManager, CollectibleSpawner, AudioManager, LightingManager,
///                 PauseMenu, EventSystem
///
/// What is dropped (replaced by demo scene art):
///                 Directional Light, Fill Light, T E R R A I N, G R A S S,
///                 F A R M, F E N C E, N A T U R E, A N I M A L S, P E O P L E,
///                 F L O W E R S, V E H I C L E S, P R O P S, separator objects
/// </summary>
public static class SceneMigrationTool
{
    private const string DemoScenePath = "Assets/PaidAssets/Poly Universal Pack/- Demo Scenes/DEMO_07_Farm.unity";
    private const string FarmScenePath = "Assets/_Project/Scenes/Farm.unity";
    private const string BackupPath    = "Assets/_Project/Scenes/Farm_Backup.unity";

    // These objects come with us into the new scene
    private static readonly HashSet<string> SystemObjects = new HashSet<string>
    {
        "GameManager",
        "Player",
        "HUD Canvas",
        "PanelsCanvas",
        "Main Camera",
        "DogManager",
        "CollectibleSpawner",
        "AudioManager",
        "LightingManager",
        "PauseMenu",
        "EventSystem",
    };

    [MenuItem("Tools/CozyFarm/Migrate To Farm Demo Scene")]
    public static void Migrate()
    {
        // ── Safety check ────────────────────────────────────────────────────
        if (!AssetDatabase.LoadAssetAtPath<Object>(DemoScenePath))
        {
            EditorUtility.DisplayDialog("Migration Failed",
                $"Could not find demo scene:\n{DemoScenePath}", "OK");
            return;
        }

        bool confirm = EditorUtility.DisplayDialog("Migrate Scene",
            "This will:\n\n" +
            "1. Back up Farm.unity -> Farm_Backup.unity\n" +
            "2. Open DEMO_07_Farm as the base\n" +
            "3. Move all game systems into it\n" +
            "4. Save as Farm.unity\n\n" +
            "Make sure you have saved any open scene changes first.",
            "Migrate", "Cancel");

        if (!confirm) return;

        // ── Step 1: Backup ──────────────────────────────────────────────────
        string farmFull   = Path.Combine(Application.dataPath, "../", FarmScenePath).Replace('/', Path.DirectorySeparatorChar);
        string backupFull = Path.Combine(Application.dataPath, "../", BackupPath).Replace('/', Path.DirectorySeparatorChar);
        File.Copy(farmFull, backupFull, overwrite: true);
        AssetDatabase.Refresh();
        Debug.Log("[SceneMigration] Backed up Farm.unity -> Farm_Backup.unity");

        // ── Step 2: Open demo scene as base ─────────────────────────────────
        var demoScene = EditorSceneManager.OpenScene(DemoScenePath, OpenSceneMode.Single);
        if (!demoScene.IsValid())
        {
            EditorUtility.DisplayDialog("Migration Failed", "Could not open demo scene.", "OK");
            return;
        }

        // ── Step 3: Open Farm additively ────────────────────────────────────
        var farmScene = EditorSceneManager.OpenScene(FarmScenePath, OpenSceneMode.Additive);
        if (!farmScene.IsValid())
        {
            EditorUtility.DisplayDialog("Migration Failed", "Could not open Farm scene additively.", "OK");
            return;
        }

        // ── Step 4: Move system objects → demo scene ─────────────────────────
        int moved   = 0;
        int dropped = 0;
        var toDestroy = new List<GameObject>();

        foreach (GameObject go in farmScene.GetRootGameObjects())
        {
            string name = go.name.Trim();

            if (SystemObjects.Contains(name))
            {
                SceneManager.MoveGameObjectToScene(go, demoScene);
                moved++;
                Debug.Log($"[SceneMigration] Moved: {name}");
            }
            else
            {
                toDestroy.Add(go);
                dropped++;
                Debug.Log($"[SceneMigration] Dropping old art: {name}");
            }
        }

        foreach (var go in toDestroy)
            Object.DestroyImmediate(go);

        // ── Step 5: Remove demo-scene duplicates of our system objects ───────
        // After moving our objects in, the demo scene may still have its own
        // versions. Destroy any root GO whose name matches a system object
        // but doesn't have our key scripts on it.
        var demoRoots = new List<GameObject>(demoScene.GetRootGameObjects());
        foreach (GameObject go in demoRoots)
        {
            string name = go.name.Trim();
            if (!SystemObjects.Contains(name)) continue;

            // Count how many root GOs share this name — if more than one, the
            // demo brought its own copy. Destroy the one without our scripts.
            var siblings = System.Array.FindAll(
                demoScene.GetRootGameObjects(),
                r => r.name.Trim() == name);

            if (siblings.Length <= 1) continue;

            foreach (var sibling in siblings)
            {
                // Keep the GO that has at least one of our identifying scripts
                bool isOurs = sibling.GetComponent<FarmGrid>()        != null
                           || sibling.GetComponent<PlayerController>() != null
                           || sibling.GetComponent<AudioManager>()     != null
                           || sibling.GetComponent<Camera>() != null && sibling.GetComponent<FarmCamera>() != null
                           || sibling.GetComponent<PauseMenuUI>()      != null
                           || sibling.GetComponent<CozyLightingSetup>() != null
                           || sibling.GetComponent<TimeOfDay>()        != null;

                if (!isOurs)
                {
                    Debug.Log($"[SceneMigration] Removing demo duplicate: {sibling.name}");
                    Object.DestroyImmediate(sibling);
                    break; // only remove one duplicate per name
                }
            }
        }

        // ── Step 6: Save demo scene as Farm.unity ───────────────────────────
        EditorSceneManager.SaveScene(demoScene, FarmScenePath);
        EditorSceneManager.CloseScene(farmScene, true);

        AssetDatabase.Refresh();

        Debug.Log($"[SceneMigration] Done. Moved {moved} system objects, dropped {dropped} old art objects.");
        EditorUtility.DisplayDialog("Migration Complete",
            $"Success!\n\n" +
            $"Moved {moved} game-system objects into the demo scene.\n" +
            $"Dropped {dropped} old art objects (replaced by demo environment).\n\n" +
            "NEXT STEPS:\n" +
            "- Reposition GameManager > FarmGrid > Grid Origin to align with the new terrain\n" +
            "- Reposition Player spawn position\n" +
            "- Reposition CollectibleSpawner spawn points\n" +
            "- Check camera framing with FarmCamera\n" +
            "- Original saved as Farm_Backup.unity",
            "OK");
    }
}
