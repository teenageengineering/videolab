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
        public List<ProjectConfig> projects = new List<ProjectConfig>();

        public ProjectConfig GetOrCreateProjectConfig(float number)
        {
            // Debug.Log("Assert project config for=" + number);
            foreach (ProjectConfig project in projects)
            {
                if (project.project == number)
                {
                    return project;
                }
            }
            ProjectConfig projectConfig = new ProjectConfig(number);
            projects.Add(projectConfig);
            return projectConfig;
        }

        public void CleanProjects()
        {
            for (int i = projects.Count - 1; i >= 0; i--)
            {
                if (projects[i].stringProperties.Count == 0 && projects[i].floatProperties.Count == 0)
                    projects.RemoveAt(i);
            }
        }
    }

    [System.Serializable]
    private class ProjectConfig
    {
        public float project;
        public List<StringProperty> stringProperties = new List<StringProperty>();
        public List<FloatProperty> floatProperties = new List<FloatProperty>();

        public ProjectConfig(float project)
        {
            this.project = project;
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

    // Workaround for knob issue in KnobIn
    Dictionary<int, Dictionary<int, float>> knobLevels = new Dictionary<int, Dictionary<int, float>>();

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

    private ProjectConfig GetOrCreateProjectConfig(string fileName, float project)
    {
        Config config = LoadOrCreateConfig(fileName);
        return config.GetOrCreateProjectConfig(project);
    }

    private void Dirty(string fileName)
    {
        dirty.Add(fileName);
        dirtyTimestamp = Time.time;
    }

    public static void SetKnobValue(int channel, int knobNumber, float level)
    {
        Dictionary<int, float> levels;
        if (Instance.knobLevels.ContainsKey(channel))
        {
            levels = Instance.knobLevels[channel];
        }
        else
        {
            levels = new Dictionary<int, float>();
            Instance.knobLevels.Add(channel, levels);
        }
        levels[knobNumber] = level;
    }

    public static float GetKnobValue(int channel, int knobNumber)
    {
        Dictionary<int, float> levels;
        if (Instance.knobLevels.TryGetValue(channel, out levels))
        {
            float level;
            if (levels.TryGetValue(knobNumber, out level))
            {
                return level;
            }
        }
        return 0;
    }

    public static string GetStringProperty(string fileName, float project, string key)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        StringProperty property = projectConfig.GetStringProperty(key);
        if (property == null)
        {
            return null;
        }
        else
        {
            return property.value;
        }
    }

    public static float? GetFloatProperty(string fileName, float project, string key)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        FloatProperty property = projectConfig.GetFloatProperty(key);
        if (property == null)
        {
            return null;
        }
        else
        {
            return property.value;
        }
    }

    public static void SetStringProperty(string fileName, float project, string key, string value)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        projectConfig.SetStringProperty(key, value);
        Instance.Dirty(fileName);
    }

    public static void RemoveStringProperty(string fileName, float project, string key)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        projectConfig.RemoveStringProperty(key);
        Instance.Dirty(fileName);
    }

    public static void SetFloatProperty(string fileName, float project, string key, float value)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        projectConfig.SetFloatProperty(key, value);
        Instance.Dirty(fileName);
    }

    public static void RemoveFloatProperty(string fileName, float project, string key)
    {
        ProjectConfig projectConfig = Instance.GetOrCreateProjectConfig(fileName, project);
        projectConfig.RemoveFloatProperty(key);
        Instance.Dirty(fileName);
    }

    public static void Save(string fileName, float timestamp)
    {
        if (Instance.dirty.Contains(fileName) && timestamp - Instance.dirtyTimestamp > 1)
        {
            Instance.dirty.Remove(fileName);
            Config config = Instance.LoadOrCreateConfig(fileName);
            config.CleanProjects();
            string json = JsonUtility.ToJson(config, true);
            // Auto create folder
            int lastIndex = fileName.LastIndexOf('/');
            if (lastIndex != -1)
            {
                string folderPath = GetFolder() + fileName.Substring(0, lastIndex);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }
            }
            string file = GetFolder() + fileName;
            // Debug.Log("Saving " + file + ":" + json);
            File.WriteAllText(file, json);
        }
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