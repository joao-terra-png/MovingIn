using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static partial class ArtPassBuilder
    {
        const string WoodL = "#C9A27A", WoodM = "#A87B55", WoodD = "#7A5A3E", White = "#F4F1EA";
        const string Card = "#C9A26B", CardD = "#A9824E", CardOld = "#9C8460", Tape = "#E6D3A3";
        const string Metal = "#9AA3AB", MetalD = "#5E666E", MetalL = "#C8CDD2", Black = "#2B2230", Hole = "#241E2A";
        const string Glass = "#BFD9EA", Red = "#C2453A", Yellow = "#E8C15A", Leaf = "#6E9B5A", LeafL = "#86B56E";
        const string Pot = "#B8674A", Blue = "#6F8FC2", Sheet = "#E9E4D8", Pink = "#E8B4B8", Copper = "#B8674A";

        sealed class Kit
        {
            readonly Transform _root;
            public Kit(Transform root) { _root = root; }

            public Transform Box(float x, float y, float z, float sx, float sy, float sz, string c, float rx = 0f, float ry = 0f, float rz = 0f)
            {
                return Prim(PrimitiveType.Cube, new Vector3(x, y, z), new Vector3(sx, sy, sz), c, new Vector3(rx, ry, rz));
            }

            public Transform Cyl(float x, float y, float z, float dx, float h, float dz, string c, float rx = 0f, float ry = 0f, float rz = 0f)
            {
                return Prim(PrimitiveType.Cylinder, new Vector3(x, y, z), new Vector3(dx, h / 2f, dz), c, new Vector3(rx, ry, rz));
            }

            public Transform Ball(float x, float y, float z, float sx, float sy, float sz, string c)
            {
                return Prim(PrimitiveType.Sphere, new Vector3(x, y, z), new Vector3(sx, sy, sz), c, Vector3.zero);
            }

            public void Bar(float x0, float y0, float x1, float y1, float z, float t, string c)
            {
                float dx = x1 - x0, dy = y1 - y0;
                float len = Mathf.Sqrt(dx * dx + dy * dy);
                Box((x0 + x1) / 2f, (y0 + y1) / 2f, z, t, len, t, c, 0f, 0f, Mathf.Atan2(dy, dx) * Mathf.Rad2Deg - 90f);
            }

            public void BarYZ(float z0, float y0, float z1, float y1, float x, float t, string c)
            {
                float dz = z1 - z0, dy = y1 - y0;
                float len = Mathf.Sqrt(dz * dz + dy * dy);
                Box(x, (y0 + y1) / 2f, (z0 + z1) / 2f, t, len, t, c, Mathf.Atan2(dz, dy) * Mathf.Rad2Deg);
            }

            public Kit Group(float x, float y, float z, float ry = 0f, float rx = 0f)
            {
                Transform g = new GameObject("Group").transform;
                g.SetParent(_root, false);
                g.localPosition = new Vector3(x, y, z);
                g.localRotation = Quaternion.Euler(rx, ry, 0f);
                return new Kit(g);
            }

            public void Shape(PrimitiveType type, float x, float y, float z, float sx, float sy, float sz)
            {
                GameObject go = GameObject.CreatePrimitive(type);
                go.name = "OutlineShape";
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(_root, false);
                go.transform.localPosition = new Vector3(x, y, z);
                go.transform.localScale = new Vector3(sx, type == PrimitiveType.Cylinder ? sy / 2f : sy, sz);
                go.GetComponent<MeshRenderer>().enabled = false;
            }

            Transform Prim(PrimitiveType type, Vector3 p, Vector3 s, string c, Vector3 r)
            {
                GameObject go = GameObject.CreatePrimitive(type);
                go.name = type.ToString();
                Object.DestroyImmediate(go.GetComponent<Collider>());
                go.transform.SetParent(_root, false);
                go.transform.localPosition = p;
                go.transform.localRotation = Quaternion.Euler(r);
                go.transform.localScale = s;
                go.GetComponent<MeshRenderer>().sharedMaterial = Mat(null, c);
                return go.transform;
            }
        }

        static bool BuildKit(string kind, Transform pivot, float w, float h, float d)
        {
            Kit k = new Kit(pivot);
            int seed = 0;
            int colon = kind.IndexOf(':');
            if (colon > 0)
            {
                int.TryParse(kind.Substring(colon + 1), out seed);
                kind = kind.Substring(0, colon);
            }
            switch (kind)
            {
                case "Books": Books(k, w, h, d, seed); break;
                case "Jars": Jars(k, w, h, d, seed); break;
                case "HangingBulb": HangingBulb(k, h); break;
                case "RugLiving": Rug(k, w, d, "#4E6E9E", "#DCE3EA"); break;
                case "ShelfBoxes": ShelfBoxes(k, w, h, d, seed); break;
                case "Dryer": Dryer(k, w, h, d); break;
                case "Chair": Chair(k, w, h, d); break;
                case "Car": Car(k, w, d); break;
                case "TreeBlock": TreeBlock(k, w, h, d); break;
                case "SwingTree": SwingTree(k, w, h); break;
                case "Pond": Pond(k, w, d); break;
                case "Bench": Bench(k, w, d); break;
                case "StairsUp": StairsUp(k, w, h, d); break;
                case "StairsDown": StairsDown(k, w, d); break;
                case "Ladder": Ladder(k, w, h, d); break;
                case "Hatch": Hatch(k, w, d); break;
                case "DoorInterior": Door(k, w, h, d, WoodL, false); break;
                case "DoorFront": Door(k, w, h, d, "#C8643C", true); break;
                case "GarageDoor": GarageDoor(k, w, h, d); break;
                case "WindowExterior": WindowExterior(k, w, h, d); break;
                case "WindowInterior": WindowInterior(k, w, h); break;
                case "RoundWindow": RoundWindow(k, h, d); break;
                case "Painting": Painting(k, w, h, d); break;
                case "FrameA": Frame(k, w, h, WoodD, "#DCEBE4", "#8FB3A6", "#6E9B5A"); break;
                case "FrameB": Frame(k, w, h, "#2B2230", "#F2E6D0", "#C56A4E", "#E8C15A"); break;
                case "PosterA": PosterRocket(k, w, h); break;
                case "PosterB": PosterDino(k, w, h); break;
                case "HamsterCage": HamsterCage(k, w, h, d); break;
                case "LaundryBasket": LaundryBasket(k, w, h); break;
                case "BreakerPanel": BreakerPanel(k, w, h); break;
                case "Boiler": Boiler(k); break;
                case "Bicycle": Bicycle(k); break;
                case "BoxPile": BoxPile(k, w, h, d); break;
                case "BoxRow": BoxRow(k, w, h, d); break;
                case "BoxStack": BoxStack(k, w, h, d, Card, CardD, Tape); break;
                case "BoxStackOld": BoxStack(k, w, h, d, CardOld, "#8A7352", "#CFC2A0"); break;
                case "Plates": Plates(k); break;
                case "Pans": Pans(k); break;
                case "ClothesPile": ClothesPile(k); break;
                case "Clothesline": Clothesline(k, w); break;
                case "LoosePipes": LoosePipes(k); break;
                case "AssembledTubes": AssembledTubes(k); break;
                case "Photos": Photos(k); break;
                case "FamilyTree": FamilyTree(k, w, h); break;
                case "Chest": Chest(k, w, d); break;
                case "Blanket": Blanket(k, w, d); break;
                case "Mailbox": Mailbox(k); break;
                case "Fence": Fence(k, w); break;
                case "TruckBox": TruckBox(k, w, h, d); break;
                case "TruckCab": TruckCab(k, w, d); break;
                case "Workbench": Workbench(k, w, h, d); break;
                case "Pegboard": Pegboard(k, w, h); break;
                case "Tires": Tires(k); break;
                case "PaintCans": PaintCans(k); break;
                case "Stain": Flat(k, w, d, "#5E5A55", "#4E4A46"); break;
                case "Puddle": Flat(k, w, d, "#7C93A6", "#93AABB"); break;
                case "MatKitchen": Rug(k, w, d, "#8FB3A6", "#DCEBE4"); break;
                case "MatDoor": Rug(k, w, d, "#7A5040", "#A8784E"); break;
                case "RugBed": Rug(k, w, d, "#E8B4B8", "#F4F1EA"); break;
                case "Runner": Rug(k, w, d, "#A8574A", "#C9A27A"); break;
                case "FloorLamp": FloorLamp(k, h); break;
                case "TableLamp": TableLamp(k, h); break;
                case "Plant": Plant(k, w, h); break;
                case "CoffeeTable": Table(k, w, h, d, true); break;
                case "Nightstand": Nightstand(k, w, h, d); break;
                case "Armchair": Armchair(k, w, h, d); break;
                case "TVSet": TVSet(k, w, h); break;
                case "Counter": Counter(k, w, h, d); break;
                case "UpperCabinet": UpperCabinet(k, w, h, d); break;
                case "Hood": Hood(k, w, h, d); break;
                case "Microwave": Microwave(k, w, h, d); break;
                case "CoffeeMachine": CoffeeMachine(k, w, h, d); break;
                case "Trash": Trash(k, w, h); break;
                case "Clock": Clock(k, w); break;
                case "Washer": Washer(k, w, h, d); break;
                case "WallShelf": WallShelf(k, w, d); break;
                case "DryingRack": DryingRack(k, w, h); break;
                case "Crates": Crates(k); break;
                case "Barrel": Barrel(k, w, h); break;
                case "Pipes": Pipes(k, w); break;
                case "Sheeted": Sheeted(k, w, h, d); break;
                case "Beanbag": Beanbag(k, w, h, d); break;
                case "LeaningFrames": LeaningFrames(k); break;
                case "RockingHorse": RockingHorse(k); break;
                case "Bush": Bush(k, w, h, d); break;
                case "StreetLamp": StreetLamp(k, h); break;
                case "Flowers": Flowers(k, w, d); break;
                case "Toys": Toys(k); break;
                default:
                    Debug.LogWarning($"[ArtPass] Improviso '{kind}' nao existe.");
                    return false;
            }
            return true;
        }

        static void StairsUp(Kit k, float w, float h, float d)
        {
            const int n = 6;
            float sd = d / n;
            for (int i = 0; i < n; i++)
            {
                float top = h * (i + 1) / n;
                float z = -d / 2f + (i + 0.5f) * sd;
                k.Box(0f, top / 2f, z, w - 0.12f, top, sd, WoodM);
                k.Box(0f, top + 0.01f, z - 0.01f, w - 0.1f, 0.02f, sd + 0.02f, WoodL);
            }
            k.Box(w / 2f - 0.03f, h / 2f, 0f, 0.06f, h, d, WoodD);
            k.Box(-w / 2f + 0.03f, h / 2f, 0f, 0.06f, h, d, WoodD);

            float x = w / 2f - 0.03f;
            float z0 = -d / 2f + 0.05f, z1 = d / 2f - 0.05f;
            float y0 = h / n + 0.9f, y1 = h + 0.9f;
            k.Box(x, y0 / 2f, z0, 0.07f, y0, 0.07f, WoodD);
            k.Box(x, y1 / 2f, z1, 0.07f, y1, 0.07f, WoodD);
            k.BarYZ(z0, y0, z1, y1, x, 0.06f, WoodD);
            for (int i = 1; i < n - 1; i++)
            {
                float z = -d / 2f + (i + 0.5f) * sd;
                float step = h * (i + 1) / n;
                float rail = Mathf.Lerp(y0, y1, (z - z0) / (z1 - z0));
                k.Box(x, (step + rail) / 2f, z, 0.03f, rail - step, 0.03f, White);
            }
            if (h < 2.4f) k.Box(0f, h + (2.6f - h) / 2f, d / 2f - 0.02f, w - 0.12f, 2.6f - h, 0.03f, "#3A3340");
        }

        static void StairsDown(Kit k, float w, float d)
        {
            k.Box(0f, 0.006f, 0f, w - 0.16f, 0.012f, d - 0.16f, Hole);
            const int n = 5;
            float sd = (d - 0.16f) / n;
            for (int i = 0; i < n; i++)
            {
                Color c = Color.Lerp(Hex("#9C7A58"), Hex(Hole), i / (float)n);
                k.Box(0f, 0.014f, -d / 2f + 0.08f + (i + 0.5f) * sd, w - 0.22f, 0.004f, sd * 0.55f, "#" + ColorUtility.ToHtmlStringRGB(c));
            }
            k.Box(0f, 0.02f, -d / 2f + 0.04f, w, 0.04f, 0.08f, WoodD);
            k.Box(0f, 0.02f, d / 2f - 0.04f, w, 0.04f, 0.08f, WoodD);
            k.Box(-w / 2f + 0.04f, 0.02f, 0f, 0.08f, 0.04f, d, WoodD);
            k.Box(w / 2f - 0.04f, 0.02f, 0f, 0.08f, 0.04f, d, WoodD);

            float px = w / 2f - 0.04f, pz = d / 2f - 0.04f;
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    k.Box(sx * px, 0.45f, sz * pz, 0.07f, 0.9f, 0.07f, WoodD);
            foreach (float sx in new[] { -1f, 1f })
            {
                k.Box(sx * px, 0.9f, 0f, 0.06f, 0.05f, d, WoodD);
                k.Box(sx * px, 0.45f, 0f, 0.04f, 0.04f, d, WoodL);
            }
            k.Box(0f, 0.9f, pz, w, 0.05f, 0.06f, WoodD);
            k.Box(0f, 0.45f, pz, w, 0.04f, 0.04f, WoodL);
        }

        static void Ladder(Kit k, float w, float h, float d)
        {
            k.Box(0f, h - 0.3f, d / 2f - 0.005f, w + 0.15f, 0.55f, 0.01f, "#3A3340");
            float rx = w / 2f - 0.08f;
            k.Box(-rx, h / 2f, -0.01f, 0.06f, h, 0.06f, WoodM);
            k.Box(rx, h / 2f, -0.01f, 0.06f, h, 0.06f, WoodM);
            for (float y = 0.3f; y < h - 0.1f; y += 0.3f)
                k.Box(0f, y, -0.01f, rx * 2f, 0.04f, 0.045f, WoodL);
        }

        static void Hatch(Kit k, float w, float d)
        {
            k.Box(0f, 0.006f, 0f, w - 0.2f, 0.012f, d - 0.2f, Hole);
            k.Box(0f, 0.02f, -d / 2f + 0.05f, w, 0.04f, 0.1f, WoodD);
            k.Box(0f, 0.02f, d / 2f - 0.05f, w, 0.04f, 0.1f, WoodD);
            k.Box(-w / 2f + 0.05f, 0.02f, 0f, 0.1f, 0.04f, d, WoodD);
            k.Box(w / 2f - 0.05f, 0.02f, 0f, 0.1f, 0.04f, d, WoodD);
            k.Box(-0.22f, 0.3f, 0.12f, 0.05f, 0.6f, 0.05f, WoodM);
            k.Box(0.22f, 0.3f, 0.12f, 0.05f, 0.6f, 0.05f, WoodM);
            k.Box(0f, 0.22f, 0.12f, 0.44f, 0.04f, 0.04f, WoodL);
            k.Box(0f, 0.48f, 0.12f, 0.44f, 0.04f, 0.04f, WoodL);
            k.Box(w / 2f + 0.42f, 0.02f, 0f, 0.8f, 0.04f, d - 0.2f, WoodL);
            k.Box(w / 2f + 0.42f, 0.042f, -0.25f, 0.8f, 0.005f, 0.04f, WoodM);
            k.Box(w / 2f + 0.42f, 0.042f, 0.25f, 0.8f, 0.005f, 0.04f, WoodM);
        }

        static void Door(Kit k, float w, float h, float d, string panel, bool front)
        {
            float f = d / 2f;
            k.Box(0f, (h - 0.07f) / 2f, 0f, w - 0.14f, h - 0.07f, d * 0.6f, panel);
            k.Box(-w / 2f + 0.035f, h / 2f, -0.01f, 0.07f, h, d + 0.04f, White);
            k.Box(w / 2f - 0.035f, h / 2f, -0.01f, 0.07f, h, d + 0.04f, White);
            k.Box(0f, h - 0.035f, -0.01f, w, 0.07f, d + 0.04f, White);
            float z = -d * 0.3f - 0.006f;
            string inset = front ? "#B05634" : WoodM;
            if (front)
            {
                k.Box(0f, h * 0.76f, z, w - 0.45f, h * 0.2f, 0.012f, Glass);
                k.Box(0f, h * 0.76f, z - 0.004f, 0.03f, h * 0.2f, 0.01f, White);
                k.Box(0f, h * 0.3f, z, w - 0.45f, h * 0.34f, 0.012f, inset);
                k.Box(0f, h + 0.13f, -f, 0.26f, 0.12f, 0.02f, Tape);
                k.Box(0f, 0.04f, -f - 0.3f, w + 0.3f, 0.08f, 0.5f, "#B9B2A6");
            }
            else
            {
                k.Box(0f, h * 0.68f, z, w - 0.36f, h * 0.34f, 0.012f, inset);
                k.Box(0f, h * 0.27f, z, w - 0.36f, h * 0.3f, 0.012f, inset);
                k.Box(0f, h * 0.88f, z - 0.006f, 0.2f, 0.1f, 0.01f, Blue);
            }
            k.Box(w / 2f - 0.17f, h * 0.48f, z - 0.03f, 0.06f, 0.06f, 0.06f, Yellow);
        }

        static void GarageDoor(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0.01f, w - 0.2f, h - 0.1f, d * 0.5f, "#4E5B66");
            k.Box(-w / 2f + 0.05f, h / 2f, -0.01f, 0.1f, h, d + 0.04f, White);
            k.Box(w / 2f - 0.05f, h / 2f, -0.01f, 0.1f, h, d + 0.04f, White);
            k.Box(0f, h - 0.05f, -0.01f, w, 0.1f, d + 0.04f, White);
            const int n = 5;
            float sh = (h - 0.1f) / n;
            for (int i = 0; i < n; i++)
                k.Box(0f, (i + 0.5f) * sh, -d * 0.25f, w - 0.2f, sh * 0.9f, 0.03f, i % 2 == 0 ? "#7D8E9E" : "#6E7F8F");
            for (int i = 0; i < 4; i++)
                k.Box(-w * 0.3f + i * w * 0.2f, (n - 0.5f) * sh, -d * 0.25f - 0.02f, w * 0.13f, sh * 0.55f, 0.01f, Glass);
            k.Box(0f, 0.45f, -d / 2f - 0.02f, 0.4f, 0.05f, 0.04f, MetalD);
        }

        static void WindowExterior(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w - 0.12f, h - 0.12f, d * 0.4f, "#A9CBE0");
            k.Box(-0.15f, h * 0.6f, -d * 0.2f - 0.003f, 0.08f, h * 0.55f, 0.005f, "#D6E7F1", 0f, 0f, -35f);
            WindowFrame(k, w, h, d);
            k.Box(0f, h / 2f, -d * 0.25f, 0.05f, h - 0.1f, 0.03f, White);
            k.Box(0f, h / 2f, -d * 0.25f, w - 0.1f, 0.05f, 0.03f, White);
            k.Box(0f, -0.03f, -0.08f, w + 0.16f, 0.06f, 0.2f, White);
            k.Box(-w / 2f - 0.21f, h / 2f, -0.02f, 0.36f, h + 0.04f, 0.04f, Blue);
            k.Box(w / 2f + 0.21f, h / 2f, -0.02f, 0.36f, h + 0.04f, 0.04f, Blue);
            for (int i = 0; i < 4; i++)
            {
                float y = h * (0.2f + i * 0.2f);
                k.Box(-w / 2f - 0.21f, y, -0.045f, 0.3f, 0.025f, 0.01f, "#5E7CB0");
                k.Box(w / 2f + 0.21f, y, -0.045f, 0.3f, 0.025f, 0.01f, "#5E7CB0");
            }
            k.Box(0f, -0.15f, -0.15f, w, 0.18f, 0.2f, WoodM);
            string[] blossoms = { "#E58FA6", "#F2D16B", "#F4F1EA", "#E58FA6", "#F2D16B" };
            for (int i = 0; i < 5; i++)
            {
                float x = -w / 2f + 0.15f + i * (w - 0.3f) / 4f;
                k.Ball(x, -0.02f, -0.15f, 0.2f, 0.12f, 0.16f, Leaf);
                k.Ball(x + 0.03f, 0.05f, -0.17f, 0.09f, 0.09f, 0.09f, blossoms[i]);
            }
        }

        static void WindowFrame(Kit k, float w, float h, float d)
        {
            k.Box(0f, h - 0.04f, -0.01f, w, 0.08f, d + 0.04f, White);
            k.Box(0f, 0.04f, -0.01f, w, 0.08f, d + 0.04f, White);
            k.Box(-w / 2f + 0.04f, h / 2f, -0.01f, 0.08f, h, d + 0.04f, White);
            k.Box(w / 2f - 0.04f, h / 2f, -0.01f, 0.08f, h, d + 0.04f, White);
        }

        static void WindowInterior(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0f, w - 0.12f, h - 0.12f, 0.02f, Glass);
            k.Box(-0.2f, h * 0.55f, -0.012f, 0.1f, h * 0.6f, 0.005f, "#E3F0F7", 0f, 0f, -35f);
            WindowFrame(k, w, h, 0.05f);
            k.Box(0f, h / 2f, -0.03f, 0.05f, h - 0.1f, 0.03f, White);
            k.Box(0f, h / 2f, -0.03f, w - 0.1f, 0.05f, 0.03f, White);
            k.Box(0f, -0.03f, -0.08f, w + 0.12f, 0.05f, 0.18f, White);
            k.Box(-w / 2f - 0.1f, h / 2f - 0.05f, -0.1f, 0.3f, h + 0.35f, 0.05f, "#F2D16B");
            k.Box(w / 2f + 0.1f, h / 2f - 0.05f, -0.1f, 0.3f, h + 0.35f, 0.05f, "#F2D16B");
            k.Box(0f, h + 0.15f, -0.1f, w + 0.7f, 0.03f, 0.03f, WoodD);
        }

        static void RoundWindow(Kit k, float h, float d)
        {
            float cy = h / 2f;
            k.Cyl(0f, cy, 0f, 0.86f, d * 0.4f, 0.86f, Glass, 90f);
            for (int i = 0; i < 8; i++)
            {
                float a = i * 45f * Mathf.Deg2Rad;
                k.Box(Mathf.Cos(a) * 0.45f, cy + Mathf.Sin(a) * 0.45f, -0.005f, 0.4f, 0.08f, d + 0.03f, WoodM, 0f, 0f, i * 45f + 90f);
            }
            k.Box(0f, cy, -0.02f, 0.04f, 0.86f, 0.02f, WoodM);
            k.Box(0f, cy, -0.02f, 0.86f, 0.04f, 0.02f, WoodM);
        }

        static void Painting(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w - 0.08f, h - 0.08f, 0.02f, "#A9CBE0");
            k.Box(0f, 0.14f, -0.012f, w - 0.08f, 0.2f, 0.005f, LeafL);
            k.Box(w * 0.25f, h * 0.72f, -0.014f, 0.1f, 0.1f, 0.005f, Yellow);
            k.Box(-0.05f, 0.27f, -0.016f, 0.16f, 0.09f, 0.005f, "#F2D16B");
            k.Box(0.05f, 0.34f, -0.016f, 0.07f, 0.07f, 0.005f, "#F2D16B");
            k.Box(0.1f, 0.335f, -0.017f, 0.04f, 0.025f, 0.005f, "#E08A3C");
            k.Box(0.06f, 0.355f, -0.019f, 0.015f, 0.015f, 0.003f, Black);
            FrameBars(k, w, h, d, WoodD);
        }

        static void FrameBars(Kit k, float w, float h, float d, string c)
        {
            k.Box(0f, h - 0.02f, -0.008f, w, 0.04f, d + 0.016f, c);
            k.Box(0f, 0.02f, -0.008f, w, 0.04f, d + 0.016f, c);
            k.Box(-w / 2f + 0.02f, h / 2f, -0.008f, 0.04f, h, d + 0.016f, c);
            k.Box(w / 2f - 0.02f, h / 2f, -0.008f, 0.04f, h, d + 0.016f, c);
        }

        static void Frame(Kit k, float w, float h, string frame, string canvas, string a, string b)
        {
            k.Box(0f, h / 2f, 0f, w - 0.06f, h - 0.06f, 0.015f, canvas);
            k.Box(-w * 0.12f, h * 0.38f, -0.01f, w * 0.38f, w * 0.38f, 0.004f, a, 0f, 0f, 45f);
            k.Box(w * 0.14f, h * 0.34f, -0.012f, w * 0.3f, w * 0.3f, 0.004f, b, 0f, 0f, 45f);
            k.Box(0f, h * 0.2f, -0.014f, w - 0.06f, h * 0.12f, 0.004f, a);
            FrameBars(k, w, h, 0.03f, frame);
        }

        static void PosterRocket(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0f, w, h, 0.008f, "#2E3A5C", 0f, 0f, 2f);
            k.Box(0f, h * 0.48f, -0.006f, w * 0.18f, h * 0.42f, 0.004f, White, 0f, 0f, 2f);
            k.Box(0f, h * 0.72f, -0.007f, w * 0.13f, w * 0.13f, 0.004f, Red, 0f, 0f, 47f);
            k.Box(-w * 0.12f, h * 0.3f, -0.007f, w * 0.1f, h * 0.12f, 0.004f, Red, 0f, 0f, 2f);
            k.Box(w * 0.12f, h * 0.3f, -0.007f, w * 0.1f, h * 0.12f, 0.004f, Red, 0f, 0f, 2f);
            k.Box(0f, h * 0.2f, -0.007f, w * 0.1f, h * 0.1f, 0.004f, "#F2B04B", 0f, 0f, 47f);
            float[][] stars = { new[] { -0.3f, 0.85f }, new[] { 0.32f, 0.7f }, new[] { -0.25f, 0.55f }, new[] { 0.28f, 0.9f }, new[] { 0.3f, 0.3f } };
            foreach (float[] s in stars) k.Box(w * s[0], h * s[1], -0.006f, 0.025f, 0.025f, 0.004f, Yellow, 0f, 0f, 45f);
        }

        static void PosterDino(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0f, w, h, 0.008f, "#F2D16B", 0f, 0f, -3f);
            k.Box(-w * 0.05f, h * 0.42f, -0.006f, w * 0.42f, h * 0.3f, 0.004f, Leaf, 0f, 0f, -3f);
            k.Box(w * 0.22f, h * 0.62f, -0.006f, w * 0.12f, h * 0.35f, 0.004f, Leaf, 0f, 0f, -18f);
            k.Box(w * 0.3f, h * 0.8f, -0.007f, w * 0.18f, h * 0.14f, 0.004f, Leaf, 0f, 0f, -3f);
            k.Box(-w * 0.32f, h * 0.36f, -0.006f, w * 0.22f, h * 0.08f, 0.004f, Leaf, 0f, 0f, 20f);
            k.Box(-w * 0.15f, h * 0.2f, -0.006f, w * 0.07f, h * 0.18f, 0.004f, Leaf, 0f, 0f, -3f);
            k.Box(w * 0.08f, h * 0.2f, -0.006f, w * 0.07f, h * 0.18f, 0.004f, Leaf, 0f, 0f, -3f);
            k.Box(w * 0.33f, h * 0.83f, -0.008f, 0.02f, 0.02f, 0.004f, Black);
        }

        static void HamsterCage(Kit k, float w, float h, float d)
        {
            k.Box(0f, 0.05f, 0f, w, 0.1f, d, "#5FB08A");
            k.Box(0f, 0.105f, 0f, w - 0.06f, 0.015f, d - 0.06f, Tape);
            float barH = h - 0.12f, barY = 0.1f + barH / 2f;
            for (float x = -w / 2f + 0.05f; x < w / 2f; x += 0.1f)
            {
                k.Box(x, barY, -d / 2f + 0.01f, 0.012f, barH, 0.012f, MetalL);
                k.Box(x, barY, d / 2f - 0.01f, 0.012f, barH, 0.012f, MetalL);
            }
            for (float z = -d / 2f + 0.1f; z < d / 2f - 0.05f; z += 0.1f)
            {
                k.Box(-w / 2f + 0.01f, barY, z, 0.012f, barH, 0.012f, MetalL);
                k.Box(w / 2f - 0.01f, barY, z, 0.012f, barH, 0.012f, MetalL);
            }
            k.Box(0f, h - 0.01f, -d / 2f + 0.01f, w, 0.02f, 0.02f, Metal);
            k.Box(0f, h - 0.01f, d / 2f - 0.01f, w, 0.02f, 0.02f, Metal);
            k.Box(-w / 2f + 0.01f, h - 0.01f, 0f, 0.02f, 0.02f, d, Metal);
            k.Box(w / 2f - 0.01f, h - 0.01f, 0f, 0.02f, 0.02f, d, Metal);
            for (float x = -w / 2f + 0.2f; x < w / 2f - 0.1f; x += 0.2f)
                k.Box(x, h - 0.01f, 0f, 0.012f, 0.012f, d, MetalL);

            k.Cyl(0.25f, 0.27f, 0.05f, 0.3f, 0.04f, 0.3f, "#E58FA6", 90f);
            k.Cyl(0.25f, 0.27f, 0.05f, 0.08f, 0.06f, 0.08f, White, 90f);
            k.Box(0.25f, 0.18f, 0.08f, 0.04f, 0.16f, 0.04f, White);
            k.Box(-0.28f, 0.18f, 0.08f, 0.22f, 0.16f, 0.2f, WoodL);
            k.Box(-0.28f, 0.275f, 0.08f, 0.26f, 0.03f, 0.24f, "#C56A4E");
            k.Box(-0.28f, 0.16f, -0.021f, 0.08f, 0.1f, 0.005f, Black);
            k.Cyl(0.4f, 0.3f, d / 2f - 0.05f, 0.06f, 0.18f, 0.06f, Glass);
            k.Cyl(0f, 0.12f, -0.12f, 0.12f, 0.03f, 0.12f, Red);
            k.Ball(0.02f, 0.15f, 0.15f, 0.12f, 0.09f, 0.1f, "#E3A35B");
            k.Ball(0.08f, 0.17f, 0.12f, 0.07f, 0.07f, 0.07f, "#E3A35B");
            k.Ball(0.1f, 0.21f, 0.12f, 0.03f, 0.03f, 0.02f, "#C98A47");
            k.Shape(PrimitiveType.Cube, 0f, h / 2f, 0f, w, h, d);
        }

        static void LaundryBasket(Kit k, float w, float h)
        {
            float r = w * 0.92f;
            k.Cyl(0f, 0.27f, 0f, r, 0.54f, r, WoodL);
            k.Cyl(0f, 0.12f, 0f, r + 0.02f, 0.04f, r + 0.02f, WoodM);
            k.Cyl(0f, 0.38f, 0f, r + 0.02f, 0.04f, r + 0.02f, WoodM);
            k.Cyl(0f, 0.55f, 0f, r + 0.05f, 0.05f, r + 0.05f, WoodM);
            k.Box(-r / 2f - 0.02f, 0.48f, 0f, 0.04f, 0.06f, 0.18f, WoodM);
            k.Box(r / 2f + 0.02f, 0.48f, 0f, 0.04f, 0.06f, 0.18f, WoodM);
            k.Ball(-0.12f, 0.58f, 0.05f, 0.35f, 0.2f, 0.3f, Blue);
            k.Ball(0.13f, 0.6f, -0.08f, 0.3f, 0.18f, 0.28f, Sheet);
            k.Ball(0.02f, 0.63f, 0.15f, 0.25f, 0.15f, 0.22f, Red);
            k.Box(0.28f, 0.5f, -0.24f, 0.08f, 0.22f, 0.05f, Blue, 0f, 0f, 20f);
            k.Shape(PrimitiveType.Cylinder, 0f, Mathf.Min(h, 0.68f) / 2f, 0f, r + 0.06f, Mathf.Min(h, 0.68f), r + 0.06f);
        }

        static void BreakerPanel(Kit k, float w, float h)
        {
            k.Box(0f, 0.55f, 0.05f, 0.8f, 1.0f, 0.2f, MetalL);
            k.Box(0f, 0.55f, -0.055f, 0.66f, 0.86f, 0.01f, "#3D434A");
            for (int i = 0; i < 5; i++)
            {
                float y = 0.25f + i * 0.15f;
                foreach (float x in new[] { -0.14f, 0.14f })
                {
                    k.Box(x, y, -0.065f, 0.16f, 0.07f, 0.02f, i % 2 == 0 ? White : "#D9D9D9");
                    k.Box(x + (i % 3 == 0 ? 0.03f : -0.03f), y, -0.08f, 0.04f, 0.04f, 0.03f, Black);
                }
            }
            k.Box(0.3f, 0.92f, -0.07f, 0.1f, 0.1f, 0.01f, Yellow, 0f, 0f, 45f);
            k.Box(0.3f, 0.92f, -0.076f, 0.02f, 0.05f, 0.005f, Black);
            k.Box(-0.3f, 0.95f, -0.07f, 0.05f, 0.05f, 0.02f, Red);
            k.Cyl(0f, 1.6f, 0.08f, 0.07f, 1.1f, 0.07f, MetalD);
            k.Box(0f, 1.06f, 0.08f, 0.14f, 0.04f, 0.14f, MetalD);
        }

        static void Boiler(Kit k)
        {
            k.Cyl(-0.25f, 0.1f, 0.05f, 0.95f, 0.2f, 0.95f, MetalD);
            k.Cyl(-0.25f, 0.95f, 0.05f, 0.9f, 1.6f, 0.9f, MetalL);
            k.Cyl(-0.25f, 1.77f, 0.05f, 0.7f, 0.06f, 0.7f, Metal);
            k.Cyl(-0.25f, 1.2f, -0.41f, 0.26f, 0.04f, 0.26f, MetalD, 90f);
            k.Cyl(-0.25f, 1.2f, -0.43f, 0.22f, 0.02f, 0.22f, White, 90f);
            k.Box(-0.23f, 1.22f, -0.445f, 0.015f, 0.08f, 0.01f, Red, 0f, 0f, -30f);
            k.Cyl(-0.25f, 1.9f, 0.05f, 0.1f, 0.3f, 0.1f, Copper);
            k.Cyl(0.18f, 2.0f, 0.05f, 0.1f, 0.86f, 0.1f, Copper, 0f, 0f, 90f);
            k.Cyl(0.6f, 1.5f, 0.05f, 0.1f, 1.0f, 0.1f, Copper);
            k.Ball(0.6f, 2.0f, 0.05f, 0.12f, 0.12f, 0.12f, Copper);
            k.Cyl(0.6f, 1.3f, -0.07f, 0.2f, 0.03f, 0.2f, Red, 90f);
            k.Box(0.55f, 0.6f, 0.1f, 0.4f, 0.5f, 0.25f, MetalL);
            k.Box(0.55f, 0.75f, -0.03f, 0.05f, 0.05f, 0.02f, "#5FB08A");
            for (int i = 0; i < 3; i++) k.Box(0.55f, 0.5f + i * 0.06f, -0.025f, 0.25f, 0.015f, 0.01f, MetalD);
        }

        static void Bicycle(Kit k)
        {
            foreach (float x in new[] { -0.5f, 0.5f })
            {
                k.Cyl(x, 0.33f, 0f, 0.66f, 0.05f, 0.66f, Black, 90f);
                k.Cyl(x, 0.33f, 0f, 0.52f, 0.055f, 0.52f, MetalL, 90f);
                k.Cyl(x, 0.33f, 0f, 0.1f, 0.08f, 0.1f, MetalD, 90f);
            }
            k.Bar(-0.5f, 0.33f, -0.05f, 0.33f, 0f, 0.035f, Red);
            k.Bar(-0.5f, 0.33f, -0.18f, 0.78f, 0f, 0.035f, Red);
            k.Bar(-0.05f, 0.33f, -0.18f, 0.78f, 0f, 0.045f, Red);
            k.Bar(-0.05f, 0.33f, 0.38f, 0.75f, 0f, 0.045f, Red);
            k.Bar(-0.18f, 0.74f, 0.38f, 0.75f, 0f, 0.04f, Red);
            k.Bar(0.38f, 0.75f, 0.5f, 0.33f, 0f, 0.035f, MetalD);
            k.Bar(0.38f, 0.75f, 0.36f, 0.86f, 0f, 0.035f, MetalD);
            k.Box(-0.2f, 0.83f, 0f, 0.2f, 0.05f, 0.09f, Black);
            k.Box(0.36f, 0.87f, 0f, 0.04f, 0.04f, 0.42f, MetalD);
            k.Box(0.36f, 0.87f, 0.2f, 0.05f, 0.05f, 0.08f, Black);
            k.Box(0.36f, 0.87f, -0.2f, 0.05f, 0.05f, 0.08f, Black);
            k.Cyl(-0.05f, 0.33f, 0f, 0.15f, 0.06f, 0.15f, MetalD, 90f);
            k.Box(-0.05f, 0.25f, -0.06f, 0.1f, 0.02f, 0.04f, Black);
        }

        static void CardBox(Kit k, float x, float y, float z, float sx, float sy, float sz, float yaw, string tone, string tape, string sticker = null)
        {
            Kit b = k.Group(x, y, z, yaw);
            b.Box(0f, sy / 2f, 0f, sx, sy, sz, tone);
            b.Box(0f, sy + 0.004f, 0f, 0.1f, 0.008f, sz + 0.006f, tape);
            b.Box(0f, sy - 0.06f, -sz / 2f - 0.003f, 0.1f, 0.12f, 0.006f, tape);
            if (sticker != null) b.Box(sx * 0.22f, sy * 0.45f, -sz / 2f - 0.004f, 0.16f, 0.12f, 0.006f, sticker);
        }

        static void BoxPile(Kit k, float w, float h, float d)
        {
            float fx = w / 1.4f, fy = h / 1.2f, fz = d / 1.4f;
            const string Mark = "#8A5CF6";
            CardBox(k, -0.35f * fx, 0f, -0.35f * fz, 0.66f * fx, 0.5f * fy, 0.66f * fz, 0f, Card, Tape, Red);
            CardBox(k, 0.35f * fx, 0f, -0.33f * fz, 0.64f * fx, 0.48f * fy, 0.62f * fz, 4f, CardD, Tape, Mark);
            CardBox(k, -0.33f * fx, 0f, 0.35f * fz, 0.62f * fx, 0.52f * fy, 0.64f * fz, -3f, CardD, Tape);
            CardBox(k, 0.34f * fx, 0f, 0.36f * fz, 0.62f * fx, 0.45f * fy, 0.6f * fz, 6f, Card, Tape);
            CardBox(k, -0.2f * fx, 0.52f * fy, -0.15f * fz, 0.6f * fx, 0.4f * fy, 0.55f * fz, 10f, Card, Tape, Mark);
            CardBox(k, 0.3f * fx, 0.48f * fy, 0.25f * fz, 0.5f * fx, 0.38f * fy, 0.5f * fz, -8f, CardD, Tape);
            CardBox(k, -0.12f * fx, 0.92f * fy, -0.05f * fz, 0.42f * fx, 0.28f * fy, 0.4f * fz, 25f, Card, Tape, Mark);
        }

        static void BoxRow(Kit k, float w, float h, float d)
        {
            const int n = 5;
            float[] heights = { 0.5f, 0.42f, 0.55f, 0.46f, 0.4f };
            for (int i = 0; i < n; i++)
            {
                float x = -w / 2f + (i + 0.5f) * w / n;
                CardBox(k, x, 0f, 0f, w / n * 0.88f, Mathf.Min(h, heights[i]), d * 0.85f, (i % 2 == 0 ? 2f : -3f), i % 2 == 0 ? Card : CardD, Tape, White);
            }
        }

        static void BoxStack(Kit k, float w, float h, float d, string a, string b, string tape)
        {
            CardBox(k, -0.2f * w, 0f, -0.17f * d, 0.5f * w, 0.52f * h, 0.5f * d, 0f, a, tape);
            CardBox(k, 0.25f * w, 0f, 0.2f * d, 0.46f * w, 0.47f * h, 0.46f * d, -12f, b, tape);
            CardBox(k, -0.12f * w, 0.52f * h, -0.12f * d, 0.42f * w, 0.45f * h, 0.4f * d, 8f, b, tape);
        }

        static void Plates(Kit k)
        {
            for (int i = 0; i < 3; i++) k.Cyl(-0.1f, 0.008f + i * 0.016f, -0.05f, 0.26f, 0.012f, 0.26f, White);
            k.Cyl(0.15f, 0.025f, 0.12f, 0.24f, 0.012f, 0.24f, White, 12f);
            k.Cyl(0.15f, 0.04f, -0.15f, 0.08f, 0.08f, 0.08f, "#8FB3A6");
            k.Box(-0.02f, 0.004f, 0.2f, 0.06f, 0.008f, 0.04f, White, 0f, 30f);
            k.Box(0.05f, 0.004f, 0.24f, 0.04f, 0.008f, 0.03f, White, 0f, -20f);
        }

        static void Pans(Kit k)
        {
            k.Cyl(-0.05f, 0.03f, 0.08f, 0.3f, 0.06f, 0.3f, MetalD);
            k.Cyl(-0.05f, 0.061f, 0.08f, 0.26f, 0.004f, 0.26f, "#3D434A");
            k.Box(0.2f, 0.05f, 0.08f, 0.2f, 0.025f, 0.04f, Black);
            k.Cyl(0.08f, 0.1f, -0.12f, 0.22f, 0.2f, 0.22f, Metal);
            k.Cyl(0.08f, 0.21f, -0.12f, 0.24f, 0.02f, 0.24f, MetalD);
            k.Ball(0.08f, 0.23f, -0.12f, 0.05f, 0.03f, 0.05f, Black);
        }

        static void ClothesPile(Kit k)
        {
            k.Ball(-0.15f, 0.04f, 0f, 0.4f, 0.08f, 0.3f, Blue);
            k.Ball(0.15f, 0.045f, 0.05f, 0.35f, 0.09f, 0.32f, Pink);
            k.Box(0.05f, 0.07f, -0.15f, 0.3f, 0.03f, 0.22f, Sheet, 0f, 25f);
            k.Box(-0.25f, 0.08f, 0.15f, 0.18f, 0.03f, 0.08f, "#F2D16B", 0f, -30f);
            k.Ball(0.3f, 0.03f, -0.2f, 0.15f, 0.06f, 0.12f, Red);
        }

        static void Clothesline(Kit k, float w)
        {
            k.Box(0f, 0.5f, -0.02f, w, 0.015f, 0.015f, White);
            k.Box(-w / 2f + 0.03f, 0.5f, 0f, 0.04f, 0.08f, 0.08f, MetalD);
            k.Box(w / 2f - 0.03f, 0.5f, 0f, 0.04f, 0.08f, 0.08f, MetalD);
            Cloth(k, -1.0f, 0.4f, 0.42f, Blue, true);
            Cloth(k, -0.4f, 0.3f, 0.55f, "#5E6E8A", false);
            Cloth(k, 0.2f, 0.4f, 0.44f, Pink, false);
            k.Box(0.2f, 0.1f, -0.036f, 0.4f, 0.05f, 0.005f, White);
            Cloth(k, 0.68f, 0.08f, 0.2f, "#F2D16B", false);
            Cloth(k, 0.8f, 0.08f, 0.2f, "#F2D16B", false);
            Cloth(k, 1.15f, 0.3f, 0.44f, "#8FB3A6", false);
        }

        static void Cloth(Kit k, float x, float w, float h, string c, bool sleeves)
        {
            k.Box(x, 0.5f - h / 2f, -0.03f, w, h, 0.02f, c);
            if (sleeves) k.Box(x, 0.44f, -0.03f, w + 0.18f, 0.1f, 0.02f, c);
            k.Box(x - w * 0.3f, 0.5f, -0.045f, 0.025f, 0.06f, 0.02f, WoodL);
            k.Box(x + w * 0.3f, 0.5f, -0.045f, 0.025f, 0.06f, 0.02f, WoodL);
        }

        static void LoosePipes(Kit k)
        {
            k.Cyl(-0.1f, 0.04f, -0.08f, 0.07f, 0.35f, 0.07f, "#7FC8E8", 0f, 0f, 90f);
            k.Cyl(0.12f, 0.04f, 0.08f, 0.07f, 0.3f, 0.07f, "#F2B04B", 90f);
            k.Cyl(0f, 0.11f, 0f, 0.07f, 0.28f, 0.07f, "#8FD18B", 0f, 35f, 90f);
            k.Ball(0.22f, 0.04f, -0.12f, 0.09f, 0.09f, 0.09f, "#7FC8E8");
            k.Ball(-0.22f, 0.04f, 0.12f, 0.09f, 0.09f, 0.09f, "#F2B04B");
        }

        static void AssembledTubes(Kit k)
        {
            k.Cyl(0f, 0.15f, 0f, 0.09f, 0.9f, 0.09f, "#7FC8E8", 0f, 0f, 90f);
            k.Ball(-0.47f, 0.15f, 0f, 0.13f, 0.13f, 0.13f, "#F2B04B");
            k.Ball(0.47f, 0.15f, 0f, 0.13f, 0.13f, 0.13f, "#F2B04B");
            k.Cyl(-0.62f, 0.15f, 0f, 0.09f, 0.3f, 0.09f, "#8FD18B", 0f, 0f, 90f);
            k.Ball(0.47f, 0.26f, 0f, 0.2f, 0.16f, 0.2f, Glass);
            k.Cyl(-0.25f, -0.05f, 0f, 0.04f, 0.4f, 0.04f, MetalL);
            k.Cyl(0.3f, -0.05f, 0f, 0.04f, 0.4f, 0.04f, MetalL);
        }

        static void Photos(Kit k)
        {
            float[][] p =
            {
                new[] { -0.35f, -0.15f, 10f }, new[] { -0.12f, 0.12f, -25f }, new[] { 0.1f, -0.1f, 40f },
                new[] { 0.32f, 0.15f, -8f }, new[] { -0.3f, 0.2f, 60f }, new[] { 0.36f, -0.2f, 20f },
            };
            string[] colors = { "#8FB3A6", "#C56A4E", "#6F8FC2", "#E8C15A", "#A87B55", "#86B56E" };
            for (int i = 0; i < p.Length; i++)
            {
                Kit g = k.Group(p[i][0], 0f, p[i][1], p[i][2]);
                g.Box(0f, 0.003f + i * 0.0005f, 0f, 0.14f, 0.006f, 0.17f, White);
                g.Box(0f, 0.0068f + i * 0.0005f, -0.015f, 0.11f, 0.002f, 0.11f, colors[i]);
            }
        }

        static void FamilyTree(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0.02f, w, h, 0.04f, "#C49A6C");
            FrameBars(k, w, h, 0.07f, WoodD);
            float[][] pos =
            {
                new[] { 0f, 1.3f }, new[] { -0.55f, 0.85f }, new[] { 0.55f, 0.85f },
                new[] { -0.9f, 0.35f }, new[] { -0.3f, 0.35f }, new[] { 0.3f, 0.35f }, new[] { 0.9f, 0.35f },
            };
            int[][] links = { new[] { 0, 1 }, new[] { 0, 2 }, new[] { 1, 3 }, new[] { 1, 4 }, new[] { 2, 5 }, new[] { 2, 6 } };
            foreach (int[] l in links)
                k.Bar(pos[l[0]][0] * w / 2.4f, pos[l[0]][1] * h / 1.6f, pos[l[1]][0] * w / 2.4f, pos[l[1]][1] * h / 1.6f, -0.012f, 0.012f, Red);
            string[] colors = { "#E8C15A", "#8FB3A6", "#C56A4E", "#6F8FC2", "#86B56E", "#A87B55", "#E8B4B8" };
            for (int i = 0; i < pos.Length; i++)
            {
                float x = pos[i][0] * w / 2.4f, y = pos[i][1] * h / 1.6f;
                k.Box(x, y, -0.015f, 0.26f, 0.3f, 0.01f, White);
                k.Box(x, y + 0.02f, -0.021f, 0.2f, 0.2f, 0.004f, colors[i]);
                k.Ball(x, y + 0.13f, -0.025f, 0.04f, 0.04f, 0.03f, Red);
            }
        }

        static void Chest(Kit k, float w, float d)
        {
            k.Box(0f, 0.27f, 0f, w - 0.04f, 0.54f, d - 0.04f, WoodM);
            k.Box(0f, 0.18f, -d / 2f + 0.015f, w - 0.02f, 0.02f, 0.01f, WoodD);
            k.Box(0f, 0.36f, -d / 2f + 0.015f, w - 0.02f, 0.02f, 0.01f, WoodD);
            k.Cyl(0f, 0.54f, 0f, 0.5f, w, d - 0.04f, WoodL, 0f, 0f, 90f).GetComponent<MeshRenderer>().receiveShadows = false;
            foreach (float x in new[] { -(w / 2f - 0.25f), w / 2f - 0.25f })
            {
                k.Box(x, 0.27f, 0f, 0.08f, 0.56f, d - 0.02f, MetalD);
                k.Cyl(x, 0.54f, 0f, 0.52f, 0.08f, d - 0.02f, MetalD, 0f, 0f, 90f).GetComponent<MeshRenderer>().receiveShadows = false;
            }
            k.Box(0f, 0.5f, -d / 2f + 0.005f, 0.14f, 0.16f, 0.04f, Yellow);
            k.Box(0f, 0.48f, -d / 2f - 0.017f, 0.03f, 0.05f, 0.01f, Black);
        }

        static void Blanket(Kit k, float w, float d)
        {
            k.Box(0f, 0.03f, -0.05f, w - 0.1f, 0.06f, d - 0.3f, Blue);
            foreach (float z in new[] { -0.45f, -0.1f, 0.25f })
                k.Box(0f, 0.062f, z * d / 1.6f, w - 0.1f, 0.005f, 0.12f, "#8FA9D6");
            k.Box(0f, 0.07f, d / 2f - 0.3f, w - 0.08f, 0.06f, 0.18f, White);
            Kit bear = k.Group(0.45f * w / 1.6f, 0.06f, 0.2f * d / 1.6f, -20f);
            bear.Ball(0f, 0.09f, 0f, 0.16f, 0.16f, 0.13f, WoodM);
            bear.Ball(0f, 0.21f, 0f, 0.12f, 0.12f, 0.11f, WoodM);
            bear.Ball(-0.05f, 0.27f, 0f, 0.04f, 0.04f, 0.03f, WoodD);
            bear.Ball(0.05f, 0.27f, 0f, 0.04f, 0.04f, 0.03f, WoodD);
            bear.Ball(0f, 0.2f, -0.05f, 0.05f, 0.04f, 0.03f, Tape);
        }

        static void Mailbox(Kit k)
        {
            k.Box(0f, 0.45f, 0f, 0.08f, 0.9f, 0.08f, WoodD);
            k.Box(0f, 0.98f, 0f, 0.24f, 0.16f, 0.4f, Red);
            k.Cyl(0f, 1.06f, 0f, 0.24f, 0.4f, 0.16f, Red, 90f);
            k.Box(0f, 1.0f, -0.205f, 0.2f, 0.18f, 0.01f, "#A8382F");
            k.Box(0.135f, 1.08f, 0.05f, 0.015f, 0.2f, 0.03f, Yellow);
            k.Box(0.135f, 1.15f, 0.1f, 0.015f, 0.07f, 0.1f, Yellow);
        }

        static void Fence(Kit k, float w)
        {
            k.Box(0f, 0.22f, 0.03f, w, 0.06f, 0.03f, White);
            k.Box(0f, 0.58f, 0.03f, w, 0.06f, 0.03f, White);
            int n = Mathf.FloorToInt(w / 0.22f);
            float sp = w / n;
            for (int i = 0; i < n; i++)
            {
                float x = -w / 2f + (i + 0.5f) * sp;
                k.Box(x, 0.36f, -0.01f, 0.1f, 0.72f, 0.03f, White);
                k.Box(x, 0.72f, -0.01f, 0.07f, 0.07f, 0.03f, White, 0f, 0f, 45f);
            }
            foreach (float x in new[] { -w / 2f + 0.05f, w / 2f - 0.05f })
            {
                k.Box(x, 0.43f, 0.03f, 0.09f, 0.86f, 0.06f, White);
                k.Box(x, 0.875f, 0.03f, 0.12f, 0.03f, 0.09f, White);
            }
        }

        static void TruckBox(Kit k, float w, float h, float d)
        {
            k.Box(0f, (h + 0.15f) / 2f, 0f, w, h - 0.15f, d, "#F2F2F2");
            k.Box(-w / 2f - 0.005f, 1.2f, 0f, 0.01f, 0.35f, d - 0.4f, "#3E6FA8");
            k.Box(w / 2f + 0.005f, 1.2f, 0f, 0.01f, 0.35f, d - 0.4f, "#3E6FA8");
            k.Box(-w / 2f - 0.012f, 1.85f, -0.6f, 0.01f, 0.5f, 0.7f, "#C56A4E");
            k.Box(-w / 2f - 0.016f, 1.75f, -0.6f, 0.01f, 0.3f, 0.4f, "#F2F2F2");
            k.Box(0f, 1.35f, d / 2f + 0.005f, w - 0.2f, h - 0.4f, 0.01f, "#E2E2E2");
            k.Box(0f, 1.3f, d / 2f + 0.012f, 0.04f, h - 0.6f, 0.01f, MetalD);
            k.Box(0f, 0.06f, 0f, w - 0.4f, 0.18f, d, Black);
            foreach (float z in new[] { 0.7f, 1.5f })
                foreach (float s in new[] { -1f, 1f })
                {
                    k.Cyl(s * (w / 2f - 0.15f), 0f, z, 0.75f, 0.25f, 0.75f, Black, 0f, 0f, 90f);
                    k.Cyl(s * (w / 2f - 0.02f), 0f, z, 0.35f, 0.02f, 0.35f, Metal, 0f, 0f, 90f);
                }
        }

        static void TruckCab(Kit k, float w, float d)
        {
            const string Body = "#3E6FA8";
            k.Box(0f, 0.55f, 0f, w, 0.9f, d, Body);
            k.Box(0f, 1.35f, 0.15f, w - 0.1f, 0.75f, d - 0.3f, Body);
            k.Box(0f, 1.38f, -0.505f, w - 0.3f, 0.55f, 0.01f, Glass);
            k.Box(-(w - 0.1f) / 2f - 0.005f, 1.4f, 0.2f, 0.01f, 0.45f, 0.7f, Glass);
            k.Box((w - 0.1f) / 2f + 0.005f, 1.4f, 0.2f, 0.01f, 0.45f, 0.7f, Glass);
            k.Box(0f, 0.55f, -d / 2f - 0.005f, w - 0.6f, 0.35f, 0.01f, "#2E3A5C");
            k.Box(-w / 2f + 0.2f, 0.6f, -d / 2f - 0.01f, 0.25f, 0.15f, 0.01f, "#F7E9B5");
            k.Box(w / 2f - 0.2f, 0.6f, -d / 2f - 0.01f, 0.25f, 0.15f, 0.01f, "#F7E9B5");
            k.Box(0f, 0.2f, -d / 2f - 0.04f, w + 0.04f, 0.15f, 0.1f, "#D9D9D9");
            k.Box(-w / 2f - 0.08f, 1.3f, -0.4f, 0.08f, 0.18f, 0.06f, Black);
            k.Box(w / 2f + 0.08f, 1.3f, -0.4f, 0.08f, 0.18f, 0.06f, Black);
            foreach (float s in new[] { -1f, 1f })
            {
                k.Cyl(s * (w / 2f - 0.15f), 0.08f, -0.1f, 0.75f, 0.25f, 0.75f, Black, 0f, 0f, 90f);
                k.Cyl(s * (w / 2f - 0.02f), 0.08f, -0.1f, 0.35f, 0.02f, 0.35f, Metal, 0f, 0f, 90f);
            }
        }

        static void Workbench(Kit k, float w, float h, float d)
        {
            k.Box(0f, h - 0.03f, 0f, w, 0.06f, d, WoodM);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    k.Box(sx * (w / 2f - 0.06f), (h - 0.06f) / 2f, sz * (d / 2f - 0.06f), 0.06f, h - 0.06f, 0.06f, WoodD);
            k.Box(0f, 0.18f, 0f, w - 0.12f, 0.03f, d - 0.12f, WoodD);
            k.Box(-w / 2f + 0.2f, h + 0.06f, -d / 2f + 0.1f, 0.15f, 0.12f, 0.15f, MetalD);
            k.Box(0.35f, h + 0.09f, 0.02f, 0.42f, 0.18f, 0.22f, Red);
            k.Box(0.35f, h + 0.2f, 0.02f, 0.2f, 0.03f, 0.03f, Black);
            k.Box(-0.05f, h + 0.015f, -0.12f, 0.3f, 0.03f, 0.05f, WoodL, 0f, 20f);
            k.Box(0.08f, h + 0.03f, -0.07f, 0.05f, 0.05f, 0.12f, MetalD, 0f, 20f);
            k.Cyl(-0.4f, 0.28f, 0f, 0.2f, 0.16f, 0.2f, Blue);
            k.Cyl(-0.15f, 0.27f, 0.05f, 0.18f, 0.14f, 0.18f, "#F2D16B");
        }

        static void Pegboard(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0f, w, h, 0.02f, Card);
            for (int i = 0; i < 6; i++)
                for (int j = 0; j < 4; j++)
                    k.Box(-w / 2f + 0.12f + i * (w - 0.24f) / 5f, 0.12f + j * (h - 0.24f) / 3f, -0.011f, 0.02f, 0.02f, 0.002f, CardD);
            k.Box(-0.48f, 0.45f, -0.02f, 0.05f, 0.32f, 0.01f, Metal, 0f, 0f, 15f);
            k.Box(-0.22f, 0.42f, -0.02f, 0.04f, 0.3f, 0.01f, WoodM);
            k.Box(-0.22f, 0.58f, -0.025f, 0.15f, 0.05f, 0.02f, MetalD);
            k.Box(0.12f, 0.42f, -0.02f, 0.34f, 0.12f, 0.01f, MetalL);
            k.Box(0.33f, 0.42f, -0.024f, 0.08f, 0.1f, 0.015f, Red);
            k.Cyl(0.52f, 0.5f, -0.03f, 0.2f, 0.04f, 0.2f, "#D8C08E", 90f);
        }

        static void Tires(Kit k)
        {
            for (int i = 0; i < 3; i++)
            {
                float o = (i % 2 == 0 ? 0.02f : -0.02f);
                k.Cyl(o, 0.11f + i * 0.23f, -o, 0.66f, 0.22f, 0.66f, Black);
            }
            k.Cyl(0.02f, 0.68f, -0.02f, 0.3f, 0.02f, 0.3f, "#141016");
        }

        static void PaintCans(Kit k)
        {
            k.Cyl(-0.15f, 0.11f, 0f, 0.2f, 0.22f, 0.2f, Blue);
            k.Cyl(-0.15f, 0.225f, 0f, 0.21f, 0.015f, 0.21f, Metal);
            k.Box(-0.15f, 0.16f, -0.101f, 0.03f, 0.08f, 0.005f, Blue);
            k.Cyl(0.12f, 0.09f, 0.05f, 0.18f, 0.18f, 0.18f, "#F2D16B");
            k.Cyl(0.12f, 0.185f, 0.05f, 0.19f, 0.015f, 0.19f, Metal);
            k.Box(0.12f, 0.02f, -0.13f, 0.18f, 0.02f, 0.04f, WoodL, 0f, 30f);
        }

        static void Flat(Kit k, float w, float d, string a, string b)
        {
            k.Cyl(0f, 0.003f, 0f, w, 0.004f, d, a);
            k.Cyl(w * 0.18f, 0.004f, -d * 0.1f, w * 0.45f, 0.004f, d * 0.5f, b);
        }

        static void Rug(Kit k, float w, float d, string border, string inner)
        {
            k.Box(0f, 0.006f, 0f, w, 0.012f, d, border);
            k.Box(0f, 0.007f, 0f, w - 0.14f, 0.014f, d - 0.14f, inner);
            k.Box(0f, 0.0075f, 0f, w - 0.34f, 0.015f, d - 0.34f, border);
        }

        static void FloorLamp(Kit k, float h)
        {
            k.Cyl(0f, 0.02f, 0f, 0.3f, 0.04f, 0.3f, MetalD);
            k.Cyl(0f, h / 2f, 0f, 0.04f, h, 0.04f, MetalD);
            k.Cyl(0f, h - 0.18f, 0f, 0.4f, 0.32f, 0.4f, "#F2E6D0");
        }

        static void TableLamp(Kit k, float h)
        {
            k.Cyl(0f, 0.03f, 0f, 0.14f, 0.06f, 0.14f, "#8FB3A6");
            k.Cyl(0f, h * 0.35f, 0f, 0.03f, h * 0.6f, 0.03f, MetalD);
            k.Cyl(0f, h * 0.75f, 0f, 0.25f, h * 0.45f, 0.25f, "#F2E6D0");
        }

        static void Plant(Kit k, float w, float h)
        {
            k.Cyl(0f, h * 0.15f, 0f, w * 0.6f, h * 0.3f, w * 0.6f, Pot);
            k.Cyl(0f, h * 0.3f, 0f, w * 0.52f, 0.01f, w * 0.52f, WoodD);
            k.Ball(0f, h * 0.55f, 0f, w * 0.9f, h * 0.45f, w * 0.9f, Leaf);
            k.Ball(w * 0.15f, h * 0.8f, -w * 0.05f, w * 0.6f, h * 0.35f, w * 0.6f, LeafL);
            k.Ball(-w * 0.15f, h * 0.75f, w * 0.1f, w * 0.55f, h * 0.3f, w * 0.55f, Leaf);
        }

        static void Table(Kit k, float w, float h, float d, bool props)
        {
            k.Box(0f, h - 0.03f, 0f, w, 0.06f, d, WoodM);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    k.Box(sx * (w / 2f - 0.05f), (h - 0.06f) / 2f, sz * (d / 2f - 0.05f), 0.05f, h - 0.06f, 0.05f, WoodD);
            if (!props) return;
            k.Box(w * 0.1f, h + 0.005f, 0f, 0.22f, 0.01f, 0.28f, Red, 0f, 15f);
            k.Cyl(-w * 0.25f, h + 0.045f, 0f, 0.07f, 0.09f, 0.07f, White);
        }

        static void Nightstand(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w, h, d, WoodM);
            k.Box(0f, h * 0.7f, -d / 2f - 0.004f, w - 0.08f, h * 0.36f, 0.008f, WoodL);
            k.Box(0f, h * 0.28f, -d / 2f - 0.004f, w - 0.08f, h * 0.36f, 0.008f, WoodL);
            k.Box(0f, h * 0.7f, -d / 2f - 0.015f, 0.06f, 0.03f, 0.02f, Yellow);
            k.Box(0f, h * 0.28f, -d / 2f - 0.015f, 0.06f, 0.03f, 0.02f, Yellow);
        }

        static void Armchair(Kit k, float w, float h, float d)
        {
            k.Box(0f, 0.22f, 0f, w, 0.3f, d, "#C56A4E");
            k.Box(0f, 0.4f, -0.05f, w - 0.2f, 0.08f, d - 0.15f, "#D9826A");
            k.Box(0f, h * 0.6f, d / 2f - 0.1f, w, h * 0.8f, 0.2f, "#C56A4E");
            k.Box(-w / 2f + 0.08f, 0.45f, 0f, 0.16f, 0.25f, d, "#B85E44");
            k.Box(w / 2f - 0.08f, 0.45f, 0f, 0.16f, 0.25f, d, "#B85E44");
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    k.Box(sx * (w / 2f - 0.08f), 0.035f, sz * (d / 2f - 0.08f), 0.05f, 0.07f, 0.05f, WoodD);
        }

        static void TVSet(Kit k, float w, float h)
        {
            k.Box(0f, 0.03f, 0f, 0.3f, 0.06f, 0.18f, Black);
            k.Box(0f, 0.09f, 0.01f, 0.06f, 0.1f, 0.04f, Black);
            k.Box(0f, 0.12f + (h - 0.12f) / 2f, 0f, w, h - 0.12f, 0.06f, Black);
            k.Box(0f, 0.12f + (h - 0.12f) / 2f, -0.031f, w - 0.06f, h - 0.18f, 0.005f, "#2E3A5C");
            k.Box(-w * 0.15f, 0.12f + (h - 0.12f) * 0.6f, -0.034f, w * 0.25f, 0.02f, 0.002f, "#4A5A84", 0f, 0f, -30f);
        }

        static void Counter(Kit k, float w, float h, float d)
        {
            k.Box(0f, (h - 0.04f) / 2f, 0.02f, w, h - 0.04f, d - 0.04f, White);
            k.Box(0f, h - 0.02f, 0f, w + 0.02f, 0.04f, d, "#C9C6C0");
            k.Box(0f, (h - 0.04f) / 2f, -d / 2f + 0.015f, 0.01f, h - 0.15f, 0.01f, "#D6D2CA");
            k.Box(-0.08f, h - 0.25f, -d / 2f + 0.005f, 0.02f, 0.1f, 0.02f, MetalD);
            k.Box(0.08f, h - 0.25f, -d / 2f + 0.005f, 0.02f, 0.1f, 0.02f, MetalD);
            k.Box(0f, 0.05f, -d / 2f + 0.03f, w, 0.1f, 0.02f, "#8E8A84");
        }

        static void UpperCabinet(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w, h, d, White);
            int doors = Mathf.Max(1, Mathf.RoundToInt(w / 0.6f));
            for (int i = 1; i < doors; i++)
                k.Box(-w / 2f + i * w / doors, h / 2f, -d / 2f - 0.004f, 0.01f, h - 0.06f, 0.008f, "#D6D2CA");
            for (int i = 0; i < doors; i++)
                k.Box(-w / 2f + (i + 0.5f) * w / doors + (i % 2 == 0 ? 0.18f : -0.18f) * w / doors, 0.12f, -d / 2f - 0.01f, 0.02f, 0.1f, 0.02f, MetalD);
        }

        static void Hood(Kit k, float w, float h, float d)
        {
            k.Box(0f, 0.1f, 0f, w, 0.2f, d, Metal);
            k.Box(0f, 0.2f + (h - 0.2f) / 2f, d * 0.2f, w * 0.35f, h - 0.2f, d * 0.5f, Metal);
            k.Box(0f, 0.005f, 0f, w - 0.1f, 0.01f, d - 0.1f, MetalD);
        }

        static void Microwave(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w, h, d, White);
            k.Box(-w * 0.12f, h / 2f, -d / 2f - 0.003f, w * 0.6f, h * 0.65f, 0.005f, Black);
            k.Box(w * 0.33f, h / 2f, -d / 2f - 0.003f, w * 0.2f, h * 0.7f, 0.005f, Metal);
        }

        static void CoffeeMachine(Kit k, float w, float h, float d)
        {
            k.Box(0f, h * 0.06f, 0f, w, h * 0.12f, d, Black);
            k.Box(0f, h / 2f, d * 0.2f, w, h, d * 0.6f, Black);
            k.Box(0f, h * 0.88f, -d * 0.15f, w, h * 0.24f, d * 0.4f, Black);
            k.Cyl(0f, h * 0.22f, -d * 0.2f, w * 0.4f, h * 0.2f, w * 0.4f, Glass);
        }

        static void Trash(Kit k, float w, float h)
        {
            k.Cyl(0f, h / 2f, 0f, w, h, w, Metal);
            k.Cyl(0f, h + 0.01f, 0f, w * 1.04f, 0.03f, w * 1.04f, MetalD);
            k.Box(0f, 0.04f, -w / 2f - 0.03f, 0.1f, 0.02f, 0.06f, MetalD);
        }

        static void Clock(Kit k, float w)
        {
            float c = w / 2f;
            k.Cyl(0f, c, 0.005f, w + 0.04f, 0.025f, w + 0.04f, Black, 90f);
            k.Cyl(0f, c, -0.008f, w, 0.02f, w, White, 90f);
            k.Box(0f, c + w * 0.14f, -0.02f, 0.015f, w * 0.3f, 0.005f, Black);
            k.Box(w * 0.1f, c, -0.022f, w * 0.22f, 0.012f, 0.005f, Black);
        }

        static void Washer(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w, h, d, White);
            k.Cyl(0f, h * 0.45f, -d / 2f - 0.01f, w * 0.62f, 0.02f, w * 0.62f, Metal, 90f);
            k.Cyl(0f, h * 0.45f, -d / 2f - 0.02f, w * 0.46f, 0.02f, w * 0.46f, "#5E6E8A", 90f);
            k.Box(0f, h - 0.08f, -d / 2f - 0.005f, w - 0.04f, 0.1f, 0.01f, "#D9D9D9");
            k.Cyl(w * 0.3f, h - 0.08f, -d / 2f - 0.015f, 0.07f, 0.02f, 0.07f, MetalD, 90f);
        }

        static void WallShelf(Kit k, float w, float d)
        {
            k.Box(0f, 0.02f, 0f, w, 0.04f, d, WoodM);
            k.Box(-w / 2f + 0.15f, -0.06f, d * 0.2f, 0.03f, 0.12f, d * 0.6f, WoodD);
            k.Box(w / 2f - 0.15f, -0.06f, d * 0.2f, 0.03f, 0.12f, d * 0.6f, WoodD);
            float s = w / 1.6f;
            k.Cyl(-0.5f * s, 0.14f, 0f, 0.1f, 0.2f, 0.1f, Blue);
            k.Cyl(-0.32f * s, 0.12f, 0f, 0.09f, 0.16f, 0.09f, "#F2D16B");
            k.Box(0.1f * s, 0.12f, 0f, 0.25f, 0.16f, 0.15f, Pink);
            k.Cyl(0.45f * s, 0.15f, 0f, 0.12f, 0.22f, 0.12f, White);
        }

        static void DryingRack(Kit k, float w, float h)
        {
            foreach (float x in new[] { -w / 2f + 0.03f, w / 2f - 0.03f })
            {
                k.BarYZ(-0.2f, 0f, 0f, h, x, 0.03f, MetalL);
                k.BarYZ(0.2f, 0f, 0f, h, x, 0.03f, MetalL);
            }
            foreach (float y in new[] { 0.35f, 0.6f, 0.85f })
            {
                float z = 0.2f * (1f - y / h);
                k.Box(0f, y, -z, w, 0.02f, 0.02f, White);
                k.Box(0f, y, z, w, 0.02f, 0.02f, White);
            }
            k.Box(0.15f, 0.72f, -0.08f, 0.35f, 0.3f, 0.02f, Pink, 12f);
            k.Box(-0.3f, 0.5f, 0.1f, 0.08f, 0.18f, 0.02f, "#F2D16B", -10f);
            k.Box(-0.15f, 0.48f, -0.12f, 0.22f, 0.22f, 0.02f, Blue, 15f);
        }

        static void Crate(Kit k, float x, float y, float z, float s, float yaw)
        {
            Kit c = k.Group(x, y, z, yaw);
            c.Box(0f, s / 2f, 0f, s, s, s, WoodL);
            foreach (float f in new[] { 0.25f, 0.75f })
            {
                c.Box(0f, s * f, -s / 2f - 0.006f, s, 0.07f, 0.012f, WoodD);
                c.Box(-s / 2f - 0.006f, s * f, 0f, 0.012f, 0.07f, s, WoodD);
                c.Box(0f, s + 0.004f, (f - 0.5f) * s, s, 0.008f, 0.06f, WoodD);
            }
        }

        static void Crates(Kit k)
        {
            Crate(k, -0.08f, 0f, 0.05f, 0.6f, 0f);
            Crate(k, 0.05f, 0.6f, 0f, 0.36f, 15f);
            Crate(k, 0.3f, 0f, -0.28f, 0.3f, -10f);
        }

        static void Barrel(Kit k, float w, float h)
        {
            k.Cyl(0f, h / 2f, 0f, w * 0.93f, h, w * 0.93f, "#8B6B4E");
            k.Cyl(0f, h * 0.17f, 0f, w * 0.97f, 0.05f, w * 0.97f, MetalD);
            k.Cyl(0f, h * 0.83f, 0f, w * 0.97f, 0.05f, w * 0.97f, MetalD);
            k.Cyl(0f, h + 0.005f, 0f, w * 0.83f, 0.01f, w * 0.83f, WoodD);
        }

        static void Pipes(Kit k, float w)
        {
            k.Cyl(0f, 0.08f, 0f, 0.1f, w, 0.1f, "#8E8A84", 0f, 0f, 90f);
            k.Cyl(0f, 0.2f, 0.04f, 0.07f, w, 0.07f, Copper, 0f, 0f, 90f);
            int n = Mathf.Max(2, Mathf.RoundToInt(w / 1.5f));
            for (int i = 0; i <= n; i++)
                k.Box(-w / 2f + 0.1f + i * (w - 0.2f) / n, 0.14f, 0.06f, 0.04f, 0.28f, 0.06f, MetalD);
        }

        static void Sheeted(Kit k, float w, float h, float d)
        {
            k.Box(0f, 0.04f, 0f, w, 0.08f, d, "#DDD7C9");
            k.Box(0f, h * 0.45f, 0f, w * 0.95f, h * 0.9f, d * 0.9f, Sheet);
            k.Ball(-w * 0.2f, h * 0.85f, d * 0.1f, w * 0.45f, h * 0.35f, d * 0.7f, Sheet);
            k.Ball(w * 0.25f, h * 0.8f, -d * 0.05f, w * 0.4f, h * 0.3f, d * 0.6f, "#DDD7C9");
        }

        static void Beanbag(Kit k, float w, float h, float d)
        {
            k.Ball(0f, h * 0.4f, 0f, w, h * 0.8f, d, "#C56A4E");
            k.Ball(0f, h * 0.65f, d * 0.18f, w * 0.75f, h * 0.6f, d * 0.55f, "#C56A4E");
            k.Ball(0f, h * 0.62f, -d * 0.12f, w * 0.5f, h * 0.15f, d * 0.4f, "#B85E44");
        }

        static void LeaningFrames(Kit k)
        {
            Kit a = k.Group(-0.22f, 0f, 0.02f, 0f, 12f);
            a.Box(0f, 0.4f, 0f, 0.6f, 0.8f, 0.04f, WoodD);
            a.Box(0f, 0.4f, -0.022f, 0.5f, 0.7f, 0.005f, "#8FB3A6");
            Kit b = k.Group(0.25f, 0f, 0f, 0f, 15f);
            b.Box(0f, 0.3f, 0f, 0.5f, 0.6f, 0.04f, WoodL);
            b.Box(0f, 0.3f, -0.022f, 0.4f, 0.5f, 0.005f, "#C56A4E");
            Kit c = k.Group(0.02f, 0f, -0.08f, 0f, 8f);
            c.Box(0f, 0.2f, 0f, 0.35f, 0.4f, 0.03f, White);
            c.Box(0f, 0.2f, -0.017f, 0.27f, 0.32f, 0.005f, "#F2D16B");
        }

        static void RockingHorse(Kit k)
        {
            const string Body = "#E8D3B0";
            k.Box(0f, 0.04f, -0.12f, 0.85f, 0.05f, 0.04f, WoodM);
            k.Box(0f, 0.04f, 0.12f, 0.85f, 0.05f, 0.04f, WoodM);
            foreach (float z in new[] { -0.1f, 0.1f })
            {
                k.Bar(-0.28f, 0.05f, -0.18f, 0.42f, z, 0.04f, WoodM);
                k.Bar(0.28f, 0.05f, 0.18f, 0.42f, z, 0.04f, WoodM);
            }
            k.Box(0f, 0.45f, 0f, 0.5f, 0.18f, 0.18f, Body);
            k.Box(0.28f, 0.62f, 0f, 0.12f, 0.3f, 0.12f, Body, 0f, 0f, -20f);
            k.Box(0.36f, 0.73f, 0f, 0.22f, 0.1f, 0.1f, Body);
            k.Box(0.24f, 0.67f, 0f, 0.05f, 0.25f, 0.13f, WoodD, 0f, 0f, -20f);
            k.Box(-0.28f, 0.48f, 0f, 0.05f, 0.18f, 0.06f, WoodD, 0f, 0f, 30f);
            k.Box(0f, 0.55f, 0f, 0.18f, 0.03f, 0.2f, Red);
        }

        static void Bush(Kit k, float w, float h, float d)
        {
            k.Ball(0f, h * 0.45f, 0f, w, h * 0.9f, d, Leaf);
            k.Ball(-w * 0.25f, h * 0.55f, -0.05f, w * 0.6f, h * 0.7f, d * 0.8f, LeafL);
            k.Ball(w * 0.28f, h * 0.5f, 0.05f, w * 0.55f, h * 0.65f, d * 0.8f, "#7FA866");
            k.Ball(-w * 0.2f, h * 0.85f, -d * 0.3f, 0.08f, 0.08f, 0.08f, "#E58FA6");
            k.Ball(w * 0.25f, h * 0.75f, -d * 0.35f, 0.08f, 0.08f, 0.08f, "#F2D16B");
            k.Ball(w * 0.05f, h * 0.6f, -d * 0.45f, 0.07f, 0.07f, 0.07f, "#E58FA6");
        }

        static void StreetLamp(Kit k, float h)
        {
            const string Pole = "#3D434A";
            k.Cyl(0f, 0.15f, 0f, 0.25f, 0.3f, 0.25f, Pole);
            k.Cyl(0f, h / 2f, 0f, 0.1f, h, 0.1f, Pole);
            k.Box(0f, h - 0.05f, -0.3f, 0.06f, 0.06f, 0.6f, Pole);
            k.Box(0f, h - 0.12f, -0.6f, 0.22f, 0.1f, 0.32f, Pole);
            k.Box(0f, h - 0.18f, -0.6f, 0.18f, 0.03f, 0.26f, "#F7E9B5");
        }

        static void Flowers(Kit k, float w, float d)
        {
            k.Box(0f, 0.03f, 0f, w, 0.06f, d, WoodD);
            k.Box(0f, 0.04f, -d / 2f - 0.03f, w + 0.06f, 0.08f, 0.06f, "#B9B2A6");
            k.Box(0f, 0.04f, d / 2f + 0.03f, w + 0.06f, 0.08f, 0.06f, "#B9B2A6");
            string[] colors = { "#E58FA6", "#F2D16B", "#F4F1EA", "#C56A4E" };
            int n = Mathf.FloorToInt(w / 0.25f);
            for (int i = 0; i < n; i++)
            {
                float x = -w / 2f + (i + 0.5f) * w / n;
                float z = (i % 2 == 0 ? -0.08f : 0.08f) * d / 0.5f;
                k.Ball(x, 0.12f, z, 0.18f, 0.14f, 0.18f, Leaf);
                k.Ball(x + 0.02f, 0.2f, z, 0.09f, 0.09f, 0.09f, colors[i % colors.Length]);
            }
        }
        static void Toys(Kit k)
        {
            k.Ball(-0.25f, 0.09f, 0.05f, 0.18f, 0.18f, 0.18f, Red);
            k.Box(0.12f, 0.05f, 0.1f, 0.1f, 0.1f, 0.1f, Yellow, 0f, 20f);
            k.Box(0.25f, 0.05f, -0.05f, 0.1f, 0.1f, 0.1f, Blue);
            k.Box(0.2f, 0.15f, 0.03f, 0.1f, 0.1f, 0.1f, LeafL, 0f, 10f);
            Kit car = k.Group(-0.05f, 0f, -0.15f, 35f);
            car.Box(0f, 0.05f, 0f, 0.2f, 0.06f, 0.11f, "#E58FA6");
            car.Box(-0.02f, 0.1f, 0f, 0.1f, 0.05f, 0.09f, Glass);
            foreach (float x in new[] { -0.07f, 0.07f })
                foreach (float z in new[] { -0.06f, 0.06f })
                    car.Cyl(x, 0.025f, z, 0.05f, 0.02f, 0.05f, Black, 90f);
        }
        static void Books(Kit k, float w, float h, float d, int seed)
        {
            string[] colors = { "#C2453A", "#6F8FC2", "#E8C15A", "#5FB08A", "#A87B55", "#8E6FB0", "#E9E4D8", "#2E3A5C" };
            float x = -w / 2f + 0.02f;
            int i = seed * 3;
            while (x < w / 2f - 0.06f)
            {
                float t = 0.035f + (i * 37 % 5) * 0.008f;
                float bh = h * (0.68f + (i * 53 % 4) * 0.08f);
                string c = colors[i % colors.Length];
                if (i % 7 == 3)
                {
                    x += 0.09f;
                }
                else if (i % 9 == 4 && x + 0.25f < w / 2f)
                {
                    for (int j = 0; j < 3; j++)
                        k.Box(x + 0.11f, 0.02f + j * 0.035f, 0f, 0.2f - j * 0.02f, 0.034f, d * 0.6f, colors[(i + j) % colors.Length]);
                    x += 0.25f;
                }
                else if (i % 11 == 6)
                {
                    k.Box(x + 0.06f, bh * 0.47f, 0f, t, bh, d * 0.6f, c, 0f, 0f, -22f);
                    x += 0.12f;
                }
                else
                {
                    k.Box(x + t / 2f, bh / 2f, 0f, t, bh, d * 0.6f, c);
                    x += t + 0.004f;
                }
                i++;
            }
        }

        static void Jars(Kit k, float w, float h, float d, int seed)
        {
            float x = -w / 2f + 0.12f;
            int i = seed;
            while (x < w / 2f - 0.1f)
            {
                float jh = Mathf.Min(h, 0.14f + (i * 29 % 4) * 0.03f);
                switch (i % 4)
                {
                    case 0:
                        k.Cyl(x, jh / 2f, 0f, 0.12f, jh, 0.12f, "#A9C4B8");
                        k.Cyl(x, jh + 0.01f, 0f, 0.13f, 0.02f, 0.13f, Red);
                        x += 0.2f;
                        break;
                    case 1:
                        CardBox(k, x + 0.06f, 0f, 0f, 0.26f, Mathf.Min(h, 0.2f), d * 0.8f, 4f, CardOld, "#CFC2A0");
                        x += 0.34f;
                        break;
                    case 2:
                        k.Cyl(x, jh / 2f, 0f, 0.15f, jh, 0.15f, Metal);
                        k.Cyl(x, jh + 0.008f, 0f, 0.16f, 0.015f, 0.16f, MetalD);
                        x += 0.22f;
                        break;
                    default:
                        k.Cyl(x, jh * 0.6f, 0f, 0.09f, jh * 1.2f, 0.09f, "#C9A26B");
                        k.Cyl(x + 0.1f, jh * 0.5f, 0.02f, 0.09f, jh, 0.09f, "#E8C15A");
                        x += 0.26f;
                        break;
                }
                i++;
            }
        }

        static void HangingBulb(Kit k, float h)
        {
            float top = h + 0.15f;
            k.Cyl(0f, top, 0f, 0.12f, 0.03f, 0.12f, White);
            k.Cyl(0f, (top + 0.2f) / 2f, 0f, 0.015f, top - 0.2f, 0.015f, Black);
            k.Cyl(0f, 0.2f, 0f, 0.06f, 0.08f, 0.06f, Black);
            k.Ball(0f, 0.1f, 0f, 0.14f, 0.18f, 0.14f, "#FFE9A8");
        }
        static void ShelfBoxes(Kit k, float w, float h, float d, int seed)
        {
            float bh = Mathf.Min(h, 0.32f);
            if (seed % 3 == 2)
            {
                CardBox(k, -w * 0.12f, 0f, 0f, w * 0.5f, bh, d * 0.85f, 3f, Card, Tape, White);
                CardBox(k, w * 0.3f, 0f, 0f, w * 0.22f, bh * 0.6f, d * 0.6f, -6f, CardD, Tape);
            }
            else
            {
                CardBox(k, -w * 0.24f, 0f, 0f, w * 0.42f, bh, d * 0.85f, seed % 2 == 0 ? 2f : -3f, seed % 2 == 0 ? Card : CardD, Tape, White);
                CardBox(k, w * 0.24f, 0f, 0f, w * 0.4f, bh * 0.85f, d * 0.8f, seed % 2 == 0 ? -4f : 3f, seed % 2 == 0 ? CardD : Card, Tape);
            }
        }

        static void Dryer(Kit k, float w, float h, float d)
        {
            k.Box(0f, h / 2f, 0f, w, h, d, "#ECE6DA");
            k.Cyl(0f, h * 0.48f, -d / 2f - 0.01f, w * 0.66f, 0.02f, w * 0.66f, MetalL, 90f);
            k.Cyl(0f, h * 0.48f, -d / 2f - 0.02f, w * 0.5f, 0.02f, w * 0.5f, "#3D434A", 90f);
            k.Box(0f, h * 0.48f, -d / 2f - 0.032f, w * 0.12f, w * 0.03f, 0.01f, MetalD);
            k.Box(0f, h - 0.08f, -d / 2f - 0.005f, w - 0.04f, 0.1f, 0.01f, "#CFC8BA");
            k.Cyl(-w * 0.3f, h - 0.08f, -d / 2f - 0.015f, 0.07f, 0.02f, 0.07f, MetalD, 90f);
            k.Cyl(w * 0.3f, h - 0.08f, -d / 2f - 0.015f, 0.07f, 0.02f, 0.07f, MetalD, 90f);
            k.Box(w * 0.05f, h - 0.08f, -d / 2f - 0.012f, 0.18f, 0.04f, 0.005f, "#5FB08A");
        }

        static void Chair(Kit k, float w, float h, float d)
        {
            k.Box(0f, 0.45f, 0f, w, 0.05f, d, WoodM);
            k.Box(0f, 0.68f, d / 2f - 0.03f, w, 0.42f, 0.05f, WoodM);
            foreach (float sx in new[] { -1f, 1f })
                foreach (float sz in new[] { -1f, 1f })
                    k.Box(sx * (w / 2f - 0.03f), 0.215f, sz * (d / 2f - 0.03f), 0.04f, 0.43f, 0.04f, WoodD);
        }

        static void Car(Kit k, float w, float d)
        {
            const string Body = "#6FA0C8", Trim = "#4E7FA8";
            k.Box(0f, 0.6f, 0f, w, 0.65f, d, Body);
            k.Box(0f, 0.6f, -d / 2f + 0.02f, w - 0.1f, 0.12f, 0.06f, Trim);
            k.Box(0f, 1.2f, 0.25f, w - 0.16f, 0.6f, d * 0.5f, Body);
            k.Box(0f, 1.22f, 0.25f - d * 0.25f - 0.005f, w - 0.36f, 0.42f, 0.01f, Glass);
            k.Box(0f, 1.22f, 0.25f + d * 0.25f + 0.005f, w - 0.36f, 0.4f, 0.01f, Glass);
            foreach (float s in new[] { -1f, 1f })
            {
                float x = s * ((w - 0.16f) / 2f + 0.005f);
                k.Box(x, 1.22f, 0.25f - d * 0.12f, 0.01f, 0.4f, d * 0.2f, Glass);
                k.Box(x, 1.22f, 0.25f + d * 0.12f, 0.01f, 0.4f, d * 0.2f, Glass);
                k.Box(s * (w / 2f + 0.005f), 0.62f, 0.25f, 0.01f, 0.5f, 0.015f, Trim);
                k.Box(s * (w / 2f + 0.03f), 0.62f, 0.05f, 0.04f, 0.04f, 0.12f, MetalL);
                k.Box(s * (w / 2f - 0.25f), 0.65f, -d / 2f - 0.01f, 0.32f, 0.16f, 0.02f, "#F7E9B5");
                k.Box(s * (w / 2f - 0.22f), 0.68f, d / 2f + 0.01f, 0.26f, 0.12f, 0.02f, Red);
                k.Box(s * (w / 2f + 0.06f), 1.05f, -d * 0.04f, 0.1f, 0.08f, 0.12f, Body);
                foreach (float z in new[] { -d / 2f + 0.75f, d / 2f - 0.75f })
                {
                    k.Cyl(s * (w / 2f - 0.1f), 0.33f, z, 0.66f, 0.26f, 0.66f, Black, 0f, 0f, 90f);
                    k.Cyl(s * (w / 2f + 0.02f), 0.33f, z, 0.32f, 0.02f, 0.32f, MetalL, 0f, 0f, 90f);
                }
            }
            k.Box(0f, 0.32f, -d / 2f - 0.04f, w + 0.04f, 0.14f, 0.1f, "#D9D9D9");
            k.Box(0f, 0.32f, d / 2f + 0.04f, w + 0.04f, 0.14f, 0.1f, "#D9D9D9");
            k.Box(0f, 0.55f, -d / 2f - 0.005f, w * 0.4f, 0.14f, 0.01f, "#2E3A5C");
            k.Box(0f, 1.52f, 0.25f, w - 0.4f, 0.04f, d * 0.4f, MetalD);
            CardBox(k, 0.1f, 1.54f, 0.3f, 0.7f, 0.25f, 0.55f, 8f, Card, Tape);
            k.Box(-0.35f, 1.62f, 0.05f, 0.4f, 0.4f, 0.18f, "#C56A4E", 0f, 0f, 0f);
        }
        static void TreeBlock(Kit k, float w, float h, float d)
        {
            float s = Mathf.Min(w, d);
            k.Box(0f, h * 0.22f, 0f, s * 0.22f, h * 0.44f, s * 0.22f, WoodD);
            k.Box(0f, h * 0.6f, 0f, s * 1.1f, h * 0.42f, s * 1.1f, Leaf);
            k.Box(0f, h * 0.9f, 0f, s * 0.72f, h * 0.3f, s * 0.72f, LeafL, 0f, 20f);
        }

        static void SwingTree(Kit k, float w, float h)
        {
            k.Box(0f, h * 0.3f, 0f, 0.4f, h * 0.6f, 0.4f, WoodD);
            k.Box(0f, h * 0.68f, 0f, 1.9f, h * 0.36f, 1.7f, Leaf);
            k.Box(0.1f, h * 0.92f, 0.05f, 1.25f, h * 0.24f, 1.15f, LeafL, 0f, 18f);
            float by = h * 0.5f;
            float tip = -w / 2f + 0.25f;
            k.Box(tip / 2f, by, 0f, -tip + 0.2f, 0.16f, 0.18f, WoodD);
            k.Box(tip + 0.1f, by + 0.12f, 0f, 0.35f, 0.3f, 0.5f, Leaf);
            float tx = tip + 0.15f, ty = 0.62f;
            foreach (float z in new[] { -0.1f, 0.1f })
                k.Cyl(tx, (by + ty + 0.28f) / 2f, z, 0.025f, by - ty - 0.28f, 0.025f, "#D8C08E");
            k.Cyl(tx, ty, 0f, 0.62f, 0.2f, 0.62f, Black, 0f, 0f, 90f);
            k.Cyl(tx, ty, 0f, 0.3f, 0.21f, 0.3f, "#141016", 0f, 0f, 90f);
        }

        static void Pond(Kit k, float w, float d)
        {
            k.Cyl(0f, 0.02f, 0f, w, 0.04f, d, "#7FA9C9");
            k.Cyl(0.1f * w, 0.021f, 0.05f * d, w * 0.6f, 0.04f, d * 0.55f, "#6A97BB");
            const int n = 18;
            for (int i = 0; i < n; i++)
            {
                float a = i * Mathf.PI * 2f / n;
                float x = Mathf.Cos(a) * w / 2f, z = Mathf.Sin(a) * d / 2f;
                k.Ball(x, 0.05f, z, 0.32f + (i % 3) * 0.04f, 0.14f, 0.26f, i % 2 == 0 ? "#A8A49C" : "#8E8A84");
            }
            k.Cyl(-w * 0.2f, 0.045f, -d * 0.12f, 0.3f, 0.01f, 0.26f, Leaf);
            k.Cyl(w * 0.22f, 0.045f, d * 0.18f, 0.26f, 0.01f, 0.22f, Leaf);
            k.Ball(w * 0.22f, 0.06f, d * 0.18f, 0.08f, 0.06f, 0.08f, "#E58FA6");
            for (int i = 0; i < 6; i++)
                k.Box(w * 0.38f + (i % 3) * 0.08f, 0.3f + (i % 2) * 0.1f, d * 0.22f - (i / 3) * 0.1f, 0.03f, 0.6f + (i % 2) * 0.2f, 0.03f, LeafL);
            Kit duck = k.Group(-w * 0.05f, 0.04f, 0.05f, -30f);
            duck.Ball(0f, 0.06f, 0f, 0.22f, 0.13f, 0.14f, "#F2D16B");
            duck.Ball(0.09f, 0.15f, 0f, 0.1f, 0.1f, 0.1f, "#F2D16B");
            duck.Box(0.15f, 0.145f, 0f, 0.06f, 0.025f, 0.04f, "#E08A3C");
            duck.Box(0.11f, 0.17f, -0.045f, 0.015f, 0.015f, 0.01f, Black);
        }

        static void Bench(Kit k, float w, float d)
        {
            const string Iron = "#3D434A";
            for (int i = 0; i < 3; i++)
                k.Box(0f, 0.44f, -d / 2f + 0.08f + i * (d - 0.16f) / 2f, w, 0.04f, 0.12f, WoodM);
            foreach (float y in new[] { 0.62f, 0.8f })
                k.Box(0f, y, d / 2f - 0.03f, w, 0.12f, 0.04f, WoodM, -10f);
            foreach (float x in new[] { -w / 2f + 0.12f, w / 2f - 0.12f })
            {
                k.Box(x, 0.21f, -d / 2f + 0.06f, 0.05f, 0.42f, 0.05f, Iron);
                k.Box(x, 0.45f, d / 2f - 0.05f, 0.05f, 0.9f, 0.05f, Iron);
                k.Box(x, 0.42f, 0f, 0.05f, 0.04f, d, Iron);
                k.Box(x, 0.58f, -d / 2f + 0.1f, 0.05f, 0.04f, d * 0.6f, Iron);
            }
        }
    }
}
