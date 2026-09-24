using UnityEngine;

namespace Mudanca.Hub
{
    /// <summary>
    /// Procura o interagível mais próximo do garoto, mostra "[E] Arrumar..."
    /// e chama Interact() quando o jogador aperta a tecla.
    /// Coloque este script no Player.
    /// </summary>
    public class PlayerInteractor : MonoBehaviour
    {
        [SerializeField] float radius = 1.6f;
        [SerializeField] KeyCode interactKey = KeyCode.E;

        readonly Collider[] hits = new Collider[16];
        MinigameInteractable nearest;
        GUIStyle promptStyle;

        void Update()
        {
            nearest = FindNearest();

            if (nearest != null && Input.GetKeyDown(interactKey))
                nearest.Interact();
        }

        MinigameInteractable FindNearest()
        {
            int count = Physics.OverlapSphereNonAlloc(
                transform.position, radius, hits, ~0, QueryTriggerInteraction.Collide);

            MinigameInteractable best = null;
            float bestDistance = float.MaxValue;

            for (int i = 0; i < count; i++)
            {
                MinigameInteractable candidate = hits[i].GetComponentInParent<MinigameInteractable>();
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

        // Interface provisória (sem precisar montar Canvas). Troquem por UI de verdade depois.
        void OnGUI()
        {
            if (nearest == null) return;

            if (promptStyle == null)
                promptStyle = new GUIStyle(GUI.skin.box) { fontSize = 22, alignment = TextAnchor.MiddleCenter };

            Rect area = new Rect(Screen.width / 2f - 220f, Screen.height - 100f, 440f, 50f);
            GUI.Box(area, $"[{interactKey}] {nearest.Prompt}", promptStyle);
        }

        // Mostra o raio de alcance na Scene view quando o Player está selecionado
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}
