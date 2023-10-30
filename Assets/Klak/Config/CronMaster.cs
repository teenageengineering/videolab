using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CronMaster
{
    [System.Serializable]
    public class CronEvent
    {
        public float timestamp;
        public float value;
    }

    [System.Serializable]
    private class Config
    {
        [SerializeField]
        public List<CronEvent> events = new List<CronEvent>();

        public void AddEvent(float timestamp, float value)
        {
            CronEvent _event = new CronEvent();
            _event.timestamp = timestamp;
            _event.value = value;
            events.Add(_event);
        }
    }

    static CronMaster _instance = null;

    Dictionary<string, Config> files = new Dictionary<string, Config>();

    HashSet<string> dirty = new HashSet<string>();

    float dirtyTimestamp = 0;

    public static CronMaster Instance
    {
        get
        {
            if (_instance == null)
                _instance = new CronMaster();
            return _instance;
        }
    }

    private Config LoadConfigFile(string fileName)
    {
        string file = FileMaster.GetFolder() + fileName;
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

    private void Dirty(string fileName)
    {
        dirty.Add(fileName);
        dirtyTimestamp = Time.time;
    }

    public static void AddEvent(string fileName, float timestamp, float value)
    {
        Config config = Instance.LoadOrCreateConfig(fileName);
        config.AddEvent(timestamp, value);
        Instance.Dirty(fileName);
    }

    public static List<CronEvent> GetEvents(string fileName)
    {
        Config config = Instance.LoadOrCreateConfig(fileName);
        return config.events;
    }

    public static void Save(string fileName, float timestamp)
    {
        if (Instance.dirty.Contains(fileName) && timestamp - Instance.dirtyTimestamp > 1)
        {
            Instance.dirty.Remove(fileName);
            Config config = Instance.LoadOrCreateConfig(fileName);
            string json = JsonUtility.ToJson(config, true);
            FileMaster.AssureFolderExists(fileName);
            File.WriteAllText(FileMaster.GetFolder() + fileName, json);
        }
    }

    public static void DeleteFile(string fileName)
    {
        Instance.files.Remove(fileName);
        File.Delete(FileMaster.GetFolder() + fileName);
    }
}