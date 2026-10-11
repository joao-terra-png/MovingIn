using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosicionaObjeto : MonoBehaviour
{
    GameObject []lista;
    GameObject atual, next;
    void Start()
    {
        lista = GameObject.FindGameObjectsWithTag("Cano");
        atual = lista[0];
        next = lista[1];
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            atual.transform.position = next.transform.position + 2*Vector3.up;
            GameObject temp = atual;
            atual = next;
            next = temp;
        }
        else if(Input.GetKeyDown(KeyCode.S))
        {
            atual.transform.position = next.transform.position + 2*Vector3.down;
            GameObject temp = atual;
            atual = next;
            next = temp;
        }
        else if(Input.GetKeyDown(KeyCode.D))
        {
            atual.transform.position = next.transform.position + 2*Vector3.forward;
            GameObject temp = atual;
            atual = next;
            next = temp;
        }
        else if(Input.GetKeyDown(KeyCode.A))
        {
            atual.transform.position = next.transform.position + 2*Vector3.back;
            GameObject temp = atual;
            atual = next;
            next = temp;
        }
    }
}
