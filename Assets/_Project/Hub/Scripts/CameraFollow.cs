using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub
{
    /// <summary>
    /// Cameta que segue o Player
    /// Botao direito permite girar ela
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 9f, -7f);
        [SerializeField] float _lookHeight = 1f;      
        [SerializeField] float _smoothTime = 0.15f;
        [SerializeField] float _rotateSpeed = 4f;

        Vector3 velocity;
        float yaw;

        void LateUpdate()
        {
            if (target == null) return;

            if (Input.GetMouseButton(1))
                yaw += Input.GetAxis("Mouse X") * _rotateSpeed;

            Vector3 desired = target.position + Quaternion.Euler(0f, yaw, 0f) * offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, _smoothTime);
            transform.LookAt(target.position + Vector3.up * _lookHeight);
        }
    }
}
