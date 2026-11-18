using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.CanRat
{
    public class InventoryTools : MonoBehaviour
    {
        //Verifica se há item no Inventario (pelo rayCast)
        float maxDistance; //Distãncia da camera do inventario à parede
        [SerializeField] GameObject RecycleBox;
        [SerializeField] GameObject TargetBox;
        Vector3 diretor;
        void Awake()
        {
            maxDistance = 1.5f * Mathf.Abs(transform.position.z);
            diretor = RecycleBox.transform.position - transform.position;
        }
        public bool TemItem()
        {   
            RaycastHit ponto;
            Physics.Raycast(transform.position, diretor, out ponto, maxDistance);
                Debug.Log($"{ponto.collider.gameObject.tag} foi atingido pelo raio do inventario");
            if(ponto.collider.gameObject != null && ponto.collider.gameObject.CompareTag("Target"))
                return false;
            else
                return true;
        }
    
    
        public bool Store(GameObject objeto)
        {
            RaycastHit InvRay;
            Physics.Raycast(transform.position, Vector3.forward, out InvRay, maxDistance);
            if(!TemItem()){
                Debug.Log("Storing in Inventory...");
                objeto.transform.position = RecycleBox.transform.position;
                objeto.GetComponent<InInventory>().SetIsStored(true);
                //TargetBox.transform.position = TargetBox.transform.position.z * Vector3.back + 
                    //RecycleBox.transform.position;


                return true;    
            }
            else{
                Debug.Log("Unfeasible to store in inventory");
                return false;
            }
        }
    
    
    }
}