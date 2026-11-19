using MovingIn.Core;
using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    public class LaundryUI : MonoBehaviour
    {
        [SerializeField] LaundryManager manager;
        [SerializeField] MinigameScreens screens;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text phaseText;
        [SerializeField] TMP_Text pileText;
        [SerializeField] TMP_Text[] colorCountTexts = new TMP_Text[ClothNames.ColorCount];
        [SerializeField] TMP_Text[] typeCountTexts = new TMP_Text[ClothNames.TypeCount];
        [SerializeField] GameObject hintPanel;

        void OnEnable()
        {
            manager.PointsChanged += UpdatePoints;
            manager.PhaseChanged += UpdatePhase;
            manager.CountsChanged += UpdateCounts;
            manager.Victory += ShowVictory;
        }

        void OnDisable()
        {
            manager.PointsChanged -= UpdatePoints;
            manager.PhaseChanged -= UpdatePhase;
            manager.CountsChanged -= UpdateCounts;
            manager.Victory -= ShowVictory;
        }

        void Start()
        {
            UpdatePoints(manager.Points);
            UpdatePhase(manager.Phase);
            UpdateCounts();
        }

        void UpdatePoints(int points)
        {
            pointsText.text = screens != null ? $"Pontos: {points} / {screens.Goal}" : $"Pontos: {points}";
        }

        void UpdatePhase(LaundryPhase phase)
        {
            switch (phase)
            {
                case LaundryPhase.Wash: phaseText.text = "Coloque cada roupa na máquina da cor certa"; break;
                case LaundryPhase.Washing: phaseText.text = "Lavando..."; break;
                case LaundryPhase.Store: phaseText.text = "Guarde cada roupa na gaveta do tipo certo"; break;
                default: phaseText.text = "Tudo lavado e guardado!"; break;
            }
            if (hintPanel != null) hintPanel.SetActive(phase == LaundryPhase.Store);
            UpdateCounts();
        }

        void UpdateCounts()
        {
            if (pileText != null) pileText.text = $"No cesto: {manager.PileRemaining}";

            for (int c = 0; c < colorCountTexts.Length; c++)
                if (colorCountTexts[c] != null)
                    colorCountTexts[c].text = $"{ClothNames.Of((ClothColor)c)}: {manager.CountByColor((ClothColor)c)}";

            for (int t = 0; t < typeCountTexts.Length; t++)
                if (typeCountTexts[t] != null)
                    typeCountTexts[t].text = $"{ClothNames.Of((ClothType)t)}: {manager.CountByType((ClothType)t)}";
        }

        void ShowVictory()
        {
            if (screens != null) screens.ShowResult(manager.Points);
        }
    }
}
