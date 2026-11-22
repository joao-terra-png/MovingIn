
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class DefesaManager : MonoBehaviour
    {
        [Header("Gerenciador de energia")]
        [SerializeField] private EnergyManager energia;

        [Header("Porta")]
        [SerializeField] private GameObject portaAbertaVisual;
        [SerializeField] private GameObject portaFechadaVisual;

        [Header("Janela")]
        [SerializeField] private GameObject janelaAbertaVisual;
        [SerializeField] private GameObject janelaFechadaVisual;

        private bool portaFechada;
        private bool janelaFechada;

        private void Start()
        {
            if (energia == null)
            {
                Debug.LogError("DefesaManager: EnergyManager não configurado!");
                return;
            }

            energia.EnergiaAcabou += AbrirTodasProtecoes;

            AtualizarPorta();
            AtualizarJanela();

            energia.SetPortaFechada(false);
            energia.SetJanelaFechada(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                AlternarPorta();
            }

            if (Input.GetKeyDown(KeyCode.E))
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
