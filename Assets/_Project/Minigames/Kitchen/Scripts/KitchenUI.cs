using System.Text;
using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen
{
    public class KitchenUI : MonoBehaviour
    {
        [SerializeField] KitchenManager manager;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text noteText;
        [SerializeField] GameObject victoryPanel;
        [SerializeField] TMP_Text finalScoreText;

        void OnEnable()
        {
            manager.PointsChanged += UpdatePoints;
            manager.NoteChanged += UpdateNote;
            manager.Victory += ShowVictory;
        }

        void OnDisable()
        {
            manager.PointsChanged -= UpdatePoints;
            manager.NoteChanged -= UpdateNote;
            manager.Victory -= ShowVictory;
        }

        void Start()
        {
            UpdatePoints(manager.Points);
            UpdateNote();
            victoryPanel.SetActive(false);
        }

        void UpdatePoints(int points) => pointsText.text = $"Pontos: {points}";

        void UpdateNote()
        {
            var sb = new StringBuilder();
            manager.ForEachNoteItem(item => sb.AppendLine("- " + item.exhibitName));
            noteText.text = sb.Length == 0 ? "Tudo guardado! <3" : sb.ToString();
        }

        void ShowVictory()
        {
            victoryPanel.SetActive(true);
            finalScoreText.text = $"Pontos: {manager.Points}";
        }

        public void BackHome()
        {
            Debug.Log("Voltar pra casa clicado");
        }
    }
}