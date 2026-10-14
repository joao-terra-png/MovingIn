using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInventory : MonoBehaviour
{
    bool IsStored;
    //Estado inicial de todos os canos (guardados e dispostos ao posicionamento)
    void Start()
    {
        IsStored = true;
    }

    /*Dependendo se entrou ou não na fila
        O estado é atualizado    
    @param: bool sucesso: indicador se a operação realizada (enq ou deq) sucedeu ou não
    */
    public void SetEstado(bool sucesso)
    {
        if(sucesso)
            IsStored = !IsStored;
    }

}
