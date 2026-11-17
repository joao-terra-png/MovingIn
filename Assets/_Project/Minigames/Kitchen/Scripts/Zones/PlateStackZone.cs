using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen{
    public class PlateStackZone : DropZone
    {
        [SerializeField] int _capacity = 3;
        [SerializeField, Range(0.1f, 1f)] float _stackSpacing = 0.6f;
        [SerializeField] bool _finalDestination;
        [SerializeField] float _gizmoWidth = 1.2f;

        StaticStack<ItemVisual> _stack;
        float _stackHeight;

        public bool IsComplete => _finalDestination && _stack != null && _stack.Full();
        public override bool IsFinalDestination => IsComplete;

        void Awake()
        {
            _stack = new StaticStack<ItemVisual>(_capacity);
        }

        public override bool TryReceive(ItemVisual item, bool belongsToKitchen)
        {
            if (item.Data.type != ItemType.Plate) return false;
            if (_stack.Full()) return false;
            if (_stack.Top(out ItemVisual top) && item.Data.size >= top.Data.size) return false;

            if (_stack.Top(out ItemVisual oldTop)) SetClickable(oldTop, false);
            _stack.Push(item);

            float height = PlateHeight(item);
            Vector3 center = transform.position + Vector3.up * (_stackHeight + height / 2f);
            item.MoveTo(center - PivotOffset(item));
            _stackHeight += height * _stackSpacing;

            item.GetComponent<SpriteRenderer>().sortingOrder = 20 + _stack.Quantity;
            SetClickable(item, !IsComplete);
            return true;
        }

        public override void Release(ItemVisual item)
        {
            if (!_stack.Pop(out ItemVisual removed)) return;
            _stackHeight = Mathf.Max(0f, _stackHeight - PlateHeight(removed) * _stackSpacing);
            if (_stack.Top(out ItemVisual newTop)) SetClickable(newTop, true);
        }

        float PlateHeight(ItemVisual plate)
        {
            return plate.GetComponent<SpriteRenderer>().bounds.size.y;
        }

        Vector3 PivotOffset(ItemVisual plate)
        {
            Vector3 offset = plate.GetComponent<SpriteRenderer>().bounds.center - plate.transform.position;
            offset.z = 0f;
            return offset;
        }

        void SetClickable(ItemVisual plate, bool value)
        {
            plate.GetComponent<Collider2D>().enabled = value;
        }

        void OnDrawGizmos()
        {
            Vector3 p = transform.position;
            Gizmos.color = _finalDestination ? new Color(0.2f, 0.85f, 0.3f) : new Color(1f, 0.65f, 0.15f);
            Gizmos.DrawLine(p + Vector3.left * (_gizmoWidth / 2f), p + Vector3.right * (_gizmoWidth / 2f));
            Gizmos.DrawLine(p + Vector3.left * (_gizmoWidth / 2f), p + Vector3.left * (_gizmoWidth / 2f) + Vector3.up * 0.15f);
            Gizmos.DrawLine(p + Vector3.right * (_gizmoWidth / 2f), p + Vector3.right * (_gizmoWidth / 2f) + Vector3.up * 0.15f);
            Gizmos.DrawWireSphere(p, 0.05f);
        }
    }
}