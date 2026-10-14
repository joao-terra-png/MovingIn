using UnityEngine;

// Base de todos os aliados.
public abstract class Aliado : MonoBehaviour
{
    protected AliadoData dados;
    protected GerenciadorOndas ondas;
    protected GerenciadorEnergia energia;
    protected bool ativo;

    public void Inicializar(AliadoData dados, GerenciadorOndas ondas, GerenciadorEnergia energia)
    {
        this.dados = dados;
        this.ondas = ondas;
        this.energia = energia;
        ativo = true;
        AoInicializar();
    }

    protected virtual void AoInicializar() { }

    public void Desativar()
    {
        ativo = false;
    }
}