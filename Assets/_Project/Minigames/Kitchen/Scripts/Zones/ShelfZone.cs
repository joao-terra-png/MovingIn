using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    public class ShelfZone : DropZone
    {
        [SerializeField] float _shelfWidth = 3f;
        [SerializeField] float _gap = 0.1f;

        float _usedWidth;

        public override bool TryReceive(ItemVisual item, bool belongsToKitchen)
        {
            if (item.Data.type != ItemType.Utensil || !belongsToKitchen) return false;

            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            Vector3 size = sr.bounds.size;

            Vector3 center = transform.position
                           + Vector3.right * (-_shelfWidth / 2f + _usedWidth + size.x / 2f)
                           + Vector3.up * (size.y / 2f);

            Vector3 offset = sr.bounds.center - item.transform.position;
            offset.z = 0f;

            item.MoveTo(center - offset);
            item.GetComponent<Collider2D>().enabled = false;
            _usedWidth += size.x + _gap;
            return true;
        }

        void OnDrawGizmos()
        {
            Vector3 p = transform.position;
            Vector3 left = p + Vector3.left * (_shelfWidth / 2f);
            Vector3 right = p + Vector3.right * (_shelfWidth / 2f);
            Gizmos.color = new Color(0.2f, 0.85f, 0.3f);
            Gizmos.DrawLine(left, right);
            Gizmos.DrawLine(left, left + Vector3.up * 0.15f);
            Gizmos.DrawLine(right, right + Vector3.up * 0.15f);
        }
    }
}