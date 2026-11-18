using UnityEngine;

namespace MovingIn.Hub{
    public class EmergencyLight : MonoBehaviour
    {
        [SerializeField] Light lamp;
        [SerializeField] Renderer marker;
        [SerializeField] float _speed = 3f;
        [SerializeField] float _minFactor = 0.15f;

        float _base;

        void Awake()
        {
            if (lamp == null) lamp = GetComponentInChildren<Light>();
            _base = lamp != null ? lamp.intensity : 1f;
        }

        void Update()
        {
            float wave = Mathf.Max(0f, Mathf.Sin(Time.time * _speed * Mathf.PI));
            float k = Mathf.Lerp(_minFactor, 1f, wave);
            if (lamp != null) lamp.intensity = _base * k;
            if (marker != null) marker.enabled = k > 0.5f;
        }
    }
}
