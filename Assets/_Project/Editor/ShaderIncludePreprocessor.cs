using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Ensures URP runtime-created shaders survive build stripping.
/// Runs automatically before every build. Also available as a menu item.
/// </summary>
public class ShaderIncludePreprocessor : IPreprocessBuildWithReport
{
    private static readonly string[] RequiredShaders =
    {
        "Universal Render Pipeline/Unlit",
        "Universal Render Pipeline/Particles/Unlit",
    };

    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        EnsureShadersIncluded();
    }

    [MenuItem("Tools/CozyFarm/Fix Always Included Shaders")]
    public static void EnsureShadersIncluded()
    {
        var graphicsSettings = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset");
        if (graphicsSettings == null || graphicsSettings.Length == 0)
        {
            Debug.LogError("[ShaderInclude] Could not load GraphicsSettings.asset");
            return;
        }

        var so = new SerializedObject(graphicsSettings[0]);
        var prop = so.FindProperty("m_AlwaysIncludedShaders");

        bool changed = false;
        foreach (string shaderName in RequiredShaders)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogWarning($"[ShaderInclude] Shader not found: '{shaderName}' — skipping.");
                continue;
            }

            bool alreadyPresent = false;
            for (int i = 0; i < prop.arraySize; i++)
            {
                if (prop.GetArrayElementAtIndex(i).objectReferenceValue == shader)
                {
                    alreadyPresent = true;
                    break;
                }
            }

            if (!alreadyPresent)
            {
                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = shader;
                changed = true;
                Debug.Log($"[ShaderInclude] Added '{shaderName}' to Always Included Shaders.");
            }
            else
            {
                Debug.Log($"[ShaderInclude] '{shaderName}' already in Always Included Shaders.");
            }
        }

        if (changed)
        {
            so.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("[ShaderInclude] GraphicsSettings saved.");
        }
    }
}
