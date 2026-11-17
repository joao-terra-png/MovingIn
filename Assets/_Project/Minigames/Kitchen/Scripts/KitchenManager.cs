using System;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Minigames.Kitchen{
    public class KitchenManager : MonoBehaviour
    {
        [SerializeField] ItemData[] noteItems;
        [SerializeField] UnpackingBox box;
        [SerializeField] int _hitPoints = 10;
        [SerializeField] int _missPoints = 5;

        public event Action<int> PointsChanged;
        public event Action NoteChanged;
        public event Action<ItemVisual> Hit;
        public event Action<ItemVisual> Miss;
        public event Action Victory;

        CadastralList<ItemData> _note;
        int _points;
        bool _boxEmpty;
        bool _won;

        public int Points => _points;

        void Awake()
        {
            _note = new CadastralList<ItemData>(noteItems.Length);
            for (int i = 0; i < noteItems.Length; i++)
                _note.Insert(noteItems[i]);
        }

        public bool ItemDropped(ItemVisual item, DropZone zone)
        {
            if (_won) return false;
            if (zone == null || zone == item.CurrentZone) return false;

            bool belongs = _note.Contains(item.Data);

            if (!zone.TryReceive(item, belongs))
            {
                _points = Mathf.Max(0, _points - _missPoints);
                PointsChanged?.Invoke(_points);
                Miss?.Invoke(item);
                return false;
            }

            bool cameFromBox = item.CurrentZone == null;
            if (!cameFromBox) item.CurrentZone.Release(item);
            item.CurrentZone = zone;

            if (zone is PlateStackZone plates)
            {
                if (plates.IsComplete) CompletePlates();
            }
            else if (zone.IsFinalDestination)
            {
                _points += _hitPoints;
                if (belongs)
                {
                    _note.Remove(item.Data);
                    NoteChanged?.Invoke();
                }
            }

            if (cameFromBox && !box.FreeNext()) _boxEmpty = true;

            PointsChanged?.Invoke(_points);
            Hit?.Invoke(item);
            CheckVictory();
            return true;
        }

        void CompletePlates()
        {
            int removed = 0;
            for (int i = 0; i < noteItems.Length; i++)
                if (noteItems[i].type == ItemType.Plate && _note.Remove(noteItems[i])) removed++;

            _points += _hitPoints * removed;
            if (removed > 0) NoteChanged?.Invoke();
        }

        public void ForEachNoteItem(Action<ItemData> action)
        {
            if (!_note.GetFirst(out ItemData item)) return;
            do
            {
                action(item);
            }
            while (_note.GetNext(out item));
        }

        public void ForEachNoteEntry(Action<ItemData, bool> action)
        {
            for (int i = 0; i < noteItems.Length; i++)
                action(noteItems[i], !_note.Contains(noteItems[i]));
        }

        void CheckVictory()
        {
            if (!_won && _boxEmpty && _note.Empty())
            {
                _won = true;
                Victory?.Invoke();
            }
        }
    }
}