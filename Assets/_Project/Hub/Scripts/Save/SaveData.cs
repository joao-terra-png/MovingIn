using System;

namespace MovingIn.Hub{
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public string chapter;
        public string room;
        public string[] completed;
        public float playTime;
        public string savedAt;
    }
}
