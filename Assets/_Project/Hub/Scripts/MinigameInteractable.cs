using UnityEngine;
using UnityEngine.Events;
using Mudanca.Core;

namespace Mudanca.Hub
{
    /// <summary>
    /// Coloque em uma caixa/objeto do cenário que deve abrir um minigame.
    /// O objeto precisa ter um Collider (pode ser o BoxCollider que o cubo já tem).
    /// </summary>
    public class MinigameInteractable : MonoBehaviour
    {
        [Header("Minigame")]
        [Tooltip("ID único deste interagível. Ex.: quarto_brinquedos, sala_estante.")]
        [SerializeField] string minigameId = "quarto_brinquedos";

        [Tooltip("Nome EXATO da cena do minigame. Ela precisa estar em File > Build Settings.")]
        [SerializeField] string sceneName = "Minigame_Exemplo";

        [Tooltip("Texto que aparece quando o jogador chega perto.")]
        [SerializeField] string prompt = "Arrumar as caixas";

        [Header("O que muda quando o minigame é vencido")]
        [Tooltip("Objetos da BAGUNÇA (caixas, brinquedos no chão...). Somem ao vencer.")]
        [SerializeField] GameObject[] hideWhenDone;

        [Tooltip("Objetos do cômodo ARRUMADO (móveis no lugar...). Aparecem ao vencer.")]
        [SerializeField] GameObject[] showWhenDone;

        [Tooltip("Extras opcionais: tocar som, partícula, liberar outra porta...")]
        [SerializeField] UnityEvent onCompleted;

        public string MinigameId => minigameId;
        public string SceneName => sceneName;
        public string Prompt => prompt;
        public bool IsCompleted { get; private set; }

        void Start()
        {
            // Começa sempre com a versão "arrumada" escondida
            foreach (GameObject go in showWhenDone)
                if (go != null) go.SetActive(false);

            if (GameProgress.IsCompleted(minigameId))
                ApplyCompletedState();
        }

        /*
         * Nome: Interact
         * Objetivo: pedir ao MinigameManager para abrir o minigame deste objeto.
         * Parâmetros: nenhum.
         * Retorno: nenhum.
         */
        public void Interact()
        {
            if (IsCompleted) return;

            if (MinigameManager.Instance == null)
            {
                Debug.LogError("[MinigameInteractable] Não existe MinigameManager na cena.");
                return;
            }
            MinigameManager.Instance.StartMinigame(this);
        }

        /*
         * Nome: MarkAsCompleted
         * Objetivo: trocar a bagunça pela versão arrumada (chamado pelo MinigameManager após a vitória).
         * Parâmetros: nenhum.
         * Retorno: nenhum.
         */
        public void MarkAsCompleted()
        {
            ApplyCompletedState();
            onCompleted?.Invoke();
        }

        void ApplyCompletedState()
        {
            IsCompleted = true;
            foreach (GameObject go in hideWhenDone)
                if (go != null) go.SetActive(false);
            foreach (GameObject go in showWhenDone)
                if (go != null) go.SetActive(true);
        }
    }
}
