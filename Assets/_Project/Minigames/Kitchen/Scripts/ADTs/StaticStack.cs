using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    ///<summary>
    /// Pilha sequencial e estatica que aceita tipo generico de dados
    /// new StaticStack<int>(5) -> pilha de inteiros
    /// new StaticStack<ItemVisual>(3) -> pilha de prato
    ///</summary>
    
namespace MovingIn.Minigames.Kitchen.ADTs
{
    public class StaticStack<T>
    {
        private readonly T[] elements;
        private int top; //-1 quando vazia

        //Construtor
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

        //desempilha: tira o elemento do topo e retornar ele pelo out
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
