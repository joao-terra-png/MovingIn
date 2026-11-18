using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    // Gera energia em intervalos regulares.
    public class Bateria : Aliado
    {
        private float proximaGeracao;

        protected override void AoInicializar()
        {
            proximaGeracao = Time.time + dados.intervaloAcao;
        }

        private void Update()
        {
            if (!ativo || Time.time < proximaGeracao) return;

            energia.Adicionar(dados.energiaGerada);
            proximaGeracao = Time.time + dados.intervaloAcao;
        }
    }
}
