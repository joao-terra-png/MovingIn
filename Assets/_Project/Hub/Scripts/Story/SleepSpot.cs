using UnityEngine;

namespace MovingIn.Hub{
    public class SleepSpot : MonoBehaviour, IInteractable
    {
        [SerializeField] string _prompt = "Dormir";

        public string Prompt => _prompt;
        public bool CanInteract => isActiveAndEnabled && StoryManager.Instance != null && StoryManager.Instance.CanSleep;

        public void Interact()
        {
            if (StoryManager.Instance != null) StoryManager.Instance.Sleep();
        }
    }
}
