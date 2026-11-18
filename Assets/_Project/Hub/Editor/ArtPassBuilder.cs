using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static partial class ArtPassBuilder
    {
        const string Root = "Assets/_Project/Hub/";
        const string TexFolder = Root + "Art/Textures/";
        const string MatFolder = Root + "Materials/Art";
        const string OutlinePath = Root + "Materials/Story/M_Outline.mat";
        const string FrontKey = "MovingIn.Art.ModelFront";
        const string FrontMenu = "MovingIn/Art/Model Front/";
        const float FloorMetersPerTile = 2f;
        const float GroundMetersPerTile = 2f;
        static readonly HashSet<string> GroundSurfaces = new HashSet<string> { "Grass", "Ground_Surroundings", "Street", "Sidewalk" };

        enum Face { Free, S, W, N, E }

        struct RoomStyle
        {
            public string room, floorTex, floorTint, wallTex, wallColor, trim;
            public RoomStyle(string room, string floorTex, string floorTint, string wallTex, string wallColor, string trim)
            {
                this.room = room; this.floorTex = floorTex; this.floorTint = floorTint; this.wallTex = wallTex; this.wallColor = wallColor; this.trim = trim;
            }
        }

        static readonly RoomStyle[] Rooms =
        {
            new RoomStyle("Garage", "Floor_Concrete", "#FFFFFF", "Wall_Plaster", "#C9C6C0", "#8E8A84"),
            new RoomStyle("LivingRoom", "Floor_Wood", "#FFFFFF", "Wall_Plaster", "#E9D9C3", "#B88D6A"),
            new RoomStyle("Kitchen", "Floor_Tile", "#FFFFFF", "Wall_Plaster", "#DCEBE4", "#8FB3A6"),
            new RoomStyle("Hall", "Floor_Wood", "#FFFFFF", "Wall_Plaster", "#EADFCB", "#B88D6A"),
            new RoomStyle("Laundry", "Floor_TileSmall", "#FFFFFF", "Wall_Plaster", "#DDE7F0", "#9DB2C6"),
            new RoomStyle("Basement", "Floor_Concrete", "#BDB8B0", "Wall_Plaster", "#A9A39A", "#6E685F"),
            new RoomStyle("Bedroom", "Floor_Wood", "#FFFFFF", "Wall_Stripes", "#D7E3F2", "#8CA3C2"),
            new RoomStyle("UpstairsHall", "Floor_Wood", "#FFFFFF", "Wall_Plaster", "#EADFCB", "#B88D6A"),
            new RoomStyle("Attic", "Floor_WoodOld", "#FFFFFF", "Wall_Plaster", "#CDB9A0", "#8B6B4E"),
        };

        struct Surface
        {
            public string name, tex, color;
            public Surface(string name, string tex, string color) { this.name = name; this.tex = tex; this.color = color; }
        }

        static readonly Surface[] Exterior =
        {
            new Surface("House_GroundFloor", "Wall_Siding", "#F1E4CC"),
            new Surface("House_Back", "Wall_Siding", "#F1E4CC"),
            new Surface("House_Upper", "Wall_Siding", "#EAD9BE"),
            new Surface("Roof_Base", "Roof_Tiles", "#C56A4E"),
            new Surface("Roof_Top", "Roof_Tiles", "#B85E44"),
            new Surface("Grass", "Grass", "#FFFFFF"),
            new Surface("Ground_Surroundings", "Grass", "#F2F2F2"),
            new Surface("Path", "Path_Stone", "#FFFFFF"),
            new Surface("Sidewalk", "Sidewalk", "#FFFFFF"),
            new Surface("Street", "Street", "#FFFFFF"),
            new Surface("Fence_Left", null, "#F4F1EA"),
            new Surface("Fence_Right", null, "#F4F1EA"),
            new Surface("FrontDoor", null, "#C8643C"),
            new Surface("GarageDoor", null, "#6E7F8F"),
            new Surface("Window_1", null, "#A9CBE0"),
            new Surface("Window_2", null, "#A9CBE0"),
            new Surface("Window_3", null, "#A9CBE0"),
            new Surface("Mailbox", null, "#C2453A"),
            new Surface("Truck_Box", null, "#F2F2F2"),
            new Surface("Truck_Cab", null, "#3E6FA8"),
            new Surface("StreetBox_1", null, "#C9A26B"),
            new Surface("StreetBox_2", null, "#C9A26B"),
        };

        struct DoorFrame
        {
            public string room; public bool north; public float at;
            public DoorFrame(string room, bool north, float at) { this.room = room; this.north = north; this.at = at; }
        }

        static readonly DoorFrame[] DoorFrames =
        {
            new DoorFrame("Garage", false, 4f),
            new DoorFrame("LivingRoom", false, 4f),
            new DoorFrame("LivingRoom", true, 8.5f),
            new DoorFrame("Kitchen", true, 15.2f),
            new DoorFrame("Bedroom", false, 4f),
        };

        struct Furniture
        {
            public string room, prop, kit;
            public string[] pieces;
            public Face face;
            public bool fill;
            public float height, turn, px, pz;
            public bool moved, solid;
            public Furniture At(float x, float z) { Furniture f = this; f.moved = true; f.px = x; f.pz = z; return f; }
            public Furniture Solid() { Furniture f = this; f.solid = true; return f; }
            public Furniture H(float h) { Furniture f = this; f.height = h; return f; }
            public Furniture T(float degrees) { Furniture f = this; f.turn = degrees; return f; }
        }

        static Furniture M(string room, string prop, Face face, bool fill, params string[] pieces)
        {
            return new Furniture { room = room, prop = prop, face = face, fill = fill, pieces = pieces };
        }

        static Furniture K(string room, string prop, Face face, string kit)
        {
            return new Furniture { room = room, prop = prop, face = face, kit = kit };
        }

        static readonly Furniture[] Furnitures =
        {
            M("LivingRoom", "Sofa", Face.E, false, "loungeSofaLong|loungeSofa|loungeDesignSofa"),
            M("LivingRoom", "TV", Face.W, true, "cabinetTelevision|cabinetTelevisionDoors").H(0.5f),
            M("LivingRoom", "Bookcase", Face.S, true, "bookcaseOpen|bookcaseClosed", "bookcaseOpen|bookcaseClosed"),
            K("LivingRoom", "Rug", Face.S, "RugLiving"),
            K("LivingRoom", "ExitMat", Face.S, "MatDoor"),
            K("LivingRoom", "MovingBoxes", Face.S, "BoxStack"),

            M("Garage", "MetalShelves", Face.S, true, "bookcaseOpen", "bookcaseOpen", "bookcaseOpen", "bookcaseOpen"),
            K("Garage", "Bicycle", Face.E, "Bicycle"),
            M("Garage", "ScatteredBoxes_1", Face.Free, false, "cardboardBoxOpen|cardboardBoxClosed"),
            M("Garage", "ScatteredBoxes_2", Face.Free, false, "cardboardBoxClosed"),
            K("Garage", "BoxPile", Face.S, "BoxPile"),

            M("Kitchen", "SinkCounter", Face.S, true, "kitchenCabinetDrawer|kitchenCabinet", "kitchenSink", "kitchenCabinet|kitchenCabinetDrawer"),
            M("Kitchen", "Stove", Face.S, true, "kitchenStove|kitchenStoveElectric"),
            M("Kitchen", "Fridge", Face.W, true, "kitchenFridgeLarge|kitchenFridge"),
            M("Kitchen", "WallCabinet", Face.W, true, "kitchenCabinetUpperDouble|kitchenCabinetUpper", "kitchenCabinetUpperDouble|kitchenCabinetUpper"),
            M("Kitchen", "Table", Face.Free, true, "table|tableCloth").At(18.0f, 3.4f).Solid(),
            M("Kitchen", "KitchenBox", Face.S, false, "cardboardBoxOpen|cardboardBoxClosed"),
            K("Kitchen", "FloorPlates", Face.S, "Plates"),
            K("Kitchen", "FloorPan", Face.S, "Pans"),

            K("Laundry", "WashingMachine", Face.S, "Washer"),
            M("Laundry", "UtilitySink", Face.S, true, "kitchenSink|bathroomSinkSquare"),
            K("Laundry", "LaundryBasket", Face.S, "LaundryBasket"),
            K("Laundry", "FloorClothes", Face.S, "ClothesPile"),
            K("Laundry", "FullClothesline", Face.S, "Clothesline"),

            K("Hall", "StairsUp", Face.S, "StairsUp"),
            K("Hall", "StairsDown", Face.S, "StairsDown"),
            M("Hall", "CoatRack", Face.S, false, "coatRackStanding|coatRack"),
            K("Hall", "StuckDoor_Restroom", Face.W, "DoorInterior"),

            K("Basement", "StairsUp", Face.S, "StairsUp"),
            K("Basement", "BreakerPanel", Face.S, "BreakerPanel"),
            K("Basement", "Boiler", Face.S, "Boiler"),
            M("Basement", "Shelves", Face.S, true, "bookcaseOpen", "bookcaseOpen"),
            K("Basement", "OldBoxes", Face.S, "BoxStackOld"),
            K("Basement", "LightBulb", Face.S, "HangingBulb"),

            M("Bedroom", "Bed", Face.S, false, "bedDouble|bedSingle"),
            K("Bedroom", "Bed_FirstNight", Face.S, "Blanket"),
            M("Bedroom", "Wardrobe", Face.W, true, "bookcaseClosedDoors|bookcaseClosed", "bookcaseClosedDoors|bookcaseClosed"),
            M("Bedroom", "Desk", Face.S, true, "desk|table"),
            M("Bedroom", "Chair", Face.N, false, "chairDesk|chair").At(2.7f, 6.55f),
            M("Bedroom", "Bookshelf", Face.S, true, "bookcaseOpen", "bookcaseOpen"),
            K("Bedroom", "Window", Face.S, "WindowInterior"),
            K("Bedroom", "HamsterCage", Face.S, "HamsterCage"),
            K("Bedroom", "LoosePipes", Face.S, "LoosePipes"),
            K("Bedroom", "AssembledTubes", Face.S, "AssembledTubes"),

            K("UpstairsHall", "StairsDown", Face.S, "StairsDown"),
            K("UpstairsHall", "AtticLadder", Face.W, "Ladder"),
            K("UpstairsHall", "StuckDoor_Bathroom", Face.W, "DoorInterior"),
            K("UpstairsHall", "Painting", Face.W, "Painting"),

            K("Attic", "Chest", Face.S, "Chest"),
            M("Attic", "OldFurniture", Face.S, false, "loungeChair|chairCushion"),
            K("Attic", "RoundWindow", Face.S, "RoundWindow"),
            K("Attic", "Hatch", Face.N, "Hatch"),
            K("Attic", "FloorPhotos", Face.S, "Photos"),
            K("Attic", "FamilyTreeBoard", Face.W, "FamilyTree"),
            M("Attic", "PhotoBox", Face.S, false, "cardboardBoxOpen|cardboardBoxClosed"),

            K("Front", "FrontDoor", Face.S, "DoorFront"),
            K("Front", "GarageDoor", Face.S, "GarageDoor"),
            K("Front", "Window_1", Face.S, "WindowExterior"),
            K("Front", "Window_2", Face.S, "WindowExterior"),
            K("Front", "Window_3", Face.S, "WindowExterior"),
            K("Front", "Mailbox", Face.S, "Mailbox"),
            K("Front", "Fence_Left", Face.S, "Fence"),
            K("Front", "Fence_Right", Face.S, "Fence"),
            K("Front", "Truck_Box", Face.E, "TruckBox"),
            K("Front", "Truck_Cab", Face.E, "TruckCab"),
            M("Front", "StreetBox_1", Face.Free, false, "cardboardBoxClosed"),
            M("Front", "StreetBox_2", Face.Free, false, "cardboardBoxClosed"),
        };

        struct RoomRect
        {
            public float x0, z0, x1, z1;
            public RoomRect(float x0, float z0, float x1, float z1) { this.x0 = x0; this.z0 = z0; this.x1 = x1; this.z1 = z1; }
        }

        static readonly Dictionary<string, RoomRect> Rects = new Dictionary<string, RoomRect>
        {
            { "Garage", new RoomRect(0, 0, 6, 8) }, { "LivingRoom", new RoomRect(6, 0, 14, 8) }, { "Kitchen", new RoomRect(14, 0, 21, 8) },
            { "Hall", new RoomRect(6, 8, 11, 12) }, { "Laundry", new RoomRect(14, 8, 21, 12) }, { "Basement", new RoomRect(4, 0, 17, 8) },
            { "Bedroom", new RoomRect(0, 0, 10, 8) }, { "UpstairsHall", new RoomRect(10, 0, 14, 8) }, { "Attic", new RoomRect(2, 0, 14, 8) },
        };

        static readonly Dictionary<string, Material> _mats = new Dictionary<string, Material>();
        static Dictionary<string, string> _modelIndex;
        static Vector3 _front = Vector3.left;

        [MenuItem("MovingIn/Art/Apply Room Style")]
        static void ApplyRoomStyle()
        {
            Transform house = FindHouse();
            if (house == null) return;
            _mats.Clear();
            int count = 0;

            foreach (RoomStyle style in Rooms)
            {
                Transform room = house.Find(style.room);
                if (room == null) continue;

                Transform floor = room.Find("Floor");
                if (floor != null && Paint(floor, Mat(style.floorTex, style.floorTint), new Vector2(floor.lossyScale.x, floor.lossyScale.z) / FloorMetersPerTile)) count++;

                Material wall = Mat(style.wallTex, style.wallColor);
                foreach (string group in new[] { "Walls_Back", "Walls_Front" })
                {
                    Transform g = room.Find(group);
                    if (g == null) continue;
                    foreach (Transform w in g)
                        if (w.name.StartsWith("Wall_") && Paint(w, wall, new Vector2(Mathf.Max(w.lossyScale.x, w.lossyScale.z), w.lossyScale.y))) count++;
                }
                Material trim = Mat(null, style.trim);
                BuildTrim(room, trim);
                BuildDoorFrames(room, style.room, wall, trim);

                Transform doors = room.Find("Doors");
                if (doors != null)
                    foreach (Transform d in doors)
                        if (d.name.StartsWith("DoorMat_") && d.GetComponent<MeshRenderer>() != null)
                        {
                            PlaceKit(d, "MatDoor", Face.S);
                            count++;
                        }
            }

            Transform front = house.Find("Front");
            if (front != null)
            {
                foreach (Surface s in Exterior)
                {
                    Transform t = FindDeep(front, s.name);
                    if (t == null) continue;
                    Vector3 sc = t.lossyScale;
                    Vector2 tiling = s.name.StartsWith("House_") ? new Vector2(Mathf.Max(sc.x, sc.z), sc.y) : new Vector2(sc.x, sc.z);
                    if (GroundSurfaces.Contains(s.name)) tiling /= GroundMetersPerTile;
                    if (Paint(t, Mat(s.tex, s.color), tiling)) count++;
                }
                foreach (string tree in new[] { "Tree_1", "Tree_2" })
                {
                    Transform t = FindDeep(front, tree);
                    if (t != null) BuildTree(t);
                }
            }

            int hidden = HidePlayerLeftovers();
            AssetDatabase.SaveAssets();
            Debug.Log($"[ArtPass] Estilo aplicado em {count} superfícies, com rodapés, batentes e árvores. Peças soltas escondidas no Player: {hidden}.");
        }

        [MenuItem("MovingIn/Art/Apply Furniture Models")]
        static void ApplyFurniture()
        {
            Transform house = FindHouse();
            if (house == null) return;
            _mats.Clear();
            BuildModelIndex();
            _front = ResolveFront(out string how);

            int models = 0, kits = 0;
            List<string> missing = new List<string>();
            foreach (Furniture f in Furnitures)
            {
                Transform room = house.Find(f.room);
                Transform prop = room != null ? FindDeep(room, f.prop) : null;
                if (prop == null) continue;
                Adjust(prop, f);

                if (f.pieces != null && PlaceKenney(prop, f)) { models++; continue; }
                if (f.kit != null)
                {
                    PlaceKit(prop, f.kit, f.face);
                    kits++;
                    continue;
                }
                missing.Add($"{f.room}/{f.prop}");
            }

            SnapBlanket(house);
            AddTableChairs(house);
            int fences = DressExtraFences(house);
            BuildCar(house);
            int shelves = FillShelves(house);
            int decor = BuildDecor(house);
            int highlights = SetupHighlights();

            AssetDatabase.SaveAssets();
            if (missing.Count > 0) Debug.LogWarning("[ArtPass] Sem modelo (continuam como bloco): " + string.Join(", ", missing));
            Debug.Log($"[ArtPass] Frente dos modelos: {FrontName(_front)} ({how}). {models} móveis com modelo, {kits} improvisados, {decor} itens de decoração, {shelves} prateleiras com objetos, {fences} cercas extras, {highlights} contornos atualizados.");
        }

        [MenuItem("MovingIn/Art/Rotate Selected Model 90°")]
        static void RotateSelected()
        {
            int n = 0;
            foreach (Transform t in Selection.transforms)
            {
                Transform model = t.name == "Model" ? t : t.Find("Model");
                for (Transform p = t; model == null && p != null; p = p.parent)
                    if (p.name == "Model") model = p;
                if (model == null) continue;
                foreach (Transform child in model)
                {
                    Undo.RecordObject(child, "Rotate Model");
                    child.RotateAround(model.position, Vector3.up, 90f);
                }
                n++;
            }
            Debug.Log($"[ArtPass] {n} modelos girados 90°. Se rodar Apply Furniture Models de novo, o giro manual se perde.");
        }

        [MenuItem(FrontMenu + "Auto (detect)")] static void Front0() => SetFront(0);
        [MenuItem(FrontMenu + "+Z")] static void Front1() => SetFront(1);
        [MenuItem(FrontMenu + "-Z")] static void Front2() => SetFront(2);
        [MenuItem(FrontMenu + "+X")] static void Front3() => SetFront(3);
        [MenuItem(FrontMenu + "-X")] static void Front4() => SetFront(4);
        [MenuItem(FrontMenu + "Auto (detect)", true)] static bool Front0V() => FrontCheck(0, "Auto (detect)");
        [MenuItem(FrontMenu + "+Z", true)] static bool Front1V() => FrontCheck(1, "+Z");
        [MenuItem(FrontMenu + "-Z", true)] static bool Front2V() => FrontCheck(2, "-Z");
        [MenuItem(FrontMenu + "+X", true)] static bool Front3V() => FrontCheck(3, "+X");
        [MenuItem(FrontMenu + "-X", true)] static bool Front4V() => FrontCheck(4, "-X");

        static void SetFront(int mode)
        {
            EditorPrefs.SetInt(FrontKey, mode);
            Debug.Log("[ArtPass] Frente dos modelos definida. Rode MovingIn > Art > Apply Furniture Models de novo.");
        }

        static bool FrontCheck(int mode, string label)
        {
            Menu.SetChecked(FrontMenu + label, EditorPrefs.GetInt(FrontKey, 0) == mode);
            return true;
        }

        [MenuItem("MovingIn/Art/Revert to Blockout")]
        static void Revert()
        {
            int restored = 0;
            foreach (ArtGenerated g in Object.FindObjectsOfType<ArtGenerated>(true))
                if (g != null) Undo.DestroyObjectImmediate(g.gameObject);
            foreach (ArtOriginal o in Object.FindObjectsOfType<ArtOriginal>(true))
            {
                MeshRenderer r = o.GetComponent<MeshRenderer>();
                if (r != null)
                {
                    Undo.RecordObject(r, "Revert Art");
                    if (o.material != null) r.sharedMaterial = o.material;
                    r.enabled = o.rendererWasEnabled;
                }
                TextureTiling tt = o.GetComponent<TextureTiling>();
                if (tt != null) Undo.DestroyObjectImmediate(tt);
                if (r != null) r.SetPropertyBlock(null);
                Undo.DestroyObjectImmediate(o);
                restored++;
            }
            Debug.Log($"[ArtPass] {restored} objetos voltaram ao blockout.");
        }

        static bool PlaceKenney(Transform prop, Furniture f)
        {
            List<GameObject> assets = new List<GameObject>();
            GameObject any = null;
            foreach (string piece in f.pieces)
            {
                GameObject a = LoadModel(piece);
                assets.Add(a);
                if (any == null) any = a;
            }
            if (any == null) return false;
            for (int i = 0; i < assets.Count; i++)
                if (assets[i] == null) assets[i] = any;

            Vector3 b = prop.lossyScale;
            float h = f.height > 0f ? f.height : b.y;
            Transform root = NewModelRoot(prop);
            bool alongX = b.x >= b.z;
            float length = alongX ? b.x : b.z;
            int n = assets.Count;
            for (int i = 0; i < n; i++)
            {
                float c = -length / 2f + (i + 0.5f) * length / n;
                Vector3 center = alongX ? new Vector3(c, 0f, 0f) : new Vector3(0f, 0f, c);
                Vector3 size = alongX ? new Vector3(length / n, h, b.z) : new Vector3(b.x, h, length / n);
                float yaw = (f.face == Face.Free ? FreeYaw(assets[i], size) : YawFor(f.face)) + f.turn;
                PlaceAsset(root, assets[i], center, size, yaw, f.fill, 1.6f);
            }
            HideBox(prop);
            return true;
        }

        static void PlaceKit(Transform prop, string kind, Face face)
        {
            Vector3 b = prop.lossyScale;
            Transform root = NewModelRoot(prop);
            bool turned = face == Face.W || face == Face.E;
            Transform pivot = NewChild("Kit", root);
            pivot.localRotation = Quaternion.Euler(0f, KitYaw(face), 0f);
            BuildKit(kind, pivot, turned ? b.z : b.x, b.y, turned ? b.x : b.z);
            HideBox(prop);
        }

        static void Adjust(Transform prop, Furniture f)
        {
            if (f.moved)
            {
                Undo.RecordObject(prop, "Art Pass");
                prop.position = new Vector3(f.px, prop.position.y, f.pz);
            }
            if (f.solid)
            {
                BoxCollider bc = prop.GetComponent<BoxCollider>();
                if (bc == null) bc = Undo.AddComponent<BoxCollider>(prop.gameObject);
                Undo.RecordObject(bc, "Art Pass");
                bc.isTrigger = false;
            }
        }

        static void AddTableChairs(Transform house)
        {
            Transform kitchen = house.Find("Kitchen");
            Transform table = kitchen != null ? FindDeep(kitchen, "Table") : null;
            Transform model = table != null ? table.Find("Model") : null;
            if (model == null) return;
            GameObject asset = LoadModel("chair|chairCushion|chairModernCushion");
            Vector3 b = table.lossyScale;
            float bottom = table.position.y - b.y / 2f;
            bool alongX = b.x >= b.z;
            Vector3 along = alongX ? Vector3.right : Vector3.forward;
            Vector3 across = alongX ? Vector3.forward : Vector3.right;
            float half = (alongX ? b.z : b.x) / 2f + 0.35f;
            Transform group = NewChild("Chairs", model);
            foreach (float s in new[] { -1f, 1f })
                foreach (float a in new[] { -0.4f, 0.4f })
                {
                    Transform holder = NewChild("Chair", group);
                    holder.position = new Vector3(table.position.x, bottom, table.position.z) + along * a + across * s * half;
                    holder.rotation = Quaternion.identity;
                    Face face = alongX ? (s < 0 ? Face.N : Face.S) : (s < 0 ? Face.E : Face.W);
                    if (asset != null) PlaceAsset(holder, asset, Vector3.zero, new Vector3(0.45f, 0.85f, 0.45f), YawFor(face), false, 1f);
                    else
                    {
                        Transform pivot = NewChild("Kit", holder);
                        pivot.localRotation = Quaternion.Euler(0f, KitYaw(face), 0f);
                        BuildKit("Chair", pivot, 0.45f, 0.85f, 0.45f);
                    }
                }
        }

        static int DressExtraFences(Transform house)
        {
            int n = 0;
            foreach (Transform t in house.GetComponentsInChildren<Transform>(true))
            {
                if (!t.name.StartsWith("Fence", System.StringComparison.OrdinalIgnoreCase)) continue;
                if (t.name == "Fence_Left" || t.name == "Fence_Right") continue;
                if (t.GetComponent<MeshRenderer>() == null || IsGenerated(t)) continue;
                Vector3 b = t.lossyScale;
                PlaceKit(t, "Fence", b.x >= b.z ? Face.S : Face.W);
                n++;
            }
            return n;
        }

        static bool IsGenerated(Transform t)
        {
            for (Transform p = t; p != null; p = p.parent)
                if (p.GetComponent<ArtGenerated>() != null) return true;
            return false;
        }

        static void BuildCar(Transform house)
        {
            Transform front = house.Find("Front");
            Transform car = front != null ? FindDeep(front, "ParentsCar") : null;
            if (car == null) return;
            Transform old = car.Find("Model");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            foreach (MeshRenderer r in car.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (!r.enabled) continue;
                Remember(r.transform, r);
                Undo.RecordObject(r, "Art Pass");
                r.enabled = false;
            }
            GameObject go = new GameObject("Model");
            Undo.RegisterCreatedObjectUndo(go, "Art Pass");
            go.AddComponent<ArtGenerated>();
            go.transform.SetParent(car, false);
            Transform pivot = NewChild("Kit", go.transform);
            pivot.localRotation = Quaternion.Euler(0f, KitYaw(Face.E), 0f);
            BuildKit("Car", pivot, 2f, 1.8f, 4f);
        }

        static Transform NewModelRoot(Transform prop)
        {
            Transform old = prop.Find("Model");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject go = new GameObject("Model");
            Undo.RegisterCreatedObjectUndo(go, "Art Pass");
            go.AddComponent<ArtGenerated>();
            go.transform.SetParent(prop, false);
            Vector3 s = prop.lossyScale;
            go.transform.localScale = new Vector3(1f / s.x, 1f / s.y, 1f / s.z);
            go.transform.localRotation = Quaternion.identity;
            go.transform.position = prop.position - prop.up * s.y / 2f;
            return go.transform;
        }

        static void HideBox(Transform prop)
        {
            MeshRenderer box = prop.GetComponent<MeshRenderer>();
            if (box == null) return;
            Remember(prop, box);
            Undo.RecordObject(box, "Art Pass");
            box.enabled = false;
        }

        static void PlaceAsset(Transform root, GameObject asset, Vector3 center, Vector3 size, float yaw, bool fill, float maxHeight)
        {
            Transform fit = NewChild("Fit", root);
            fit.localPosition = center;
            GameObject inst = (GameObject)PrefabUtility.InstantiatePrefab(asset, fit);
            inst.transform.localPosition = Vector3.zero;
            inst.transform.localRotation = Quaternion.Euler(0f, yaw, 0f) * asset.transform.localRotation;
            inst.transform.localScale = asset.transform.localScale;
            foreach (Collider c in inst.GetComponentsInChildren<Collider>(true)) Object.DestroyImmediate(c);

            Vector3 n = MeshBounds(inst).size;
            n = new Vector3(Mathf.Max(0.001f, n.x), Mathf.Max(0.001f, n.y), Mathf.Max(0.001f, n.z));
            Vector3 k;
            if (fill)
            {
                k = new Vector3(size.x * 0.99f / n.x, size.y / n.y, size.z * 0.99f / n.z);
            }
            else
            {
                float s = Mathf.Min(size.x / n.x, size.z / n.z) * 0.98f;
                if (size.y > 0.05f && n.y * s > size.y * maxHeight) s = size.y * maxHeight / n.y;
                k = Vector3.one * s;
            }
            fit.localScale = k;

            Bounds placed = MeshBounds(inst);
            Vector3 target = root.TransformPoint(center);
            fit.position += target - new Vector3(placed.center.x, placed.min.y, placed.center.z);
        }

        static float FreeYaw(GameObject asset, Vector3 size)
        {
            Vector3 n = NaturalSize(asset);
            return (size.x >= size.z) == (n.x >= n.z) ? 0f : 90f;
        }

        static float YawFor(Face face)
        {
            float a = Vector3.SignedAngle(_front, FaceDir(face), Vector3.up);
            return Mathf.Round(a / 90f) * 90f;
        }

        static float KitYaw(Face face)
        {
            return face == Face.W ? 90f : face == Face.N ? 180f : face == Face.E ? 270f : 0f;
        }

        static Vector3 FaceDir(Face face)
        {
            return face == Face.W ? Vector3.left : face == Face.N ? Vector3.forward : face == Face.E ? Vector3.right : Vector3.back;
        }

        static Vector3 ResolveFront(out string how)
        {
            int mode = EditorPrefs.GetInt(FrontKey, 0);
            if (mode > 0)
            {
                how = "escolhida no menu";
                return mode == 1 ? Vector3.forward : mode == 2 ? Vector3.back : mode == 3 ? Vector3.right : Vector3.left;
            }
            foreach (string key in new[] { "loungeSofa", "loungeSofaLong", "chair", "chairDesk", "bedSingle", "loungeChair" })
            {
                GameObject a = LoadModel(key);
                if (a == null) continue;
                Vector3 f = DetectFront(a);
                if (f == Vector3.zero) continue;
                how = $"detectada pelo encosto do modelo {key}";
                return f;
            }
            how = "padrão, não deu para detectar";
            return Vector3.left;
        }

        static Vector3 DetectFront(GameObject asset)
        {
            GameObject probe = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            probe.transform.position = Vector3.zero;
            List<Vector3> points = new List<Vector3>();
            foreach (MeshFilter mf in probe.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null) continue;
                Matrix4x4 m = mf.transform.localToWorldMatrix;
                foreach (Vector3 v in mf.sharedMesh.vertices) points.Add(m.MultiplyPoint3x4(v));
            }
            Object.DestroyImmediate(probe);
            if (points.Count == 0) return Vector3.zero;

            Bounds b = new Bounds(points[0], Vector3.zero);
            foreach (Vector3 p in points) b.Encapsulate(p);
            float cut = b.min.y + b.size.y * 0.72f;
            Vector3 sum = Vector3.zero;
            int n = 0;
            foreach (Vector3 p in points)
                if (p.y >= cut) { sum += p; n++; }
            if (n == 0) return Vector3.zero;

            Vector3 c = sum / n - b.center;
            float ox = c.x / Mathf.Max(0.001f, b.size.x);
            float oz = c.z / Mathf.Max(0.001f, b.size.z);
            if (Mathf.Max(Mathf.Abs(ox), Mathf.Abs(oz)) < 0.12f) return Vector3.zero;
            return Mathf.Abs(ox) > Mathf.Abs(oz) ? new Vector3(-Mathf.Sign(ox), 0f, 0f) : new Vector3(0f, 0f, -Mathf.Sign(oz));
        }

        static string FrontName(Vector3 f)
        {
            if (f == Vector3.forward) return "+Z";
            if (f == Vector3.back) return "-Z";
            if (f == Vector3.right) return "+X";
            return "-X";
        }

        static void SnapBlanket(Transform house)
        {
            Transform bedroom = house.Find("Bedroom");
            if (bedroom == null) return;
            Transform bed = FindDeep(bedroom, "Bed");
            Transform blanket = FindDeep(bedroom, "Bed_FirstNight");
            Transform model = blanket != null ? blanket.Find("Model") : null;
            if (bed == null || model == null) return;
            float top = bed.position.y + bed.lossyScale.y / 2f + 1.5f;
            if (SurfaceY(bed, blanket.position, top, out float y))
                model.position = new Vector3(model.position.x, y, model.position.z);
        }

        static bool SurfaceY(Transform support, Vector3 at, float fromY, out float y)
        {
            y = 0f;
            Transform model = support.Find("Model");
            if (model == null) return false;
            List<MeshCollider> temp = new List<MeshCollider>();
            foreach (MeshFilter mf in model.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null) continue;
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                temp.Add(mc);
            }
            Physics.SyncTransforms();
            bool found = false;
            foreach (RaycastHit hit in Physics.RaycastAll(new Vector3(at.x, fromY, at.z), Vector3.down, 10f, ~0, QueryTriggerInteraction.Ignore))
            {
                if (!(hit.collider is MeshCollider mc) || !temp.Contains(mc)) continue;
                if (!found || hit.point.y > y) y = hit.point.y;
                found = true;
            }
            foreach (MeshCollider mc in temp) Object.DestroyImmediate(mc);
            return found;
        }

        struct ShelfFill
        {
            public string room, prop, kind, target;
            public ShelfFill(string room, string prop, string kind, string target = null) { this.room = room; this.prop = prop; this.kind = kind; this.target = target; }
        }

        static readonly ShelfFill[] ShelfFills =
        {
            new ShelfFill("LivingRoom", "Bookcase", "Books"),
            new ShelfFill("Bedroom", "Bookshelf", "Books"),
            new ShelfFill("Basement", "Shelves", "Jars"),
            new ShelfFill("Garage", "MetalShelves", "ShelfBoxes", "ShelvedBoxes"),
        };

        static int FillShelves(Transform house)
        {
            int filled = 0;
            foreach (ShelfFill s in ShelfFills)
            {
                Transform room = house.Find(s.room);
                Transform prop = room != null ? FindDeep(room, s.prop) : null;
                Transform model = prop != null ? prop.Find("Model") : null;
                if (model == null) continue;

                Face face = Face.S;
                int pieces = 1;
                foreach (Furniture f in Furnitures)
                    if (f.room == s.room && f.prop == s.prop) { face = f.face; pieces = f.pieces != null ? f.pieces.Length : 1; }

                Vector3 b = prop.lossyScale;
                bool alongX = b.x >= b.z;
                Vector3 axis = alongX ? Vector3.right : Vector3.forward;
                float length = alongX ? b.x : b.z;
                float depth = alongX ? b.z : b.x;
                float bottom = prop.position.y - b.y / 2f;
                float top = bottom + b.y + 0.3f;

                Transform parent = model;
                if (s.target != null)
                {
                    Transform target = FindDeep(room, s.target);
                    if (target == null) continue;
                    parent = NewModelRoot(target);
                    HideBox(target);
                }
                Transform group = NewChild("ShelfItems", parent);
                int seed = 0;
                for (int i = 0; i < pieces; i++)
                {
                    float c = -length / 2f + (i + 0.5f) * length / pieces;
                    Vector3 center = prop.position + axis * c;
                    Vector3 probe = center - axis * (length / pieces) * 0.2f;
                    List<float> levels = ShelfLevels(model, probe, top, bottom);
                    for (int j = 0; j < levels.Count; j++)
                    {
                        float gap = j == 0 ? 0.4f : levels[j - 1] - levels[j];
                        float h = Mathf.Min(0.3f, gap - 0.08f);
                        if (h < 0.1f) continue;
                        Transform holder = NewChild("Shelf", group);
                        holder.position = new Vector3(center.x, levels[j], center.z);
                        holder.rotation = Quaternion.Euler(0f, KitYaw(face), 0f);
                        BuildKit(s.kind + ":" + (seed++), holder, length / pieces * 0.82f, h, depth * 0.65f);
                    }
                }
                filled++;
            }
            return filled;
        }

        static List<float> ShelfLevels(Transform model, Vector3 at, float top, float bottom)
        {
            const int Layer = 31;
            List<MeshCollider> temp = new List<MeshCollider>();
            Dictionary<GameObject, int> layers = new Dictionary<GameObject, int>();
            foreach (MeshFilter mf in model.GetComponentsInChildren<MeshFilter>())
            {
                if (mf.sharedMesh == null || layers.ContainsKey(mf.gameObject)) continue;
                layers[mf.gameObject] = mf.gameObject.layer;
                mf.gameObject.layer = Layer;
                MeshCollider mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
                temp.Add(mc);
            }
            Physics.SyncTransforms();

            List<float> levels = new List<float>();
            float y = top;
            for (int guard = 0; guard < 12 && y > bottom; guard++)
            {
                if (!Physics.Raycast(new Vector3(at.x, y, at.z), Vector3.down, out RaycastHit hit, y - bottom, 1 << Layer, QueryTriggerInteraction.Ignore)) break;
                if (hit.point.y < bottom + 0.05f) break;
                levels.Add(hit.point.y);
                y = hit.point.y - 0.02f;
            }

            foreach (MeshCollider mc in temp) Object.DestroyImmediate(mc);
            foreach (KeyValuePair<GameObject, int> kv in layers) kv.Key.layer = kv.Value;
            return levels;
        }

        static int SetupHighlights()
        {
            Material outline = AssetDatabase.LoadAssetAtPath<Material>(OutlinePath);
            Shader hull = Shader.Find("MovingIn/OutlineHull");
            if (outline != null && hull != null && outline.shader != hull)
            {
                outline.shader = hull;
                EditorUtility.SetDirty(outline);
            }

            Material mask = null;
            Shader maskShader = Shader.Find("MovingIn/OutlineMask");
            if (maskShader == null) Debug.LogWarning("[ArtPass] Shader MovingIn/OutlineMask nao encontrado. Copie OutlineMask.shader para a pasta Shaders.");
            else
            {
                EnsureMatFolder();
                string path = MatFolder + "/M_OutlineMask.mat";
                mask = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mask == null)
                {
                    mask = new Material(maskShader);
                    AssetDatabase.CreateAsset(mask, path);
                }
                mask.shader = maskShader;
                EditorUtility.SetDirty(mask);
            }

            int n = 0;
            foreach (InteractHighlight h in Object.FindObjectsOfType<InteractHighlight>(true))
            {
                SerializedObject so = new SerializedObject(h);
                SerializedProperty m = so.FindProperty("maskMaterial");
                SerializedProperty o = so.FindProperty("outlineMaterial");
                if (m != null && mask != null) m.objectReferenceValue = mask;
                if (o != null && o.objectReferenceValue == null && outline != null) o.objectReferenceValue = outline;
                so.ApplyModifiedProperties();
                n++;
            }
            return n;
        }

        static int HidePlayerLeftovers()
        {
            GameObject player = GameObject.Find("Player");
            if (player == null) return 0;
            int n = 0;
            foreach (MeshRenderer r in player.GetComponentsInChildren<MeshRenderer>(true))
            {
                if (r.transform == player.transform || !r.enabled) continue;
                bool keep = false;
                for (Transform p = r.transform; p != null && p != player.transform; p = p.parent)
                    if (p.name == "Visual" || p.name == "BlackoutLights") keep = true;
                if (keep) continue;
                Remember(r.transform, r);
                Undo.RecordObject(r, "Art Pass");
                r.enabled = false;
                n++;
            }
            return n;
        }

        static bool Paint(Transform t, Material mat, Vector2 tiling)
        {
            MeshRenderer r = t.GetComponent<MeshRenderer>();
            if (r == null || mat == null) return false;
            Remember(t, r);
            Undo.RecordObject(r, "Art Pass");
            r.sharedMaterial = mat;
            TextureTiling tt = t.GetComponent<TextureTiling>();
            if (mat.mainTexture != null)
            {
                if (tt == null) tt = Undo.AddComponent<TextureTiling>(t.gameObject);
                tt.Set(tiling);
            }
            else if (tt != null)
            {
                Undo.DestroyObjectImmediate(tt);
                r.SetPropertyBlock(null);
            }
            return true;
        }

        static void Remember(Transform t, MeshRenderer r)
        {
            if (t.GetComponent<ArtOriginal>() != null) return;
            ArtOriginal o = Undo.AddComponent<ArtOriginal>(t.gameObject);
            o.material = r.sharedMaterial;
            o.rendererWasEnabled = r.enabled;
        }

        static void BuildTrim(Transform room, Material trim)
        {
            Transform old = room.Find("Trim");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            Transform back = room.Find("Walls_Back");
            if (back == null) return;

            GameObject group = new GameObject("Trim");
            Undo.RegisterCreatedObjectUndo(group, "Art Pass");
            group.transform.SetParent(room, false);
            group.AddComponent<ArtGenerated>();

            foreach (Transform w in back)
            {
                MeshRenderer r = w.GetComponent<MeshRenderer>();
                if (!w.name.StartsWith("Wall_") || r == null || !r.enabled) continue;
                Vector3 p = w.position;
                Vector3 s = w.lossyScale;
                float floor = p.y - s.y / 2f;
                bool north = w.name.StartsWith("Wall_North");
                Vector3 pos = north ? new Vector3(p.x, floor + 0.06f, p.z - s.z / 2f - 0.02f) : new Vector3(p.x - s.x / 2f - 0.02f, floor + 0.06f, p.z);
                Vector3 size = north ? new Vector3(s.x, 0.12f, 0.04f) : new Vector3(0.04f, 0.12f, s.z);
                GameObject b = Cube(group.transform, "Baseboard_" + w.name, pos, size, trim);
                b.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        static void BuildDoorFrames(Transform room, string roomName, Material wall, Material trim)
        {
            Transform old = room.Find("DoorFrames");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            if (!Rects.TryGetValue(roomName, out RoomRect r)) return;
            float floor = FloorTop(room);

            GameObject group = null;
            foreach (DoorFrame f in DoorFrames)
            {
                if (f.room != roomName) continue;
                if (group == null)
                {
                    group = new GameObject("DoorFrames");
                    Undo.RegisterCreatedObjectUndo(group, "Art Pass");
                    group.transform.SetParent(room, false);
                    group.AddComponent<ArtGenerated>();
                }
                Vector3 c = f.north ? new Vector3(f.at, floor, r.z1) : new Vector3(r.x1, floor, f.at);
                Vector3 along = f.north ? Vector3.right : Vector3.forward;
                Vector3 thick = f.north ? Vector3.forward : Vector3.right;

                GameObject lintel = Cube(group.transform, "Lintel", c + Vector3.up * 2.65f, Size(along, 1.6f, 0.7f, thick, 0.2f), wall);
                TextureTiling tt = lintel.AddComponent<TextureTiling>();
                tt.Set(new Vector2(1.6f, 0.7f));
                Cube(group.transform, "Header", c + Vector3.up * 2.25f, Size(along, 1.6f, 0.1f, thick, 0.26f), trim);
                Cube(group.transform, "Jamb_A", c - along * 0.75f + Vector3.up * 1.1f, Size(along, 0.1f, 2.2f, thick, 0.26f), trim);
                Cube(group.transform, "Jamb_B", c + along * 0.75f + Vector3.up * 1.1f, Size(along, 0.1f, 2.2f, thick, 0.26f), trim);
                GameObject dark = Cube(group.transform, "Opening", c + thick * 0.09f + Vector3.up * 1.1f, Size(along, 1.5f, 2.2f, thick, 0.02f), Mat(null, "#3A3340"));
                dark.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }

        static Vector3 Size(Vector3 along, float a, float h, Vector3 thick, float t)
        {
            return along * a + Vector3.up * h + thick * t;
        }

        static float FloorTop(Transform room)
        {
            Transform floor = room.Find("Floor");
            return floor != null ? floor.position.y + floor.lossyScale.y / 2f : room.position.y;
        }

        static void BuildTree(Transform tree)
        {
            Transform parent = tree.parent;
            Transform old = parent.Find(tree.name + "_Art");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            MeshRenderer r = tree.GetComponent<MeshRenderer>();
            if (r == null) return;
            Remember(tree, r);
            r.enabled = false;

            float h = tree.lossyScale.y;
            float w = Mathf.Max(tree.lossyScale.x, tree.lossyScale.z);
            GameObject root = new GameObject(tree.name + "_Art");
            Undo.RegisterCreatedObjectUndo(root, "Art Pass");
            root.transform.SetParent(parent, false);
            root.transform.position = new Vector3(tree.position.x, tree.position.y - h / 2f, tree.position.z);
            root.AddComponent<ArtGenerated>();
            Vector3 o = root.transform.position;
            Cube(root.transform, "Trunk", o + Vector3.up * h * 0.22f, new Vector3(w * 0.22f, h * 0.44f, w * 0.22f), Mat(null, "#7A5A3E"));
            Cube(root.transform, "Leaves_Low", o + Vector3.up * h * 0.6f, new Vector3(w * 1.15f, h * 0.42f, w * 1.15f), Mat(null, "#6E9B5A"));
            GameObject top = Cube(root.transform, "Leaves_Top", o + Vector3.up * h * 0.92f, new Vector3(w * 0.75f, h * 0.3f, w * 0.75f), Mat(null, "#86B56E"));
            top.transform.rotation = Quaternion.Euler(0f, 20f, 0f);
        }

        static Bounds MeshBounds(GameObject go)
        {
            bool any = false;
            Bounds b = new Bounds(go.transform.position, Vector3.zero);
            foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>(true))
            {
                if (mf.sharedMesh == null || mf.name == "OutlineShape") continue;
                Bounds mb = mf.sharedMesh.bounds;
                Matrix4x4 m = mf.transform.localToWorldMatrix;
                for (int i = 0; i < 8; i++)
                {
                    Vector3 corner = mb.center + Vector3.Scale(mb.extents, new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1));
                    Vector3 w = m.MultiplyPoint3x4(corner);
                    if (!any) { b = new Bounds(w, Vector3.zero); any = true; }
                    else b.Encapsulate(w);
                }
            }
            return b;
        }

        static Vector3 NaturalSize(GameObject asset)
        {
            GameObject probe = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            probe.transform.position = Vector3.zero;
            Vector3 size = MeshBounds(probe).size;
            Object.DestroyImmediate(probe);
            return size;
        }

        static void BuildModelIndex()
        {
            _modelIndex = new Dictionary<string, string>();
            foreach (string guid in AssetDatabase.FindAssets("t:Model"))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string key = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                bool fbx = path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase);
                if (!_modelIndex.TryGetValue(key, out string existing) || (fbx && !existing.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)))
                    _modelIndex[key] = path;
            }
        }

        static GameObject LoadModel(string options)
        {
            if (string.IsNullOrEmpty(options) || _modelIndex == null) return null;
            foreach (string name in options.Split('|'))
            {
                if (!_modelIndex.TryGetValue(name.Trim().ToLowerInvariant(), out string path)) continue;
                GameObject a = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (a != null) return a;
            }
            return null;
        }

        static void EnsureMatFolder()
        {
            if (AssetDatabase.IsValidFolder(MatFolder)) return;
            if (!AssetDatabase.IsValidFolder(Root + "Materials")) AssetDatabase.CreateFolder(Root.TrimEnd('/'), "Materials");
            AssetDatabase.CreateFolder(Root + "Materials", "Art");
        }

        static Material Mat(string tex, string color)
        {
            string key = (tex ?? "Plain") + "_" + color.TrimStart('#');
            if (_mats.TryGetValue(key, out Material cached) && cached != null) return cached;
            EnsureMatFolder();
            string path = $"{MatFolder}/M_Art_{key}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(Shader.Find("Standard"));
                AssetDatabase.CreateAsset(m, path);
            }
            m.color = Hex(color);
            m.SetFloat("_Glossiness", 0.05f);
            m.mainTexture = tex != null ? LoadTexture(tex) : null;
            EditorUtility.SetDirty(m);
            _mats[key] = m;
            return m;
        }

        static Texture2D LoadTexture(string name)
        {
            string path = TexFolder + name + ".png";
            TextureImporter ti = AssetImporter.GetAtPath(path) as TextureImporter;
            if (ti == null)
            {
                Debug.LogWarning($"[ArtPass] Textura {path} nao encontrada.");
                return null;
            }
            if (ti.filterMode != FilterMode.Point || ti.mipmapEnabled || ti.wrapMode != TextureWrapMode.Repeat)
            {
                ti.textureType = TextureImporterType.Default;
                ti.filterMode = FilterMode.Point;
                ti.wrapMode = TextureWrapMode.Repeat;
                ti.mipmapEnabled = false;
                ti.textureCompression = TextureImporterCompression.Uncompressed;
                ti.SaveAndReimport();
            }
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }

        static GameObject Cube(Transform parent, string name, Vector3 worldPos, Vector3 size, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            Object.DestroyImmediate(go.GetComponent<Collider>());
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.localScale = size;
            go.GetComponent<MeshRenderer>().sharedMaterial = mat;
            return go;
        }

        static Transform NewChild(string name, Transform parent)
        {
            Transform t = new GameObject(name).transform;
            t.SetParent(parent, false);
            return t;
        }

        static Transform FindHouse()
        {
            GameObject house = GameObject.Find("House");
            if (house == null) Debug.LogError("[ArtPass] Objeto 'House' nao encontrado. Abra a cena Hub.");
            return house != null ? house.transform : null;
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

        static Color Hex(string h)
        {
            ColorUtility.TryParseHtmlString(h, out Color c);
            return c;
        }
    }
}
