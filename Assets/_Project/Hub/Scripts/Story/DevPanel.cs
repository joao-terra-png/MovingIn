using UnityEngine;

namespace MovingIn.Hub{
    public class DevPanel : MonoBehaviour
    {
        [SerializeField] KeyCode toggleKey = KeyCode.F1;

        bool _open;
        GUIStyle _title;
        GUIStyle _hint;

        void Awake()
        {
            if (!Debug.isDebugBuild) enabled = false;
        }

        void Update()
        {
            if (Input.GetKeyDown(toggleKey)) _open = !_open;
        }

        void OnGUI()
        {
            float scale = Screen.height / 1080f;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
                _hint = new GUIStyle(GUI.skin.label) { fontSize = 16 };
                _hint.normal.textColor = new Color(1f, 1f, 1f, 0.45f);
            }

            if (!_open)
            {
                GUI.Label(new Rect(12f, 1080f - 32f, 300f, 26f), $"{toggleKey}: painel de dev", _hint);
                return;
            }

            GUI.skin.button.fontSize = 18;
            GUI.skin.label.fontSize = 18;
            GUILayout.BeginArea(new Rect(20f, 20f, 440f, 640f), GUI.skin.box);
            GUILayout.Label("Painel de dev", _title);

            StoryManager sm = StoryManager.Instance;
            if (sm == null)
            {
                GUILayout.Label("Sem StoryManager na cena.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Capítulo: {sm.Current}{(sm.IsBusy ? "  (cena rodando)" : "")}");
            GUI.enabled = !sm.IsBusy;
            if (GUILayout.Button("Avançar capítulo", GUILayout.Height(36f))) sm.DevNextChapter();
            GUI.enabled = true;

            GUILayout.Space(12f);
            GUILayout.Label("Marcar minigame como concluído:");
            foreach (TrackedMinigame t in sm.Minigames)
            {
                bool done = t.minigame != null && t.minigame.IsCompleted;
                GUI.enabled = t.minigame != null && !done && !sm.IsBusy;
                string text = done ? $"{t.id}  (feito)" : t.id;
                if (GUILayout.Button(text, GUILayout.Height(34f))) sm.DevComplete(t);
            }
            GUI.enabled = true;

            GUILayout.Space(12f);
            GUILayout.Label($"{toggleKey} fecha este painel.", _hint);
            GUILayout.EndArea();
        }
    }
}
