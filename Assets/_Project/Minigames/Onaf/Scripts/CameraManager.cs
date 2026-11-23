using System.Collections.Generic;
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

        [Header("Cômodos visíveis")]
        [Tooltip("Liga só o cômodo mostrado (câmera de segurança) ou só os do jogador (visão do quarto). Evita ver os cômodos vizinhos nos cantos da imagem.")]
        [SerializeField] private bool isolarComodos = true;
        [Tooltip("Cômodos ligados enquanto o jogador olha o próprio quarto: o quarto (60) e o corredor (20), visto pela porta.")]
        [SerializeField] private int[] comodosDoJogador = { 60, 20 };

        private Arvore tree;
        private QuartoNode currentRoom;

        private bool cameraLigada;

        private readonly Dictionary<int, Room> comodos = new Dictionary<int, Room>();
        private Boneco bonecoObservado;

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

            // guarda os cômodos da árvore (id -> Room) para poder ligar/desligar
            tree.EmOrdem(q =>
            {
                if (q.GetRoom() != null)
                    comodos[q.GetID()] = q.GetRoom();
            });

            cameraJogador.enabled = true;
            cameraSeguranca.enabled = false;

            energia.EnergiaAcabou += DesligarCamera;

            AtualizarCamera();
        }

        private void Update()
        {
            AtualizarObservacao();
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

            AtualizarCamera();
        }

        private void AtualizarCamera()
        {
            if (currentRoom == null)
                return;

            Room room = currentRoom.GetRoom();

            if (room == null)
                return;

            AplicarVisibilidade();

            if (cameraLigada)
                isoCamera.SnapTo(room);

            AtualizarObservacao();
        }

        private void AplicarVisibilidade()
        {
            if (!isolarComodos)
                return;

            foreach (KeyValuePair<int, Room> par in comodos)
            {
                bool ligado = cameraLigada ? par.Key == idCurrentRoom : EhComodoDoJogador(par.Key);

                if (par.Value != null && par.Value.gameObject.activeSelf != ligado)
                    par.Value.gameObject.SetActive(ligado);
            }
        }

        private bool EhComodoDoJogador(int id)
        {
            foreach (int ativo in comodosDoJogador)
            {
                if (ativo == id)
                    return true;
            }

            return false;
        }

        private void AtualizarObservacao()
        {
            Boneco alvo = null;

            if (cameraLigada && currentRoom != null)
                alvo = currentRoom.GetBoneco();

            if (alvo == bonecoObservado)
                return;

            if (bonecoObservado != null)
                bonecoObservado.SetSendoObservado(false);

            if (alvo != null)
                alvo.SetSendoObservado(true);

            bonecoObservado = alvo;
        }

        public bool CameraEstaLigada()
        {
            return cameraLigada;
        }

        public int GetIDRoomAtual()
        {
            return idCurrentRoom;
        }

        public Camera GetCameraJogador()
        {
            return cameraJogador;
        }

        private void OnDestroy()
        {
            if (energia != null)
                energia.EnergiaAcabou -= DesligarCamera;

            if (bonecoObservado != null)
                bonecoObservado.SetSendoObservado(false);
        }
    }
}
