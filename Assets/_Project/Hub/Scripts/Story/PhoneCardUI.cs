using System.Collections;
using UnityEngine;

namespace MovingIn.Hub{
    public class PhoneCardUI : MonoBehaviour
    {
        [SerializeField] RectTransform card;
        [SerializeField] CanvasGroup group;
        [SerializeField] float _slideTime = 0.3f;
        [SerializeField] float _slideDistance = 80f;

        Vector2 _shownPos;
        Coroutine _anim;

        void Awake()
        {
            _shownPos = card.anchoredPosition;
            group.alpha = 0f;
            card.gameObject.SetActive(false);
        }

        public void Show()
        {
            Play(Slide(true));
        }

        public void Hide()
        {
            Play(Slide(false));
        }

        void Play(IEnumerator routine)
        {
            if (_anim != null) StopCoroutine(_anim);
            _anim = StartCoroutine(routine);
        }

        IEnumerator Slide(bool show)
        {
            card.gameObject.SetActive(true);
            float start = group.alpha;
            float end = show ? 1f : 0f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / _slideTime;
                float s = Mathf.SmoothStep(0f, 1f, t);
                float a = Mathf.Lerp(start, end, s);
                group.alpha = a;
                card.anchoredPosition = _shownPos + Vector2.down * (_slideDistance * (1f - a));
                yield return null;
            }
            if (!show) card.gameObject.SetActive(false);
            _anim = null;
        }
    }
}
