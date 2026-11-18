using System;
using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    // Só anda, recebe dano e avisa quando morre ou chega ao fim.
    // Quem tira o inimigo da fila é o GerenciadorOndas (dono da fila).
    public class Inimigo : MonoBehaviour
    {
        public event Action<Inimigo> Morreu;
        public event Action<Inimigo> ChegouAoFim;

        public InimigoData Dados { get; private set; }
        public int VidaAtual { get; private set; }

        private static Sprite spriteQuadrado;

        private Transform[] caminho;
        private int proximoPonto;
        private float velocidade;
        private bool ativo;

        private int vidaMaxima;
        private Transform barra;
        private SpriteRenderer barraSprite;
        private float larguraBarraLocal;

        private const float LarguraBarra = 0.8f;
        private const float AlturaBarra = 0.12f;

        public void Inicializar(InimigoData dados, Transform[] caminho, float velocidade)
        {
            if (caminho == null || caminho.Length < 2)
            {
                Debug.LogError("Inimigo: o caminho precisa de pelo menos 2 pontos.");
                return;
            }

            Dados = dados;
            VidaAtual = dados.vidaMaxima;
            vidaMaxima = Mathf.Max(1, dados.vidaMaxima);
            this.caminho = caminho;
            this.velocidade = velocidade;

            AplicarVisual(dados);
            CriarBarraDeVida();

            transform.position = caminho[0].position;
            proximoPonto = 1;
            ativo = true;
        }

        private void AplicarVisual(InimigoData dados)
        {
            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (dados.sprite != null) sr.sprite = dados.sprite;
                sr.color = dados.cor;
            }
            transform.localScale = Vector3.one * dados.escala;
        }

        private void CriarBarraDeVida()
        {
            if (spriteQuadrado == null)
            {
                Texture2D tex = Texture2D.whiteTexture;
                spriteQuadrado = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                                               new Vector2(0f, 0.5f), tex.width);
            }

            // Mantém a barra com tamanho fixo no mundo, mesmo com a escala do inimigo.
            float escala = Mathf.Max(0.01f, transform.localScale.x);
            larguraBarraLocal = LarguraBarra / escala;
            float altura = AlturaBarra / escala;
            Vector3 posicao = new Vector3(-larguraBarraLocal / 2f, 0.5f + 0.18f / escala, 0f);

            GameObject fundo = new GameObject("BarraVida_Fundo");
            fundo.transform.SetParent(transform, false);
            fundo.transform.localPosition = posicao;
            fundo.transform.localScale = new Vector3(larguraBarraLocal, altura, 1f);
            SpriteRenderer sf = fundo.AddComponent<SpriteRenderer>();
            sf.sprite = spriteQuadrado;
            sf.color = new Color(0f, 0f, 0f, 0.75f);
            sf.sortingOrder = 10;

            GameObject frente = new GameObject("BarraVida");
            frente.transform.SetParent(transform, false);
            frente.transform.localPosition = posicao;
            frente.transform.localScale = new Vector3(larguraBarraLocal, altura, 1f);
            barra = frente.transform;
            barraSprite = frente.AddComponent<SpriteRenderer>();
            barraSprite.sprite = spriteQuadrado;
            barraSprite.sortingOrder = 11;

            AtualizarBarra();
        }

        private void AtualizarBarra()
        {
            if (barra == null) return;

            float proporcao = Mathf.Clamp01((float)VidaAtual / vidaMaxima);
            Vector3 s = barra.localScale;
            s.x = larguraBarraLocal * proporcao;
            barra.localScale = s;
            barraSprite.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.9f, 0.3f), proporcao);
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
            AtualizarBarra();

            if (VidaAtual <= 0)
            {
                ativo = false;
                Morreu?.Invoke(this);
            }
        }
    }
}
