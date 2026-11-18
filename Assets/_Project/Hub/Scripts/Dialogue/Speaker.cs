using UnityEngine;

namespace MovingIn.Hub{
    public class Speaker : MonoBehaviour
    {
        [SerializeField] string _displayName = "Garoto";
        [SerializeField] Vector3 _headOffset = new Vector3(0f, 0.45f, 0f);
        [SerializeField] float _voicePitch = 1.25f;
        [SerializeField] EmoteDisplay emoteDisplay;

        public string DisplayName => _displayName;
        public float VoicePitch => _voicePitch;
        public EmoteDisplay Emotes => emoteDisplay;
        public virtual Vector3 HeadPosition => transform.position + _headOffset;

        void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0.55f, 0.2f);
            Gizmos.DrawWireSphere(HeadPosition, 0.12f);
        }
    }
}
