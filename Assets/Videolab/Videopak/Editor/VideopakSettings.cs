using System.Collections.Generic;
using UnityEngine;

public class VideopakSettings : ScriptableObject
{
    [System.Serializable]
    public class BundleConfig
    {
        public string bundleName;
        public string pakName;
        public string author;
        public Texture2D icon;
    }

    [HideInInspector]
    public List<BundleConfig> configs;
}
