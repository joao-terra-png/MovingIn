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
        GameObject InicialCage;
        public IntelliFila Fila { get; private set; }
        public bool enableFila { get; set; }
    
        [SerializeField] GameObject SuperiorManager; 
    
        public void DisableFila(){
            enableFila = false;
        }
        
        void Awake()
        {
            Fila = new IntelliFila();
            InicialCage = GameObject.FindGameObjectWithTag("InicialCage");
            Fila.SetInicialPos(InicialCage.transform.position + Vector3.right);
            enableFila = true;
        }
    
        void Update()
        {
            if(!enableFila)
                SuperiorManager.GetComponent<FinishGameEvent>().FinishGame();
        }
    
    }
}