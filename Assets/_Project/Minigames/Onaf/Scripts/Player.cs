using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class Player : MonoBehaviour
    {
        [SerializeField] private CameraManager cameraManager;

        [Header("Olhar com o mouse (visão do quarto)")]
        [SerializeField] private bool olharComMouse = true;
        [Tooltip("Ângulo (graus, em Y) com o mouse na borda esquerda: a janela.")]
        [SerializeField] private float anguloEsquerda = -30f;
        [Tooltip("Ângulo com o mouse na borda direita: a porta.")]
        [SerializeField] private float anguloDireita = 100f;
        [SerializeField] private float suavizacao = 8f;

        private Transform visao;
        private float inclinacao;
        private float giro;

        private void Start()
        {
            if (cameraManager == null)
                return;

            Camera cameraJogador = cameraManager.GetCameraJogador();

            if (cameraJogador == null)
                return;

            visao = cameraJogador.transform;

            Vector3 angulos = visao.localEulerAngles;
            inclinacao = Mathf.DeltaAngle(0f, angulos.x);
            giro = Mathf.DeltaAngle(0f, angulos.y);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (cameraManager != null)
                {
                    cameraManager.ToggleCamera();
                }
                else
                {
                    Debug.LogError("CameraManager não foi atribuído!");
                }
            }

            OlharComMouse();
        }

        private void OlharComMouse()
        {
            if (!olharComMouse || visao == null || cameraManager == null)
                return;

            if (cameraManager.CameraEstaLigada())
                return;

            float posicao = Input.mousePosition.x / Mathf.Max(1f, Screen.width);
            float t = Mathf.InverseLerp(0.1f, 0.9f, posicao);
            float alvo = Mathf.Lerp(anguloEsquerda, anguloDireita, t);

            giro = Mathf.LerpAngle(giro, alvo, 1f - Mathf.Exp(-suavizacao * Time.deltaTime));
            visao.localRotation = Quaternion.Euler(inclinacao, giro, 0f);
        }
    }
}
