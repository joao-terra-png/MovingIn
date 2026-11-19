using System;
using System.Collections;
using UnityEngine;
using MovingIn.Minigames.Laundry.ADTs;

namespace MovingIn.Minigames.Laundry{
    public class LaundryManager : MonoBehaviour
    {
        [Header("Roupas")]
        [SerializeField] ClothPiece piecePrefab;
        [SerializeField] Transform pilePoint;
        [SerializeField, Min(1)] int _pieceCount = 12;

        [Tooltip("Sprites por tipo, na ordem Camiseta, Calca, Meia (desenhe em branco/cinza). Vazio = quadrado provisorio")]
        [SerializeField] Sprite[] typeSprites = new Sprite[ClothNames.TypeCount];

        [Header("Cores das roupas")]
        [SerializeField] Color _whiteTint = new Color(0.95f, 0.95f, 0.95f);
        [SerializeField] Color _darkTint = new Color(0.22f, 0.24f, 0.32f);
        [SerializeField] Color[] coloredTints =
        {
            new Color(0.90f, 0.30f, 0.30f),
            new Color(0.30f, 0.70f, 0.35f),
            new Color(0.95f, 0.80f, 0.25f),
            new Color(0.30f, 0.55f, 0.90f)
        };

        [Header("Varal (uma fileira por cor: Brancas, Escuras, Coloridas)")]
        [SerializeField] Transform[] lineRows = new Transform[ClothNames.ColorCount];
        [SerializeField] float _lineSpacing = 1f;

        [Header("Regras")]
        [SerializeField] int _hitPoints = 10;
        [SerializeField] int _missPoints = 5;
        [SerializeField] int _hintCost = 2;
        [SerializeField] float _washTime = 1.5f;

        public event Action<int> PointsChanged;
        public event Action<LaundryPhase> PhaseChanged;
        public event Action CountsChanged;
        public event Action Victory;

        public LaundryPhase Phase { get; private set; }
        public int Points => _points;
        public int PileRemaining => _pile.Length - _pileIndex + (_current != null ? 1 : 0);
        public int CountByColor(ClothColor c) => _clothes.CountByColor((int)c);
        public int CountByType(ClothType t) => _clothes.CountByType((int)t);

        MultiList<ClothPiece> _clothes;      
        (ClothColor color, ClothType type)[] _pile;
        int _pileIndex;
        ClothPiece _current;
        int _points;

        void Awake()
        {
            _clothes = new MultiList<ClothPiece>(ClothNames.ColorCount, ClothNames.TypeCount);
            BuildPile();
        }

        void Start()
        {
            SetPhase(LaundryPhase.Wash);
            SpawnNext();
        }

        void BuildPile()
        {
            _pile = new (ClothColor, ClothType)[_pieceCount];
            for (int i = 0; i < _pile.Length; i++)
            {
                var color = (ClothColor)UnityEngine.Random.Range(0, ClothNames.ColorCount);
                var type = (ClothType)UnityEngine.Random.Range(0, ClothNames.TypeCount);
                _pile[i] = (color, type);
            }
        }

        void SpawnNext()
        {
            if (_pileIndex >= _pile.Length)
            {
                _current = null;
                CountsChanged?.Invoke();
                StartCoroutine(WashRoutine());
                return;
            }

            var (color, type) = _pile[_pileIndex];
            _pileIndex++;

            _current = Instantiate(piecePrefab, pilePoint.position, Quaternion.identity);
            _current.Setup(color, type, SpriteFor(type), PlaceholderSize(type), TintFor(color), this);
            _current.Order = 20;
            CountsChanged?.Invoke();
        }

        public bool PieceDropped(ClothPiece piece, LaundryZone zone)
        {
            if (zone == null) return false;
            if (zone.ActivePhase != Phase) return false;   

            if (!zone.Accepts(piece))
            {
                _points = Mathf.Max(0, _points - _missPoints);
                PointsChanged?.Invoke(_points);
                piece.Flash(Color.red);
                return false;
            }

            _points += _hitPoints;
            PointsChanged?.Invoke(_points);

            if (Phase == LaundryPhase.Wash)
            {
                _clothes.Insert(piece, (int)piece.ClothColor, (int)piece.ClothType);
                piece.transform.position = zone.transform.position;
                piece.SetVisible(false);   
                _current = null;
                SpawnNext();
            }
            else if (Phase == LaundryPhase.Store)
            {
                _clothes.Remove(piece);
                piece.SetClickable(false);
                piece.MoveTo(zone.transform.position);
                Destroy(piece.gameObject, 0.25f);   

                CountsChanged?.Invoke();
                LayoutLine();                        

                if (_clothes.Empty())
                {
                    SetPhase(LaundryPhase.Done);
                    Victory?.Invoke();
                }
            }
            return true;
        }

        public void ShowHint(int type)
        {
            if (Phase != LaundryPhase.Store) return;
            if (_clothes.CountByType(type) == 0) return;

            _points = Mathf.Max(0, _points - _hintCost);
            PointsChanged?.Invoke(_points);

            _clothes.ForEachByType(type, piece => piece.Flash(Color.yellow));
        }

        IEnumerator WashRoutine()
        {
            SetPhase(LaundryPhase.Washing);
            yield return new WaitForSeconds(_washTime);
            LayoutLine();
            SetPhase(LaundryPhase.Store);
        }

        void LayoutLine()
        {
            for (int c = 0; c < ClothNames.ColorCount; c++)
            {
                if (lineRows[c] == null) continue;
                Vector3 start = lineRows[c].position;
                int i = 0;
                _clothes.ForEachByColor(c, piece =>
                {
                    piece.SetVisible(true);
                    piece.Order = 20 + i;
                    piece.MoveTo(start + Vector3.right * (_lineSpacing * i));
                    i++;
                });
            }
        }

        void SetPhase(LaundryPhase phase)
        {
            Phase = phase;
            PhaseChanged?.Invoke(phase);
        }

        Sprite SpriteFor(ClothType type)
        {
            int t = (int)type;
            return typeSprites != null && t < typeSprites.Length ? typeSprites[t] : null;
        }

        Color TintFor(ClothColor color)
        {
            switch (color)
            {
                case ClothColor.White: return _whiteTint;
                case ClothColor.Dark: return _darkTint;
                default:
                    if (coloredTints == null || coloredTints.Length == 0) return Color.red;
                    return coloredTints[UnityEngine.Random.Range(0, coloredTints.Length)];
            }
        }
        
        static Vector2 PlaceholderSize(ClothType type)
        {
            switch (type)
            {
                case ClothType.Shirt: return new Vector2(0.9f, 0.8f);
                case ClothType.Pants: return new Vector2(0.55f, 1.0f);
                default: return new Vector2(0.4f, 0.5f);
            }
        }
    }
}
