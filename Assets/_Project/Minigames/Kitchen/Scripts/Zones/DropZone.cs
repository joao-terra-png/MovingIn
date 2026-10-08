using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary>
    /// base de toda zona onde um item pode ser solto
    ///</sumarry>
    
    public abstract class DropZone : MonoBehaviour
    {
        //marcar check na lista se true
        public virtual bool IsFinalDestination => true;

        //tenta receber um item (true se aceitou)
        public abstract bool TryRecive(ItemVisual item, bool belongsToKitchen);

        //quando um item foi para outra zona
        public virtual void Release(ItemVisual item){}
    }
}
