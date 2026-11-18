using UnityEngine;

namespace MovingIn.Hub{
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusic : MonoBehaviour
    {
        [SerializeField] AudioClip music;
        [SerializeField, Range(0f, 1f)] float _volume = 0.5f;
        [SerializeField] float _fadeIn = 2f;

        AudioSource _source;

        void Awake()
        {
            _source = GetComponent<AudioSource>();
            _source.clip = music;
            _source.loop = true;
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.ignoreListenerPause = true;
            _source.volume = 0f;
        }

        void Start()
        {
            if (music != null) _source.Play();
        }

        void Update()
        {
            float step = _fadeIn > 0f ? Time.unscaledDeltaTime * _volume / _fadeIn : 1f;
            _source.volume = Mathf.MoveTowards(_source.volume, _volume, step);
        }
    }
}
