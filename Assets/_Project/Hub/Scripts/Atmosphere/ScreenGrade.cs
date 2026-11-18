using UnityEngine;

namespace MovingIn.Hub{
    [RequireComponent(typeof(Camera))]
    public class ScreenGrade : MonoBehaviour
    {
        static readonly int VignetteId = Shader.PropertyToID("_Vignette");
        static readonly int SaturationId = Shader.PropertyToID("_Saturation");
        static readonly int ContrastId = Shader.PropertyToID("_Contrast");
        static readonly int TintId = Shader.PropertyToID("_Tint");

        [SerializeField] Shader shader;
        [SerializeField, Range(0f, 1f)] float vignette = 0.28f;
        [SerializeField, Range(0f, 1f)] float blackoutVignette = 0.35f;
        [SerializeField] Color dayTint = new Color(1f, 0.97f, 0.92f);
        [SerializeField] Color nightTint = new Color(0.82f, 0.86f, 1f);
        [SerializeField, Range(0.5f, 1.5f)] float contrast = 1.05f;

        Material _mat;
        float _sun = 1f, _dark, _black;

        void OnDisable()
        {
            if (_mat != null) Destroy(_mat);
            _mat = null;
        }

        void OnRenderImage(RenderTexture src, RenderTexture dst)
        {
            if (shader == null)
            {
                Graphics.Blit(src, dst);
                return;
            }
            if (_mat == null) _mat = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };

            float k = Application.isPlaying ? Mathf.Clamp01(Time.unscaledDeltaTime * 1.5f) : 1f;
            _sun = Mathf.Lerp(_sun, Atmosphere.Weight(AtmosphereMode.Sunlit), k);
            _dark = Mathf.Lerp(_dark, Atmosphere.Weight(AtmosphereMode.Dark), k);
            _black = Mathf.Lerp(_black, Atmosphere.Weight(AtmosphereMode.Blackout), k);

            Color tint = Color.Lerp(Color.Lerp(Color.white, dayTint, _sun), nightTint, _dark);
            _mat.SetColor(TintId, tint);
            _mat.SetFloat(SaturationId, 1f + 0.08f * _sun - 0.2f * _dark);
            _mat.SetFloat(ContrastId, contrast);
            _mat.SetFloat(VignetteId, Mathf.Lerp(vignette, blackoutVignette, _black));
            Graphics.Blit(src, dst, _mat);
        }
    }
}
