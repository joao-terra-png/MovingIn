using System.Text;
using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    public class KitchenUI : MonoBehaviour
    {
        [SerializeField] KitchenManager manager;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text noteText;
        [SerializeField] TMP_Text noteTextRight;
        [SerializeField] GameObject victoryPanel;
        [SerializeField] TMP_Text finalScoreText;
        [SerializeField] string _doneColor = "#4A5A62";

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
            int total = 0;
            manager.ForEachNoteEntry((item, done) => total++);
            int half = noteTextRight != null ? (total + 1) / 2 : total;

            var left = new StringBuilder();
            var right = new StringBuilder();
            int index = 0;

            manager.ForEachNoteEntry((item, done) =>
            {
                string line = done
                    ? $"<color={_doneColor}><s>{item.exhibitName}</s></color>"
                    : $"• {item.exhibitName}";
                (index < half ? left : right).AppendLine(line);
                index++;
            });

            noteText.text = left.ToString();
            if (noteTextRight != null) noteTextRight.text = right.ToString();
        }

        void ShowVictory()
        {
            victoryPanel.SetActive(true);
            finalScoreText.text = $"Pontos: {manager.Points}";
        }
    }
}