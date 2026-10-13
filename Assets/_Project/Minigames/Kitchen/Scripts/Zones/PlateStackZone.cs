using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen
{
    public class PlateStackZone : DropZone
    {
        [SerializeField] int _capacity = 3;
        [SerializeField] float _heightPerPlate = 0.25f;

        [Tooltip("Armario: so aceita comencando pelo grande e sempre menor do que o de cima")]
        [SerializeField] bool _requiresOrder;

        [Tooltip("Armario: guardar aqui cumpre o bilhete e os pratos nao podem amis sair")]
        [SerializeField] bool _finalDestination;

        StaticStack<ItemVisual> _stack;
        public override bool IsFinalDestination => _finalDestination;

        private void Awake()
        {
            _stack = new StaticStack<ItemVisual>(_capacity);

        }

        public override bool TryReceive(ItemVisual item, bool belongsToKitchen)
        {
            if(item.Data.type != ItemType.Plate) return false;
            if(_stack.Full()) return false;

            if(_requiresOrder)
            {
                if(_stack.Empty())
                {
                    if(item.Data.size != 3) return false;
                }
                else if (_stack.Top(out ItemVisual top) && item.Data.size >= top.Data.size)
                {
                    return false;
                }
            }

            if(_stack.Top(out ItemVisual oldTop)) SetClickable(oldTop, false);
            _stack.Push(item);

            int n = _stack.Quantity;
            item.transform.position = transform.position + Vector3.up * (_heightPerPlate * (n - 1));
            item.GetComponent<SpriteRenderer>().sortingOrder = 20 + n;
            SetClickable(item, !_finalDestination);
            return true;
        }

        public override void Release(ItemVisual item)
        {
            if(!_stack.Pop(out ItemVisual removed)) return;
            if(removed != item) Debug.LogWarning("[PlateStackZone] saiu um prato diferente do que era pra ser", this);
            if(_stack.Top(out ItemVisual newTop)) SetClickable(newTop, true);
        }

        void SetClickable(ItemVisual plate, bool value)
        {
            plate.GetComponent<Collider2D>().enabled = value;
        }
    }
}
