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

        [Tooltip("Se marcado, a ordem dos itens muda a cada partida")]
        [SerializeField, FormerlySerializedAs("shuffle")] bool _shuffle;

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
            FreeNext();  
        }

        // tira a proxima ficha da fila e cria o item na cena
        // retorna false quando a caixa ja esta vazia
        public bool FreeNext()
        {
            if (!_queue.Remove(out ItemData data)) return false;

            CurrentItem = Instantiate(prefabItem, exitPoint.position, Quaternion.identity);
            CurrentItem.Configure(data);

            return true;
        }

        // "ponte" para testar pelo menu de tres pontinhos do componente 
        [ContextMenu("Free next")]
        void FreeNextMenu()
        {
            if (!FreeNext())
                Debug.Log("[UnpackingBox] A caixa esta vazia.");
        }

        // embaralha os itens
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