using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using MovingIn.Minigames.CanRat;

public class FilaInGame : MonoBehaviour
{
    //Inicialização de uma fila vazia
    IntelliFila Fila;
    public bool enableFila;
    public void DisableFila(){
        enableFila = false;
    }

    [SerializeField] UnityEvent ReajustaInventario = new UnityEvent();

    [SerializeField] GameObject CanoSelecionado;
    void Start()
    {
        Fila = gameObject.GetComponent<IntelliFila>();
        CanoSelecionado = null;
        enableFila = true;
    }

    void Update()
    {
        MenuFila();
    }

    void MenuFila()
    {
        CanoSelecionado = SelecionaAlvo.GetAlvo();
        if(Input.GetKeyDown(KeyCode.E) && enableFila){
            if(Fila.GetComponent<IntelliFila>().IntelliEnque(CanoSelecionado))
            {
                //Desseleciona:
                CanoSelecionado.GetComponent<PipeSelection>().SelectPipe();
                //Reajusta a direção do Inventario
                ReajustaInventario.Invoke();
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
        if(Fila.GetComponent<IntelliFila>().IntelliDeque())
            {
                //Reajusta a direção do Inventario
                ReajustaInventario.Invoke();
            }
        }
    }
}
