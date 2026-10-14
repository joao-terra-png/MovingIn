using System;
using UnityEngine;

// Saldo do jogador.
public class GerenciadorEnergia : MonoBehaviour
{
    [SerializeField] private int saldoInicial = 100;

    public int Saldo { get; private set; }
    public event Action<int> SaldoMudou;

    private void Awake()
    {
        Saldo = saldoInicial;
    }

    private void Start()
    {
        SaldoMudou?.Invoke(Saldo);
    }

    public void Adicionar(int valor)
    {
        if (valor <= 0) return;
        Saldo += valor;
        SaldoMudou?.Invoke(Saldo);
    }

    public bool TemSaldo(int valor)
    {
        return Saldo >= valor;
    }

    public bool TentarGastar(int valor)
    {
        if (!TemSaldo(valor)) return false;
        Saldo -= valor;
        SaldoMudou?.Invoke(Saldo);
        return true;
    }
}
