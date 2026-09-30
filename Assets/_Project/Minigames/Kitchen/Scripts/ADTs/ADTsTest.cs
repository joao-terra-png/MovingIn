using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen
{
    public class ADTsTest : MonoBehaviour
    {
        private void Start()
        {
            var s = new StaticStack<int>(3);
            Debug.Log($"Vazia no inicio? {s.Empty()}");
            s.Push(10);
            s.Push(20);
            s.Push(30);
            Debug.Log($"Cheia? {s.Full()}");
            Debug.Log($"Empilhar 40 deu certo? {s.Push(40)}");

            while(s.Pop(out int x))
            {
                Debug.Log($"Saiu {x}");
            }
        }
    }
}