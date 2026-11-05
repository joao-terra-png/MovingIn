using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat
{
    public class FilaInGame2 : MonoBehaviour
    {
        [SerializeField]IntelliFila currentFila;
        public bool enableCurrentFila;

        [SerializeField] UnityEvent ReajustaInventario = new UnityEvent();
        [SerializeField] GameObject CanoSelecionado;

        [SerializeField] GameObject LastEnqPipe;
        [SerializeField] GameObject FirstEnqdEmpty;

        //Inicialização de uma fila vazia
        void Start()
        {
            CanoSelecionado = null;
            currentFila = gameObject.GetComponent<FilaManager2>().GetFilaSelecionada();
            enableCurrentFila = gameObject.GetComponent<FilaManager2>().EnableFilaSelecionada();

        }

        void Update()
        {

            LastEnqPipe = currentFila.LastEnqPipe;
            FirstEnqdEmpty = currentFila.FirstEnqdEmpty;

            if(currentFila != null && 
                    gameObject.GetComponent<FilaManager2>().GetFilaSelecionada() != currentFila)
            {
                currentFila = gameObject.GetComponent<FilaManager2>().GetFilaSelecionada();
                enableCurrentFila = gameObject.GetComponent<FilaManager2>().EnableFilaSelecionada();
            }
            if(Time.timeScale != 0){
                MenuFila();
            }
        }

        void MenuFila()
        {
            CanoSelecionado = SelecionaAlvo.GetAlvo();
            if(Input.GetKeyDown(KeyCode.E) && enableCurrentFila)
            {
                Debug.Log($"Tentando inserir na fila {currentFila}");
                if(currentFila.IntelliEnque(CanoSelecionado) )
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
                Debug.Log($"Tentando remover da fila {currentFila}");
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