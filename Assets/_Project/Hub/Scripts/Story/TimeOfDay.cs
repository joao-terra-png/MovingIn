using System;
using System.Collections;
using UnityEngine;

namespace MovingIn.Hub{
    [Serializable]
    public class LightPreset
    {
        public string name = "Day";
        public Color sunColor = Color.white;
        public float sunIntensity = 1f;
        public Vector3 sunRotation = new Vector3(50f, -30f, 0f);
        public Color ambient = new Color(0.5f, 0.5f, 0.5f);
        public Color background = Color.black;
        [Range(0f, 1f)] public float roomLights = 1f;
        public Color characterTint = Color.white;
    }

    public class TimeOfDay : MonoBehaviour
    {
        [SerializeField] Light sun;
        [SerializeField] Camera cam;
        [SerializeField] Light[] roomLights;
        [SerializeField] SpriteRenderer[] tinted;
        [SerializeField] LightPreset[] presets;

        float[] _roomBase;
        float _roomFactor = 1f;
        Coroutine _blend;

        public static TimeOfDay Instance { get; private set; }
        public string Current { get; private set; }
        public float RoomFactor => _roomFactor;
        public float SunIntensity => sun != null ? sun.intensity : 1f;
        public Color SunColor => sun != null ? sun.color : Color.white;

        void Awake()
        {
            Instance = this;
            if (cam == null) cam = Camera.main;
            _roomBase = new float[roomLights.Length];
            for (int i = 0; i < roomLights.Length; i++)
                _roomBase[i] = roomLights[i] != null ? roomLights[i].intensity : 1f;
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            if (cam != null) cam.clearFlags = CameraClearFlags.SolidColor;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Apply(string presetName, float duration)
        {
            LightPreset target = Find(presetName);
            if (target == null)
            {
                Debug.LogWarning($"[TimeOfDay] Preset '{presetName}' nao existe.");
                return;
            }
            Current = presetName;
            if (_blend != null) StopCoroutine(_blend);
            if (duration <= 0f) Set(Snapshot(), target, 1f);
            else _blend = StartCoroutine(Blend(Snapshot(), target, duration));
        }

        public IEnumerator Flicker(int times = 3)
        {
            float keep = _roomFactor;
            for (int i = 0; i < times; i++)
            {
                SetRoomLights(0f);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.05f, 0.12f));
                SetRoomLights(keep);
                yield return new WaitForSeconds(UnityEngine.Random.Range(0.08f, 0.25f));
            }
        }

        LightPreset Find(string presetName)
        {
            foreach (LightPreset p in presets)
                if (p != null && p.name == presetName) return p;
            return null;
        }

        LightPreset Snapshot()
        {
            return new LightPreset
            {
                name = "snapshot",
                sunColor = sun != null ? sun.color : Color.white,
                sunIntensity = sun != null ? sun.intensity : 1f,
                sunRotation = sun != null ? sun.transform.eulerAngles : Vector3.zero,
                ambient = RenderSettings.ambientLight,
                background = cam != null ? cam.backgroundColor : Color.black,
                roomLights = _roomFactor,
                characterTint = tinted != null && tinted.Length > 0 && tinted[0] != null ? tinted[0].color : Color.white,
            };
        }

        IEnumerator Blend(LightPreset from, LightPreset to, float duration)
        {
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                Set(from, to, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            Set(from, to, 1f);
            _blend = null;
        }

        void Set(LightPreset a, LightPreset b, float k)
        {
            if (sun != null)
            {
                sun.color = Color.Lerp(a.sunColor, b.sunColor, k);
                sun.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, k);
                sun.transform.rotation = Quaternion.Slerp(Quaternion.Euler(a.sunRotation), Quaternion.Euler(b.sunRotation), k);
            }
            RenderSettings.ambientLight = Color.Lerp(a.ambient, b.ambient, k);
            if (cam != null) cam.backgroundColor = Color.Lerp(a.background, b.background, k);
            SetRoomLights(Mathf.Lerp(a.roomLights, b.roomLights, k));
            Color tint = Color.Lerp(a.characterTint, b.characterTint, k);
            foreach (SpriteRenderer sr in tinted)
                if (sr != null) sr.color = tint;
        }

        void SetRoomLights(float factor)
        {
            _roomFactor = factor;
            for (int i = 0; i < roomLights.Length; i++)
            {
                Light l = roomLights[i];
                if (l == null) continue;
                l.intensity = _roomBase[i] * factor;
                l.enabled = factor > 0.01f;
            }
        }
    }
}
