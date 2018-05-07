using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraSnapshot : MonoBehaviour {

    public Vector2 frameSize;
    public RawImage rawImage;
    public AspectRatioFitter aspectRatioFitter;

    Camera _camera;
    RenderTexture _renderTex;
    Texture2D _tex;

    void Start()
    {
        _camera = GetComponent<Camera>();
        if (!_camera)
            return;
        
        _renderTex = new RenderTexture((int)frameSize.x, (int)frameSize.y, 24);
        _tex = new Texture2D((int)frameSize.x, (int)frameSize.y, TextureFormat.RGB24, false);
    }

    void Snap()
    {
        if (!_camera)
            return;

        _camera.targetTexture = _renderTex;
        _camera.Render();
        _camera.targetTexture = null;

        RenderTexture prevRt = RenderTexture.active;

        RenderTexture.active = _renderTex;
        _tex.ReadPixels(new Rect(0, 0, _tex.width, _tex.height), 0, 0);
        _tex.Apply();

        if (rawImage)
            rawImage.texture = _tex;

        if (aspectRatioFitter)
            aspectRatioFitter.aspectRatio = (float)_tex.width / _tex.height;

        RenderTexture.active = prevRt;
    }
}
