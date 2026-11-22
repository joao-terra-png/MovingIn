using System.Collections.Generic;
using MovingIn.Core.Structures;
using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class Boneco : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField] private int idRoom = 60;
        [SerializeField] private float tempoEntreMovimentos = 8f;
        [Header("Probabilidade de movimento")]
        [SerializeField, Range(0f, 1f)]
        private float chanceMover = 1f;

        [SerializeField, Range(0f, 1f)]
        private float chancePai = 0.6f;

        [SerializeField, Range(0f, 1f)]
        private float chanceEsquerda = 0.2f;

        [SerializeField, Range(0f, 1f)]
        private float chanceDireita = 0.2f;

        [Header("Aparência")]
        [Tooltip("Deslocamento a partir do ponto de foco do cômodo. -0.6 deixa os pés no chão.")]
        [SerializeField] private Vector3 deslocamentoNoComodo = new Vector3(0f, -0.6f, 0f);
        [Tooltip("Cor da cápsula usada quando o boneco não tem nenhum visual próprio.")]
        [SerializeField] private Color corPadrao = new Color(0.8f, 0.1f, 0.1f);

        private Arvore ABB;

        private float tempoAtual;
        private bool movEnable = true;
        private bool sendoObservado;

        private GameManager gameManager;
        private Renderer[] renderers;

        public void Configurar(Arvore arvore, GameManager manager)
        {
            ABB = arvore;
            gameManager = manager;

            GarantirVisual();

            QuartoNode quarto = ABB.GetQuarto(idRoom);

            if (quarto == null)
            {
                Debug.LogError("Quarto inicial do boneco não existe: " + idRoom);
                return;
            }

            if (!quarto.SetBoneco(this))
            {
                Debug.LogError("O quarto inicial já está ocupado: " + idRoom);
                return;
            }

            AtualizarPosicao(quarto);
        }

        private void Update()
        {
            AtualizarVisibilidade();

            if (ABB == null || !movEnable || sendoObservado)
                return;

            tempoAtual += Time.deltaTime;

            if (tempoAtual >= tempoEntreMovimentos)
            {
                tempoAtual = 0f;

                if (Random.value <= chanceMover)
                    GoToNextRoom();
            }
        }

        public bool GoToNextRoom()
        {
            if (ABB == null || !movEnable || sendoObservado)
                return false;

            QuartoNode atual = ABB.GetQuarto(idRoom);

            if (atual == null)
                return false;

            float sorteio = Random.value;

            QuartoNode destino = null;

            if (sorteio < chancePai)
            {
                destino = atual.GetPai();
            }
            else if (sorteio < chancePai + chanceEsquerda)
            {
                destino = atual.GetEsq();
            }
            else
            {
                destino = atual.GetDir();
            }

            if (destino == null || !destino.IsEmpty())
                return false;

            if (destino.GetID() == 60)
            {
                if (gameManager != null)
                    gameManager.VerificarInvasao(idRoom);

                return false;
            }

            return MoverPara(atual, destino);
        }

        private bool MoverPara(QuartoNode atual, QuartoNode destino)
        {
            if (destino == null || !destino.IsEmpty())
                return false;

            if (!destino.SetBoneco(this))
                return false;

            atual.RemoveBoneco();

            idRoom = destino.GetID();

            AtualizarPosicao(destino);

            return true;
        }

        private void AtualizarPosicao(QuartoNode quarto)
        {
            Room room = quarto.GetRoom();

            if (room != null)
                transform.position = room.FocusPoint + deslocamentoNoComodo;
        }

        private void GarantirVisual()
        {
            renderers = GetComponentsInChildren<Renderer>(true);

            if (renderers.Length > 0)
                return;

            GameObject capsula = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            capsula.name = "VisualBoneco";

            Collider colisor = capsula.GetComponent<Collider>();
            if (colisor != null)
                Destroy(colisor);

            capsula.transform.SetParent(transform, false);
            capsula.transform.localPosition = Vector3.zero;
            capsula.transform.localScale = new Vector3(0.8f, 0.9f, 0.8f);

            Renderer render = capsula.GetComponent<Renderer>();
            render.material.color = corPadrao;

            renderers = new Renderer[] { render };
        }

        private void AtualizarVisibilidade()
        {
            if (ABB == null || renderers == null)
                return;

            QuartoNode quarto = ABB.GetQuarto(idRoom);
            Room room = quarto != null ? quarto.GetRoom() : null;
            bool visivel = room != null && room.gameObject.activeInHierarchy;

            foreach (Renderer render in renderers)
            {
                if (render != null && render.enabled != visivel)
                    render.enabled = visivel;
            }
        }

        public int GetIDRoom()
        {
            return idRoom;
        }

        public void SetIDRoom(int id)
        {
            idRoom = id;
        }

        public void SetMovEnable(bool valor)
        {
            movEnable = valor;
        }

        public void SetSendoObservado(bool valor)
        {
            sendoObservado = valor;
        }

        public bool EstaSendoObservado()
        {
            return sendoObservado;
        }
    }
}
