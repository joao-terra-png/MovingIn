using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using MovingIn.Core;

namespace MovingIn.Hub{
    ///<summary>
    /// vai em cada caixa/objeto do cenairo que abre um minigame
    /// o objeto precisa ter um Collider (o BoxCollider do cubo ja serve)
    /// </summary>
    public class MinigameInteractable : MonoBehaviour
    {
        [Header("Minigame")]
        [Tooltip("ID unico deste interagivel. Ex: quarto_brinquedos, sala_cartas")]
        [SerializeField, FormerlySerializedAs("minigameId")] string _minigameId = "quarto_brinquedos";

        [Tooltip("Nome EXATO da cena do minigame (tem que estar em File > Build Settings)")]
        [SerializeField, FormerlySerializedAs("sceneName")] string _sceneName = "Minigame_Exemplo";

        [Tooltip("Texto que aparece quando o jogador chega perto")]
        [SerializeField, FormerlySerializedAs("prompt")] string _prompt = "Arrumar as caixas";

        [Header("O que muda quando o minigame e vencido")]
        [Tooltip("Objetos da BAGUNCA (caixas, brinquedos no chao...). Somem ao vencer")]
        [SerializeField] GameObject[] hideWhenDone;

        [Tooltip("Objetos do comodo ARRUMADO (moveis no lugar...). Aparecem ao vencer")]
        [SerializeField] GameObject[] showWhenDone;

        [Tooltip("Extras opcionais: som, particula, liberar porta...")]
        [SerializeField] UnityEvent onCompleted;

        public string MinigameId => _minigameId;
        public string SceneName => _sceneName;
        public string Prompt => _prompt;
        public bool IsCompleted { get; private set; }

        void Start()
        {
            // comeca sempre com a versao arrumada escondida
            foreach (GameObject go in showWhenDone)
                if (go != null) go.SetActive(false);

            if (GameProgress.IsCompleted(_minigameId))
                ApplyCompletedState();
        }

        // chamado pelo PlayerInteractor quando o jogador aperta E
        public void Interact()
        {
            if (IsCompleted) return;

            if (MinigameManager.Instance == null)
            {
                Debug.LogError("[MinigameInteractable] Nao existe MinigameManager na cena.");
                return;
            }
            MinigameManager.Instance.StartMinigame(this);
        }

        // chamado pelo MinigameManager depois da vitoria
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
