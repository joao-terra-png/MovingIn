using UnityEngine;

namespace MovingIn.Hub{
    public class PlayerSpriteVisual : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] SpriteRenderer body;
        [SerializeField] Transform shadow;
        [SerializeField] Sprite frontSprite;
        [SerializeField] Sprite backSprite;
        [SerializeField] Sprite frontAirSprite;
        [SerializeField] Sprite backAirSprite;
        [SerializeField] Camera cam;

        [Header("Facing")]
        [SerializeField] float _turnThreshold = 0.35f;
        [SerializeField] float _minSpeed = 0.2f;
        [SerializeField] float _teleportDistance = 1f;

        [Header("Hop")]
        [SerializeField] float _hopHeight = 0.08f;
        [SerializeField] float _hopsPerMeter = 1f;
        [SerializeField] float _squash = 0.06f;
        [SerializeField] float _settleSpeed = 1.5f;
        [SerializeField] float _airThreshold = 0.6f;

        [Header("Shadow")]
        [SerializeField] float _shadowShrink = 0.3f;

        Vector3 _lastPos;
        Vector3 _shadowScale;
        float _phase;
        float _hop;
        bool _facingBack;
        bool _flip;

        public bool FacingBack => _facingBack;

        public void FaceFront() => _facingBack = false;
        public bool IsMoving { get; private set; }

        void Start()
        {
            if (target == null) target = transform.parent;
            if (cam == null) cam = Camera.main;
            _lastPos = target.position;
            _shadowScale = shadow != null ? shadow.localScale : Vector3.one;
        }

        void LateUpdate()
        {
            if (cam == null || target == null) return;

            Vector3 delta = target.position - _lastPos;
            _lastPos = target.position;
            delta.y = 0f;

            float dt = Time.deltaTime;
            float speed = dt > 0f ? delta.magnitude / dt : 0f;
            if (speed > 20f || delta.magnitude > _teleportDistance) speed = 0f;
            IsMoving = speed > _minSpeed;

            Vector3 screenRight = Flat(cam.transform.right);
            Vector3 screenUp = Flat(cam.transform.forward);

            if (IsMoving)
            {
                Vector3 dir = delta.normalized;
                float v = Vector3.Dot(dir, screenUp);
                float h = Vector3.Dot(dir, screenRight);
                if (v > _turnThreshold) _facingBack = true;
                else if (v < -_turnThreshold) _facingBack = false;
                if (h > 0.1f) _flip = false;
                else if (h < -0.1f) _flip = true;

                _phase += delta.magnitude * _hopsPerMeter;
                _hop = Mathf.Abs(Mathf.Sin(_phase * Mathf.PI));
            }
            else
            {
                _hop = Mathf.MoveTowards(_hop, 0f, _settleSpeed * dt);
                if (_hop <= 0f) _phase = 0f;
            }

            transform.rotation = Quaternion.Euler(0f, cam.transform.eulerAngles.y, 0f);

            float pitch = cam.transform.eulerAngles.x * Mathf.Deg2Rad;
            float stretch = 1f / Mathf.Max(0.2f, Mathf.Cos(pitch));
            float ground = IsMoving ? 1f - _hop : 0f;
            float sy = (1f - _squash * ground) * stretch;
            float sx = 1f + _squash * 0.5f * ground;

            bool inAir = IsMoving && _hop > _airThreshold;
            Sprite air = _facingBack ? backAirSprite : frontAirSprite;
            body.sprite = inAir && air != null ? air : (_facingBack ? backSprite : frontSprite);
            body.flipX = _flip;
            body.transform.localPosition = new Vector3(0f, _hop * _hopHeight, 0f);
            body.transform.localScale = new Vector3(sx, sy, 1f);

            if (shadow != null)
                shadow.localScale = _shadowScale * (1f - _shadowShrink * _hop);
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }
    }
}
