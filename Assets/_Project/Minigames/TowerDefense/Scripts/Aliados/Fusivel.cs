using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    // Ataca sempre o primeiro inimigo da fila, se ele estiver no alcance.
    public class Fusivel : Aliado
    {
        private float proximoAtaque;
        private LineRenderer raio;
        private float fimDoRaio;

        protected override void AoInicializar()
        {
            CriarRaio();
        }

        private void Update()
        {
            if (!ativo) return;

            if (raio != null && raio.enabled && Time.time >= fimDoRaio)
                raio.enabled = false;

            if (Time.time < proximoAtaque) return;

            Inimigo alvo = ondas.AlvoAtual;
            if (alvo == null) return;

            Vector3 posicaoAlvo = alvo.transform.position;
            float distancia = Vector2.Distance(transform.position, alvo.transform.position);
            if (distancia > dados.alcance) return;

            MostrarRaio(posicaoAlvo);
            alvo.ReceberDano(dados.dano);
            proximoAtaque = Time.time + dados.intervaloAcao;
        }

        private void CriarRaio()
        {
            raio = gameObject.AddComponent<LineRenderer>();
            raio.positionCount = 2;
            raio.useWorldSpace = true;
            raio.startWidth = 0.07f;
            raio.endWidth = 0.03f;
            raio.sortingOrder = 8;

            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null) raio.material = new Material(shader);

            raio.startColor = dados.corDoRaio;
            raio.endColor = dados.corDoRaio;
            raio.enabled = false;
        }

        private void MostrarRaio(Vector3 destino)
        {
            if (raio == null) return;

            raio.SetPosition(0, transform.position);
            raio.SetPosition(1, destino);
            raio.enabled = true;
            fimDoRaio = Time.time + 0.08f;
        }
    }
}
