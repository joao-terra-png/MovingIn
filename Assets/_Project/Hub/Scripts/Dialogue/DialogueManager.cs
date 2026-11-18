using System;
using UnityEngine;
using MovingIn.Minigames.Kitchen.ADTs;

namespace MovingIn.Hub{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager Instance { get; private set; }

        [SerializeField] SpeechBubbleUI bubble;
        [SerializeField] Speaker playerSpeaker;
        [SerializeField] AudioSource voice;
        [SerializeField] AudioClip blip;
        [SerializeField] Behaviour[] lockDuringDialogue;
        [SerializeField] KeyCode advanceKey = KeyCode.E;
        [SerializeField] int _capacity = 32;

        [Header("Typing")]
        [SerializeField] float _charsPerSecond = 40f;
        [SerializeField] float _punctuationPause = 0.18f;
        [SerializeField] int _blipEvery = 2;
        [SerializeField] float _blipVolume = 0.5f;

        [Header("Timing")]
        [SerializeField] float _baseHold = 1.2f;
        [SerializeField] float _holdPerChar = 0.045f;
        [SerializeField] float _emoteOnlyHold = 1.1f;
        [SerializeField] float _emoteLead = 0.6f;
        [SerializeField] float _gapBetweenLines = 0.12f;

        StaticQueue<DialogueLine> _queue;
        DialogueLine _current;
        int _visible;
        int _total;
        float _charTimer;
        float _holdTimer;
        float _gapTimer;
        bool _typed;
        bool _bubbleOpen;
        float _emoteDelay;
        int _lockedPending;
        bool _locked;
        int _closedFrame = -10;

        public Speaker PlayerSpeaker => playerSpeaker;
        public bool IsBusy => _current != null || _queue.Quantity > 0;
        public bool BlocksInteraction => IsBusy || Time.frameCount - _closedFrame <= 1;
        public bool IsWaitingForInput => _current != null && _bubbleOpen && _typed && _current.waitForInput;
        public float CharsPerSecond
        {
            get => _charsPerSecond;
            set => _charsPerSecond = Mathf.Max(5f, value);
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _queue = new StaticQueue<DialogueLine>(_capacity);
            if (bubble != null) bubble.Close();
        }

        public void Say(string text, Emote emote = Emote.None)
        {
            Say(playerSpeaker, text, emote);
        }

        public void Say(Speaker speaker, string text, Emote emote = Emote.None)
        {
            Enqueue(new DialogueLine(speaker, text, emote));
        }

        public void Play(DialogueLine[] lines, bool lockPlayer = false, Action onFinished = null)
        {
            if (lines == null || lines.Length == 0)
            {
                onFinished?.Invoke();
                return;
            }

            for (int i = 0; i < lines.Length; i++)
            {
                DialogueLine line = lines[i].Copy();
                if (line.speaker == null) line.speaker = playerSpeaker;
                line.lockPlayer = lockPlayer;
                if (lockPlayer) line.waitForInput = true;
                if (i == lines.Length - 1) line.onDone = onFinished;
                Enqueue(line);
            }
        }

        public void ShowEmote(Speaker speaker, Emote emote, float duration = 1.5f)
        {
            if (speaker == null) speaker = playerSpeaker;
            if (speaker != null && speaker.Emotes != null) speaker.Emotes.Show(emote, duration);
        }

        public void ClearAll()
        {
            while (_queue.Quantity > 0) _queue.Remove(out DialogueLine _);
            _lockedPending = 0;
            _current = null;
            _gapTimer = 0f;
            _emoteDelay = 0f;
            if (bubble != null) bubble.Close();
            _bubbleOpen = false;
            SetLock(false);
        }

        void Enqueue(DialogueLine line)
        {
            int before = _queue.Quantity;
            _queue.Insert(line);
            if (_queue.Quantity == before)
            {
                Debug.LogWarning($"[DialogueManager] Fila de falas cheia ({_capacity}). A fala \"{line.text}\" foi ignorada.");
                return;
            }
            if (line.lockPlayer) _lockedPending++;
        }

        void Update()
        {
            float dt = Time.deltaTime;

            if (_current == null)
            {
                if (_gapTimer > 0f)
                {
                    _gapTimer -= dt;
                    return;
                }
                if (_queue.Quantity > 0) StartNext();
                return;
            }

            if (_emoteDelay > 0f)
            {
                _emoteDelay -= dt;
                if (_emoteDelay <= 0f) OpenBubble();
                return;
            }

            bool pressed = Time.timeScale > 0f && Input.GetKeyDown(advanceKey);

            if (!_typed)
            {
                if (pressed)
                {
                    _visible = _total;
                    bubble.SetVisible(_visible);
                    OnTyped();
                    return;
                }
                Type(dt);
                return;
            }

            if (pressed && _bubbleOpen)
            {
                Finish();
                return;
            }

            if (!_current.waitForInput || !_bubbleOpen)
            {
                _holdTimer -= dt;
                if (_holdTimer <= 0f) Finish();
            }
        }

        void StartNext()
        {
            _queue.Remove(out DialogueLine next);
            _current = next;
            if (_current == null) return;
            if (_current.lockPlayer)
            {
                _lockedPending = Mathf.Max(0, _lockedPending - 1);
                SetLock(true);
            }

            if (_current.speaker == null) _current.speaker = playerSpeaker;
            if (_current.speaker == null)
            {
                Debug.LogWarning("[DialogueManager] Fala sem Speaker e sem Player Speaker definido.");
                Finish();
                return;
            }

            _emoteDelay = 0f;
            _bubbleOpen = false;
            _typed = false;

            if (string.IsNullOrEmpty(_current.text))
            {
                if (_current.emote != Emote.None) ShowEmote(_current.speaker, _current.emote, _emoteOnlyHold);
                _typed = true;
                _holdTimer = _emoteOnlyHold;
                return;
            }

            if (_current.emote != Emote.None)
            {
                ShowEmote(_current.speaker, _current.emote, _emoteLead + 0.1f);
                _emoteDelay = _emoteLead;
                return;
            }

            OpenBubble();
        }

        void OpenBubble()
        {
            _emoteDelay = 0f;
            _bubbleOpen = true;
            _total = _current.text.Length;
            _visible = 0;
            _charTimer = 0f;
            _typed = false;
            if (_current.speaker.Emotes != null) _current.speaker.Emotes.Hide();
            bubble.Open(_current.speaker, _current.text);
        }

        void Type(float dt)
        {
            _charTimer += dt * _charsPerSecond;
            while (_charTimer >= 1f && _visible < _total)
            {
                _charTimer -= 1f;
                _visible++;
                char c = _current.text[_visible - 1];
                if (char.IsLetterOrDigit(c) && (_visible - 1) % Mathf.Max(1, _blipEvery) == 0) Blip();
                if (c == '.' || c == '!' || c == '?' || c == ',' || c == '…') _charTimer -= _punctuationPause * _charsPerSecond;
            }
            bubble.SetVisible(_visible);
            if (_visible >= _total) OnTyped();
        }

        void OnTyped()
        {
            _typed = true;
            _holdTimer = _baseHold + _holdPerChar * _total;
            bubble.SetAdvance(true);
        }

        void Finish()
        {
            if (_bubbleOpen && bubble != null) bubble.Close();
            _bubbleOpen = false;
            _emoteDelay = 0f;

            DialogueLine done = _current;
            _current = null;
            _closedFrame = Time.frameCount;
            _gapTimer = _gapBetweenLines;

            if (done != null && done.lockPlayer && _lockedPending == 0) SetLock(false);
            done?.onDone?.Invoke();
        }

        void SetLock(bool on)
        {
            if (_locked == on) return;
            _locked = on;
            foreach (Behaviour b in lockDuringDialogue)
                if (b != null) b.enabled = !on;
        }

        void Blip()
        {
            if (voice == null || blip == null || _current == null) return;
            voice.pitch = _current.speaker.VoicePitch * UnityEngine.Random.Range(0.94f, 1.06f);
            voice.PlayOneShot(blip, _blipVolume);
        }

        [ContextMenu("Testar falas")]
        void TestLines()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[DialogueManager] Aperte Play antes de testar.");
                return;
            }
            Play(new[]
            {
                new DialogueLine(null, "Ufa! Finalmente acabou o descarregamento."),
                new DialogueLine(null, "Hora de descansar um pouco..."),
                new DialogueLine(null, "", Emote.Exclaim),
                new DialogueLine(null, "Ué? Caiu a luz?"),
            }, true, () => Debug.Log("[DialogueManager] Teste terminou."));
        }

        [ContextMenu("Testar fala solta")]
        void TestSingle()
        {
            if (!Application.isPlaying) return;
            Say("Essa caixa diz FRÁGIL... melhor nem mexer.", Emote.Question);
        }
    }
}
