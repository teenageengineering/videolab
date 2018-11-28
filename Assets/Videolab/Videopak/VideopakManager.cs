using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Collections;

public class VideopakManager : MonoBehaviour
{
    AssetBundle _bundle;
    string _path;

    static VideopakManager _instance;

    public static VideopakManager Instance {
        get {
            if (_instance == null)
            {
                GameObject go = new GameObject("Videopak Manager");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<VideopakManager>();
            }

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

    public static IEnumerator LoadPak(string pakRoot)
    {
        if (pakRoot == Instance._path)
            yield break;
        
        yield return Instance.StartCoroutine(Unload());

        string platform = GetPlatformString(Application.platform);
        if (platform == null)
        {
            Debug.Log("[VideopakManager] Unsupported platform");
            yield break;
        }

        string pakName = Path.GetFileName(pakRoot);
        string path = pakRoot + "/" + platform + "/" + pakName;
        if (!File.Exists(path))
        {
            Debug.Log("[VideopakManager] Missing platform payload " + path);
            yield break;
        }

        AssetBundle bundle = AssetBundle.LoadFromFile(path);
        if (bundle == null)
        {
            Debug.Log("[VideopakManager] Failed to load videopak " + path);
            yield break;
        }

        var scenePaths = bundle.GetAllScenePaths();
        if (scenePaths.Length == 0)
        {
            Debug.Log("[VideopakManager] No scene to load");
            yield break;
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePaths[0], LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
            yield return null;

        Instance._bundle = bundle;
        Instance._path = pakRoot;
    }

    public static IEnumerator Unload()
    {
        AssetBundle bundle = Instance._bundle;

        if (bundle != null)
        {
            var scenePaths = bundle.GetAllScenePaths();
            yield return SceneManager.UnloadSceneAsync(scenePaths[0]);

            bundle.Unload(true);

            Instance._bundle = null;
            Instance._path = null;
        }
    }

    public static bool IsLoaded()
    {
        return Instance._bundle;
    }
}
