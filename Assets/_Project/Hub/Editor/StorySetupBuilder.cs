using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MovingIn.Hub.EditorTools{
    public static class StorySetupBuilder
    {
        const string Root = "Assets/_Project/Hub/";
        const string BubblePath = Root + "Art/UI/Bubble.png";
        const string PhonePath = Root + "Art/UI/Phone.png";
        const string KeyPath = Root + "Art/UI/Key.png";
        const string OutlineShader = "MovingIn/OutlineHull";
        const string RingPath = Root + "Audio/Ring.wav";
        const string FontAssetPath = Root + "Fonts/PixelifySans SDF.asset";
        const string MaterialFolder = Root + "Materials/Story";
        const float UiPixel = 4f;
        const float CeilingHeight = 2.6f;

        static readonly Color Ink = Hex("#2B2230");
        static readonly Color Accent = Hex("#B0603A");
        static readonly Color Warm = Hex("#FFD9A8");
        static readonly Color Alarm = Hex("#FF4A3A");

        struct RoomRect
        {
            public string name; public float y, x0, z0, x1, z1;
            public RoomRect(string n, float y, float x0, float z0, float x1, float z1) { name = n; this.y = y; this.x0 = x0; this.z0 = z0; this.x1 = x1; this.z1 = z1; }
        }

        static readonly RoomRect[] Interiors =
        {
            new RoomRect("Garage", 0, 0, 0, 6, 8),
            new RoomRect("LivingRoom", 0, 6, 0, 14, 8),
            new RoomRect("Kitchen", 0, 14, 0, 21, 8),
            new RoomRect("Hall", 0, 6, 8, 11, 12),
            new RoomRect("Laundry", 0, 14, 8, 21, 12),
            new RoomRect("Basement", -20, 4, 0, 17, 8),
            new RoomRect("Bedroom", 20, 0, 0, 10, 8),
            new RoomRect("UpstairsHall", 20, 10, 0, 14, 8),
            new RoomRect("Attic", 40, 2, 0, 14, 8),
        };

        struct Tracked
        {
            public string room, obj, id, doneLine;
            public Tracked(string room, string obj, string id, string doneLine) { this.room = room; this.obj = obj; this.id = id; this.doneLine = doneLine; }
        }

        static readonly Tracked[] Minigames =
        {
            new Tracked("Basement", "BreakerPanel", "basement_breaker", ""),
            new Tracked("Kitchen", "KitchenBox", "kitchen_box", "Cozinha pronta!"),
            new Tracked("Laundry", "LaundryBasket", "laundry_basket", "Roupa lavada e dobrada!"),
            new Tracked("Garage", "BoxPile", "garage_boxes", "Caixas no lugar. Que Tetris!"),
            new Tracked("Bedroom", "HamsterCage", "bedroom_hamster", "A casa do hamster tá pronta!"),
            new Tracked("Bedroom", "Bed_FirstNight", "bedroom_first_night", ""),
        };

        [MenuItem("MovingIn/Setup Story")]
        static void Setup()
        {
            GameObject player = GameObject.Find("Player");
            GameObject house = GameObject.Find("House");
            GameObject hud = GameObject.Find("Canvas_HUD");
            if (player == null || house == null)
            {
                Debug.LogError("[StorySetup] Player ou House nao encontrados. Abra a cena Hub e rode antes o Build House.");
                return;
            }
            if (hud == null || Object.FindObjectOfType<DialogueManager>(true) == null)
            {
                Debug.LogError("[StorySetup] Rode antes o MovingIn > Setup Dialogue (Etapa 3).");
                return;
            }

            Sprite bubble = AssetDatabase.LoadAssetAtPath<Sprite>(BubblePath);
            Sprite phone = ImportUiSprite(PhonePath, Vector4.zero);
            Sprite key = ImportUiSprite(KeyPath, new Vector4(4, 4, 4, 4));
            AudioClip ringClip = AssetDatabase.LoadAssetAtPath<AudioClip>(RingPath);
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
            if (phone == null) Debug.LogWarning($"[StorySetup] {PhonePath} nao encontrado. O cartao do celular fica sem icone.");
            if (ringClip == null) Debug.LogWarning($"[StorySetup] {RingPath} nao encontrado. O celular vai tocar em silencio.");

            List<Light> roomLights = BuildRoomLights(house.transform);
            List<GameObject> blackoutOnly = new List<GameObject>();
            blackoutOnly.AddRange(BuildEmergencyLights(house.transform));
            blackoutOnly.AddRange(BuildBlockers(house.transform));
            GameObject flashlight = BuildFlashlight(player);

            List<string> missingNames = new List<string>();
            List<(Tracked info, MinigameInteractable mi, MinigameGate gate)> tracked = BuildGates(house.transform, missingNames);
            GameObject firstNightBed = null;
            foreach (var t in tracked)
                if (t.info.id == "bedroom_first_night") firstNightBed = t.mi.gameObject;
            bool bedOk = BuildSleepSpot(house.transform);
            if (!bedOk) missingNames.Add("Bedroom/Bed");

            Transform phoneHolder = BuildPhoneCard(hud.transform, bubble, phone, font, out PhoneCardUI card, out PhoneSpeaker mom, out PhoneSpeaker dad);
            CanvasGroup fade = BuildFade();
            if (key != null) BuildTutorial(hud.transform, bubble, key, font, player);
            else Debug.LogWarning($"[StorySetup] {KeyPath} nao encontrado. As dicas de controle nao foram criadas.");

            GameObject systems = FindRoot("Systems");
            if (systems == null)
            {
                systems = new GameObject("Systems");
                Undo.RegisterCreatedObjectUndo(systems, "Setup Story");
            }
            RemoveChildrenNamed("Story", "TimeOfDay");

            GameObject todGo = NewChild("TimeOfDay", systems.transform);
            TimeOfDay tod = todGo.AddComponent<TimeOfDay>();
            SerializedObject tso = new SerializedObject(tod);
            tso.FindProperty("sun").objectReferenceValue = FindSun();
            tso.FindProperty("cam").objectReferenceValue = Camera.main;
            SetArray(tso.FindProperty("roomLights"), roomLights.ConvertAll(l => (Object)l));
            Transform body = player.transform.Find("Visual/Body");
            List<Object> tinted = new List<Object>();
            if (body != null && body.GetComponent<SpriteRenderer>() != null) tinted.Add(body.GetComponent<SpriteRenderer>());
            SetArray(tso.FindProperty("tinted"), tinted);
            WritePresets(tso.FindProperty("presets"));
            tso.ApplyModifiedPropertiesWithoutUndo();

            GameObject storyGo = NewChild("Story", systems.transform);
            AudioSource ring = storyGo.AddComponent<AudioSource>();
            ring.clip = ringClip;
            ring.loop = true;
            ring.playOnAwake = false;
            ring.spatialBlend = 0f;
            StoryManager sm = storyGo.AddComponent<StoryManager>();
            SerializedObject sso = new SerializedObject(sm);
            sso.FindProperty("timeOfDay").objectReferenceValue = tod;
            sso.FindProperty("firstNightBed").objectReferenceValue = firstNightBed;
            sso.FindProperty("phoneCard").objectReferenceValue = card;
            sso.FindProperty("mom").objectReferenceValue = mom;
            sso.FindProperty("dad").objectReferenceValue = dad;
            sso.FindProperty("ring").objectReferenceValue = ring;
            sso.FindProperty("fade").objectReferenceValue = fade;
            sso.FindProperty("flashlight").objectReferenceValue = flashlight;
            SetArray(sso.FindProperty("blackoutOnly"), blackoutOnly.ConvertAll(g => (Object)g));
            SetArray(sso.FindProperty("lockDuringScenes"), FindByTypeName(player, "PlayerMovement", "PlayerInteractor"));
            SerializedProperty list = sso.FindProperty("minigames");
            list.arraySize = tracked.Count;
            for (int i = 0; i < tracked.Count; i++)
            {
                SerializedProperty e = list.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("id").stringValue = tracked[i].info.id;
                e.FindPropertyRelative("minigame").objectReferenceValue = tracked[i].mi;
                e.FindPropertyRelative("gate").objectReferenceValue = tracked[i].gate;
                e.FindPropertyRelative("doneLine").stringValue = tracked[i].info.doneLine;
            }
            sso.ApplyModifiedPropertiesWithoutUndo();

            storyGo.AddComponent<DevPanel>();

            foreach (GameObject go in blackoutOnly)
                if (go != null) go.SetActive(false);
            flashlight.SetActive(false);

            if (missingNames.Count > 0) Debug.LogWarning("[StorySetup] Nao encontrados: " + string.Join(", ", missingNames));
            Selection.activeGameObject = storyGo;
            Debug.Log($"[StorySetup] Pronto. {roomLights.Count} luzes, {tracked.Count} minigames na historia, cama, celular e fade criados.");
        }

        static List<Light> BuildRoomLights(Transform house)
        {
            List<Light> lights = new List<Light>();
            foreach (RoomRect r in Interiors)
            {
                Transform room = house.Find(r.name);
                if (room == null) continue;
                Transform group = ResetGroup(room, "Lights");
                float w = r.x1 - r.x0;
                float d = r.z1 - r.z0;
                Vector3 pos = new Vector3((r.x0 + r.x1) / 2f, r.y + CeilingHeight, (r.z0 + r.z1) / 2f);
                lights.Add(MakeLight(group, "CeilingLight", LightType.Point, pos, Warm, 1.1f, Mathf.Max(w, d) * 0.85f, LightShadows.None));
            }

            Transform front = house.Find("Front");
            if (front != null)
            {
                Transform group = ResetGroup(front, "Lights");
                lights.Add(MakeLight(group, "PorchLight", LightType.Point, new Vector3(10f, 2.5f, -0.7f), Warm, 1.3f, 5f, LightShadows.None));
            }
            return lights;
        }

        static List<GameObject> BuildEmergencyLights(Transform house)
        {
            List<GameObject> result = new List<GameObject>();
            Material red = GetUnlitMaterial("M_Emergency", Alarm);

            Transform hall = house.Find("Hall");
            if (hall != null)
            {
                Transform group = hall.Find("Lights") ?? ResetGroup(hall, "Lights");
                GameObject root = NewChild("EmergencyLight", group);
                root.transform.position = new Vector3(9.8f, 2.4f, 11.3f);
                Light l = MakeLight(root.transform, "Lamp", LightType.Point, root.transform.position, Alarm, 1.8f, 4.5f, LightShadows.None);
                GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
                marker.name = "Marker";
                Object.DestroyImmediate(marker.GetComponent<Collider>());
                marker.transform.SetParent(root.transform, false);
                marker.transform.position = new Vector3(9.8f, 2.5f, 11.85f);
                marker.transform.localScale = new Vector3(0.5f, 0.2f, 0.08f);
                MeshRenderer mr = marker.GetComponent<MeshRenderer>();
                mr.sharedMaterial = red;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                AddEmergency(root, l, mr);
                result.Add(root);
            }

            Transform living = house.Find("LivingRoom");
            if (living != null)
            {
                Transform group = living.Find("Lights") ?? ResetGroup(living, "Lights");
                GameObject root = NewChild("EmergencyGlow", group);
                root.transform.position = new Vector3(8.5f, 1.8f, 7.3f);
                Light l = MakeLight(root.transform, "Lamp", LightType.Point, root.transform.position, Alarm, 0.9f, 3f, LightShadows.None);
                AddEmergency(root, l, null);
                result.Add(root);
            }
            return result;
        }

        static void AddEmergency(GameObject root, Light lamp, Renderer marker)
        {
            EmergencyLight e = root.AddComponent<EmergencyLight>();
            SerializedObject so = new SerializedObject(e);
            so.FindProperty("lamp").objectReferenceValue = lamp;
            so.FindProperty("marker").objectReferenceValue = marker;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static List<GameObject> BuildBlockers(Transform house)
        {
            List<GameObject> result = new List<GameObject>();
            foreach (string roomName in new[] { "LivingRoom", "Hall" })
            {
                Transform room = house.Find(roomName);
                if (room != null) ResetGroup(room, "Story");
            }
            string dark = "Tá escuro demais. Primeiro a luz.";
            AddBlocker(house, "LivingRoom", "DarkBlocker_Garage", new Vector3(6.4f, 1f, 4f), new Vector3(0.8f, 2f, 1.8f), new Vector3(0.3f, 0f, 0f), new Vector3(1.4f, 2f, 2.4f), dark, result);
            AddBlocker(house, "LivingRoom", "DarkBlocker_Kitchen", new Vector3(13.6f, 1f, 4f), new Vector3(0.8f, 2f, 1.8f), new Vector3(-0.3f, 0f, 0f), new Vector3(1.4f, 2f, 2.4f), dark, result);
            AddBlocker(house, "Hall", "DarkBlocker_Stairs", new Vector3(7.2f, 1f, 11f), new Vector3(2.3f, 2f, 2.3f), Vector3.zero, new Vector3(2.9f, 2f, 2.9f), "Tá escuro demais pra subir. Primeiro a luz.", result);
            return result;
        }

        static void AddBlocker(Transform house, string roomName, string name, Vector3 pos, Vector3 solid, Vector3 talkOffset, Vector3 talk, string line, List<GameObject> result)
        {
            Transform room = house.Find(roomName);
            if (room == null) return;
            Transform group = room.Find("Story") ?? ResetGroup(room, "Story");
            GameObject go = NewChild(name, group);
            go.transform.position = pos;
            BoxCollider wall = go.AddComponent<BoxCollider>();
            wall.size = solid;
            BoxCollider zone = go.AddComponent<BoxCollider>();
            zone.isTrigger = true;
            zone.center = talkOffset;
            zone.size = talk;
            DarkBlocker b = go.AddComponent<DarkBlocker>();
            SerializedObject so = new SerializedObject(b);
            so.FindProperty("_line").stringValue = line;
            so.ApplyModifiedPropertiesWithoutUndo();
            result.Add(go);
        }

        static GameObject BuildFlashlight(GameObject player)
        {
            Transform old = player.transform.Find("BlackoutLights");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            float feet = FeetY(player);
            GameObject root = NewChild("BlackoutLights", player.transform);
            Light spot = MakeLight(root.transform, "Flashlight", LightType.Spot, Vector3.zero, Hex("#FFF1D6"), 2.4f, 8f, LightShadows.Hard);
            spot.transform.localPosition = new Vector3(0f, feet + 1.1f, 0.25f);
            spot.transform.localRotation = Quaternion.Euler(28f, 0f, 0f);
            spot.spotAngle = 60f;
            Light glow = MakeLight(root.transform, "PlayerGlow", LightType.Point, Vector3.zero, Hex("#9DB0FF"), 0.6f, 3.2f, LightShadows.None);
            glow.transform.localPosition = new Vector3(0f, feet + 1.8f, 0f);
            return root;
        }

        static List<(Tracked, MinigameInteractable, MinigameGate)> BuildGates(Transform house, List<string> missing)
        {
            var result = new List<(Tracked, MinigameInteractable, MinigameGate)>();
            Material outline = GetOutlineMaterial();
            if (outline == null) Debug.LogWarning($"[StorySetup] Shader {OutlineShader} nao encontrado. Copie Hub/Shaders/OutlineHull.shader. Os minigames ficam sem contorno.");
            foreach (Tracked t in Minigames)
            {
                Transform room = house.Find(t.room);
                Transform obj = room != null ? FindDeep(room, t.obj) : null;
                MinigameInteractable mi = obj != null ? obj.GetComponent<MinigameInteractable>() : null;
                if (mi == null)
                {
                    missing.Add($"{t.room}/{t.obj}");
                    continue;
                }
                foreach (MinigameGate g in obj.GetComponents<MinigameGate>())
                    Undo.DestroyObjectImmediate(g);
                MinigameGate gate = Undo.AddComponent<MinigameGate>(obj.gameObject);
                SerializedObject so = new SerializedObject(gate);
                so.FindProperty("minigame").objectReferenceValue = mi;
                so.ApplyModifiedPropertiesWithoutUndo();

                foreach (InteractHighlight h in obj.GetComponents<InteractHighlight>())
                    Undo.DestroyObjectImmediate(h);
                if (outline != null)
                {
                    InteractHighlight hl = Undo.AddComponent<InteractHighlight>(obj.gameObject);
                    SerializedObject hso = new SerializedObject(hl);
                    hso.FindProperty("outlineMaterial").objectReferenceValue = outline;
                    hso.ApplyModifiedPropertiesWithoutUndo();
                }
                result.Add((t, mi, gate));
            }
            return result;
        }

        static bool BuildSleepSpot(Transform house)
        {
            Transform bedroom = house.Find("Bedroom");
            Transform bed = bedroom != null ? FindDeep(bedroom, "Bed") : null;
            if (bed == null) return false;
            foreach (SleepSpot s in bed.GetComponents<SleepSpot>())
                Undo.DestroyObjectImmediate(s);
            if (bed.GetComponent<Collider>() == null) Undo.AddComponent<BoxCollider>(bed.gameObject);
            Undo.AddComponent<SleepSpot>(bed.gameObject);
            return true;
        }

        static Transform BuildPhoneCard(Transform hud, Sprite bubble, Sprite phone, TMP_FontAsset font, out PhoneCardUI ui, out PhoneSpeaker mom, out PhoneSpeaker dad)
        {
            Transform old = hud.Find("Phone");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject holder = new GameObject("Phone", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(holder, "Setup Story");
            holder.transform.SetParent(hud, false);
            RectTransform h = (RectTransform)holder.transform;
            h.anchorMin = Vector2.zero;
            h.anchorMax = Vector2.one;
            h.offsetMin = Vector2.zero;
            h.offsetMax = Vector2.zero;

            GameObject cardGo = new GameObject("Card", typeof(RectTransform));
            cardGo.transform.SetParent(holder.transform, false);
            RectTransform card = (RectTransform)cardGo.transform;
            card.anchorMin = card.anchorMax = Vector2.zero;
            card.pivot = Vector2.zero;
            card.anchoredPosition = new Vector2(40f, 40f);
            card.sizeDelta = new Vector2(320f, 104f);
            Image bg = cardGo.AddComponent<Image>();
            bg.sprite = bubble;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f / UiPixel;
            bg.raycastTarget = false;
            CanvasGroup group = cardGo.AddComponent<CanvasGroup>();
            group.blocksRaycasts = false;

            if (phone != null)
            {
                GameObject iconGo = new GameObject("Icon", typeof(RectTransform));
                iconGo.transform.SetParent(card, false);
                RectTransform icon = (RectTransform)iconGo.transform;
                icon.anchorMin = icon.anchorMax = new Vector2(0f, 0.5f);
                icon.pivot = new Vector2(0f, 0.5f);
                icon.anchoredPosition = new Vector2(24f, 0f);
                icon.sizeDelta = new Vector2(phone.rect.width * UiPixel, phone.rect.height * UiPixel);
                Image img = iconGo.AddComponent<Image>();
                img.sprite = phone;
                img.raycastTarget = false;
            }

            MakeText(card, "Title", "Mãe e Pai", font, 28, Ink, new Vector2(92f, 12f));
            MakeText(card, "Subtitle", "em chamada", font, 20, Accent, new Vector2(92f, -20f));

            ui = holder.AddComponent<PhoneCardUI>();
            SerializedObject uso = new SerializedObject(ui);
            uso.FindProperty("card").objectReferenceValue = card;
            uso.FindProperty("group").objectReferenceValue = group;
            uso.ApplyModifiedPropertiesWithoutUndo();

            mom = AddPhoneSpeaker(holder, card, "Mãe", 1.05f);
            dad = AddPhoneSpeaker(holder, card, "Pai", 0.8f);
            cardGo.SetActive(false);
            return holder.transform;
        }

        static PhoneSpeaker AddPhoneSpeaker(GameObject holder, RectTransform card, string displayName, float pitch)
        {
            PhoneSpeaker s = holder.AddComponent<PhoneSpeaker>();
            SerializedObject so = new SerializedObject(s);
            so.FindProperty("_displayName").stringValue = displayName;
            so.FindProperty("_voicePitch").floatValue = pitch;
            so.FindProperty("anchor").objectReferenceValue = card;
            so.FindProperty("cam").objectReferenceValue = Camera.main;
            so.ApplyModifiedPropertiesWithoutUndo();
            return s;
        }

        static void MakeText(RectTransform parent, string name, string text, TMP_FontAsset font, float size, Color color, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            RectTransform rt = (RectTransform)go.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(210f, 34f);
            TextMeshProUGUI t = go.AddComponent<TextMeshProUGUI>();
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Left;
            t.enableWordWrapping = false;
            t.raycastTarget = false;
        }

        static void BuildTutorial(Transform hud, Sprite bubble, Sprite key, TMP_FontAsset font, GameObject player)
        {
            Transform old = hud.Find("Tutorial");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);

            GameObject root = new GameObject("Tutorial", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(root, "Setup Story");
            root.transform.SetParent(hud, false);
            RectTransform rt = (RectTransform)root.transform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
            rt.pivot = new Vector2(0.5f, 0f);
            rt.anchoredPosition = new Vector2(0f, 48f);
            Image bg = root.AddComponent<Image>();
            bg.sprite = bubble;
            bg.type = Image.Type.Sliced;
            bg.pixelsPerUnitMultiplier = 1f / UiPixel;
            bg.color = new Color(Ink.r, Ink.g, Ink.b, 0.9f);
            bg.raycastTarget = false;
            CanvasGroup group = root.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            HorizontalLayoutGroup row = root.AddComponent<HorizontalLayoutGroup>();
            row.padding = new RectOffset(20, 26, 14, 14);
            row.spacing = 18f;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = true;
            row.childControlHeight = true;
            row.childForceExpandWidth = false;
            row.childForceExpandHeight = false;
            ContentSizeFitter fit = root.AddComponent<ContentSizeFitter>();
            fit.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            GameObject keysGo = new GameObject("Keys", typeof(RectTransform));
            keysGo.transform.SetParent(root.transform, false);
            HorizontalLayoutGroup keysRow = keysGo.AddComponent<HorizontalLayoutGroup>();
            keysRow.spacing = 6f;
            keysRow.childAlignment = TextAnchor.MiddleCenter;
            keysRow.childControlWidth = true;
            keysRow.childControlHeight = true;
            keysRow.childForceExpandWidth = false;
            keysRow.childForceExpandHeight = false;

            GameObject keyGo = new GameObject("KeyTemplate", typeof(RectTransform));
            keyGo.transform.SetParent(keysGo.transform, false);
            Image keyImg = keyGo.AddComponent<Image>();
            keyImg.sprite = key;
            keyImg.type = Image.Type.Sliced;
            keyImg.pixelsPerUnitMultiplier = 1f / UiPixel;
            keyImg.raycastTarget = false;
            LayoutElement keyLayout = keyGo.AddComponent<LayoutElement>();
            keyLayout.preferredWidth = 44f;
            keyLayout.preferredHeight = 48f;
            GameObject letterGo = new GameObject("Letter", typeof(RectTransform));
            letterGo.transform.SetParent(keyGo.transform, false);
            RectTransform lrt = (RectTransform)letterGo.transform;
            lrt.anchorMin = Vector2.zero;
            lrt.anchorMax = Vector2.one;
            lrt.offsetMin = new Vector2(0f, 12f);
            lrt.offsetMax = new Vector2(0f, -2f);
            TextMeshProUGUI letter = letterGo.AddComponent<TextMeshProUGUI>();
            if (font != null) letter.font = font;
            letter.text = "E";
            letter.fontSize = 26;
            letter.color = Ink;
            letter.alignment = TextAlignmentOptions.Center;
            letter.raycastTarget = false;

            GameObject labelGo = new GameObject("Label", typeof(RectTransform));
            labelGo.transform.SetParent(root.transform, false);
            TextMeshProUGUI label = labelGo.AddComponent<TextMeshProUGUI>();
            if (font != null) label.font = font;
            label.text = "andar";
            label.fontSize = 28;
            label.color = Hex("#FBF6EC");
            label.alignment = TextAlignmentOptions.Left;
            label.enableWordWrapping = false;
            label.raycastTarget = false;

            TutorialHints hints = root.AddComponent<TutorialHints>();
            SerializedObject so = new SerializedObject(hints);
            so.FindProperty("group").objectReferenceValue = group;
            so.FindProperty("keysRoot").objectReferenceValue = keysGo.transform;
            so.FindProperty("keyTemplate").objectReferenceValue = keyImg;
            so.FindProperty("label").objectReferenceValue = label;
            so.FindProperty("player").objectReferenceValue = player.transform;
            so.FindProperty("interactor").objectReferenceValue = player.GetComponent<PlayerInteractor>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static CanvasGroup BuildFade()
        {
            GameObject old = GameObject.Find("Canvas_StoryFade");
            if (old != null) Undo.DestroyObjectImmediate(old);
            GameObject go = new GameObject("Canvas_StoryFade", typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Setup Story");
            GameObject hubRoot = GameObject.Find("HubRoot");
            if (hubRoot != null) go.transform.SetParent(hubRoot.transform, false);
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = 40;
            go.AddComponent<CanvasScaler>();
            CanvasGroup group = go.AddComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;

            GameObject imgGo = new GameObject("Black", typeof(RectTransform));
            imgGo.transform.SetParent(go.transform, false);
            RectTransform rt = (RectTransform)imgGo.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            Image img = imgGo.AddComponent<Image>();
            img.color = Color.black;
            img.raycastTarget = false;
            return group;
        }

        static void WritePresets(SerializedProperty prop)
        {
            object[][] data =
            {
                new object[] { "Evening",    "#FFA066", 0.85f, new Vector3(20f, -35f, 0f),  "#6B5C70", "#3A2E48", 1f,   "#FFE2CF" },
                new object[] { "Blackout",   "#5E6FA8", 0.08f, new Vector3(55f, -60f, 0f),  "#0B0D1A", "#05060C", 0f,   "#6C7396" },
                new object[] { "Night",      "#5E6FA8", 0.12f, new Vector3(55f, -60f, 0f),  "#26243A", "#0C0D18", 1f,   "#E6DCD2" },
                new object[] { "Afternoon",  "#FFC77A", 1.15f, new Vector3(24f, -40f, 0f),  "#A0866E", "#E9BE94", 0f,   "#FFF1DD" },
                new object[] { "FirstNight", "#4E5C96", 0.10f, new Vector3(55f, -60f, 0f),  "#1A1C2E", "#080913", 0.8f, "#CFC8D6" },
                new object[] { "Dawn",       "#FFB3B8", 0.75f, new Vector3(12f, 130f, 0f),  "#A58E9E", "#F2C4CC", 0f,   "#FFE6E6" },
            };
            prop.arraySize = data.Length;
            for (int i = 0; i < data.Length; i++)
            {
                SerializedProperty e = prop.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("name").stringValue = (string)data[i][0];
                e.FindPropertyRelative("sunColor").colorValue = Hex((string)data[i][1]);
                e.FindPropertyRelative("sunIntensity").floatValue = (float)data[i][2];
                e.FindPropertyRelative("sunRotation").vector3Value = (Vector3)data[i][3];
                e.FindPropertyRelative("ambient").colorValue = Hex((string)data[i][4]);
                e.FindPropertyRelative("background").colorValue = Hex((string)data[i][5]);
                e.FindPropertyRelative("roomLights").floatValue = (float)data[i][6];
                e.FindPropertyRelative("characterTint").colorValue = Hex((string)data[i][7]);
            }
        }

        static Light FindSun()
        {
            foreach (Light l in Object.FindObjectsOfType<Light>(true))
                if (l.type == LightType.Directional) return l;
            Debug.LogWarning("[StorySetup] Nenhuma Directional Light na cena.");
            return null;
        }

        static Light MakeLight(Transform parent, string name, LightType type, Vector3 worldPos, Color color, float intensity, float range, LightShadows shadows)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            Light l = go.AddComponent<Light>();
            l.type = type;
            l.color = color;
            l.intensity = intensity;
            l.range = range;
            l.shadows = shadows;
            return l;
        }

        static Material GetOutlineMaterial()
        {
            Shader shader = Shader.Find(OutlineShader);
            if (shader == null) return null;
            GetUnlitMaterial("M_Emergency", Alarm);
            string path = $"{MaterialFolder}/M_Outline.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(shader);
            m.SetColor("_Color", Hex("#FFC66B"));
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Material GetUnlitMaterial(string name, Color color)
        {
            if (!AssetDatabase.IsValidFolder(MaterialFolder))
            {
                if (!AssetDatabase.IsValidFolder(Root + "Materials")) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Materials");
                AssetDatabase.CreateFolder(Root + "Materials", "Story");
            }
            string path = $"{MaterialFolder}/{name}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m != null) return m;
            m = new Material(Shader.Find("Unlit/Color")) { color = color };
            AssetDatabase.CreateAsset(m, path);
            return m;
        }

        static Sprite ImportUiSprite(string path, Vector4 border)
        {
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null) return null;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 100f;
            ti.filterMode = FilterMode.Point;
            ti.textureCompression = TextureImporterCompression.Uncompressed;
            ti.mipmapEnabled = false;
            ti.alphaIsTransparency = true;
            TextureImporterSettings settings = new TextureImporterSettings();
            ti.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteBorder = border;
            ti.SetTextureSettings(settings);
            ti.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        static Transform ResetGroup(Transform room, string name)
        {
            Transform old = room.Find(name);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            return NewChild(name, room).transform;
        }

        static GameObject NewChild(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(go, "Setup Story");
            go.transform.SetParent(parent, false);
            return go;
        }

        static GameObject FindRoot(string name)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                if (root.name == name) return root;
            return null;
        }

        static void RemoveChildrenNamed(params string[] names)
        {
            HashSet<string> set = new HashSet<string>(names);
            List<GameObject> toRemove = new List<GameObject>();
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (set.Contains(t.name) && t.parent != null && t.parent.name == "Systems")
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

        static float FeetY(GameObject player)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) return cc.center.y - cc.height / 2f;
            return 0f;
        }

        static List<Object> FindByTypeName(GameObject player, params string[] names)
        {
            List<Object> found = new List<Object>();
            HashSet<string> set = new HashSet<string>(names);
            foreach (MonoBehaviour mb in player.GetComponents<MonoBehaviour>())
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
