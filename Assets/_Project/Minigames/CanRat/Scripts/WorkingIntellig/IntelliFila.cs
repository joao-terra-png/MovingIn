using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using MovingIn.Core.Structures;

namespace MovingIn.Minigames.CanRat
{
    public class IntelliFila
    {
        FilaEncadeada filaBurra;
        //Guarda a posição onde o próximo cano da Fila será colocado
        public Vector3 NextPipePos {get; private set;}
        //Guarda o ultimo cano colocado
        public GameObject LastEnqPipe {get; private set;}
        

        //Objeto Inventario para comandos como store (em retirar)
        public GameObject Inventario;   //Passado como referencia para ser associado no FilaManager
        public IntelliFila()
        {
            filaBurra = new FilaEncadeada();
            //PosInicial como (0, 0, 0)
            NextPipePos = Vector3.zero;
            LastEnqPipe = null;

        }

        //Enfileira o proximo objeto de forma inteligente, validando a posição e o encaixe
        public bool IntelliEnque(GameObject NextObj)
        {
            if(NextObj == null)
                return false;
            else if (filaBurra.Vazia() && (NextObj.CompareTag("RightToUpPipe") ||
                    NextObj.CompareTag("RightToDownPipe") || 
                    NextObj.CompareTag("HorizontalPipe")))
            {
                //Enfileira e muda o estado de inventario
                filaBurra.Enf(NextObj);
                NextObj.GetComponent<InInventory>().SetIsStored(false);

                //add torna-se o LastEnqPipe
                LastEnqPipe = NextObj;
                LastEnqPipe.transform.position = NextPipePos;
                //Atualização do NexPipePos
                AtualizaNextPos();
                return true;
            }
            else if(LastEnqPipe.GetComponent<MatchPipe>().TestaEncaixe(NextObj.tag))
            {
                //Enfileira e muda o estado de inventario
                filaBurra.Enf(NextObj);
                NextObj.GetComponent<InInventory>().SetIsStored(false);

                //add torna-se o LastEnqPipe
                LastEnqPipe = NextObj;
                LastEnqPipe.transform.position = NextPipePos;
                //Atualização do NexPipePos
                AtualizaNextPos();
                return true;
            }
            else
                return false;
        }

        public GameObject FirstEnqdEmpty {get; private set;}
        public bool IntelliDeque()
        {
            FirstEnqdEmpty = (GameObject)filaBurra.Primeiro().GetInfo();  //Usado para ver se há hamster
            GameObject FstEnqdCasted;   //Receberá o Objeto em si se retirar da fila
            object fstObjectTemp;
            if (FirstEnqdEmpty.GetComponent<EmptyPipe>().IsEmpty() &&
                !Inventario.GetComponent<InventoryTools>().TemItem() && 
                filaBurra.Desenf(out fstObjectTemp))
            {
                FstEnqdCasted = (GameObject)fstObjectTemp;
                
                //coloca no inventario (atualizar posição)
                Inventario.GetComponent<InventoryTools>().Store(FstEnqdCasted);
                FirstEnqdEmpty = null;
                return true;
            }
            else{
                return false;
            }
        }

        //Verifica o Encaixe do LastEnqPipe com o próximo a ser adicionado
        void AtualizaNextPos()
        {
            switch (LastEnqPipe.tag)
            {
                case "HorizontalPipe":
                case "DownToRightPipe":
                case "UpToRightPipe":
                    NextPipePos = LastEnqPipe.transform.position + Vector3.right;
                    break;
                case "RightToUpPipe":
                case "VerticalPipe":
                    NextPipePos = LastEnqPipe.transform.position + Vector3.up;
                    break;
                case "RightToDownPipe":
                    NextPipePos = LastEnqPipe.transform.position + Vector3.down;
                    break;
                default:
                    NextPipePos = LastEnqPipe.transform.position + Vector3.zero;
                    break;
            }
        }

        public void SetInicialPos(Vector3 PosInicial)
        {
            if(NextPipePos == Vector3.zero)
                NextPipePos = PosInicial;
        }

    }
}