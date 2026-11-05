using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat
{
    public class FilaInGame : MonoBehaviour
    {
        IntelliFila currentFila;
        public bool enableCurrentFila;

        [SerializeField] UnityEvent ReajustaInventario = new UnityEvent();
        [SerializeField] GameObject CanoSelecionado;
        //Inicialização de uma fila vazia
        void Start()
        {
            CanoSelecionado = null;
            currentFila = gameObject.GetComponent<FilaManager1>().Fila;
            enableCurrentFila = gameObject.GetComponent<FilaManager1>().enableFila;

        }

        void Update()
        {
            if(currentFila != null && gameObject.GetComponent<FilaManager1>().Fila != currentFila)
                currentFila = gameObject.GetComponent<FilaManager1>().Fila;
            if(Time.timeScale != 0){
                MenuFila();
            }
        }

        void MenuFila()
        {
            CanoSelecionado = SelecionaAlvo.GetAlvo();
            if(Input.GetKeyDown(KeyCode.E) && enableCurrentFila)
            {
                if(currentFila.IntelliEnque(CanoSelecionado))
                {
                    Debug.Log($"Inserindo {CanoSelecionado.tag} na fila...");
                    //Desseleciona:
                    CanoSelecionado.GetComponent<PipeSelection>().SelectPipe();
                    //Reajusta a direção do Inventario
                    ReajustaInventario.Invoke();
                }
            }
            else if(Input.GetKeyDown(KeyCode.D))
            {
                if(currentFila.IntelliDeque())
                {
                    Debug.Log("Retirando objeto da fila...");
                    //Reajusta a direção do Inventario
                    ReajustaInventario.Invoke();
                }
            }
        }
    }
}