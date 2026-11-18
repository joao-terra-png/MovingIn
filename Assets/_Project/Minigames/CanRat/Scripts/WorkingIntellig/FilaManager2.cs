using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MovingIn.Minigames.CanRat
{
    /*
    Fila Manager2: Script para duas filas
    Escolhe a fila

    Com validação de fila
    */
    public class FilaManager2 : MonoBehaviour
    {
        //Final Cages pra validação de filas
        [SerializeField]GameObject []InicialCages;
        [SerializeField]GameObject []FinalCages;
        [SerializeField]IntelliFila []Filas;

        //Referencia para a IntelliFila
        [SerializeField] GameObject Inventario;

        int indiceFilaAtiva;

        [SerializeField] GameObject SuperiorManager; 



        void Awake()
        {
            indiceFilaAtiva = 0;
            Filas = new IntelliFila[2];
            for(int j = 0; j < 2; j++)
            {
                Filas[j] = new IntelliFila();
                Filas[j].SetInicialPos(InicialCages[j].transform.position + Vector3.right);        
                Filas[j].Inventario = this.Inventario;
            }
        }

        void Update()
        { 
            //Seleção de fila:
            if(Input.GetKeyDown(KeyCode.Alpha1))
                indiceFilaAtiva = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                indiceFilaAtiva = 1;


            if(!(FinalCages[0].GetComponent<ValidaFila>().IsEnabled() || 
                    FinalCages[1].GetComponent<ValidaFila>().IsEnabled()))
                SuperiorManager.GetComponent<FinishGameEvent>().FinishGame();
        }

        public IntelliFila GetFilaSelecionada()
        {
            return Filas[indiceFilaAtiva];
        } 
        public bool EnableFilaSelecionada()
        {
            return FinalCages[indiceFilaAtiva].GetComponent<ValidaFila>().IsEnabled();
        }

    }
}