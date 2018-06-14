using UnityEngine;
using System.IO;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#else
using UnityEngine.SceneManagement;
#endif

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

        if (platform == RuntimePlatform.OSXEditor || 
            platform == RuntimePlatform.OSXPlayer ||
            platform == RuntimePlatform.WindowsEditor || 
            platform == RuntimePlatform.WindowsPlayer)
            return "PC";

        return null;
    }

    public static void LoadPak(string pakRoot)
    {
        Unload();

        string platform = GetPlatformString(Application.platform);
        if (platform == null)
        {
            Debug.Log("[VideopakManager] Unsupported platform");
            return;
        }

        string pakName = Path.GetFileName(pakRoot);
        string path = pakRoot + "/" + platform + "/" + pakName;

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

        #if UNITY_EDITOR
        EditorSceneManager.OpenScene(scenePaths[0], OpenSceneMode.Additive);
        #else
        SceneManager.LoadScene(scenePaths[0], LoadSceneMode.Additive);
        #endif

        Instance._bundle = bundle;
    }

    public static void Unload()
    {
        AssetBundle bundle = Instance._bundle;

        if (bundle != null)
        {
            var scenePaths = bundle.GetAllScenePaths();

            #if UNITY_EDITOR
            EditorSceneManager.CloseScene(EditorSceneManager.GetSceneByName(scenePaths[0]), true);
            #else
            SceneManager.UnloadScene(scenePaths[0]);
            #endif

            bundle.Unload(true);
        }
    }
}
