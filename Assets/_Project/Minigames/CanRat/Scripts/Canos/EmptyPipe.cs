using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace MovingIn.Minigames.CanRat{
    public class EmptyPipe : MonoBehaviour
    {
        public bool temHamster;
        public GameObject HamsterInside;
    
        void Start()
        {
            temHamster = false;
        }
        void OnCollisionEnter(Collision collision)
        {
            if(collision.gameObject.CompareTag("Hamster")){
                temHamster = true;  
                HamsterInside = collision.gameObject;    
            }
        }
    
        void OnCollisionExit(Collision collision)
        {
            if(collision.gameObject.CompareTag("Hamster")){
                HamsterInside = null;
                temHamster = false;  
            }
        }
    
        /*Retorna 0 se não está vazio, e 1 se está*/
        public bool IsEmpty()
        {
            return !temHamster;
        }
    }
}