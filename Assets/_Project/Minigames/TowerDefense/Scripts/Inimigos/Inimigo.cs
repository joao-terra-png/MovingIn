using System;
using UnityEngine;

// Só anda, recebe dano e AVISA quando morre ou chega ao fim.
// Quem tira o inimigo da fila é o GerenciadorOndas (dono da fila).
public class Inimigo : MonoBehaviour
{
    public event Action<Inimigo> Morreu;
    public event Action<Inimigo> ChegouAoFim;

    public InimigoData Dados { get; private set; }
    public int VidaAtual { get; private set; }

    private Transform[] caminho;
    private int proximoPonto;
    private float velocidade;
    private bool ativo;

    public void Inicializar(InimigoData dados, Transform[] caminho, float velocidade)
    {
        if (caminho == null || caminho.Length < 2)
        {
            Debug.LogError("Inimigo: o caminho precisa de pelo menos 2 pontos.");
            return;
        }

        Dados = dados;
        VidaAtual = dados.vidaMaxima;
        this.caminho = caminho;
        this.velocidade = velocidade;

        transform.position = caminho[0].position;
        proximoPonto = 1;
        ativo = true;
    }

    private void Update()
    {
        if (!ativo) return;

        Vector3 destino = caminho[proximoPonto].position;
        destino.z = transform.position.z;

        transform.position = Vector3.MoveTowards(transform.position, destino, velocidade * Time.deltaTime);

        if ((transform.position - destino).sqrMagnitude < 0.0001f)
        {
            proximoPonto++;
            if (proximoPonto >= caminho.Length)
            {
                ativo = false;
                ChegouAoFim?.Invoke(this);
            }
        }
    }

    public void ReceberDano(int dano)
    {
        if (!ativo) return;

        VidaAtual -= dano;
        if (VidaAtual <= 0)
        {
            ativo = false;
            Morreu?.Invoke(this);
        }
    }
}
