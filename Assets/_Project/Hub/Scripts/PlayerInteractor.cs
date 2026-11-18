using UnityEngine;

namespace MovingIn.Hub{
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float radius = 1.6f;
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] InteractPromptUI promptUI;

        readonly Collider[] _hits = new Collider[32];
        MinigameInteractable _nearestMinigame;
        IInteractable _nearestOther;
        Collider _nearestCollider;
        InteractHighlight _highlighted;

        public bool HasTarget => _nearestCollider != null;

        void Update()
        {
            FindNearest();

            DialogueManager dm = DialogueManager.Instance;
            bool blocked = dm != null && dm.isActiveAndEnabled && dm.BlocksInteraction;

            if (promptUI != null)
            {
                if (_nearestCollider != null && !blocked) promptUI.Show(_nearestCollider, CurrentPrompt());
                else promptUI.Hide();
            }
            SetHighlight(_nearestMinigame != null && !blocked ? _nearestMinigame.GetComponent<InteractHighlight>() : null);

            if (_nearestCollider == null || blocked || Time.timeScale <= 0f) return;
            if (!Input.GetKeyDown(interactKey)) return;

            if (_nearestMinigame != null) _nearestMinigame.Interact();
            else _nearestOther?.Interact();
        }

        void OnDisable()
        {
            _nearestMinigame = null;
            _nearestOther = null;
            _nearestCollider = null;
            if (promptUI != null) promptUI.Hide();
            SetHighlight(null);
        }

        void SetHighlight(InteractHighlight target)
        {
            if (target == _highlighted) return;
            if (_highlighted != null) _highlighted.SetHighlighted(false);
            _highlighted = target;
            if (_highlighted != null) _highlighted.SetHighlighted(true);
        }

        string CurrentPrompt()
        {
            if (_nearestMinigame != null) return _nearestMinigame.Prompt;
            return _nearestOther != null ? _nearestOther.Prompt : "";
        }

        void FindNearest()
        {
            _nearestMinigame = null;
            _nearestOther = null;
            _nearestCollider = null;

            Vector3 pos = transform.position;
            int count = Physics.OverlapSphereNonAlloc(pos, radius, _hits, ~0, QueryTriggerInteraction.Collide);
            float best = float.MaxValue;
            int bestPriority = int.MaxValue;

            for (int i = 0; i < count; i++)
            {
                Collider col = _hits[i];
                if (col == null || col.transform.IsChildOf(transform)) continue;

                MinigameInteractable mi = col.GetComponentInParent<MinigameInteractable>();
                int priority = mi != null ? 0 : 1;
                IInteractable other = null;
                if (mi != null && mi.IsCompleted) mi = null;
                if (mi != null)
                {
                    MinigameGate gate = mi.GetComponent<MinigameGate>();
                    if (gate != null && gate.Locked) mi = null;
                }
                if (mi == null)
                {
                    other = col.GetComponentInParent<IInteractable>();
                    if (other != null && !other.CanInteract) other = null;
                }
                if (mi == null && other == null) continue;

                Vector3 p = col.bounds.ClosestPoint(pos);
                p.y = pos.y;
                float d = (p - pos).sqrMagnitude;
                if (priority > bestPriority || (priority == bestPriority && d >= best)) continue;

                best = d;
                bestPriority = priority;
                _nearestMinigame = mi;
                _nearestOther = other;
                _nearestCollider = col;
            }
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
