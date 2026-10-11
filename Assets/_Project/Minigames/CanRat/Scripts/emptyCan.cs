using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
    Componente que atualiza e retorna se o Cano contém o Rato
    Em caso positivo, a operação Retira da fila é barrada (não acontece)
*/
public class emptyCan : MonoBehaviour
{
    private bool HasRat;
    void Start()
    {
        HasRat = false;
    }

    public bool CanState {
        get { return HasRat; }
        set { HasRat = value; } 
    }
}
