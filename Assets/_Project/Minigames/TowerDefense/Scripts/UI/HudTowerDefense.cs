using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MovingIn.Core;

namespace MovingIn.Minigames.TowerDefense
{
    // HUD do minigame.
    public class HudTowerDefense : MonoBehaviour
    {
        private class Carta
        {
            public AliadoData dados;
            public RectTransform rect;
            public Image fundo;
            public CanvasGroup grupo;
        }

        [Header("Sistemas (arraste o objeto Sistema e a torre)")]
        [SerializeField] private GerenciadorEnergia energia;
        [SerializeField] private TorreFinal torre;
        [SerializeField] private GerenciadorOndas ondas;
        [SerializeField] private GerenciadorTowerDefense partida;
        [SerializeField] private LojaAliados loja;

        [Header("Aliados da loja (até 4, na ordem das teclas 1 a 4)")]
        [SerializeField] private AliadoData[] aliadosDaLoja;

        [Header("Sprites do HUD (opcionais)")]
        [SerializeField] private Sprite iconeEnergia;
        [SerializeField] private Sprite iconeVida;
        [SerializeField] private Sprite moldura;

        private RectTransform raiz;
        private TMP_Text textoEnergia;
        private TMP_Text textoVida;
        private TMP_Text textoOnda;
        private TMP_Text textoMensagem;
        private TMP_Text tituloFinal;
        private TMP_Text subtituloFinal;
        private RectTransform preenchimentoVida;
        private Image preenchimentoVidaImagem;
        private GameObject painelFinal;
        private RectTransform botaoJogarDeNovo;
        private RectTransform botaoCasa;

        private readonly List<Carta> cartas = new List<Carta>();
        private AliadoData selecionado;
        private bool finalizada;
        private bool saindo;
        private Color corNormal;
        private Color corSelecionada;

        private static readonly Color Amarelo = new Color(1f, 0.88f, 0.3f);

        private void Awake()
        {
            corNormal = moldura != null ? Color.white : new Color(0.07f, 0.12f, 0.16f, 0.95f);
            corSelecionada = moldura != null ? new Color(0.6f, 1f, 1f, 1f) : new Color(0.1f, 0.35f, 0.45f, 1f);

            ConstruirCanvas();
            ConstruirBarraSuperior();
            ConstruirLoja();
            ConstruirMensagem();
            ConstruirPainelFinal();
        }

        private void ConstruirCanvas()
        {
            GameObject go = new GameObject("HudCanvas", typeof(RectTransform), typeof(Canvas),
                                           typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.SetParent(transform, false);

            Canvas canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler escala = go.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 0.5f;

            raiz = go.GetComponent<RectTransform>();
        }

        private void ConstruirBarraSuperior()
        {
            Image barra = CriarImagem("BarraSuperior", raiz, null, new Color(0.04f, 0.08f, 0.11f, 0.88f));
            RectTransform rb = barra.rectTransform;
            rb.anchorMin = new Vector2(0f, 1f);
            rb.anchorMax = new Vector2(1f, 1f);
            rb.pivot = new Vector2(0.5f, 1f);
            rb.anchoredPosition = Vector2.zero;
            rb.sizeDelta = new Vector2(0f, 90f);

            Vector2 esquerda = new Vector2(0f, 0.5f);
            Vector2 centro = new Vector2(0.5f, 0.5f);
            Vector2 direita = new Vector2(1f, 0.5f);

            // Energia
            Image iconeE = CriarImagem("IconeEnergia", barra.transform, iconeEnergia, iconeEnergia != null ? Color.white : Amarelo);
            iconeE.preserveAspect = true;
            Posicionar(iconeE.rectTransform, esquerda, new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(60f, 60f));

            textoEnergia = CriarTexto("TextoEnergia", barra.transform, "0", 46f, TextAlignmentOptions.MidlineLeft, Amarelo);
            Posicionar(textoEnergia.rectTransform, esquerda, new Vector2(0f, 0.5f), new Vector2(100f, 0f), new Vector2(300f, 70f));

            // Onda
            textoOnda = CriarTexto("TextoOnda", barra.transform, "Onda 0/0", 42f, TextAlignmentOptions.Center, Color.white);
            Posicionar(textoOnda.rectTransform, centro, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 70f));

            // Vida da torre
            textoVida = CriarTexto("TextoVida", barra.transform, "0/0", 40f, TextAlignmentOptions.MidlineRight, Color.white);
            Posicionar(textoVida.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-30f, 0f), new Vector2(140f, 70f));

            Image fundoBarra = CriarImagem("BarraVidaFundo", barra.transform, null, new Color(0f, 0f, 0f, 0.6f));
            Posicionar(fundoBarra.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-180f, 0f), new Vector2(300f, 30f));

            preenchimentoVidaImagem = CriarImagem("BarraVidaPreenchimento", fundoBarra.transform, null, new Color(0.2f, 0.9f, 0.3f));
            preenchimentoVida = preenchimentoVidaImagem.rectTransform;
            Esticar(preenchimentoVida);

            Image iconeV = CriarImagem("IconeVida", barra.transform, iconeVida, iconeVida != null ? Color.white : new Color(0.9f, 0.2f, 0.3f));
            iconeV.preserveAspect = true;
            Posicionar(iconeV.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-490f, 0f), new Vector2(56f, 56f));
        }

        private void ConstruirLoja()
        {
            const float larguraCarta = 170f;
            const float alturaCarta = 200f;
            const float espaco = 16f;

            int quantidade = aliadosDaLoja != null ? aliadosDaLoja.Length : 0;

            RectTransform lojaRaiz = CriarRect("Loja", raiz);
            Posicionar(lojaRaiz, Vector2.zero, Vector2.zero, new Vector2(20f, 20f),
                       new Vector2(Mathf.Max(1, quantidade) * (larguraCarta + espaco), alturaCarta));

            for (int i = 0; i < quantidade; i++)
            {
                AliadoData dados = aliadosDaLoja[i];
                if (dados == null) continue;

                Image fundo = CriarImagem("Carta_" + dados.nome, lojaRaiz, moldura, corNormal);
                fundo.raycastTarget = true;
                Posicionar(fundo.rectTransform, new Vector2(0f, 0.5f), new Vector2(0.5f, 0.5f),
                           new Vector2(i * (larguraCarta + espaco) + larguraCarta / 2f, 0f),
                           new Vector2(larguraCarta, alturaCarta));

                CanvasGroup grupo = fundo.gameObject.AddComponent<CanvasGroup>();
                Button botao = fundo.gameObject.AddComponent<Button>();
                botao.targetGraphic = fundo;
                AliadoData alvo = dados;
                botao.onClick.AddListener(() => loja.SelecionarAliado(alvo));

                Vector2 baseInferior = new Vector2(0.5f, 0f);

                Image icone = CriarImagem("Icone", fundo.transform, dados.icone, dados.cor);
                icone.preserveAspect = true;
                icone.enabled = dados.icone != null;
                Posicionar(icone.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -12f), new Vector2(100f, 100f));

                TMP_Text nome = CriarTexto("Nome", fundo.transform, dados.nome, 26f, TextAlignmentOptions.Center, Color.white, true, 14f);
                Posicionar(nome.rectTransform, baseInferior, new Vector2(0.5f, 0.5f), new Vector2(0f, 64f), new Vector2(160f, 40f));

                TMP_Text custo = CriarTexto("Custo", fundo.transform, dados.custo.ToString(), 30f, TextAlignmentOptions.MidlineLeft, Amarelo);
                float larguraTexto = custo.GetPreferredValues().x;
                float larguraTotal = 34f + 6f + larguraTexto;
                float inicio = -larguraTotal / 2f;

                Image iconeCusto = CriarImagem("IconeCusto", fundo.transform, iconeEnergia, iconeEnergia != null ? Color.white : Amarelo);
                iconeCusto.preserveAspect = true;
                Posicionar(iconeCusto.rectTransform, baseInferior, new Vector2(0f, 0.5f), new Vector2(inicio, 22f), new Vector2(34f, 34f));
                Posicionar(custo.rectTransform, baseInferior, new Vector2(0f, 0.5f), new Vector2(inicio + 40f, 22f), new Vector2(larguraTexto + 6f, 36f));

                TMP_Text tecla = CriarTexto("Tecla", fundo.transform, (i + 1).ToString(), 24f, TextAlignmentOptions.TopLeft, new Color(0.7f, 0.8f, 0.85f));
                Posicionar(tecla.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(12f, -8f), new Vector2(40f, 34f));

                Carta carta = new Carta();
                carta.dados = dados;
                carta.rect = fundo.rectTransform;
                carta.fundo = fundo;
                carta.grupo = grupo;
                cartas.Add(carta);
            }
        }

        private void ConstruirMensagem()
        {
            textoMensagem = CriarTexto("Mensagem", raiz, "", 34f, TextAlignmentOptions.Center, Color.white);
            Posicionar(textoMensagem.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -105f), new Vector2(1500f, 50f));
        }

        private void ConstruirPainelFinal()
        {
            Image painel = CriarImagem("PainelFinal", raiz, null, new Color(0f, 0f, 0f, 0.75f));
            painel.raycastTarget = true;
            Esticar(painel.rectTransform);
            painelFinal = painel.gameObject;

            Vector2 meio = new Vector2(0.5f, 0.5f);

            tituloFinal = CriarTexto("Titulo", painel.transform, "", 110f, TextAlignmentOptions.Center, Color.white);
            Posicionar(tituloFinal.rectTransform, meio, meio, new Vector2(0f, 110f), new Vector2(1400f, 140f));

            subtituloFinal = CriarTexto("Subtitulo", painel.transform, "", 44f, TextAlignmentOptions.Center, Color.white);
            Posicionar(subtituloFinal.rectTransform, meio, meio, new Vector2(0f, 20f), new Vector2(1400f, 70f));

            botaoJogarDeNovo = CriarBotaoFinal("BotaoJogarDeNovo", "Jogar de novo", new Color(0.1f, 0.5f, 0.7f, 1f), painel.transform, Reiniciar);
            botaoCasa = CriarBotaoFinal("BotaoCasa", "Voltar para a casa", new Color(0.25f, 0.32f, 0.4f, 1f), painel.transform, VoltarParaCasa);

            painelFinal.SetActive(false);
        }

        private RectTransform CriarBotaoFinal(string nome, string rotulo, Color cor, Transform pai, System.Action acao)
        {
            Vector2 meio = new Vector2(0.5f, 0.5f);

            Image imagem = CriarImagem(nome, pai, null, cor);
            imagem.raycastTarget = true;
            Posicionar(imagem.rectTransform, meio, meio, new Vector2(0f, -90f), new Vector2(460f, 100f));

            Button botao = imagem.gameObject.AddComponent<Button>();
            botao.targetGraphic = imagem;
            botao.onClick.AddListener(() => acao());

            TMP_Text texto = CriarTexto("Rotulo", imagem.transform, rotulo, 38f, TextAlignmentOptions.Center, Color.white, true, 20f);
            Esticar(texto.rectTransform);

            return imagem.rectTransform;
        }

        private void OnEnable()
        {
            energia.SaldoMudou += AoSaldoMudar;
            torre.VidaMudou += AoVidaMudar;
            ondas.OndaIniciada += AoOndaIniciar;
            partida.Finalizado += AoFinalizar;
            loja.SelecaoMudou += AoSelecaoMudar;
            loja.EnergiaInsuficiente += AoEnergiaInsuficiente;
        }

        private void OnDisable()
        {
            energia.SaldoMudou -= AoSaldoMudar;
            torre.VidaMudou -= AoVidaMudar;
            ondas.OndaIniciada -= AoOndaIniciar;
            partida.Finalizado -= AoFinalizar;
            loja.SelecaoMudou -= AoSelecaoMudar;
            loja.EnergiaInsuficiente -= AoEnergiaInsuficiente;
        }

        private void Start()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            AoOndaIniciar(0, ondas.TotalOndas);
            AtualizarCartas(energia.Saldo);
        }

        private void Update()
        {
            if (finalizada) return;

            for (int i = 0; i < cartas.Count && i < 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                    loja.SelecionarAliado(cartas[i].dados);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (selecionado != null) loja.Cancelar();
                else Sair(false);
            }
        }

        private void AoSaldoMudar(int saldo)
        {
            textoEnergia.text = saldo.ToString();
            AtualizarCartas(saldo);
        }

        private void AoVidaMudar(int atual, int maxima)
        {
            textoVida.text = atual + "/" + maxima;

            float proporcao = maxima > 0 ? Mathf.Clamp01((float)atual / maxima) : 0f;
            preenchimentoVida.anchorMax = new Vector2(proporcao, 1f);
            preenchimentoVidaImagem.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.2f, 0.9f, 0.3f), proporcao);
        }

        private void AoOndaIniciar(int atual, int total)
        {
            textoOnda.text = "Onda " + atual + "/" + total;
        }

        private void AoSelecaoMudar(AliadoData dados)
        {
            selecionado = dados;
            AtualizarCartas(energia.Saldo);

            if (finalizada) return;

            CancelInvoke(nameof(LimparMensagem));
            textoMensagem.text = dados == null
                ? ""
                : "Colocando " + dados.nome + ": clique em um slot (botão direito cancela)";
        }

        private void AoEnergiaInsuficiente()
        {
            if (finalizada) return;

            textoMensagem.text = "Energia insuficiente!";
            CancelInvoke(nameof(LimparMensagem));
            Invoke(nameof(LimparMensagem), 2f);
        }

        private void LimparMensagem()
        {
            if (finalizada) return;
            textoMensagem.text = selecionado == null
                ? ""
                : "Colocando " + selecionado.nome + ": clique em um slot (botão direito cancela)";
        }

        private void AoFinalizar(bool venceu)
        {
            finalizada = true;
            CancelInvoke(nameof(LimparMensagem));
            textoMensagem.text = "";

            painelFinal.SetActive(true);
            tituloFinal.text = venceu ? "VITÓRIA!" : "DERROTA";
            tituloFinal.color = venceu ? new Color(0.3f, 1f, 0.5f) : new Color(1f, 0.35f, 0.35f);
            subtituloFinal.text = venceu ? "A energia foi estabilizada." : "O circuito falhou.";

            if (venceu)
            {
                botaoJogarDeNovo.gameObject.SetActive(false);
                botaoCasa.anchoredPosition = new Vector2(0f, -90f);
            }
            else
            {
                botaoJogarDeNovo.gameObject.SetActive(true);
                botaoJogarDeNovo.anchoredPosition = new Vector2(-250f, -90f);
                botaoCasa.anchoredPosition = new Vector2(250f, -90f);
            }
        }

        private void AtualizarCartas(int saldo)
        {
            foreach (Carta carta in cartas)
            {
                bool temSaldo = saldo >= carta.dados.custo;
                bool escolhida = carta.dados == selecionado;

                carta.grupo.alpha = temSaldo ? 1f : 0.5f;
                carta.fundo.color = escolhida ? corSelecionada : corNormal;
                carta.rect.localScale = escolhida ? Vector3.one * 1.06f : Vector3.one;
            }
        }

        public void VoltarParaCasa()
        {
            bool venceu = partida.Estado == EstadoPartida.Vitoria;
            Sair(venceu);
        }

        private void Sair(bool venceu)
        {
            if (saindo) return;
            saindo = true;

            if (!MinigameBridge.IsInsideHub)
            {
                Debug.Log("[HudTowerDefense] Modo teste (vitoria = " + venceu + "): recarregando a cena");
                SceneManager.LoadScene(gameObject.scene.name);
                return;
            }

            MinigameBridge.Finish(venceu);
        }

        public void Reiniciar()
        {
            if (saindo) return;
            saindo = true;

            if (!MinigameBridge.IsInsideHub)
            {
                SceneManager.LoadScene(gameObject.scene.name);
                return;
            }

            StartCoroutine(RecarregarDentroDoHub());
        }

        private IEnumerator RecarregarDentroDoHub()
        {
            Scene antiga = gameObject.scene;

            yield return SceneManager.LoadSceneAsync(antiga.name, LoadSceneMode.Additive);

            Scene nova = antiga;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene candidata = SceneManager.GetSceneAt(i);
                if (candidata.name == antiga.name && candidata != antiga)
                {
                    nova = candidata;
                    break;
                }
            }

            if (nova == antiga)
            {
                Debug.LogError("[HudTowerDefense] Não consegui recarregar a cena do minigame.");
                saindo = false;
                yield break;
            }

            SceneManager.SetActiveScene(nova);
            SceneManager.UnloadSceneAsync(antiga);
        }

        private static RectTransform CriarRect(string nome, Transform pai)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            return go.GetComponent<RectTransform>();
        }

        private static Image CriarImagem(string nome, Transform pai, Sprite sprite, Color cor)
        {
            RectTransform rt = CriarRect(nome, pai);
            Image imagem = rt.gameObject.AddComponent<Image>();
            imagem.sprite = sprite;
            imagem.color = cor;
            imagem.raycastTarget = false;
            return imagem;
        }

        private static TMP_Text CriarTexto(string nome, Transform pai, string texto, float tamanho,
                                           TextAlignmentOptions alinhamento, Color cor,
                                           bool ajustar = false, float tamanhoMinimo = 14f)
        {
            RectTransform rt = CriarRect(nome, pai);
            TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.text = texto;
            t.fontSize = tamanho;
            t.alignment = alinhamento;
            t.color = cor;
            t.raycastTarget = false;

            if (ajustar)
            {
                t.enableAutoSizing = true;
                t.fontSizeMin = tamanhoMinimo;
                t.fontSizeMax = tamanho;
            }
            return t;
        }

        private static void Posicionar(RectTransform rt, Vector2 ancora, Vector2 pivot, Vector2 posicao, Vector2 tamanho)
        {
            rt.anchorMin = ancora;
            rt.anchorMax = ancora;
            rt.pivot = pivot;
            rt.anchoredPosition = posicao;
            rt.sizeDelta = tamanho;
        }

        private static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
