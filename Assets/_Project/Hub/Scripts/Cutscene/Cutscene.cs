using System.Collections;
using UnityEngine;

namespace MovingIn.Hub{
    public static class Cutscene
    {
        public static bool Playing { get; set; }

        public static IEnumerator Move(Transform t, Vector3 to, float duration, bool easeIn = false, bool easeOut = true)
        {
            Vector3 from = t.position;
            float k = 0f;
            while (k < 1f)
            {
                k += Time.deltaTime / Mathf.Max(0.01f, duration);
                float e = Mathf.Clamp01(k);
                if (easeIn && easeOut) e = Mathf.SmoothStep(0f, 1f, e);
                else if (easeIn) e = e * e;
                else if (easeOut) e = 1f - (1f - e) * (1f - e);
                t.position = Vector3.Lerp(from, to, e);
                yield return null;
            }
            t.position = to;
        }

        public static IEnumerator Walk(Transform t, Vector3 to, float speed)
        {
            to.y = t.position.y;
            while ((t.position - to).sqrMagnitude > 0.0004f)
            {
                t.position = Vector3.MoveTowards(t.position, to, speed * Time.deltaTime);
                yield return null;
            }
        }

        public static IEnumerator Fade(CanvasGroup group, float target, float duration)
        {
            if (group == null) yield break;
            float start = group.alpha;
            float k = 0f;
            while (k < 1f)
            {
                k += Time.unscaledDeltaTime / Mathf.Max(0.01f, duration);
                group.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, k));
                yield return null;
            }
            group.alpha = target;
        }

        public static void SetBehaviours(Behaviour[] list, bool enabled)
        {
            if (list == null) return;
            foreach (Behaviour b in list)
                if (b != null) b.enabled = enabled;
        }

        public static void Teleport(Transform player, Vector3 position)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.position = position;
            if (cc != null) cc.enabled = true;
        }
    }
}
