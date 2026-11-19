using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ClothPiece : MonoBehaviour
    {
        [SerializeField] float _moveDuration = 0.15f;
        [SerializeField] float _outline = 0.05f;
        [SerializeField] Color _outlineInk = new Color(0.10f, 0.08f, 0.12f);

        public ClothColor ClothColor { get; private set; }
        public ClothType ClothType { get; private set; }
        public LaundryManager Manager { get; private set; }
        public Color BaseColor { get; private set; }

        static Sprite _square;

        SpriteRenderer _sr;
        BoxCollider2D _col;
        Coroutine _move;
        Coroutine _flash;
        bool _useSprite;
        int _order;
        Color _detailColor;
        readonly List<SpriteRenderer> _outlines = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> _fills = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> _details = new List<SpriteRenderer>();

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _col = GetComponent<BoxCollider2D>();
        }

        public void Setup(ClothColor color, ClothType type, Sprite sprite, Vector2 placeholderSize,
                          Color tint, LaundryManager manager)
        {
            ClothColor = color;
            ClothType = type;
            Manager = manager;
            BaseColor = tint;
            transform.localScale = Vector3.one;
            _useSprite = sprite != null;

            if (_useSprite)
            {
                _sr.sprite = sprite;
                _sr.color = tint;
                _sr.enabled = true;
                _col.size = sprite.bounds.size;
                _col.offset = Vector2.zero;
            }
            else
            {
                _sr.enabled = false;
                BuildShape(ClothShapes.For(type), tint);
            }

            Order = _order;
            name = $"Roupa_{ClothNames.Of(type)}_{ClothNames.Of(color)}";
        }

        void BuildShape(ClothPart[] parts, Color tint)
        {
            float lum = tint.r * 0.299f + tint.g * 0.587f + tint.b * 0.114f;
            _detailColor = lum < 0.35f ? Color.Lerp(tint, Color.white, 0.22f) : Color.Lerp(tint, Color.black, 0.22f);
            Color outline = Color.Lerp(tint, _outlineInk, 0.75f);

            Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 max = new Vector2(float.MinValue, float.MinValue);

            foreach (ClothPart p in parts)
            {
                _outlines.Add(MakePart("Outline", p, Vector2.one * _outline, outline));
                (p.detail ? _details : _fills).Add(MakePart(p.detail ? "Detail" : "Fill", p, Vector2.zero, p.detail ? _detailColor : tint));
                Grow(p, ref min, ref max);
            }

            _col.size = max - min + Vector2.one * _outline;
            _col.offset = (min + max) / 2f;
        }

        SpriteRenderer MakePart(string partName, ClothPart p, Vector2 grow, Color color)
        {
            GameObject go = new GameObject(partName);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = p.center;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, p.angle);
            go.transform.localScale = new Vector3(p.size.x + grow.x, p.size.y + grow.y, 1f);
            SpriteRenderer r = go.AddComponent<SpriteRenderer>();
            r.sprite = Square();
            r.color = color;
            r.sortingLayerID = _sr.sortingLayerID;
            return r;
        }

        static void Grow(ClothPart p, ref Vector2 min, ref Vector2 max)
        {
            Quaternion rot = Quaternion.Euler(0f, 0f, p.angle);
            for (int i = 0; i < 4; i++)
            {
                Vector2 corner = new Vector2((i & 1) == 0 ? -0.5f : 0.5f, (i & 2) == 0 ? -0.5f : 0.5f);
                Vector2 c = p.center + (Vector2)(rot * Vector2.Scale(corner, p.size));
                min = Vector2.Min(min, c);
                max = Vector2.Max(max, c);
            }
        }

        static Sprite Square()
        {
            if (_square == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                _square = Sprite.Create(tex, new Rect(0f, 0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), tex.width);
            }
            return _square;
        }

        public int Order
        {
            get => _order;
            set
            {
                _order = value;
                _sr.sortingOrder = value * 4 + 1;
                foreach (SpriteRenderer r in _outlines) r.sortingOrder = value * 4;
                foreach (SpriteRenderer r in _fills) r.sortingOrder = value * 4 + 1;
                foreach (SpriteRenderer r in _details) r.sortingOrder = value * 4 + 2;
            }
        }

        public void SetVisible(bool visible)
        {
            _sr.enabled = visible && _useSprite;
            foreach (SpriteRenderer r in _outlines) r.enabled = visible;
            foreach (SpriteRenderer r in _fills) r.enabled = visible;
            foreach (SpriteRenderer r in _details) r.enabled = visible;
            _col.enabled = visible;
        }

        public void SetClickable(bool clickable) => _col.enabled = clickable;

        public void MoveTo(Vector3 target)
        {
            StopMoving();
            _move = StartCoroutine(MoveRoutine(target));
        }

        public void StopMoving()
        {
            if (_move != null) StopCoroutine(_move);
            _move = null;
        }

        IEnumerator MoveRoutine(Vector3 target)
        {
            Vector3 start = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _moveDuration;
                transform.position = Vector3.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            transform.position = target;
            _move = null;
        }

        public void Flash(Color color)
        {
            if (_flash != null) StopCoroutine(_flash);
            _flash = StartCoroutine(FlashRoutine(color));
        }

        IEnumerator FlashRoutine(Color color)
        {
            for (int i = 0; i < 3; i++)
            {
                Paint(color, Color.Lerp(color, Color.black, 0.2f));
                yield return new WaitForSeconds(0.12f);
                Paint(BaseColor, _detailColor);
                yield return new WaitForSeconds(0.12f);
            }
            _flash = null;
        }

        void Paint(Color fill, Color detail)
        {
            _sr.color = fill;
            foreach (SpriteRenderer r in _fills) r.color = fill;
            foreach (SpriteRenderer r in _details) r.color = detail;
        }
    }
}
