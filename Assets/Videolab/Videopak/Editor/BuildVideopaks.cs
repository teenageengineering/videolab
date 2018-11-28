using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildVideopaks
{
    [MenuItem ("Assets/Build Videopaks")]
    static void BuildAll()
    {
        string rootDir = EditorUtility.SaveFolderPanel("Select output directory", "", "");

        if (string.IsNullOrEmpty(rootDir))
            return;

        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetBundleBuild buildInfo = new AssetBundleBuild();
            buildInfo.assetBundleName = bundleName;
            buildInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            AssetBundleBuild[] buildMap = new AssetBundleBuild[] { buildInfo };

            string pakDir = rootDir + "/" + bundleName + "/";

            string iosDir = pakDir + VideopakManager.GetPlatformString(RuntimePlatform.IPhonePlayer);
            Directory.CreateDirectory(iosDir);
            BuildPipeline.BuildAssetBundles(iosDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);

            string osxDir = pakDir + VideopakManager.GetPlatformString(RuntimePlatform.OSXPlayer);
            Directory.CreateDirectory(osxDir);
            #if UNITY_2017_3_OR_NEWER
            BuildPipeline.BuildAssetBundles(osxDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSX);
            #else
            BuildPipeline.BuildAssetBundles(osxDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSXUniversal);
            #endif
        }
    }
}
