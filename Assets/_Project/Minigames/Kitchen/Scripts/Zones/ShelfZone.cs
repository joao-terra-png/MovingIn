using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    public class ShelfZone : DropZone
    {
        [SerializeField] Vector3 _firstOffset = new Vector3(-1.5f, 0.5f, 0f);
        [SerializeField] float _spacing = 1f;
        int _count;

        public override bool TryReceive(ItemVisual item, bool belongToKitchen)
        {
            if(item.Data.type != ItemType.Utensil || !belongToKitchen) return false;

            item.transform.position = transform.position + _firstOffset + Vector3.right * (_spacing * _count);
            item.GetComponent<Collider2D>().enabled = false;
            _count++;
            return true;
        }
    }
}
