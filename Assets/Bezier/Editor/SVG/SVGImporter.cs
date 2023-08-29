using UnityEngine;
using UnityEditor;
using System.IO;
using Bezier;

#if UNITY_2018_1_OR_NEWER



[UnityEditor.AssetImporters.ScriptedImporter(1, "svg")]
class SVGImporter : UnityEditor.AssetImporters.ScriptedImporter
{
    [SerializeField]
    bool _stripGroups = false;

    public override void OnImportAsset(UnityEditor.AssetImporters.AssetImportContext ctx)
    {
        string svgStr = null;
        using (StreamReader reader = new StreamReader(ctx.assetPath))
            svgStr = reader.ReadToEnd();

        SVGParser svgParser = new SVGParser();
        GameObject svgObj = svgParser.Parse(svgStr, _stripGroups);

        ctx.AddObjectToAsset("SVG root", svgObj);
        ctx.SetMainObject(svgObj);
    }
}

#else

class SVGImporter : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        if (importedAssets.Length == 0)
            return;

        if (Path.GetExtension(importedAssets[0]) != ".svg")
            return;

        string svgStr = null;
        using (StreamReader reader = new StreamReader(importedAssets[0]))
            svgStr = reader.ReadToEnd();

        SVGParser svgParser = new SVGParser();
        GameObject svgObj = svgParser.Parse(svgStr);

        if (svgObj != null) 
        {
            string prefabPath = Path.GetDirectoryName(importedAssets[0]) + "/" 
                + Path.GetFileNameWithoutExtension(importedAssets[0])
                + ".prefab";
            PrefabUtility.CreatePrefab(prefabPath, svgObj, ReplacePrefabOptions.ReplaceNameBased);
            GameObject.DestroyImmediate(svgObj);
        }
    }
}

#endif // UNITY_2018_1_OR_NEWER
