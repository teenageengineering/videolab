using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildVideopaks
{
    [MenuItem ("Assets/Build Videopaks")]
    static void BuildAllVideopaks()
    {
        string rootDir = EditorUtility.SaveFolderPanel("Output directory", "", "");

        if (string.IsNullOrEmpty(rootDir))
            return;
        
        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetBundleBuild buildInfo = new AssetBundleBuild();
            buildInfo.assetBundleName = bundleName;
            buildInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            AssetBundleBuild[] buildMap = new AssetBundleBuild[] { buildInfo };

            string pakDir = rootDir + "/videopaks/" + bundleName + "/";

            string iosDir = pakDir + VideopakManager.GetPlatformString(RuntimePlatform.IPhonePlayer);
            Directory.CreateDirectory(iosDir);
            BuildPipeline.BuildAssetBundles(iosDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);

            string pcDir = pakDir + "PC";
            Directory.CreateDirectory(pcDir);
            BuildTarget target = BuildTarget.StandaloneWindows64;
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                #if UNITY_2017_3_OR_NEWER
                target =  BuildTarget.StandaloneOSX;
                #else
                target =  BuildTarget.StandaloneOSXUniversal;
                #endif
            }

            BuildPipeline.BuildAssetBundles(pcDir, buildMap, BuildAssetBundleOptions.None, target);
        }
    }

    [MenuItem ("Assets/Load Videopak")]
    static void LoadVideopak()
    {
        string pakDir = EditorUtility.OpenFolderPanel("Select videopak", "", "");

        if (string.IsNullOrEmpty(pakDir))
            return;

        VideopakManager.LoadPak(pakDir);
    }
}