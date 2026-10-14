using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EmptyPipe : MonoBehaviour
{
    public bool temHamster;
    void OnCollisionStay(Collision collision)
    {
        if(collision.gameObject.CompareTag("Hamster"))
            temHamster = true;
        else
            temHamster = false;    
    }
    
    /*Retorna 0 se não está vazio, e 1 se está*/
    public bool IsEmpty()
    {
        return !temHamster;
    }
}