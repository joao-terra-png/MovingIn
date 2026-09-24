using UnityEngine;

namespace Mudanca.Hub
{
    /// <summary>
    /// Movimento do garoto com WASD/setas, relativo à câmera.
    /// Shift = correr. Usa CharacterController (não precisa de Rigidbody).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float moveSpeed = 4f;
        [SerializeField] float runMultiplier = 1.6f;
        [SerializeField] float turnSpeed = 720f;   // graus por segundo
        [SerializeField] float gravity = -20f;

        [Tooltip("Se ficar vazio, usa a Main Camera automaticamente.")]
        [SerializeField] Transform cameraTransform;

        CharacterController controller;
        float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
                cameraTransform = Camera.main.transform;
        }

        void Update()
        {
            // 1. Lê o teclado (-1 a 1 em cada eixo)
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            // 2. Converte para a direção da câmera ("frente" = para onde a câmera olha)
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;
            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }
            Vector3 move = forward * input.z + right * input.x;

            // 3. Gira o personagem para onde ele está andando
            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, turnSpeed * Time.deltaTime);
            }

            // 4. Gravidade simples
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += gravity * Time.deltaTime;

            // 5. Move
            float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? runMultiplier : 1f);
            Vector3 velocity = move * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}
