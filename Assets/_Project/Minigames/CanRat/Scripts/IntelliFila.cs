using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using 

public class IntelliFila : MonoBehaviour
{
    FilaEncadeada filaBurra;
    //Guarda a posição onde o próximo cano da Fila será colocado
    Vector3 NextPipePos;
    //Guarda o ultimo cano colocado
    GameObject LastEnqPipe;

    //Objeto Inventario para comandos como store (em retirar)
    GameObject Inventario;


    void Start()
    {
        Inventario = GameObject.FindGameObjectWithTag("Inventory");    
    }
    public IntelliFila()
    {
        filaBurra = new FilaEncadeada();
        //PosInicial como (0, 0, 0)
        NextPipePos = Vector3.right;
        LastEnqPipe = null;
        
    }

    //Enfileira o proximo objeto de forma inteligente, validando a posição e o encaixe
    public bool IntelliEnque(GameObject NextObj)
    {
        //if (filaBurra.Vazia())
        //{
            //Enfileira e muda o estado de inventario
            filaBurra.Enf(NextObj);
            NextObj.GetComponent<InInventory>().SetEstado(true);

            //Se válido, o add torna-se o LastEnqPipe
            LastEnqPipe.transform.position = NextPipePos;
            LastEnqPipe = NextObj;

            //Atualização do NexPipePos
            AtualizaNextPos();
            return true;
        }   //Verifica o Encaixe do LastEnqPipe com o próximo a ser adicionado
        /*else if(LastEnqPipe.GetComponent<MatchPipe>().TestaEncaixe(NextObj.tag))
        {

            //Enfileira e muda o estado de inventario
            filaBurra.Enf(NextObj);
            NextObj.GetComponent<InInventory>().SetEstado(true);

            //Se válido, o add torna-se o LastEnqPipe
            LastEnqPipe.transform.position = NextPipePos;
            LastEnqPipe = NextObj;

            //Atualização do NexPipePos
            AtualizaNextPos();
            return true;
        }
        else
        {
            return false;
        }
    }*/

    public bool IntelliDeque(GameObject FirstObject)
    {
        if (!Inventario.GetComponent<InventoryTools>().TemItem() && filaBurra.Desenf(FirstObject))
        {
            //coloca no inventario (atualizar posição)
            Inventario.GetComponent<InventoryTools>().Store(FirstObject);
            //atualiza estado de inventario
            FirstObject.GetComponent<InInventory>().SetEstado(true);
            return true;
        }
        else
            return false;
    }

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

}
