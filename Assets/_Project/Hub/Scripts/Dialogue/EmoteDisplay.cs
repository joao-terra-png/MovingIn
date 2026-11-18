using UnityEngine;

namespace MovingIn.Hub{
    [RequireComponent(typeof(SpriteRenderer))]
    public class EmoteDisplay : MonoBehaviour
    {
        [SerializeField] SpriteRenderer rend;
        [SerializeField] Sprite exclaim;
        [SerializeField] Sprite question;
        [SerializeField] Sprite dots;
        [SerializeField] Sprite sleep;
        [SerializeField] Sprite note;
        [SerializeField] float _popTime = 0.15f;
        [SerializeField] float _bobAmount = 0.04f;

        Vector3 _baseScale;
        Vector3 _basePos;
        float _shownAt;
        float _hideAt;

        public bool IsShowing => rend != null && rend.enabled;

        void Awake()
        {
            if (rend == null) rend = GetComponent<SpriteRenderer>();
            _baseScale = transform.localScale;
            _basePos = transform.localPosition;
            rend.enabled = false;
        }

        public void Show(Emote emote, float duration)
        {
            Sprite s = Pick(emote);
            if (s == null)
            {
                Hide();
                return;
            }
            rend.sprite = s;
            rend.enabled = true;
            _shownAt = Time.time;
            _hideAt = duration > 0f ? Time.time + duration : float.PositiveInfinity;
        }

        public void Hide()
        {
            if (rend != null) rend.enabled = false;
        }

        void LateUpdate()
        {
            if (!rend.enabled) return;
            if (Time.time >= _hideAt)
            {
                Hide();
                return;
            }
            float age = Time.time - _shownAt;
            float t = Mathf.Clamp01(age / _popTime);
            float k = t < 1f ? Mathf.Lerp(0.3f, 1.15f, t) : Mathf.Lerp(1.15f, 1f, Mathf.Clamp01((age - _popTime) / _popTime));
            transform.localScale = _baseScale * k;
            transform.localPosition = _basePos + Vector3.up * (Mathf.Sin(age * 5f) * _bobAmount);
        }

        Sprite Pick(Emote e)
        {
            switch (e)
            {
                case Emote.Exclaim: return exclaim;
                case Emote.Question: return question;
                case Emote.Dots: return dots;
                case Emote.Sleep: return sleep;
                case Emote.Note: return note;
                default: return null;
            }
        }
    }
}
