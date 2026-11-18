using UnityEngine;

namespace MovingIn.Hub{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class LightBeam : MonoBehaviour
    {
        static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] AtmosphereMode mode = AtmosphereMode.Sunlit;
        [SerializeField] Color color = new Color(1f, 0.9f, 0.7f, 1f);
        [SerializeField] bool useSunColor;
        [SerializeField, Range(0f, 1f)] float strength = 0.25f;
        [SerializeField] Vector2 topSize = new Vector2(1f, 0.3f);
        [SerializeField] Vector2 bottomSize = new Vector2(2f, 1.2f);
        [SerializeField] float length = 2.5f;

        Mesh _mesh;
        MaterialPropertyBlock _block;
        float _weight;

        void OnEnable()
        {
            Build();
            _weight = Atmosphere.Weight(mode);
            Apply();
        }

        void OnDisable()
        {
            if (_mesh == null) return;
            if (Application.isPlaying) Destroy(_mesh);
            else DestroyImmediate(_mesh);
            _mesh = null;
        }

        void OnValidate()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null || !isActiveAndEnabled) return;
                Build();
                Apply();
            };
#endif
        }

        void Update()
        {
            if (!Application.isPlaying) return;
            _weight = Mathf.MoveTowards(_weight, Atmosphere.Weight(mode), Time.deltaTime * 1.5f);
            Apply();
        }

        void Apply()
        {
            MeshRenderer r = GetComponent<MeshRenderer>();
            if (_block == null) _block = new MaterialPropertyBlock();
            Color c = useSunColor && TimeOfDay.Instance != null ? TimeOfDay.Instance.SunColor : color;
            c.a = strength * _weight;
            _block.SetColor(ColorId, c);
            r.SetPropertyBlock(_block);
            r.enabled = _weight > 0.01f;
        }

        void Build()
        {
            if (_mesh == null)
            {
                _mesh = new Mesh { name = "LightBeam" };
                _mesh.hideFlags = HideFlags.HideAndDontSave;
            }
            float tx = topSize.x / 2f, tz = topSize.y / 2f, bx = bottomSize.x / 2f, bz = bottomSize.y / 2f, y = -length;
            Vector3[] top = { new Vector3(-tx, 0f, -tz), new Vector3(tx, 0f, -tz), new Vector3(tx, 0f, tz), new Vector3(-tx, 0f, tz) };
            Vector3[] bottom = { new Vector3(-bx, y, -bz), new Vector3(bx, y, -bz), new Vector3(bx, y, bz), new Vector3(-bx, y, bz) };
            Vector3[] v = new Vector3[16];
            Vector2[] uv = new Vector2[16];
            int[] tris = new int[24];
            for (int s = 0; s < 4; s++)
            {
                int n = (s + 1) % 4, i = s * 4;
                v[i] = top[s]; v[i + 1] = top[n]; v[i + 2] = bottom[n]; v[i + 3] = bottom[s];
                uv[i] = new Vector2(0f, 0f); uv[i + 1] = new Vector2(1f, 0f); uv[i + 2] = new Vector2(1f, 1f); uv[i + 3] = new Vector2(0f, 1f);
                int t = s * 6;
                tris[t] = i; tris[t + 1] = i + 1; tris[t + 2] = i + 2;
                tris[t + 3] = i; tris[t + 4] = i + 2; tris[t + 5] = i + 3;
            }
            _mesh.Clear();
            _mesh.vertices = v;
            _mesh.uv = uv;
            _mesh.triangles = tris;
            _mesh.RecalculateBounds();
            GetComponent<MeshFilter>().sharedMesh = _mesh;
        }
    }
}
