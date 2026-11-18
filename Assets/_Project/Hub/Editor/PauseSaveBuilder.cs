using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MovingIn.Hub.EditorTools{
    public static class PauseSaveBuilder
    {
        const string Root = "Assets/_Project/Hub/";
        const string BubblePath = Root + "Art/UI/Bubble.png";
        const string FontAssetPath = Root + "Fonts/PixelifySans SDF.asset";
        const float UiPixel = 4f;
        const float PanelWidth = 600f;

        static readonly Color Ink = Hex("#2B2230");
        static readonly Color Accent = Hex("#B0603A");

        static Sprite _bubble;
        static TMP_FontAsset _font;

        [MenuItem("MovingIn/Setup Pause & Save")]
        static void Setup()
        {
            _bubble = AssetDatabase.LoadAssetAtPath<Sprite>(BubblePath);
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (_bubble == null)
            {
                Debug.LogError($"[PauseSetup] {BubblePath} nao encontrado. Rode antes o Setup Dialogue (Etapa 3).");
                return;
            }

            GameObject systems = FindRoot("Systems");
            if (systems == null)
            {
                Debug.LogError("[PauseSetup] Objeto 'Systems' nao encontrado na raiz da cena.");
                return;
            }
            Transform old = systems.transform.Find("Canvas_Pause");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject canvasGo = new GameObject("Canvas_Pause", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Setup Pause");
            canvasGo.transform.SetParent(systems.transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 90;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            canvasGo.AddComponent<GraphicRaycaster>();

            RectTransform root = Stretch(NewRect("Root", canvasGo.transform));
            Image dim = root.gameObject.AddComponent<Image>();
            dim.color = new Color(0.05f, 0.04f, 0.08f, 0.6f);

            RectTransform main = Panel("Main", root);
            TextMeshProUGUI title = Label(main, "Title", "Pausado", 44, TextAlignmentOptions.Center, 64f);
            Button resume = MenuButton(main, "Button_Resume", "Continuar");
            Button options = MenuButton(main, "Button_Options", "Opções");
            Button save = MenuButton(main, "Button_Save", "Salvar jogo");
            Button load = MenuButton(main, "Button_Load", "Carregar jogo");
            Button leave = MenuButton(main, "Button_LeaveMinigame", "Sair do minigame");
            Button quit = MenuButton(main, "Button_Quit", "Sair do jogo");

            RectTransform opt = Panel("Options", root);
            Label(opt, "Title", "Opções", 44, TextAlignmentOptions.Center, 64f);
            Button volume = OptionRow(opt, "Row_Volume", "Volume", out TextMeshProUGUI volumeValue);
            Button text = OptionRow(opt, "Row_TextSpeed", "Velocidade do texto", out TextMeshProUGUI textValue);
            Button full = OptionRow(opt, "Row_Fullscreen", "Tela cheia", out TextMeshProUGUI fullValue);
            Button pixel = OptionRow(opt, "Row_Pixel", "Efeito pixel", out TextMeshProUGUI pixelValue);
            Label(opt, "Hint", "Clique ou use as setas para mudar", 20, TextAlignmentOptions.Center, 30f).color = Accent;
            Button optBack = MenuButton(opt, "Button_Back", "Voltar");

            RectTransform slots = Panel("Slots", root);
            TextMeshProUGUI slotsTitle = Label(slots, "Title", "Salvar jogo", 44, TextAlignmentOptions.Center, 64f);
            List<Button> slotButtons = new List<Button>();
            List<TextMeshProUGUI> slotLabels = new List<TextMeshProUGUI>();
            for (int i = 0; i <= SaveSystem.ManualSlots; i++)
            {
                Button b = MenuButton(slots, i == 0 ? "Slot_Auto" : $"Slot_{i}", "", 96f);
                TextMeshProUGUI l = b.GetComponentInChildren<TextMeshProUGUI>();
                l.alignment = TextAlignmentOptions.Left;
                l.rectTransform.offsetMin = new Vector2(24f, 0f);
                l.rectTransform.offsetMax = new Vector2(-24f, 0f);
                l.fontSize = 28;
                slotButtons.Add(b);
                slotLabels.Add(l);
            }
            TextMeshProUGUI feedback = Label(slots, "Feedback", "", 22, TextAlignmentOptions.Center, 30f);
            feedback.color = Accent;
            Button slotsBack = MenuButton(slots, "Button_Back", "Voltar");

            PauseMenu menu = canvasGo.AddComponent<PauseMenu>();

            OptionsScreen os = opt.gameObject.AddComponent<OptionsScreen>();
            SerializedObject oso = new SerializedObject(os);
            oso.FindProperty("menu").objectReferenceValue = menu;
            oso.FindProperty("volumeRow").objectReferenceValue = volume;
            oso.FindProperty("textRow").objectReferenceValue = text;
            oso.FindProperty("fullscreenRow").objectReferenceValue = full;
            oso.FindProperty("pixelRow").objectReferenceValue = pixel;
            oso.FindProperty("backButton").objectReferenceValue = optBack;
            oso.FindProperty("volumeValue").objectReferenceValue = volumeValue;
            oso.FindProperty("textValue").objectReferenceValue = textValue;
            oso.FindProperty("fullscreenValue").objectReferenceValue = fullValue;
            oso.FindProperty("pixelValue").objectReferenceValue = pixelValue;
            oso.ApplyModifiedPropertiesWithoutUndo();

            SlotsScreen ss = slots.gameObject.AddComponent<SlotsScreen>();
            SerializedObject sso = new SerializedObject(ss);
            sso.FindProperty("menu").objectReferenceValue = menu;
            sso.FindProperty("title").objectReferenceValue = slotsTitle;
            sso.FindProperty("feedback").objectReferenceValue = feedback;
            sso.FindProperty("backButton").objectReferenceValue = slotsBack;
            SetArray(sso.FindProperty("slotButtons"), slotButtons.ConvertAll(b => (Object)b));
            SetArray(sso.FindProperty("slotLabels"), slotLabels.ConvertAll(l => (Object)l));
            sso.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject mso = new SerializedObject(menu);
            mso.FindProperty("root").objectReferenceValue = root.gameObject;
            mso.FindProperty("mainScreen").objectReferenceValue = main.gameObject;
            mso.FindProperty("optionsScreen").objectReferenceValue = opt.gameObject;
            mso.FindProperty("slotsScreen").objectReferenceValue = ss;
            mso.FindProperty("title").objectReferenceValue = title;
            mso.FindProperty("resumeButton").objectReferenceValue = resume;
            mso.FindProperty("optionsButton").objectReferenceValue = options;
            mso.FindProperty("saveButton").objectReferenceValue = save;
            mso.FindProperty("loadButton").objectReferenceValue = load;
            mso.FindProperty("leaveMinigameButton").objectReferenceValue = leave;
            mso.FindProperty("quitButton").objectReferenceValue = quit;
            mso.ApplyModifiedPropertiesWithoutUndo();

            opt.gameObject.SetActive(false);
            slots.gameObject.SetActive(false);
            root.gameObject.SetActive(false);

            Selection.activeGameObject = canvasGo;
            Debug.Log("[PauseSetup] Pronto. Menu de pausa criado em Systems/Canvas_Pause. Aperte ESC no Play.");
        }

        static RectTransform Panel(string name, Transform parent)
        {
            RectTransform p = NewRect(name, parent);
            p.anchorMin = p.anchorMax = new Vector2(0.5f, 0.5f);
            p.pivot = new Vector2(0.5f, 0.5f);
            p.sizeDelta = new Vector2(PanelWidth, 0f);
            Image img = p.gameObject.AddComponent<Image>();
            img.sprite = _bubble;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f / UiPixel;
            VerticalLayoutGroup v = p.gameObject.AddComponent<VerticalLayoutGroup>();
            v.padding = new RectOffset(40, 40, 28, 36);
            v.spacing = 12f;
            v.childAlignment = TextAnchor.UpperCenter;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            ContentSizeFitter fit = p.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            return p;
        }

        static TextMeshProUGUI Label(Transform parent, string name, string text, float size, TextAlignmentOptions align, float height)
        {
            RectTransform rt = NewRect(name, parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font != null) t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.color = Ink;
            t.alignment = align;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
            return t;
        }

        static Button MenuButton(Transform parent, string name, string label, float height = 62f)
        {
            RectTransform rt = NewRect(name, parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = height;
            Image img = rt.gameObject.AddComponent<Image>();
            img.sprite = _bubble;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f / UiPixel;
            Button b = rt.gameObject.AddComponent<Button>();
            ColorBlock cb = b.colors;
            cb.normalColor = Hex("#F1E9DC");
            cb.highlightedColor = Hex("#FFD9B0");
            cb.selectedColor = Hex("#FFD9B0");
            cb.pressedColor = Hex("#F2B98A");
            cb.disabledColor = new Color(1f, 1f, 1f, 0.4f);
            cb.fadeDuration = 0.05f;
            b.colors = cb;
            Navigation nav = b.navigation;
            nav.mode = Navigation.Mode.Vertical;
            b.navigation = nav;

            RectTransform lr = Stretch(NewRect("Label", rt));
            TextMeshProUGUI t = lr.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font != null) t.font = _font;
            t.text = label;
            t.fontSize = 30;
            t.color = Ink;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
            return b;
        }

        static Button OptionRow(Transform parent, string name, string label, out TextMeshProUGUI value)
        {
            Button b = MenuButton(parent, name, label);
            TextMeshProUGUI l = b.GetComponentInChildren<TextMeshProUGUI>();
            l.alignment = TextAlignmentOptions.Left;
            l.rectTransform.offsetMin = new Vector2(24f, 0f);
            l.fontSize = 28;

            RectTransform vr = Stretch(NewRect("Value", b.transform));
            vr.offsetMax = new Vector2(-20f, 0f);
            value = vr.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font != null) value.font = _font;
            value.text = "<  -  >";
            value.fontSize = 28;
            value.color = Accent;
            value.alignment = TextAlignmentOptions.Right;
            value.enableWordWrapping = false;
            value.raycastTarget = false;
            return b;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        static GameObject FindRoot(string name)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.name == name) return go;
            return null;
        }

        static void SetArray(SerializedProperty prop, List<Object> items)
        {
            prop.arraySize = items.Count;
            for (int i = 0; i < items.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = items[i];
        }

        static Color Hex(string h)
        {
            ColorUtility.TryParseHtmlString(h, out Color c);
            return c;
        }
    }
}
