using System.Text;
using MovingIn.Core;
using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    ///<summary>
    /// interface: pontos e bilhete
    /// so ESCUTA os eventos do KitchenManager (nao conhece as regras)
    /// o resultado final (meta, "Tentar de novo") fica com o MinigameScreens
    /// </summary>
    public class KitchenUI : MonoBehaviour
    {
        [SerializeField] KitchenManager manager;
        [SerializeField] MinigameScreens screens;
        [SerializeField] TMP_Text pointsText;
        [SerializeField] TMP_Text noteText;

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
            // estado inicial: nada "mudou" ainda, entao desenha na mao
            UpdatePoints(manager.Points);
            UpdateNote();
        }

        void UpdatePoints(int points)
        {
            pointsText.text = screens != null ? $"Pontos: {points} / {screens.Goal}" : $"Pontos: {points}";
        }

        void UpdateNote()
        {
            var sb = new StringBuilder();
            manager.ForEachNoteItem(item => sb.AppendLine("- " + item.exhibitName));
            noteText.text = sb.Length == 0 ? "Tudo guardado! <3" : sb.ToString();
        }

        void ShowVictory()
        {
            if (screens != null) screens.ShowResult(manager.Points);
        }
    }
}
