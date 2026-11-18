using UnityEngine;

namespace MovingIn.Hub{
    public class MinigameGate : MonoBehaviour, IInteractable
    {
        [SerializeField] MinigameInteractable minigame;

        bool _locked;
        string _line = "Agora não.";

        public bool Locked => _locked && minigame != null && !minigame.IsCompleted;
        public string Prompt => minigame != null ? minigame.Prompt : "";
        public bool CanInteract => isActiveAndEnabled && Locked;

        void Awake()
        {
            if (minigame == null) minigame = GetComponent<MinigameInteractable>();
        }

        public void SetLocked(bool locked, string line)
        {
            _locked = locked;
            if (!string.IsNullOrEmpty(line)) _line = line;
        }

        public void Interact()
        {
            DialogueManager dm = DialogueManager.Instance;
            if (dm != null) dm.Say(_line);
        }
    }
}
