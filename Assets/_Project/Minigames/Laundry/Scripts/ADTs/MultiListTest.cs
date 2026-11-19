using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.Laundry.ADTs;

namespace MovingIn.Minigames.Laundry{
    public class MultiListTest : MonoBehaviour
    {
        int _passed, _failed;

        void Start()
        {
            var m = new MultiList<string>(3, 3);
            Check("Vazia no inicio", m.Empty(), true);

            Check("Insere camiseta branca", m.Insert("camiseta-branca", 0, 0), true);
            Check("Insere meia branca", m.Insert("meia-branca", 0, 2), true);
            Check("Insere calca escura", m.Insert("calca-escura", 1, 1), true);
            Check("Insere meia colorida", m.Insert("meia-colorida", 2, 2), true);
            Check("Insere repetido", m.Insert("meia-branca", 0, 2), false);
            Check("Cor invalida", m.Insert("x", 5, 0), false);

            Check("Quantidade", m.Quantity, 4);
            Check("Brancas", m.CountByColor(0), 2);
            Check("Meias", m.CountByType(2), 2);
            Check("Percurso brancas", Join(m, true, 0), "meia-branca camiseta-branca");
            Check("Percurso meias", Join(m, false, 2), "meia-colorida meia-branca");

            Check("Remove meia branca", m.Remove("meia-branca"), true);
            Check("Remove de novo", m.Remove("meia-branca"), false);
            Check("Saiu da lista de COR", Join(m, true, 0), "camiseta-branca");
            Check("Saiu da lista de TIPO", Join(m, false, 2), "meia-colorida");
            Check("Brancas depois", m.CountByColor(0), 1);
            Check("Meias depois", m.CountByType(2), 1);

            Check("Remove o unico da cor", m.Remove("calca-escura"), true);
            Check("Escuras vazia", Join(m, true, 1), "");
            Check("Calcas vazia", m.CountByType(1), 0);

            m.Remove("camiseta-branca");
            m.Remove("meia-colorida");
            Check("Vazia no fim", m.Empty(), true);

            if (_failed == 0) Debug.Log($"===== MultiList: todos os {_passed} testes passaram =====");
            else Debug.LogError($"===== MultiList: {_failed} falharam, {_passed} passaram =====");
        }

        string Join(MultiList<string> m, bool byColor, int key)
        {
            string s = "";
            if (byColor) m.ForEachByColor(key, x => s += x + " ");
            else m.ForEachByType(key, x => s += x + " ");
            return s.Trim();
        }

        void Check<T>(string label, T actual, T expected)
        {
            if (EqualityComparer<T>.Default.Equals(actual, expected)) { _passed++; Debug.Log($"✓ {label}: {actual}"); }
            else { _failed++; Debug.LogError($"✗ {label}: obteve {actual}, esperava {expected}"); }
        }
    }
}
