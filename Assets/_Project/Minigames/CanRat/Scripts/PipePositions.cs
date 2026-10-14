using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PipePositions : MonoBehaviour
{
    //Guarda a posição onde o próximo cano da Fila será colocado
    Vector3 NextPipePos;
    
    //Guarda o ultimo cano colocado
    GameObject LastEnqPipe;

    void Start()
    {
        //PosInicial como (0, 0, 0)
        NextPipePos = Vector3.right; 
    }

    
    //Enfileira o proximo objeto de forma inteligente, validando a posição e o encaixe
    
    bool IntelliEnque(GameObject NextObj)
    {
        //Verifica o Encaixe do LastEnqPipe com o próximo a ser adicionado
        if(LastEnqPipe.GetComponent<MatchPipe>().TestaEncaixe(NextObj.tag)){
        //Se válido, o add torna-se o LastEnqPipe
        LastEnqPipe.transform.position = NextPipePos;
        LastEnqPipe = NextObj;
        //Atualização do NexPipePos
        AtualizaNextPos();

        }


    }

    void AtualizaNextPos()
    {
        switch (LastEnqPipe.tag)
        {
            case "HorizontalPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.right;
                break;
            case "RightToUpPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.up;
                break;
            case "RightToDownPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.down;
                break;
            case "UpToRightPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.right;
                break;
            case "DownToRightPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.right;
                break;
            case "VerticalPipe":
                NextPipePos = LastEnqPipe.transform.position + Vector3.up;
                break;
            default:
                NextPipePos = LastEnqPipe.transform.position + Vector3.zero;
                break;
        }
    }

}
