using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MovingIn.Minigames.Onaf
{
    public class EnergiaUI : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private EnergyManager energia;
        [SerializeField] private Slider barraEnergia;
        [SerializeField] private TMP_Text textoEnergia;
        [SerializeField] private TMP_Text textoConsumo;

        private void Update()
        {
            if (energia == null)
                return;

            float atual = energia.GetEnergiaAtual();
            float maxima = energia.GetEnergiaMaxima();

            barraEnergia.value = atual / maxima * 100f;

            textoEnergia.text =
                "Energia: " + Mathf.CeilToInt(atual) + "%";

            textoConsumo.text =
                "Consumo: " + energia.GetConsumoPorSegundo().ToString("F2");
        }
    }
}