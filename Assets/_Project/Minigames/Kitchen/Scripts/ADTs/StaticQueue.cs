using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen.ADTs
{
    public class StaticQueue<T>
    {
        private readonly T[] elements;
        private int first;
        private int last;
        private int quantity;

        public StaticQueue(int capacity)
        {
            elements = new T[capacity];
            first = 0;
            last = elements.Length - 1;
            quantity = 0;
        }

        public bool Empty() => quantity == 0;
        public bool Full() => quantity == elements.Length - 1;
        public int Quantity => quantity;
        
        public bool Insert(T x)
        {
            if(Full()) return false;
            last = (last + 1) % elements.Length;
            elements[last] = x;
            quantity++;
            return true;
        }

        public bool Remove(out T x)
        {
            if(Empty())
            {
                x = default;
                return false;
            }
            x = elements[first];
            elements[first] = default;
            first = (first + 1) % elements.Length;
            quantity--;
            return true;
        }
    }
}   