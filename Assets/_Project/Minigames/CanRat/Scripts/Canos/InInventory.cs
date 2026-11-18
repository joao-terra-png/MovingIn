using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
namespace MovingIn.Minigames.CanRat
{
    public class InInventory : MonoBehaviour
    {

        /*to do: Projeto pra static InvVazio: contador que decrementa/aumenta conforme eu retiro os canos*/
        [SerializeField]bool IsStored;
        //Estado inicial de todos os canos (guardados e dispostos ao posicionamento)
        void Start()
        {
            IsStored = true;
        }

        /*Dependendo se entrou ou não na fila
            O estado é atualizado    
        @param: bool sucesso: indicador se a operação realizada (enq ou deq) sucedeu ou não
        */
        public void SetIsStored(bool state)
        {
            IsStored = state;
        }

    }
}