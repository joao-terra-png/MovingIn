using System;
using UnityEngine;

// Escolha e compra de aliados.
public class LojaAliados : MonoBehaviour
{
    [SerializeField] private GerenciadorEnergia energia;

    public AliadoData AliadoSelecionado { get; private set; }
    public event Action<AliadoData> SelecaoMudou;
    public event Action EnergiaInsuficiente;

    public bool PodeComprar(AliadoData dados)
    {
        return dados != null && energia.TemSaldo(dados.custo);
    }

    public void SelecionarAliado(AliadoData dados)
    {
        if (!PodeComprar(dados))
        {
            EnergiaInsuficiente?.Invoke();
            return;
        }

        AliadoSelecionado = dados;
        SelecaoMudou?.Invoke(dados);
    }

    public void Cancelar()
    {
        AliadoSelecionado = null;
        SelecaoMudou?.Invoke(null);
    }

    public bool TentarComprar(AliadoData dados)
    {
        return dados != null && energia.TentarGastar(dados.custo);
    }
}
