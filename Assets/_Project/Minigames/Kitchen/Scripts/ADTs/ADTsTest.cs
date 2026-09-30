using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary>
    /// teste geral dos TADs (StaticStack, StaticQueue e CadastralList)
    /// coloque num objeto qualquer de uma cena de testes e de Play
    /// cada verificacao mostra ✓ (certo) ou ✗ em vermelho (errado)
    /// no fim aparece um resumo com quantas passaram e quantas falharam
    /// </summary>
    
    public class ADTsTest : MonoBehaviour
    {
        int _passed;
        int _failed;

        private void Start()
        {
            TestStack();
            TestQueue();
            TestCadastralList();

            if (_failed == 0)
                Debug.Log($"===== RESULTADO: todos os {_passed} testes passaram =====");
            else
                Debug.LogError($"===== RESULTADO: {_failed} falharam, {_passed} passaram =====");
        }

        // ------------------------------------------------------------
        // auxiliares
        // ------------------------------------------------------------

        // compara o valor obtido com o esperado e mostra no Console
        void Check<T>(string label, T actual, T expected)
        {
            if (EqualityComparer<T>.Default.Equals(actual, expected))
            {
                _passed++;
                Debug.Log($"✓ {label}: {actual}");
            }
            else
            {
                _failed++;
                Debug.LogError($"✗ {label}: obteve {actual}, esperava {expected}");
            }
        }

        // desempilha tudo e junta numa string (a pilha termina vazia)
        string DrainStack(StaticStack<int> s)
        {
            string result = "";
            while (s.Pop(out int x))
                result += x + " ";
            return result.Trim();
        }

        // retira tudo da fila e junta numa string (a fila termina vazia)
        string DrainQueue(StaticQueue<int> q)
        {
            string result = "";
            while (q.Remove(out int x))
                result += x + " ";
            return result.Trim();
        }

        // percorre a lista com GetFirst/GetNext e junta numa string (a lista nao muda)
        string Traverse(CadastralList<string> l)
        {
            string result = "";
            if (l.GetFirst(out string item))
            {
                do
                {
                    result += item + " ";
                }
                while (l.GetNext(out item));
            }
            return result.Trim();
        }

        // ------------------------------------------------------------
        // PILHA
        // ------------------------------------------------------------
        void TestStack()
        {
            Debug.Log("########## PILHA (StaticStack) ##########");

            Debug.Log("--- 1. Basico ---");
            var s = new StaticStack<int>(3);
            Check("Vazia no inicio", s.Empty(), true);
            Check("Quantidade no inicio", s.Quantity, 0);
            Check("Empilha 10", s.Push(10), true);
            Check("Empilha 20", s.Push(20), true);
            Check("Empilha 30", s.Push(30), true);
            Check("Cheia", s.Full(), true);
            Check("Quantidade", s.Quantity, 3);
            Check("Empilha 40 com a pilha cheia", s.Push(40), false);
            Check("Quantidade nao mudou", s.Quantity, 3);

            Debug.Log("--- 2. Ordem de saida (LIFO) ---");
            Check("Desempilha tudo", DrainStack(s), "30 20 10");
            Check("Vazia depois", s.Empty(), true);

            Debug.Log("--- 3. Pilha vazia ---");
            Check("Desempilha na vazia", s.Pop(out int x), false);
            Check("Valor entregue na falha (default)", x, 0);

            Debug.Log("--- 4. Reuso depois de esvaziar ---");
            Check("Empilha 5", s.Push(5), true);
            Check("Empilha 6", s.Push(6), true);
            Check("Desempilha um", s.Pop(out int y), true);
            Check("Saiu o topo", y, 6);
            Check("Quantidade", s.Quantity, 1);
            Check("Cheia?", s.Full(), false);
            Check("Desempilha o resto", DrainStack(s), "5");
        }

        // ------------------------------------------------------------
        // FILA
        // ------------------------------------------------------------
        void TestQueue()
        {
            Debug.Log("########## FILA (StaticQueue) ##########");

            Debug.Log("--- 1. Basico ---");
            var q = new StaticQueue<int>(3);
            Check("Vazia no inicio", q.Empty(), true);
            Check("Quantidade no inicio", q.Quantity, 0);
            Check("Insere 1", q.Insert(1), true);
            Check("Insere 2", q.Insert(2), true);
            Check("Insere 3", q.Insert(3), true);
            Check("Cheia", q.Full(), true);
            Check("Quantidade", q.Quantity, 3);
            Check("Insere 99 com a fila cheia", q.Insert(99), false);

            Debug.Log("--- 2. Dando a volta no vetor ---");
            Check("Retira um", q.Remove(out int first), true);
            Check("Saiu o primeiro (FIFO)", first, 1);
            Check("Insere 4 (usa a posicao liberada)", q.Insert(4), true);
            Check("Cheia de novo", q.Full(), true);
            Check("Retira tudo", DrainQueue(q), "2 3 4");
            Check("Vazia depois", q.Empty(), true);

            Debug.Log("--- 3. Fila vazia ---");
            Check("Retira na vazia", q.Remove(out int x), false);
            Check("Valor entregue na falha (default)", x, 0);

            Debug.Log("--- 4. Varias voltas seguidas ---");
            // insere e retira 10 vezes numa fila de capacidade 3:
            // os indices dao a volta varias vezes; se o % estiver errado, quebra aqui
            bool allOk = true;
            for (int i = 1; i <= 10; i++)
            {
                q.Insert(i);
                q.Remove(out int saiu);
                if (saiu != i) allOk = false;
            }
            Check("10 ciclos de insere/retira na ordem certa", allOk, true);
            Check("Vazia depois dos ciclos", q.Empty(), true);

            Debug.Log("--- 5. Enche e esvazia depois das voltas ---");
            q.Insert(7);
            q.Insert(8);
            q.Insert(9);
            Check("Cheia depois das voltas", q.Full(), true);
            Check("Ordem depois das voltas", DrainQueue(q), "7 8 9");
        }

        // ------------------------------------------------------------
        // LISTA CADASTRAL
        // ------------------------------------------------------------
        void TestCadastralList()
        {
            Debug.Log("########## LISTA CADASTRAL (CadastralList) ##########");

            Debug.Log("--- 1. Basico ---");
            var l = new CadastralList<string>(5);
            Check("Vazia no inicio", l.Empty(), true);
            Check("Insere prato", l.Insert("prato"), true);
            Check("Insere xicara", l.Insert("xicara"), true);
            Check("Insere panela", l.Insert("panela"), true);
            Check("Insere prato de novo (repetido)", l.Insert("prato"), false);
            Check("Tem prato? (posicao 0)", l.Contains("prato"), true);
            Check("Tem panela? (ultima)", l.Contains("panela"), true);
            Check("Tem ursinho?", l.Contains("ursinho"), false);
            Check("Quantidade", l.Quantity, 3);
            Check("Vazia agora?", l.Empty(), false);

            Debug.Log("--- 2. Remove no meio ---");
            Check("Remove xicara", l.Remove("xicara"), true);
            Check("Remove xicara de novo", l.Remove("xicara"), false);
            Check("Tem xicara?", l.Contains("xicara"), false);
            Check("Tem panela? (foi deslocada)", l.Contains("panela"), true);
            Check("Quantidade", l.Quantity, 2);
            Check("Insere talheres", l.Insert("talheres"), true);
            Check("Quantidade", l.Quantity, 3);

            Debug.Log("--- 3. Percurso ---");
            Check("Percurso", Traverse(l), "prato panela talheres");
            Check("Percurso de novo (GetFirst reinicia)", Traverse(l), "prato panela talheres");

            Debug.Log("--- 4. Lista vazia ---");
            var empty = new CadastralList<string>(3);
            Check("Empty()", empty.Empty(), true);
            Check("GetFirst na vazia", empty.GetFirst(out _), false);
            Check("Remove na vazia", empty.Remove("prato"), false);
            Check("Contains na vazia", empty.Contains("prato"), false);
            Check("Percurso da vazia", Traverse(empty), "");

            Debug.Log("--- 5. Lista cheia ---");
            var full = new CadastralList<string>(3);
            full.Insert("prato");
            full.Insert("xicara");
            full.Insert("panela");
            Check("Full()", full.Full(), true);
            Check("Insere com a lista cheia", full.Insert("talheres"), false);
            Check("Remove o PRIMEIRO com a lista cheia", full.Remove("prato"), true);
            Check("Percurso depois", Traverse(full), "xicara panela");
            Check("Full() depois de remover", full.Full(), false);
            Check("Insere de novo", full.Insert("talheres"), true);
            Check("Percurso final", Traverse(full), "xicara panela talheres");

            Debug.Log("--- 6. Ultimo e unico ---");
            Check("Remove o ULTIMO (talheres)", full.Remove("talheres"), true);
            Check("Percurso", Traverse(full), "xicara panela");

            var single = new CadastralList<string>(3);
            single.Insert("prato");
            Check("Remove o UNICO", single.Remove("prato"), true);
            Check("Vazia depois", single.Empty(), true);
            Check("Quantidade", single.Quantity, 0);
            Check("Insere depois de esvaziar", single.Insert("panela"), true);
            Check("Percurso", Traverse(single), "panela");
        }
    }
}