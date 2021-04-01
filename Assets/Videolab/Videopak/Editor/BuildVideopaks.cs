using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

public class BuildVideopakWindow : EditorWindow
{
    private static VideopakSettings _settings;
    int _selectedAssetBundle = 0;
    string _outputLog = "";
    Vector2 _logScrollPosition;
    static string _lastOutputPath = "";
    static string _lastOutputFolder = "";

    RuntimePlatform[] platforms = { RuntimePlatform.IPhonePlayer, RuntimePlatform.Android, RuntimePlatform.OSXPlayer };
    BuildTarget[] targets = { BuildTarget.iOS, BuildTarget.Android, BuildTarget.StandaloneOSX };
    string[] platformNames = { "iOS", "Android", "OSX" };

    [MenuItem("Assets/Build Videopaks")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(BuildVideopakWindow), true, "Build Videopaks");
    }

    public void InitSettings()
    {
        _settings = null;

        var allFound = AssetDatabase.FindAssets("t: VideopakSettings");
        if (allFound.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(allFound[0]);
            _settings = AssetDatabase.LoadAssetAtPath<VideopakSettings>(path);
        }

        if (_settings == null)
        {
            _settings = ScriptableObject.CreateInstance<VideopakSettings>();
            _settings.configs = new List<VideopakSettings.BundleConfig>();
            AssetDatabase.CreateAsset(_settings, AssetDatabase.GenerateUniqueAssetPath("Assets/VideopakSettings.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    void BuildAssetBundle(VideopakSettings.BundleConfig config, string targetFile)
    {
        if (string.IsNullOrEmpty(targetFile))
            return;

        _lastOutputPath = Path.GetDirectoryName(targetFile);

        AssetBundleBuild buildInfo = new AssetBundleBuild();
        buildInfo.assetBundleName = config.bundleName;
        buildInfo.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(config.bundleName);
        AssetBundleBuild[] buildMap = { buildInfo };

        PakManifest manifest = new PakManifest(config.pakName, config.author);
        manifest.version = 1;
        manifest.unityVersion = Application.unityVersion;

        string tmpPath = Path.Combine(FileUtil.GetUniqueTempPathInProject(), config.bundleName);
        Directory.CreateDirectory(tmpPath);

        if (config.icon != null)
        {
            string iconPath = Path.Combine(Application.dataPath, "../", AssetDatabase.GetAssetPath(config.icon));
            File.Copy(iconPath, Path.Combine(tmpPath, "icon.png"));
        }

        string jsonPath = Path.Combine(tmpPath, "videopak.json");
        string json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(jsonPath, json);

        for (int i = 0; i < platforms.Length; i++)
        {
            AddLogText(string.Format("building {0}..\n", platformNames[i]));

            string platformStr = VideopakManager.GetPlatformString(platforms[i]);
            string platformDir = Path.Combine(tmpPath, platformStr);
            Directory.CreateDirectory(platformDir);
            BuildPipeline.BuildAssetBundles(platformDir, buildMap, BuildAssetBundleOptions.None, targets[i]);
        }

        AddLogText(string.Format("compressing..\n"));

        VideopakManager.CompressPak(tmpPath, targetFile);

        AddLogText(string.Format("build succeeded\n\n"));
    }

    private bool ValidatePakName(string pakName)
    {
        if (string.IsNullOrEmpty(pakName))
            return false;

        return true;
    }

    private bool ValidateIcon(Texture2D icon)
    {
        if (icon == null)
            return true;

        string ext = Path.GetExtension(AssetDatabase.GetAssetPath(icon));

        return (ext.ToLower() == ".png");
    }

    bool ValidateConfig(VideopakSettings.BundleConfig config)
    {
        if (!ValidatePakName(config.pakName))
        {
            _outputLog = "Invalid pak name. ";
            return false;
        }
        else if (!ValidateIcon(config.icon))
        {
            _outputLog = "Icon is not a png file. ";
            return false;
        }

        return true;
    }

    VideopakSettings.BundleConfig GetBundleConfig(string bundleName)
    {
        if (string.IsNullOrEmpty(bundleName))
            return null;

        foreach (var conf in _settings.configs)
        {
            if (conf.bundleName == bundleName)
                return conf;
        }

        var config = new VideopakSettings.BundleConfig();
        config.bundleName = bundleName;
        config.pakName = bundleName;
        config.author = "user";

        _settings.configs.Add(config);

        return config;
    }

    void CleanUpConfigs(string[] bundleNames)
    {
        // remove any configs that has and bundleName that doesn't exist
        foreach (var conf in _settings.configs)
        {
            if (!System.Array.Exists(bundleNames, n => n == conf.bundleName))
                conf.bundleName = "";
        }
        _settings.configs.RemoveAll(c => string.IsNullOrEmpty(c.bundleName));
    }

    void AddLogText(string text)
    {
        _outputLog += text;
        float height = CalculateLogHeight();
        if (height > 200f) //TODO: clean way of only scrolling if needed
            _logScrollPosition = new Vector2(0, height);
    }

    float CalculateLogHeight()
    {
        GUIStyle g = new GUIStyle();
        return g.CalcHeight(new GUIContent(_outputLog), 1000f);
    }

    void ClearLogText()
    {
        _outputLog = "";
    }

    void OnGUI()
    {
        if (_settings == null)
            InitSettings();

        GUILayout.Label("Config", EditorStyles.boldLabel);

        EditorGUILayout.Space();

        var bundleNames = AssetDatabase.GetAllAssetBundleNames();

        CleanUpConfigs(bundleNames);

        _selectedAssetBundle = EditorGUILayout.Popup("Choose AssetBundle", _selectedAssetBundle, bundleNames);

        string selectedBundleName = (bundleNames.Length > 0) ? bundleNames[_selectedAssetBundle] : "";
        VideopakSettings.BundleConfig config = GetBundleConfig(selectedBundleName);

        if (bundleNames.Length > 0)
        {
            config.pakName = EditorGUILayout.TextField("Name", config.pakName);
            config.author = EditorGUILayout.TextField("Author", config.author);
            config.icon = EditorGUILayout.ObjectField("Icon", config.icon, typeof(Texture2D), false) as Texture2D;

            EditorUtility.SetDirty(_settings);
        }

        EditorGUILayout.Space();
        GUILayout.Label("Build", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(string.Format("Build ({0})", selectedBundleName)))
        {
            ClearLogText();

            if (config == null)
            {
                AddLogText("No AssetBundle selected. Aborting..\n");
            }
            else if (!ValidateConfig(config))
            {
                AddLogText("Aborting..\n");
            }
            else
            {
                AddLogText(string.Format("[build {0}]\n", selectedBundleName));
                string targetFile = EditorUtility.SaveFilePanel("Select output file", _lastOutputPath, selectedBundleName + ".zpak", "zpak");
                BuildAssetBundle(config, targetFile);
                GUIUtility.ExitGUI();
            }
        }

        EditorGUI.BeginDisabledGroup(bundleNames.Length < 2);

        if (GUILayout.Button(string.Format("Build All ({0})", bundleNames.Length), GUILayout.Width(120)))
        {
            ClearLogText();

            string targetFolder = EditorUtility.SaveFolderPanel("Select output file", _lastOutputFolder, "");
            if (targetFolder.Length > 0)
            {
                _lastOutputFolder = targetFolder;

                foreach (var bundleName in bundleNames)
                {
                    VideopakSettings.BundleConfig conf = GetBundleConfig(bundleName);

                    AddLogText(string.Format("Preparing to build {0}..\n", bundleName));

                    if (conf == null)
                    {
                        AddLogText("No config for bundle. Skipping..\n");
                        continue;
                    }

                    if (!ValidateConfig(conf))
                    {
                        AddLogText("Skipping..\n");
                        continue;
                    }

                    string targetFile = Path.Combine(targetFolder, bundleName + ".zpak");
                    BuildAssetBundle(conf, targetFile);
                }

                GUIUtility.ExitGUI();
            }
        }

        EditorGUI.EndDisabledGroup();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        using (var scrollView = new EditorGUILayout.ScrollViewScope(_logScrollPosition, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
        {
            _logScrollPosition = scrollView.scrollPosition;
            //TODO: Set MinHeight to the equivalent of GUILayout.ExpandHeight(true). Videolab community, help me out with this one :D
            EditorGUILayout.SelectableLabel(_outputLog, EditorStyles.textArea, GUILayout.Height(CalculateLogHeight()), GUILayout.MinHeight(600f));
        }

        EditorGUILayout.Space();
    }
}
