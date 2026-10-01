using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ItemVisual : MonoBehaviour
    {
        public ItemData Data { get; private set; }

        private SpriteRenderer sr;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }

        public void Configure(ItemData data)
        {
            if (data == null)
            {
                Debug.LogError($"[ItemVisual] {name} recebeu uma ficha vazia (null). Confira os campos no Inspector.");
                return;
            }

            Data = data;
            if(data.sprite != null) sr.sprite = data.sprite;
            sr.color = data.colorPlaceholder;
            transform.localScale = Vector3.one * (0.6f + 0.2f * data.size);
            name = $"Item_{data.exhibitName}";
        }

    }
}
