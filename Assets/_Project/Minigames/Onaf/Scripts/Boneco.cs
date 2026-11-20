using System.Collections;
using System.Collections.Generic;
using MovingIn.Core.Structures;
using UnityEngine;

namespace MovingIn.Minigames.Onaf{
    public class Boneco : MonoBehaviour
    {
        public int idRoom {get; set;}
        public bool movEnable {get; set;}
    
        public void GoToNextRoom(Arvore ABB)
        {
            if(!movEnable)
                return ;
            
            QuartoNode nextRoom;
            float randomValue = Random.value;
            if(randomValue < 0.2)          //ABB->left
                nextRoom = ABB.GetQuarto(idRoom).GetEsq();
            else if(randomValue < 0.4)      //ABB->right
                nextRoom = ABB.GetQuarto(idRoom).GetDir();
            else                            //ABB->parent
                nextRoom = ABB.GetQuarto(idRoom).GetPai();

            int newIdRoom = nextRoom.GetID();
            ABB.ColocarBoneco(newIdRoom, this);
            ABB.GetQuarto(idRoom).RemoveBoneco();
            idRoom = newIdRoom;
        }
    
    
    }
}