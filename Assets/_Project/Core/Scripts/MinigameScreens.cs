using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MovingIn.Core{
    [RequireComponent(typeof(MinigameExit))]
    public class MinigameScreens : MonoBehaviour
    {
        public static readonly Vector2 IntroSize = new Vector2(1040f, 720f);
        public static readonly Vector2 ResultSize = new Vector2(1040f, 480f);
        public static readonly Vector2 ButtonSize = new Vector2(344f, 88f);

        static readonly Regex KeyToken = new Regex(@"\{([^{}]+)\}");

        [SerializeField] string _title = "Como jogar";
        [Tooltip("Escreva teclas entre chaves para virarem icones: {W}, {Espaço}, {ESC}")]
        [SerializeField, TextArea(5, 14)] string _howToPlay = "";
        [SerializeField, Min(0)] int _goal = 50;
        [SerializeField] TMP_FontAsset font;
        [Tooltip("Mostra a meta de pontos na tela de abertura")]
        [SerializeField] bool _showGoal = true;
        [Tooltip("Desmarque se o minigame tem a propria tela de inicio e solta o tempo sozinho")]
        [SerializeField] bool _unpauseOnPlay = true;
        [Tooltip("Texto do botao que pula o minigame (conta como concluido). Deixe vazio para esconder o botao")]
        [SerializeField] string _skipLabel = "Pular";

        [Header("Arte (opcional)")]
        [Tooltip("Fundo da tela de abertura, proporcao 1040 x 720 (ex.: 260 x 180 pixels)")]
        [SerializeField] Sprite _introCard;
        [Tooltip("Fundo da tela de resultado, proporcao 1040 x 480 (ex.: 260 x 120 pixels)")]
        [SerializeField] Sprite _resultCard;
        [Tooltip("Fundo dos botoes, proporcao 344 x 88 (ex.: 86 x 22 pixels). Fica tingido pela cor do botao")]
        [SerializeField] Sprite _buttonSprite;
        [Tooltip("Desenho da tecla (o mesmo do hub: Hub/Art/UI/Key)")]
        [SerializeField] Sprite _keySprite;
        [SerializeField] float _spritePixelScale = 0.35f;

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
        Image _frame;
        Image _card;
        TMP_Text _titleText;
        TMP_Text _bodyText;
        TMP_Text _footer;
        Button _primary;
        Button _secondary;
        TMP_Text _primaryLabel;
        TMP_Text _secondaryLabel;
        Action _onPrimary;
        Action _onSecondary;
        bool _intro;
        readonly List<GameObject> _keys = new List<GameObject>();

        void Awake()
        {
            _exit = GetComponent<MinigameExit>();
            if (_exit == null) _exit = gameObject.AddComponent<MinigameExit>();
            EnsureEventSystem();
            Build();
        }

        void Start()
        {
            _intro = true;
            Time.timeScale = 0f;
            string body = _showGoal ? $"{_howToPlay}\n\n<b>Meta: {_goal} pontos</b>" : _howToPlay;
            Show(true, _title, body, TextAlignmentOptions.TopLeft, "Jogar", Play, _skipLabel, Skip);
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
            if (_unpauseOnPlay) Time.timeScale = 1f;
        }

        public void Skip()
        {
            if (!_intro) return;
            _intro = false;
            Hide();
            _exit.Leave(true);
        }

        public void ShowResult(int points)
        {
            if (Finished) return;
            Finished = true;
            _intro = false;
            Time.timeScale = 0f;

            if (points >= _goal)
                Show(false, "Muito bem!", $"Você fez <b>{points}</b> pontos.\nMeta: {_goal} pontos.",
                     TextAlignmentOptions.Center, "Voltar pra casa", () => _exit.Leave(true), null, null);
            else
                Show(false, "Quase lá!", $"Você fez <b>{points}</b> pontos.\nA meta é {_goal}. Bora de novo?",
                     TextAlignmentOptions.Center, "Tentar de novo", _exit.Retry, "Sair", () => _exit.Leave(false));
        }

        void Show(bool intro, string title, string body, TextAlignmentOptions align, string primary, Action onPrimary,
                  string secondary, Action onSecondary)
        {
            Vector2 size = intro ? IntroSize : ResultSize;
            Sprite art = intro ? _introCard : _resultCard;
            _card.rectTransform.sizeDelta = size;
            _frame.rectTransform.sizeDelta = size + new Vector2(12f, 12f);
            _frame.enabled = art == null;
            _card.sprite = art;
            _card.color = art != null ? Color.white : _paper;

            _titleText.text = title;
            _bodyText.text = WithKeys(body);
            _bodyText.alignment = align;
            _footer.text = WithKeys(_exit.EscapeLeaves ? "{Enter} confirmar      {ESC} sair" : "{Enter} confirmar");
            _onPrimary = onPrimary;
            _onSecondary = onSecondary;
            _primaryLabel.text = primary;

            bool two = !string.IsNullOrEmpty(secondary);
            _secondary.gameObject.SetActive(two);
            if (two) _secondaryLabel.text = secondary;
            ((RectTransform)_primary.transform).anchoredPosition = new Vector2(two ? 190f : 0f, 70f);
            ((RectTransform)_secondary.transform).anchoredPosition = new Vector2(-190f, 70f);
            _root.SetActive(true);

            foreach (GameObject k in _keys) Destroy(k);
            _keys.Clear();
            try
            {
                Canvas.ForceUpdateCanvases();
                PlaceKeys(_bodyText);
                PlaceKeys(_footer);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MinigameScreens] Nao consegui desenhar as teclas: {e.Message}");
            }
        }

        void Hide()
        {
            _root.SetActive(false);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        static string WithKeys(string text)
        {
            return KeyToken.Replace(text, m => $"<link=\"{m.Groups[1].Value}\"><color=#00000000>\u00A0{m.Groups[1].Value}\u00A0</color></link>");
        }

        void PlaceKeys(TMP_Text text)
        {
            text.ForceMeshUpdate();
            TMP_TextInfo info = text.textInfo;
            for (int i = 0; i < info.linkCount; i++)
            {
                TMP_LinkInfo link = info.linkInfo[i];
                if (link.linkTextLength == 0) continue;
                TMP_CharacterInfo first = info.characterInfo[link.linkTextfirstCharacterIndex];
                TMP_CharacterInfo last = info.characterInfo[link.linkTextfirstCharacterIndex + link.linkTextLength - 1];
                if (first.lineNumber != last.lineNumber) continue;

                TMP_LineInfo line = info.lineInfo[first.lineNumber];
                TMP_CharacterInfo glyph = info.characterInfo[link.linkTextfirstCharacterIndex + Mathf.Min(1, link.linkTextLength - 1)];
                float height = (line.ascender - line.descender) * 0.85f;
                float middle = (glyph.bottomLeft.y + glyph.topRight.y) / 2f;
                var rect = new Rect(first.origin, middle - height / 2f, last.xAdvance - first.origin, height);
                MakeKey(text.rectTransform, link.GetLinkID(), rect, first.pointSize);
            }
        }

        void MakeKey(RectTransform parent, string label, Rect rect, float pointSize)
        {
            GameObject go = new GameObject("Key_" + label, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = parent.pivot;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = rect.center;
            rt.sizeDelta = rect.size;

            Image img = go.GetComponent<Image>();
            img.raycastTarget = false;
            if (_keySprite != null)
            {
                img.sprite = _keySprite;
                img.type = _keySprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                img.pixelsPerUnitMultiplier = _spritePixelScale;
                img.color = Color.white;
            }
            else
            {
                img.color = _soft;
                Shadow shadow = go.AddComponent<Shadow>();
                shadow.effectColor = new Color(_ink.r, _ink.g, _ink.b, 0.6f);
                shadow.effectDistance = new Vector2(0f, -3f);
            }

            TMP_Text t = Label("Label", rt, pointSize * 0.8f, _ink, FontStyles.Bold);
            Stretch(t.rectTransform);
            t.alignment = TextAlignmentOptions.CenterGeoAligned;
            t.enableWordWrapping = false;
            t.text = label;
            _keys.Add(go);
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

            RectTransform frame = Panel("Frame", shade, _ink);
            _frame = frame.GetComponent<Image>();
            RectTransform card = Panel("Card", frame, _paper);
            _card = card.GetComponent<Image>();

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
            br.offsetMin = new Vector2(80f, 170f);
            br.offsetMax = new Vector2(-80f, -160f);
            _bodyText.enableAutoSizing = true;
            _bodyText.fontSizeMin = 22f;
            _bodyText.fontSizeMax = 34f;
            _bodyText.lineSpacing = 14f;

            _primary = MakeButton("Primary", card, _accent, Color.white, out _primaryLabel);
            _primary.onClick.AddListener(() => _onPrimary?.Invoke());
            _secondary = MakeButton("Secondary", card, _soft, _ink, out _secondaryLabel);
            _secondary.onClick.AddListener(() => _onSecondary?.Invoke());

            _footer = Label("Footer", card, 22f, new Color(_ink.r, _ink.g, _ink.b, 0.7f), FontStyles.Normal);
            RectTransform fr = _footer.rectTransform;
            fr.anchorMin = new Vector2(0f, 0f);
            fr.anchorMax = new Vector2(1f, 0f);
            fr.pivot = new Vector2(0.5f, 0f);
            fr.sizeDelta = new Vector2(-60f, 36f);
            fr.anchoredPosition = new Vector2(0f, 16f);
            _footer.alignment = TextAlignmentOptions.Center;

            _root.SetActive(false);
        }

        Button MakeButton(string name, Transform parent, Color bg, Color fg, out TMP_Text label)
        {
            RectTransform rt = Panel(name, parent, Color.white);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.sizeDelta = ButtonSize;

            Image img = rt.GetComponent<Image>();
            if (_buttonSprite != null)
            {
                img.sprite = _buttonSprite;
                img.type = _buttonSprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
                img.pixelsPerUnitMultiplier = _spritePixelScale;
            }

            Button b = rt.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
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
