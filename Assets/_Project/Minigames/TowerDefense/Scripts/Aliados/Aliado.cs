using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
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

            AplicarVisual();
            ativo = true;
            AoInicializar();
        }

        private void AplicarVisual()
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (dados.icone != null) sr.sprite = dados.icone;
                sr.color = dados.cor;
            }
            transform.localScale = Vector3.one * dados.escala;
        }

        protected virtual void AoInicializar() { }

        public void Desativar()
        {
            ativo = false;
        }
    }
}
