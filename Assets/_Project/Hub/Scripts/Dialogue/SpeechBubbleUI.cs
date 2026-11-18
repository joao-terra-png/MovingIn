using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class SpeechBubbleUI : MonoBehaviour
    {
        [SerializeField] RectTransform box;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] RectTransform tail;
        [SerializeField] Graphic advanceIcon;
        [SerializeField] Camera cam;
        [SerializeField] float _maxWidth = 520f;
        [SerializeField] Vector2 _padding = new Vector2(28f, 18f);
        [SerializeField] float _advanceSpace = 30f;
        [SerializeField] float _tailHeight = 16f;
        [SerializeField] float _lift = 12f;
        [SerializeField] float _screenMargin = 24f;

        RectTransform _rect;
        RectTransform _canvasRect;
        Speaker _speaker;
        float _blinkTime;

        void Awake()
        {
            _rect = (RectTransform)transform;
            _canvasRect = (RectTransform)GetComponentInParent<Canvas>(true).transform;
            if (cam == null) cam = Camera.main;
        }

        public void Open(Speaker speaker, string text)
        {
            if (_rect == null) Awake();
            _speaker = speaker;
            label.text = text;
            label.maxVisibleCharacters = 0;

            Vector2 size = label.GetPreferredValues(text, _maxWidth, 0f);
            size.x = Mathf.Min(Mathf.Ceil(size.x) + 2f, _maxWidth);
            size.y = Mathf.Ceil(label.GetPreferredValues(text, size.x, 0f).y);
            label.rectTransform.sizeDelta = size;
            label.rectTransform.anchoredPosition = new Vector2(-_advanceSpace / 2f, 0f);
            box.sizeDelta = size + _padding * 2f + new Vector2(_advanceSpace, 0f);

            SetAdvance(false);
            gameObject.SetActive(true);
            Reposition();
        }

        public void SetVisible(int count)
        {
            label.maxVisibleCharacters = count;
        }

        public void SetAdvance(bool on)
        {
            if (advanceIcon == null) return;
            advanceIcon.gameObject.SetActive(on);
            _blinkTime = 0f;
        }

        public void Close()
        {
            _speaker = null;
            gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            Reposition();
            if (advanceIcon != null && advanceIcon.gameObject.activeSelf)
            {
                _blinkTime += Time.unscaledDeltaTime;
                advanceIcon.rectTransform.anchoredPosition = new Vector2(advanceIcon.rectTransform.anchoredPosition.x, 10f + Mathf.Round(Mathf.Sin(_blinkTime * 6f) * 2f));
            }
        }

        void Reposition()
        {
            if (_speaker == null || cam == null) return;

            Vector3 screen = cam.WorldToScreenPoint(_speaker.HeadPosition);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, null, out Vector2 local);
            local.y += _lift;

            Rect area = _canvasRect.rect;
            float half = box.sizeDelta.x / 2f;
            float minX = area.xMin + _screenMargin + half;
            float maxX = area.xMax - _screenMargin - half;
            float boxX = Mathf.Clamp(local.x, minX, maxX) - local.x;

            float top = local.y + _tailHeight + box.sizeDelta.y;
            if (top > area.yMax - _screenMargin) local.y -= top - (area.yMax - _screenMargin);

            _rect.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y));
            box.anchoredPosition = new Vector2(Mathf.Round(boxX), _tailHeight);
            if (tail != null)
            {
                float limit = half - 28f;
                tail.anchoredPosition = new Vector2(Mathf.Clamp(0f, boxX - limit, boxX + limit), 0f);
            }
        }
    }
}
