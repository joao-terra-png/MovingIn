using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static class HouseBlockoutBuilder
    {
        const float WallHeight = 3f;
        const float WallThickness = 0.2f;
        const float DoorWidth = 1.6f;
        const float CameraSize = 4.5f;
        const float FollowMargin = 2.5f;
        const string MaterialFolder = "Assets/_Project/Hub/Materials/Blockout";
        const string RootName = "House";

        enum Group { Fixos, Bagunca, Arrumado }

        class RoomDef
        {
            public string name; public float y, x0, z0, x1, z1; public string floor, wall; public bool exterior;
            public RoomDef(string n, float y, float x0, float z0, float x1, float z1, string floor, string wall, bool exterior = false)
            { name = n; this.y = y; this.x0 = x0; this.z0 = z0; this.x1 = x1; this.z1 = z1; this.floor = floor; this.wall = wall; this.exterior = exterior; }
        }

        class EdgeDoor
        {
            public string a, b; public float x, z;
            public EdgeDoor(string a, string b, float x, float z) { this.a = a; this.b = b; this.x = x; this.z = z; }
        }

        class Portal
        {
            public string from, to; public Vector2 trigger, spawn, size;
            public Portal(string f, string t, float tx, float tz, float sx, float sz, float w = 2f, float d = 2f)
            { from = f; to = t; trigger = new Vector2(tx, tz); spawn = new Vector2(sx, sz); size = new Vector2(w, d); }
        }

        class Prop
        {
            public string room, name, color; public Vector3 pos, size; public Group group; public bool collider;
            public Prop(string r, string n, float x, float y, float z, float sx, float sy, float sz, string c, Group g = Group.Fixos, bool col = true)
            { room = r; name = n; pos = new Vector3(x, y, z); size = new Vector3(sx, sy, sz); color = c; group = g; collider = col; }
        }

        class MinigameDef
        {
            public string room, name, id, scene, prompt, color; public Vector3 pos, size; public bool active, hideSelf, roomState;
            public MinigameDef(string r, string n, string id, string scene, string prompt, float x, float y, float z, float sx, float sy, float sz, string c,
                               bool hideSelf = true, bool roomState = true, bool active = true)
            { room = r; name = n; this.id = id; this.scene = scene; this.prompt = prompt; pos = new Vector3(x, y, z); size = new Vector3(sx, sy, sz); color = c;
              this.hideSelf = hideSelf; this.roomState = roomState; this.active = active; }
        }

        const string Wall = "#E2E2E2";
        const string Solid = "#BDBDBD";
        const string Dark = "#8F8F8F";
        const string Glass = "#C9D5DD";
        const string Mess = "#CDB79C";
        const string Tidy = "#A3B8C9";
        const string Marker = "#E8843F";

        static readonly RoomDef[] Rooms =
        {
            new RoomDef("Front",         0,  -4, -10, 25,  0, "#B7C4AC", Wall, true),
            new RoomDef("Garage",        0,   0,   0,  6,  8, "#A9A9A9", Wall),
            new RoomDef("LivingRoom",    0,   6,   0, 14,  8, "#B3B3B3", Wall),
            new RoomDef("Kitchen",       0,  14,   0, 21,  8, "#ADADAD", Wall),
            new RoomDef("Hall",          0,   6,   8, 11, 12, "#B3B3B3", Wall),
            new RoomDef("Laundry",       0,  14,   8, 21, 12, "#ABABAB", Wall),
            new RoomDef("Basement",    -20,   4,   0, 17,  8, "#9C9C9C", "#C4C4C4"),
            new RoomDef("Bedroom",      20,   0,   0, 10,  8, "#B0B0B0", Wall),
            new RoomDef("UpstairsHall", 20,  10,   0, 14,  8, "#B3B3B3", Wall),
            new RoomDef("Attic",        40,   2,   0, 14,  8, "#A5A5A5", "#CFCFCF")
        };

        static readonly EdgeDoor[] EdgeDoors =
        {
            new EdgeDoor("LivingRoom", "Garage", 6, 4),
            new EdgeDoor("LivingRoom", "Kitchen", 14, 4),
            new EdgeDoor("LivingRoom", "Hall", 8.5f, 8),
            new EdgeDoor("Kitchen", "Laundry", 15.2f, 8),
            new EdgeDoor("UpstairsHall", "Bedroom", 10, 4)
        };

        static readonly Portal[] Portals =
        {
            new Portal("Front", "LivingRoom", 10, -0.6f, 10, 1.8f, 1.6f, 0.6f),
            new Portal("LivingRoom", "Front", 10, 0.3f, 10, -2.4f, 1.6f, 0.6f),
            new Portal("Hall", "UpstairsHall", 7.2f, 11, 12, 5.4f),
            new Portal("UpstairsHall", "Hall", 12, 7.2f, 8.2f, 9.6f),
            new Portal("Hall", "Basement", 9.8f, 11, 15.6f, 5.6f),
            new Portal("Basement", "Hall", 16, 7.2f, 9.2f, 9.6f),
            new Portal("UpstairsHall", "Attic", 13, 1.2f, 8, 2.6f),
            new Portal("Attic", "UpstairsHall", 8, 0.8f, 12, 2.8f)
        };

        static readonly Prop[] Props =
        {
            new Prop("Front", "Grass", 10.5f, -0.05f, -5, 40, 0.1f, 10, "#B7C4AC"),
            new Prop("Front", "Ground_Surroundings", 10, -0.2f, 5, 100, 0.1f, 90, "#B7C4AC", Group.Fixos, false),
            new Prop("Front", "Sidewalk", 10.5f, -0.02f, -10.75f, 40, 0.12f, 1.5f, "#CFCFCF"),
            new Prop("Front", "Street", 10.5f, -0.08f, -14, 40, 0.1f, 5, "#8A8A8A"),
            new Prop("Front", "Path", 10, 0.01f, -5, 1.6f, 0.04f, 10, "#C7C2B8", Group.Fixos, false),
            new Prop("Front", "House_GroundFloor", 10.5f, 1.5f, 4, 21, 3, 8, Wall),
            new Prop("Front", "House_Back", 13.5f, 1.5f, 10, 15, 3, 4, Wall),
            new Prop("Front", "House_Upper", 7, 4.5f, 6, 14, 3, 8, Wall),
            new Prop("Front", "Roof_Base", 7, 6.2f, 6, 14.8f, 0.5f, 8.8f, Dark),
            new Prop("Front", "Roof_Top", 7, 6.9f, 6, 12, 0.9f, 6, Dark),
            new Prop("Front", "FrontDoor", 10, 1.1f, -0.06f, 1.2f, 2.2f, 0.12f, Marker, Group.Fixos, false),
            new Prop("Front", "GarageDoor", 3, 1.2f, -0.06f, 4, 2.4f, 0.12f, Dark, Group.Fixos, false),
            new Prop("Front", "Window_1", 6, 1.7f, -0.05f, 1.4f, 1, 0.1f, Glass, Group.Fixos, false),
            new Prop("Front", "Window_2", 16, 1.7f, -0.05f, 1.4f, 1, 0.1f, Glass, Group.Fixos, false),
            new Prop("Front", "Window_3", 19, 1.7f, -0.05f, 1.4f, 1, 0.1f, Glass, Group.Fixos, false),
            new Prop("Front", "Mailbox", 8.2f, 0.6f, -9.4f, 0.3f, 1.2f, 0.3f, Solid),
            new Prop("Front", "Tree_1", -2, 1.5f, -6, 1.6f, 3, 1.6f, "#A7B59E"),
            new Prop("Front", "Tree_2", 23, 1.3f, -3, 1.4f, 2.6f, 1.4f, "#A7B59E"),
            new Prop("Front", "Fence_Left", 2.5f, 0.4f, -9.8f, 13, 0.8f, 0.12f, Solid),
            new Prop("Front", "Fence_Right", 18, 0.4f, -9.8f, 14, 0.8f, 0.12f, Solid),
            new Prop("Front", "Truck_Box", 14, 1.7f, -13, 5, 2.6f, 2.2f, "#D6D6D6"),
            new Prop("Front", "Truck_Cab", 17.3f, 1.2f, -13, 1.6f, 1.8f, 2.1f, Solid),
            new Prop("Front", "StreetBox_1", 12, 0.3f, -10.9f, 0.6f, 0.6f, 0.6f, Mess),
            new Prop("Front", "StreetBox_2", 12.8f, 0.25f, -10.6f, 0.5f, 0.5f, 0.5f, Mess),

            new Prop("Garage", "MetalShelves", 3, 1, 7.6f, 5, 2, 0.6f, Solid),
            new Prop("Garage", "Bicycle", 0.6f, 0.5f, 5, 0.4f, 1, 1.6f, Solid),
            new Prop("Garage", "ScatteredBoxes_1", 2, 0.3f, 2.5f, 1, 0.6f, 1, Mess, Group.Bagunca, false),
            new Prop("Garage", "ScatteredBoxes_2", 4.2f, 0.25f, 1.5f, 0.8f, 0.5f, 0.8f, Mess, Group.Bagunca, false),
            new Prop("Garage", "ShelvedBoxes", 3, 1.6f, 7.6f, 4.5f, 0.8f, 0.5f, Tidy, Group.Arrumado, false),

            new Prop("LivingRoom", "Sofa", 10.3f, 0.4f, 6, 1.2f, 0.8f, 3, Solid),
            new Prop("LivingRoom", "TV", 13.6f, 0.6f, 6.4f, 0.4f, 1.2f, 2, Dark),
            new Prop("LivingRoom", "Bookcase", 12.3f, 1, 7.6f, 2, 2, 0.6f, Solid),
            new Prop("LivingRoom", "Rug", 11.9f, 0.02f, 6, 2.2f, 0.04f, 2.6f, "#C4C4C4", Group.Fixos, false),
            new Prop("LivingRoom", "ExitMat", 10, 0.02f, 0.7f, 1.4f, 0.04f, 0.8f, Marker, Group.Fixos, false),
            new Prop("LivingRoom", "MovingBoxes", 7, 0.4f, 1.4f, 1.2f, 0.8f, 1.2f, Mess),

            new Prop("Kitchen", "SinkCounter", 18.2f, 0.45f, 7.6f, 2.6f, 0.9f, 0.8f, Solid),
            new Prop("Kitchen", "Stove", 20.2f, 0.45f, 7.6f, 1, 0.9f, 0.8f, Dark),
            new Prop("Kitchen", "Fridge", 20.5f, 1, 5.6f, 1, 2, 1, Solid),
            new Prop("Kitchen", "FloorPlates", 16, 0.05f, 2.5f, 0.6f, 0.1f, 0.6f, Mess, Group.Bagunca, false),
            new Prop("Kitchen", "FloorPan", 19.5f, 0.15f, 2.5f, 0.5f, 0.3f, 0.5f, Mess, Group.Bagunca, false),
            new Prop("Kitchen", "WallCabinet", 20.75f, 2.3f, 3.5f, 0.4f, 0.8f, 2.5f, Tidy, Group.Arrumado, false),
            new Prop("Kitchen", "Table", 16.5f, 0.4f, 4.5f, 1.6f, 0.8f, 1, Tidy, Group.Arrumado, false),

            new Prop("Laundry", "WashingMachine", 20.4f, 0.5f, 11.4f, 1, 1, 1, Solid),
            new Prop("Laundry", "UtilitySink", 18.8f, 0.45f, 11.4f, 1.2f, 0.9f, 1, Solid),
            new Prop("Laundry", "FloorClothes", 19.5f, 0.05f, 9.3f, 0.8f, 0.1f, 0.6f, Mess, Group.Bagunca, false),
            new Prop("Laundry", "FullClothesline", 16.5f, 2, 11.8f, 3, 0.6f, 0.1f, Tidy, Group.Arrumado, false),

            new Prop("Hall", "StairsUp", 7.2f, 0.6f, 11, 1.4f, 1.2f, 1.6f, Marker, Group.Fixos, false),
            new Prop("Hall", "StairsDown", 9.8f, 0.02f, 11, 1.4f, 0.04f, 1.6f, Marker, Group.Fixos, false),
            new Prop("Hall", "CoatRack", 10.7f, 1, 8.7f, 0.3f, 2, 0.3f, Solid),
            new Prop("Hall", "StuckDoor_Restroom", 10.95f, 1, 9.6f, 0.1f, 2, 0.9f, Dark, Group.Fixos, false),

            new Prop("Basement", "StairsUp", 16, 0.6f, 7.2f, 1.4f, 1.2f, 1.4f, Marker, Group.Fixos, false),
            new Prop("Basement", "Boiler", 12, 1, 7.3f, 1.6f, 2, 1.2f, Dark),
            new Prop("Basement", "Shelves", 8.5f, 1, 7.6f, 3, 2, 0.6f, Solid),
            new Prop("Basement", "OldBoxes", 16.3f, 0.4f, 3, 1, 0.8f, 1, Solid),
            new Prop("Basement", "LightBulb", 10.5f, 2.7f, 4, 0.3f, 0.3f, 0.3f, "#F2F2F2", Group.Fixos, false),

            new Prop("Bedroom", "Bed", 8.6f, 0.3f, 6.6f, 2.2f, 0.6f, 2.6f, Solid),
            new Prop("Bedroom", "Wardrobe", 9.5f, 1.1f, 1.3f, 0.8f, 2.2f, 2, Solid),
            new Prop("Bedroom", "Desk", 2.5f, 0.4f, 7.5f, 2.2f, 0.8f, 1, Solid),
            new Prop("Bedroom", "Chair", 3.4f, 0.25f, 6.6f, 0.6f, 0.5f, 0.6f, Dark),
            new Prop("Bedroom", "Bookshelf", 5.2f, 1, 7.7f, 1.6f, 2, 0.5f, Solid),
            new Prop("Bedroom", "Window", 6.8f, 1.7f, 7.95f, 1.6f, 1.2f, 0.05f, Glass, Group.Fixos, false),
            new Prop("Bedroom", "LoosePipes", 3.2f, 0.88f, 7.4f, 0.6f, 0.15f, 0.4f, Mess, Group.Bagunca, false),
            new Prop("Bedroom", "AssembledTubes", 3.1f, 1.2f, 7.5f, 1, 0.3f, 0.3f, Tidy, Group.Arrumado, false),

            new Prop("UpstairsHall", "StairsDown", 12, 0.02f, 7.2f, 1.6f, 0.04f, 1.4f, Marker, Group.Fixos, false),
            new Prop("UpstairsHall", "AtticLadder", 13.9f, 1.5f, 1.2f, 0.1f, 3, 0.8f, Marker, Group.Fixos, false),
            new Prop("UpstairsHall", "StuckDoor_Bathroom", 13.95f, 1, 4, 0.1f, 2, 0.9f, Dark, Group.Fixos, false),
            new Prop("UpstairsHall", "Painting", 13.95f, 1.8f, 6, 0.05f, 0.6f, 0.8f, Solid, Group.Fixos, false),

            new Prop("Attic", "Chest", 12.5f, 0.4f, 7, 1.4f, 0.8f, 0.8f, Solid),
            new Prop("Attic", "OldFurniture", 3.5f, 0.6f, 7.2f, 1.6f, 1.2f, 1, Solid),
            new Prop("Attic", "RoundWindow", 8, 2, 7.95f, 1, 1, 0.05f, Glass, Group.Fixos, false),
            new Prop("Attic", "Hatch", 8, 0.02f, 0.8f, 1.2f, 0.04f, 1.2f, Marker, Group.Fixos, false),
            new Prop("Attic", "FloorPhotos", 6, 0.02f, 3, 1, 0.04f, 0.7f, Mess, Group.Bagunca, false),
            new Prop("Attic", "FamilyTreeBoard", 13.9f, 1.6f, 4, 0.1f, 1.6f, 2.4f, Tidy, Group.Arrumado, false)
        };

        static readonly MinigameDef[] Minigames =
        {
            new MinigameDef("Basement", "BreakerPanel", "basement_breaker", "Minigame_TowerDefense", "Mexer no quadro de luz", 5.2f, 1.4f, 7.8f, 1, 1.2f, 0.3f, "#D64545", false),
            new MinigameDef("Kitchen", "KitchenBox", "kitchen_box", "Kitchen_Minigame", "Arrumar a cozinha", 18.6f, 0.3f, 4, 0.9f, 0.6f, 0.9f, "#E8743B"),
            new MinigameDef("Laundry", "LaundryBasket", "laundry_basket", "Laundry_Minigame", "Lavar as roupas", 17, 0.35f, 10, 0.8f, 0.7f, 0.8f, "#3B8BE8"),
            new MinigameDef("Garage", "BoxPile", "garage_boxes", "Minigame_Tetris", "Organizar as caixas", 3, 0.6f, 4, 1.4f, 1.2f, 1.4f, "#8A5CF6"),
            new MinigameDef("Bedroom", "HamsterCage", "bedroom_hamster", "Minigame_CanRat", "Montar a gaiola do hamster", 1.9f, 1.05f, 7.5f, 1, 0.5f, 0.6f, "#2FA36B", false),
            new MinigameDef("Bedroom", "Bed_FirstNight", "bedroom_first_night", "Minigame_PrimeiraNoite", "Dormir (primeira noite)", 8.6f, 0.65f, 6.6f, 1.6f, 0.1f, 1.6f, "#3A3F6B", false, false, false),
            new MinigameDef("Attic", "PhotoBox", "attic_photos", "Minigame_ArvoreGenealogica", "Organizar as fotos da família", 8, 0.3f, 4, 0.8f, 0.6f, 0.8f, "#B5832F")
        };

        static readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();

        [MenuItem("MovingIn/Build House (blockout)")]
        public static void Build()
        {
            if (GameObject.Find(RootName) != null)
            {
                if (!EditorUtility.DisplayDialog("Casa já existe", $"Já existe um objeto '{RootName}' na cena. Apagar e construir de novo?", "Reconstruir", "Cancelar")) return;
                Undo.DestroyObjectImmediate(GameObject.Find(RootName));
            }

            _materials.Clear();
            EnsureFolder(MaterialFolder);

            GameObject root = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(root, "Build house");
            GameObject hubRoot = GameObject.Find("HubRoot");
            if (hubRoot != null) root.transform.SetParent(hubRoot.transform, false);

            var rooms = new Dictionary<string, Room>();
            var roomDefs = new Dictionary<string, RoomDef>();
            var groups = new Dictionary<string, Transform>();

            foreach (RoomDef def in Rooms)
            {
                Room room = CreateRoom(root.transform, def, groups);
                rooms[def.name] = room;
                roomDefs[def.name] = def;
            }

            var gaps = new Dictionary<string, List<Vector2>>();
            foreach (EdgeDoor door in EdgeDoors)
            {
                AddEdgeDoor(rooms, roomDefs, groups, gaps, door.a, door.b, door.x, door.z);
                AddEdgeDoor(rooms, roomDefs, groups, gaps, door.b, door.a, door.x, door.z);
            }
            foreach (Portal p in Portals)
            {
                RoomDef from = roomDefs[p.from];
                if (OnEdge(from, p.trigger.x, p.trigger.y, out int side)) AddGap(gaps, p.from, side, p.trigger);
                Transform spawn = CreateSpawn(groups[p.to + "/Spawns"], $"Spawn_From_{p.from}", roomDefs[p.to], p.spawn);
                CreateDoorTrigger(groups[p.from + "/Doors"], $"Door_To_{p.to}", from, p.trigger, new Vector3(p.size.x, 2f, p.size.y), rooms[p.to], spawn);
            }

            foreach (RoomDef def in Rooms)
                if (!def.exterior) CreateWalls(groups[def.name + "/Walls_Back"], groups[def.name + "/Walls_Front"], def, gaps);

            CreateExteriorLimits(groups["Front/Walls_Front"]);

            foreach (Prop prop in Props)
            {
                Transform parent = groups[$"{prop.room}/{GroupFolder(prop.group)}"];
                RoomDef def = roomDefs[prop.room];
                Vector3 pos = def.exterior ? prop.pos : AwayFromWalls(def, prop.pos, prop.size);
                CreateCube(parent, prop.name, def.y, pos, prop.size, prop.color, prop.collider);
            }

            foreach (MinigameDef mg in Minigames)
                CreateMinigame(groups, roomDefs[mg.room], mg);

            Transform start = CreateSpawn(groups["Front/Spawns"], "Spawn_Start", roomDefs["Front"], new Vector2(13, -6));
            foreach (RoomDef def in Rooms)
            {
                Vector2 min, max;
                if (def.exterior)
                {
                    min = new Vector2(-1f, -8f);
                    max = new Vector2(22f, -2f);
                }
                else
                {
                    FollowRange(def.x0, def.x1, out min.x, out max.x);
                    FollowRange(def.z0, def.z1, out min.y, out max.y);
                }
                SetupFollowCamera(groups[def.name + "/Systems"], def.y, min, max);
            }
            SetupRoomManagerAndPlayer(rooms["Front"], start);

            WarnOldRooms(root);
            EditorSceneManager.MarkSceneDirty(root.scene);
            Selection.activeGameObject = root;
            Debug.Log("[House] Casa construida. Confira os avisos acima, se houver.");
        }

        static Vector3 AwayFromWalls(RoomDef r, Vector3 pos, Vector3 size)
        {
            pos.x = AwayOnAxis(pos.x, size.x, r.x0 + WallThickness / 2f, r.x1 - WallThickness / 2f);
            pos.z = AwayOnAxis(pos.z, size.z, r.z0 + WallThickness / 2f, r.z1 - WallThickness / 2f);
            return pos;
        }

        static float AwayOnAxis(float center, float size, float innerMin, float innerMax)
        {
            const float Gap = 0.01f;
            const float Thin = 0.15f;
            float half = size / 2f;

            if (size <= Thin)
            {
                if (center + half > innerMax - Gap) return innerMax - half - Gap;
                if (center - half < innerMin + Gap) return innerMin + half + Gap;
                return center;
            }
            if (Mathf.Abs(center + half - innerMax) < Gap) return center - 2f * Gap;
            if (Mathf.Abs(center - half - innerMin) < Gap) return center + 2f * Gap;
            return center;
        }

        static Room CreateRoom(Transform root, RoomDef def, Dictionary<string, Transform> groups)
        {
            GameObject go = new GameObject(def.name);
            go.transform.SetParent(root, false);
            Room room = go.AddComponent<Room>();

            foreach (string g in new[] { "Walls_Back", "Walls_Front", "Furniture", $"{def.name}_Mess", $"{def.name}_Tidy", "Minigames", "Doors", "Spawns", "Systems" })
            {
                GameObject child = new GameObject(g);
                child.transform.SetParent(go.transform, false);
                groups[$"{def.name}/{GroupKey(def.name, g)}"] = child.transform;
            }

            Transform focus = new GameObject("CameraFocus").transform;
            focus.SetParent(go.transform, false);
            focus.position = new Vector3((def.x0 + def.x1) / 2f, def.y + 1.5f, (def.z0 + def.z1) / 2f);

            if (!def.exterior)
                CreateCube(go.transform, "Floor", def.y, new Vector3((def.x0 + def.x1) / 2f, -0.05f, (def.z0 + def.z1) / 2f),
                    new Vector3(def.x1 - def.x0, 0.1f, def.z1 - def.z0), def.floor, true);

            SetString(room, def.name, "_roomName", "roomName");
            SetObject(room, focus, "cameraFocus", "_cameraFocus");
            SetFloat(room, CameraSize, "_cameraSize", "cameraSize");
            return room;
        }

        static string GroupKey(string room, string folder)
        {
            if (folder == $"{room}_Mess") return "Mess";
            if (folder == $"{room}_Tidy") return "Tidy";
            return folder;
        }

        static string GroupFolder(Group g) => g == Group.Fixos ? "Furniture" : g == Group.Bagunca ? "Mess" : "Tidy";

        static void AddEdgeDoor(Dictionary<string, Room> rooms, Dictionary<string, RoomDef> defs, Dictionary<string, Transform> groups,
                                Dictionary<string, List<Vector2>> gaps, string from, string to, float x, float z)
        {
            RoomDef a = defs[from];
            RoomDef b = defs[to];
            if (!OnEdge(a, x, z, out int sideA) || !OnEdge(b, x, z, out int sideB))
            {
                Debug.LogError($"[House] Porta {from}->{to} em ({x}, {z}) nao esta na borda dos dois comodos.");
                return;
            }
            AddGap(gaps, from, sideA, new Vector2(x, z));

            bool alongX = sideA == 0 || sideA == 1;
            Vector2 matXZ = new Vector2(x, z) + Inward(sideA) * 0.45f;
            Vector3 matSize = alongX ? new Vector3(1.4f, 0.04f, 0.6f) : new Vector3(0.6f, 0.04f, 1.4f);
            CreateCube(groups[from + "/Doors"], $"DoorMat_To_{to}", a.y, new Vector3(matXZ.x, 0.02f, matXZ.y), matSize, Marker, false);

            Vector2 inward = Inward(sideB) * 1.5f;
            Transform spawn = CreateSpawn(groups[to + "/Spawns"], $"Spawn_From_{from}", b, new Vector2(x, z) + inward);
            bool horizontal = sideA == 0 || sideA == 1;
            Vector3 size = horizontal ? new Vector3(DoorWidth, 2f, 0.6f) : new Vector3(0.6f, 2f, DoorWidth);
            CreateDoorTrigger(groups[from + "/Doors"], $"Door_To_{to}", a, new Vector2(x, z), size, rooms[to], spawn);
        }

        static bool OnEdge(RoomDef r, float x, float z, out int side)
        {
            const float e = 0.01f;
            bool inX = x >= r.x0 - e && x <= r.x1 + e;
            bool inZ = z >= r.z0 - e && z <= r.z1 + e;
            side = -1;
            if (Mathf.Abs(z - r.z0) < e && inX) side = 0;
            else if (Mathf.Abs(z - r.z1) < e && inX) side = 1;
            else if (Mathf.Abs(x - r.x0) < e && inZ) side = 2;
            else if (Mathf.Abs(x - r.x1) < e && inZ) side = 3;
            return side >= 0;
        }

        static Vector2 Inward(int side)
        {
            switch (side)
            {
                case 0: return new Vector2(0, 1);
                case 1: return new Vector2(0, -1);
                case 2: return new Vector2(1, 0);
                default: return new Vector2(-1, 0);
            }
        }

        static void AddGap(Dictionary<string, List<Vector2>> gaps, string room, int side, Vector2 point)
        {
            string key = $"{room}/{side}";
            if (!gaps.TryGetValue(key, out List<Vector2> list)) gaps[key] = list = new List<Vector2>();
            list.Add(point);
        }

        static void CreateWalls(Transform back, Transform front, RoomDef r, Dictionary<string, List<Vector2>> gaps)
        {
            for (int side = 0; side < 4; side++)
            {
                bool horizontal = side == 0 || side == 1;
                float start = horizontal ? r.x0 : r.z0;
                float end = horizontal ? r.x1 : r.z1;
                float fixedCoord = side == 0 ? r.z0 : side == 1 ? r.z1 : side == 2 ? r.x0 : r.x1;
                bool visible = side == 1 || side == 3;

                var cuts = new List<float>();
                if (gaps.TryGetValue($"{r.name}/{side}", out List<Vector2> list))
                    foreach (Vector2 g in list) cuts.Add(horizontal ? g.x : g.y);
                cuts.Sort();

                float cursor = start;
                int index = 0;
                foreach (float c in cuts)
                {
                    CreateWallSegment(visible ? back : front, r, side, horizontal, fixedCoord, cursor, c - DoorWidth / 2f, visible, index++);
                    cursor = c + DoorWidth / 2f;
                }
                CreateWallSegment(visible ? back : front, r, side, horizontal, fixedCoord, cursor, end, visible, index);
            }
        }

        static void CreateWallSegment(Transform parent, RoomDef r, int side, bool horizontal, float fixedCoord, float a, float b, bool visible, int index)
        {
            if (b - a < 0.05f) return;
            string[] names = { "South", "North", "West", "East" };
            float mid = (a + b) / 2f;
            Vector3 pos = horizontal ? new Vector3(mid, WallHeight / 2f, fixedCoord) : new Vector3(fixedCoord, WallHeight / 2f, mid);
            Vector3 size = horizontal ? new Vector3(b - a, WallHeight, WallThickness) : new Vector3(WallThickness, WallHeight, b - a);
            GameObject wall = CreateCube(parent, $"Wall_{names[side]}_{index}", r.y, pos, size, r.wall, true);
            if (!visible) wall.GetComponent<MeshRenderer>().enabled = false;
        }

        static void CreateExteriorLimits(Transform parent)
        {
            Make("Limit_West", new Vector3(-4.1f, 1.5f, -5), new Vector3(0.2f, 3, 10));
            Make("Limit_East", new Vector3(25.1f, 1.5f, -5), new Vector3(0.2f, 3, 10));
            Make("Limit_South", new Vector3(10.5f, 1.5f, -10.1f), new Vector3(29.4f, 3, 0.2f));
            Make("Limit_NorthWest", new Vector3(-2.05f, 1.5f, 0.1f), new Vector3(4.3f, 3, 0.2f));
            Make("Limit_NorthEast", new Vector3(23.05f, 1.5f, 0.1f), new Vector3(4.3f, 3, 0.2f));

            void Make(string n, Vector3 p, Vector3 s)
            {
                GameObject g = CreateCube(parent, n, 0, p, s, "#FFFFFF", true);
                g.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        static void CreateMinigame(Dictionary<string, Transform> groups, RoomDef room, MinigameDef mg)
        {
            GameObject go = CreateCube(groups[room.name + "/Minigames"], mg.name, room.y, AwayFromWalls(room, mg.pos, mg.size), mg.size, mg.color, true);
            MinigameInteractable mi = go.AddComponent<MinigameInteractable>();
            SetString(mi, mg.id, "_minigameId", "minigameId");
            SetString(mi, mg.scene, "_sceneName", "sceneName");
            SetString(mi, mg.prompt, "_prompt", "prompt");
            var hide = new List<Object>();
            var show = new List<Object>();
            if (mg.hideSelf) hide.Add(go);
            if (mg.roomState)
            {
                hide.Add(groups[room.name + "/Mess"].gameObject);
                show.Add(groups[room.name + "/Tidy"].gameObject);
            }
            SetArray(mi, hide.ToArray(), "hideWhenDone", "_hideWhenDone");
            SetArray(mi, show.ToArray(), "showWhenDone", "_showWhenDone");
            go.SetActive(mg.active);
        }

        static Transform CreateSpawn(Transform parent, string name, RoomDef room, Vector2 xz)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(parent, false);
            t.position = new Vector3(xz.x, room.y + 0.8f, xz.y);
            return t;
        }

        static void CreateDoorTrigger(Transform parent, string name, RoomDef room, Vector2 xz, Vector3 size, Room target, Transform spawn)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(xz.x, room.y + 1f, xz.y);
            BoxCollider box = go.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;
            Door door = go.AddComponent<Door>();
            box.isTrigger = true;
            SetObject(door, target, "targetRoom", "_targetRoom");
            SetObject(door, spawn, "spawnPoint", "_spawnPoint");
        }

        static void FollowRange(float a, float b, out float min, out float max)
        {
            min = a + FollowMargin;
            max = b - FollowMargin;
            if (min > max) min = max = (a + b) / 2f;
        }

        static void SetupFollowCamera(Transform parent, float floorY, Vector2 min, Vector2 max)
        {
            GameObject go = new GameObject("CameraFollow");
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(0f, floorY, 0f);
            RoomCameraFollow follow = go.AddComponent<RoomCameraFollow>();
            var bounds = new SerializedObject(follow);
            bounds.FindProperty("_min").vector2Value = min;
            bounds.FindProperty("_max").vector2Value = max;
            bounds.ApplyModifiedPropertiesWithoutUndo();

            PlayerMovement player = Object.FindObjectOfType<PlayerMovement>(true);
            IsoCamera iso = Object.FindObjectOfType<IsoCamera>(true);
            if (player != null) SetObject(follow, player.transform, "player");
            if (iso != null)
            {
                SetObject(follow, iso.transform, "cameraTransform");
                var so = new SerializedObject(iso);
                SerializedProperty d = so.FindProperty("_distance");
                if (d != null) SetFloat(follow, d.floatValue, "_distance");
            }
            if (player == null || iso == null)
                Debug.LogWarning($"[House] Ligue Player e Camera Transform no CameraFollow de {parent.parent.name}.", follow);
        }

        static void SetupRoomManagerAndPlayer(Room start, Transform startPoint)
        {
            RoomManager manager = Object.FindObjectOfType<RoomManager>(true);
            if (manager != null) SetObject(manager, start, "startRoom", "_startRoom");
            else Debug.LogWarning("[House] RoomManager nao encontrado: defina o Start Room como 'Front'.");

            PlayerMovement player = Object.FindObjectOfType<PlayerMovement>(true);
            if (player != null)
            {
                Undo.RecordObject(player.transform, "Move player");
                player.transform.position = startPoint.position;
            }
        }

        static void WarnOldRooms(GameObject root)
        {
            foreach (Room r in Object.FindObjectsOfType<Room>(true))
                if (!r.transform.IsChildOf(root.transform))
                    Debug.LogWarning($"[House] Comodo antigo encontrado: '{r.name}'. Apague a casa antiga para o RoomManager nao se confundir.", r);
        }

        static GameObject CreateCube(Transform parent, string name, float floorY, Vector3 pos, Vector3 size, string color, bool collider)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = new Vector3(pos.x, floorY + pos.y, pos.z);
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = GetMaterial(color);
            if (!collider) Object.DestroyImmediate(go.GetComponent<BoxCollider>());
            return go;
        }

        static Material GetMaterial(string hex)
        {
            if (_materials.TryGetValue(hex, out Material cached)) return cached;
            string path = $"{MaterialFolder}/M_{hex.TrimStart('#')}.mat";
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null)
            {
                mat = new Material(Shader.Find("Standard"));
                ColorUtility.TryParseHtmlString(hex, out Color c);
                mat.color = c;
                mat.SetFloat("_Glossiness", 0.1f);
                AssetDatabase.CreateAsset(mat, path);
            }
            _materials[hex] = mat;
            return mat;
        }

        static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }

        static SerializedProperty Find(SerializedObject so, string[] names)
        {
            foreach (string n in names)
            {
                SerializedProperty p = so.FindProperty(n);
                if (p != null) return p;
            }
            Debug.LogWarning($"[House] Campo '{names[0]}' nao encontrado em {so.targetObject.GetType().Name}. Preencha no Inspector.", so.targetObject);
            return null;
        }

        static void SetString(Object target, string value, params string[] names)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = Find(so, names);
            if (p == null) return;
            p.stringValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetFloat(Object target, float value, params string[] names)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = Find(so, names);
            if (p == null) return;
            p.floatValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetObject(Object target, Object value, params string[] names)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = Find(so, names);
            if (p == null) return;
            p.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void SetArray(Object target, Object[] values, params string[] names)
        {
            var so = new SerializedObject(target);
            SerializedProperty p = Find(so, names);
            if (p == null || !p.isArray) return;
            p.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
                p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
