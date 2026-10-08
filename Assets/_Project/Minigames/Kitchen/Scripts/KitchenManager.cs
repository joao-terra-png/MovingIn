using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen
{
    ///<summary>
    /// regras gerais do minigame: pontos, bilhete, vitoria
    ///</summary>
     
    public class KitchenManager : MonoBehaviour
    {
        [Tooltip("Os itens que sao da cozinha")]
        [SerializeField] ItemData[] noteItems;
        [SerializeField] UnpackingBox box;
        [SerializeField] int _hitPoints = 10;
        [SerializeField] int _missPoints = 5;

        CadastralList<ItemData> _note;
        int _points;
        bool _boxEmpty;
        bool _won;

        void Awake()
        {
            _note = new CadastralList<ItemData>(noteItems.Length);
            for (int i = 0; i < noteItems.Length; i++)
            {
                _note.Insert(noteItems[i]);
            }
        }

        //chamado ao soltar; zone == null --> chao; true se foi colocado em algum lugar
        public bool ItemDropped(ItemVisual item, DropZone zone)
        {
            if(zone == null || zone == item.CurrentZone) return false;

            bool belongs = _note.Contains(item.Data);

            if(!zone.TryRecive(item, belongs))
            {
                _points = Mathf.Max(0, _points - _missPoints);
                Debug.Log($"Erro! Pontos: {_points}");
                 return false;
            }
            
            _points += _hitPoints;
            if(zone.IsFinalDestination && belongs) _note.Remove(item.Data);

            bool cameFromBox = item.CurrentZone == null;
            if(!cameFromBox) item.CurrentZone.Release(item);
            item.CurrentZone = zone;

            if	(cameFromBox &&	!box.FreeNext()) _boxEmpty = true;
            Debug.Log($"Acerto!	Pontos:	{_points}");
            CheckVictory();
            return	true;
        }   

        void CheckVictory()
        {
            if(!_won && _boxEmpty && _note.Empty())
            {
                _won = true;
                Debug.Log("Cozinha arrumada");
            }
        }
    }
}