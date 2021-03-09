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
            if (material != null)
                Graphics.Blit(source, destination, material);
        }
    }
}
