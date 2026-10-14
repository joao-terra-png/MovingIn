using UnityEngine;
using UnityEngine.SceneManagement;
using MovingIn.Core;

namespace MovingIn.Minigames.Kitchen{
    public class KitchenExit : MonoBehaviour
    {
        bool _finished;   

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Leave(false);

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.F9)) Leave(true);
#endif
        }

        public void BackHome()
        {
            Leave(true);
        }

        void Leave(bool success)
        {
            if (_finished) return;   
            _finished = true;

            Time.timeScale = 1f;    

            if (!MinigameBridge.IsInsideHub)
            {
                Debug.Log($"[KitchenExit] Modo teste (vitoria = {success}): recarregando a cena");
                SceneManager.LoadScene(gameObject.scene.name);
                return;
            }

            MinigameBridge.Finish(success);
        }
    }
}
