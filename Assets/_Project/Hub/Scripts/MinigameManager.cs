using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [Tooltip("Se a cena do minigame nao existir no Build Settings, conta como feito (com aviso no Console) para a historia nao travar")]
        [SerializeField] bool _skipMissingScenes = true;

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
                if (_skipMissingScenes)
                {
                    Debug.LogWarning($"[MinigameManager] A cena '{source.SceneName}' nao existe no Build Settings. " +
                                     "O minigame foi PULADO e contado como feito.");
                    GameProgress.MarkCompleted(source.MinigameId);
                    source.MarkAsCompleted();
                    return;
                }
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

            // Freeze do Hub (o EventSystem da casa fica fora do HubRoot: desliga para nao ficarem dois)
            hubRoot.SetActive(false);
            var hubEventSystems = new List<GameObject>();
            foreach (EventSystem es in FindObjectsOfType<EventSystem>())
            {
                if (es.gameObject.scene != gameObject.scene || !es.gameObject.activeSelf) continue;
                es.gameObject.SetActive(false);
                hubEventSystems.Add(es.gameObject);
            }

            // Espera o minigame acabar e inicia ele
            MinigameBridge.Finished += OnMinigameFinished;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // aguarda o retorno do minigame
            yield return new WaitUntil(() => _finished);
            MinigameBridge.Finished -= OnMinigameFinished;

            // unfreeze do hub (descarrega tudo o que nao e o hub: o minigame pode ter trocado de fase)
            var loaded = new List<Scene>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s != gameObject.scene && s.isLoaded) loaded.Add(s);
            }
            foreach (Scene s in loaded)
                yield return SceneManager.UnloadSceneAsync(s);
            SceneManager.SetActiveScene(gameObject.scene);
            foreach (GameObject es in hubEventSystems)
                if (es != null) es.SetActive(true);
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
