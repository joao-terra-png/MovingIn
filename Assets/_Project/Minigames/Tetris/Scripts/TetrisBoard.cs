using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Tetris{
    public class TetrisBoard
    {
        public const int Empty = -1;

        readonly int[,] _cells;

        public int Width { get; }
        public int Height { get; }

        public TetrisBoard(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new int[width, height];
            Clear();
        }

        public int Get(int x, int y) => _cells[x, y];

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
                for (int y = 0; y < Height; y++)
                    _cells[x, y] = Empty;
        }

        public bool IsFree(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0) return false;
            if (y >= Height) return true;
            return _cells[x, y] == Empty;
        }

        public bool Fits(Vector2Int[] shape, Vector2Int origin)
        {
            foreach (Vector2Int c in shape)
                if (!IsFree(origin.x + c.x, origin.y + c.y)) return false;
            return true;
        }

        public bool Place(Vector2Int[] shape, Vector2Int origin, int type)
        {
            bool inside = true;
            foreach (Vector2Int c in shape)
            {
                int x = origin.x + c.x;
                int y = origin.y + c.y;
                if (y >= Height) inside = false;
                else _cells[x, y] = type;
            }
            return inside;
        }

        public bool RowIsFull(int y)
        {
            for (int x = 0; x < Width; x++)
                if (_cells[x, y] == Empty) return false;
            return true;
        }

        public void FindFullRows(List<int> rows)
        {
            rows.Clear();
            for (int y = 0; y < Height; y++)
                if (RowIsFull(y)) rows.Add(y);
        }

        public int RemoveFullRows()
        {
            int write = 0;
            for (int read = 0; read < Height; read++)
            {
                if (RowIsFull(read)) continue;
                if (write != read)
                    for (int x = 0; x < Width; x++)
                        _cells[x, write] = _cells[x, read];
                write++;
            }

            int removed = Height - write;
            for (int y = write; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    _cells[x, y] = Empty;
            return removed;
        }
    }
}
