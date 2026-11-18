using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Hub.EditorTools{
    public static class FinalTools
    {
        const string PhotoBoxId = "attic_photos";

        [MenuItem("MovingIn/Final/1. Cortar Arvore Genealogica")]
        static void CutFamilyTree()
        {
            MinigameInteractable target = null;
            foreach (MinigameInteractable mi in Object.FindObjectsOfType<MinigameInteractable>(true))
                if (mi.MinigameId == PhotoBoxId) target = mi;

            int removed = RemoveTracked();
            if (target == null)
            {
                Debug.Log($"[FinalTools] Nenhum minigame '{PhotoBoxId}' na cena. Entradas removidas do StoryManager: {removed}.");
                return;
            }

            GameObject box = target.gameObject;
            SerializedObject so = new SerializedObject(target);
            SetActiveAll(Find(so, "showWhenDone", "_showWhenDone"), false);
            SetActiveAll(Find(so, "hideWhenDone", "_hideWhenDone"), true);

            MinigameGate gate = box.GetComponent<MinigameGate>();
            if (gate != null) Undo.DestroyObjectImmediate(gate);
            Undo.DestroyObjectImmediate(target);

            CommentInteractable comment = box.GetComponent<CommentInteractable>();
            if (comment == null) comment = Undo.AddComponent<CommentInteractable>(box);
            SerializedObject cso = new SerializedObject(comment);
            cso.FindProperty("_prompt").stringValue = "Olhar as fotos";
            cso.FindProperty("_firstEmote").enumValueIndex = (int)Emote.Dots;
            SerializedProperty lines = cso.FindProperty("_lines");
            string[] text =
            {
                "Fotos antigas da família...",
                "Olha a vó quando era nova!",
                "Depois eu organizo isso com calma."
            };
            lines.arraySize = text.Length;
            for (int i = 0; i < text.Length; i++) lines.GetArrayElementAtIndex(i).stringValue = text[i];
            cso.FindProperty("_sayAllAtOnce").boolValue = true;
            cso.ApplyModifiedProperties();

            EditorSceneManager.MarkSceneDirty(box.scene);
            Selection.activeGameObject = box;
            Debug.Log($"[FinalTools] {box.name} agora e so um comentario. Entradas removidas do StoryManager: {removed}. Salve a cena.");
        }

        [MenuItem("MovingIn/Final/2. Criar Musica de Fundo")]
        static void CreateMusic()
        {
            BackgroundMusic existing = Object.FindObjectOfType<BackgroundMusic>(true);
            if (existing != null)
            {
                Selection.activeGameObject = existing.gameObject;
                Debug.Log("[FinalTools] Ja existe um objeto com BackgroundMusic. Selecionei ele.");
                return;
            }

            GameObject go = new GameObject("Music", typeof(AudioSource), typeof(BackgroundMusic));
            Undo.RegisterCreatedObjectUndo(go, "Criar Musica");
            EditorSceneManager.MarkSceneDirty(go.scene);
            Selection.activeGameObject = go;
            Debug.Log("[FinalTools] Objeto Music criado na raiz da cena. Arraste a sua musica no campo Music.");
        }

        [MenuItem("MovingIn/Final/3. Remover Scripts Faltando")]
        static void RemoveMissingScripts()
        {
            int total = 0;
            for (int s = 0; s < SceneManager.sceneCount; s++)
            {
                Scene scene = SceneManager.GetSceneAt(s);
                if (!scene.isLoaded) continue;
                int before = total;
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
                    {
                        int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject);
                        if (count == 0) continue;
                        Undo.RegisterCompleteObjectUndo(t.gameObject, "Remover Scripts Faltando");
                        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(t.gameObject);
                        total += count;
                    }
                if (total > before) EditorSceneManager.MarkSceneDirty(scene);
            }
            Debug.Log($"[FinalTools] {total} componente(s) sem script removido(s). Salve a cena.");
        }

        static int RemoveTracked()
        {
            StoryManager sm = Object.FindObjectOfType<StoryManager>(true);
            if (sm == null) return 0;

            SerializedObject so = new SerializedObject(sm);
            SerializedProperty list = so.FindProperty("minigames");
            int removed = 0;
            for (int i = list.arraySize - 1; i >= 0; i--)
            {
                if (list.GetArrayElementAtIndex(i).FindPropertyRelative("id").stringValue != PhotoBoxId) continue;
                list.DeleteArrayElementAtIndex(i);
                removed++;
            }
            so.ApplyModifiedProperties();
            return removed;
        }

        static SerializedProperty Find(SerializedObject so, params string[] names)
        {
            foreach (string n in names)
            {
                SerializedProperty p = so.FindProperty(n);
                if (p != null) return p;
            }
            return null;
        }

        static void SetActiveAll(SerializedProperty array, bool active)
        {
            if (array == null || !array.isArray) return;
            for (int i = 0; i < array.arraySize; i++)
            {
                GameObject go = array.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                if (go == null) continue;
                Undo.RecordObject(go, "Cortar Arvore Genealogica");
                go.SetActive(active);
            }
        }
    }
}
