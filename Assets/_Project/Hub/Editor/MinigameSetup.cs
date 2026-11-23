using System;
using System.Collections.Generic;
using System.IO;
using MovingIn.Core;
using MovingIn.Minigames.Kitchen;
using MovingIn.Minigames.Laundry;
using MovingIn.Minigames.Tetris;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace MovingIn.Hub.EditorTools{
    public static class MinigameSetup
    {
        const string Root = "Assets/_Project/";
        const string HubScene = Root + "Hub/Scenes/Hub.unity";
        const string KitchenScene = Root + "Minigames/Kitchen/Scenes/Kitchen_Minigame.unity";
        const string LaundryScene = Root + "Minigames/Laundry/Scenes/Laundry_Minigame.unity";
        const string TowerScene = Root + "Minigames/TowerDefense/Scenes/TowerDefense.unity";
        const string CanRatFolder = Root + "Minigames/CanRat/Scenes/";
        const string TetrisFolder = Root + "Minigames/Tetris/";
        const string TetrisScene = TetrisFolder + "Scenes/Minigame_Tetris.unity";
        const string TetrisPrefab = TetrisFolder + "Prefabs/Block.prefab";

        static readonly string[] CanRatLevels = { "HAMSTER_TUTORIAL", "HAMSTER_LEVEL1", "HAMSTER_LEVEL2" };
        static readonly string[] BoxNames = { "Box_I", "Box_O", "Box_T", "Box_S", "Box_Z", "Box_J", "Box_L" };

        static readonly Dictionary<string, string> HubScenes = new Dictionary<string, string>
        {
            { "basement_breaker", "TowerDefense" },
            { "kitchen_box", "Kitchen_Minigame" },
            { "laundry_basket", "Laundry_Minigame" },
            { "garage_boxes", "Minigame_Tetris" },
            { "bedroom_hamster", "HAMSTER_TUTORIAL" },
        };

        const string KeySpritePath = Root + "Hub/Art/UI/Key.png";

        const string KitchenTitle = "Cozinha em Ordem";
        const string KitchenHowTo =
            "Arraste com o mouse cada item da caixa para o lugar certo.\n" +
            "O bilhete mostra o que ainda falta guardar.\n\n" +
            "Pratos: um prato só pode ficar em cima de um prato MAIOR.\n" +
            "Use a bancada como apoio para trocar a ordem.\n\n" +
            "Acerto: +10    Erro: -5\n" +
            "{P} pausar";

        const string LaundryTitle = "Lavanderia em Dia";
        const string LaundryHowTo =
            "Arraste as roupas com o mouse.\n\n" +
            "1. Coloque cada roupa do cesto na máquina da cor certa:\n" +
            "    brancas, escuras ou coloridas.\n" +
            "2. Depois da lavagem, guarde cada roupa do varal\n" +
            "    na gaveta do tipo certo: camisetas, calças ou meias.\n\n" +
            "Acerto: +10    Erro: -5";

        const string TetrisTitle = "Caixas na Garagem";
        const string TetrisHowTo =
            "Empilhe as caixas da garagem sem deixar buracos.\n" +
            "Uma linha completa some e vale pontos.\n\n" +
            "{A} {D}  mover            {W}  girar\n" +
            "{S}  descer mais rápido\n" +
            "{Espaço}  soltar de vez\n" +
            "As setas do teclado também funcionam.";

        const string TowerTitle = "Quadro de Luz";
        const string TowerHowTo =
            "Faíscas, surtos e cargas correm pelos fios até o quadro de luz. Não deixe chegarem!\n\n" +
            "{1} {2} {3} {4}  escolher um aliado (ou clique na carta)\n" +
            "Clique num espaço vazio para colocar. Botão direito cancela.\n\n" +
            "Fusíveis atacam quem passa por perto. Baterias geram energia.\n" +
            "Aguente todas as ondas para a luz voltar.";

        const string CanRatTitle = "Gaiola do Hamster";
        const string CanRatHowTo =
            "Leve o hamster até a gaiola de chegada montando um caminho de canos.\n" +
            "Os canos formam uma FILA: o primeiro que entra é o primeiro que sai.\n\n" +
            "{W} {A} {S} {D}  escolher um cano        {Espaço}  pegar\n" +
            "{E}  colocar no fim da fila        {D}  tirar o primeiro\n" +
            "{1} {2}  trocar de fila (quando houver duas)\n" +
            "{ESC}  pausar";

        [MenuItem("MovingIn/Final/4. Montar e Ligar Minigames")]
        static void Run()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var log = new List<string>();
            Step(log, "Tetris", BuildTetris);
            Step(log, "Cozinha", SetupKitchen);
            Step(log, "Lavanderia", SetupLaundry);
            Step(log, "Tower Defense", SetupTowerDefense);
            Step(log, "CanRat", SetupCanRat);
            Step(log, "Build Settings", UpdateBuildSettings);
            Step(log, "Hub", SetupHub);

            Debug.Log("[MinigameSetup] Resultado:\n" + string.Join("\n", log));
        }

        [MenuItem("MovingIn/Final/6. Atualizar Telas dos Minigames")]
        static void RefreshScreens()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var log = new List<string>();
            Step(log, "Cozinha", () => Refresh(KitchenScene, KitchenHowTo));
            Step(log, "Lavanderia", () => Refresh(LaundryScene, LaundryHowTo));
            Step(log, "Tower Defense", () => Refresh(TowerScene, TowerHowTo));
            Step(log, "Tetris", () => Refresh(TetrisScene, TetrisHowTo));
            Step(log, "CanRat", () => Refresh(CanRatFolder + CanRatLevels[0] + ".unity", CanRatHowTo));
            Open(HubScene);

            Debug.Log("[MinigameSetup] Telas atualizadas:\n" + string.Join("\n", log));
        }

        static readonly Dictionary<string, string> DisplayNames = new Dictionary<string, string>
        {
            { "basement_breaker", "Fuse Defense" },
            { "kitchen_box", "Kitchen Organizer" },
            { "laundry_basket", "Laundry" },
            { "garage_boxes", "Box Tetris" },
            { "bedroom_hamster", "Hamsters' Pipe" },
        };

        [MenuItem("MovingIn/Final/8. Nomes dos Minigames (Rejogar)")]
        static void ReplayNames()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            Scene scene = Open(HubScene);
            var log = new List<string>();
            foreach (MinigameInteractable mi in FindAllInScene<MinigameInteractable>(scene))
            {
                if (!DisplayNames.TryGetValue(mi.MinigameId, out string display))
                {
                    log.Add($"- {mi.MinigameId}: sem nome definido (usa o Prompt)");
                    continue;
                }
                var so = new SerializedObject(mi);
                SetIf(so, "_displayName", p => p.stringValue = display);
                so.ApplyModifiedPropertiesWithoutUndo();
                log.Add($"- {mi.MinigameId}: {display}");
            }
            Save(scene);
            Debug.Log("[MinigameSetup] Nomes para rejogar:\n" + string.Join("\n", log));
        }

        const string SkipButtonName = "B_Pular";

        static readonly (string screen, string template, float y)[] SkipSpots =
        {
            ("StartScreen", "B_Jogar", -215f),
            ("PauseScreen", "B_Restart", -160f),
            ("DefeatScreen", "B_Restart", -160f),
        };

        [MenuItem("MovingIn/Final/7. Botoes de Pular no Can-Rat")]
        static void SkipButtons()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var log = new List<string>();
            foreach (string level in CanRatLevels)
                Step(log, level, () =>
                {
                    Scene scene = Open(CanRatFolder + level + ".unity");
                    MinigameExit exit = Ensure<MinigameExit>(FindOrCreateRoot(scene, "HubLink"));
                    int added = AddSkipButtons(scene);
                    int wired = 0;
                    foreach (Button b in FindAllInScene<Button>(scene))
                        if (b.name.StartsWith(SkipButtonName))
                        {
                            Rewire(b, ev => UnityEventTools.AddBoolPersistentListener(ev, exit.Leave, true));
                            wired++;
                        }
                    Save(scene);
                    return wired == 0 ? "sem menus do Can-Rat nessa cena" : $"{added} criados, {wired} ligados";
                });
            Open(HubScene);

            Debug.Log("[MinigameSetup] Botoes de pular:\n" + string.Join("\n", log));
        }

        static int AddSkipButtons(Scene scene)
        {
            int added = 0;
            foreach (var spot in SkipSpots)
            {
                Transform screen = null;
                foreach (Transform t in FindAllInScene<Transform>(scene))
                    if (t.name == spot.screen) { screen = t; break; }
                if (screen == null) continue;

                Transform existing = screen.Find(SkipButtonName);
                if (existing != null)
                {
                    PlaceInCorner((RectTransform)existing);
                    EditorUtility.SetDirty(existing.gameObject);
                    continue;
                }

                Transform template = null;
                foreach (Transform child in screen)
                    if (child.name.StartsWith(spot.template) && child.GetComponent<Button>() != null) { template = child; break; }
                if (template == null) continue;

                GameObject copy = Object.Instantiate(template.gameObject, screen);
                copy.name = SkipButtonName;
                PlaceInCorner((RectTransform)copy.transform);
                foreach (TMP_Text t in copy.GetComponentsInChildren<TMP_Text>(true)) t.text = "Pular";
                foreach (Text t in copy.GetComponentsInChildren<Text>(true)) t.text = "Pular";
                EditorUtility.SetDirty(copy);
                added++;
            }
            return added;
        }

        static void PlaceInCorner(RectTransform rt)
        {
            rt.anchorMin = new Vector2(1f, 0f);
            rt.anchorMax = new Vector2(1f, 0f);
            rt.pivot = new Vector2(1f, 0f);
            rt.anchoredPosition = new Vector2(-20f, 20f);
        }

        static string Refresh(string scenePath, string howTo)
        {
            Scene scene = Open(scenePath);
            MinigameScreens screens = FindInScene<MinigameScreens>();
            if (screens == null) throw new Exception("sem MinigameScreens (rode o passo 4 antes)");

            var so = new SerializedObject(screens);
            SetIf(so, "_howToPlay", p => p.stringValue = howTo);
            Sprite key = AssetDatabase.LoadAssetAtPath<Sprite>(KeySpritePath);
            if (key != null) SetIf(so, "_keySprite", p => p.objectReferenceValue = key);
            so.ApplyModifiedPropertiesWithoutUndo();

            Save(scene);
            return key != null ? "texto com teclas + desenho da tecla do hub" : "texto com teclas (Key.png nao encontrado, usando tecla simples)";
        }

        static void Step(List<string> log, string label, Func<string> action)
        {
            try
            {
                log.Add($"- {label}: {action()}");
            }
            catch (Exception e)
            {
                log.Add($"- {label}: ERRO ({e.Message})");
                Debug.LogException(e);
            }
        }

        static string SetupKitchen()
        {
            Scene scene = Open(KitchenScene);
            KitchenUI ui = FindInScene<KitchenUI>();
            if (ui == null) throw new Exception("KitchenUI nao encontrado na cena");

            KitchenExit old = FindInScene<KitchenExit>();
            GameObject host = old != null ? old.gameObject : ui.gameObject;
            if (old != null) old.enabled = false;

            MinigameExit exit = Ensure<MinigameExit>(host);
            SetBool(exit, "_escapeLeaves", true);
            MinigameScreens screens = Ensure<MinigameScreens>(host);
            ConfigureScreens(screens, KitchenTitle, KitchenHowTo, 60, true, true);
            SetObject(ui, "screens", screens);
            int hidden = HideByName(scene, "VictoryPanel");

            Save(scene);
            return $"tela de abertura + meta 60 em '{host.name}'; KitchenExit desligado; {hidden} painel(is) antigo(s) escondido(s)";
        }

        static string SetupLaundry()
        {
            Scene scene = Open(LaundryScene);
            LaundryUI ui = FindInScene<LaundryUI>();
            if (ui == null) throw new Exception("LaundryUI nao encontrado na cena");

            MinigameExit exit = FindInScene<MinigameExit>();
            GameObject host = exit != null ? exit.gameObject : ui.gameObject;
            exit = Ensure<MinigameExit>(host);
            SetBool(exit, "_escapeLeaves", true);
            MinigameScreens screens = Ensure<MinigameScreens>(host);
            ConfigureScreens(screens, LaundryTitle, LaundryHowTo, 180, true, true);
            SetObject(ui, "screens", screens);
            int hidden = HideByName(scene, "Victory Panel");

            Save(scene);
            return $"tela de abertura + meta 180 em '{host.name}'; {hidden} painel(is) antigo(s) escondido(s)";
        }

        static string SetupTowerDefense()
        {
            Scene scene = Open(TowerScene);
            GameObject host = FindOrCreateRoot(scene, "HubScreens");
            MinigameExit exit = Ensure<MinigameExit>(host);
            SetBool(exit, "_escapeLeaves", false);
            MinigameScreens screens = Ensure<MinigameScreens>(host);
            ConfigureScreens(screens, TowerTitle, TowerHowTo, 0, false, true);

            Save(scene);
            return "tela de abertura em 'HubScreens' (saida e resultado continuam com o HUD do proprio minigame)";
        }

        static string SetupCanRat()
        {
            var parts = new List<string>();
            for (int i = 0; i < CanRatLevels.Length; i++)
            {
                Scene scene = Open(CanRatFolder + CanRatLevels[i] + ".unity");
                bool last = i == CanRatLevels.Length - 1;
                string next = last ? null : CanRatLevels[i + 1];

                GameObject host = FindOrCreateRoot(scene, "HubLink");
                MinigameExit exit = Ensure<MinigameExit>(host);
                SetBool(exit, "_escapeLeaves", false);

                if (i == 0)
                {
                    MinigameScreens screens = Ensure<MinigameScreens>(host);
                    ConfigureScreens(screens, CanRatTitle, CanRatHowTo, 0, false, false);
                }

                AddSkipButtons(scene);

                int rewired = 0;
                foreach (Button b in FindAllInScene<Button>(scene))
                {
                    string n = b.name;
                    if (n.StartsWith("B_Proximo"))
                    {
                        if (last) Rewire(b, ev => UnityEventTools.AddBoolPersistentListener(ev, exit.Leave, true));
                        else Rewire(b, ev => UnityEventTools.AddStringPersistentListener(ev, exit.LoadLevel, next));
                    }
                    else if (n.StartsWith("B_Restart"))
                        Rewire(b, ev => UnityEventTools.AddPersistentListener(ev, exit.Retry));
                    else if (n.StartsWith(SkipButtonName))
                        Rewire(b, ev => UnityEventTools.AddBoolPersistentListener(ev, exit.Leave, true));
                    else if (n.StartsWith("B_Sair"))
                    {
                        bool win = last && HasAncestor(b.transform, "Victory");
                        Rewire(b, ev => UnityEventTools.AddBoolPersistentListener(ev, exit.Leave, win));
                    }
                    else continue;
                    rewired++;
                }

                Save(scene);
                parts.Add($"{CanRatLevels[i]} ({rewired} botoes)");
            }
            return string.Join(", ", parts);
        }

        static string BuildTetris()
        {
            if (File.Exists(TetrisScene)) return RepairTetris();

            EnsureFolder(TetrisFolder, "Scenes");
            EnsureFolder(TetrisFolder, "Prefabs");

            Sprite wall = LoadSprite("Wall");
            Sprite pixel = LoadSprite("Pixel");
            Sprite grid = LoadSprite("Grid");
            var boxes = new Sprite[BoxNames.Length];
            for (int i = 0; i < boxes.Length; i++) boxes[i] = LoadSprite(BoxNames[i]);

            GameObject temp = new GameObject("Block");
            SpriteRenderer tempRenderer = temp.AddComponent<SpriteRenderer>();
            tempRenderer.sprite = boxes[2];
            tempRenderer.sortingOrder = 4;
            GameObject blockAsset = PrefabUtility.SaveAsPrefabAsset(temp, TetrisPrefab);
            Object.DestroyImmediate(temp);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camGo.tag = "MainCamera";
            camGo.transform.position = new Vector3(5f, 10f, -10f);
            Camera cam = camGo.GetComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 11.5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("2E2833");

            SpriteRenderer background = Sprite2D("Background", null, wall, new Vector3(5f, 10f), Vector2.one, Color.white, -10);
            background.drawMode = SpriteDrawMode.Tiled;
            background.size = new Vector2(48f, 26f);

            Transform board = Empty("Board", null, Vector3.zero);
            Sprite2D("Frame", board, pixel, new Vector3(5f, 10f), new Vector2(10.6f, 20.6f), Hex("6B5444"), 0);
            Sprite2D("Well", board, pixel, new Vector3(5f, 10f), new Vector2(10f, 20f), Hex("1C1820"), 1);
            SpriteRenderer gridRenderer = Sprite2D("Grid", board, grid, new Vector3(5f, 10f), Vector2.one, Color.white, 2);
            gridRenderer.drawMode = SpriteDrawMode.Tiled;
            gridRenderer.size = new Vector2(10f, 20f);
            Transform cells = Empty("Cells", board, Vector3.zero);

            Transform nextPanel = Empty("NextPanel", null, Vector3.zero);
            Sprite2D("Frame", nextPanel, pixel, new Vector3(14.5f, 11.8f), new Vector2(5.6f, 13.6f), Hex("6B5444"), 0);
            Sprite2D("Back", nextPanel, pixel, new Vector3(14.5f, 11.8f), new Vector2(5f, 13f), Hex("1C1820"), 1);
            Text3D("Label", nextPanel, new Vector3(14.5f, 19.2f), "PRÓXIMAS", 5f, Hex("E0A060"));
            Transform next0 = Empty("Next0", nextPanel, new Vector3(14.5f, 15.5f));
            Transform next1 = Empty("Next1", nextPanel, new Vector3(14.5f, 11.5f));
            Transform next2 = Empty("Next2", nextPanel, new Vector3(14.5f, 7.5f));
            next1.localScale = next2.localScale = Vector3.one * 0.8f;

            Transform hud = Empty("Hud", null, Vector3.zero);
            TextMeshPro score = Text3D("ScoreText", hud, new Vector3(-4.5f, 16.5f), "PONTOS\n0", 5f, Hex("F4EAD8"));
            TextMeshPro level = Text3D("LevelText", hud, new Vector3(-4.5f, 12.5f), "NÍVEL\n1", 5f, Hex("F4EAD8"));
            TextMeshPro lines = Text3D("LinesText", hud, new Vector3(-4.5f, 8.5f), "LINHAS\n0", 5f, Hex("F4EAD8"));
            Transform goalBar = Empty("GoalBar", hud, Vector3.zero);
            Sprite2D("Frame", goalBar, pixel, new Vector3(-1.3f, 10f), new Vector2(0.7f, 16.3f), Hex("6B5444"), 0);
            Sprite2D("Back", goalBar, pixel, new Vector3(-1.3f, 10f), new Vector2(0.4f, 16f), Hex("1C1820"), 1);
            Transform goalFill = Empty("GoalFill", goalBar, new Vector3(-1.3f, 2f));
            goalFill.localScale = new Vector3(0.4f, 16f, 1f);
            Sprite2D("Fill", goalFill, pixel, new Vector3(0f, 0.5f), Vector2.one, Hex("E0A060"), 2);
            Text3D("Label", goalBar, new Vector3(-1.3f, 18.9f), "META", 3.5f, Hex("E0A060"));

            GameObject systems = new GameObject("Systems");
            MinigameScreens screens = systems.AddComponent<MinigameScreens>();
            ConfigureScreens(screens, TetrisTitle, TetrisHowTo, 800, true, true);
            TetrisGame game = systems.AddComponent<TetrisGame>();
            SetObject(game, "screens", screens);

            TetrisView view = systems.AddComponent<TetrisView>();
            var so = new SerializedObject(view);
            so.FindProperty("game").objectReferenceValue = game;
            so.FindProperty("blockPrefab").objectReferenceValue = blockAsset.GetComponent<SpriteRenderer>();
            SetArray(so.FindProperty("boxSprites"), boxes);
            so.FindProperty("cells").objectReferenceValue = cells;
            SetArray(so.FindProperty("nextSlots"), new Object[] { next0, next1, next2 });
            so.FindProperty("scoreText").objectReferenceValue = score;
            so.FindProperty("levelText").objectReferenceValue = level;
            so.FindProperty("linesText").objectReferenceValue = lines;
            so.FindProperty("goalFill").objectReferenceValue = goalFill;
            so.ApplyModifiedPropertiesWithoutUndo();

            EditorSceneManager.SaveScene(scene, TetrisScene);
            return "cena Minigame_Tetris e prefab Block criados";
        }

        static string UpdateBuildSettings()
        {
            var wanted = new List<string> { HubScene, KitchenScene, LaundryScene, TowerScene, TetrisScene };
            foreach (string level in CanRatLevels) wanted.Add(CanRatFolder + level + ".unity");

            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            var missing = new List<string>();
            foreach (string path in wanted)
            {
                if (!File.Exists(path))
                {
                    missing.Add(Path.GetFileNameWithoutExtension(path));
                    continue;
                }
                EditorBuildSettingsScene found = scenes.Find(s => s.path == path);
                if (found == null) scenes.Add(new EditorBuildSettingsScene(path, true));
                else found.enabled = true;
            }
            EditorBuildSettings.scenes = scenes.ToArray();
            return missing.Count == 0 ? "todas as cenas na lista" : "faltando: " + string.Join(", ", missing);
        }

        static string SetupHub()
        {
            Scene scene = Open(HubScene);
            var changed = new List<string>();
            foreach (MinigameInteractable mi in Object.FindObjectsOfType<MinigameInteractable>(true))
            {
                if (!HubScenes.TryGetValue(mi.MinigameId, out string sceneName)) continue;
                var so = new SerializedObject(mi);
                SerializedProperty p = so.FindProperty("_sceneName");
                if (p.stringValue == sceneName) continue;
                p.stringValue = sceneName;
                so.ApplyModifiedPropertiesWithoutUndo();
                changed.Add($"{mi.MinigameId} -> {sceneName}");
            }
            Save(scene);
            return changed.Count == 0 ? "nomes de cena ja estavam certos" : string.Join(", ", changed);
        }

        static void ConfigureScreens(MinigameScreens screens, string title, string howTo, int goal, bool showGoal, bool unpause)
        {
            var so = new SerializedObject(screens);
            SetIf(so, "_title", p => p.stringValue = title);
            SetIf(so, "_howToPlay", p => p.stringValue = howTo);
            SetIf(so, "_goal", p => p.intValue = goal);
            SetIf(so, "_showGoal", p => p.boolValue = showGoal);
            SetIf(so, "_unpauseOnPlay", p => p.boolValue = unpause);
            TMP_FontAsset font = ProjectFont();
            if (font != null) SetIf(so, "font", p => p.objectReferenceValue = font);
            Sprite key = AssetDatabase.LoadAssetAtPath<Sprite>(KeySpritePath);
            if (key != null) SetIf(so, "_keySprite", p => p.objectReferenceValue = key);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetIf(SerializedObject so, string name, Action<SerializedProperty> set)
        {
            SerializedProperty p = so.FindProperty(name);
            if (p != null) set(p);
            else Debug.LogWarning($"[MinigameSetup] Campo '{name}' nao existe no {so.targetObject.GetType().Name}. Confira se o script esta atualizado.");
        }

        static void Rewire(Button button, Action<UnityEngine.Events.UnityEvent> add)
        {
            Button.ButtonClickedEvent ev = button.onClick;
            for (int i = ev.GetPersistentEventCount() - 1; i >= 0; i--)
                UnityEventTools.RemovePersistentListener(ev, i);
            add(ev);
            EditorUtility.SetDirty(button);
            PrefabUtility.RecordPrefabInstancePropertyModifications(button);
        }

        static bool HasAncestor(Transform t, string part)
        {
            for (Transform p = t.parent; p != null; p = p.parent)
                if (p.name.Contains(part)) return true;
            return false;
        }

        static Scene Open(string path)
        {
            if (!File.Exists(path)) throw new FileNotFoundException("cena nao encontrada: " + path);
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            RemoveMissing(scene);
            return scene;
        }

        static int RemoveMissing(Scene scene)
        {
            int removed = 0;
            foreach (Transform t in FindAllInScene<Transform>(scene))
            {
                GameObject go = t.gameObject;
                if (PrefabUtility.IsPartOfPrefabInstance(go)) continue;
                int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
                if (count == 0) continue;
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                removed += count;
            }
            if (removed > 0) Debug.Log($"[MinigameSetup] {scene.name}: {removed} componente(s) sem script removido(s).");
            return removed;
        }

        static string RepairTetris()
        {
            Scene scene = Open(TetrisScene);
            TetrisGame game = FindInScene<TetrisGame>();
            if (game == null) throw new Exception("TetrisGame nao encontrado na cena");

            MinigameScreens screens = game.GetComponent<MinigameScreens>();
            bool fresh = screens == null;
            if (fresh)
            {
                screens = game.gameObject.AddComponent<MinigameScreens>();
                ConfigureScreens(screens, TetrisTitle, TetrisHowTo, 800, true, true);
            }
            SetBool(Ensure<MinigameExit>(game.gameObject), "_escapeLeaves", true);
            SetObject(game, "screens", screens);

            Save(scene);
            return fresh ? "cena ja existia: tela de abertura recolocada (meta 800)" : "cena ja existia e estava ligada";
        }

        static void Save(Scene scene)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static T FindInScene<T>() where T : Component
        {
            T[] all = Object.FindObjectsOfType<T>(true);
            return all.Length > 0 ? all[0] : null;
        }

        static List<T> FindAllInScene<T>(Scene scene) where T : Component
        {
            var list = new List<T>();
            foreach (GameObject root in scene.GetRootGameObjects())
                list.AddRange(root.GetComponentsInChildren<T>(true));
            return list;
        }

        static T Ensure<T>(GameObject go) where T : Component
        {
            T c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static GameObject FindOrCreateRoot(Scene scene, string name)
        {
            foreach (GameObject root in scene.GetRootGameObjects())
                if (root.name == name) return root;
            return new GameObject(name);
        }

        static int HideByName(Scene scene, string name)
        {
            int count = 0;
            foreach (Transform t in FindAllInScene<Transform>(scene))
            {
                if (t.name != name || !t.gameObject.activeSelf) continue;
                t.gameObject.SetActive(false);
                count++;
            }
            return count;
        }

        static void SetBool(Object target, string property, bool value)
        {
            var so = new SerializedObject(target);
            SetIf(so, property, p => p.boolValue = value);
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetObject(Object target, string property, Object value)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = so.FindProperty(property);
            if (p == null) throw new Exception($"campo '{property}' nao existe em {target.GetType().Name}");
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(SerializedProperty array, Object[] values)
        {
            array.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                array.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }

        static TMP_FontAsset ProjectFont()
        {
            foreach (string guid in AssetDatabase.FindAssets("Pixelify t:TMP_FontAsset"))
            {
                TMP_FontAsset f = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(AssetDatabase.GUIDToAssetPath(guid));
                if (f != null) return f;
            }
            return null;
        }

        static Sprite LoadSprite(string name)
        {
            string path = TetrisFolder + "Sprites/" + name + ".png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) throw new Exception($"sprite nao encontrado (confira se o PNG esta como Sprite): {path}");
            return s;
        }

        static void EnsureFolder(string parent, string child)
        {
            string trimmed = parent.TrimEnd('/');
            if (!AssetDatabase.IsValidFolder(trimmed + "/" + child)) AssetDatabase.CreateFolder(trimmed, child);
        }

        static Transform Empty(string name, Transform parent, Vector3 position)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            return go.transform;
        }

        static SpriteRenderer Sprite2D(string name, Transform parent, Sprite sprite, Vector3 position, Vector2 scale, Color color, int order)
        {
            Transform t = Empty(name, parent, position);
            t.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer r = t.gameObject.AddComponent<SpriteRenderer>();
            r.sprite = sprite;
            r.color = color;
            r.sortingOrder = order;
            return r;
        }

        static TextMeshPro Text3D(string name, Transform parent, Vector3 position, string text, float size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            TextMeshPro t = go.AddComponent<TextMeshPro>();
            TMP_FontAsset font = ProjectFont();
            if (font != null) t.font = font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAlignmentOptions.Center;
            t.enableWordWrapping = false;
            t.rectTransform.sizeDelta = new Vector2(7f, 3f);
            t.sortingOrder = 10;
            return t;
        }

        static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out Color c);
            return c;
        }
    }
}
