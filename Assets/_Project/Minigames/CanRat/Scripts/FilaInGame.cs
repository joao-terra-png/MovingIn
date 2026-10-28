using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.CanRat;

public class FilaInGame : MonoBehaviour
{
    //Inicialização de uma fila vazia
    public IntelliFila fila;

    public bool testandoafila;
    [SerializeField] GameObject CanoSelecionado;
    void Start()
    {
        fila = gameObject.GetComponent<IntelliFila>();
        testandoafila = false;
        CanoSelecionado = null;
    }

    void Update()
    {
        MenuFila();
    }

    void MenuFila()
    {
        CanoSelecionado = SelecionaAlvo.GetAlvo();
        if(Input.GetKeyDown(KeyCode.E)){
            if(fila.GetComponent<IntelliFila>().IntelliEnque(CanoSelecionado))
            {
                CanoSelecionado.GetComponent<PipeSelection>().SelectPipe();
                testandoafila = true;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
        if(fila.GetComponent<IntelliFila>().IntelliDeque())
            {
                testandoafila = false;
            }
        }
    }
}
