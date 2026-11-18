using TMPro;
using UnityEngine;

namespace MovingIn.Hub{
    public class InteractPromptUI : MonoBehaviour
    {
        [SerializeField] RectTransform box;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Camera cam;
        [SerializeField] string _keyLabel = "E";
        [SerializeField] Vector2 _padding = new Vector2(16f, 8f);
        [SerializeField] float _lift = 20f;
        [SerializeField] float _screenMargin = 24f;

        RectTransform _rect;
        RectTransform _canvasRect;
        Collider _target;
        string _shown;

        void Awake()
        {
            _rect = (RectTransform)transform;
            _canvasRect = (RectTransform)GetComponentInParent<Canvas>(true).transform;
            if (cam == null) cam = Camera.main;
        }

        public void Show(Collider target, string prompt)
        {
            if (_rect == null) Awake();
            _target = target;
            if (prompt != _shown)
            {
                _shown = prompt;
                label.text = $"<color=#FFD9B0>[{_keyLabel}]</color> {prompt}";
                Vector2 size = label.GetPreferredValues(label.text);
                size.x = Mathf.Ceil(size.x) + 2f;
                size.y = Mathf.Ceil(size.y);
                label.rectTransform.sizeDelta = size;
                box.sizeDelta = size + _padding * 2f;
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            Reposition();
        }

        public void Hide()
        {
            _target = null;
            _shown = null;
            if (gameObject.activeSelf) gameObject.SetActive(false);
        }

        void LateUpdate()
        {
            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                Hide();
                return;
            }
            Reposition();
        }

        void Reposition()
        {
            if (_target == null || cam == null) return;
            Bounds b = _target.bounds;
            Vector3 top = new Vector3(b.center.x, b.max.y, b.center.z);
            Vector3 screen = cam.WorldToScreenPoint(top);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvasRect, screen, null, out Vector2 local);
            local.y += _lift;

            Rect area = _canvasRect.rect;
            Vector2 half = box.sizeDelta / 2f;
            local.x = Mathf.Clamp(local.x, area.xMin + _screenMargin + half.x, area.xMax - _screenMargin - half.x);
            local.y = Mathf.Clamp(local.y, area.yMin + _screenMargin, area.yMax - _screenMargin - box.sizeDelta.y);
            _rect.anchoredPosition = new Vector2(Mathf.Round(local.x), Mathf.Round(local.y));
        }
    }
}
