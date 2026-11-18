using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class SlotsScreen : MonoBehaviour
    {
        [SerializeField] PauseMenu menu;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] TextMeshProUGUI feedback;
        [SerializeField] Button[] slotButtons;
        [SerializeField] TextMeshProUGUI[] slotLabels;
        [SerializeField] Button backButton;

        bool _saving;

        void Awake()
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                int slot = i;
                slotButtons[i].onClick.AddListener(() => Pick(slot));
            }
            backButton.onClick.AddListener(menu.Back);
        }

        public void Setup(bool saving)
        {
            _saving = saving;
            title.text = saving ? "Salvar jogo" : "Carregar jogo";
            feedback.text = "";
            Refresh();
        }

        void Refresh()
        {
            for (int i = 0; i < slotButtons.Length; i++)
            {
                SaveData data = SaveSystem.Peek(i);
                string name = i == SaveSystem.AutoSlot ? "Automático" : $"Slot {i}";
                slotLabels[i].text = $"{name}\n<size=70%><color=#6B5E4A>{SaveSystem.Describe(data)}</color></size>";
                slotButtons[i].gameObject.SetActive(!(_saving && i == SaveSystem.AutoSlot));
                slotButtons[i].interactable = _saving ? SaveSystem.CanSave : data != null;
            }
        }

        void Pick(int slot)
        {
            if (_saving)
            {
                bool ok = SaveSystem.Save(slot);
                feedback.text = ok ? $"Salvo no slot {slot}!" : "Não dá pra salvar agora.";
                Refresh();
                return;
            }
            if (!SaveSystem.Load(slot)) feedback.text = "Esse save não abriu.";
        }
    }
}
