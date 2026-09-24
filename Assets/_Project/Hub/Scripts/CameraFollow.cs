using UnityEngine;

namespace Mudanca.Hub
{
    /// <summary>
    /// Câmera de cima e em ângulo (estilo "casinha sem teto") que segue o player.
    /// Segure o botão DIREITO do mouse e arraste para girar em volta do garoto.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset = new Vector3(0f, 9f, -7f);
        [SerializeField] float lookHeight = 1f;       // olha um pouco acima do pé do player
        [SerializeField] float smoothTime = 0.15f;
        [SerializeField] float rotateSpeed = 4f;

        Vector3 velocity;
        float yaw;

        void LateUpdate()
        {
            if (target == null) return;

            if (Input.GetMouseButton(1))
                yaw += Input.GetAxis("Mouse X") * rotateSpeed;

            Vector3 desired = target.position + Quaternion.Euler(0f, yaw, 0f) * offset;
            transform.position = Vector3.SmoothDamp(transform.position, desired, ref velocity, smoothTime);
            transform.LookAt(target.position + Vector3.up * lookHeight);
        }
    }
}
