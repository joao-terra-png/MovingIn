using System.Collections;
using System.Collections.Generic;
using MovingIn.Core;
using MovingIn.Minigames.Kitchen.ADTs;
using UnityEngine;

namespace MovingIn.Minigames.Tetris{
    public enum TetrisState { Playing, Clearing, Over }

    public class TetrisGame : MonoBehaviour
    {
        public const int Width = 10;
        public const int VisibleHeight = 20;
        public const int PreviewCount = 3;

        static readonly Vector2Int[] Kicks =
        {
            new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(1, 0),
            new Vector2Int(0, 1), new Vector2Int(-2, 0), new Vector2Int(2, 0)
        };

        [SerializeField] MinigameScreens screens;

        [Header("Velocidade")]
        [SerializeField] float _startFall = 0.8f;
        [SerializeField] float _minFall = 0.1f;
        [SerializeField, Range(0.5f, 1f)] float _speedUp = 0.85f;
        [SerializeField, Min(1)] int _linesPerLevel = 5;
        [SerializeField] float _softFall = 0.04f;

        [Header("Controles")]
        [SerializeField] float _lockDelay = 0.5f;
        [SerializeField] int _maxLockResets = 12;
        [SerializeField] float _repeatDelay = 0.17f;
        [SerializeField] float _repeatRate = 0.05f;
        [SerializeField] float _clearTime = 0.3f;

        [Header("Pontos")]
        [SerializeField] int[] _lineScores = { 0, 100, 300, 500, 800 };
        [SerializeField] int _softDropPoints = 1;
        [SerializeField] int _hardDropPoints = 2;

        public TetrisBoard Board { get; private set; }
        public TetrisState State { get; private set; }
        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Level => Lines / _linesPerLevel + 1;
        public int Goal => screens != null ? screens.Goal : 0;
        public int CurrentType { get; private set; }
        public Vector2Int CurrentPosition => _position;
        public Vector2Int[] CurrentCells => TetrisPieces.Cells(CurrentType, _rotation);
        public bool HasPiece => State == TetrisState.Playing;
        public bool ToppedOut { get; private set; }
        public List<int> ClearingRows => _fullRows;
        public float ClearProgress => _clearTime > 0f ? Mathf.Clamp01(_clearTimer / _clearTime) : 1f;

        StaticQueue<int> _next;
        readonly int[] _bag = new int[TetrisPieces.Count];
        readonly List<int> _fullRows = new List<int>();
        Vector2Int _position;
        int _rotation;
        float _fallTimer;
        float _lockTimer;
        int _lockResets;
        float _moveTimer;
        int _moveDir;
        float _clearTimer;

        float FallInterval => Mathf.Max(_minFall, _startFall * Mathf.Pow(_speedUp, Level - 1));

        void Awake()
        {
            Board = new TetrisBoard(Width, VisibleHeight + 2);
            _next = new StaticQueue<int>(TetrisPieces.Count * 2);
            Refill();
            SpawnNext();
        }

        public int NextType(int index)
        {
            return _next.ElementAt(index, out int type) ? type : 0;
        }

        public Vector2Int GhostPosition()
        {
            Vector2Int p = _position;
            while (Board.Fits(CurrentCells, p + Vector2Int.down)) p.y--;
            return p;
        }

        void Update()
        {
            if (State != TetrisState.Playing || Time.timeScale == 0f) return;
            if (screens != null && !screens.IsPlaying) return;

            ReadInput();
            if (State == TetrisState.Playing) Fall();
        }

        void ReadInput()
        {
            bool leftDown = Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A);
            bool rightDown = Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D);
            int held = 0;
            if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) held--;
            if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) held++;

            if (leftDown || rightDown)
            {
                _moveDir = leftDown ? -1 : 1;
                _moveTimer = _repeatDelay;
                TryMove(new Vector2Int(_moveDir, 0));
            }
            else if (held != 0 && held == _moveDir)
            {
                _moveTimer -= Time.deltaTime;
                if (_moveTimer <= 0f)
                {
                    _moveTimer = _repeatRate;
                    TryMove(new Vector2Int(_moveDir, 0));
                }
            }
            else
            {
                _moveDir = held;
                _moveTimer = _repeatDelay;
            }

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.X)) TryRotate(1);
            if (Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.Q)) TryRotate(-1);
            if (Input.GetKeyDown(KeyCode.Space)) HardDrop();
        }

        void Fall()
        {
            bool soft = Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
            float interval = soft ? Mathf.Min(FallInterval, _softFall) : FallInterval;

            if (Board.Fits(CurrentCells, _position + Vector2Int.down))
            {
                _lockTimer = 0f;
                _fallTimer += Time.deltaTime;
                if (_fallTimer < interval) return;
                _fallTimer = 0f;
                _position.y--;
                if (soft) Score += _softDropPoints;
                return;
            }

            _lockTimer += Time.deltaTime;
            if (_lockTimer >= _lockDelay) Lock();
        }

        bool TryMove(Vector2Int offset)
        {
            if (!Board.Fits(CurrentCells, _position + offset)) return false;
            _position += offset;
            AfterMove();
            return true;
        }

        void TryRotate(int direction)
        {
            int rotation = _rotation + direction;
            Vector2Int[] cells = TetrisPieces.Cells(CurrentType, rotation);
            foreach (Vector2Int kick in Kicks)
            {
                if (!Board.Fits(cells, _position + kick)) continue;
                _rotation = rotation;
                _position += kick;
                AfterMove();
                return;
            }
        }

        void AfterMove()
        {
            if (Board.Fits(CurrentCells, _position + Vector2Int.down)) return;
            if (_lockResets >= _maxLockResets) return;
            _lockResets++;
            _lockTimer = 0f;
        }

        void HardDrop()
        {
            int distance = 0;
            while (Board.Fits(CurrentCells, _position + Vector2Int.down))
            {
                _position.y--;
                distance++;
            }
            Score += distance * _hardDropPoints;
            Lock();
        }

        void Lock()
        {
            Board.Place(CurrentCells, _position, CurrentType);
            Board.FindFullRows(_fullRows);
            if (_fullRows.Count > 0) StartCoroutine(ClearRows());
            else AfterLock();
        }

        IEnumerator ClearRows()
        {
            State = TetrisState.Clearing;
            _clearTimer = 0f;
            while (_clearTimer < _clearTime)
            {
                _clearTimer += Time.deltaTime;
                yield return null;
            }

            int removed = Board.RemoveFullRows();
            Lines += removed;
            Score += _lineScores[Mathf.Min(removed, _lineScores.Length - 1)];
            _fullRows.Clear();
            State = TetrisState.Playing;
            AfterLock();
        }

        void AfterLock()
        {
            if (Goal > 0 && Score >= Goal)
            {
                Finish();
                return;
            }
            SpawnNext();
        }

        void SpawnNext()
        {
            _next.Remove(out int type);
            Refill();

            CurrentType = type;
            _rotation = 0;
            int size = TetrisPieces.Size(type);
            _position = new Vector2Int((Width - size) / 2, VisibleHeight - size + 1);
            _fallTimer = 0f;
            _lockTimer = 0f;
            _lockResets = 0;

            if (!Board.Fits(CurrentCells, _position))
            {
                ToppedOut = true;
                Finish();
            }
        }

        void Refill()
        {
            while (_next.Quantity <= PreviewCount)
            {
                for (int i = 0; i < _bag.Length; i++) _bag[i] = i;
                for (int i = _bag.Length - 1; i > 0; i--)
                {
                    int j = Random.Range(0, i + 1);
                    (_bag[i], _bag[j]) = (_bag[j], _bag[i]);
                }
                for (int i = 0; i < _bag.Length; i++) _next.Insert(_bag[i]);
            }
        }

        void Finish()
        {
            State = TetrisState.Over;
            if (screens != null) screens.ShowResult(Score);
            else Debug.Log($"[TetrisGame] Fim de jogo: {Score} pontos.");
        }
    }
}
