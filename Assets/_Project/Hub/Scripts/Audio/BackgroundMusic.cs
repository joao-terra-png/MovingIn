using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace MovingIn.Hub{
    public enum MusicTrack { Day, Night, Silence }

    [Serializable]
    public class ChapterMusic
    {
        public Chapter chapter;
        public MusicTrack track;

        public ChapterMusic(Chapter chapter, MusicTrack track)
        {
            this.chapter = chapter;
            this.track = track;
        }
    }

    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusic : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("music")] AudioClip dayMusic;
        [SerializeField] AudioClip nightMusic;
        [SerializeField, Range(0f, 1f)] float _volume = 0.5f;
        [SerializeField] float _fadeTime = 2f;
        [Tooltip("Silencio enquanto escurece antes da primeira noite; a musica da noite entra quando a cena termina")]
        [SerializeField] bool _quietDuringNightfall = true;
        [SerializeField] ChapterMusic[] chapters =
        {
            new ChapterMusic(Chapter.Arrival, MusicTrack.Day),
            new ChapterMusic(Chapter.Blackout, MusicTrack.Silence),
            new ChapterMusic(Chapter.PowerRestored, MusicTrack.Night),
            new ChapterMusic(Chapter.Afternoon, MusicTrack.Day),
            new ChapterMusic(Chapter.FirstNight, MusicTrack.Night),
            new ChapterMusic(Chapter.Morning, MusicTrack.Day),
        };

        AudioSource _day;
        AudioSource _night;

        void Awake()
        {
            _day = GetComponent<AudioSource>();
            _night = gameObject.AddComponent<AudioSource>();
            Setup(_day, dayMusic);
            Setup(_night, nightMusic);
        }

        static void Setup(AudioSource source, AudioClip clip)
        {
            source.clip = clip;
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.ignoreListenerPause = true;
            source.volume = 0f;
            if (clip != null) source.Play();
        }

        void Update()
        {
            MusicTrack track = CurrentTrack();
            float step = _fadeTime > 0f ? Time.unscaledDeltaTime * _volume / _fadeTime : 1f;
            _day.volume = Mathf.MoveTowards(_day.volume, track == MusicTrack.Day ? _volume : 0f, step);
            _night.volume = Mathf.MoveTowards(_night.volume, track == MusicTrack.Night ? _volume : 0f, step);
        }

        MusicTrack CurrentTrack()
        {
            StoryManager sm = StoryManager.Instance;
            if (sm == null) return MusicTrack.Day;
            if (_quietDuringNightfall && sm.Current == Chapter.FirstNight && sm.IsBusy) return MusicTrack.Silence;
            foreach (ChapterMusic c in chapters)
                if (c.chapter == sm.Current) return c.track;
            return MusicTrack.Day;
        }
    }
}
