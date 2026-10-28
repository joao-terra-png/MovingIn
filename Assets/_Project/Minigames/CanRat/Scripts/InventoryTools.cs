using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MovingIn.Minigames.CanRat
{
    public class InventoryTools : MonoBehaviour
    {
    
        //Verifica se há item no Inventario (pelo rayCast)
        public bool TemItem()
        {   
            RaycastHit ponto;
            Physics.Raycast(transform.position, Vector3.forward, out ponto, 2f);
            if(ponto.collider.gameObject.CompareTag("Backdrop"))
                return false;
            else
                return true;
        }
    
    
        public bool Store(GameObject objeto)
        {
            RaycastHit InvRay;
            Physics.Raycast(transform.position, Vector3.forward, out InvRay, 2f);
            if(!TemItem()){
                objeto.transform.position = InvRay.point + 0.1f * Vector3.up;
                objeto.GetComponent<InInventory>().SetIsStored(true);
                return true;    
            }
            else
                return false;
        }
    
    
    }
}