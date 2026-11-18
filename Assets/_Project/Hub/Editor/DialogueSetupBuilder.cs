using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

namespace MovingIn.Hub.EditorTools{
    public static class DialogueSetupBuilder
    {
        const string Root = "Assets/_Project/Hub/";
        const string UiArt = Root + "Art/UI/";
        const string FontPath = Root + "Fonts/PixelifySans-Regular.ttf";
        const string FontAssetPath = Root + "Fonts/PixelifySans SDF.asset";
        const string BlipPath = Root + "Audio/Blip.wav";
        const float UiPixel = 4f;
        const float WorldPPU = 30f;
        const string FontChars = " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~ÀÁÂÃÇÉÊÍÓÔÕÚÜàáâãçéêíóôõúü…“”‘’—–ºª°";

        static readonly Color Ink = Hex("#2B2230");
        static readonly Color Paper = Hex("#FBF6EC");

        struct Comment
        {
            public string room, obj, prompt;
            public string[] lines;
            public Emote emote;
            public Comment(string room, string obj, string prompt, Emote emote, params string[] lines)
            {
                this.room = room; this.obj = obj; this.prompt = prompt; this.emote = emote; this.lines = lines;
            }
        }

        static readonly Comment[] Comments =
        {
            new Comment("Front", "Mailbox", "Olhar a caixa de correio", Emote.None, "Vazia. Ninguém sabe que a gente mora aqui."),
            new Comment("Front", "Truck_Box", "Olhar o caminhão", Emote.None, "O caminhão da mudança. Ufa, acabou."),
            new Comment("Front", "Tree_1", "Olhar a árvore", Emote.None, "Uma árvore boa pra subir.", "Talvez amanhã."),
            new Comment("LivingRoom", "Sofa", "Olhar o sofá", Emote.None, "Esse sofá veio da casa velha.", "Ainda tem o rasgo que o gato fez."),
            new Comment("LivingRoom", "TV", "Olhar a TV", Emote.None, "Sem antena ainda. Só chuvisco."),
            new Comment("LivingRoom", "Bookcase", "Olhar a estante", Emote.None, "Os livros do pai, em ordem de cor.", "Ele diz que é um sistema."),
            new Comment("LivingRoom", "MovingBoxes", "Olhar as caixas", Emote.None, "Escrito SALA. Depois eu abro."),
            new Comment("Garage", "Bicycle", "Olhar a bicicleta", Emote.None, "Minha bicicleta! O pneu tá murcho."),
            new Comment("Garage", "MetalShelves", "Olhar a prateleira", Emote.None, "Ferramentas do pai. Melhor não mexer."),
            new Comment("Kitchen", "Fridge", "Abrir a geladeira", Emote.None, "Vazia. Só uma garrafa d'água.", "E um ímã de pizzaria da cidade velha."),
            new Comment("Kitchen", "Stove", "Olhar o fogão", Emote.None, "O fogão ainda tá com o plástico."),
            new Comment("Kitchen", "SinkCounter", "Olhar a pia", Emote.Dots, "A torneira pinga. Pinga. Pinga."),
            new Comment("Laundry", "WashingMachine", "Olhar a máquina", Emote.None, "A máquina faz barulho de avião."),
            new Comment("Laundry", "UtilitySink", "Olhar o tanque", Emote.None, "Cheiro de sabão de coco."),
            new Comment("Hall", "CoatRack", "Olhar o cabideiro", Emote.None, "O casaco da mãe já tá aqui."),
            new Comment("Hall", "StuckDoor_Restroom", "Abrir o lavabo", Emote.None, "Emperrada. Depois eu vejo."),
            new Comment("Basement", "Boiler", "Olhar a caldeira", Emote.Dots, "Isso faz um barulho estranho...", "Vou fingir que não ouvi."),
            new Comment("Basement", "Shelves", "Olhar a prateleira", Emote.None, "Potes de conserva dos antigos donos.", "Quem guarda pepino por dez anos?"),
            new Comment("Basement", "OldBoxes", "Olhar as caixas velhas", Emote.None, "Caixas velhas. Não são nossas."),
            new Comment("Bedroom", "Wardrobe", "Abrir o guarda-roupa", Emote.None, "Meu guarda-roupa. Ainda vazio."),
            new Comment("Bedroom", "Desk", "Olhar a escrivaninha", Emote.None, "Aqui vai ficar o computador."),
            new Comment("Bedroom", "Bookshelf", "Olhar os gibis", Emote.None, "Meus gibis. Faltam uns três volumes."),
            new Comment("Bedroom", "Window", "Olhar pela janela", Emote.None, "Dá pra ver a rua inteira daqui."),
            new Comment("UpstairsHall", "StuckDoor_Bathroom", "Abrir o banheiro", Emote.Exclaim, "O banheiro também emperrou?!", "Essa casa não gosta de portas."),
            new Comment("UpstairsHall", "Painting", "Olhar o quadro", Emote.None, "Um quadro de pato. Veio com a casa.", "Acho que ele me olha."),
            new Comment("Attic", "Chest", "Abrir o baú", Emote.Question, "Trancado. Cadê a chave?"),
            new Comment("Attic", "OldFurniture", "Olhar os móveis", Emote.None, "Móveis cobertos com lençol. Que medo."),
            new Comment("Attic", "RoundWindow", "Olhar pela janela", Emote.None, "Parece janela de navio."),
        };

        [MenuItem("MovingIn/Setup Dialogue")]
        static void Setup()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null)
            {
                Debug.LogError("[DialogueSetup] Objeto 'Player' nao encontrado. Abra a cena Hub.");
                return;
            }

            Sprite bubbleSprite = ImportSprite(UiArt + "Bubble.png", 100f, SpriteAlignment.Center, new Vector4(5, 5, 5, 5));
            Sprite tailSprite = ImportSprite(UiArt + "Bubble_Tail.png", 100f, SpriteAlignment.Center, Vector4.zero);
            Sprite advanceSprite = ImportSprite(UiArt + "Advance.png", 100f, SpriteAlignment.Center, Vector4.zero);
            Sprite exclaim = ImportSprite(UiArt + "Emote_Exclaim.png", WorldPPU, SpriteAlignment.BottomCenter, Vector4.zero);
            Sprite question = ImportSprite(UiArt + "Emote_Question.png", WorldPPU, SpriteAlignment.BottomCenter, Vector4.zero);
            Sprite dots = ImportSprite(UiArt + "Emote_Dots.png", WorldPPU, SpriteAlignment.BottomCenter, Vector4.zero);
            Sprite sleep = ImportSprite(UiArt + "Emote_Sleep.png", WorldPPU, SpriteAlignment.BottomCenter, Vector4.zero);
            Sprite note = ImportSprite(UiArt + "Emote_Note.png", WorldPPU, SpriteAlignment.BottomCenter, Vector4.zero);

            if (bubbleSprite == null || tailSprite == null || exclaim == null)
            {
                Debug.LogError($"[DialogueSetup] Arte nao encontrada em {UiArt}. Copie a pasta Art/UI do pacote e rode de novo.");
                return;
            }

            TMP_FontAsset font = GetFont();
            AudioClip blip = AssetDatabase.LoadAssetAtPath<AudioClip>(BlipPath);
            if (blip == null) Debug.LogWarning($"[DialogueSetup] {BlipPath} nao encontrado. As falas vao funcionar sem som.");

            Camera cam = Camera.main;
            GameObject hubRoot = GameObject.Find("HubRoot");

            DestroyIfExists("Canvas_HUD");
            GameObject canvasGo = new GameObject("Canvas_HUD", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(canvasGo, "Setup Dialogue");
            if (hubRoot != null) canvasGo.transform.SetParent(hubRoot.transform, false);
            Canvas canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5;
            CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            RaiseFadeCanvas(canvas.sortingOrder);

            InteractPromptUI prompt = BuildPrompt(canvasGo.transform, bubbleSprite, font, cam);
            SpeechBubbleUI bubble = BuildBubble(canvasGo.transform, bubbleSprite, tailSprite, advanceSprite, font, cam);

            Speaker speaker = SetupPlayer(player, exclaim, question, dots, sleep, note);

            RemoveOldSystems();
            GameObject systems = FindRootObject("Systems");
            if (systems == null)
            {
                systems = new GameObject("Systems");
                Undo.RegisterCreatedObjectUndo(systems, "Setup Dialogue");
            }

            Transform oldDialogue = systems.transform.Find("Dialogue");
            if (oldDialogue != null) Undo.DestroyObjectImmediate(oldDialogue.gameObject);
            GameObject dialogueGo = new GameObject("Dialogue");
            Undo.RegisterCreatedObjectUndo(dialogueGo, "Setup Dialogue");
            dialogueGo.transform.SetParent(systems.transform, false);
            AudioSource source = dialogueGo.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            DialogueManager dm = dialogueGo.AddComponent<DialogueManager>();
            SerializedObject dso = new SerializedObject(dm);
            dso.FindProperty("bubble").objectReferenceValue = bubble;
            dso.FindProperty("playerSpeaker").objectReferenceValue = speaker;
            dso.FindProperty("voice").objectReferenceValue = source;
            dso.FindProperty("blip").objectReferenceValue = blip;
            SetArray(dso.FindProperty("lockDuringDialogue"), FindByTypeName(player, "PlayerMovement", "PlayerInteractor"));
            dso.ApplyModifiedPropertiesWithoutUndo();

            PlayerInteractor interactor = player.GetComponent<PlayerInteractor>();
            if (interactor == null) interactor = Undo.AddComponent<PlayerInteractor>(player);
            SerializedObject iso = new SerializedObject(interactor);
            SerializedProperty promptProp = iso.FindProperty("promptUI");
            if (promptProp == null)
                Debug.LogWarning("[DialogueSetup] O PlayerInteractor do Player ainda e o antigo. Substitua o arquivo PlayerInteractor.cs pelo do pacote e rode de novo.");
            else
            {
                promptProp.objectReferenceValue = prompt;
                iso.ApplyModifiedPropertiesWithoutUndo();
            }

            int placed = PlaceComments(out List<string> missing);
            if (missing.Count > 0) Debug.LogWarning("[DialogueSetup] Objetos nao encontrados para comentario: " + string.Join(", ", missing));

            Selection.activeGameObject = dialogueGo;
            Debug.Log($"[DialogueSetup] Pronto. Balao e prompt criados em Systems/Dialogue; {placed} objetos com comentario.");
        }

        static InteractPromptUI BuildPrompt(Transform canvas, Sprite bubble, TMP_FontAsset font, Camera cam)
        {
            RectTransform root = NewRect("InteractPrompt", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero);
            RectTransform box = NewRect("Box", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero);
            Image img = box.gameObject.AddComponent<Image>();
            SetSliced(img, bubble, Ink);
            TextMeshProUGUI label = NewText("Label", box, font, 26, Paper, TextAlignmentOptions.Center, false);
            CenterIn(label.rectTransform);

            InteractPromptUI ui = root.gameObject.AddComponent<InteractPromptUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("box").objectReferenceValue = box;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("cam").objectReferenceValue = cam;
            so.ApplyModifiedPropertiesWithoutUndo();
            root.gameObject.SetActive(false);
            return ui;
        }

        static SpeechBubbleUI BuildBubble(Transform canvas, Sprite bubble, Sprite tail, Sprite advance, TMP_FontAsset font, Camera cam)
        {
            RectTransform root = NewRect("SpeechBubble", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0f), Vector2.zero);
            RectTransform box = NewRect("Box", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f));
            Image img = box.gameObject.AddComponent<Image>();
            SetSliced(img, bubble, Color.white);
            TextMeshProUGUI label = NewText("Label", box, font, 30, Ink, TextAlignmentOptions.TopLeft, true);
            CenterIn(label.rectTransform);

            RectTransform adv = NewRect("Advance", box, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 10f));
            adv.sizeDelta = new Vector2(advance.rect.width * UiPixel, advance.rect.height * UiPixel);
            Image advImg = adv.gameObject.AddComponent<Image>();
            advImg.sprite = advance;
            advImg.raycastTarget = false;

            RectTransform tailRt = NewRect("Tail", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero);
            tailRt.sizeDelta = new Vector2(tail.rect.width * UiPixel, tail.rect.height * UiPixel);
            Image tailImg = tailRt.gameObject.AddComponent<Image>();
            tailImg.sprite = tail;
            tailImg.raycastTarget = false;

            SpeechBubbleUI ui = root.gameObject.AddComponent<SpeechBubbleUI>();
            SerializedObject so = new SerializedObject(ui);
            so.FindProperty("box").objectReferenceValue = box;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("tail").objectReferenceValue = tailRt;
            so.FindProperty("advanceIcon").objectReferenceValue = advImg;
            so.FindProperty("cam").objectReferenceValue = cam;
            so.FindProperty("_tailHeight").floatValue = tail.rect.height * UiPixel - UiPixel;
            so.ApplyModifiedPropertiesWithoutUndo();
            box.anchoredPosition = new Vector2(0f, tail.rect.height * UiPixel - UiPixel);
            root.gameObject.SetActive(false);
            return ui;
        }

        static Speaker SetupPlayer(GameObject player, Sprite exclaim, Sprite question, Sprite dots, Sprite sleep, Sprite note)
        {
            float feet = FeetY(player);
            Transform visual = player.transform.Find("Visual");
            Transform parent = visual != null ? visual : player.transform;
            if (visual == null) Debug.LogWarning("[DialogueSetup] Player/Visual nao encontrado. Rode antes o Setup Player Sprite (Etapa 2). O emote foi preso direto no Player.");

            Transform oldEmote = parent.Find("Emote");
            if (oldEmote != null) Undo.DestroyObjectImmediate(oldEmote.gameObject);
            GameObject emoteGo = new GameObject("Emote");
            Undo.RegisterCreatedObjectUndo(emoteGo, "Setup Dialogue");
            emoteGo.transform.SetParent(parent, false);
            float stretch = 1f / Mathf.Cos(30f * Mathf.Deg2Rad);
            emoteGo.transform.localPosition = new Vector3(0f, (visual != null ? 0f : feet) + 1.35f, 0f);
            emoteGo.transform.localScale = new Vector3(1f, visual != null ? stretch : 1f, 1f);
            SpriteRenderer sr = emoteGo.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 3;
            EmoteDisplay ed = emoteGo.AddComponent<EmoteDisplay>();
            SerializedObject eso = new SerializedObject(ed);
            eso.FindProperty("rend").objectReferenceValue = sr;
            eso.FindProperty("exclaim").objectReferenceValue = exclaim;
            eso.FindProperty("question").objectReferenceValue = question;
            eso.FindProperty("dots").objectReferenceValue = dots;
            eso.FindProperty("sleep").objectReferenceValue = sleep;
            eso.FindProperty("note").objectReferenceValue = note;
            eso.ApplyModifiedPropertiesWithoutUndo();

            Speaker speaker = player.GetComponent<Speaker>();
            if (speaker == null) speaker = Undo.AddComponent<Speaker>(player);
            SerializedObject sso = new SerializedObject(speaker);
            sso.FindProperty("_displayName").stringValue = "Garoto";
            sso.FindProperty("_headOffset").vector3Value = new Vector3(0f, feet + 1.5f, 0f);
            sso.FindProperty("_voicePitch").floatValue = 1.25f;
            sso.FindProperty("emoteDisplay").objectReferenceValue = ed;
            sso.ApplyModifiedPropertiesWithoutUndo();
            return speaker;
        }

        static int PlaceComments(out List<string> missing)
        {
            missing = new List<string>();
            GameObject house = GameObject.Find("House");
            if (house == null)
            {
                missing.Add("House (rode o Build House antes)");
                return 0;
            }

            int placed = 0;
            foreach (Comment c in Comments)
            {
                Transform room = house.transform.Find(c.room);
                Transform obj = room != null ? FindDeep(room, c.obj) : null;
                if (obj == null)
                {
                    missing.Add($"{c.room}/{c.obj}");
                    continue;
                }

                foreach (CommentInteractable old in obj.GetComponents<CommentInteractable>())
                    Undo.DestroyObjectImmediate(old);
                if (obj.GetComponent<Collider>() == null)
                {
                    BoxCollider bc = Undo.AddComponent<BoxCollider>(obj.gameObject);
                    bc.isTrigger = true;
                }

                CommentInteractable ci = Undo.AddComponent<CommentInteractable>(obj.gameObject);
                SerializedObject so = new SerializedObject(ci);
                so.FindProperty("_prompt").stringValue = c.prompt;
                so.FindProperty("_firstEmote").enumValueIndex = (int)c.emote;
                SerializedProperty lines = so.FindProperty("_lines");
                lines.arraySize = c.lines.Length;
                for (int i = 0; i < c.lines.Length; i++)
                    lines.GetArrayElementAtIndex(i).stringValue = c.lines[i];
                so.ApplyModifiedPropertiesWithoutUndo();
                placed++;
            }
            return placed;
        }

        static GameObject FindRootObject(string name)
        {
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        static void RemoveOldSystems()
        {
            List<GameObject> toRemove = new List<GameObject>();
            foreach (GameObject root in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if ((t.name == "Dialogue" || t.name == "Objectives") && t.parent != null && t.parent.name == "Systems")
                        toRemove.Add(t.gameObject);
            foreach (GameObject go in toRemove)
                if (go != null) Undo.DestroyObjectImmediate(go);
        }

        static Transform FindDeep(Transform parent, string name)
        {
            foreach (Transform child in parent)
            {
                if (child.name == name) return child;
                Transform found = FindDeep(child, name);
                if (found != null) return found;
            }
            return null;
        }

        static TMP_FontAsset GetFont()
        {
            TMP_FontAsset existing = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (existing != null) return existing;

            Font source = AssetDatabase.LoadAssetAtPath<Font>(FontPath);
            if (source == null)
            {
                Debug.LogWarning($"[DialogueSetup] {FontPath} nao encontrada. Usando a fonte padrao do TextMesh Pro.");
                return TMP_Settings.defaultFontAsset;
            }

            TMP_FontAsset fa = TMP_FontAsset.CreateFontAsset(source, 64, 6, GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, false);
            if (fa == null)
            {
                Debug.LogWarning("[DialogueSetup] Nao consegui criar a fonte. Usando a fonte padrao do TextMesh Pro.");
                return TMP_Settings.defaultFontAsset;
            }

            fa.name = "PixelifySans SDF";
            AssetDatabase.CreateAsset(fa, FontAssetPath);
            fa.atlasTextures[0].name = "PixelifySans SDF Atlas";
            AssetDatabase.AddObjectToAsset(fa.atlasTextures[0], fa);
            fa.material.name = "PixelifySans SDF Material";
            AssetDatabase.AddObjectToAsset(fa.material, fa);
            fa.TryAddCharacters(FontChars, out string _);
            EditorUtility.SetDirty(fa);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath) ?? fa;
        }

        static Sprite ImportSprite(string path, float ppu, SpriteAlignment alignment, Vector4 border)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return null;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = ppu;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)alignment;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = border;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static void SetSliced(Image img, Sprite sprite, Color color)
        {
            img.sprite = sprite;
            img.type = Image.Type.Sliced;
            img.pixelsPerUnitMultiplier = 1f / UiPixel;
            img.color = color;
            img.raycastTarget = false;
        }

        static RectTransform NewRect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = Vector2.zero;
            return rt;
        }

        static TextMeshProUGUI NewText(string name, Transform parent, TMP_FontAsset font, float size, Color color, TextAlignmentOptions align, bool wrap)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.enableWordWrapping = wrap;
            t.raycastTarget = false;
            t.richText = true;
            return t;
        }

        static void CenterIn(RectTransform rt)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
        }

        static void Stretch(RectTransform rt, Vector2 left, Vector2 rightTop, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.offsetMin = new Vector2(left.x, rightTop.y - height);
            rt.offsetMax = new Vector2(rightTop.x, rightTop.y);
        }

        static void RaiseFadeCanvas(int hudOrder)
        {
            GameObject fade = GameObject.Find("Canvas_Fade");
            if (fade == null) return;
            Canvas c = fade.GetComponent<Canvas>();
            if (c != null && c.sortingOrder <= hudOrder)
            {
                Undo.RecordObject(c, "Setup Dialogue");
                c.sortingOrder = 50;
            }
        }

        static void DestroyIfExists(string name)
        {
            GameObject go = GameObject.Find(name);
            if (go != null) Undo.DestroyObjectImmediate(go);
        }

        static float FeetY(GameObject player)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) return cc.center.y - cc.height / 2f;
            CapsuleCollider col = player.GetComponent<CapsuleCollider>();
            if (col != null) return col.center.y - col.height / 2f;
            return 0f;
        }

        static List<Behaviour> FindByTypeName(GameObject player, params string[] names)
        {
            List<Behaviour> found = new List<Behaviour>();
            HashSet<string> set = new HashSet<string>(names);
            foreach (MonoBehaviour mb in player.GetComponents<MonoBehaviour>())
                if (mb != null && set.Contains(mb.GetType().Name))
                    found.Add(mb);
            return found;
        }

        static void SetArray(SerializedProperty prop, List<Behaviour> items)
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
