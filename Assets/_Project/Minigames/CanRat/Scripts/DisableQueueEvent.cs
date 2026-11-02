using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat{
    public class DisableQueueEvent : MonoBehaviour
    {
        public UnityEvent DisableQueue = new UnityEvent();
        [SerializeField]bool TemRato;

        void Update()
        {
            TemRato = !gameObject.GetComponent<EmptyPipe>().IsEmpty();
            if(TemRato){
                Debug.Log("Desabilitando fila...");
                DisableQueue.Invoke();
            }
        }
    }
}
