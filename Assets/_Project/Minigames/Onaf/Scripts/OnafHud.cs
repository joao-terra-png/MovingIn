using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using MovingIn.Core;

namespace MovingIn.Minigames.Onaf
{
    // HUD da noite, montado por código (não precisa criar Canvas, textos nem botões na mão).
    //   Barra superior: hora, onde o jogador está olhando (quarto ou câmera) e energia.
    //   Canto inferior esquerdo: porta, janela e câmeras, com as teclas.
    //   Centro: contagem do apagão quando a energia acaba.
    public class OnafHud : MonoBehaviour
    {
        [Header("Sistemas (vazio = procura sozinho)")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EnergyManager energia;
        [SerializeField] private NightManager nightManager;
        [SerializeField] private DefesaManager defesaManager;
        [SerializeField] private CameraManager cameraManager;

        [Header("Fim da partida")]
        [Tooltip("Desligado: na derrota só aparece 'Tentar de novo' (o jogador precisa vencer a noite). Ligado: aparece também 'Voltar para a casa'.")]
        [SerializeField] private bool voltarParaCasaNaDerrota = false;
        [Tooltip("Desligado: o Esc não sai do minigame (não dá para ir embora antes de vencer). Ligado: Esc sai, como nos outros minigames.")]
        [SerializeField] private bool permitirSairComEsc = false;

        private RectTransform raiz;
        private TMP_Text textoHora;
        private TMP_Text textoLocal;
        private TMP_Text textoEnergia;
        private TMP_Text textoConsumo;
        private TMP_Text textoApagao;
        private TMP_Text textoPorta;
        private TMP_Text textoJanela;
        private TMP_Text textoCamera;
        private TMP_Text tituloFinal;
        private TMP_Text subtituloFinal;
        private RectTransform preenchimentoEnergia;
        private Image preenchimentoEnergiaImagem;
        private GameObject painelFinal;
        private RectTransform botaoDeNovo;
        private RectTransform botaoCasa;

        private bool finalizada;
        private bool venceuPartida;
        private bool saindo;

        private static readonly Color Verde = new Color(0.3f, 1f, 0.5f);
        private static readonly Color Vermelho = new Color(1f, 0.35f, 0.35f);
        private static readonly Color Amarelo = new Color(1f, 0.88f, 0.3f);

        private void Awake()
        {
            Scene cena = gameObject.scene;

            if (gameManager == null) gameManager = GetComponent<GameManager>();
            if (gameManager == null) gameManager = OnafUtil.EncontrarNaCena<GameManager>(cena);
            if (energia == null) energia = OnafUtil.EncontrarNaCena<EnergyManager>(cena);
            if (nightManager == null) nightManager = OnafUtil.EncontrarNaCena<NightManager>(cena);
            if (defesaManager == null) defesaManager = OnafUtil.EncontrarNaCena<DefesaManager>(cena);
            if (cameraManager == null) cameraManager = OnafUtil.EncontrarNaCena<CameraManager>(cena);

            ConstruirCanvas();
            ConstruirBarraSuperior();
            ConstruirPainelDeControles();
            ConstruirApagao();
            ConstruirPainelFinal();
        }

        private void ConstruirCanvas()
        {
            GameObject go = new GameObject("OnafHudCanvas", typeof(RectTransform), typeof(Canvas),
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
            Image barra = CriarImagem("BarraSuperior", raiz, new Color(0.03f, 0.05f, 0.07f, 0.78f));
            RectTransform rb = barra.rectTransform;
            rb.anchorMin = new Vector2(0f, 1f);
            rb.anchorMax = new Vector2(1f, 1f);
            rb.pivot = new Vector2(0.5f, 1f);
            rb.anchoredPosition = Vector2.zero;
            rb.sizeDelta = new Vector2(0f, 100f);

            Vector2 esquerda = new Vector2(0f, 0.5f);
            Vector2 centro = new Vector2(0.5f, 0.5f);
            Vector2 direita = new Vector2(1f, 0.5f);

            // hora (esquerda)
            textoHora = CriarTexto("Hora", barra.transform, "12 AM", 60f, TextAlignmentOptions.MidlineLeft, Color.white);
            Posicionar(textoHora.rectTransform, esquerda, new Vector2(0f, 0.5f), new Vector2(30f, 0f), new Vector2(300f, 80f));

            // onde o jogador está olhando (centro)
            textoLocal = CriarTexto("Local", barra.transform, "SEU QUARTO", 38f, TextAlignmentOptions.Center, Color.white);
            Posicionar(textoLocal.rectTransform, centro, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(760f, 60f));

            // energia (direita): texto, barra e consumo
            textoEnergia = CriarTexto("Energia", barra.transform, "ENERGIA 100%", 36f, TextAlignmentOptions.MidlineRight, Amarelo);
            Posicionar(textoEnergia.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-30f, 22f), new Vector2(420f, 44f));

            Image fundoBarra = CriarImagem("EnergiaFundo", barra.transform, new Color(0f, 0f, 0f, 0.6f));
            Posicionar(fundoBarra.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-30f, -12f), new Vector2(360f, 18f));

            preenchimentoEnergiaImagem = CriarImagem("EnergiaPreenchimento", fundoBarra.transform, new Color(0.3f, 0.9f, 0.4f));
            preenchimentoEnergia = preenchimentoEnergiaImagem.rectTransform;
            Esticar(preenchimentoEnergia);

            textoConsumo = CriarTexto("Consumo", barra.transform, "consumo 0.00/s", 20f, TextAlignmentOptions.MidlineRight, new Color(0.7f, 0.8f, 0.85f));
            Posicionar(textoConsumo.rectTransform, direita, new Vector2(1f, 0.5f), new Vector2(-30f, -36f), new Vector2(360f, 26f));
        }

        private void ConstruirPainelDeControles()
        {
            Image painel = CriarImagem("Controles", raiz, new Color(0f, 0f, 0f, 0.5f));
            Posicionar(painel.rectTransform, Vector2.zero, Vector2.zero, new Vector2(20f, 20f), new Vector2(620f, 160f));

            Vector2 topoEsquerda = new Vector2(0f, 1f);

            textoPorta = CriarTexto("Porta", painel.transform, "[E] PORTA: ABERTA", 32f, TextAlignmentOptions.MidlineLeft, Color.white);
            Posicionar(textoPorta.rectTransform, topoEsquerda, topoEsquerda, new Vector2(20f, -12f), new Vector2(580f, 44f));

            textoJanela = CriarTexto("Janela", painel.transform, "[Q] JANELA: ABERTA", 32f, TextAlignmentOptions.MidlineLeft, Color.white);
            Posicionar(textoJanela.rectTransform, topoEsquerda, topoEsquerda, new Vector2(20f, -58f), new Vector2(580f, 44f));

            textoCamera = CriarTexto("Camera", painel.transform, "[Espaço] ABRIR AS CÂMERAS", 32f, TextAlignmentOptions.MidlineLeft, new Color(0.6f, 0.85f, 1f));
            Posicionar(textoCamera.rectTransform, topoEsquerda, topoEsquerda, new Vector2(20f, -104f), new Vector2(580f, 44f));
        }

        private void ConstruirApagao()
        {
            textoApagao = CriarTexto("Apagao", raiz, "", 64f, TextAlignmentOptions.Center, Vermelho);
            Posicionar(textoApagao.rectTransform, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -130f), new Vector2(1000f, 90f));
        }

        private void ConstruirPainelFinal()
        {
            Image painel = CriarImagem("PainelFinal", raiz, new Color(0f, 0f, 0f, 0.78f));
            painel.raycastTarget = true;
            Esticar(painel.rectTransform);
            painelFinal = painel.gameObject;

            Vector2 meio = new Vector2(0.5f, 0.5f);

            tituloFinal = CriarTexto("Titulo", painel.transform, "", 110f, TextAlignmentOptions.Center, Color.white);
            Posicionar(tituloFinal.rectTransform, meio, meio, new Vector2(0f, 110f), new Vector2(1400f, 140f));

            subtituloFinal = CriarTexto("Subtitulo", painel.transform, "", 44f, TextAlignmentOptions.Center, Color.white);
            Posicionar(subtituloFinal.rectTransform, meio, meio, new Vector2(0f, 20f), new Vector2(1400f, 70f));

            botaoDeNovo = CriarBotaoFinal("BotaoTentarDeNovo", "Tentar de novo", new Color(0.1f, 0.5f, 0.7f, 1f), painel.transform, Reiniciar);
            botaoCasa = CriarBotaoFinal("BotaoCasa", "Voltar para a casa", new Color(0.25f, 0.32f, 0.4f, 1f), painel.transform, VoltarParaCasa);

            painelFinal.SetActive(false);
        }

        private RectTransform CriarBotaoFinal(string nome, string rotulo, Color cor, Transform pai, System.Action acao)
        {
            Vector2 meio = new Vector2(0.5f, 0.5f);

            Image imagem = CriarImagem(nome, pai, cor);
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
            if (gameManager != null)
                gameManager.PartidaTerminou += AoPartidaTerminar;
        }

        private void OnDisable()
        {
            if (gameManager != null)
                gameManager.PartidaTerminou -= AoPartidaTerminar;
        }

        private void Start()
        {
            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

            AtualizarTextos();
        }

        private void Update()
        {
            AtualizarTextos();

            if (permitirSairComEsc && Input.GetKeyDown(KeyCode.Escape) && !finalizada)
                Sair(false);
        }

        private void AtualizarTextos()
        {
            if (nightManager != null)
                Definir(textoHora, nightManager.GetHoraAtual());

            if (energia != null)
            {
                float percentual = energia.GetPercentual();
                Definir(textoEnergia, "ENERGIA " + Mathf.CeilToInt(percentual) + "%");
                Definir(textoConsumo, "consumo " + energia.GetConsumoPorSegundo().ToString("0.00") + "/s");

                float proporcao = Mathf.Clamp01(percentual / 100f);
                preenchimentoEnergia.anchorMax = new Vector2(proporcao, 1f);
                preenchimentoEnergiaImagem.color = Color.Lerp(new Color(0.9f, 0.2f, 0.2f), new Color(0.3f, 0.9f, 0.4f), proporcao);

                if (energia.EmApagao && !finalizada)
                    Definir(textoApagao, "APAGÃO! " + energia.GetToleranciaRestante().ToString("0.0") + "s");
                else
                    Definir(textoApagao, "");
            }

            bool cameraLigada = cameraManager != null && cameraManager.CameraEstaLigada();

            if (cameraManager != null)
            {
                Definir(textoLocal, cameraLigada
                    ? "CÂMERA - " + OnafUtil.NomeDoComodo(cameraManager.GetIDRoomAtual())
                    : "SEU QUARTO");

                Definir(textoCamera, cameraLigada
                    ? "[Espaço] VOLTAR PARA O QUARTO"
                    : "[Espaço] ABRIR AS CÂMERAS");
            }

            if (defesaManager != null)
            {
                bool porta = defesaManager.PortaEstaFechada();
                bool janela = defesaManager.JanelaEstaFechada();

                Definir(textoPorta, "[E] PORTA: " + (porta ? "FECHADA" : "ABERTA"));
                Definir(textoJanela, "[Q] JANELA: " + (janela ? "FECHADA" : "ABERTA"));

                textoPorta.color = porta ? Verde : Color.white;
                textoJanela.color = janela ? Verde : Color.white;
            }
        }

        private void AoPartidaTerminar(bool venceu)
        {
            finalizada = true;
            venceuPartida = venceu;
            Definir(textoApagao, "");

            painelFinal.SetActive(true);
            tituloFinal.text = venceu ? "VOCÊ SOBREVIVEU!" : "VOCÊ FOI PEGO";
            tituloFinal.color = venceu ? Verde : Vermelho;
            subtituloFinal.text = venceu ? "Amanheceu: 6 AM." : "Sobreviva até as 6 AM para sair.";

            if (venceu)
            {
                botaoDeNovo.gameObject.SetActive(false);
                botaoCasa.gameObject.SetActive(true);
                botaoCasa.anchoredPosition = new Vector2(0f, -90f);
            }
            else if (voltarParaCasaNaDerrota)
            {
                botaoDeNovo.gameObject.SetActive(true);
                botaoCasa.gameObject.SetActive(true);
                botaoDeNovo.anchoredPosition = new Vector2(-250f, -90f);
                botaoCasa.anchoredPosition = new Vector2(250f, -90f);
            }
            else
            {
                botaoDeNovo.gameObject.SetActive(true);
                botaoCasa.gameObject.SetActive(false);
                botaoDeNovo.anchoredPosition = new Vector2(0f, -90f);
            }
        }

        public void VoltarParaCasa()
        {
            Sair(venceuPartida);
        }

        private void Sair(bool venceu)
        {
            if (saindo) return;
            saindo = true;

            if (!MinigameBridge.IsInsideHub)
            {
                Debug.Log("[OnafHud] Modo teste (vitoria = " + venceu + "): recarregando a cena");
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
                Debug.LogError("[OnafHud] Não consegui recarregar a cena do minigame.");
                saindo = false;
                yield break;
            }

            SceneManager.SetActiveScene(nova);
            SceneManager.UnloadSceneAsync(antiga);
        }

        private static void Definir(TMP_Text campo, string texto)
        {
            if (campo != null && campo.text != texto)
                campo.text = texto;
        }

        private static RectTransform CriarRect(string nome, Transform pai)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);
            return go.GetComponent<RectTransform>();
        }

        private static Image CriarImagem(string nome, Transform pai, Color cor)
        {
            RectTransform rt = CriarRect(nome, pai);
            Image imagem = rt.gameObject.AddComponent<Image>();
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
