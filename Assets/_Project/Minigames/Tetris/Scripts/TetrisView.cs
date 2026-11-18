using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Tetris{
    public class TetrisView : MonoBehaviour
    {
        [SerializeField] TetrisGame game;

        [Header("Caixas")]
        [SerializeField] SpriteRenderer blockPrefab;
        [SerializeField] Sprite[] boxSprites = new Sprite[TetrisPieces.Count];

        [Header("Objetos da cena")]
        [SerializeField] Transform cells;
        [SerializeField] Transform[] nextSlots = new Transform[TetrisGame.PreviewCount];
        [SerializeField] TMP_Text scoreText;
        [SerializeField] TMP_Text levelText;
        [SerializeField] TMP_Text linesText;
        [SerializeField] Transform goalFill;

        [Header("Aparência")]
        [SerializeField, Range(0f, 1f)] float ghostAlpha = 0.25f;
        [SerializeField] Color clearColor = new Color(1f, 0.95f, 0.8f);
        [SerializeField] Color topOutColor = new Color(0.5f, 0.5f, 0.5f);

        SpriteRenderer[,] _board;
        SpriteRenderer[] _piece;
        SpriteRenderer[] _ghost;
        SpriteRenderer[,] _next;
        float _goalHeight;

        void Start()
        {
            _board = new SpriteRenderer[TetrisGame.Width, TetrisGame.VisibleHeight];
            for (int x = 0; x < TetrisGame.Width; x++)
                for (int y = 0; y < TetrisGame.VisibleHeight; y++)
                    _board[x, y] = NewBlock(cells, CellCenter(x, y), 0);

            _piece = new SpriteRenderer[4];
            _ghost = new SpriteRenderer[4];
            for (int i = 0; i < 4; i++)
            {
                _ghost[i] = NewBlock(cells, Vector3.zero, -1);
                _ghost[i].color = new Color(1f, 1f, 1f, ghostAlpha);
                _piece[i] = NewBlock(cells, Vector3.zero, 1);
            }

            _next = new SpriteRenderer[nextSlots.Length, 4];
            for (int p = 0; p < nextSlots.Length; p++)
                for (int i = 0; i < 4; i++)
                    _next[p, i] = NewBlock(nextSlots[p], Vector3.zero, 0);

            _goalHeight = goalFill.localScale.y;
        }

        void LateUpdate()
        {
            DrawBoard();
            DrawPiece();
            DrawNext();
            DrawHud();
        }

        void DrawBoard()
        {
            bool clearing = game.State == TetrisState.Clearing;
            bool blinkOff = clearing && ((int)(game.ClearProgress * 6f)) % 2 == 0;

            for (int x = 0; x < TetrisGame.Width; x++)
                for (int y = 0; y < TetrisGame.VisibleHeight; y++)
                {
                    SpriteRenderer block = _board[x, y];
                    int type = game.Board.Get(x, y);
                    bool rowClearing = clearing && game.ClearingRows.Contains(y);

                    block.enabled = type != TetrisBoard.Empty && !(rowClearing && blinkOff);
                    if (!block.enabled) continue;

                    block.sprite = boxSprites[type];
                    if (rowClearing) block.color = clearColor;
                    else if (game.ToppedOut) block.color = topOutColor;
                    else block.color = Color.white;
                }
        }

        void DrawPiece()
        {
            bool show = game.HasPiece;
            Vector2Int[] shape = game.CurrentCells;
            Vector2Int position = game.CurrentPosition;
            Vector2Int ghost = show ? game.GhostPosition() : position;
            Sprite sprite = boxSprites[game.CurrentType];

            for (int i = 0; i < 4; i++)
            {
                Place(_piece[i], sprite, position + shape[i], show);
                Place(_ghost[i], sprite, ghost + shape[i], show && ghost != position);
            }
        }

        void Place(SpriteRenderer block, Sprite sprite, Vector2Int cell, bool show)
        {
            block.enabled = show && cell.y < TetrisGame.VisibleHeight;
            if (!block.enabled) return;
            block.sprite = sprite;
            block.transform.localPosition = CellCenter(cell.x, cell.y);
        }

        void DrawNext()
        {
            for (int p = 0; p < nextSlots.Length; p++)
            {
                int type = game.NextType(p);
                Vector2Int[] shape = TetrisPieces.Cells(type, 0);

                Vector2 min = shape[0];
                Vector2 max = shape[0];
                foreach (Vector2Int c in shape)
                {
                    min = Vector2.Min(min, c);
                    max = Vector2.Max(max, c);
                }
                Vector2 middle = (min + max) / 2f;

                for (int i = 0; i < 4; i++)
                {
                    _next[p, i].enabled = true;
                    _next[p, i].sprite = boxSprites[type];
                    _next[p, i].transform.localPosition = (Vector2)shape[i] - middle;
                }
            }
        }

        void DrawHud()
        {
            scoreText.text = $"PONTOS\n<size=170%>{game.Score}</size>";
            levelText.text = $"NÍVEL\n<size=170%>{game.Level}</size>";
            linesText.text = $"LINHAS\n<size=170%>{game.Lines}</size>";

            float progress = game.Goal > 0 ? Mathf.Clamp01((float)game.Score / game.Goal) : 0f;
            Vector3 scale = goalFill.localScale;
            scale.y = _goalHeight * progress;
            goalFill.localScale = scale;
        }

        SpriteRenderer NewBlock(Transform parent, Vector3 localPosition, int orderOffset)
        {
            SpriteRenderer block = Instantiate(blockPrefab, parent);
            block.transform.localPosition = localPosition;
            block.sortingOrder += orderOffset;
            block.enabled = false;
            return block;
        }

        static Vector3 CellCenter(int x, int y) => new Vector3(x + 0.5f, y + 0.5f, 0f);
    }
}
