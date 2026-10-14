using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelecionaAlvo : MonoBehaviour
{
    
    RaycastHit alvoMarcado;
    public GameObject canoSelecionado;
    bool ok; //variavel que retorna se houve atualização de direção
    Vector3 vetorDirecao;
    // Start is called before the first frame update
    void Start()
    {
    vetorDirecao = 2 *Vector3.forward;
    Physics.Raycast(transform.position, vetorDirecao, out alvoMarcado, 2f);

    ok = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        ok = false;
        if(Input.GetKeyDown(KeyCode.UpArrow)){
            AtualizaDirecao(2*Vector3.up, out ok);
        }
        else if(Input.GetKeyDown(KeyCode.DownArrow)){
            AtualizaDirecao(2*Vector3.down, out ok);
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow)){
            AtualizaDirecao(2*Vector3.left, out ok);
        }
        else if(Input.GetKeyDown(KeyCode.RightArrow)){
            AtualizaDirecao(2*Vector3.right, out ok);
         }
        //Seleciona/desseleciona o objeto selecionado no momento
        if(ok == true && canoSelecionado.GetComponent<SelectedPipe>().GetIsSelected() == true)
            canoSelecionado.GetComponent<SelectedPipe>().SelectPipe();
        
        if(Physics.Raycast(transform.position, vetorDirecao, out alvoMarcado, 2f))
            Debug.DrawLine(transform.position, alvoMarcado.point);
        
        MarcaAlvo(alvoMarcado);

    }

    void AtualizaDirecao(Vector3 vetorDeslocamento, out bool sucesso)
    {
        vetorDirecao += vetorDeslocamento;
        sucesso = true;
    }

    void MarcaAlvo(RaycastHit alvo)
    {
        if(alvo.collider.gameObject != null && alvo.collider.gameObject.CompareTag("Backdrop") == false){
            //fazer o visual de seleção
            if(Input.GetKeyDown(KeyCode.KeypadEnter)){
                canoSelecionado = alvo.collider.gameObject;
                canoSelecionado.GetComponent<SelectedPipe>().SelectPipe();
            }
        }
    }

}
