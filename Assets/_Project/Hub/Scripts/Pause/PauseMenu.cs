using System.Collections.Generic;
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
        [SerializeField] Button resumeButton;
        [SerializeField] Button optionsButton;
        [SerializeField] Button quitButton;
        [SerializeField] KeyCode key = KeyCode.Escape;

        [Header("Rejogar minigames (liberado depois do final)")]
        [SerializeField] string _replayLabel = "Minigames";
        [Tooltip("Mostra o botao mesmo antes do final, para testar")]
        [SerializeField] bool _alwaysShowReplay = false;

        StaticStack<GameObject> _screens;
        bool _open;
        float _previousTimeScale = 1f;
        bool _previousCursorVisible;
        CursorLockMode _previousLock;
        GameObject _tempEventSystem;
        Button _replayButton;
        GameObject _replayScreen;

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
            BuildReplayButton();
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
            if (_replayScreen != null) _replayScreen.SetActive(false);
            if (_replayButton != null) _replayButton.gameObject.SetActive(_alwaysShowReplay || EndingDirector.Finished);
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

        void BuildReplayButton()
        {
            _replayButton = Instantiate(optionsButton, optionsButton.transform.parent);
            _replayButton.name = "Button_Replay";
            _replayButton.onClick.RemoveAllListeners();
            _replayButton.transform.SetSiblingIndex(optionsButton.transform.GetSiblingIndex());
            SetLabel(_replayButton, _replayLabel);
            _replayButton.onClick.AddListener(OpenReplay);
            _replayButton.gameObject.SetActive(false);
        }

        void OpenReplay()
        {
            if (_replayScreen == null) BuildReplayScreen();
            Show(_replayScreen);
        }

        void BuildReplayScreen()
        {
            _replayScreen = Instantiate(mainScreen, mainScreen.transform.parent);
            _replayScreen.name = "Replay";
            _replayScreen.SetActive(false);

            foreach (Button b in _replayScreen.GetComponentsInChildren<Button>(true))
            {
                GameObject old = b.gameObject;
                old.SetActive(false);
                old.transform.SetParent(null, false);
                Destroy(old);
            }

            Transform title = _replayScreen.transform.Find("Title");
            TMP_Text titleText = title != null ? title.GetComponent<TMP_Text>() : null;
            if (titleText != null) titleText.text = _replayLabel;

            foreach (MinigameInteractable mi in ReplayList())
            {
                MinigameInteractable target = mi;
                AddReplayButton("Button_" + mi.MinigameId, mi.DisplayName, () => Replay(target));
            }
            AddReplayButton("Button_Back", "Voltar", Back);
        }

        void AddReplayButton(string name, string label, UnityEngine.Events.UnityAction action)
        {
            Button b = Instantiate(resumeButton, _replayScreen.transform);
            b.name = name;
            b.onClick.RemoveAllListeners();
            b.gameObject.SetActive(true);
            SetLabel(b, label);
            b.onClick.AddListener(action);
        }

        static List<MinigameInteractable> ReplayList()
        {
            var list = new List<MinigameInteractable>();
            StoryManager story = StoryManager.Instance;
            if (story != null && story.Minigames != null)
                foreach (TrackedMinigame t in story.Minigames)
                    if (t != null && t.minigame != null && !list.Contains(t.minigame)) list.Add(t.minigame);

            var others = new List<MinigameInteractable>(FindObjectsOfType<MinigameInteractable>(true));
            others.Sort((a, b) => string.CompareOrdinal(a.DisplayName, b.DisplayName));
            foreach (MinigameInteractable mi in others)
                if (!list.Contains(mi)) list.Add(mi);
            return list;
        }

        void Replay(MinigameInteractable mi)
        {
            Close();
            if (MinigameManager.Instance != null) MinigameManager.Instance.StartMinigame(mi, true);
        }

        static void SetLabel(Button b, string text)
        {
            TMP_Text label = b.GetComponentInChildren<TMP_Text>(true);
            if (label != null) label.text = text;
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
