using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace MovingIn.Minigames.CanRat
{
    public class FilaInGame1 : MonoBehaviour
    {
        IntelliFila currentFila;
        public bool enableCurrentFila;

        [SerializeField] UnityEvent ReajustaInventario = new UnityEvent();
        [SerializeField] GameObject CanoSelecionado;
        GameObject TextureCanoSelec;
        GameObject prevCanoSelec; 

        [SerializeField] GameObject RecycleBox;
        //Inicialização de uma fila vazia
        void Start()
        {
            CanoSelecionado = null;
            TextureCanoSelec = null;
            currentFila = gameObject.GetComponent<FilaManager1>().GetFila();
            enableCurrentFila = gameObject.GetComponent<FilaManager1>().GetEnableFila();

        }

        void Update()
        {
            if(currentFila != null && gameObject.GetComponent<FilaManager1>().GetFila() != currentFila){
                currentFila = gameObject.GetComponent<FilaManager1>().GetFila();
            }
            enableCurrentFila = gameObject.GetComponent<FilaManager1>().GetEnableFila();
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