using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// controla qual comodo esta ativo
    ///
    /// No inicio: desliga todos os comodos e liga so o inicial
    /// Quando o Player passa por uma Door:
    ///   (com fader) escurece a tela, troca o comodo, teleporta o Player,
    ///               posiciona a camera e clareia a tela
    ///   (sem fader) troca o comodo e a camera desliza ate ele
    ///
    /// </summary>
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [SerializeField] Room startRoom;
        [SerializeField] Transform player;
        [SerializeField] IsoCamera isoCamera;

        [Tooltip("Tempo minimo entre duas trocas de comodo (evita ficar indo e voltando na porta)")]
        [SerializeField] float _doorCooldown = 0.3f;

        [Header("Transicao")]
        [Tooltip("Tela preta que escurece na troca de comodo. Se ficar vazio, a camera so desliza")]
        [SerializeField] ScreenFader fader;

        [Tooltip("Duracao de cada metade do fade (escurecer e clarear), em segundos")]
        [SerializeField] float _fadeTime = 0.2f;

        [Tooltip("Tempo que a tela fica toda preta entre escurecer e clarear")]
        [SerializeField] float _blackPause = 0.05f;

        public Room CurrentRoom { get; private set; }
        public bool IsTransitioning { get; private set; }

        Room[] _rooms;
        float _lastChangeTime = -10f;
        PlayerMovement _playerMovement;
        PlayerInteractor _playerInteractor;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (startRoom == null || player == null || isoCamera == null)
            {
                Debug.LogError("[RoomManager] Preencha Start Room, Player e Iso Camera no Inspector.");
                return;
            }

            _playerMovement = player.GetComponent<PlayerMovement>();
            _playerInteractor = player.GetComponent<PlayerInteractor>();

            // acha todos os comodos da cena, inclusive os que estao desligados
            _rooms = FindObjectsOfType<Room>(true);

            foreach (Room room in _rooms)
                room.Hide();

            CurrentRoom = startRoom;
            CurrentRoom.Show();
            isoCamera.SnapTo(CurrentRoom);
        }

        public void GoToRoom(Room target, Transform spawnPoint)
        {
            if (target == null || target == CurrentRoom || IsTransitioning) return;
            if (Time.time - _lastChangeTime < _doorCooldown) return;
            _lastChangeTime = Time.time;

            if (fader != null)
            {
                StartCoroutine(FadeTransition(target, spawnPoint));
            }
            else
            {
                SwitchRoom(target, spawnPoint);
                isoCamera.MoveTo(CurrentRoom);
            }
        }

        IEnumerator FadeTransition(Room target, Transform spawnPoint)
        {
            IsTransitioning = true;
            SetPlayerControl(false);

            // escurece
            yield return fader.FadeOut(_fadeTime);

            // com a tela preta, troca tudo de uma vez (o jogador nao ve)
            SwitchRoom(target, spawnPoint);
            isoCamera.SnapTo(CurrentRoom);

            yield return new WaitForSecondsRealtime(_blackPause);

            // clareia
            yield return fader.FadeIn(_fadeTime);

            SetPlayerControl(true);
            IsTransitioning = false;
        }

        void SwitchRoom(Room target, Transform spawnPoint)
        {
            CurrentRoom.Hide();
            target.Show();
            CurrentRoom = target;

            if (spawnPoint != null)
                TeleportPlayer(spawnPoint);
        }

        // trava o movimento e a interacao durante a transicao
        void SetPlayerControl(bool enabled)
        {
            if (_playerMovement != null) _playerMovement.enabled = enabled;
            if (_playerInteractor != null) _playerInteractor.enabled = enabled;
        }

        void TeleportPlayer(Transform spawnPoint)
        {
            // o CharacterController "segura" a posicao do objeto,
            // entao precisa desligar ele antes de mudar a posicao na mao
            CharacterController controller = player.GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            player.position = spawnPoint.position;
            player.rotation = spawnPoint.rotation;

            if (controller != null) controller.enabled = true;
        }
    }
}