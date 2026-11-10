using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace MovingIn.Minigames.CanRat
{
    public class SelecionaAlvo : MonoBehaviour
    {

        RaycastHit alvoMarcado;
        [SerializeField] static private GameObject canoSelecionado;

        bool ok; //variavel que retorna se houve atualização de direção
        Vector3 vetorDirecao;
        [SerializeField]GameObject SelectionBox;
        Vector3 BoxOrigPosition;
        float distanciaCam;
        void Start()
        {
            distanciaCam = 1.5f * Mathf.Abs(transform.position.z);  //distanciamax >= distancia da camera da parede
            vetorDirecao = distanciaCam * Vector3.forward;
            Physics.Raycast(transform.position, vetorDirecao, out alvoMarcado, distanciaCam);
            canoSelecionado = null;
            ok = false;   
            BoxOrigPosition = SelectionBox.transform.position;
        }
        static public GameObject GetAlvo()
        {
            return canoSelecionado;
        }

        void Update()
        {
            //Impede movimento seo jogo estiver pausado
            if(Time.timeScale != 0){
            ok = false;
            if(Input.GetKeyDown(KeyCode.W)){
                AtualizaDirecao(1.5f*Vector3.up, out ok);
            }
            else if(Input.GetKeyDown(KeyCode.S)){
                AtualizaDirecao(1.5f*Vector3.down, out ok);
            }
            else if(Input.GetKeyDown(KeyCode.A)){
                AtualizaDirecao(1.5f*Vector3.left, out ok);
            }
            else if(Input.GetKeyDown(KeyCode.D)){
                AtualizaDirecao(1.5f*Vector3.right, out ok);
             }
            //Seleciona/desseleciona o objeto selecionado no momento ao mudar a seleção(raio)
            if(canoSelecionado != null)
                DesmarcaAlvo(canoSelecionado);

            if(Physics.Raycast(transform.position, vetorDirecao, out alvoMarcado, distanciaCam))
                Debug.DrawLine(transform.position, alvoMarcado.point);

            MarcaAlvo(alvoMarcado);
            //Tira a seleção caso já não esteja mais selecionado
            if(canoSelecionado != null && canoSelecionado.GetComponent<PipeSelection>().IsSelected == false)
                canoSelecionado = null;
            }
        }

        void AtualizaDirecao(Vector3 vetorDeslocamento, out bool sucesso)
        {
            vetorDirecao += vetorDeslocamento;
            sucesso = true;
            AtualizaSelectionBox(vetorDeslocamento/1.5f);
        }

        void AtualizaSelectionBox(Vector3 vetorDeslocamento)
        {
            SelectionBox.transform.position += vetorDeslocamento;
        }

        void MarcaAlvo(RaycastHit alvo)
        {
            if(alvo.collider.gameObject != null && alvo.collider.gameObject.CompareTag("Backdrop") == false){
                //fazer o visual de seleção
                if(Input.GetKeyDown(KeyCode.Space)){
                    canoSelecionado = alvo.collider.gameObject;
                    canoSelecionado.GetComponent<PipeSelection>().SelectPipe();
                    canoSelecionado.GetComponent<PipeSelection>().MarkSelected();
                }
            }
        }

        void DesmarcaAlvo(GameObject canoSelecionado){
            if(ok == true && canoSelecionado.GetComponent<PipeSelection>().GetIsSelected() == true){
                canoSelecionado.GetComponent<PipeSelection>().SelectPipe();
                canoSelecionado.GetComponent<PipeSelection>().MarkSelected();
                }
        }

        public void ReajustaDirecao()
        {
            vetorDirecao = distanciaCam * Vector3.forward;
            SelectionBox.transform.position = BoxOrigPosition;
        }
    }
}