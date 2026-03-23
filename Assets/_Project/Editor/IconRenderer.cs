using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

/// <summary>
/// Renders each CropData and BuildingData prefab to a 128x128 PNG icon.
/// Uses PreviewRenderUtility for URP-compatible off-screen rendering.
/// Saves PNGs to Assets/_Project/Art/Icons/, assigns back to assets, and
/// packs a TMP_SpriteAsset atlas so icons can be used in TMP text via
///   <sprite name="carrot"> / <sprite name="watering_well">
///
/// Run via: Tools > CozyFarm > Render Icons
/// </summary>
public class IconRenderer : Editor
{
    const int    IconSize  = 128;
    const string OutputDir = "Assets/_Project/Art/Icons";
    const string CropDir   = "Assets/_Project/Art/Icons/Crops";
    const string BldgDir   = "Assets/_Project/Art/Icons/Buildings";

    struct IconEntry { public string id; public string assetPath; }

    // ─────────────────────────── Menu entry ──────────────────────────

    [MenuItem("Tools/CozyFarm/Render Icons")]
    public static void RenderAllIcons()
    {
        EnsureDir(OutputDir);
        EnsureDir(CropDir);
        EnsureDir(BldgDir);

        var entries = new List<IconEntry>();
        int skipped = 0;

        // ── Crops ────────────────────────────────────────────────────
        foreach (var guid in AssetDatabase.FindAssets("t:CropData"))
        {
            var crop = AssetDatabase.LoadAssetAtPath<CropData>(
                AssetDatabase.GUIDToAssetPath(guid));
            var cropPrefab = GetCropIconPrefab(crop);
            if (crop == null || cropPrefab == null) { skipped++; continue; }

            Texture2D icon = RenderPrefabIcon(cropPrefab);
            if (icon == null) { skipped++; continue; }

            string iconPath = $"{CropDir}/{crop.CropId}.png";
            SavePNG(icon, iconPath);
            Object.DestroyImmediate(icon);
            entries.Add(new IconEntry { id = crop.CropId, assetPath = iconPath });
        }

        // ── Buildings ────────────────────────────────────────────────
        foreach (var guid in AssetDatabase.FindAssets("t:BuildingData"))
        {
            var bldg = AssetDatabase.LoadAssetAtPath<BuildingData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (bldg == null || bldg.Prefab == null) { skipped++; continue; }

            Texture2D icon = RenderPrefabIcon(bldg.Prefab);
            if (icon == null) { skipped++; continue; }

            string iconPath = $"{BldgDir}/{bldg.BuildingId}.png";
            SavePNG(icon, iconPath);
            Object.DestroyImmediate(icon);
            entries.Add(new IconEntry { id = bldg.BuildingId, assetPath = iconPath });
        }

        if (entries.Count == 0)
        {
            EditorUtility.DisplayDialog("Icon Renderer",
                $"No icons rendered (all prefabs null or skipped={skipped}).", "OK");
            return;
        }

        // ── Import as sprites ────────────────────────────────────────
        AssetDatabase.Refresh();
        foreach (var e in entries)
            ImportAsSprite(e.assetPath);
        AssetDatabase.Refresh();

        // ── Assign back to CropData / BuildingData ───────────────────
        AssignCropIcons(entries);
        AssignBuildingIcons(entries);

        // ── Pack atlas + create TMP Sprite Asset ─────────────────────
        CreateTMPSpriteAsset(entries);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Icon Renderer",
            $"Done! {entries.Count} icons rendered, {skipped} skipped.\n\n" +
            $"Icons:        {OutputDir}/Crops|Buildings/\n" +
            $"Atlas:        {OutputDir}/Icons_Atlas.png\n" +
            $"TMP Asset:    {OutputDir}/Icons_SpriteAsset.asset\n\n" +
            "Use in TMP text:\n  <sprite name=\"carrot\">",
            "OK");
    }

    // ─────────────────────────── Rendering ───────────────────────────

    /// <summary>
    /// Renders the prefab to a 128x128 Texture2D using PreviewRenderUtility.
    /// Bypasses BeginStaticPreview/EndStaticPreview (which composite onto an opaque
    /// gray background) and instead renders to our own ARGB32 RenderTexture so the
    /// background stays fully transparent.
    /// Camera: orthographic, isometric-ish 30/45 angle, auto-framed to bounds.
    /// </summary>
    static Texture2D RenderPrefabIcon(GameObject prefab)
    {
        var preview = new PreviewRenderUtility();

        // Key light — top-left, warm
        preview.lights[0].type      = LightType.Directional;
        preview.lights[0].intensity = 1.3f;
        preview.lights[0].color     = new Color(1f, 0.97f, 0.9f);
        preview.lights[0].transform.rotation = Quaternion.Euler(35f, -135f, 0f);

        // Fill light — soft from right
        if (preview.lights.Length > 1)
        {
            preview.lights[1].type      = LightType.Directional;
            preview.lights[1].intensity = 0.4f;
            preview.lights[1].color     = new Color(0.8f, 0.88f, 1f);
            preview.lights[1].transform.rotation = Quaternion.Euler(-10f, 50f, 0f);
        }

        // Instantiate into preview scene to get accurate world bounds
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        preview.AddSingleGO(instance);

        Bounds bounds = GetRendererBounds(instance);
        float extent  = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z, 0.1f);

        // Centre the model at origin inside the preview scene
        instance.transform.position = -bounds.center;

        // Orthographic camera — transparent background
        preview.camera.orthographic     = true;
        preview.camera.orthographicSize = extent * 1.35f;
        preview.camera.nearClipPlane    = 0.001f;
        preview.camera.farClipPlane     = extent * 30f;
        preview.camera.clearFlags       = CameraClearFlags.SolidColor;
        preview.camera.backgroundColor  = new Color(0f, 0f, 0f, 0f); // fully transparent

        Quaternion camRot = Quaternion.Euler(30f, -45f, 0f);
        preview.camera.transform.rotation = camRot;
        preview.camera.transform.position = camRot * (Vector3.back * extent * 8f);

        // Render to our own ARGB32 RenderTexture — preserves alpha channel
        var rt = new RenderTexture(IconSize, IconSize, 24, RenderTextureFormat.ARGB32);
        preview.camera.targetTexture = rt;

        Texture2D result = null;
        RenderTexture prevActive = RenderTexture.active;
        try
        {
            preview.camera.Render();

            RenderTexture.active = rt;
            result = new Texture2D(IconSize, IconSize, TextureFormat.RGBA32, false);
            result.ReadPixels(new Rect(0, 0, IconSize, IconSize), 0, 0);
            result.Apply();
        }
        finally
        {
            RenderTexture.active = prevActive;
            preview.camera.targetTexture = null;
            Object.DestroyImmediate(rt);
            Object.DestroyImmediate(instance);
            preview.Cleanup();
        }

        return result;
    }

    /// <summary>Returns the last non-null growth stage prefab (stage 3 = ready to harvest).</summary>
    static GameObject GetCropIconPrefab(CropData crop)
    {
        if (crop == null) return null;
        var stages = crop.GrowthStagePrefabs;
        if (stages == null || stages.Length == 0) return null;
        for (int i = stages.Length - 1; i >= 0; i--)
            if (stages[i] != null) return stages[i];
        return null;
    }

    static Bounds GetRendererBounds(GameObject go)
    {
        var renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.one);
        Bounds b = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        return b;
    }

    // ─────────────────────────── File I/O ────────────────────────────

    static void SavePNG(Texture2D tex, string assetPath)
    {
        string full = ToFullPath(assetPath);
        File.WriteAllBytes(full, tex.EncodeToPNG());
    }

    static void EnsureDir(string assetPath)
    {
        Directory.CreateDirectory(ToFullPath(assetPath));
    }

    static string ToFullPath(string assetPath) =>
        Application.dataPath + assetPath.Substring("Assets".Length);

    static void ImportAsSprite(string path)
    {
        var ti = AssetImporter.GetAtPath(path) as TextureImporter;
        if (ti == null) return;
        ti.textureType           = TextureImporterType.Sprite;
        ti.spriteImportMode      = SpriteImportMode.Single;
        ti.mipmapEnabled         = false;
        ti.isReadable            = true;   // needed for atlas GetPixels()
        ti.filterMode            = FilterMode.Bilinear;
        ti.textureCompression    = TextureImporterCompression.Uncompressed;
        ti.alphaIsTransparency   = true;
        ti.SaveAndReimport();
    }

    // ───────────────────────── Asset assignment ───────────────────────

    static void AssignCropIcons(List<IconEntry> entries)
    {
        var map = BuildSpriteMap(entries);
        foreach (var guid in AssetDatabase.FindAssets("t:CropData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var crop = AssetDatabase.LoadAssetAtPath<CropData>(path);
            if (crop == null || !map.TryGetValue(crop.CropId, out Sprite sprite)) continue;
            var so = new SerializedObject(crop);
            var prop = so.FindProperty("icon");
            if (prop == null) continue;
            prop.objectReferenceValue = sprite;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(crop);
        }
    }

    static void AssignBuildingIcons(List<IconEntry> entries)
    {
        var map = BuildSpriteMap(entries);
        foreach (var guid in AssetDatabase.FindAssets("t:BuildingData"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var bldg = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            if (bldg == null || !map.TryGetValue(bldg.BuildingId, out Sprite sprite)) continue;
            var so = new SerializedObject(bldg);
            var prop = so.FindProperty("icon");
            if (prop == null) continue;
            prop.objectReferenceValue = sprite;
            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(bldg);
        }
    }

    static Dictionary<string, Sprite> BuildSpriteMap(List<IconEntry> entries)
    {
        var map = new Dictionary<string, Sprite>();
        foreach (var e in entries)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(e.assetPath);
            if (sprite != null) map[e.id] = sprite;
        }
        return map;
    }

    // ──────────────────────── TMP Sprite Asset ────────────────────────

    /// <summary>
    /// Packs all icon sprites into a power-of-2 atlas and creates a
    /// TMP_SpriteAsset so icons can be embedded in TMP text:
    ///   <sprite name="carrot">
    /// The asset is saved to Assets/_Project/Art/Icons/Icons_SpriteAsset.asset.
    /// Assign it to the TMP Settings "Default Sprite Asset" or reference it
    /// per-text via the TMP Sprite Asset field.
    /// </summary>
    static void CreateTMPSpriteAsset(List<IconEntry> entries)
    {
        if (entries.Count == 0) return;

        // ── Build atlas ───────────────────────────────────────────────
        int cols    = Mathf.CeilToInt(Mathf.Sqrt(entries.Count));
        int rows    = Mathf.CeilToInt((float)entries.Count / cols);
        int atlasW  = NextPow2(cols * IconSize);
        int atlasH  = NextPow2(rows * IconSize);

        var atlas   = new Texture2D(atlasW, atlasH, TextureFormat.RGBA32, false);
        atlas.SetPixels32(new Color32[atlasW * atlasH]); // clear to transparent

        var positioned = new List<(string id, int x, int y)>();
        int placed = 0;

        for (int i = 0; i < entries.Count; i++)
        {
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(entries[i].assetPath);
            if (sprite == null || sprite.texture == null) continue;

            int col = placed % cols;
            int row = placed / cols;
            int x   = col * IconSize;
            int y   = atlasH - (row + 1) * IconSize; // TMP uses bottom-up origin

            Color[] pixels;
            try   { pixels = sprite.texture.GetPixels(); }
            catch { Debug.LogWarning($"[IconRenderer] {entries[i].id}: texture not readable, skipping atlas."); continue; }

            if (pixels.Length != IconSize * IconSize)
            {
                Debug.LogWarning($"[IconRenderer] {entries[i].id}: unexpected size {sprite.texture.width}x{sprite.texture.height}, skipping.");
                continue;
            }

            atlas.SetPixels(x, y, IconSize, IconSize, pixels);
            positioned.Add((entries[i].id, x, y));
            placed++;
        }

        atlas.Apply();

        // ── Save atlas PNG ────────────────────────────────────────────
        string atlasPath = $"{OutputDir}/Icons_Atlas.png";
        SavePNG(atlas, atlasPath);
        Object.DestroyImmediate(atlas);
        AssetDatabase.Refresh();

        // Import atlas as Default texture (TMP requires non-Sprite type)
        var ti = AssetImporter.GetAtPath(atlasPath) as TextureImporter;
        if (ti != null)
        {
            ti.textureType        = TextureImporterType.Default;
            ti.alphaIsTransparency = true;
            ti.mipmapEnabled      = false;
            ti.isReadable         = false;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.SaveAndReimport();
        }

        var atlasAsset = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
        if (atlasAsset == null)
        {
            Debug.LogError("[IconRenderer] Could not load atlas after import.");
            return;
        }

        // ── TMP Sprite Asset ──────────────────────────────────────────
        var spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
        spriteAsset.name        = "CropBuildingIcons";
        spriteAsset.spriteSheet = atlasAsset;

        spriteAsset.spriteGlyphTable.Clear();
        spriteAsset.spriteCharacterTable.Clear();

        for (int i = 0; i < positioned.Count; i++)
        {
            var (id, x, y) = positioned[i];

            var glyph = new TMP_SpriteGlyph
            {
                index     = (uint)i,
                metrics   = new GlyphMetrics(IconSize, IconSize, 0f, IconSize * 0.8f, IconSize),
                glyphRect = new GlyphRect(x, y, IconSize, IconSize),
                scale     = 1f,
                atlasIndex = 0
            };
            spriteAsset.spriteGlyphTable.Add(glyph);

            var character = new TMP_SpriteCharacter
            {
                name       = id,
                unicode    = (uint)(0xE000 + i),
                glyphIndex = (uint)i,
                scale      = 1f
            };
            spriteAsset.spriteCharacterTable.Add(character);
        }

        spriteAsset.UpdateLookupTables();

        // ── Save asset (embed material) ───────────────────────────────
        string saPath = $"{OutputDir}/Icons_SpriteAsset.asset";
        if (File.Exists(ToFullPath(saPath))) AssetDatabase.DeleteAsset(saPath);
        AssetDatabase.CreateAsset(spriteAsset, saPath);

        // Create and embed material for the TMP sprite shader
        var mat = new Material(Shader.Find("TextMeshPro/Sprite"));
        mat.name        = "Icons_Material";
        mat.mainTexture = atlasAsset;
        AssetDatabase.AddObjectToAsset(mat, saPath);
        spriteAsset.material = mat;
        EditorUtility.SetDirty(spriteAsset);

        AssetDatabase.SaveAssets();
        Debug.Log($"[IconRenderer] TMP Sprite Asset: {positioned.Count} sprites at {saPath}");
    }

    // ─────────────────────────── Utilities ───────────────────────────

    static int NextPow2(int n)
    {
        int p = 1;
        while (p < n) p *= 2;
        return p;
    }
}
