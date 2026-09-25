using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// camera isometrica fixa 
    /// nao segue o player: enquadra o comodo atual inteiro
    /// a ROTACAO e definida no Inspector (Transform), ex: (30, 45, 0)
    /// o RoomManager avisa quando o comodo muda
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class IsoCamera : MonoBehaviour
    {
        [Tooltip("Distancia da camera ate o centro do comodo (numa camera ortografica so precisa nao cortar nada)")]
        [SerializeField] float _distance = 30f;

        [Tooltip("Tempo da transicao entre comodos em segundos. 0 = corte seco")]
        [SerializeField] float _moveTime = 0.25f;

        Camera _cam;
        Vector3 _targetPosition;
        float _targetSize;
        Vector3 _velocity;
        float _sizeVelocity;

        void Awake()
        {
            _cam = GetComponent<Camera>();
            _cam.orthographic = true;
            _targetPosition = transform.position;
            _targetSize = _cam.orthographicSize;
        }

        // define o novo comodo como alvo (a camera desliza ate ele)
        public void MoveTo(Room room)
        {
            _targetPosition = room.FocusPoint - transform.forward * _distance;
            _targetSize = room.CameraSize;

            if (_moveTime <= 0f)
                Snap();
        }

        // vai pro comodo na hora, sem transicao (usado no inicio do jogo)
        public void SnapTo(Room room)
        {
            MoveTo(room);
            Snap();
        }

        void Snap()
        {
            transform.position = _targetPosition;
            _cam.orthographicSize = _targetSize;
            _velocity = Vector3.zero;
            _sizeVelocity = 0f;
        }

        void LateUpdate()
        {
            if (_moveTime <= 0f) return;

            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _velocity, _moveTime);
            _cam.orthographicSize = Mathf.SmoothDamp(_cam.orthographicSize, _targetSize, ref _sizeVelocity, _moveTime);
        }
    }
}
