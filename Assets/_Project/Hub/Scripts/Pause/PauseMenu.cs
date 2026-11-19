using MovingIn.Minigames.Kitchen.ADTs;
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
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
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
            quitButton.onClick.AddListener(Quit);
        }

        void Start()
        {
            GameSettings.Apply();
        }

        void Update()
        {
            if (!Input.GetKeyDown(key)) return;
            if (!_open && MinigameManager.Instance != null && MinigameManager.Instance.IsPlayingMinigame) return;
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

            mainScreen.SetActive(false);
            optionsScreen.SetActive(false);
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
