using System;
using System.Collections;
using System.Reflection;
using MovingIn.Core;
using UnityEngine;

namespace MovingIn.Hub{
    public enum Chapter { Arrival, Blackout, PowerRestored, Afternoon, FirstNight, Morning }

    [Serializable]
    public class TrackedMinigame
    {
        public string id;
        public MinigameInteractable minigame;
        public MinigameGate gate;
        public string doneLine;
        [NonSerialized] public bool seenDone;
    }

    public class StoryManager : MonoBehaviour
    {
        public static StoryManager Instance { get; private set; }

        [SerializeField] Chapter _startChapter = Chapter.Arrival;
        [SerializeField] TimeOfDay timeOfDay;
        [SerializeField] TrackedMinigame[] minigames;
        [SerializeField] GameObject[] blackoutOnly;
        [SerializeField] GameObject flashlight;
        [SerializeField] GameObject firstNightBed;
        [SerializeField] PhoneCardUI phoneCard;
        [SerializeField] Speaker mom;
        [SerializeField] Speaker dad;
        [SerializeField] AudioSource ring;
        [SerializeField] CanvasGroup fade;
        [SerializeField] Behaviour[] lockDuringScenes;

        [Header("Ids")]
        [SerializeField] string _breakerId = "basement_breaker";
        [SerializeField] string _firstNightId = "bedroom_first_night";

        [Header("Rooms")]
        [SerializeField] string _blackoutRoom = "LivingRoom";
        [SerializeField] string _breakerRoom = "Basement";

        [Header("Timing")]
        [SerializeField] float _lightsTime = 1.5f;
        [SerializeField] float _nightfallTime = 4f;
        [SerializeField] float _fadeTime = 1.2f;
        [SerializeField] float _darkPause = 1.8f;

        bool _callDone;
        bool _busy;
        bool _breakerRoomSeen;
        bool _ready;
        bool _flashlightOn;
        string _lastRoom;

        public Chapter Current { get; private set; }
        public bool IsBusy => _busy;
        public bool CanSleep => Current == Chapter.PowerRestored && !_busy;
        public TrackedMinigame[] Minigames => minigames;
        public Chapter StartChapter => _startChapter;

        DialogueManager Dm => DialogueManager.Instance;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            if (fade != null)
            {
                fade.alpha = 0f;
                fade.blocksRaycasts = false;
            }
        }

        IEnumerator Start()
        {
            yield return null;
            while (MainMenu.Active) yield return null;

            bool loaded = SaveSystem.TakePending(out SaveData save);
            Chapter start = _startChapter;
            if (loaded && Enum.TryParse(save.chapter, out Chapter saved)) start = saved;

            foreach (TrackedMinigame t in minigames)
                t.seenDone = GameProgress.IsCompleted(t.id) || (t.minigame != null && t.minigame.IsCompleted);

            _callDone = start >= Chapter.Afternoon;
            _flashlightOn = start == Chapter.Blackout;
            EnterChapter(start, 0f);

            if (loaded) yield return MoveToRoom(save.room);

            _lastRoom = CurrentRoomName();
            _ready = true;

            if (!loaded && start == Chapter.Arrival)
            {
                yield return new WaitForSeconds(1f);
                if (Dm != null)
                {
                    Dm.Say("Ufa! Finalmente acabou o descarregamento.");
                    Dm.Say("Hora de descansar um pouco...");
                }
            }
        }

        IEnumerator MoveToRoom(string roomName)
        {
            RoomManager rm = RoomManager.Instance;
            if (rm == null || string.IsNullOrEmpty(roomName) || roomName == CurrentRoomName()) yield break;

            Room target = null;
            foreach (Room r in FindObjectsOfType<Room>(true))
                if (r.name == roomName) target = r;
            if (target == null) yield break;

            Transform spawns = target.transform.Find("Spawns");
            Transform spawn = spawns != null && spawns.childCount > 0 ? spawns.GetChild(0) : target.transform;
            rm.GoToRoom(target, spawn);

            float limit = Time.time + 3f;
            while (Time.time < limit && (rm.IsTransitioning || rm.CurrentRoom != target))
                yield return null;
        }

        void Update()
        {
            if (!_ready || _busy) return;
            WatchRoom();
            WatchMinigames();
        }

        void WatchRoom()
        {
            RoomManager rm = RoomManager.Instance;
            if (rm == null || rm.IsTransitioning) return;
            string room = CurrentRoomName();
            if (room == _lastRoom) return;
            _lastRoom = room;

            if (Current == Chapter.Arrival && room == _blackoutRoom)
                StartCoroutine(BlackoutScene());
            else if (Current == Chapter.Blackout && room == _breakerRoom && !_breakerRoomSeen)
            {
                _breakerRoomSeen = true;
                Dm?.Say("Achei o quadro de luz!", Emote.Exclaim);
            }
        }

        void WatchMinigames()
        {
            foreach (TrackedMinigame t in minigames)
            {
                if (t.seenDone || t.minigame == null || !t.minigame.IsCompleted) continue;
                t.seenDone = true;
                OnMinigameDone(t);
                return;
            }
        }

        void OnMinigameDone(TrackedMinigame t)
        {
            if (t.id == _breakerId)
            {
                StartCoroutine(PowerBackScene());
                return;
            }
            if (t.id == _firstNightId)
            {
                StartCoroutine(MorningScene());
                return;
            }

            if (!string.IsNullOrEmpty(t.doneLine)) Dm?.Say(t.doneLine);
            int left = RemainingAfternoon();
            if (left == 0) StartCoroutine(FirstNightScene());
            else
            {
                Dm?.Say(left == 1 ? "Só falta um!" : $"Faltam {left}.");
                SaveSystem.AutoSave();
            }
        }

        int RemainingAfternoon()
        {
            int left = 0;
            foreach (TrackedMinigame t in minigames)
                if (t.id != _breakerId && t.id != _firstNightId && !t.seenDone) left++;
            return left;
        }

        void EnterChapter(Chapter chapter, float lightTime)
        {
            Current = chapter;
            if (timeOfDay != null) timeOfDay.Apply(PresetFor(chapter), lightTime);
            foreach (GameObject go in blackoutOnly)
                if (go != null) go.SetActive(chapter == Chapter.Blackout);
            if (flashlight != null) flashlight.SetActive(chapter == Chapter.Blackout && _flashlightOn);
            if (firstNightBed != null) firstNightBed.SetActive(chapter == Chapter.FirstNight);
            RefreshGates();
        }

        static string PresetFor(Chapter chapter)
        {
            switch (chapter)
            {
                case Chapter.Arrival: return "Evening";
                case Chapter.Blackout: return "Blackout";
                case Chapter.PowerRestored: return "Night";
                case Chapter.Afternoon: return "Afternoon";
                case Chapter.FirstNight: return "FirstNight";
                default: return "Dawn";
            }
        }

        void RefreshGates()
        {
            string line = LockedLine();
            foreach (TrackedMinigame t in minigames)
                if (t.gate != null) t.gate.SetLocked(!IsOpen(t.id), line);
        }

        bool IsOpen(string id)
        {
            switch (Current)
            {
                case Chapter.Blackout: return id == _breakerId;
                case Chapter.Afternoon: return _callDone && id != _breakerId && id != _firstNightId;
                case Chapter.FirstNight: return id == _firstNightId;
                case Chapter.Morning: return true;
                default: return false;
            }
        }

        string LockedLine()
        {
            switch (Current)
            {
                case Chapter.Arrival: return "Depois eu vejo isso. Tô cansado.";
                case Chapter.Blackout: return "Primeiro preciso resolver a luz.";
                case Chapter.PowerRestored: return "Tô com muito sono. Amanhã eu arrumo.";
                case Chapter.Afternoon: return _callDone ? "Agora não." : "Peraí, o celular!";
                case Chapter.FirstNight: return "Agora é hora de dormir.";
                default: return "Agora não.";
            }
        }

        public void Sleep()
        {
            if (!CanSleep) return;
            StartCoroutine(SleepScene());
        }

        IEnumerator BlackoutScene()
        {
            Begin();
            _flashlightOn = false;
            yield return new WaitForSeconds(0.8f);
            if (timeOfDay != null) yield return timeOfDay.Flicker();
            EnterChapter(Chapter.Blackout, 0.15f);
            yield return new WaitForSeconds(_darkPause);
            yield return Talk(true,
                new DialogueLine(null, "", Emote.Exclaim),
                new DialogueLine(null, "Ué?"),
                new DialogueLine(null, "Ih... caiu o disjuntor?"),
                new DialogueLine(null, "Peraí... o celular tem lanterna."));
            yield return new WaitForSeconds(0.6f);
            yield return FlashlightOn();
            yield return new WaitForSeconds(1f);
            yield return Talk(true,
                new DialogueLine(null, "Pronto. Agora dá pra ver alguma coisa."),
                new DialogueLine(null, "O quadro de luz deve estar no porão."));
            End();
        }

        IEnumerator FlashlightOn()
        {
            _flashlightOn = true;
            if (flashlight == null) yield break;
            flashlight.SetActive(true);
            yield return new WaitForSeconds(0.08f);
            flashlight.SetActive(false);
            yield return new WaitForSeconds(0.15f);
            flashlight.SetActive(true);
            yield return new WaitForSeconds(0.06f);
            flashlight.SetActive(false);
            yield return new WaitForSeconds(0.1f);
            flashlight.SetActive(true);
        }

        IEnumerator PowerBackScene()
        {
            Begin();
            yield return new WaitForSeconds(0.8f);
            EnterChapter(Chapter.PowerRestored, _lightsTime);
            yield return new WaitForSeconds(_lightsTime);
            yield return Talk(true,
                new DialogueLine(null, "Pronto, luz de volta!", Emote.Note),
                new DialogueLine(null, "Agora sim, posso dormir."));
            End();
        }

        IEnumerator SleepScene()
        {
            Begin();
            yield return Fade(1f);
            _callDone = false;
            EnterChapter(Chapter.Afternoon, 0f);
            yield return new WaitForSeconds(1f);
            yield return Fade(0f);
            Dm?.ShowEmote(null, Emote.Sleep, 2f);
            yield return new WaitForSeconds(2.2f);
            yield return Talk(true,
                new DialogueLine(null, "", Emote.Exclaim),
                new DialogueLine(null, "Opa... dormi demais!"));
            yield return new WaitForSeconds(1.2f);
            yield return PhoneCall(
                new DialogueLine(mom, "Filho! A gente vai demorar um pouco."),
                new DialogueLine(dad, "Vai arrumando a casa pra gente?"),
                new DialogueLine(null, "Deixa comigo!"));
            _callDone = true;
            RefreshGates();
            yield return Talk(false, new DialogueLine(null, "Bora arrumar essa casa."));
            End();
        }

        IEnumerator FirstNightScene()
        {
            Begin();
            yield return new WaitUntil(() => Dm == null || !Dm.IsBusy);
            yield return new WaitForSeconds(1f);
            EnterChapter(Chapter.FirstNight, _nightfallTime);
            yield return new WaitForSeconds(_nightfallTime);
            yield return PhoneCall(
                new DialogueLine(mom, "Filho, só chegamos amanhã cedo, tá bom?"),
                new DialogueLine(null, "Tá bom..."));
            yield return Talk(true,
                new DialogueLine(null, "", Emote.Dots),
                new DialogueLine(null, "Primeira noite oficialmente na casa nova..."));
            End();
        }

        IEnumerator MorningScene()
        {
            Begin();
            yield return new WaitForSeconds(0.8f);
            EnterChapter(Chapter.Morning, 3f);
            yield return new WaitForSeconds(2f);
            yield return Talk(true,
                new DialogueLine(null, "Amanheceu!", Emote.Exclaim),
                new DialogueLine(null, "Sobrevivi à primeira noite."),
                new DialogueLine(null, "Eles devem estar chegando!"));
            if (EndingDirector.Instance != null) yield return EndingDirector.Instance.Play();
            End();
        }

        IEnumerator PhoneCall(params DialogueLine[] lines)
        {
            if (ring != null) ring.Play();
            Dm?.ShowEmote(null, Emote.Note, 0f);
            yield return new WaitForSeconds(1.5f);
            yield return Talk(true, new DialogueLine(null, "O celular! Deixa eu atender."));
            if (ring != null) ring.Stop();
            Dm?.ShowEmote(null, Emote.None, 0f);
            if (phoneCard != null) phoneCard.Show();
            yield return new WaitForSeconds(0.5f);
            yield return Talk(true, lines);
            if (phoneCard != null) phoneCard.Hide();
            yield return new WaitForSeconds(0.4f);
        }

        IEnumerator Talk(bool wait, params DialogueLine[] lines)
        {
            if (Dm == null) yield break;
            bool done = false;
            Dm.Play(lines, wait, () => done = true);
            yield return new WaitUntil(() => done);
            SetPlayerLock(true);
        }

        IEnumerator Fade(float target)
        {
            if (fade == null) yield break;
            fade.blocksRaycasts = target > 0f;
            float start = fade.alpha;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / _fadeTime;
                fade.alpha = Mathf.Lerp(start, target, Mathf.SmoothStep(0f, 1f, t));
                yield return null;
            }
            fade.alpha = target;
        }

        void Begin()
        {
            _busy = true;
            SetPlayerLock(true);
        }

        void End()
        {
            SetPlayerLock(false);
            _busy = false;
            _lastRoom = CurrentRoomName();
            SaveSystem.AutoSave();
        }

        void SetPlayerLock(bool on)
        {
            foreach (Behaviour b in lockDuringScenes)
                if (b != null) b.enabled = !on;
        }

        static string CurrentRoomName()
        {
            RoomManager rm = RoomManager.Instance;
            return rm != null && rm.CurrentRoom != null ? rm.CurrentRoom.name : "";
        }

        [ContextMenu("Avancar capitulo (teste)")]
        public void DevNextChapter()
        {
            if (!Application.isPlaying || _busy) return;
            Chapter next = Current == Chapter.Morning ? Chapter.Arrival : Current + 1;
            if (next == Chapter.Afternoon) _callDone = true;
            if (next == Chapter.Blackout) _flashlightOn = true;
            EnterChapter(next, 1f);
            _lastRoom = CurrentRoomName();
            Debug.Log($"[Story] Capitulo: {next}");
        }

        public void DevComplete(TrackedMinigame t)
        {
            if (t == null || t.minigame == null || t.minigame.IsCompleted) return;
            MethodInfo mark = typeof(MinigameInteractable).GetMethod("MarkAsCompleted", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mark == null)
            {
                Debug.LogWarning("[Story] MinigameInteractable nao tem o metodo MarkAsCompleted.");
                return;
            }
            GameProgress.MarkCompleted(t.id);
            mark.Invoke(t.minigame, null);
            Debug.Log($"[Story] Dev: {t.id} marcado como concluido.");
        }

        [ContextMenu("Testar ligacao")]
        void TestCall()
        {
            if (!Application.isPlaying || _busy) return;
            StartCoroutine(TestCallRoutine());
        }

        IEnumerator TestCallRoutine()
        {
            Begin();
            yield return PhoneCall(
                new DialogueLine(mom, "Filho! A gente vai demorar um pouco."),
                new DialogueLine(dad, "Vai arrumando a casa pra gente?"),
                new DialogueLine(null, "Deixa comigo!"));
            End();
        }
    }
}
