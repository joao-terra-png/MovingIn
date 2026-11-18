using UnityEngine;

namespace MovingIn.Minigames.Tetris{
    public static class TetrisPieces
    {
        public const int Count = 7;

        static readonly int[] Sizes = { 4, 2, 3, 3, 3, 3, 3 };

        static readonly Vector2Int[][] Spawn =
        {
            new[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2), new Vector2Int(3, 2) },
            new[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 2) },
            new[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 2), new Vector2Int(2, 2) },
            new[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new[] { new Vector2Int(0, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
            new[] { new Vector2Int(2, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1) },
        };

        static readonly Vector2Int[][][] Rotations = BuildRotations();

        public static int Size(int type) => Sizes[type];

        public static Vector2Int[] Cells(int type, int rotation) => Rotations[type][((rotation % 4) + 4) % 4];

        static Vector2Int[][][] BuildRotations()
        {
            Vector2Int[][][] all = new Vector2Int[Count][][];
            for (int type = 0; type < Count; type++)
            {
                all[type] = new Vector2Int[4][];
                all[type][0] = Spawn[type];
                for (int r = 1; r < 4; r++)
                    all[type][r] = RotateClockwise(all[type][r - 1], Sizes[type]);
            }
            return all;
        }

        static Vector2Int[] RotateClockwise(Vector2Int[] cells, int size)
        {
            Vector2Int[] result = new Vector2Int[cells.Length];
            for (int i = 0; i < cells.Length; i++)
                result[i] = new Vector2Int(cells[i].y, size - 1 - cells[i].x);
            return result;
        }
    }
}
