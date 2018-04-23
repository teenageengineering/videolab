using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;

public class VideopakManager
{
    AssetBundle _bundle;

    static VideopakManager _instance;

    public static VideopakManager Instance {
        get {
            if (_instance == null)
                _instance = new VideopakManager();

            return _instance;
        }
    }

    public static string GetPlatformString(RuntimePlatform platform)
    {
        if (platform == RuntimePlatform.IPhonePlayer)
            return "iOS";

        if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
            return "OSX";

        return null;
    }

    public static void LoadPak(string pakName, bool isRom = false)
    {
        Unload();

        string root = (isRom) ? Application.streamingAssetsPath + "/videopaks/" : Application.persistentDataPath + "/videopaks/";

        string platform = GetPlatformString(Application.platform);
        if (platform == null)
        {
            Debug.Log("[VideopakManager] Unsupported platform");
            return;
        }

        string path = root + pakName + "/" + platform + "/" + pakName;

        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.Log("[VideopakManager] Failed to load videopak " + path);
            return;
        }

        var scenePaths = bundle.GetAllScenePaths();
        if (scenePaths.Length == 0)
        {
            Debug.Log("[VideopakManager] No scene to load");
            return;
        }

        SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Additive);

        Instance._bundle = bundle;
    }

    public static void Unload()
    {
        AssetBundle bundle = Instance._bundle;

        if (bundle != null)
        {
            var scenePaths = bundle.GetAllScenePaths();
            SceneManager.UnloadScene(scenePaths[0]);

            bundle.Unload(true);
        }
    }
}
