using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class DefesaManager : MonoBehaviour
    {
        [Header("Gerenciador de energia")]
        [SerializeField] private EnergyManager energia;

        [Header("Controles")]
        [SerializeField] private KeyCode teclaPorta = KeyCode.E;
        [SerializeField] private KeyCode teclaJanela = KeyCode.Q;
        [Tooltip("Com a câmera (tablet) ligada o jogador não está olhando o quarto, então não fecha porta nem janela.")]
        [SerializeField] private bool bloquearComCamera = true;

        [Header("Porta")]
        [SerializeField] private GameObject portaAbertaVisual;
        [SerializeField] private GameObject portaFechadaVisual;

        [Header("Janela")]
        [SerializeField] private GameObject janelaAbertaVisual;
        [SerializeField] private GameObject janelaFechadaVisual;

        [Header("Sprites da porta (opcional: se preencher, substitui o visual abaixo)")]
        [SerializeField] private Sprite portaAbertaSprite;
        [SerializeField] private Sprite portaFechadaSprite;
        [Tooltip("Centro do sprite, em coordenadas do mundo (parede leste do quarto).")]
        [SerializeField] private Vector3 portaSpritePosicao = new Vector3(9.85f, 21.1f, 4f);
        [Tooltip("Rotação do sprite. Y = 90 deixa o sprite virado para dentro do quarto (parede leste).")]
        [SerializeField] private Vector3 portaSpriteRotacao = new Vector3(0f, 90f, 0f);
        [Tooltip("Altura do sprite no jogo (a largura segue a proporção do PNG).")]
        [SerializeField] private float portaSpriteAltura = 2.2f;
        [Tooltip("Objetos originais da porta que devem sumir quando houver sprites (opcional).")]
        [SerializeField] private GameObject[] esconderComSpritesDaPorta;

        [Header("Sprites da janela (opcional: se preencher, substitui o visual abaixo)")]
        [SerializeField] private Sprite janelaAbertaSprite;
        [SerializeField] private Sprite janelaFechadaSprite;
        [Tooltip("Centro do sprite, em coordenadas do mundo (parede norte do quarto).")]
        [SerializeField] private Vector3 janelaSpritePosicao = new Vector3(6.8f, 21.7f, 7.83f);
        [Tooltip("Rotação do sprite. Y = 0 deixa o sprite virado para dentro do quarto (parede norte).")]
        [SerializeField] private Vector3 janelaSpriteRotacao = Vector3.zero;
        [Tooltip("Altura do sprite no jogo (a largura segue a proporção do PNG).")]
        [SerializeField] private float janelaSpriteAltura = 1.2f;
        [Tooltip("Objetos originais da janela que devem sumir quando houver sprites (opcional).")]
        [SerializeField] private GameObject[] esconderComSpritesDaJanela;

        [Header("Visual automático (usado se os 'Fechada' acima estiverem vazios)")]
        [Tooltip("Posição, em coordenadas do mundo, da tábua que tampa a porta do quarto.")]
        [SerializeField] private Vector3 portaPosicao = new Vector3(9.95f, 21.1f, 4f);
        [SerializeField] private Vector3 portaEscala = new Vector3(0.14f, 2.2f, 1.5f);
        [Tooltip("Posição, em coordenadas do mundo, da tábua que tampa a janela do quarto.")]
        [SerializeField] private Vector3 janelaPosicao = new Vector3(6.8f, 21.7f, 7.8f);
        [SerializeField] private Vector3 janelaEscala = new Vector3(1.8f, 1.4f, 0.1f);
        [SerializeField] private Color corMadeira = new Color(0.28f, 0.16f, 0.08f);

        private CameraManager cameraManager;

        private bool portaFechada;
        private bool janelaFechada;

        private void Start()
        {
            if (energia == null)
            {
                Debug.LogError("DefesaManager: EnergyManager não configurado!");
                return;
            }

            cameraManager = OnafUtil.EncontrarNaCena<CameraManager>(gameObject.scene);

            CriarVisuaisSeNecessario();

            energia.EnergiaAcabou += AbrirTodasProtecoes;

            AtualizarPorta();
            AtualizarJanela();

            energia.SetPortaFechada(false);
            energia.SetJanelaFechada(false);
        }

        private void Update()
        {
            if (bloquearComCamera && cameraManager != null && cameraManager.CameraEstaLigada())
                return;

            if (Input.GetKeyDown(teclaPorta))
            {
                AlternarPorta();
            }

            if (Input.GetKeyDown(teclaJanela))
            {
                AlternarJanela();
            }
        }

        public void AlternarPorta()
        {
            if (!portaFechada && (energia == null || !energia.TemEnergia()))
                return;

            portaFechada = !portaFechada;

            energia.SetPortaFechada(portaFechada);

            AtualizarPorta();
        }

        public void AlternarJanela()
        {
            if (!janelaFechada && (energia == null || !energia.TemEnergia()))
                return;

            janelaFechada = !janelaFechada;

            energia.SetJanelaFechada(janelaFechada);

            AtualizarJanela();
        }

        private void AtualizarPorta()
        {
            if (portaAbertaVisual != null)
                portaAbertaVisual.SetActive(!portaFechada);

            if (portaFechadaVisual != null)
                portaFechadaVisual.SetActive(portaFechada);
        }

        private void AtualizarJanela()
        {
            if (janelaAbertaVisual != null)
                janelaAbertaVisual.SetActive(!janelaFechada);

            if (janelaFechadaVisual != null)
                janelaFechadaVisual.SetActive(janelaFechada);
        }

        private void AbrirTodasProtecoes()
        {
            portaFechada = false;
            janelaFechada = false;

            AtualizarPorta();
            AtualizarJanela();
        }

        private void CriarVisuaisSeNecessario()
        {
            Transform pai = null;

            GameManager gm = GetComponent<GameManager>();
            if (gm == null)
                gm = OnafUtil.EncontrarNaCena<GameManager>(gameObject.scene);

            if (gm != null && gm.GetTree() != null)
            {
                Room quarto = gm.GetTree().GetRoom(60);

                if (quarto != null)
                    pai = quarto.transform;
            }

            // porta
            if (portaAbertaVisual == null && portaAbertaSprite != null)
                portaAbertaVisual = CriarSprite("PortaAberta", portaAbertaSprite, portaSpritePosicao, portaSpriteRotacao, portaSpriteAltura, pai);

            if (portaFechadaVisual == null)
            {
                portaFechadaVisual = portaFechadaSprite != null
                    ? CriarSprite("PortaFechada", portaFechadaSprite, portaSpritePosicao, portaSpriteRotacao, portaSpriteAltura, pai)
                    : CriarTabua("PortaFechada", portaPosicao, portaEscala, pai);
            }

            if (portaAbertaSprite != null || portaFechadaSprite != null)
                Esconder(esconderComSpritesDaPorta);

            // janela
            if (janelaAbertaVisual == null && janelaAbertaSprite != null)
                janelaAbertaVisual = CriarSprite("JanelaAberta", janelaAbertaSprite, janelaSpritePosicao, janelaSpriteRotacao, janelaSpriteAltura, pai);

            if (janelaFechadaVisual == null)
            {
                janelaFechadaVisual = janelaFechadaSprite != null
                    ? CriarSprite("JanelaFechada", janelaFechadaSprite, janelaSpritePosicao, janelaSpriteRotacao, janelaSpriteAltura, pai)
                    : CriarTabua("JanelaFechada", janelaPosicao, janelaEscala, pai);
            }

            if (janelaAbertaSprite != null || janelaFechadaSprite != null)
                Esconder(esconderComSpritesDaJanela);
        }

        private static void Esconder(GameObject[] objetos)
        {
            if (objetos == null)
                return;

            foreach (GameObject go in objetos)
            {
                if (go != null)
                    go.SetActive(false);
            }
        }

        private GameObject CriarSprite(string nome, Sprite sprite, Vector3 posicao, Vector3 rotacao, float altura, Transform pai)
        {
            GameObject go = new GameObject(nome);

            if (pai != null)
                go.transform.SetParent(pai, true);

            go.transform.position = posicao;
            go.transform.rotation = Quaternion.Euler(rotacao);

            float alturaDoSprite = sprite.bounds.size.y;
            float escala = alturaDoSprite > 0f ? altura / alturaDoSprite : 1f;
            go.transform.localScale = new Vector3(escala, escala, 1f);

            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            return go;
        }

        private GameObject CriarTabua(string nome, Vector3 posicao, Vector3 escala, Transform pai)
        {
            GameObject tabua = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tabua.name = nome;

            Collider colisor = tabua.GetComponent<Collider>();
            if (colisor != null)
                Destroy(colisor);

            if (pai != null)
                tabua.transform.SetParent(pai, true);

            tabua.transform.position = posicao;
            tabua.transform.localScale = escala;

            tabua.GetComponent<Renderer>().material.color = corMadeira;

            return tabua;
        }

        public KeyCode GetTeclaPorta()
        {
            return teclaPorta;
        }

        public KeyCode GetTeclaJanela()
        {
            return teclaJanela;
        }

        public bool PortaEstaFechada()
        {
            return portaFechada;
        }

        public bool JanelaEstaFechada()
        {
            return janelaFechada;
        }

        private void OnDestroy()
        {
            if (energia != null)
                energia.EnergiaAcabou -= AbrirTodasProtecoes;
        }
    }
}