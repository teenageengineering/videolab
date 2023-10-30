using System.IO;
using UnityEngine;

public class FileMaster
{
    public static void AssureFolderExists(string fileName)
    {
        // Auto create folder
        int lastIndex = fileName.LastIndexOf('/');
        string folderPath = FileMaster.GetFolder();
        if (lastIndex != -1)
        {
            folderPath += fileName.Substring(0, lastIndex);
        }
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
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