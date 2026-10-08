using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary>
    /// permite arratar o item com o mouse
    ///</summary>
    
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        Camera _cam;
        Vector3 _originalPosition;
        Vector3 _offset; //pro item nao ficar clipando no mouse

        SpriteRenderer _sr;
        int _originalOrder;

        ItemVisual _item;

        void Awake()
        {
            _sr = GetComponent<SpriteRenderer>();
            _item = GetComponent<ItemVisual>();
        }

        public void OnBeginDrag(PointerEventData e)
        {
            _cam = e.pressEventCamera;
            _originalPosition = transform.position;
            _offset = transform.position - ScreenToWorld(e.position);
            _originalOrder = _sr.sortingOrder;
            _sr.sortingOrder = 100;
        }

        public void OnDrag(PointerEventData e)
        {
            transform.position = ScreenToWorld(e.position) + _offset;
        }

        public void OnEndDrag(PointerEventData e)
        {
            _sr.sortingOrder = _originalOrder;

            DropZone zone = FindZoneAt(ScreenToWorld(e.position));

            bool accepted = _item.Manager.ItemDropped(_item, zone);

            if(!accepted) transform.position = _originalPosition;
        }

        Vector3 ScreenToWorld(Vector2 screenPos)
        {
            Vector3 p = _cam.ScreenToWorldPoint(screenPos);
            p.z = 0f;
            return p;
        }

        DropZone FindZoneAt(Vector3 point)
        {
            Collider2D[] hits = Physics2D.OverlapPointAll(point);
            foreach(Collider2D hit in hits)
            {
                if(hit.gameObject == gameObject) continue;
                if(hit.TryGetComponent(out DropZone zone)) return zone;
            }

            return null;
        }

    }
}

