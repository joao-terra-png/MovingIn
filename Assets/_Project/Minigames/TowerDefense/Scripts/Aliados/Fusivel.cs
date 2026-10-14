using UnityEngine;

// Ataca sempre o primeiro inimigo da fila, se ele estiver no alcance.
public class Fusivel : Aliado
{
    private float proximoAtaque;

    private void Update()
    {
        if (!ativo || Time.time < proximoAtaque) return;

        Inimigo alvo = ondas.AlvoAtual;
        if (alvo == null) return;

        float distancia = Vector2.Distance(transform.position, alvo.transform.position);
        if (distancia > dados.alcance) return;

        alvo.ReceberDano(dados.dano);
        proximoAtaque = Time.time + dados.intervaloAcao;
    }
}
