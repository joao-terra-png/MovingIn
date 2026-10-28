using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EmptyPipe : MonoBehaviour
{
    public bool temHamster;

    void Start()
    {
        temHamster = false;
    }
    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Hamster"))
            temHamster = true;  
    }

        void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.CompareTag("Hamster"))
            temHamster = false;  
    }

    /*Retorna 0 se não está vazio, e 1 se está*/
    public bool IsEmpty()
    {
        return !temHamster;
    }
}