using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TimerMaster
{
    [System.Serializable]
    private class Timer
    {
        public float timestamp;
        public float value;
    }

    [System.Serializable]
    private class Config
    {
        public List<Timer> timers = new List<Timer>();
    }

    static TimerMaster _instance = null;
    Dictionary<string, Config> files = new Dictionary<string, Config>();
    float timestamp = float.MaxValue;

    public static TimerMaster Instance
    {
        get
        {
            if (_instance == null)
                _instance = new TimerMaster();
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

    public static void AddTimestamp(string fileName, float timestamp, float value) {
        Config config = Instance.LoadOrCreateConfig(fileName);
        Timer timer = new Timer();
        timer.timestamp = timestamp;
        timer.value = value;
        config.timers.Add(timer);
    }

    public static float? GetValue(string _fileName, float _timestamp) {
        Config config = Instance.LoadOrCreateConfig(_fileName);
        return null;
    }

    public static void Save(string fileName)
    {
        if (Time.time - Instance.timestamp > 1)
        {
            Instance.timestamp = Time.time;
            Config config = Instance.LoadOrCreateConfig(fileName);
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
            Debug.Log("Saving " + file + ":" + json);
            File.WriteAllText(file, json);
        }
    }

    public static string GetFolder()
    {
        return Application.persistentDataPath + "/";
    }
}