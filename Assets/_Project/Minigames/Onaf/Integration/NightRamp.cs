using System;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class NightRamp : MonoBehaviour
    {
        [Serializable]
        public class Entry
        {
            [Tooltip("Nome do objeto do boneco na cena")]
            public string monster = "Rato";
            [Tooltip("Momento da noite em que ele comeca a andar (0 = meia-noite, 1 = 6 AM)")]
            [Range(0f, 1f)] public float startAt = 0.33f;
        }

        [SerializeField] Entry[] _schedule = { new Entry() };

        NightManager _night;
        readonly List<Boneco> _waiting = new List<Boneco>();
        readonly List<float> _startAt = new List<float>();

        void Start()
        {
            _night = OnafUtil.EncontrarNaCena<NightManager>(gameObject.scene);

            var all = new List<Boneco>();
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                all.AddRange(root.GetComponentsInChildren<Boneco>(true));

            foreach (Entry entry in _schedule)
            {
                Boneco monster = all.Find(b => b.name == entry.monster);
                if (monster == null || entry.startAt <= 0f) continue;
                monster.SetMovEnable(false);
                _waiting.Add(monster);
                _startAt.Add(entry.startAt);
            }
        }

        void Update()
        {
            if (_night == null || _waiting.Count == 0 || !_night.NoiteEstaAtiva()) return;

            float progress = _night.GetProgresso();
            for (int i = _waiting.Count - 1; i >= 0; i--)
            {
                if (progress < _startAt[i]) continue;
                if (_waiting[i] != null) _waiting[i].SetMovEnable(true);
                _waiting.RemoveAt(i);
                _startAt.RemoveAt(i);
            }
        }
    }
}
