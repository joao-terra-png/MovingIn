using UnityEngine;
using UnityEngine.EventSystems;

namespace MovingIn.Minigames.Laundry{
    [RequireComponent(typeof(ClothPiece))]
    public class ClothDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        ClothPiece _piece;
        Camera _cam;
        Vector3 _originalPosition;
        Vector3 _offset;
        int _originalOrder;
        bool _dragging;

        void Awake()
        {
            _piece = GetComponent<ClothPiece>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (Time.timeScale == 0f) return;
            _dragging = true;
            _piece.StopMoving();

            _cam = e.pressEventCamera;
            _originalPosition = transform.position;
            _offset = transform.position - ScreenToWorld(e.position);
            _originalOrder = _piece.Order;
            _piece.Order = 100;
        }

        public void OnDrag(PointerEventData e)
        {
            if (!_dragging) return;
            transform.position = ScreenToWorld(e.position) + _offset;
        }

        public void OnEndDrag(PointerEventData e)
        {
            if (!_dragging) return;
            _dragging = false;
            _piece.Order = _originalOrder;

            LaundryZone zone = FindZoneAt(ScreenToWorld(e.position));
            bool accepted = _piece.Manager != null && _piece.Manager.PieceDropped(_piece, zone);
            if (!accepted) _piece.MoveTo(_originalPosition);
        }

        LaundryZone FindZoneAt(Vector3 point)
        {
            foreach (Collider2D hit in Physics2D.OverlapPointAll(point))
            {
                if (hit.gameObject == gameObject) continue;
                if (hit.TryGetComponent(out LaundryZone zone)) return zone;
            }
            return null;
        }

        Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 p = _cam.ScreenToWorldPoint(screenPos);
            p.z = 0f;
            return p;
        }
    }
}
