using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static partial class ArtPassBuilder
    {
        struct Decor
        {
            public string room, name, models, kit, on;
            public float x, z, y, w, h, d;
            public Face face;
            public bool solid, fill;
            public float hitW, hitD;
            public Decor Hit(float w, float d) { Decor e = this; e.hitW = w; e.hitD = d; return e; }
            public int row;
            public Decor On(string support) { Decor e = this; e.on = support; return e; }
            public Decor Fill(int pieces = 1) { Decor e = this; e.fill = true; e.row = pieces; return e; }
        }

        static Decor D(string room, string name, float x, float z, float y, float w, float h, float d, Face face, bool solid, string models, string kit)
        {
            return new Decor { room = room, name = name, x = x, z = z, y = y, w = w, h = h, d = d, face = face, solid = solid, models = models, kit = kit, row = 1 };
        }

        static readonly Decor[] Decors =
        {
            D("Garage", "Workbench", 5.55f, 6.2f, 0f, 1.6f, 0.9f, 0.6f, Face.W, true, null, "Workbench"),
            D("Garage", "Pegboard", 5.87f, 6.2f, 1.15f, 1.4f, 0.8f, 0.04f, Face.W, false, null, "Pegboard"),
            D("Garage", "Tires", 0.55f, 6.75f, 0f, 0.7f, 0.75f, 0.7f, Face.Free, true, null, "Tires"),
            D("Garage", "PaintCans", 5.5f, 0.6f, 0f, 0.6f, 0.3f, 0.4f, Face.W, false, null, "PaintCans"),
            D("Garage", "OilStain", 4.2f, 5.9f, 0f, 1.0f, 0.01f, 0.7f, Face.S, false, null, "Stain"),

            D("LivingRoom", "FloorLamp", 6.45f, 7.45f, 0f, 0.5f, 1.7f, 0.5f, Face.S, true, "lampRoundFloor|lampSquareFloor", "FloorLamp"),
            D("LivingRoom", "Armchair", 6.8f, 5.9f, 0f, 0.9f, 0.9f, 0.9f, Face.E, true, "loungeChair|loungeChairRelax|chairCushion", "Armchair"),
            D("LivingRoom", "CoffeeTable", 12.15f, 6.0f, 0f, 1.0f, 0.4f, 0.55f, Face.E, true, "tableCoffee|tableCoffeeSquare", "CoffeeTable"),
            D("LivingRoom", "TVSet", 13.6f, 6.4f, 0f, 1.1f, 0.65f, 0.3f, Face.W, false, "televisionModern|televisionVintage", "TVSet").On("TV"),
            D("LivingRoom", "Plant", 13.45f, 0.55f, 0f, 0.5f, 1.1f, 0.5f, Face.Free, true, "pottedPlant|plantSmall1", "Plant"),
            D("LivingRoom", "WallArt", 10.3f, 7.88f, 1.5f, 0.9f, 0.65f, 0.04f, Face.S, false, null, "FrameA"),
            D("LivingRoom", "WallArtSmall", 7.0f, 7.88f, 1.6f, 0.5f, 0.6f, 0.04f, Face.S, false, null, "FrameB"),

            D("Kitchen", "CounterLeft", 16.55f, 7.55f, 0f, 0.7f, 0.9f, 0.7f, Face.S, true, "kitchenCabinetDrawer|kitchenCabinet", "Counter").Fill(),
            D("Kitchen", "CoffeeMachine", 16.55f, 7.65f, 0f, 0.28f, 0.38f, 0.28f, Face.S, false, "kitchenCoffeeMachine", "CoffeeMachine").On("CounterLeft"),
            D("Kitchen", "UpperCabinets", 18.2f, 7.72f, 1.6f, 2.6f, 0.7f, 0.35f, Face.S, false, "kitchenCabinetUpperDouble|kitchenCabinetUpper", "UpperCabinet").Fill(2),
            D("Kitchen", "Hood", 20.2f, 7.67f, 1.65f, 0.9f, 0.75f, 0.45f, Face.S, false, "hoodModern|hoodLarge", "Hood"),
            D("Kitchen", "EastCounter", 20.55f, 3.0f, 0f, 1.2f, 0.9f, 0.65f, Face.W, true, "kitchenCabinetDrawer|kitchenCabinet", "Counter").Fill(),
            D("Kitchen", "Microwave", 20.6f, 3.1f, 0f, 0.55f, 0.32f, 0.4f, Face.W, false, "kitchenMicrowave", "Microwave").On("EastCounter"),
            D("Kitchen", "Trash", 20.55f, 1.0f, 0f, 0.4f, 0.6f, 0.4f, Face.W, true, "trashcan", "Trash"),
            D("Kitchen", "Clock", 20.88f, 6.6f, 2.0f, 0.35f, 0.35f, 0.04f, Face.W, false, null, "Clock"),
            D("Kitchen", "SinkMat", 18.2f, 6.85f, 0f, 1.2f, 0.02f, 0.5f, Face.S, false, null, "MatKitchen"),
            D("Kitchen", "MovingBoxes", 15.0f, 0.9f, 0f, 0.9f, 0.7f, 0.9f, Face.Free, true, null, "BoxStack"),

            D("Laundry", "Dryer", 17.55f, 11.4f, 0f, 1.0f, 1.0f, 0.95f, Face.S, true, null, "Dryer"),
            D("Laundry", "WallShelf", 19.6f, 11.77f, 1.55f, 1.6f, 0.35f, 0.25f, Face.S, false, null, "WallShelf"),
            D("Laundry", "DryingRack", 14.75f, 11.0f, 0f, 1.0f, 1.0f, 0.45f, Face.E, true, null, "DryingRack"),
            D("Laundry", "WasherMat", 20.4f, 10.45f, 0f, 1.0f, 0.02f, 0.6f, Face.S, false, null, "MatKitchen"),

            D("Hall", "Runner", 8.5f, 9.5f, 0f, 1.3f, 0.02f, 0.9f, Face.E, false, "rugRectangle", "Runner").Fill(),
            D("Hall", "WallArt", 10.88f, 11.0f, 1.4f, 0.6f, 0.5f, 0.04f, Face.W, false, null, "FrameB"),
            D("Hall", "Plant", 6.45f, 8.45f, 0f, 0.45f, 0.9f, 0.45f, Face.Free, true, "plantSmall1|pottedPlant", "Plant"),

            D("Basement", "Crates", 4.75f, 3.6f, 0f, 0.9f, 0.9f, 0.9f, Face.Free, true, null, "Crates"),
            D("Basement", "Barrel", 16.4f, 1.1f, 0f, 0.6f, 0.9f, 0.6f, Face.Free, true, null, "Barrel"),
            D("Basement", "Pipes", 10.5f, 7.8f, 2.45f, 12.4f, 0.25f, 0.18f, Face.S, false, null, "Pipes"),
            D("Basement", "Puddle", 9.6f, 2.2f, 0f, 1.1f, 0.01f, 0.7f, Face.S, false, null, "Puddle"),
            D("Basement", "SheetedChair", 14.2f, 1.4f, 0f, 1.0f, 1.0f, 0.9f, Face.S, true, null, "Sheeted"),
            D("Basement", "Crates_2", 11.8f, 0.9f, 0f, 0.9f, 0.9f, 0.9f, Face.Free, true, null, "Crates"),
            D("Basement", "OldBoxes_B", 6.2f, 1.0f, 0f, 0.9f, 0.8f, 0.9f, Face.Free, true, null, "BoxStackOld"),

            D("Bedroom", "Nightstand", 7.0f, 7.55f, 0f, 0.55f, 0.55f, 0.45f, Face.S, true, "sideTableDrawers|cabinetBedDrawer|sideTable", "Nightstand"),
            D("Bedroom", "TableLamp", 7.0f, 7.6f, 0f, 0.3f, 0.45f, 0.3f, Face.S, false, "lampRoundTable|lampSquareTable", "TableLamp").On("Nightstand"),
            D("Bedroom", "Rug", 5.0f, 4.4f, 0f, 2.2f, 0.02f, 1.5f, Face.S, false, "rugRounded|rugRectangle", "RugBed").Fill(),
            D("Bedroom", "PosterRocket", 1.2f, 7.89f, 1.55f, 0.5f, 0.7f, 0.02f, Face.S, false, null, "PosterA"),
            D("Bedroom", "PosterDino", 3.0f, 7.89f, 1.7f, 0.6f, 0.45f, 0.02f, Face.S, false, null, "PosterB"),
            D("Bedroom", "Beanbag", 1.0f, 3.2f, 0f, 0.9f, 0.55f, 0.9f, Face.S, true, null, "Beanbag"),
            D("Bedroom", "MovingBoxes", 6.6f, 1.0f, 0f, 0.9f, 0.7f, 0.9f, Face.Free, true, null, "BoxStack"),
            D("Bedroom", "Toys", 3.6f, 2.4f, 0f, 0.8f, 0.2f, 0.5f, Face.S, false, null, "Toys"),

            D("UpstairsHall", "Runner", 12.0f, 4.0f, 0f, 3.0f, 0.02f, 1.1f, Face.E, false, "rugRectangle", "Runner").Fill(),
            D("UpstairsHall", "SideTable", 13.45f, 6.0f, 0f, 0.8f, 0.6f, 0.45f, Face.W, true, "sideTable|sideTableDrawers", "Nightstand"),
            D("UpstairsHall", "SmallPlant", 13.45f, 6.15f, 0f, 0.3f, 0.4f, 0.3f, Face.Free, false, "plantSmall2|plantSmall1", "Plant").On("SideTable"),
            D("UpstairsHall", "Plant", 10.5f, 7.45f, 0f, 0.45f, 0.9f, 0.45f, Face.Free, true, "pottedPlant|plantSmall3", "Plant"),

            D("Attic", "SheetedShape", 10.3f, 7.2f, 0f, 1.4f, 1.1f, 0.9f, Face.S, true, null, "Sheeted"),
            D("Attic", "LeaningFrames", 6.3f, 7.72f, 0f, 1.0f, 0.9f, 0.3f, Face.S, false, null, "LeaningFrames"),
            D("Attic", "OldBoxes_A", 13.4f, 6.0f, 0f, 0.8f, 0.9f, 0.8f, Face.Free, true, null, "BoxStackOld"),
            D("Attic", "OldLamp", 2.55f, 5.9f, 0f, 0.4f, 1.5f, 0.4f, Face.S, true, null, "FloorLamp"),
            D("Attic", "RockingHorse", 10.6f, 4.6f, 0f, 0.9f, 0.8f, 0.35f, Face.W, true, null, "RockingHorse"),
            D("Attic", "OldBoxes_2", 3.2f, 2.2f, 0f, 0.8f, 0.9f, 0.8f, Face.Free, true, null, "BoxStackOld"),
            D("Attic", "OldRug", 8.8f, 5.9f, 0f, 2.0f, 0.02f, 1.2f, Face.S, false, null, "Runner"),

            D("Front", "Bush_1", 6.0f, -0.5f, 0f, 1.3f, 0.7f, 0.6f, Face.S, true, null, "Bush"),
            D("Front", "Bush_2", 8.3f, -0.5f, 0f, 0.9f, 0.65f, 0.6f, Face.S, true, null, "Bush"),
            D("Front", "Bush_3", 11.7f, -0.5f, 0f, 0.9f, 0.65f, 0.6f, Face.S, true, null, "Bush"),
            D("Front", "Bush_4", 16.0f, -0.5f, 0f, 1.3f, 0.7f, 0.6f, Face.S, true, null, "Bush"),
            D("Front", "Bush_5", 19.0f, -0.5f, 0f, 1.3f, 0.7f, 0.6f, Face.S, true, null, "Bush"),
            D("Front", "StreetLamp_1", 5.0f, -10.6f, 0f, 0.3f, 3.2f, 0.3f, Face.S, false, null, "StreetLamp"),
            D("Front", "StreetLamp_2", 21.0f, -10.6f, 0f, 0.3f, 3.2f, 0.3f, Face.S, false, null, "StreetLamp"),
            D("Front", "SwingTree", 14.8f, -5.6f, 0f, 3.2f, 3.4f, 1.8f, Face.S, true, null, "SwingTree").Hit(0.45f, 0.45f),
            D("Front", "Pond", 19.3f, -6.6f, 0f, 3.2f, 0.3f, 2.0f, Face.S, true, null, "Pond"),
            D("Front", "Bench", 19.3f, -4.4f, 0f, 1.5f, 0.9f, 0.55f, Face.S, true, null, "Bench"),
            D("Front", "Tree_3", 22.6f, -8.1f, 0f, 1.6f, 3.0f, 1.6f, Face.S, true, null, "TreeBlock").Hit(0.35f, 0.35f),
            D("Front", "Tree_4", 3.4f, -6.8f, 0f, 1.5f, 2.8f, 1.5f, Face.S, true, null, "TreeBlock").Hit(0.35f, 0.35f),
            D("Front", "Tree_5", -1.0f, -2.2f, 0f, 1.3f, 2.6f, 1.3f, Face.S, true, null, "TreeBlock").Hit(0.3f, 0.3f),
            D("Front", "DoorMat", 10.0f, -0.95f, 0.03f, 1.0f, 0.02f, 0.45f, Face.S, false, null, "MatDoor"),
        };

        static int BuildDecor(Transform house)
        {
            Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
            int built = 0;
            foreach (Decor e in Decors)
            {
                Transform room = house.Find(e.room);
                if (room == null) continue;
                if (!groups.TryGetValue(e.room, out Transform group))
                {
                    Transform old = room.Find("Decor");
                    if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
                    GameObject g = new GameObject("Decor");
                    Undo.RegisterCreatedObjectUndo(g, "Art Pass");
                    g.transform.SetParent(room, false);
                    g.AddComponent<ArtGenerated>();
                    groups[e.room] = group = g.transform;
                }

                float y = FloorTop(room) + e.y;
                if (e.on != null)
                {
                    Transform support = group.Find(e.on);
                    if (support == null) support = FindDeep(room, e.on);
                    if (support != null) y = TopOf(support);
                }

                GameObject holder = new GameObject(e.name);
                holder.transform.SetParent(group, false);
                holder.transform.position = new Vector3(e.x, y, e.z);
                bool turned = e.face == Face.W || e.face == Face.E;
                Vector3 world = turned ? new Vector3(e.d, e.h, e.w) : new Vector3(e.w, e.h, e.d);

                if (!PlaceDecorModel(holder.transform, e, world, turned) && !PlaceDecorKit(holder.transform, e))
                {
                    Object.DestroyImmediate(holder);
                    continue;
                }
                if (e.solid)
                {
                    BoxCollider bc = holder.AddComponent<BoxCollider>();
                    bc.center = Vector3.up * e.h / 2f;
                    bc.size = e.hitW > 0f ? new Vector3(e.hitW, e.h, e.hitD) : world;
                }
                built++;
            }
            return built;
        }

        static bool PlaceDecorModel(Transform holder, Decor e, Vector3 world, bool turned)
        {
            GameObject asset = LoadModel(e.models);
            if (asset == null) return false;
            int n = Mathf.Max(1, e.row);
            float length = turned ? world.z : world.x;
            for (int i = 0; i < n; i++)
            {
                float c = -length / 2f + (i + 0.5f) * length / n;
                Vector3 center = turned ? new Vector3(0f, 0f, c) : new Vector3(c, 0f, 0f);
                Vector3 size = turned ? new Vector3(world.x, world.y, length / n) : new Vector3(length / n, world.y, world.z);
                float yaw = e.face == Face.Free ? FreeYaw(asset, size) : YawFor(e.face);
                PlaceAsset(holder, asset, center, size, yaw, e.fill, 1f);
            }
            return true;
        }

        static bool PlaceDecorKit(Transform holder, Decor e)
        {
            if (e.kit == null) return false;
            Transform pivot = NewChild("Kit", holder);
            pivot.localRotation = Quaternion.Euler(0f, KitYaw(e.face), 0f);
            return BuildKit(e.kit, pivot, e.w, e.h, e.d);
        }

        static float TopOf(Transform support)
        {
            Transform model = support.Find("Model");
            Bounds b = MeshBounds((model != null ? model : support).gameObject);
            return b.max.y;
        }
    }
}
