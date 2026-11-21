using System;
using System.Collections;
using MovingIn.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class MainMenu : MonoBehaviour
    {
        public static bool Active { get; private set; }
        public static event Action Finished;

        [Header("Scene")]
        [SerializeField] Camera cam;
        [SerializeField] Transform player;
        [SerializeField] GameObject playerVisual;
        [SerializeField] Behaviour[] cameraFollowers;
        [SerializeField] Behaviour[] playerControl;
        [SerializeField] TimeOfDay timeOfDay;
        [SerializeField] NpcWalker[] movers;
        [SerializeField] Transform truck;
        [SerializeField] Transform frontDoor;
        [SerializeField] Transform playerStop;

        [Header("UI")]
        [SerializeField] CanvasGroup group;
        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button quitButton;

        [Header("Camera")]
        [SerializeField] Vector3 _menuFocus = new Vector3(18.65f, 1.5f, -8.35f);
        [SerializeField] float _menuSize = 8.5f;
        [SerializeField] float _distance = 30f;
        [SerializeField] float _gamePitch = 30f;
        [SerializeField] float _skyPitch = -14f;
        [SerializeField] float _descentTime = 3f;
        [SerializeField] float _gameSize = 4.5f;
        [SerializeField] float _followHeight = 1.5f;

        [Header("Timing")]
        [SerializeField] float _duskTime = 5f;
        [SerializeField] Vector3 _truckDrive = new Vector3(40f, 0f, 0f);
        [SerializeField] float _truckTime = 4.5f;
        [SerializeField] float _zoomTime = 1.8f;

        [Header("Testing")]
        [SerializeField] bool _skipMenu;

        float _pitch;
        bool _holdCamera;
        Behaviour[] _followers;
        Vector3 _camPosition;

        void Awake()
        {
            Active = false;
        }

        IEnumerator Start()
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;

            StoryManager sm = StoryManager.Instance;
            bool testingChapter = sm != null && sm.StartChapter != Chapter.Arrival;
            if (_skipMenu || SaveSystem.HasPending || testingChapter)
            {
                gameObject.SetActive(false);
                yield break;
            }

            Active = true;
            Cutscene.Playing = true;
            if (cam == null) cam = Camera.main;
            GameProgress.ResetAll();

            _followers = Array.FindAll(cameraFollowers, b => b != null && b.enabled);
            Cutscene.SetBehaviours(_followers, false);
            Cutscene.SetBehaviours(playerControl, false);
            if (playerVisual != null) playerVisual.SetActive(false);

            yield return null;
            if (timeOfDay != null) timeOfDay.Apply("Day", 0f);
            foreach (NpcWalker m in movers) if (m != null) m.StartLoop();

            _camPosition = _menuFocus - Forward(_gamePitch) * _distance;
            _pitch = _skyPitch;
            _holdCamera = true;

            yield return new WaitForSeconds(0.8f);
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / _descentTime;
                _pitch = Mathf.Lerp(_skyPitch, _gamePitch, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            _pitch = _gamePitch;

            newGameButton.onClick.AddListener(NewGame);
            optionsButton.onClick.AddListener(OpenOptions);
            quitButton.onClick.AddListener(Quit);
            if (continueButton != null) continueButton.gameObject.SetActive(false);

            yield return Cutscene.Fade(group, 1f, 0.6f);
            group.interactable = true;
            group.blocksRaycasts = true;
            EnsureEventSystem();
            EventSystem.current?.SetSelectedGameObject(newGameButton.gameObject);
        }

        void LateUpdate()
        {
            if (!_holdCamera || cam == null) return;
            cam.transform.rotation = Quaternion.Euler(_pitch, 45f, 0f);
            cam.transform.position = _camPosition;
            cam.orthographicSize = _menuSize;
        }

        void NewGame()
        {
            if (!group.interactable) return;
            StartCoroutine(NewGameRoutine());
        }

        IEnumerator NewGameRoutine()
        {
            group.interactable = false;
            group.blocksRaycasts = false;
            StartCoroutine(Cutscene.Fade(group, 0f, 0.4f));
            if (timeOfDay != null) timeOfDay.Apply("Evening", _duskTime);

            int leaving = 0;
            foreach (NpcWalker m in movers)
            {
                if (m == null) continue;
                leaving++;
                StartCoroutine(Track(m.Leave(), () => leaving--));
            }
            float limit = Time.time + 8f;
            while (leaving > 0 && Time.time < limit) yield return null;

            if (truck != null)
            {
                yield return new WaitForSeconds(0.5f);
                yield return Cutscene.Move(truck, truck.position + _truckDrive, _truckTime, true, false);
                truck.gameObject.SetActive(false);
            }

            yield return new WaitForSeconds(0.6f);
            if (player != null && frontDoor != null)
            {
                Vector3 door = new Vector3(frontDoor.position.x, player.position.y, frontDoor.position.z);
                Cutscene.Teleport(player, door);
                if (playerVisual != null) playerVisual.SetActive(true);
                yield return new WaitForSeconds(0.3f);
            }

            Vector3 startPos = _camPosition;
            float startSize = cam.orthographicSize;
            Quaternion rot = Quaternion.Euler(_gamePitch, 45f, 0f);
            Vector3 stop = playerStop != null ? playerStop.position : (player != null ? player.position : Vector3.zero);
            if (player != null) stop.y = player.position.y;

            CharacterController cc = player != null ? player.GetComponent<CharacterController>() : null;
            if (cc != null) cc.enabled = false;
            _holdCamera = false;
            float t = 0f;
            Vector3 walkFrom = player != null ? player.position : Vector3.zero;
            while (t < 1f)
            {
                t += Time.deltaTime / _zoomTime;
                float s = Mathf.SmoothStep(0f, 1f, t);
                if (player != null) player.position = Vector3.Lerp(walkFrom, stop, Mathf.Clamp01(t * 1.2f));
                Vector3 focus = player != null ? new Vector3(player.position.x, _followHeight + FloorY(), player.position.z) : _menuFocus;
                cam.transform.rotation = rot;
                cam.transform.position = Vector3.Lerp(startPos, focus - rot * Vector3.forward * _distance, s);
                cam.orthographicSize = Mathf.Lerp(startSize, _gameSize, s);
                yield return null;
            }
            if (cc != null) cc.enabled = true;
            cam.orthographicSize = _gameSize;

            Cutscene.SetBehaviours(_followers, true);
            Cutscene.SetBehaviours(playerControl, true);
            Active = false;
            Cutscene.Playing = false;
            Finished?.Invoke();
            gameObject.SetActive(false);
        }

        float FloorY()
        {
            return frontDoor != null ? frontDoor.position.y : 0f;
        }

        IEnumerator Track(IEnumerator routine, Action done)
        {
            yield return routine;
            done();
        }

        void OpenOptions()
        {
            if (PauseMenu.Instance != null) PauseMenu.Instance.OpenOptions();
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static Vector3 Forward(float pitch)
        {
            return Quaternion.Euler(pitch, 45f, 0f) * Vector3.forward;
        }

        static void EnsureEventSystem()
        {
            if (EventSystem.current != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
    }
}
