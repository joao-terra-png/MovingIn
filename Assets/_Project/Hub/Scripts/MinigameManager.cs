using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MovingIn.Core;

namespace MovingIn.Hub{
    ///<summary>
    /// lógica para abrir e fechar minigames
    /// 
    /// Ele vai freezar o Hub
    /// Carregar o minigame por cima
    /// Quando o minigame acabar, ele descarrega a cena e carrega o hub dnv
    /// Se o jogador vencer, o como deve aparecer arrumado
    /// </summary>
    public class MinigameManager : MonoBehaviour
    {
        public static MinigameManager Instance { get; private set; }

        [SerializeField] GameObject hubRoot;

        public bool IsPlayingMinigame { get; private set; }

        bool _finished;
        bool _lastResult;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (hubRoot == null)
                Debug.LogError("[MinigameManager] Arraste o HubRoot no campo 'Hub Root' do Inspector.");
            else if (transform.IsChildOf(hubRoot.transform))
                Debug.LogError("[MinigameManager] Este objeto esta DENTRO do HubRoot. Tire ele de la");
        }

        public void StartMinigame(MinigameInteractable source)
        {
            if (IsPlayingMinigame) return;

            if (!Application.CanStreamedLevelBeLoaded(source.SceneName))
            {
                Debug.LogError($"[MinigameManager] A cena '{source.SceneName}' nao foi encontrada. " +
                               "Confira o nome e se ela esta em File > Build Settings > Scenes In Build.");
                return;
            }

            StartCoroutine(RunMinigame(source));
        }

        IEnumerator RunMinigame(MinigameInteractable source)
        {
            IsPlayingMinigame = true;
            _finished = false;
            string sceneName = source.SceneName;

            // Freeze do Hub
            hubRoot.SetActive(false);

            // Espera o minigame acabar e inicia ele
            MinigameBridge.Finished += OnMinigameFinished;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // aguarda o retorno do minigame
            yield return new WaitUntil(() => _finished);
            MinigameBridge.Finished -= OnMinigameFinished;

            // unfreeze do hub
            yield return SceneManager.UnloadSceneAsync(sceneName);
            SceneManager.SetActiveScene(gameObject.scene);
            hubRoot.SetActive(true);

            // Resultado
            if (_lastResult)
            {
                GameProgress.MarkCompleted(source.MinigameId);
                source.MarkAsCompleted();
            }

            IsPlayingMinigame = false;
        }

        void OnMinigameFinished(bool success)
        {
            _lastResult = success;
            _finished = true;
        }
    }
}
