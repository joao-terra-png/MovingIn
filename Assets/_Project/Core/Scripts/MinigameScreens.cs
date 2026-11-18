using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MovingIn.Core{
    [RequireComponent(typeof(MinigameExit))]
    public class MinigameScreens : MonoBehaviour
    {
        [SerializeField] string _title = "Como jogar";
        [SerializeField, TextArea(5, 14)] string _howToPlay = "";
        [SerializeField, Min(0)] int _goal = 50;
        [SerializeField] TMP_FontAsset font;

        [Header("Cores")]
        [SerializeField] Color _paper = new Color32(0xF4, 0xEA, 0xD8, 0xFF);
        [SerializeField] Color _ink = new Color32(0x2B, 0x22, 0x30, 0xFF);
        [SerializeField] Color _accent = new Color32(0xB0, 0x60, 0x3A, 0xFF);
        [SerializeField] Color _soft = new Color32(0xDD, 0xCF, 0xB8, 0xFF);
        [SerializeField] Color _shade = new Color(0.08f, 0.05f, 0.1f, 0.72f);

        public int Goal => _goal;
        public bool IsPlaying => !_intro && !Finished;
        public bool Finished { get; private set; }

        MinigameExit _exit;
        GameObject _root;
        RectTransform _frame;
        RectTransform _card;
        TMP_Text _titleText;
        TMP_Text _bodyText;
        Button _primary;
        Button _secondary;
        TMP_Text _primaryLabel;
        TMP_Text _secondaryLabel;
        Action _onPrimary;
        Action _onSecondary;
        bool _intro;

        void Awake()
        {
            _exit = GetComponent<MinigameExit>();
            EnsureEventSystem();
            Build();
        }

        void Start()
        {
            _intro = true;
            Time.timeScale = 0f;
            Show(_title, $"{_howToPlay}\n\n<b>Meta: {_goal} pontos</b>", TextAlignmentOptions.TopLeft, 720f,
                 "Jogar", Play, null, null);
        }

        void Update()
        {
            if (_intro) Time.timeScale = 0f;
            if (!_root.activeSelf) return;
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) _onPrimary?.Invoke();
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        public void Play()
        {
            if (!_intro) return;
            _intro = false;
            Hide();
            Time.timeScale = 1f;
        }

        public void ShowResult(int points)
        {
            if (Finished) return;
            Finished = true;
            _intro = false;
            Time.timeScale = 0f;

            if (points >= _goal)
                Show("Muito bem!", $"Você fez <b>{points}</b> pontos.\nMeta: {_goal} pontos.",
                     TextAlignmentOptions.Center, 470f, "Voltar pra casa", () => _exit.Leave(true), null, null);
            else
                Show("Quase lá!", $"Você fez <b>{points}</b> pontos.\nA meta é {_goal}. Bora de novo?",
                     TextAlignmentOptions.Center, 470f, "Tentar de novo", _exit.Retry, "Sair", () => _exit.Leave(false));
        }

        void Show(string title, string body, TextAlignmentOptions align, float height, string primary, Action onPrimary,
                  string secondary, Action onSecondary)
        {
            _card.sizeDelta = new Vector2(1040f, height);
            _frame.sizeDelta = new Vector2(1052f, height + 12f);
            _titleText.text = title;
            _bodyText.text = body;
            _bodyText.alignment = align;
            _onPrimary = onPrimary;
            _onSecondary = onSecondary;
            _primaryLabel.text = primary;

            bool two = !string.IsNullOrEmpty(secondary);
            _secondary.gameObject.SetActive(two);
            if (two) _secondaryLabel.text = secondary;
            ((RectTransform)_primary.transform).anchoredPosition = new Vector2(two ? 190f : 0f, 70f);
            ((RectTransform)_secondary.transform).anchoredPosition = new Vector2(-190f, 70f);
            _root.SetActive(true);
        }

        void Hide()
        {
            _root.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null) return;
            GameObject es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(transform, false);
        }

        void Build()
        {
            GameObject canvasGo = new GameObject("ScreensCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasGo.transform.SetParent(transform, false);
            Canvas canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;
            CanvasScaler scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform shade = Panel("Shade", canvasGo.transform, _shade);
            Stretch(shade);
            _root = shade.gameObject;

            _frame = Panel("Frame", shade, _ink);
            _card = Panel("Card", _frame, _paper);
            RectTransform card = _card;

            _titleText = Label("Title", card, 64f, _accent, FontStyles.Bold);
            RectTransform tr = _titleText.rectTransform;
            tr.anchorMin = new Vector2(0f, 1f);
            tr.anchorMax = new Vector2(1f, 1f);
            tr.pivot = new Vector2(0.5f, 1f);
            tr.sizeDelta = new Vector2(-120f, 110f);
            tr.anchoredPosition = new Vector2(0f, -36f);
            _titleText.alignment = TextAlignmentOptions.Center;

            _bodyText = Label("Body", card, 34f, _ink, FontStyles.Normal);
            RectTransform br = _bodyText.rectTransform;
            br.anchorMin = Vector2.zero;
            br.anchorMax = Vector2.one;
            br.offsetMin = new Vector2(80f, 150f);
            br.offsetMax = new Vector2(-80f, -160f);
            _bodyText.enableAutoSizing = true;
            _bodyText.fontSizeMin = 22f;
            _bodyText.fontSizeMax = 34f;
            _bodyText.lineSpacing = 8f;

            _primary = MakeButton("Primary", card, _accent, Color.white, out _primaryLabel);
            _primary.onClick.AddListener(() => _onPrimary?.Invoke());
            _secondary = MakeButton("Secondary", card, _soft, _ink, out _secondaryLabel);
            _secondary.onClick.AddListener(() => _onSecondary?.Invoke());

            TMP_Text footer = Label("Footer", card, 22f, new Color(_ink.r, _ink.g, _ink.b, 0.55f), FontStyles.Normal);
            RectTransform fr = footer.rectTransform;
            fr.anchorMin = new Vector2(0f, 0f);
            fr.anchorMax = new Vector2(1f, 0f);
            fr.pivot = new Vector2(0.5f, 0f);
            fr.sizeDelta = new Vector2(-60f, 36f);
            fr.anchoredPosition = new Vector2(0f, 16f);
            footer.alignment = TextAlignmentOptions.Center;
            footer.text = "Enter: confirmar  ·  ESC: sair";

            _root.SetActive(false);
        }

        Button MakeButton(string name, Transform parent, Color bg, Color fg, out TMP_Text label)
        {
            RectTransform rt = Panel(name, parent, Color.white);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = new Vector2(340f, 86f);

            Button b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = rt.GetComponent<Image>();
            b.navigation = new Navigation { mode = Navigation.Mode.None };
            ColorBlock cb = b.colors;
            cb.normalColor = bg;
            cb.selectedColor = bg;
            cb.highlightedColor = Color.Lerp(bg, Color.white, 0.18f);
            cb.pressedColor = Color.Lerp(bg, Color.black, 0.18f);
            cb.colorMultiplier = 1f;
            b.colors = cb;

            label = Label("Label", rt, 34f, fg, FontStyles.Bold);
            Stretch(label.rectTransform);
            label.alignment = TextAlignmentOptions.Center;
            return b;
        }

        RectTransform Panel(string name, Transform parent, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<Image>().color = color;
            return (RectTransform)go.transform;
        }

        TMP_Text Label(string name, Transform parent, float size, Color color, FontStyles style)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = color;
            t.fontStyle = style;
            t.raycastTarget = false;
            t.enableWordWrapping = true;
            return t;
        }

        static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
    }
}
