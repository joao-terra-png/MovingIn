using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mudanca.Core;

namespace Mudanca.Hub
{
    /// <summary>
    /// Abre e fecha minigames a partir do hub.
    ///
    /// Como funciona:
    ///  1. Desliga o "HubRoot" (player, câmera, casa, luz...) — a cena 3D "pausa".
    ///  2. Carrega a cena do minigame por cima (modo Additive).
    ///  3. Espera o minigame chamar MinigameBridge.Finish(...).
    ///  4. Descarrega a cena do minigame e religa o HubRoot.
    ///  5. Se o jogador venceu, o cômodo fica arrumado.
    ///
    /// IMPORTANTE: este objeto precisa ficar FORA do HubRoot,
    /// senão ele seria desligado junto e pararia de funcionar.
    /// </summary>
    public class MinigameManager : MonoBehaviour
    {
        public static MinigameManager Instance { get; private set; }

        [Tooltip("Objeto pai de TUDO do hub (Player, Câmera, Casa, Luz, UI). É desligado enquanto o minigame roda.")]
        [SerializeField] GameObject hubRoot;

        public bool IsPlayingMinigame { get; private set; }

        bool finished;
        bool lastResult;

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
                Debug.LogError("[MinigameManager] Este objeto está DENTRO do HubRoot. Tire ele de lá!");
        }

        /*
         * Nome: StartMinigame
         * Objetivo: abrir o minigame associado a um objeto interagível.
         * Parâmetros: source (entrada) - o interagível que o jogador ativou.
         * Retorno: nenhum.
         */
        public void StartMinigame(MinigameInteractable source)
        {
            if (IsPlayingMinigame) return;

            if (!Application.CanStreamedLevelBeLoaded(source.SceneName))
            {
                Debug.LogError($"[MinigameManager] A cena '{source.SceneName}' não foi encontrada. " +
                               "Confira o nome e se ela está em File > Build Settings > Scenes In Build.");
                return;
            }

            StartCoroutine(RunMinigame(source));
        }

        IEnumerator RunMinigame(MinigameInteractable source)
        {
            IsPlayingMinigame = true;
            finished = false;
            string sceneName = source.SceneName;

            // 1. "Pausa" o hub desligando tudo dele
            hubRoot.SetActive(false);

            // 2. Começa a escutar o fim do minigame e carrega a cena dele
            MinigameBridge.Finished += OnMinigameFinished;
            yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            // 3. Espera o minigame chamar MinigameBridge.Finish(...)
            yield return new WaitUntil(() => finished);
            MinigameBridge.Finished -= OnMinigameFinished;

            // 4. Fecha o minigame e volta ao hub
            yield return SceneManager.UnloadSceneAsync(sceneName);
            SceneManager.SetActiveScene(gameObject.scene);
            hubRoot.SetActive(true);

            // 5. Resultado
            if (lastResult)
            {
                GameProgress.MarkCompleted(source.MinigameId);
                source.MarkAsCompleted();
            }

            IsPlayingMinigame = false;
        }

        void OnMinigameFinished(bool success)
        {
            lastResult = success;
            finished = true;
        }
    }
}
