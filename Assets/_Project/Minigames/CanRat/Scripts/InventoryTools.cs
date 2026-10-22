using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryTools : MonoBehaviour
{

    //Verifica se há item no Inventario (pelo rayCast)
    public bool TemItem()
    {   
        RaycastHit ponto;
        Physics.Raycast(transform.position, Vector3.forward, out ponto, 2f);
        if(ponto.collider.gameObject.CompareTag("Backdrop"))
        {
            return true;
        }
        else
            return false;
    }


    public bool Store(GameObject objeto)
    {
        RaycastHit InvRay;
        Physics.Raycast(transform.position, Vector3.forward, out InvRay, 2f);
        if(!TemItem()){
            objeto.transform.position = InvRay.point;
            return true;    
        }
        else
        {
             return false;
        }
    }


}