using UnityEngine;

namespace MovingIn.Hub{
    public class RoomCameraFollow : MonoBehaviour
    {
        [SerializeField] Transform player;
        [SerializeField] Transform cameraTransform;
        [SerializeField] float _distance = 30f;
        [SerializeField] float _height = 1.5f;
        [SerializeField] Vector2 _min = new Vector2(-1f, -8f);
        [SerializeField] Vector2 _max = new Vector2(22f, -2f);
        [SerializeField] float _smooth = 8f;

        void LateUpdate()
        {
            if (player == null || cameraTransform == null) return;

            Vector3 focus = new Vector3(
                Mathf.Clamp(player.position.x, _min.x, _max.x),
                transform.position.y + _height,
                Mathf.Clamp(player.position.z, _min.y, _max.y));

            Vector3 target = focus - cameraTransform.forward * _distance;
            float t = 1f - Mathf.Exp(-_smooth * Time.deltaTime);
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, target, t);
        }

        void OnDrawGizmosSelected()
        {
            float y = transform.position.y + 0.05f;
            Vector3 a = new Vector3(_min.x, y, _min.y);
            Vector3 b = new Vector3(_max.x, y, _min.y);
            Vector3 c = new Vector3(_max.x, y, _max.y);
            Vector3 d = new Vector3(_min.x, y, _max.y);
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
            Gizmos.DrawLine(b, c);
            Gizmos.DrawLine(c, d);
            Gizmos.DrawLine(d, a);
        }
    }
}
