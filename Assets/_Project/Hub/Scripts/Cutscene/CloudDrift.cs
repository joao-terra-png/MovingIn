using UnityEngine;

namespace MovingIn.Hub{
    public class CloudDrift : MonoBehaviour
    {
        [SerializeField] Vector3 _direction = new Vector3(1f, 0f, -1f);
        [SerializeField] float _speed = 0.4f;
        [SerializeField] float _range = 30f;

        Vector3 _origin;

        void Start()
        {
            _origin = transform.position;
        }

        void Update()
        {
            Vector3 dir = _direction.normalized;
            float d = Mathf.Repeat(Time.time * _speed + transform.GetSiblingIndex() * 7f, _range) - _range / 2f;
            transform.position = _origin + dir * d;
        }
    }
}
