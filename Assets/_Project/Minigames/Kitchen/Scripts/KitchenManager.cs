using System;
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

        public event Action<int> PointsChanged;
        public event Action	NoteChanged;
        public event Action<ItemVisual>	Hit;
        public event Action<ItemVisual>	Miss;
        public event Action	Victory;

        CadastralList<ItemData> _note;
        int _points;
        bool _boxEmpty;
        bool _won;

        public int Points => _points;

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

            if(!zone.TryReceive(item, belongs))
            {
                _points = Mathf.Max(0, _points - _missPoints);
                PointsChanged?.Invoke(_points);
                Miss?.Invoke(item);
                 return false;
            }
            
            if (zone.IsFinalDestination) _points += _hitPoints;   
            if(zone.IsFinalDestination && belongs)
            {
                _note.Remove(item.Data);
                NoteChanged?.Invoke();
            } 

            bool cameFromBox = item.CurrentZone == null;
            if(!cameFromBox) item.CurrentZone.Release(item);
            item.CurrentZone = zone;

            if	(cameFromBox &&	!box.FreeNext()) _boxEmpty = true;
            PointsChanged?.Invoke(_points);
            Hit?.Invoke(item);
            CheckVictory();
            return	true;
        }   

        public void ForEachNoteItem(Action<ItemData> action)
        {
            if(!_note.GetFirst(out ItemData item)) return;
            do
            {
                action(item);
            }while(_note.GetNext(out item));
        }

        void CheckVictory()
        {
            if(!_won && _boxEmpty && _note.Empty())
            {
                _won = true;
                Victory?.Invoke();
            }
        }
    }
}