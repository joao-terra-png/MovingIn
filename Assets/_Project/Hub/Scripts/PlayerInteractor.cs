using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// procura o interagivel mais proximo do player,
    /// mostra "[E] Arrumar..." e chama Interact() quando aperta a tecla
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float _radius = 1.6f;
        [SerializeField] KeyCode _interactKey = KeyCode.E;

        readonly Collider[] _hits = new Collider[16];
        MinigameInteractable _nearest;
        GUIStyle _promptStyle;

        void Update()
        {
            _nearest = FindNearest();

            if (_nearest != null && Input.GetKeyDown(_interactKey))
                _nearest.Interact();
        }

        MinigameInteractable FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, _radius, _hits, ~0, QueryTriggerInteraction.Collide);

            MinigameInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                MinigameInteractable candidate = _hits[i].GetComponentInParent<MinigameInteractable>();
                if (candidate == null || candidate.IsCompleted) continue;

                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = candidate;
                }
            }
            return best;
        }

        // interface provisoria
        void OnGUI()
        {
            if (_nearest == null) return;

            if (_promptStyle == null)
                _promptStyle = new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter };

            Rect area = new Rect(Screen.width / 2f - 220f, Screen.height - 100f, 440f, 50f);
            GUI.Box(area, $"[{_interactKey}] {_nearest.Prompt}", _promptStyle);
        }

        // mostra o raio de alcance na Scene quando o Player esta selecionado
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}
