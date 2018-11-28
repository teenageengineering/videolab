using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

namespace Videolab
{
    [CustomEditor(typeof(WebcamManager), true)]
    public class WebcamManagerEditor : Editor 
    {
        public override void OnInspectorGUI()
        {
            WebcamManager manager = target as WebcamManager;

            List<string> deviceNames = new List<string>();

            deviceNames.Add("No device");

            foreach (WebCamDevice dev in WebCamTexture.devices)
                deviceNames.Add(dev.name);

            manager.deviceIndex = EditorGUILayout.Popup("Device", manager.deviceIndex + 1, deviceNames.ToArray()) - 1;

            EditorGUILayout.Space();

            manager.material = (Material)EditorGUILayout.ObjectField("Material", manager.material, typeof(Material), true);

            EditorGUILayout.Space();

            manager.rawImage = (RawImage)EditorGUILayout.ObjectField("Raw Image", manager.rawImage, typeof(RawImage), true);
            manager.aspectRatioFitter = (AspectRatioFitter)EditorGUILayout.ObjectField("Aspect Ratio Fitter", manager.aspectRatioFitter, typeof(AspectRatioFitter), true);

            EditorGUILayout.Space();

            manager.targetFrameRate = EditorGUILayout.FloatField("Target Frame Rate", manager.targetFrameRate);
            if (EditorApplication.isPlaying && !serializedObject.isEditingMultipleObjects)
            {
                manager.playing = EditorGUILayout.Toggle("Playing", manager.playing);
            }
            else {
                manager.autoPlay = EditorGUILayout.Toggle("Auto Play", manager.autoPlay);
            }
        }
    }
}
