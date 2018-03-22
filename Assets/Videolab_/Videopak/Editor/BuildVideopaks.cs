using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildVideopaks
{
    [MenuItem ("Assets/Build Videopaks")]
    static void BuildAllAssetBundles()
    {
        string dir = "videopaks/";
        
        foreach (string bundleName in AssetDatabase.GetAllAssetBundleNames())
        {
            AssetBundleBuild buildInfo = new AssetBundleBuild();
            buildInfo.assetBundleName = bundleName;
            buildInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
            AssetBundleBuild[] buildMap = new AssetBundleBuild[] { buildInfo };

            string osxDir = dir + bundleName + "/" + VideopakManager.GetPlatformString(RuntimePlatform.OSXPlayer);
            Directory.CreateDirectory(osxDir);
            BuildPipeline.BuildAssetBundles(osxDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneOSXUniversal);

            string iosDir = dir + bundleName + "/" + VideopakManager.GetPlatformString(RuntimePlatform.IPhonePlayer);
            Directory.CreateDirectory(iosDir);
            BuildPipeline.BuildAssetBundles(iosDir, buildMap, BuildAssetBundleOptions.None, BuildTarget.iOS);
        }
    }
}