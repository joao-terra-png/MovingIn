using UnityEngine;

namespace MovingIn.Hub{
    public class CommentInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] string _prompt = "Olhar";
        [SerializeField, TextArea] string[] _lines;
        [SerializeField] Emote _firstEmote = Emote.None;
        [SerializeField] bool _sayAllAtOnce;

        int _next;

        public string Prompt => _prompt;
        public bool CanInteract => isActiveAndEnabled && _lines != null && _lines.Length > 0;

        public void Interact()
        {
            DialogueManager dm = DialogueManager.Instance;
            if (!CanInteract) return;
            if (dm == null || !dm.isActiveAndEnabled)
            {
                Debug.LogWarning("[CommentInteractable] DialogueManager desligado ou inexistente. Rode MovingIn > Setup Dialogue.");
                return;
            }

            if (_sayAllAtOnce)
            {
                for (int i = 0; i < _lines.Length; i++)
                    dm.Say(_lines[i], i == 0 ? _firstEmote : Emote.None);
                return;
            }

            dm.Say(_lines[_next], _next == 0 ? _firstEmote : Emote.None);
            _next = (_next + 1) % _lines.Length;
        }
    }
}
