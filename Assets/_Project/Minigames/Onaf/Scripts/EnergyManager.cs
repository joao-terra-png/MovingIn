using System;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    // Energia.
    //  - Gasta sozinha (consumo base) e mais enquanto a câmera está ligada e a porta/janela estão fechadas.
    //  - Porta e janela fechadas ao mesmo tempo gastam um extra.
    //  - Ao zerar começa o apagão: câmera, porta e janela desligam e o jogador tem alguns segundos de
    //    tolerância. Quando eles acabam, é a derrota.
    public class EnergyManager : MonoBehaviour
    {
        [Header("Energia")]
        [SerializeField] private float energiaMaxima = 100f;

        [Header("Consumo por segundo")]
        [SerializeField] private float consumoBase = 0.05f;
        [SerializeField] private float consumoCamera = 0.10f;
        [SerializeField] private float consumoPorta = 0.15f;
        [SerializeField] private float consumoJanela = 0.15f;
        [Tooltip("Gasto extra quando a porta e a janela estão fechadas ao mesmo tempo.")]
        [SerializeField] private float consumoExtraAmbas = 0.10f;

        [Header("Apagão")]
        [Tooltip("Segundos que o jogador ainda tem depois que a energia zera.")]
        [SerializeField] private float toleranciaSemEnergia = 10f;

        private float energiaAtual;
        private float toleranciaRestante;
        private bool cameraLigada;
        private bool portaTrancada;
        private bool janelaTrancada;
        private bool emApagao;
        private bool parado;
        private int ultimoInteiro = -1;

        // Zerou: começa o apagão. Quem usa energia deve desligar (câmera, portas e janela abrem).
        public event Action EnergiaAcabou;

        // Acabou a tolerância do apagão: derrota.
        public event Action ToleranciaEsgotada;

        // (atual, máxima). Só dispara quando o valor inteiro muda, então dá para ligar direto na UI.
        public event Action<float, float> EnergiaMudou;

        public bool CameraAtiva => cameraLigada;
        public bool PortaFechada => portaTrancada;
        public bool JanelaFechada => janelaTrancada;
        public bool EmApagao => emApagao;

        // true enquanto o jogador está gastando energia além do consumo base (câmera, porta ou janela).
        public bool IsSpendingEnergy => !parado && !emApagao && (cameraLigada || portaTrancada || janelaTrancada);

        private void Awake()
        {
            energiaAtual = energiaMaxima;
            toleranciaRestante = toleranciaSemEnergia;
        }

        private void Start()
        {
            NotificarMudanca(true);
        }

        private void Update()
        {
            if (parado) return;

            if (emApagao)
            {
                toleranciaRestante -= Time.deltaTime;

                if (toleranciaRestante <= 0f)
                {
                    toleranciaRestante = 0f;
                    parado = true;
                    ToleranciaEsgotada?.Invoke();
                }
                return;
            }

            energiaAtual = Mathf.Max(0f, energiaAtual - GetConsumoPorSegundo() * Time.deltaTime);
            NotificarMudanca(false);

            if (energiaAtual <= 0f) IniciarApagao();
        }

        private void IniciarApagao()
        {
            if (emApagao) return;

            emApagao = true;
            energiaAtual = 0f;
            toleranciaRestante = toleranciaSemEnergia;

            cameraLigada = false;
            portaTrancada = false;
            janelaTrancada = false;

            NotificarMudanca(false);
            EnergiaAcabou?.Invoke();
        }

        private void NotificarMudanca(bool forcar)
        {
            int inteiro = Mathf.CeilToInt(energiaAtual);
            if (!forcar && inteiro == ultimoInteiro) return;

            ultimoInteiro = inteiro;
            EnergiaMudou?.Invoke(energiaAtual, energiaMaxima);
        }

        // Sem energia não liga nada: o Set...(true) é ignorado durante o apagão.
        public void SetCameraAtiva(bool ativa)
        {
            cameraLigada = ativa && TemEnergia();
        }

        public void SetPortaFechada(bool fechada)
        {
            portaTrancada = fechada && TemEnergia();
        }

        public void SetJanelaFechada(bool fechada)
        {
            janelaTrancada = fechada && TemEnergia();
        }

        // Fim da noite (vitória ou derrota): congela a energia.
        public void Parar()
        {
            parado = true;
        }

        public bool TemEnergia()
        {
            return !emApagao && energiaAtual > 0f;
        }

        public float GetEnergiaAtual()
        {
            return energiaAtual;
        }

        public float GetEnergiaMaxima()
        {
            return energiaMaxima;
        }

        // 0 a 100
        public float GetPercentual()
        {
            return energiaMaxima > 0f ? energiaAtual / energiaMaxima * 100f : 0f;
        }

        public float GetToleranciaRestante()
        {
            return emApagao ? toleranciaRestante : toleranciaSemEnergia;
        }

        public float GetConsumoPorSegundo()
        {
            if (parado || emApagao) return 0f;

            float consumo = consumoBase;

            if (cameraLigada) consumo += consumoCamera;
            if (portaTrancada) consumo += consumoPorta;
            if (janelaTrancada) consumo += consumoJanela;
            if (portaTrancada && janelaTrancada) consumo += consumoExtraAmbas;

            return consumo;
        }
    }
}
