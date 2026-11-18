using System;
using System.Collections.Generic;
using System.IO;
using MovingIn.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Hub{
    public static class SaveSystem
    {
        public const int AutoSlot = 0;
        public const int ManualSlots = 3;

        static SaveData _pending;
        static float _playTimeBase;
        static float _sessionStart;

        public static float PlayTime => _playTimeBase + Time.unscaledTime - _sessionStart;
        public static bool HasPending => _pending != null;

        public static bool CanSave
        {
            get
            {
                StoryManager sm = StoryManager.Instance;
                if (sm == null || sm.IsBusy) return false;
                if (MinigameManager.Instance != null && MinigameManager.Instance.IsPlayingMinigame) return false;
                if (RoomManager.Instance != null && RoomManager.Instance.IsTransitioning) return false;
                return true;
            }
        }

        public static string PathFor(int slot)
        {
            string file = slot == AutoSlot ? "save_auto.json" : $"save_{slot}.json";
            return Path.Combine(Application.persistentDataPath, file);
        }

        public static bool Save(int slot)
        {
            if (!CanSave) return false;
            try
            {
                File.WriteAllText(PathFor(slot), JsonUtility.ToJson(Capture(), true));
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Nao consegui salvar no slot {slot}: {e.Message}");
                return false;
            }
        }

        public static void AutoSave()
        {
            Save(AutoSlot);
        }

        public static SaveData Peek(int slot)
        {
            string path = PathFor(slot);
            if (!File.Exists(path)) return null;
            try
            {
                return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Save {slot} corrompido: {e.Message}");
                return null;
            }
        }

        public static bool Load(int slot)
        {
            SaveData data = Peek(slot);
            if (data == null) return false;

            GameProgress.ResetAll();
            if (data.completed != null)
                foreach (string id in data.completed)
                    GameProgress.MarkCompleted(id);

            _pending = data;
            _playTimeBase = data.playTime;
            _sessionStart = Time.unscaledTime;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return true;
        }

        public static void Delete(int slot)
        {
            string path = PathFor(slot);
            if (File.Exists(path)) File.Delete(path);
        }

        public static bool TakePending(out SaveData data)
        {
            data = _pending;
            _pending = null;
            return data != null;
        }

        public static string Describe(SaveData data)
        {
            if (data == null) return "vazio";
            return $"{ChapterTitle(data.chapter)}  ·  {data.savedAt}";
        }

        public static string ChapterTitle(string chapter)
        {
            switch (chapter)
            {
                case nameof(Chapter.Arrival): return "Chegada";
                case nameof(Chapter.Blackout): return "Apagão";
                case nameof(Chapter.PowerRestored): return "Luz de volta";
                case nameof(Chapter.Afternoon): return "A tarde";
                case nameof(Chapter.FirstNight): return "Primeira noite";
                case nameof(Chapter.Morning): return "Amanhecer";
                default: return chapter;
            }
        }

        static SaveData Capture()
        {
            StoryManager sm = StoryManager.Instance;
            List<string> done = new List<string>();
            if (sm != null)
                foreach (TrackedMinigame t in sm.Minigames)
                    if (GameProgress.IsCompleted(t.id) || (t.minigame != null && t.minigame.IsCompleted))
                        done.Add(t.id);

            RoomManager rm = RoomManager.Instance;
            return new SaveData
            {
                chapter = sm != null ? sm.Current.ToString() : nameof(Chapter.Arrival),
                room = rm != null && rm.CurrentRoom != null ? rm.CurrentRoom.name : "",
                completed = done.ToArray(),
                playTime = PlayTime,
                savedAt = DateTime.Now.ToString("dd/MM HH:mm"),
            };
        }
    }
}
