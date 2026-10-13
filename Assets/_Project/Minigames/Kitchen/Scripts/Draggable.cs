using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary>
    /// permite arratar o item com o mouse
    ///</summary>
    
    [RequireComponent(typeof(ItemVisual))]
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        ItemVisual _item;
        SpriteRenderer _sr;
        Camera _cam;
        Vector3 _originalPosition;
        Vector3 _offset;
        int _originalOrder;
        bool _dragging;  

        void Awake()
        {
            _item = GetComponent<ItemVisual>();
            _sr = GetComponent<SpriteRenderer>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            if (Time.timeScale == 0f) return;   

            _dragging = true;
            _item.StopMoving();                 

            _cam = e.pressEventCamera;
            _originalPosition = transform.position;
            _offset = transform.position - ScreenToWorld(e.position);
            _originalOrder = _sr.sortingOrder;
            _sr.sortingOrder = 100;
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

            _sr.sortingOrder = _originalOrder;   

            DropZone zone = FindZoneAt(ScreenToWorld(e.position));
            bool accepted = _item.Manager != null && _item.Manager.ItemDropped(_item, zone);

            if (!accepted) _item.MoveTo(_originalPosition); 
        }

        DropZone FindZoneAt(Vector3 point)
        {
            foreach (Collider2D hit in Physics2D.OverlapPointAll(point))
            {
                if (hit.gameObject == gameObject) continue;
                if (hit.TryGetComponent(out DropZone zone)) return zone;
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

