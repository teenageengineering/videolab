using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class ConfigMaster
{
    [System.Serializable]
    private class StringProperty
    {
        public string key;
        public string value;

        public StringProperty(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    private class FloatProperty
    {
        public string key;
        public float value;

        public FloatProperty(string key, float value)
        {
            this.key = key;
            this.value = value;
        }
    }

    [System.Serializable]
    private class Config
    {
        public List<PresetConfig> presets = new List<PresetConfig>();

        public PresetConfig GetOrCreatePresetConfig(float number)
        {
            // Debug.Log("Assert preset config for=" + number);
            foreach (PresetConfig preset in presets)
            {
                if (preset.preset == number)
                {
                    return preset;
                }
            }
            PresetConfig presetConfig = new PresetConfig(number);
            presets.Add(presetConfig);
            return presetConfig;
        }

        public void CleanPresets()
        {
            for (int i = presets.Count - 1; i >= 0; i--)
            {
                if (presets[i].stringProperties.Count == 0 && presets[i].floatProperties.Count == 0)
                    presets.RemoveAt(i);
            }
        }
    }

    [System.Serializable]
    private class PresetConfig
    {
        public float preset;
        public List<StringProperty> stringProperties = new List<StringProperty>();
        public List<FloatProperty> floatProperties = new List<FloatProperty>();

        public PresetConfig(float preset)
        {
            this.preset = preset;
        }

        public StringProperty GetStringProperty(string key)
        {
            foreach (StringProperty stringProperty in stringProperties)
            {
                if (stringProperty.key == key)
                {
                    return stringProperty;
                }
            }
            return null;
        }

        public FloatProperty GetFloatProperty(string key)
        {
            foreach (FloatProperty floatProperty in floatProperties)
            {
                if (floatProperty.key == key)
                {
                    return floatProperty;
                }
            }
            return null;
        }

        public void SetStringProperty(string key, string value)
        {
            StringProperty property = GetStringProperty(key);
            if (property == null)
            {
                stringProperties.Add(new StringProperty(key, value));
            }
            else
            {
                property.value = value;
            }
        }

        public void RemoveStringProperty(string key)
        {
            for (int i = stringProperties.Count - 1; i >= 0; i--)
            {
                if (stringProperties[i].key == key)
                    stringProperties.RemoveAt(i);
            }
        }

        public void SetFloatProperty(string key, float value)
        {
            FloatProperty property = GetFloatProperty(key);
            if (property == null)
            {
                floatProperties.Add(new FloatProperty(key, value));
            }
            else
            {
                property.value = value;
            }
        }

        public void RemoveFloatProperty(string key)
        {
            for (int i = floatProperties.Count - 1; i >= 0; i--)
            {
                if (floatProperties[i].key == key)
                    floatProperties.RemoveAt(i);
            }
        }
    }

    static ConfigMaster _instance = null;

    Dictionary<string, Config> files = new Dictionary<string, Config>();

    HashSet<string> dirty = new HashSet<string>();

    float dirtyTimestamp = 0;

    public static ConfigMaster Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ConfigMaster();
            return _instance;
        }
    }

    private Config LoadConfigFile(string fileName)
    {
        string file = GetFolder() + fileName;
        if (File.Exists(file))
        {
            string json = File.ReadAllText(file);
            return JsonUtility.FromJson<Config>(json);
        }
        return null;
    }

    private Config LoadOrCreateConfig(string fileName)
    {
        if (Instance.files.ContainsKey(fileName))
        {
            return Instance.files[fileName];
        }
        else
        {
            Config config = LoadConfigFile(fileName);
            if (config == null)
            {
                config = new Config();
            }
            Instance.files.Add(fileName, config);
            return config;
        }
    }

    private PresetConfig GetOrCreatePresetConfig(string fileName, float preset)
    {
        Config config = LoadOrCreateConfig(fileName);
        return config.GetOrCreatePresetConfig(preset);
    }

    private void Dirty(string fileName)
    {
        dirty.Add(fileName);
        dirtyTimestamp = Time.time;
    }

    public static string GetStringProperty(string fileName, float preset, string key)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        StringProperty property = presetConfig.GetStringProperty(key);
        if (property == null)
        {
            return null;
        }
        else
        {
            return property.value;
        }
    }

    public static float? GetFloatProperty(string fileName, float preset, string key)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        FloatProperty property = presetConfig.GetFloatProperty(key);
        if (property == null)
        {
            return null;
        }
        else
        {
            return property.value;
        }
    }

    public static void SetStringProperty(string fileName, float preset, string key, string value)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        presetConfig.SetStringProperty(key, value);
        Instance.Dirty(fileName);
    }

    public static void RemoveStringProperty(string fileName, float preset, string key)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        presetConfig.RemoveStringProperty(key);
        Instance.Dirty(fileName);
    }

    public static void SetFloatProperty(string fileName, float preset, string key, float value)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        presetConfig.SetFloatProperty(key, value);
        Instance.Dirty(fileName);
    }

    public static void RemoveFloatProperty(string fileName, float preset, string key)
    {
        PresetConfig presetConfig = Instance.GetOrCreatePresetConfig(fileName, preset);
        presetConfig.RemoveFloatProperty(key);
        Instance.Dirty(fileName);
    }

    public static void Save(string fileName, float timestamp)
    {
        if (Instance.dirty.Contains(fileName) && timestamp - Instance.dirtyTimestamp > 1)
        {
            Instance.dirty.Remove(fileName);
            Config config = Instance.LoadOrCreateConfig(fileName);
            config.CleanPresets();
            string json = JsonUtility.ToJson(config, true);
            // Auto create folder
            int lastIndex = fileName.LastIndexOf('/');
            string folderPath = GetFolder();
            if (lastIndex != -1)
            {
                folderPath += fileName.Substring(0, lastIndex);
            }
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
            fileName = GetFolder() + fileName;
            File.WriteAllText(fileName, json);
        }
    }

    public static void Revert(string fileName, float preset)
    {
        Instance.files.Remove(fileName);
    }

    public static string GetFolder()
    {
#if UNITY_EDITOR
        return Application.dataPath + "/Config/";
#else
        return Application.persistentDataPath + "/";
#endif
    }
}