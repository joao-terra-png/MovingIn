using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Hub.EditorTools{
    public static class MissingScriptFinder
    {
        [MenuItem("MovingIn/Tools/Find Missing Scripts")]
        static void Find()
        {
            List<GameObject> found = Collect();
            foreach (GameObject go in found)
                Debug.LogWarning($"[MissingScripts] {Path(go.transform)} tem {GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go)} componente(s) sem script.", go);
            Selection.objects = found.ToArray();
            Debug.Log($"[MissingScripts] {found.Count} objeto(s) com script faltando. Eles ficaram selecionados na Hierarchy.");
        }

        [MenuItem("MovingIn/Tools/Remove Missing Scripts")]
        static void Remove()
        {
            List<GameObject> found = Collect();
            if (found.Count == 0)
            {
                Debug.Log("[MissingScripts] Nenhum componente sem script na cena.");
                return;
            }
            if (!EditorUtility.DisplayDialog("Remove Missing Scripts", $"Remover os componentes sem script de {found.Count} objeto(s)?", "Remover", "Cancelar")) return;
            int removed = 0;
            foreach (GameObject go in found)
            {
                Undo.RegisterCompleteObjectUndo(go, "Remove Missing Scripts");
                removed += GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            }
            Debug.Log($"[MissingScripts] {removed} componente(s) removido(s). Salve a cena.");
        }

        static List<GameObject> Collect()
        {
            List<GameObject> found = new List<GameObject>();
            Scene scene = SceneManager.GetActiveScene();
            foreach (GameObject root in scene.GetRootGameObjects())
                foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                        found.Add(t.gameObject);
            return found;
        }

        static string Path(Transform t)
        {
            string p = t.name;
            while (t.parent != null)
            {
                t = t.parent;
                p = t.name + "/" + p;
            }
            return p;
        }
    }
}
