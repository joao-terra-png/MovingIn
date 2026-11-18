using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace MovingIn.Hub.EditorTools{
    public static partial class ArtPassBuilder
    {
        const string AtmoGroup = "Atmosphere";
        const string SunWarm = "#FFE2B8";
        const string LampWarm = "#FFC98A";
        const string StreetWarm = "#FFD9A0";

        static readonly Quaternion BeamForward = Quaternion.Euler(36.87f, 0f, 0f);
        static readonly Quaternion BeamWest = Quaternion.Euler(0f, 0f, -36.87f);

        static Material _beamMat, _glowMat, _particleAlpha, _particleAdd;
        static Quaternion _camRot = Quaternion.Euler(30f, 45f, 0f);
        static readonly Dictionary<Transform, Transform> _atmoRoots = new Dictionary<Transform, Transform>();

        [MenuItem("MovingIn/Art/Apply Atmosphere")]
        static void ApplyAtmosphere()
        {
            Transform house = FindHouse();
            if (house == null) return;
            _mats.Clear();
            _atmoRoots.Clear();
            if (!AtmoMaterials()) return;
            Camera cam = Camera.main;
            if (cam != null) _camRot = cam.transform.rotation;

            int beams = 0, lamps = 0, fx = 0;

            Transform bedroom = house.Find("Bedroom");
            Transform window = bedroom != null ? FindDeep(bedroom, "Window") : null;
            if (window != null)
            {
                SunBeam(bedroom, window.position + Vector3.back * 0.06f, window.lossyScale.x, 2.4f);
                beams++;
            }

            Transform attic = house.Find("Attic");
            Transform round = attic != null ? FindDeep(attic, "RoundWindow") : null;
            if (round != null)
            {
                SunBeam(attic, round.position + Vector3.back * 0.06f, 0.9f, 2.7f);
                Particles(AtmoRoot(attic), "Motes", new Vector3(8f, FloorTop(attic) + 1.3f, 4f), AmbientParticles.Kind.Motes, AtmosphereMode.Always, new Vector3(11f, 2.4f, 7f), 5f, false);
                beams++; fx++;
            }

            Transform basement = house.Find("Basement");
            if (basement != null)
            {
                float f = FloorTop(basement);
                Transform root = AtmoRoot(basement);
                Transform win = NewChild("HighWindow", root);
                win.position = new Vector3(14f, f + 1.85f, 7.875f);
                WindowPlain(new Kit(win), 0.9f, 0.45f);
                SunBeam(basement, new Vector3(14f, f + 2.07f, 7.82f), 0.85f, 3f);
                Particles(root, "Motes", new Vector3(10.5f, f + 1.3f, 4f), AmbientParticles.Kind.Motes, AtmosphereMode.Always, new Vector3(12f, 2.4f, 7f), 4f, false);
                Particles(root, "Drips", new Vector3(9.6f, f + 2.9f, 2.2f), AmbientParticles.Kind.Drips, AtmosphereMode.Always, new Vector3(0.2f, 0f, 0.2f), 0.5f, false);

                Transform panel = FindDeep(basement, "BreakerPanel");
                if (panel != null)
                {
                    AmbientParticles sparks = Particles(root, "Sparks", panel.position + new Vector3(0f, 0.1f, -0.2f), AmbientParticles.Kind.Sparks, AtmosphereMode.Blackout, Vector3.one, 0f, true);
                    sparks.transform.rotation = Quaternion.Euler(-30f, 180f, 0f);
                    fx++;
                }

                Transform bulb = FindDeep(basement, "LightBulb");
                if (bulb != null)
                {
                    Vector3 p = new Vector3(bulb.position.x, bulb.position.y - bulb.lossyScale.y / 2f + 0.1f, bulb.position.z);
                    Lamp(root, "BulbLamp", p, AtmosphereMode.RoomLamps, LampWarm, 1.4f, 6f, 0.6f, 0.25f);
                    Beam(root, "BulbBeam", p, Quaternion.identity, AtmosphereMode.RoomLamps, StreetWarm, false, 0.1f, new Vector2(0.12f, 0.12f), new Vector2(3.2f, 3.2f), 2.6f);
                    lamps++; beams++;
                }
                beams++; fx += 2;
            }

            Transform laundry = house.Find("Laundry");
            if (laundry != null)
            {
                float f = FloorTop(laundry);
                Transform root = AtmoRoot(laundry);
                Transform win = NewChild("Window", root);
                win.position = new Vector3(20.875f, f + 1.2f, 9.8f);
                win.rotation = Quaternion.Euler(0f, KitYaw(Face.W), 0f);
                WindowPlain(new Kit(win), 1.2f, 1.0f);
                Vector3 origin = new Vector3(20.82f, f + 1.7f, 9.8f);
                Beam(root, "SunBeam", origin, BeamWest, AtmosphereMode.Sunlit, SunWarm, true, 0.3f, new Vector2(0.25f, 1.05f), new Vector2(1.1f, 1.5f), 2.1f);
                beams++;
            }

            lamps += DecorLamp(house, "LivingRoom", "FloorLamp", 1.45f, 1.3f, 4.5f, 0.9f);
            lamps += DecorLamp(house, "Bedroom", "TableLamp", 0.35f, 0.9f, 3.2f, 0.6f);

            Transform front = house.Find("Front");
            if (front != null)
            {
                Transform root = AtmoRoot(front);
                Transform decor = front.Find("Decor");
                foreach (string name in new[] { "StreetLamp_1", "StreetLamp_2" })
                {
                    Transform s = decor != null ? decor.Find(name) : null;
                    if (s == null) continue;
                    Vector3 head = s.position + new Vector3(0f, 3.0f, -0.6f);
                    Lamp(root, name + "_Light", head + Vector3.down * 0.08f, AtmosphereMode.Dark, StreetWarm, 1.4f, 6.5f, 0.8f, 0f);
                    Beam(root, name + "_Beam", head, Quaternion.identity, AtmosphereMode.Dark, StreetWarm, false, 0.1f, new Vector2(0.18f, 0.26f), new Vector2(2.4f, 2.4f), 3f);
                    lamps++; beams++;
                }

                foreach (float x in new[] { 9.0f, 11.0f })
                {
                    Transform lantern = NewChild("Lantern", root);
                    lantern.position = new Vector3(x, 1.55f, -0.02f);
                    Lantern(new Kit(lantern));
                    Lamp(root, "Lantern_Glow", new Vector3(x, 1.71f, -0.1f), AtmosphereMode.RoomLamps, LampWarm, 0f, 0f, 0.55f, 0f);
                    lamps++;
                }

                foreach (string name in new[] { "Tree_1", "Tree_2" })
                {
                    Transform t = FindDeep(front, name);
                    if (t == null) continue;
                    float bottom = t.position.y - t.lossyScale.y / 2f;
                    float w = Mathf.Max(t.lossyScale.x, t.lossyScale.z);
                    Leaves(root, name, new Vector3(t.position.x, bottom + t.lossyScale.y * 0.65f, t.position.z), w * 1.1f);
                    fx++;
                }
                foreach (Decor e in Decors)
                {
                    if (e.room != "Front" || (e.kit != "TreeBlock" && e.kit != "SwingTree")) continue;
                    Transform t = decor != null ? decor.Find(e.name) : null;
                    if (t == null) continue;
                    float w = e.kit == "SwingTree" ? 1.9f : Mathf.Min(e.w, e.d) * 1.1f;
                    Leaves(root, e.name, t.position + Vector3.up * e.h * 0.66f, w);
                    fx++;
                }

                Particles(root, "Fireflies", new Vector3(11f, 0.9f, -5f), AmbientParticles.Kind.Fireflies, AtmosphereMode.Dark, new Vector3(24f, 1f, 8f), 5f, true);
                Transform pond = decor != null ? decor.Find("Pond") : null;
                if (pond != null) Particles(root, "Glints", pond.position + Vector3.up * 0.07f, AmbientParticles.Kind.Glints, AtmosphereMode.Sunlit, new Vector3(2.2f, 0f, 1.2f), 4f, true);
                fx += 2;
            }

            if (FlashlightBeam()) beams++;
            bool grade = SetupGrade(cam);
            bool night = TuneBlackout();

            AssetDatabase.SaveAssets();
            Debug.Log($"[ArtPass] Atmosfera: {beams} feixes, {lamps} luminárias, {fx} efeitos de partícula" + (grade ? ", gradação de cor na câmera" : ". Câmera principal não encontrada: sem gradação de cor") + (night ? ", apagão clareado." : "."));
        }

        [MenuItem("MovingIn/Art/Remove Atmosphere")]
        static void RemoveAtmosphere()
        {
            Transform house = FindHouse();
            if (house != null)
                foreach (Transform t in house.GetComponentsInChildren<Transform>(true))
                    if (t != null && t.name == AtmoGroup && t.GetComponent<ArtGenerated>() != null)
                        Undo.DestroyObjectImmediate(t.gameObject);
            GameObject player = GameObject.Find("Player");
            Transform beam = player != null ? FindDeep(player.transform, "FlashlightBeam") : null;
            if (beam != null) Undo.DestroyObjectImmediate(beam.gameObject);
            Camera cam = Camera.main;
            ScreenGrade g = cam != null ? cam.GetComponent<ScreenGrade>() : null;
            if (g != null) Undo.DestroyObjectImmediate(g);
            Debug.Log("[ArtPass] Atmosfera removida.");
        }

        static bool AtmoMaterials()
        {
            _beamMat = ShaderMat("M_Beam", "MovingIn/LightBeam");
            _glowMat = ShaderMat("M_Glow", "MovingIn/Glow");
            _particleAlpha = ShaderMat("M_ParticleAlpha", "MovingIn/ParticleAlpha");
            _particleAdd = ShaderMat("M_ParticleAdd", "MovingIn/ParticleAdd");
            bool ok = _beamMat != null && _glowMat != null && _particleAlpha != null && _particleAdd != null;
            if (!ok) Debug.LogError("[ArtPass] Shaders da atmosfera nao encontrados. Copie LightBeam, Glow, ParticleAlpha, ParticleAdd e ScreenGrade para a pasta Shaders.");
            return ok;
        }

        static Material ShaderMat(string name, string shaderName)
        {
            Shader s = Shader.Find(shaderName);
            if (s == null) return null;
            EnsureMatFolder();
            string path = $"{MatFolder}/{name}.mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null)
            {
                m = new Material(s);
                AssetDatabase.CreateAsset(m, path);
            }
            m.shader = s;
            EditorUtility.SetDirty(m);
            return m;
        }

        static Transform AtmoRoot(Transform room)
        {
            if (_atmoRoots.TryGetValue(room, out Transform cached) && cached != null) return cached;
            Transform old = room.Find(AtmoGroup);
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            GameObject g = new GameObject(AtmoGroup);
            Undo.RegisterCreatedObjectUndo(g, "Atmosphere");
            g.transform.SetParent(room, false);
            g.AddComponent<ArtGenerated>();
            _atmoRoots[room] = g.transform;
            return g.transform;
        }

        static void SunBeam(Transform room, Vector3 origin, float width, float length)
        {
            Transform root = AtmoRoot(room);
            Beam(root, "SunBeam", origin, BeamForward, AtmosphereMode.Sunlit, SunWarm, true, 0.3f, new Vector2(width * 0.85f, 0.25f), new Vector2(width * 1.3f, 1.1f), length);
        }

        static LightBeam Beam(Transform parent, string name, Vector3 pos, Quaternion rot, AtmosphereMode mode, string color, bool sun, float strength, Vector2 top, Vector2 bottom, float length)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.transform.rotation = rot;
            go.AddComponent<MeshFilter>();
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            r.sharedMaterial = _beamMat;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            LightBeam b = go.AddComponent<LightBeam>();
            SerializedObject so = new SerializedObject(b);
            so.FindProperty("mode").enumValueIndex = (int)mode;
            so.FindProperty("color").colorValue = Hex(color);
            so.FindProperty("useSunColor").boolValue = sun;
            so.FindProperty("strength").floatValue = strength;
            so.FindProperty("topSize").vector2Value = top;
            so.FindProperty("bottomSize").vector2Value = bottom;
            so.FindProperty("length").floatValue = length;
            so.ApplyModifiedPropertiesWithoutUndo();
            return b;
        }

        static AmbientParticles Particles(Transform parent, string name, Vector3 pos, AmbientParticles.Kind kind, AtmosphereMode mode, Vector3 area, float rate, bool additive)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;
            go.AddComponent<ParticleSystem>();
            ParticleSystemRenderer r = go.GetComponent<ParticleSystemRenderer>();
            r.sharedMaterial = additive ? _particleAdd : _particleAlpha;
            AmbientParticles ap = go.AddComponent<AmbientParticles>();
            ap.Setup(kind, mode, area, rate);
            EditorUtility.SetDirty(ap);
            return ap;
        }

        static void Leaves(Transform parent, string name, Vector3 canopy, float width)
        {
            Particles(parent, name + "_Leaves", canopy, AmbientParticles.Kind.Leaves, AtmosphereMode.Always, new Vector3(width, 0.4f, width), 0.3f, false);
        }

        static LampFx Lamp(Transform parent, string name, Vector3 pos, AtmosphereMode mode, string color, float intensity, float range, float glowSize, float flicker)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.position = pos;

            Light light = null;
            if (intensity > 0f)
            {
                GameObject lg = new GameObject("Light");
                lg.transform.SetParent(go.transform, false);
                light = lg.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = Hex(color);
                light.intensity = intensity;
                light.range = range;
                light.shadows = LightShadows.None;
                light.enabled = false;
            }

            GameObject glow = GameObject.CreatePrimitive(PrimitiveType.Quad);
            glow.name = "Glow";
            Object.DestroyImmediate(glow.GetComponent<Collider>());
            glow.transform.SetParent(go.transform, false);
            glow.transform.rotation = _camRot;
            glow.transform.localScale = Vector3.one * glowSize;
            MeshRenderer gr = glow.GetComponent<MeshRenderer>();
            gr.sharedMaterial = _glowMat;
            gr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            gr.receiveShadows = false;
            gr.enabled = false;

            LampFx fx = go.AddComponent<LampFx>();
            SerializedObject so = new SerializedObject(fx);
            so.FindProperty("mode").enumValueIndex = (int)mode;
            so.FindProperty("lamp").objectReferenceValue = light;
            so.FindProperty("intensity").floatValue = intensity;
            SerializedProperty glows = so.FindProperty("glows");
            glows.arraySize = 1;
            glows.GetArrayElementAtIndex(0).objectReferenceValue = gr;
            Color c = Hex(color);
            c.a = 0.8f;
            so.FindProperty("glowColor").colorValue = c;
            so.FindProperty("flicker").floatValue = flicker;
            so.ApplyModifiedPropertiesWithoutUndo();
            return fx;
        }

        static int DecorLamp(Transform house, string roomName, string decorName, float height, float intensity, float range, float glowSize)
        {
            Transform room = house.Find(roomName);
            Transform decor = room != null ? room.Find("Decor") : null;
            Transform holder = decor != null ? decor.Find(decorName) : null;
            if (holder == null) return 0;
            Lamp(AtmoRoot(room), decorName + "_Light", holder.position + Vector3.up * height, AtmosphereMode.RoomLamps, LampWarm, intensity, range, glowSize, 0f);
            return 1;
        }

        static bool FlashlightBeam()
        {
            GameObject player = GameObject.Find("Player");
            Transform spot = player != null ? FindDeep(player.transform, "Flashlight") : null;
            if (spot == null) return false;
            Transform old = spot.Find("FlashlightBeam");
            if (old != null) Undo.DestroyObjectImmediate(old.gameObject);
            LightBeam b = Beam(spot, "FlashlightBeam", spot.position, spot.rotation * Quaternion.Euler(-90f, 0f, 0f), AtmosphereMode.Always, "#FFF1D6", false, 0.09f, new Vector2(0.12f, 0.12f), new Vector2(3.2f, 3.2f), 4f);
            b.gameObject.AddComponent<ArtGenerated>();
            Undo.RegisterCreatedObjectUndo(b.gameObject, "Atmosphere");
            return true;
        }

        static bool SetupGrade(Camera cam)
        {
            if (cam == null) return false;
            ScreenGrade grade = cam.GetComponent<ScreenGrade>();
            if (grade == null) grade = Undo.AddComponent<ScreenGrade>(cam.gameObject);
            SerializedObject so = new SerializedObject(grade);
            so.FindProperty("shader").objectReferenceValue = Shader.Find("MovingIn/ScreenGrade");
            so.FindProperty("blackoutVignette").floatValue = 0.35f;
            so.ApplyModifiedPropertiesWithoutUndo();

            PixelCamera pixel = cam.GetComponent<PixelCamera>();
            if (pixel != null)
            {
                List<Component> comps = new List<Component>(cam.GetComponents<Component>());
                int g = comps.IndexOf(grade), p = comps.IndexOf(pixel);
                for (; g > p; g--) ComponentUtility.MoveComponentUp(grade);
            }
            return true;
        }

        static bool TuneBlackout()
        {
            bool done = false;
            TimeOfDay tod = Object.FindObjectOfType<TimeOfDay>(true);
            if (tod != null)
            {
                SerializedObject so = new SerializedObject(tod);
                SerializedProperty presets = so.FindProperty("presets");
                for (int i = 0; presets != null && i < presets.arraySize; i++)
                {
                    SerializedProperty p = presets.GetArrayElementAtIndex(i);
                    if (p.FindPropertyRelative("name").stringValue != "Blackout") continue;
                    p.FindPropertyRelative("sunColor").colorValue = Hex("#7D8CC8");
                    p.FindPropertyRelative("sunIntensity").floatValue = 0.3f;
                    p.FindPropertyRelative("ambient").colorValue = Hex("#2A2E48");
                    p.FindPropertyRelative("background").colorValue = Hex("#0C0E1C");
                    p.FindPropertyRelative("characterTint").colorValue = Hex("#9AA2C8");
                    done = true;
                }
                so.ApplyModifiedProperties();
            }

            GameObject player = GameObject.Find("Player");
            Transform glow = player != null ? FindDeep(player.transform, "PlayerGlow") : null;
            Light l = glow != null ? glow.GetComponent<Light>() : null;
            if (l != null)
            {
                Undo.RecordObject(l, "Atmosphere");
                l.intensity = 1.1f;
                l.range = 5.5f;
            }
            return done;
        }

        static void WindowPlain(Kit k, float w, float h)
        {
            k.Box(0f, h / 2f, 0f, w - 0.1f, h - 0.1f, 0.02f, Glass);
            k.Box(-w * 0.15f, h * 0.55f, -0.012f, 0.06f, h * 0.6f, 0.005f, "#E3F0F7", 0f, 0f, -35f);
            k.Box(0f, h - 0.035f, -0.01f, w, 0.07f, 0.07f, White);
            k.Box(0f, 0.035f, -0.01f, w, 0.07f, 0.07f, White);
            k.Box(-w / 2f + 0.035f, h / 2f, -0.01f, 0.07f, h, 0.07f, White);
            k.Box(w / 2f - 0.035f, h / 2f, -0.01f, 0.07f, h, 0.07f, White);
            k.Box(0f, h / 2f, -0.03f, 0.04f, h - 0.08f, 0.03f, White);
            k.Box(0f, -0.025f, -0.07f, w + 0.1f, 0.05f, 0.14f, White);
        }

        static void Lantern(Kit k)
        {
            const string Iron = "#3D434A";
            k.Box(0f, 0.17f, 0.005f, 0.14f, 0.3f, 0.02f, Iron);
            k.Box(0f, 0.16f, -0.07f, 0.13f, 0.18f, 0.12f, "#F7E9B5");
            k.Box(0f, 0.27f, -0.07f, 0.17f, 0.04f, 0.16f, Iron);
            k.Box(0f, 0.055f, -0.07f, 0.15f, 0.03f, 0.14f, Iron);
            k.Box(0f, 0.16f, -0.133f, 0.015f, 0.18f, 0.005f, Iron);
        }
    }
}
