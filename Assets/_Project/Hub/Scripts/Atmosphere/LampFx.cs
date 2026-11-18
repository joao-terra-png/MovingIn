using UnityEngine;

namespace MovingIn.Hub{
    public class LampFx : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] AtmosphereMode mode = AtmosphereMode.RoomLamps;
        [SerializeField] Light lamp;
        [SerializeField] float intensity = 1.2f;
        [SerializeField] Renderer[] glows;
        [SerializeField] Color glowColor = new Color(1f, 0.8f, 0.5f, 0.8f);
        [SerializeField, Range(0f, 1f)] float flicker;

        MaterialPropertyBlock _block;
        float _weight;
        float _seed;

        void OnEnable()
        {
            _seed = Random.value * 100f;
            _weight = Atmosphere.Weight(mode);
            Apply();
        }

        void Update()
        {
            _weight = Mathf.MoveTowards(_weight, Atmosphere.Weight(mode), Time.deltaTime * 1.5f);
            Apply();
        }

        void Apply()
        {
            float f = flicker > 0f ? 1f - flicker * Mathf.Pow(Mathf.PerlinNoise(Time.time * 6f, _seed), 3f) : 1f;
            float w = _weight * f;
            bool on = _weight > 0.01f;
            if (lamp != null)
            {
                lamp.intensity = intensity * w;
                lamp.enabled = on;
            }
            if (glows == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            Color c = glowColor;
            c.a *= w;
            _block.SetColor(ColorId, c);
            foreach (Renderer r in glows)
            {
                if (r == null) continue;
                r.enabled = on;
                r.SetPropertyBlock(_block);
            }
        }
    }
}
