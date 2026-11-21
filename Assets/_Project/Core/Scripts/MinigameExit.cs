using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Core{
    public class MinigameExit : MonoBehaviour
    {
        [Tooltip("Desmarque se o minigame ja usa o ESC para outra coisa (pausa, cancelar...)")]
        [SerializeField] bool _escapeLeaves = true;

        bool _finished;

        public bool EscapeLeaves => _escapeLeaves;

        void Update()
        {
            if (_escapeLeaves && Input.GetKeyDown(KeyCode.Escape)) Leave(false);
#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F9)) Leave(true);
#endif
        }

        public void BackHome() => Leave(true);

        public void Leave(bool success)
        {
            if (_finished) return;
            _finished = true;
            Time.timeScale = 1f;

            if (!MinigameBridge.IsInsideHub)
            {
                Debug.Log($"[MinigameExit] Modo teste (vitoria = {success}): recarregando {gameObject.scene.name}");
                SceneManager.LoadScene(gameObject.scene.name);
                return;
            }

            MinigameBridge.Finish(success);
        }

        public void Retry() => LoadLevel(gameObject.scene.name);

        public void LoadLevel(string sceneName)
        {
            if (_finished) return;
            _finished = true;
            Time.timeScale = 1f;

            if (!MinigameBridge.IsInsideHub)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            AsyncOperation unload = SceneManager.UnloadSceneAsync(gameObject.scene);
            unload.completed += _ =>
            {
                AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                load.completed += __ => SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
            };
        }
    }
}
