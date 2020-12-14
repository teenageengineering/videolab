using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class AsciiArtFx : MonoBehaviour
{
    public Color colorTint = Color.white;

    [SerializeField, Range(0, 1)]
    float _blendRatio = 1.0f;
    public float blendRatio {
        get { return _blendRatio; }
        set { _blendRatio = value; }
    }

    [SerializeField, Range(0.5f, 10.0f)]
    float _scaleFactor = 1.0f;
    public float scaleFactor {
        get { return _scaleFactor; }
        set { _scaleFactor = value; }
    }

    [SerializeField] Shader shader;

    private Material _material;
    
    Material material {
        get {
            if (_material == null)
            {
                _material = new Material(shader);
                _material.hideFlags = HideFlags.HideAndDontSave;   
            }
            return _material;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        material.color = colorTint;
        material.SetFloat("_Alpha", blendRatio);
        material.SetFloat("_Scale", scaleFactor);
        Graphics.Blit(source, destination, material);
    }
    
    void OnDisable ()
    {
        if (_material) DestroyImmediate(_material);   
    }
}
