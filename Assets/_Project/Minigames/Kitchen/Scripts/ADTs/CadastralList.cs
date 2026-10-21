using System.Collections.Generic;

namespace MovingIn.Minigames.Kitchen.ADTs
{
    public class CadastralList<T>
    {
        private readonly T[] elements;
        private int quantity;
        private int cursor;

        public CadastralList(int capacity)
        {
            elements = new T[capacity];
            quantity = 0;
            cursor = -1;
        }

        public bool Empty() => quantity == 0;
        public bool Full() => quantity == elements.Length;
        public int Quantity => quantity;

        public bool Insert(T x)
        {
            if(Full()) return false;
            if(IndexOf(x) != -1) return false;

            elements[quantity] = x;
            quantity++;
            return true;
        }

        public bool Remove(T x)
        {
            int pos = IndexOf(x);
            if(pos == -1) return false;

            for(int i = pos; i < quantity - 1; i++)
            {
                elements[i] = elements[i + 1];
            }

            elements[quantity - 1] = default;
            quantity--;
            return true;

        }

        public bool Contains(T x) => IndexOf(x) != -1;

        public bool GetFirst(out T x)
        {
            if(Empty())
            {
                x = default;
                return false;
            }else
            {
                cursor = 0;
                x = elements[cursor];
                return true;
            }


        }

        public bool GetNext(out T x)
        {
            cursor++;

            if(cursor >= quantity)
            {
                x = default;
                return false;
            }else
            {
                x = elements[cursor];
                return true;
            }
        }

        private int IndexOf(T x)
        {
            for(int i = 0; i < quantity; i++)
            {
                if(EqualityComparer<T>.Default.Equals(elements[i], x))
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
