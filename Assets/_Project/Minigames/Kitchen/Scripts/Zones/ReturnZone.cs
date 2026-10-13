using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary> caixa de devoulcao, so vai aceitar itens q nao sao da cozinha </summary>
    
    public class ReturnZone : DropZone
    {
        public override bool TryReceive(ItemVisual item, bool belongsToKitchen)
        {
            if(belongsToKitchen) return false;
            Destroy(item.gameObject);
            return true;
        }
    }
}
