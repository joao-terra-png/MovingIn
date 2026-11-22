using UnityEngine;
using TMPro;

namespace MovingIn.Minigames.Onaf
{
    public class BonecoUI : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private Boneco boneco;
        [SerializeField] private TMP_Text textoComodo;

        private void Update()
        {
            if (boneco == null || gameManager == null)
                return;

            int id = boneco.GetIDRoom();

            textoComodo.text =
                "Boneco: " + gameManager.GetNomeComodo(id);
        }
    }
}