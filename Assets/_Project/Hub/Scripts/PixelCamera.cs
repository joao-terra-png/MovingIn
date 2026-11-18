using UnityEngine;

namespace MovingIn.Hub{
    [RequireComponent(typeof(Camera))]
    public class PixelCamera : MonoBehaviour
    {
        [SerializeField] bool _pixelate = true;
        [SerializeField] int _height = 270;

        public bool Pixelate
        {
            get => _pixelate;
            set => _pixelate = value;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (!_pixelate || _height <= 0 || src.height <= _height)
            {
                Graphics.Blit(src, dst);
                return;
            }

            src.filterMode = FilterMode.Point;
            int w = Mathf.RoundToInt(_height * (float)src.width / src.height);
            RenderTexture low = RenderTexture.GetTemporary(w, _height, 0, src.format);
            low.filterMode = FilterMode.Point;
            Graphics.Blit(src, low);
            Graphics.Blit(low, dst);
            RenderTexture.ReleaseTemporary(low);
        }
    }
}
