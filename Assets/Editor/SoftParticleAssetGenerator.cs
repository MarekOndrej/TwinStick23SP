#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

// One-shot utility that generates a soft-circle particle texture (radial alpha
// gradient) plus an additive URP material referencing it. Outputs:
//   Assets/Resources/SoftCircle.png
//   Assets/Resources/Materials/SoftParticleAdditive.mat
// Run via the menu: Tools → Generate Soft Particle Assets.
// Idempotent — safe to re-run; it'll overwrite both files.
public static class SoftParticleAssetGenerator
{
    const string TexturePath = "Assets/Resources/SoftCircle.png";
    const string MaterialPath = "Assets/Resources/Materials/SoftParticleAdditive.mat";

    [MenuItem("Tools/Generate Soft Particle Assets")]
    public static void Generate()
    {
        EnsureDirectory("Assets/Resources");
        EnsureDirectory("Assets/Resources/Materials");

        // --- 1) Generate the texture ---
        const int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, mipChain: false);
        float center = (size - 1) * 0.5f;
        float maxDist = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                // Linear distance → squared falloff for a soft glow.
                float t = Mathf.Clamp01(1f - dist / maxDist);
                float alpha = t * t;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();

        File.WriteAllBytes(TexturePath, tex.EncodeToPNG());
        Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(TexturePath, ImportAssetOptions.ForceUpdate);

        // Texture import settings.
        var importer = (TextureImporter)AssetImporter.GetAtPath(TexturePath);
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
        }

        // --- 2) Generate the additive URP particle material ---
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
        {
            Debug.LogError("URP Particle shader not found. Is URP installed?");
            return;
        }

        var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
        if (mat == null)
        {
            mat = new Material(shader);
            AssetDatabase.CreateAsset(mat, MaterialPath);
        }
        else
        {
            mat.shader = shader;
        }

        var loadedTex = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (loadedTex != null) mat.SetTexture("_BaseMap", loadedTex);

        mat.SetColor("_BaseColor", Color.white);
        // URP/Particles/Unlit supports a Surface Type (Opaque=0 / Transparent=1)
        // and Blend Mode (Alpha=0, Premultiply=1, Additive=2, Multiply=3).
        mat.SetFloat("_Surface", 1f);   // Transparent
        mat.SetFloat("_Blend", 2f);     // Additive
        mat.SetFloat("_ZWrite", 0f);    // don't write depth for additive
        mat.SetFloat("_AlphaClip", 0f);
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        // Make sure transparent + additive keywords are enabled (shader-feature based).
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.EnableKeyword("_ALPHAPREMULTIPLY_OFF");
        mat.DisableKeyword("_ALPHATEST_ON");

        // Additive blend: SrcBlend=One, DstBlend=One.
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.One);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);

        EditorUtility.SetDirty(mat);
        AssetDatabase.SaveAssetIfDirty(mat);
        AssetDatabase.Refresh();

        Debug.Log($"Soft particle assets generated:\n  {TexturePath}\n  {MaterialPath}");
    }

    static void EnsureDirectory(string path)
    {
        if (Directory.Exists(path)) return;
        Directory.CreateDirectory(path);
    }
}
#endif
