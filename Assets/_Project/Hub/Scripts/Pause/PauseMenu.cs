using MovingIn.Core;
using MovingIn.Minigames.Kitchen.ADTs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class PauseMenu : MonoBehaviour
    {
        public static PauseMenu Instance { get; private set; }
        public static bool IsOpen => Instance != null && Instance._open;

        [SerializeField] GameObject root;
        [SerializeField] GameObject mainScreen;
        [SerializeField] GameObject optionsScreen;
        [SerializeField] SlotsScreen slotsScreen;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button loadButton;
        [SerializeField] Button leaveMinigameButton;
        [SerializeField] Button quitButton;
        [SerializeField] KeyCode key = KeyCode.Escape;

        StaticStack<GameObject> _screens;
        bool _open;
        float _previousTimeScale = 1f;
        bool _previousCursorVisible;
        CursorLockMode _previousLock;
        GameObject _tempEventSystem;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _screens = new StaticStack<GameObject>(8);
            root.SetActive(false);

            resumeButton.onClick.AddListener(Close);
            optionsButton.onClick.AddListener(() => Show(optionsScreen));
            saveButton.onClick.AddListener(() => OpenSlots(true));
            loadButton.onClick.AddListener(() => OpenSlots(false));
            leaveMinigameButton.onClick.AddListener(LeaveMinigame);
            quitButton.onClick.AddListener(Quit);
        }

        void Start()
        {
            GameSettings.Apply();
        }

        void Update()
        {
            if (!Input.GetKeyDown(key)) return;
            if (_open) Back();
            else if (!Cutscene.Playing) Open();
        }

        public void Open()
        {
            if (_open) return;
            _open = true;

            _previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            _previousCursorVisible = Cursor.visible;
            _previousLock = Cursor.lockState;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            if (EventSystem.current == null)
                _tempEventSystem = new GameObject("EventSystem (Pause)", typeof(EventSystem), typeof(StandaloneInputModule));

            bool inMinigame = MinigameManager.Instance != null && MinigameManager.Instance.IsPlayingMinigame;
            title.text = "Pausado";
            saveButton.gameObject.SetActive(!inMinigame);
            loadButton.gameObject.SetActive(!inMinigame);
            leaveMinigameButton.gameObject.SetActive(inMinigame);
            saveButton.interactable = SaveSystem.CanSave;

            mainScreen.SetActive(false);
            optionsScreen.SetActive(false);
            slotsScreen.gameObject.SetActive(false);
            root.SetActive(true);
            Show(mainScreen);
        }

        public void OpenOptions()
        {
            if (!_open) Open();
            while (_screens.Pop(out GameObject s)) s.SetActive(false);
            Show(optionsScreen);
        }

        public void Show(GameObject screen)
        {
            if (_screens.Pop(out GameObject top))
            {
                top.SetActive(false);
                _screens.Push(top);
            }
            screen.SetActive(true);
            _screens.Push(screen);
            SelectFirst(screen);
        }

        public void Back()
        {
            if (!_screens.Pop(out GameObject top))
            {
                Close();
                return;
            }
            top.SetActive(false);

            if (_screens.Pop(out GameObject previous))
            {
                previous.SetActive(true);
                _screens.Push(previous);
                SelectFirst(previous);
            }
            else Close();
        }

        public void Close()
        {
            if (!_open) return;
            while (_screens.Pop(out GameObject screen)) screen.SetActive(false);
            root.SetActive(false);
            _open = false;

            Time.timeScale = _previousTimeScale;
            Cursor.visible = _previousCursorVisible;
            Cursor.lockState = _previousLock;
            if (_tempEventSystem != null) Destroy(_tempEventSystem);
            _tempEventSystem = null;
        }

        void OpenSlots(bool saving)
        {
            slotsScreen.Setup(saving);
            Show(slotsScreen.gameObject);
        }

        void LeaveMinigame()
        {
            Close();
            Time.timeScale = 1f;
            MinigameBridge.Finish(false);
        }

        void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        static void SelectFirst(GameObject screen)
        {
            if (EventSystem.current == null) return;
            foreach (Selectable s in screen.GetComponentsInChildren<Selectable>())
            {
                if (!s.interactable || !s.gameObject.activeInHierarchy) continue;
                EventSystem.current.SetSelectedGameObject(s.gameObject);
                return;
            }
        }
    }
}
