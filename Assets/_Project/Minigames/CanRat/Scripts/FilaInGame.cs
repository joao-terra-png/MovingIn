using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilaInGame : MonoBehaviour
{
    //Inicialização de uma fila vazia
    public IntelliFila fila;
    public GameObject teste;

    public bool testandoafila;
    void Start()
    {
        fila = gameObject.GetComponent<IntelliFila>();
        testandoafila = false;
    }

    void Update()
    {
        MenuFila();
    }

    void MenuFila()
    {
        GameObject CanoSelecionado = SelecionaAlvo.GetAlvo();
        teste = CanoSelecionado;
        if(Input.GetKeyDown(KeyCode.E))
            testandoafila = fila.IntelliEnque(CanoSelecionado);


    }
}
