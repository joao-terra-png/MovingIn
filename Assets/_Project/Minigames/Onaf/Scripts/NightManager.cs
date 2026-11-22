using System;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class NightManager : MonoBehaviour
    {
        [Header("Duração")]
        [SerializeField] private float duracaoNoite = 360f;

        private float tempoAtual;
        private bool noiteAtiva;
        private bool noiteFinalizada;

        public event Action NoiteTerminou;

        private void Update()
        {
            if (!noiteAtiva)
                return;

            tempoAtual += Time.deltaTime;

            if (tempoAtual >= duracaoNoite)
            {
                tempoAtual = duracaoNoite;

                noiteAtiva = false;
                noiteFinalizada = true;

                NoiteTerminou?.Invoke();
            }
        }

        public void IniciarNoite()
        {
            if (noiteFinalizada)
                return;

            tempoAtual = 0f;
            noiteAtiva = true;
        }

        public void Parar()
        {
            noiteAtiva = false;
        }

        public float GetTempoRestante()
        {
            return Mathf.Max(0f, duracaoNoite - tempoAtual);
        }

        public float GetProgresso()
        {
            if (duracaoNoite <= 0f)
                return 1f;

            return tempoAtual / duracaoNoite;
        }

        public string GetHoraAtual()
        {
            if (duracaoNoite <= 0f)
                return "6 AM";

            int hora = Mathf.FloorToInt(
                (tempoAtual / duracaoNoite) * 6f
            );

            if (hora >= 6)
                return "6 AM";

            if (hora == 0)
                return "12 AM";

            return hora + " AM";
        }

        public bool NoiteEstaAtiva()
        {
            return noiteAtiva;
        }
    }
}
