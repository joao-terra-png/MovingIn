using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    public class InteractHighlight : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");
        const string ShapeName = "OutlineShape";

        [SerializeField] Material outlineMaterial;
        [SerializeField] Material maskMaterial;
        [SerializeField] float _width = 0.06f;
        [SerializeField] Color _colorA = new Color(1f, 0.72f, 0.35f);
        [SerializeField] Color _colorB = new Color(1f, 0.95f, 0.78f);
        [SerializeField] float _pulseSpeed = 1.6f;

        readonly List<GameObject> _parts = new List<GameObject>();
        readonly List<MeshRenderer> _hulls = new List<MeshRenderer>();
        MaterialPropertyBlock _block;
        bool _on;

        public void SetHighlighted(bool on)
        {
            if (on && _parts.Count == 0) Build();
            _on = on && _parts.Count > 0;
            foreach (GameObject p in _parts)
                if (p != null) p.SetActive(_on);
        }

        public void Rebuild()
        {
            foreach (GameObject p in _parts)
                if (p != null) Destroy(p);
            _parts.Clear();
            _hulls.Clear();
        }

        void OnDisable()
        {
            foreach (GameObject p in _parts)
                if (p != null) p.SetActive(false);
            _on = false;
        }

        void Update()
        {
            if (!_on) return;
            float t = (Mathf.Sin(Time.unscaledTime * _pulseSpeed * Mathf.PI * 2f) + 1f) / 2f;
            _block.SetColor(ColorId, Color.Lerp(_colorA, _colorB, t));
            foreach (MeshRenderer r in _hulls)
                if (r != null) r.SetPropertyBlock(_block);
        }

        void Build()
        {
            if (outlineMaterial == null) return;
            _block = new MaterialPropertyBlock();

            List<MeshFilter> sources = new List<MeshFilter>();
            Transform model = transform.Find("Model");
            if (model != null)
            {
                foreach (MeshFilter mf in model.GetComponentsInChildren<MeshFilter>(true))
                    if (mf.name == ShapeName) sources.Add(mf);
                if (sources.Count == 0)
                    foreach (MeshFilter mf in model.GetComponentsInChildren<MeshFilter>())
                        if (IsVisible(mf)) sources.Add(mf);
            }
            else
            {
                MeshFilter own = GetComponent<MeshFilter>();
                if (own != null && IsVisible(own)) sources.Add(own);
            }

            foreach (MeshFilter mf in sources) AddHull(mf);
        }

        static bool IsVisible(MeshFilter mf)
        {
            MeshRenderer r = mf.GetComponent<MeshRenderer>();
            return r != null && r.enabled && mf.sharedMesh != null;
        }

        void AddHull(MeshFilter mf)
        {
            Mesh mesh = mf.sharedMesh;
            if (mesh == null) return;
            int subs = Mathf.Max(1, mesh.subMeshCount);

            Bounds b = mesh.bounds;
            Vector3 world = Vector3.Scale(b.size, mf.transform.lossyScale);
            Vector3 k = new Vector3(
                1f + 2f * _width / Mathf.Max(0.01f, Mathf.Abs(world.x)),
                1f + 2f * _width / Mathf.Max(0.01f, Mathf.Abs(world.y)),
                1f + 2f * _width / Mathf.Max(0.01f, Mathf.Abs(world.z)));

            GameObject hull = NewPart("Outline", mf.transform, mesh, outlineMaterial, subs);
            hull.transform.localPosition = Vector3.Scale(b.center, Vector3.one - k);
            hull.transform.localScale = k;
            _hulls.Add(hull.GetComponent<MeshRenderer>());

            if (maskMaterial != null) NewPart("OutlineMask", mf.transform, mesh, maskMaterial, subs);
        }

        GameObject NewPart(string name, Transform parent, Mesh mesh, Material mat, int subs)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.AddComponent<MeshFilter>().sharedMesh = mesh;
            MeshRenderer r = go.AddComponent<MeshRenderer>();
            Material[] mats = new Material[subs];
            for (int i = 0; i < subs; i++) mats[i] = mat;
            r.sharedMaterials = mats;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            go.SetActive(false);
            _parts.Add(go);
            return go;
        }
    }
}
