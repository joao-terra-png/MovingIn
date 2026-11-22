using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MovingIn.Core.Structures;

namespace MovingIn.Minigames.Onaf
{
    // Arruma os botões das câmeras como um MAPA DA ÁRVORE da casa, no canto inferior direito do tablet.
    // Vai no Canvas que fica dentro do CameraManager (o que tem os "Button Quarto", "Button Corredor"...).
    //   - posição de cada botão vem da árvore: da esquerda para a direita na ordem do percurso em ordem
    //     e de cima para baixo por nível (o quarto do jogador, a raiz, fica no topo);
    //   - os botões que você já tem (e o OnClick -> SwitchCamera) são reaproveitados;
    //   - cada botão ganha o nome do cômodo e o da câmera que está sendo vista fica destacado;
    //   - o Canvas passa a escalar com a tela (1920x1080) e a ter distância do plano maior que 0.
    public class MapaCamerasUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CameraManager cameraManager;

        [Header("Tamanho (em pixels da tela de referência 1920x1080)")]
        [SerializeField] private Vector2 tamanhoDoMapa = new Vector2(620f, 400f);
        [SerializeField] private Vector2 tamanhoDoBotao = new Vector2(96f, 42f);
        [Tooltip("Distância do canto inferior direito da tela.")]
        [SerializeField] private Vector2 margem = new Vector2(30f, 30f);

        private readonly Dictionary<int, Image> botoes = new Dictionary<int, Image>();

        private static readonly Color Normal = new Color(0.14f, 0.2f, 0.26f, 1f);
        private static readonly Color Selecionado = new Color(0.2f, 0.7f, 0.35f, 1f);

        private void Start()
        {
            if (gameManager == null)
                gameManager = OnafUtil.EncontrarNaCena<GameManager>(gameObject.scene);

            if (cameraManager == null)
                cameraManager = GetComponentInParent<CameraManager>();

            if (gameManager == null || gameManager.GetTree() == null)
            {
                Debug.LogError("MapaCamerasUI: GameManager ou árvore não encontrados.");
                return;
            }

            ConfigurarCanvas();
            Construir(gameManager.GetTree());
        }

        private void Update()
        {
            if (cameraManager == null)
                return;

            int atual = cameraManager.GetIDRoomAtual();

            foreach (KeyValuePair<int, Image> par in botoes)
                par.Value.color = par.Key == atual ? Selecionado : Normal;
        }

        private void ConfigurarCanvas()
        {
            Canvas canvas = GetComponent<Canvas>();

            // com distância 0 o Canvas fica dentro do plano de corte da câmera e não aparece
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.planeDistance < 1f)
                canvas.planeDistance = 10f;

            CanvasScaler escala = GetComponent<CanvasScaler>();

            if (escala != null)
            {
                escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                escala.referenceResolution = new Vector2(1920f, 1080f);
                escala.matchWidthOrHeight = 0.5f;
            }
        }

        private void Construir(Arvore arvore)
        {
            // botões existentes dentro deste Canvas: id do quarto -> Button
            Dictionary<int, Button> existentes = new Dictionary<int, Button>();

            foreach (Button botao in GetComponentsInChildren<Button>(true))
            {
                int id = OnafUtil.IdDoBotao(botao.name);

                if (id < 0)
                {
                    Debug.LogWarning("MapaCamerasUI: não reconheci o botão '" + botao.name + "'.", botao);
                    continue;
                }

                existentes[id] = botao;
            }

            List<QuartoNode> nos = new List<QuartoNode>();
            arvore.EmOrdem(q => nos.Add(q));

            if (nos.Count == 0)
                return;

            int maiorNivel = 0;
            foreach (QuartoNode no in nos)
                maiorNivel = Mathf.Max(maiorNivel, Nivel(no));

            // contêiner do mapa, no canto inferior direito
            RectTransform mapa = CriarRect("MapaCameras", transform);
            mapa.anchorMin = new Vector2(1f, 0f);
            mapa.anchorMax = new Vector2(1f, 0f);
            mapa.pivot = new Vector2(1f, 0f);
            mapa.anchoredPosition = new Vector2(-margem.x, margem.y);
            mapa.sizeDelta = tamanhoDoMapa;

            Image fundo = CriarImagem("Fundo", mapa, new Color(0f, 0f, 0f, 0.65f));
            Esticar(fundo.rectTransform);

            TMP_Text titulo = CriarTexto("Titulo", mapa, "CÂMERAS", 28f, TextAlignmentOptions.MidlineLeft, new Color(0.3f, 1f, 0.5f));
            Posicionar(titulo.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -8f), new Vector2(300f, 40f));

            // posição de cada nó: x pela ordem do percurso em ordem, y pelo nível na árvore
            float margemX = tamanhoDoBotao.x / 2f + 12f;
            float passoX = nos.Count > 1 ? (tamanhoDoMapa.x - 2f * margemX) / (nos.Count - 1) : 0f;
            float topo = 70f;
            float passoY = maiorNivel > 0
                ? (tamanhoDoMapa.y - topo - tamanhoDoBotao.y / 2f - 12f) / maiorNivel
                : 0f;

            Dictionary<int, Vector2> posicoes = new Dictionary<int, Vector2>();

            for (int i = 0; i < nos.Count; i++)
            {
                float x = margemX + i * passoX;
                float y = -(topo + Nivel(nos[i]) * passoY);
                posicoes[nos[i].GetID()] = new Vector2(x, y);
            }

            // linhas pai -> filho (criadas antes dos botões para ficarem atrás deles)
            foreach (QuartoNode no in nos)
            {
                QuartoNode pai = no.GetPai();

                if (pai != null)
                    CriarLinha(mapa, posicoes[pai.GetID()], posicoes[no.GetID()]);
            }

            // botões
            foreach (QuartoNode no in nos)
            {
                int id = no.GetID();

                if (!existentes.TryGetValue(id, out Button botao))
                {
                    Debug.LogWarning("MapaCamerasUI: não há botão para o quarto " + id + " (" + OnafUtil.NomeDoComodo(id) + ").");
                    continue;
                }

                RectTransform rt = (RectTransform)botao.transform;
                rt.SetParent(mapa, false);
                rt.anchorMin = new Vector2(0f, 1f);
                rt.anchorMax = new Vector2(0f, 1f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition3D = new Vector3(posicoes[id].x, posicoes[id].y, 0f);
                rt.sizeDelta = tamanhoDoBotao;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;

                Image imagem = botao.GetComponent<Image>();

                if (imagem != null)
                {
                    imagem.color = Normal;
                    botao.targetGraphic = imagem;
                    botoes[id] = imagem;
                }

                TMP_Text rotulo = botao.GetComponentInChildren<TMP_Text>(true);

                if (rotulo == null)
                {
                    rotulo = CriarTexto("Rotulo", rt, "", 20f, TextAlignmentOptions.Center, Color.white, true, 11f);
                    Esticar(rotulo.rectTransform);
                }

                rotulo.text = OnafUtil.NomeDoComodo(id);
                rotulo.raycastTarget = false;
            }
        }

        private int Nivel(QuartoNode no)
        {
            int nivel = 0;

            while (no != null && no.GetPai() != null)
            {
                nivel++;
                no = no.GetPai();
            }

            return nivel;
        }

        private void CriarLinha(RectTransform pai, Vector2 de, Vector2 ate)
        {
            Image linha = CriarImagem("Linha", pai, new Color(0.35f, 0.45f, 0.5f, 1f));
            RectTransform rt = linha.rectTransform;

            Vector2 delta = ate - de;

            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = de;
            rt.sizeDelta = new Vector2(delta.magnitude, 4f);
            rt.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg);
        }

        // ------------------------------------------------------------------ utilitários de UI

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
                                           bool ajustar = false, float tamanhoMinimo = 12f)
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
