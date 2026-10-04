using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen{
    ///<summary>
    /// caixa de mudanca: solta um item por vez, na ordem em que foi empacotada
    ///
    /// No Awake: copia o conteudo, embaralha (se marcado) e coloca tudo numa FILA
    /// No Start: solta o primeiro item
    /// FreeNext: tira a proxima ficha da fila, cria o item na cena e configura
    /// UpdatePreviews: mostra a cor dos proximos itens (usa o ElementAt, sem alterar a fila)
    ///
    /// vai no objeto da caixa de mudanca
    /// </summary>
    public class UnpackingBox : MonoBehaviour
    {
        [Tooltip("Fichas dos itens, na ordem em que foram empacotados")]
        [SerializeField] ItemData[] content;

        [Tooltip("Prefab do item (arraste da janela Project, nao da Hierarchy)")]
        [SerializeField] ItemVisual prefabItem;

        [Tooltip("Ponto onde cada item aparece ao sair da caixa")]
        [SerializeField] Transform exitPoint;

        [Tooltip("Quadradinhos que mostram os proximos itens (o primeiro e o proximo a sair)")]
        [SerializeField] SpriteRenderer[] previews;

        [Tooltip("Se marcado, a ordem dos itens muda a cada partida")]
        [SerializeField, FormerlySerializedAs("shuffle")] bool _shuffle;

        // a fila e um CAMPO: existe enquanto a caixa existir
        StaticQueue<ItemData> _queue;

        // o ultimo item que saiu da caixa 
        public ItemVisual CurrentItem { get; private set; }

        private void Awake()
        {
            // trabalha numa copia, para nao alterar o vetor original
            ItemData[] items = (ItemData[])content.Clone();

            // embaralha a copia, se a opcao estiver marcada
            if (_shuffle) Shuffle(items);

            // enche a fila com a copia
            _queue = new StaticQueue<ItemData>(items.Length);
            for (int i = 0; i < items.Length; i++)
            {
                _queue.Insert(items[i]);
            }
        }

        private void Start()
        {
            FreeNext();   // a caixa ja comeca soltando o primeiro item
        }

        // tira a proxima ficha da fila e cria o item na cena
        // retorna false quando a caixa ja esta vazia
        public bool FreeNext()
        {
            bool removed = _queue.Remove(out ItemData data);

            // atualiza as previas mesmo quando a caixa acabou 
            UpdatePreviews();

            if (!removed) return false;

            CurrentItem = Instantiate(prefabItem, exitPoint.position, Quaternion.identity);
            CurrentItem.Configure(data);

            return true;
        }

        // mostra a cor dos proximos itens da fila, sem altera-la
        void UpdatePreviews()
        {
            for (int i = 0; i < previews.Length; i++)
            {
                if (_queue.ElementAt(i, out ItemData next))
                {
                    previews[i].enabled = true;               
                    previews[i].color = next.colorPlaceholder;
                }
                else
                {
                    previews[i].enabled = false;            
                }
            }
        }

        // "ponte" para testar pelo menu de tres pontinhos do componente 
        [ContextMenu("Free next")]
        void FreeNextMenu()
        {
            if (!FreeNext())
                Debug.Log("[UnpackingBox] A caixa esta vazia.");
        }

        // embaralhamento Fisher-Yates: percorre de tras para frente,
        // sorteia uma posicao entre 0 e i e troca os dois elementos
        void Shuffle(ItemData[] v)
        {
            for (int i = v.Length - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);  

                ItemData temp = v[i];   
                v[i] = v[j];
                v[j] = temp;
            }
        }
    }
}