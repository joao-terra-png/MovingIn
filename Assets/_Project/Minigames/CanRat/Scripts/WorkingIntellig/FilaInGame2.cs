using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
        GameObject TextureCanoSelec;
        GameObject prevCanoSelec; 

        [SerializeField] GameObject LastEnqPipe;
        [SerializeField] GameObject FirstEnqdEmpty;

        [SerializeField] GameObject RecycleBox;

        //Inicialização de uma fila vazia
        void Start()
        {
            CanoSelecionado = null;
            TextureCanoSelec = null;
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
            enableCurrentFila = gameObject.GetComponent<FilaManager2>().EnableFilaSelecionada();

            if(Time.timeScale != 0){
                MenuFila();
            }


        }

        void MenuFila()
        {
            CanoSelecionado = SelecionaAlvo.GetAlvo();
            //Mostra o cano selecionado na fila
            if(CanoSelecionado == null && TextureCanoSelec != null){
                //Destroi a previa
                Destroy(prevCanoSelec);
                TextureCanoSelec = null;
            }
            else if (CanoSelecionado != null && 
                CanoSelecionado.transform.childCount > 0 &&
                CanoSelecionado.transform.GetChild(0).gameObject != TextureCanoSelec)
            {
                //Atribui a textura à variavel e salva sua posição original
                TextureCanoSelec = CanoSelecionado.transform.GetChild(0).gameObject;
                prevCanoSelec = Instantiate(TextureCanoSelec);

            }

            if(TextureCanoSelec != null)
            {
                float tempo = 0;    
                tempo += Time.time;
                Color cor = prevCanoSelec.GetComponent<Renderer>().material.color;
                cor.a = Mathf.Abs(Mathf.Sin(tempo*1.5f));
                prevCanoSelec.GetComponent<Renderer>().material.color = cor;
                prevCanoSelec.transform.position = currentFila.NextPipePos + 
                Vector3.back * CanoSelecionado.transform.position.z;
            }

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
            else if(RecycleBox.GetComponent<PipeSelection>().GetIsSelected())
            {
                RecycleBox.GetComponent<PipeSelection>().SelectPipe();
                RecycleBox.GetComponent<PipeSelection>().MarkSelected();

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