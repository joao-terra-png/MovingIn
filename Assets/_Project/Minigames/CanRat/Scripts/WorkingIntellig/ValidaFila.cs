using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat
{
    public class ValidaFila : MonoBehaviour
    {
        public UnityEvent ShowDefeatScreen = new UnityEvent();
        [SerializeField]GameObject HamsterPredestinado; // Hamster que deve chegar a essa cage

        bool EnableQueue;
        void Awake()
        {
            EnableQueue = true;
        }

        public bool IsEnabled()
        {
            return EnableQueue;
        }
        void Update()
        {
            if(!gameObject.GetComponent<EmptyPipe>().IsEmpty() && EnableQueue){
                Debug.Log("Desabilitando fila...");
                EnableQueue = false;
            }
        }
        void LateUpdate()
        {
            if(!EnableQueue && !IsValid()){
                    ShowDefeatScreen.Invoke();
                    Time.timeScale = 0f;    //Pausa o jogo    
                }
        }



    bool IsValid()
        {
            if(gameObject.GetComponent<EmptyPipe>().HamsterInside == HamsterPredestinado){
                Debug.Log("Fila valida!");
                return true;
            }
            else{
                Debug.Log("Fila invalida!");
                return false;
            }
        }

    }
}
