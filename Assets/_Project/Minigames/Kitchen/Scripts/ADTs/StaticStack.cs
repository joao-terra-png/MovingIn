using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen.ADTs
{
    public class StaticStack<T>
    {
        private readonly T[] elements;
        private int top; 

        public StaticStack(int capacity)
        {
            elements = new T[capacity];
            top = -1;
        }

        public bool Empty() => top == -1;
        public bool Full() => top == elements.Length - 1;
        public int Quantity => top + 1;

        //empilha: coloca x no top
        public bool Push(T x)
        {
            if(Full()) return false;
            top++;
            elements[top] = x;
            return true;
        }

        public bool Pop(out T x)
        {
            if(Empty())
            {
                x = default;
                return false;
            }
            x = elements[top];
            elements[top] = default;
            top--;
            return true;
        }
    }
}
