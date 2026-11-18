using UnityEngine;

namespace MovingIn.Hub{
    [ExecuteAlways]
    [RequireComponent(typeof(Renderer))]
    public class TextureTiling : MonoBehaviour
    {
        static readonly int MainTexSt = Shader.PropertyToID("_MainTex_ST");

        [SerializeField] Vector2 _tiling = Vector2.one;
        [SerializeField] Vector2 _offset = Vector2.zero;

        MaterialPropertyBlock _block;

        public void Set(Vector2 tiling)
        {
            _tiling = tiling;
            Apply();
        }

        void OnEnable()
        {
            Apply();
        }

        void OnValidate()
        {
            Apply();
        }

        void Apply()
        {
            Renderer r = GetComponent<Renderer>();
            if (r == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();
            r.GetPropertyBlock(_block);
            _block.SetVector(MainTexSt, new Vector4(_tiling.x, _tiling.y, _offset.x, _offset.y));
            r.SetPropertyBlock(_block);
        }
    }
}
