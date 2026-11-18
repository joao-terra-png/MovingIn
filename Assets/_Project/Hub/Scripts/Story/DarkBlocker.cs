using UnityEngine;

namespace MovingIn.Hub{
    public class DarkBlocker : MonoBehaviour
    {
        [SerializeField] string _line = "Tá escuro demais. Primeiro a luz.";
        [SerializeField] float _cooldown = 4f;

        float _lastTime = -99f;

        void OnTriggerEnter(Collider other)
        {
            DialogueManager dm = DialogueManager.Instance;
            if (dm == null || dm.PlayerSpeaker == null) return;
            Transform player = dm.PlayerSpeaker.transform;
            if (other.transform != player && !other.transform.IsChildOf(player)) return;
            if (Time.time - _lastTime < _cooldown || dm.IsBusy) return;
            _lastTime = Time.time;
            dm.Say(_line);
        }
    }
}
