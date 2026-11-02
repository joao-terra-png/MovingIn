using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.CanRat
{
    public class InventoryTools : MonoBehaviour
    {
        //Verifica se há item no Inventario (pelo rayCast)
        [SerializeField] bool ComItem;
        void Update()
        {
            ComItem = TemItem();
        }

        public bool TemItem()
        {   
            RaycastHit ponto;
            Physics.Raycast(transform.position, Vector3.forward, out ponto, 1.6f);
                //Debug.Log($"{ponto.collider.gameObject.tag} foi atingido pelo raio do inventario");
            if(ponto.collider.gameObject != null && ponto.collider.gameObject.CompareTag("Backdrop"))
                return false;
            else
                return true;
        }
    
    
        public bool Store(GameObject objeto)
        {
            RaycastHit InvRay;
            Physics.Raycast(transform.position, Vector3.forward, out InvRay, 1.6f);
            if(!TemItem()){
                Debug.Log("Storing in Inventory...");
                objeto.transform.position = InvRay.point + 0.1f * Vector3.up;
                objeto.GetComponent<InInventory>().SetIsStored(true);
                return true;    
            }
            else{
                Debug.Log("Unfeasible to store in inventory");
                return false;
            }
        }
    
    
    }
}