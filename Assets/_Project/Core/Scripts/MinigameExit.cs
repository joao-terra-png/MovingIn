using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Core{
    public class MinigameExit : MonoBehaviour
    {
        bool _finished;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Leave(false);
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

        public void Retry()
        {
            if (_finished) return;
            _finished = true;
            Time.timeScale = 1f;

            string sceneName = gameObject.scene.name;
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
