using UnityEngine;

namespace Videolab
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(Camera))]
    public class ImageEffect : MonoBehaviour
    {
        public Material material;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            Graphics.Blit(source, destination, material);
        }
    }
}
