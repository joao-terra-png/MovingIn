using System;
using MovingIn.Core.Structures;
using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class GameManager : MonoBehaviour
    {
        [Header("Casa")]
        [SerializeField] private GameObject House;

        [Header("Gerenciadores")]
        [SerializeField] private EnergyManager energia;
        [SerializeField] private CameraManager cameraManager;
        [SerializeField] private NightManager nightManager;
        [SerializeField] private DefesaManager defesaManager;

        private Arvore HouseMap;
        private Boneco[] bonecos;

        private bool partidaFinalizada;

        public event Action<bool> PartidaTerminou;

        private void Awake()
        {
            CriarArvore();
            ConfigurarBonecos();
        }

        private void Start()
        {
            if (nightManager != null)
                nightManager.NoiteTerminou += Vitoria;

            if (energia != null)
                energia.ToleranciaEsgotada += Derrota;

            if (nightManager != null)
                nightManager.IniciarNoite();
        }

        private void CriarArvore()
        {
            HouseMap = new Arvore();

            if (House == null)
            {
                return;
            }

            Room[] rooms = House.GetComponentsInChildren<Room>(true);

            foreach (Room room in rooms)
            {
                HouseMap.Insere(
                    room.GetRoomId(),
                    room,
                    out bool ok
                );
            }

            if (HouseMap.Vazia())
                Debug.LogError("A árvore de cômodos está vazia.");
        }

        private void ConfigurarBonecos()
        {
            bonecos = FindObjectsOfType<Boneco>();

            foreach (Boneco boneco in bonecos)
            {
                boneco.Configurar(HouseMap, this);
            }
        }

        public Arvore GetTree()
        {
            return HouseMap;
        }

        public void VerificarInvasao(int idOrigem)
        {
            if (partidaFinalizada)
                return;

            if (defesaManager == null)
            {
                Debug.LogError("GameManager: DefesaManager não configurado!");
                Derrota();
                return;
            }

            // Entrada pelo corredor: verifica a porta.
            if (idOrigem == 20 && defesaManager.PortaEstaFechada())
            {
                Debug.Log("Boneco bloqueado pela porta!");
                return;
            }

            // Entrada pelo quintal: verifica a janela.
            if (idOrigem == 80 && defesaManager.JanelaEstaFechada())
            {
                Debug.Log("Boneco bloqueado pela janela!");
                return;
            }

            Debug.Log("O boneco invadiu o quarto do jogador!");
            Derrota();
        }

        public void Vitoria()
        {
            FinalizarPartida(true);
        }

        public void Derrota()
        {
            FinalizarPartida(false);
        }

        private void FinalizarPartida(bool venceu)
        {
            if (partidaFinalizada)
                return;

            partidaFinalizada = true;

            if (nightManager != null)
                nightManager.Parar();

            if (energia != null)
                energia.Parar();

            if (cameraManager != null)
                cameraManager.DesligarCamera();

            PartidaTerminou?.Invoke(venceu);

            if (venceu)
                Debug.Log("Você sobreviveu até as 6 AM!");
            else
                Debug.Log("Você perdeu!");
        }

        public string GetNomeComodo(int id)
        {
            if (HouseMap == null)
            {
                Debug.LogError("GameManager: a árvore ainda não foi inicializada!");
                return "Árvore não encontrada";
            }

            Room room = HouseMap.GetRoom(id);

            if (room == null)
            {
                Debug.LogWarning("GameManager: não existe Room associado ao ID " + id);
                return "Desconhecido";
            }

            return room.GetRoomName();
        }

        private void OnDestroy()
        {
            if (nightManager != null)
                nightManager.NoiteTerminou -= Vitoria;

            if (energia != null)
                energia.ToleranciaEsgotada -= Derrota;
        }
    }
}
