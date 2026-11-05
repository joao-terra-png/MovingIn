using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat{
    public class ValidaFila : MonoBehaviour
    {
        public UnityEvent ShowDefeatScreen = new UnityEvent();
        [SerializeField]GameObject HamsterPredestinado; // Hamster que deve chegar a essa cage
        GameObject HamsterAlcançado;    //Hamster que realmente chegou a cage

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
                //Testa se a fila é inválida
                if(!IsValid())
                    ShowDefeatScreen.Invoke();
            }
        }

        void OnCollsionStay(Collision collision)
        {
            HamsterAlcançado = collision.gameObject;
        }

    bool IsValid()
        {
            if(HamsterAlcançado == HamsterPredestinado)
                return true;
            else
                return false;
        }

    }
}
