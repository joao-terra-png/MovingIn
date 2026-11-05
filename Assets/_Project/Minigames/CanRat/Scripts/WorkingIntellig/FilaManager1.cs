using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MovingIn.Minigames.CanRat
{
    /*
    Fila Manager1: Script para Apenas uma fila
    "Escolhe a fila"
    
    Apenas uma maneira de ganhar, portanto sem validação
    */
    public class FilaManager1 : MonoBehaviour
    {
        [SerializeField]GameObject InicialCage;
        [SerializeField]GameObject FinalCage;
        IntelliFila Fila;
        public bool enableFila;
        [SerializeField] GameObject Inventario;
        [SerializeField] GameObject SuperiorManager; 

        public IntelliFila GetFila()
        {
            return Fila;
        }

        public bool GetEnableFila()
        {
            return enableFila;
        }
    
        public void DisableFila(){
            enableFila = false;
        }
        
        void Awake()
        {
            Fila = new IntelliFila();
            Fila.SetInicialPos(InicialCage.transform.position + Vector3.right);
            enableFila = FinalCage.GetComponent<ValidaFila>().IsEnabled();
            Fila.Inventario = this.Inventario;
        }
    
        void Update()
        {
            enableFila = enableFila = FinalCage.GetComponent<ValidaFila>().IsEnabled();
            if(!enableFila)
                SuperiorManager.GetComponent<FinishGameEvent>().FinishGame();
        }
    
    }
}