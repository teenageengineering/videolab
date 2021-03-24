using UnityEngine;
using UnityEditor;
using System.IO;

public class BuildVideopakWindow : EditorWindow
{
    private static VideopakSettings _settings;
    int _selectedAssetBundle = 0;
    string _outputLog = "";
    static string _lastOutputPath = "";

    RuntimePlatform[] platforms = { RuntimePlatform.OSXPlayer, RuntimePlatform.IPhonePlayer, RuntimePlatform.Android };
    BuildTarget[] targets = { BuildTarget.StandaloneOSX, BuildTarget.iOS, BuildTarget.Android };
    string[] platformNames = { "OSX", "iOS", "Android" };

    [MenuItem("Assets/Build Videopaks")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(BuildVideopakWindow));
    }

    public void InitSettings()
    {
        _settings = null;

        var allFound = AssetDatabase.FindAssets("t: VideopakSettings");
        if (allFound.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(allFound[0]);
            _settings = AssetDatabase.LoadAssetAtPath<VideopakSettings>(path);
            //Debug.Log("found object " + path + " " + _settings);
        }

        if (_settings == null)
        {
            _settings = ScriptableObject.CreateInstance<VideopakSettings>();
            _settings.pakName = "videopak";
            _settings.author = "user";
            AssetDatabase.CreateAsset(_settings, AssetDatabase.GenerateUniqueAssetPath("Assets/VideopakSettings.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void BuildAssetBundle(string bundleName)
    {
        string targetFile = EditorUtility.SaveFilePanel("Select output directory", _lastOutputPath, bundleName + ".zpak", "zpak");

        if (string.IsNullOrEmpty(targetFile))
            return;

        _lastOutputPath = Path.GetDirectoryName(targetFile);
        _outputLog = "";

        AssetBundleBuild buildInfo = new AssetBundleBuild();
        buildInfo.assetBundleName = bundleName;
        buildInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
        AssetBundleBuild[] buildMap = { buildInfo };

        PakManifest manifest = new PakManifest(_settings.pakName, _settings.author);
        manifest.version = 1;
        manifest.unityVersion = Application.unityVersion;

        string tmpPath = Path.Combine(FileUtil.GetUniqueTempPathInProject(), bundleName);
        Directory.CreateDirectory(tmpPath);

        if (_settings.icon != null)
        {
            // TODO: verify that it's actually a png
            string iconPath = Path.Combine(Application.dataPath, "../", AssetDatabase.GetAssetPath(_settings.icon));
            File.Copy(iconPath, Path.Combine(tmpPath, "icon.png"));
        }

        string jsonPath = Path.Combine(tmpPath, "videopak.json");
        string json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(jsonPath, json);

        for (int i = 0; i < platforms.Length; i++)
        {
            _outputLog += string.Format("building {0}..\n", platformNames[i]);

            string platformStr = VideopakManager.GetPlatformString(platforms[i]);
            string platformDir = Path.Combine(tmpPath, platformStr);
            Directory.CreateDirectory(platformDir);
            BuildPipeline.BuildAssetBundles(platformDir, buildMap, BuildAssetBundleOptions.None, targets[i]);
        }

        _outputLog += string.Format("compressing..\n");

        VideopakManager.CompressPak(tmpPath, targetFile);

        _outputLog += string.Format("build succeeded\n\n");
    }

    void OnGUI()
    {
        if (_settings == null)
            InitSettings();

        GUILayout.Label("Settings", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        var bundleNames = AssetDatabase.GetAllAssetBundleNames();
        _selectedAssetBundle = EditorGUILayout.Popup("Choose AssetBundle", _selectedAssetBundle, bundleNames);

        EditorGUI.BeginDisabledGroup(bundleNames.Length <= 0);
        _settings.pakName = EditorGUILayout.TextField("Name", _settings.pakName);
        _settings.author = EditorGUILayout.TextField("Author", _settings.author);
        _settings.icon = EditorGUILayout.ObjectField("Icon", _settings.icon, typeof(Texture2D), false) as Texture2D;

        EditorUtility.SetDirty(_settings);

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Build"))
        {
            AssetDatabase.SaveAssets();

            if (_selectedAssetBundle < bundleNames.Length)
            {
                BuildAssetBundle(bundleNames[_selectedAssetBundle]);
                GUIUtility.ExitGUI();
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.Label("Output:", EditorStyles.boldLabel);
        EditorGUILayout.SelectableLabel(_outputLog, EditorStyles.textArea, GUILayout.ExpandHeight(true));

        EditorGUILayout.Space();
    }
}
