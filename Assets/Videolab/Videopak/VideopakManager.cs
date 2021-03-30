using UnityEngine;
using System.IO;
using System.IO.Compression;

public class PakManifest
{
    public PakManifest(string _name, string _author) { name = _name; author = _author; }
    public string name;
    public string token;
    public string author;
    public string unityVersion;
    public int version;
    public bool encryption;
}

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

        if (platform == RuntimePlatform.Android)
            return "Android";

        if (platform == RuntimePlatform.OSXEditor || platform == RuntimePlatform.OSXPlayer)
            return "OSX";

        return null;
    }

    public static void CompressPak(string dir, string zipPath)
    {
        if (File.Exists(zipPath))
            File.Delete(zipPath);
        ZipFile.CreateFromDirectory(dir, zipPath, System.IO.Compression.CompressionLevel.Fastest, true);
        Debug.Log("Compress " + dir + " => " + zipPath);
    }

    public static void ExtractPak(string zipPath, string dir)
    {
        if (Directory.Exists(dir))
            Directory.Delete(dir, true);
        Directory.CreateDirectory(dir);

        ZipFile.ExtractToDirectory(zipPath, dir);

        Debug.Log("Extract " + zipPath + " => " + dir);
    }

    public static PakManifest ReadManifest(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return new PakManifest("unknown", "unknown");
        string json = File.ReadAllText(jsonPath);
        return JsonUtility.FromJson<PakManifest>(json);
    }

    public static void WriteManifest(PakManifest manifest, string jsonPath)
    {
        string json = JsonUtility.ToJson(manifest, true);
        File.WriteAllText(jsonPath, json);
    }

    public static string FindPakFolder(string rootFolder)
    {
        var directories = Directory.GetDirectories(rootFolder);
        foreach (var dir in directories)
        {
            if (Path.GetFileName(dir).StartsWith("__", System.StringComparison.InvariantCultureIgnoreCase))
                continue;
            return dir;
        }
        return "";
    }
}
