using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MovingIn.Hub.EditorTools{
    public static class MenuEndingBuilder
    {
        const string Root = "Assets/_Project/Hub/";
        const string NpcArt = Root + "Art/NPC/";
        const string ShadowPath = Root + "Art/Player/Player_Shadow.png";
        const string BubblePath = Root + "Art/UI/Bubble.png";
        const string FontAssetPath = Root + "Fonts/PixelifySans SDF.asset";
        const string MaterialFolder = Root + "Materials/Story";
        const float WorldPPU = 30f;
        const float UiPixel = 4f;

        static readonly Color Ink = Hex("#2B2230");
        static readonly Color Paper = Hex("#FBF6EC");
        static readonly Color Accent = Hex("#B0603A");

        static Sprite _bubble;
        static TMP_FontAsset _font;
        static Sprite _shadow;

        [MenuItem("MovingIn/Setup Menu & Ending")]
        static void Setup()
        {
            GameObject house = GameObject.Find("House");
            GameObject player = GameObject.Find("Player");
            GameObject systems = FindRoot("Systems");
            Transform front = house != null ? house.transform.Find("Front") : null;
            TimeOfDay tod = Object.FindObjectOfType<TimeOfDay>(true);
            if (house == null || player == null || systems == null || front == null)
            {
                Debug.LogError("[MenuSetup] Abra a cena Hub. Precisa de House/Front, Player e Systems.");
                return;
            }
            if (tod == null || Object.FindObjectOfType<PauseMenu>(true) == null)
            {
                Debug.LogError("[MenuSetup] Rode antes o Setup Story (Etapa 4) e o Setup Pause & Save (Etapa 5).");
                return;
            }

            _bubble = AssetDatabase.LoadAssetAtPath<Sprite>(BubblePath);
            _font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            _shadow = AssetDatabase.LoadAssetAtPath<Sprite>(ShadowPath);
            Dictionary<string, Sprite> npc = new Dictionary<string, Sprite>();
            foreach (string n in new[] { "Mover_A_Front", "Mover_A_Back", "Mover_B_Front", "Mover_B_Back", "Mom_Front", "Mom_Back", "Dad_Front", "Dad_Back", "Box" })
            {
                Sprite s = ImportWorldSprite(NpcArt + n + ".png");
                if (s == null)
                {
                    Debug.LogError($"[MenuSetup] {NpcArt}{n}.png nao encontrado. Copie a pasta Art/NPC do pacote.");
                    return;
                }
                npc[n] = s;
            }

            AddDayPreset(tod);

            Transform cut = ResetGroup(front, "Cutscene");
            Transform door = Point(cut, "DoorPoint", 10f, -0.6f);
            Transform playerDoor = Point(cut, "PlayerDoor", 10f, -1.8f);
            Transform gate = Point(cut, "Gate", 10f, -10.6f);
            Transform truckBack = Point(cut, "TruckBack", 10.8f, -12.3f);
            Transform cabDoor = Point(cut, "CabDoor", 16.6f, -11.8f);
            Transform playerStop = Point(cut, "PlayerStop", 11.2f, -3.4f);
            Transform kidSpot = Point(cut, "KidSpot", 10f, -2.4f);
            Transform momSpot = Point(cut, "MomSpot", 9f, -2.7f);
            Transform dadSpot = Point(cut, "DadSpot", 11f, -2.7f);
            Transform carDoor = Point(cut, "CarDoor", 6.2f, -12.2f);
            Transform[] parentPath =
            {
                Point(cut, "ParentPath_1", 9.6f, -12f),
                Point(cut, "ParentPath_2", 10f, -10.6f),
                Point(cut, "ParentPath_3", 10f, -8.5f),
            };
            GameObject photoExtras = new GameObject("PhotoExtras");
            photoExtras.transform.SetParent(cut, false);

            Transform truck = BuildTruck(front);
            NpcWalker moverA = BuildNpc(cut, "Mover_A", npc["Mover_A_Front"], npc["Mover_A_Back"], npc["Box"], truckBack.position, new[] { truckBack, gate, door }, cabDoor, 0f, null, 1f);
            NpcWalker moverB = BuildNpc(cut, "Mover_B", npc["Mover_B_Front"], npc["Mover_B_Back"], npc["Box"], truckBack.position, new[] { truckBack, gate, door }, cabDoor, 2.6f, null, 1f);
            NpcWalker mom = BuildNpc(cut, "Mom", npc["Mom_Front"], npc["Mom_Back"], null, momSpot.position, new[] { carDoor }, carDoor, 0f, "Mãe", 1.05f);
            NpcWalker dad = BuildNpc(cut, "Dad", npc["Dad_Front"], npc["Dad_Back"], null, dadSpot.position, new[] { carDoor }, carDoor, 0f, "Pai", 0.8f);
            Transform car = BuildCar(cut);
            BuildClouds(cut);

            List<Object> followers = new List<Object>();
            foreach (RoomCameraFollow f in front.GetComponentsInChildren<RoomCameraFollow>(true)) followers.Add(f);
            followers.AddRange(FindByTypeName(null, "IsoCamera"));
            List<Object> control = FindByTypeName(player, "PlayerMovement", "PlayerInteractor");
            Room frontRoom = front.GetComponent<Room>();
            Transform frontSpawn = FindDeep(front, "Spawn_From_LivingRoom");
            Transform visual = player.transform.Find("Visual");
            Speaker playerSpeaker = player.GetComponent<Speaker>();

            BuildMenu(systems.transform, tod, player, visual, followers, control, new[] { moverA, moverB }, truck, playerDoor, playerStop);
            BuildEnding(systems.transform, player, playerSpeaker, followers, frontRoom, frontSpawn, car, mom, dad, momSpot, dadSpot, kidSpot, carDoor, parentPath, photoExtras);

            Debug.Log("[MenuSetup] Pronto. Menu inicial, carregadores, caminhão, carro dos pais, nuvens e final criados.");
        }

        static void AddDayPreset(TimeOfDay tod)
        {
            SerializedObject so = new SerializedObject(tod);
            SerializedProperty list = so.FindProperty("presets");
            int index = -1;
            for (int i = 0; i < list.arraySize; i++)
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("name").stringValue == "Day") index = i;
            if (index < 0)
            {
                list.arraySize++;
                index = list.arraySize - 1;
            }
            SerializedProperty e = list.GetArrayElementAtIndex(index);
            e.FindPropertyRelative("name").stringValue = "Day";
            e.FindPropertyRelative("sunColor").colorValue = Hex("#FFF4E0");
            e.FindPropertyRelative("sunIntensity").floatValue = 1.1f;
            e.FindPropertyRelative("sunRotation").vector3Value = new Vector3(50f, -30f, 0f);
            e.FindPropertyRelative("ambient").colorValue = Hex("#9FA8B8");
            e.FindPropertyRelative("background").colorValue = Hex("#8EC5F0");
            e.FindPropertyRelative("roomLights").floatValue = 0f;
            e.FindPropertyRelative("characterTint").colorValue = Color.white;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Transform BuildTruck(Transform front)
        {
            Transform truck = front.Find("Truck");
            if (truck == null)
            {
                GameObject go = new GameObject("Truck");
                Undo.RegisterCreatedObjectUndo(go, "Setup Menu");
                go.transform.SetParent(front, false);
                truck = go.transform;
            }
            truck.gameObject.SetActive(true);
            foreach (string part in new[] { "Truck_Box", "Truck_Cab" })
            {
                Transform t = FindDeep(front, part);
                if (t != null && t.parent != truck) Undo.SetTransformParent(t, truck, "Setup Menu");
            }
            if (truck.childCount == 0) Debug.LogWarning("[MenuSetup] Truck_Box e Truck_Cab nao encontrados. O caminhao nao vai andar.");
            return truck;
        }

        static NpcWalker BuildNpc(Transform parent, string name, Sprite front, Sprite back, Sprite box, Vector3 pos, Transform[] path, Transform exit, float delay, string speakerName, float pitch)
        {
            GameObject root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(pos.x, 0f, pos.z);

            GameObject visual = new GameObject("Visual");
            visual.transform.SetParent(root.transform, false);
            GameObject body = new GameObject("Body");
            body.transform.SetParent(visual.transform, false);
            SpriteRenderer bodyRenderer = body.AddComponent<SpriteRenderer>();
            bodyRenderer.sprite = front;
            bodyRenderer.sortingOrder = 1;

            GameObject shadow = new GameObject("Shadow");
            shadow.transform.SetParent(visual.transform, false);
            shadow.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            shadow.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadow.transform.localScale = new Vector3(1.3f, 1.3f, 1f);
            SpriteRenderer shadowRenderer = shadow.AddComponent<SpriteRenderer>();
            shadowRenderer.sprite = _shadow;
            shadowRenderer.color = new Color(0f, 0f, 0f, 0.3f);

            SpriteRenderer boxRenderer = null;
            if (box != null)
            {
                GameObject b = new GameObject("Box");
                b.transform.SetParent(visual.transform, false);
                b.transform.localPosition = new Vector3(0f, 0.5f, 0f);
                b.transform.localScale = new Vector3(1f, 1f / Mathf.Cos(30f * Mathf.Deg2Rad), 1f);
                boxRenderer = b.AddComponent<SpriteRenderer>();
                boxRenderer.sprite = box;
                boxRenderer.sortingOrder = 2;
            }

            PlayerSpriteVisual pv = visual.AddComponent<PlayerSpriteVisual>();
            SerializedObject pso = new SerializedObject(pv);
            pso.FindProperty("target").objectReferenceValue = root.transform;
            pso.FindProperty("body").objectReferenceValue = bodyRenderer;
            pso.FindProperty("shadow").objectReferenceValue = shadow.transform;
            pso.FindProperty("frontSprite").objectReferenceValue = front;
            pso.FindProperty("backSprite").objectReferenceValue = back;
            pso.FindProperty("cam").objectReferenceValue = Camera.main;
            pso.ApplyModifiedPropertiesWithoutUndo();

            NpcWalker walker = root.AddComponent<NpcWalker>();
            SerializedObject wso = new SerializedObject(walker);
            wso.FindProperty("visual").objectReferenceValue = visual;
            wso.FindProperty("box").objectReferenceValue = boxRenderer;
            wso.FindProperty("sprite").objectReferenceValue = pv;
            SerializedProperty list = wso.FindProperty("toHouse");
            list.arraySize = path.Length;
            for (int i = 0; i < path.Length; i++) list.GetArrayElementAtIndex(i).objectReferenceValue = path[i];
            wso.FindProperty("exitPoint").objectReferenceValue = exit;
            wso.FindProperty("_startDelay").floatValue = delay;
            wso.ApplyModifiedPropertiesWithoutUndo();

            if (speakerName != null)
            {
                Speaker sp = root.AddComponent<Speaker>();
                SerializedObject sso = new SerializedObject(sp);
                sso.FindProperty("_displayName").stringValue = speakerName;
                sso.FindProperty("_headOffset").vector3Value = new Vector3(0f, 1.75f, 0f);
                sso.FindProperty("_voicePitch").floatValue = pitch;
                sso.ApplyModifiedPropertiesWithoutUndo();
            }
            return walker;
        }

        static Transform BuildCar(Transform parent)
        {
            GameObject car = new GameObject("ParentsCar");
            car.transform.SetParent(parent, false);
            car.transform.position = new Vector3(5.5f, 0f, -13.6f);
            Cube(car.transform, "Body", new Vector3(0f, 0.75f, 0f), new Vector3(4f, 1.1f, 2f), "#6FA0C8");
            Cube(car.transform, "Cabin", new Vector3(-0.2f, 1.7f, 0f), new Vector3(2.2f, 0.8f, 1.8f), "#CFE3EE");
            Cube(car.transform, "Wheels_Front", new Vector3(1.3f, 0.35f, 0f), new Vector3(0.7f, 0.7f, 2.06f), "#2B2230");
            Cube(car.transform, "Wheels_Back", new Vector3(-1.3f, 0.35f, 0f), new Vector3(0.7f, 0.7f, 2.06f), "#2B2230");
            return car.transform;
        }

        static void BuildClouds(Transform parent)
        {
            Transform sky = new GameObject("Sky").transform;
            sky.SetParent(parent, false);
            Vector3 focus = new Vector3(18.65f, 1.5f, -8.35f);
            Vector3 camPos = focus - Quaternion.Euler(30f, 45f, 0f) * Vector3.forward * 30f;
            Quaternion skyRot = Quaternion.Euler(-14f, 45f, 0f);
            Vector3 fwd = skyRot * Vector3.forward;
            Vector3 right = skyRot * Vector3.right;
            Vector3 up = skyRot * Vector3.up;
            float[,] layout = { { 35f, -12f, 4f }, { 45f, 2f, 7f }, { 40f, 14f, 2f }, { 55f, -4f, -1f }, { 50f, 20f, 8f }, { 38f, -20f, -3f } };
            for (int i = 0; i < layout.GetLength(0); i++)
            {
                Vector3 p = camPos + fwd * layout[i, 0] + right * layout[i, 1] + up * layout[i, 2];
                GameObject c = Cube(sky, $"Cloud_{i + 1}", Vector3.zero, new Vector3(4f + i % 3, 1f, 2.5f + i % 2), "#FFFFFF");
                c.transform.position = p;
                c.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                c.AddComponent<CloudDrift>();
            }
        }

        static void BuildMenu(Transform systems, TimeOfDay tod, GameObject player, Transform visual, List<Object> followers, List<Object> control, NpcWalker[] movers, Transform truck, Transform door, Transform stop)
        {
            Transform old = systems.Find("Canvas_Menu");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject canvasGo = NewCanvas("Canvas_Menu", systems, 60);
            CanvasGroup group = canvasGo.AddComponent<CanvasGroup>();

            RectTransform panel = NewRect("Panel", canvasGo.transform);
            panel.anchorMin = panel.anchorMax = new Vector2(1f, 0.5f);
            panel.pivot = new Vector2(1f, 0.5f);
            panel.anchoredPosition = new Vector2(-130f, 0f);
            panel.sizeDelta = new Vector2(520f, 0f);
            VerticalLayoutGroup v = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = 14f;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = false;
            ContentSizeFitter fit = panel.gameObject.AddComponent<ContentSizeFitter>();
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            RectTransform logo = NewRect("Logo", panel);
            logo.gameObject.AddComponent<LayoutElement>().preferredHeight = 170f;
            TextMeshProUGUI shadowText = Text(logo, "Shadow", "Moving In", 104, Ink, TextAlignmentOptions.Center);
            Stretch(shadowText.rectTransform, new Vector2(7f, -7f));
            TextMeshProUGUI logoText = Text(logo, "Title", "Moving In", 104, Paper, TextAlignmentOptions.Center);
            Stretch(logoText.rectTransform, Vector2.zero);
            RectTransform spacer = NewRect("Spacer", panel);
            spacer.gameObject.AddComponent<LayoutElement>().preferredHeight = 10f;

            Button newGame = MenuButton(panel, "Button_NewGame", "Novo jogo");
            Button cont = MenuButton(panel, "Button_Continue", "Continuar");
            Button options = MenuButton(panel, "Button_Options", "Opções");
            Button quit = MenuButton(panel, "Button_Quit", "Sair");

            MainMenu menu = canvasGo.AddComponent<MainMenu>();
            SerializedObject so = new SerializedObject(menu);
            so.FindProperty("cam").objectReferenceValue = Camera.main;
            so.FindProperty("player").objectReferenceValue = player.transform;
            so.FindProperty("playerVisual").objectReferenceValue = visual != null ? visual.gameObject : null;
            SetArray(so.FindProperty("cameraFollowers"), followers);
            SetArray(so.FindProperty("playerControl"), control);
            so.FindProperty("timeOfDay").objectReferenceValue = tod;
            SetArray(so.FindProperty("movers"), new List<Object>(movers));
            so.FindProperty("truck").objectReferenceValue = truck;
            so.FindProperty("frontDoor").objectReferenceValue = door;
            so.FindProperty("playerStop").objectReferenceValue = stop;
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("newGameButton").objectReferenceValue = newGame;
            so.FindProperty("continueButton").objectReferenceValue = cont;
            so.FindProperty("optionsButton").objectReferenceValue = options;
            so.FindProperty("quitButton").objectReferenceValue = quit;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void BuildEnding(Transform systems, GameObject player, Speaker playerSpeaker, List<Object> followers, Room frontRoom, Transform frontSpawn, Transform car,
                                NpcWalker mom, NpcWalker dad, Transform momSpot, Transform dadSpot, Transform kidSpot, Transform carDoor, Transform[] parentPath, GameObject photoExtras)
        {
            Transform old = systems.Find("Canvas_Ending");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject canvasGo = NewCanvas("Canvas_Ending", systems, 70);

            RectTransform polaroid = NewRect("Polaroid", canvasGo.transform);
            polaroid.anchorMin = polaroid.anchorMax = new Vector2(0.5f, 0.5f);
            polaroid.sizeDelta = new Vector2(620f, 470f);
            polaroid.anchoredPosition = new Vector2(0f, 20f);
            polaroid.localEulerAngles = new Vector3(0f, 0f, -3f);
            Image frame = polaroid.gameObject.AddComponent<Image>();
            frame.sprite = _bubble;
            frame.type = Image.Type.Sliced;
            frame.pixelsPerUnitMultiplier = 1f / UiPixel;
            RectTransform photoRt = NewRect("Photo", polaroid);
            photoRt.anchorMin = photoRt.anchorMax = new Vector2(0.5f, 1f);
            photoRt.pivot = new Vector2(0.5f, 1f);
            photoRt.anchoredPosition = new Vector2(0f, -26f);
            photoRt.sizeDelta = new Vector2(568f, 320f);
            RawImage photo = photoRt.gameObject.AddComponent<RawImage>();
            Stretch(NewRect("Stickers", polaroid), Vector2.zero).SetAsLastSibling();
            TextMeshProUGUI caption = Text(polaroid, "Caption", "Casa nova", 36, Ink, TextAlignmentOptions.Center);
            caption.rectTransform.anchorMin = caption.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            caption.rectTransform.pivot = new Vector2(0.5f, 0f);
            caption.rectTransform.anchoredPosition = new Vector2(0f, 34f);
            caption.rectTransform.sizeDelta = new Vector2(560f, 60f);

            CanvasGroup flash = FullImage(canvasGo.transform, "Flash", Color.white);
            CanvasGroup black = FullImage(canvasGo.transform, "Black", Color.black);

            CanvasGroup credits = FullImage(canvasGo.transform, "Credits", Hex("#15121C"));
            RectTransform viewport = Stretch(NewRect("Viewport", credits.transform), Vector2.zero);
            viewport.gameObject.AddComponent<RectMask2D>();
            RectTransform content = NewRect("Content", viewport);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(1200f, 100f);
            TextMeshProUGUI creditsText = Text(content, "Text", "", 34, Paper, TextAlignmentOptions.Center);
            creditsText.rectTransform.anchorMin = creditsText.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            creditsText.rectTransform.sizeDelta = new Vector2(1200f, 100f);
            creditsText.enableWordWrapping = true;
            creditsText.overflowMode = TextOverflowModes.Overflow;
            TextMeshProUGUI hint = Text(credits.transform, "ContinueHint", "Aperte E para continuar explorando a casa", 26, Hex("#FFD9B0"), TextAlignmentOptions.Center);
            hint.rectTransform.anchorMin = hint.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            hint.rectTransform.pivot = new Vector2(0.5f, 0f);
            hint.rectTransform.anchoredPosition = new Vector2(0f, 60f);
            hint.rectTransform.sizeDelta = new Vector2(1200f, 50f);

            EndingDirector ed = canvasGo.AddComponent<EndingDirector>();
            SerializedObject so = new SerializedObject(ed);
            so.FindProperty("cam").objectReferenceValue = Camera.main;
            so.FindProperty("player").objectReferenceValue = player.transform;
            so.FindProperty("playerSpeaker").objectReferenceValue = playerSpeaker;
            SetArray(so.FindProperty("cameraFollowers"), followers);
            so.FindProperty("frontRoom").objectReferenceValue = frontRoom;
            so.FindProperty("frontSpawn").objectReferenceValue = frontSpawn;
            so.FindProperty("car").objectReferenceValue = car;
            so.FindProperty("mom").objectReferenceValue = mom;
            so.FindProperty("dad").objectReferenceValue = dad;
            so.FindProperty("momSpot").objectReferenceValue = momSpot;
            so.FindProperty("dadSpot").objectReferenceValue = dadSpot;
            so.FindProperty("kidSpot").objectReferenceValue = kidSpot;
            so.FindProperty("carDoor").objectReferenceValue = carDoor;
            SerializedProperty path = so.FindProperty("arrivalPath");
            path.arraySize = parentPath.Length;
            for (int i = 0; i < parentPath.Length; i++) path.GetArrayElementAtIndex(i).objectReferenceValue = parentPath[i];
            so.FindProperty("photoExtras").objectReferenceValue = photoExtras;
            so.FindProperty("black").objectReferenceValue = black;
            so.FindProperty("flash").objectReferenceValue = flash;
            so.FindProperty("polaroid").objectReferenceValue = polaroid;
            so.FindProperty("photo").objectReferenceValue = photo;
            so.FindProperty("caption").objectReferenceValue = caption;
            so.FindProperty("credits").objectReferenceValue = credits;
            so.FindProperty("creditsContent").objectReferenceValue = content;
            so.FindProperty("creditsText").objectReferenceValue = creditsText;
            so.FindProperty("continueHint").objectReferenceValue = hint;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject NewCanvas(string name, Transform parent, int order)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Setup Menu");
            go.transform.SetParent(parent, false);
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            CanvasScaler scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 1f;
            go.AddComponent<GraphicRaycaster>();
            return go;
        }

        static CanvasGroup FullImage(Transform parent, string name, Color color)
        {
            RectTransform rt = Stretch(NewRect(name, parent), Vector2.zero);
            Image img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            CanvasGroup g = rt.gameObject.AddComponent<CanvasGroup>();
            g.alpha = 0f;
            g.blocksRaycasts = false;
            g.interactable = false;
            return g;
        }

        static Button MenuButton(Transform parent, string name, string label)
        {
            RectTransform rt = NewRect(name, parent);
            rt.gameObject.AddComponent<LayoutElement>().preferredHeight = 66f;
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
            cb.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            cb.fadeDuration = 0.05f;
            b.colors = cb;
            Navigation nav = b.navigation;
            nav.mode = Navigation.Mode.Vertical;
            b.navigation = nav;
            TextMeshProUGUI t = Text(rt, "Label", label, 32, Ink, TextAlignmentOptions.Center);
            Stretch(t.rectTransform, Vector2.zero);
            return b;
        }

        static TextMeshProUGUI Text(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions align)
        {
            RectTransform rt = NewRect(name, parent);
            TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            if (_font != null) t.font = _font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
            return t;
        }

        static GameObject Cube(Transform parent, string name, Vector3 localPos, Vector3 size, string hex)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = ColorMaterial(hex);
            return go;
        }

        static Material ColorMaterial(string hex)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                if (!AssetDatabase.IsValidFolder(Root + "Materials")) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Materials");
                AssetDatabase.CreateFolder(Root + "Materials", "Story");
            }
            string path = $"{MaterialFolder}/M_{hex.TrimStart('#')}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Standard")) { color = Hex(hex) };
            m.SetFloat("_Glossiness", 0.1f);
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Sprite ImportWorldSprite(string path)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return null;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = WorldPPU;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.BottomCenter;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Transform Point(Transform parent, string name, float x, float z)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(x, 0f, z);
            return go.transform;
        }

        static Transform ResetGroup(Transform parent, string name)
        {
            Transform old = parent.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Setup Menu");
            go.transform.SetParent(parent, false);
            return go.transform;
        }

        static RectTransform NewRect(string name, Transform parent)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        static RectTransform Stretch(RectTransform rt, Vector2 offset)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = offset;
            rt.offsetMax = offset;
            return rt;
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

        static GameObject FindRoot(string name)
        {
            foreach (GameObject go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.name == name) return go;
            return null;
        }

        static List<Object> FindByTypeName(GameObject owner, params string[] names)
        {
            List<Object> found = new List<Object>();
            HashSet<string> set = new HashSet<string>(names);
            MonoBehaviour[] all = owner != null ? owner.GetComponents<MonoBehaviour>() : Object.FindObjectsOfType<MonoBehaviour>(true);
            foreach (MonoBehaviour mb in all)
                if (mb != null && set.Contains(mb.GetType().Name))
                    found.Add(mb);
            return found;
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
