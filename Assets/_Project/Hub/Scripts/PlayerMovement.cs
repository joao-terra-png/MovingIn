using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub
{
    /// <summary>
    /// Movimento do player com WASD
    /// Shift = correr. Usa CharacterController
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float _moveSpeed = 4f;
        [SerializeField] float _runningMult = 1.6f;
        [SerializeField] float _turningSpeed = 720f;
        [SerializeField] float _gravity = -20f;
        
        [SerializeField] Transform cameraTransform;

        CharacterController controller;
        float verticalVelocity;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }
        }

        void Update()
        {
            // leitura do teclado (atribui -1 a 1)
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
            input = Vector3.ClampMagnitude(input, 1f);

            // converte direcao geral para direcao da camera
            Vector3 forward = Vector3.forward;
            Vector3 right = Vector3.right;

            if (cameraTransform != null)
            {
                forward = Vector3.ProjectOnPlane(cameraTransform.forward, Vector3.up).normalized;
                right = Vector3.ProjectOnPlane(cameraTransform.right, Vector3.up).normalized;
            }
            Vector3 move = forward * input.z + right * input.x;

            // rotaciona o Player
            if (move.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(move);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, target, _turningSpeed * Time.deltaTime);
            }

            // Gravidade
            if (controller.isGrounded && verticalVelocity < 0f)
                verticalVelocity = -2f;
            verticalVelocity += _gravity * Time.deltaTime;

            // logica de movimento
            float speed = _moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? _runningMult : 1f);
            Vector3 velocity = move * speed;
            velocity.y = verticalVelocity;
            controller.Move(velocity * Time.deltaTime);
        }
    }
}