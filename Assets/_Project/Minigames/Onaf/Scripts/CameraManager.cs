using MovingIn.Core.Structures;
using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class CameraManager : MonoBehaviour
    {
        [Header("Câmeras")]
        [SerializeField] private Camera cameraJogador;
        [SerializeField] private Camera cameraSeguranca;
        [SerializeField] private IsoCamera isoCamera;

        [Header("Gerenciadores")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private EnergyManager energia;

        [Header("Configuração")]
        [SerializeField] private int idCurrentRoom = 60;

        private Arvore tree;
        private QuartoNode currentRoom;

        private bool cameraLigada;

        private void Start()
        {
            if (gameManager == null || energia == null)
            {
                Debug.LogError("CameraManager: faltam referências.");
                return;
            }

            tree = gameManager.GetTree();

            if (cameraSeguranca == null ||
                cameraJogador == null ||
                isoCamera == null ||
                tree == null)
            {
                Debug.LogError("CameraManager: câmera ou árvore não configurada.");
                return;
            }

            currentRoom = tree.GetQuarto(idCurrentRoom);

            if (currentRoom == null)
            {
                Debug.LogError("Cômodo inicial não encontrado.");
                return;
            }

            cameraJogador.enabled = true;
            cameraSeguranca.enabled = false;

            energia.EnergiaAcabou += DesligarCamera;

            AtualizarCamera();
        }

        public void ToggleCamera()
        {
            if (cameraLigada)
            {
                DesligarCamera();
                return;
            }

            if (!energia.TemEnergia())
                return;

            LigarCamera();
        }

        private void LigarCamera()
        {
            if (!energia.TemEnergia())
                return;

            cameraLigada = true;

            cameraJogador.enabled = false;
            cameraSeguranca.enabled = true;

            AtualizarCamera();

            energia.SetCameraAtiva(true);
        }

        public void DesligarCamera()
        {
            cameraLigada = false;

            if (cameraJogador != null)
                cameraJogador.enabled = true;

            if (cameraSeguranca != null)
                cameraSeguranca.enabled = false;

            if (energia != null)
                energia.SetCameraAtiva(false);

            AtualizarCamera();
        }

        public void SwitchCamera(int id)
        {
            if (!cameraLigada)
            {
                return;
            }

            QuartoNode novoQuarto = tree.GetQuarto(id);

            if (novoQuarto == null)
            {
                return;
            }

            currentRoom = novoQuarto;
            idCurrentRoom = id;

            isoCamera.SnapTo(currentRoom.GetRoom());

            Debug.Log("Câmera mudou para: " + id);
        }

        private void AtualizarCamera()
        {
            if (currentRoom == null)
                return;

            Room room = currentRoom.GetRoom();

            if (room == null)
                return;

            if (cameraLigada)
                isoCamera.SnapTo(room);

            Boneco boneco = currentRoom.GetBoneco();

            if (boneco != null)
                boneco.SetSendoObservado(cameraLigada);
        }

        public bool CameraEstaLigada()
        {
            return cameraLigada;
        }

        public int GetIDRoomAtual()
        {
            return idCurrentRoom;
        }

        private void OnDestroy()
        {
            if (energia != null)
                energia.EnergiaAcabou -= DesligarCamera;

            if (currentRoom != null && currentRoom.GetBoneco() != null)
                currentRoom.GetBoneco().SetSendoObservado(false);
        }
    }
}
