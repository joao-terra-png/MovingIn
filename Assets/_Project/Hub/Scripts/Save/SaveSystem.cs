using System;
using System.Collections.Generic;
using System.IO;
using MovingIn.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Hub{
    public static class SaveSystem
    {
        static SaveData _pending;

        static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

        public static bool HasSave => File.Exists(SavePath);
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

        public static void AutoSave()
        {
            if (!CanSave) return;
            try
            {
                File.WriteAllText(SavePath, JsonUtility.ToJson(Capture(), true));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Nao consegui salvar: {e.Message}");
            }
        }

        public static bool Load()
        {
            if (!HasSave) return false;

            SaveData data;
            try
            {
                data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveSystem] Save corrompido: {e.Message}");
                return false;
            }
            if (data == null) return false;

            GameProgress.ResetAll();
            if (data.completed != null)
                foreach (string id in data.completed)
                    GameProgress.MarkCompleted(id);

            _pending = data;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            return true;
        }

        public static void Delete()
        {
            if (HasSave) File.Delete(SavePath);
        }

        public static bool TakePending(out SaveData data)
        {
            data = _pending;
            _pending = null;
            return data != null;
        }

        static SaveData Capture()
        {
            StoryManager sm = StoryManager.Instance;
            var done = new List<string>();
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
            };
        }
    }
}
