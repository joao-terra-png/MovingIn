using System;
using System.Collections.Generic;

namespace MovingIn.Minigames.Laundry.ADTs{
    public class MultiList<T>
    {
        private class Node
        {
            public T info;
            public int color;      
            public int type;       
            public Node nextColor; 
            public Node nextType;  
        }

        private readonly Node[] colorHeads;   
        private readonly Node[] typeHeads;    
        private readonly int[] colorCounts;   
        private readonly int[] typeCounts;    
        private int quantity;                 

        public MultiList(int colorCount, int typeCount)
        {
            colorHeads = new Node[colorCount];
            typeHeads = new Node[typeCount];
            colorCounts = new int[colorCount];
            typeCounts = new int[typeCount];
            quantity = 0;
        }

        public bool Empty() => quantity == 0;
        public int Quantity => quantity;
        public int CountByColor(int color) => ValidColor(color) ? colorCounts[color] : 0;
        public int CountByType(int type) => ValidType(type) ? typeCounts[type] : 0;

        public bool Insert(T x, int color, int type)
        {
            if (!ValidColor(color) || !ValidType(type)) return false;
            if (Contains(x)) return false;

            Node node = new Node { info = x, color = color, type = type };

            node.nextColor = colorHeads[color];   
            colorHeads[color] = node;

            node.nextType = typeHeads[type];      
            typeHeads[type] = node;

            colorCounts[color]++;
            typeCounts[type]++;
            quantity++;
            return true;
        }

        public bool Remove(T x)
        {
            for (int c = 0; c < colorHeads.Length; c++)
            {
                Node prev = null;
                Node cur = colorHeads[c];
                while (cur != null && !Same(cur.info, x))
                {
                    prev = cur;
                    cur = cur.nextColor;
                }
                if (cur == null) continue;   

                if (prev == null) colorHeads[c] = cur.nextColor;
                else prev.nextColor = cur.nextColor;

                UnlinkFromType(cur);

                colorCounts[cur.color]--;
                typeCounts[cur.type]--;
                quantity--;
                return true;   
            }
            return false;
        }

        public bool Contains(T x) => Find(x) != null;

        public void ForEachByColor(int color, Action<T> action)
        {
            if (!ValidColor(color)) return;
            for (Node n = colorHeads[color]; n != null; n = n.nextColor)
                action(n.info);
        }

        public void ForEachByType(int type, Action<T> action)
        {
            if (!ValidType(type)) return;
            for (Node n = typeHeads[type]; n != null; n = n.nextType)
                action(n.info);
        }

        void UnlinkFromType(Node target)
        {
            Node prev = null;
            Node cur = typeHeads[target.type];
            while (cur != null && cur != target)
            {
                prev = cur;
                cur = cur.nextType;
            }
            if (cur == null) return;

            if (prev == null) typeHeads[target.type] = cur.nextType;
            else prev.nextType = cur.nextType;
        }

        Node Find(T x)
        {
            for (int c = 0; c < colorHeads.Length; c++)
                for (Node n = colorHeads[c]; n != null; n = n.nextColor)
                    if (Same(n.info, x)) return n;
            return null;
        }

        bool Same(T a, T b) => EqualityComparer<T>.Default.Equals(a, b);
        bool ValidColor(int c) => c >= 0 && c < colorHeads.Length;
        bool ValidType(int t) => t >= 0 && t < typeHeads.Length;
    }
}
