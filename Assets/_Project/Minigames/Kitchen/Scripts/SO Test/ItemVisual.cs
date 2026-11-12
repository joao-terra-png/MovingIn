using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ItemVisual : MonoBehaviour
    {
        public ItemData Data { get; private set; }
        public KitchenManager Manager { get; private set; }
        public DropZone CurrentZone { get; set; }
        public Color BaseColor { get; private set; }

        SpriteRenderer _sr;
        Movement _movement;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _movement = GetComponent<Movement>();
        }

        public void Configure(ItemData data, KitchenManager manager)
        {
            if (data == null)
            {
                Debug.LogError($"[ItemVisual] {name} recebeu uma ficha vazia (null).", this);
                return;
            }

            Data = data;
            Manager = manager;

            if (data.sprite != null)
            {
                transform.localScale = Vector3.one;
                _sr.sprite = data.sprite;
                BoxCollider2D col = GetComponent<BoxCollider2D>();
                col.size = data.sprite.bounds.size;
                col.offset = data.sprite.bounds.center;
                BaseColor = Color.white;
            }
            else
            {
                BaseColor = data.colorPlaceholder;
                transform.localScale = Vector3.one * (0.6f + 0.2f * data.size);
            }
            _sr.color = BaseColor;

            name = $"Item_{data.exhibitName}";
        }

        public void MoveTo(Vector3 destination)
        {
            if (_movement != null) _movement.MoveTo(destination);
            else transform.position = destination;
        }

        public void StopMoving()
        {
            if (_movement != null) _movement.Stop();
        }
    }
}