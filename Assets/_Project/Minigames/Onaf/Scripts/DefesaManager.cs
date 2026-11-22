using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class DefesaManager : MonoBehaviour
    {
        [Header("Gerenciador de energia")]
        [SerializeField] private EnergyManager energia;

        [Header("Controles")]
        [SerializeField] private KeyCode teclaPorta = KeyCode.Q;
        [SerializeField] private KeyCode teclaJanela = KeyCode.E;
        [Tooltip("Com a câmera (tablet) ligada o jogador não está olhando o quarto, então não fecha porta nem janela.")]
        [SerializeField] private bool bloquearComCamera = true;

        [Header("Porta")]
        [SerializeField] private GameObject portaAbertaVisual;
        [SerializeField] private GameObject portaFechadaVisual;

        [Header("Janela")]
        [SerializeField] private GameObject janelaAbertaVisual;
        [SerializeField] private GameObject janelaFechadaVisual;

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

            if (portaFechadaVisual == null)
                portaFechadaVisual = CriarTabua("PortaFechada", portaPosicao, portaEscala, pai);

            if (janelaFechadaVisual == null)
                janelaFechadaVisual = CriarTabua("JanelaFechada", janelaPosicao, janelaEscala, pai);
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
