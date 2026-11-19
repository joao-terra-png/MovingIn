using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    public struct ClothPart
    {
        public Vector2 center;
        public Vector2 size;
        public float angle;
        public bool detail;

        public ClothPart(float x, float y, float w, float h, float angle = 0f, bool detail = false)
        {
            center = new Vector2(x, y);
            size = new Vector2(w, h);
            this.angle = angle;
            this.detail = detail;
        }
    }

    public static class ClothShapes
    {
        static readonly ClothPart[] Shirt =
        {
            new ClothPart(-0.36f, 0.14f, 0.30f, 0.22f, 30f),
            new ClothPart(0.36f, 0.14f, 0.30f, 0.22f, -30f),
            new ClothPart(0f, 0.12f, 0.58f, 0.36f),
            new ClothPart(0f, -0.13f, 0.54f, 0.56f),
            new ClothPart(0f, 0.27f, 0.20f, 0.07f, 0f, true),
            new ClothPart(0f, -0.38f, 0.54f, 0.05f, 0f, true),
        };

        static readonly ClothPart[] Pants =
        {
            new ClothPart(0f, 0.30f, 0.54f, 0.22f),
            new ClothPart(-0.15f, -0.10f, 0.24f, 0.70f, -3f),
            new ClothPart(0.15f, -0.10f, 0.24f, 0.70f, 3f),
            new ClothPart(0f, 0.44f, 0.54f, 0.09f, 0f, true),
            new ClothPart(-0.167f, -0.42f, 0.25f, 0.06f, -3f, true),
            new ClothPart(0.167f, -0.42f, 0.25f, 0.06f, 3f, true),
        };

        static readonly ClothPart[] Sock =
        {
            new ClothPart(-0.06f, 0.07f, 0.20f, 0.36f),
            new ClothPart(0.04f, -0.13f, 0.40f, 0.17f),
            new ClothPart(-0.06f, 0.25f, 0.22f, 0.08f, 0f, true),
            new ClothPart(0.20f, -0.13f, 0.09f, 0.17f, 0f, true),
        };

        public static ClothPart[] For(ClothType type)
        {
            switch (type)
            {
                case ClothType.Shirt: return Shirt;
                case ClothType.Pants: return Pants;
                default: return Sock;
            }
        }
    }
}
