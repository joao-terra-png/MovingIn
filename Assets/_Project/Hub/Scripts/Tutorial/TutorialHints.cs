using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class TutorialHints : MonoBehaviour
    {
        enum Hint { None, Move, Interact, Advance }

        [SerializeField] CanvasGroup group;
        [SerializeField] RectTransform keysRoot;
        [SerializeField] Image keyTemplate;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] Transform player;
        [SerializeField] PlayerInteractor interactor;
        [SerializeField] KeyCode interactKey = KeyCode.E;
        [SerializeField] float _startDelay = 2.5f;
        [SerializeField] float _moveDistance = 2f;
        [SerializeField] float _fadeTime = 0.25f;

        readonly List<GameObject> _keys = new List<GameObject>();
        Hint _shown = Hint.None;
        bool _moveDone;
        bool _interactDone;
        bool _advanceDone;
        float _walked;
        float _startTime;
        Vector3 _lastPos;

        void Start()
        {
            _startTime = Time.time;
            if (player != null) _lastPos = player.position;
            keyTemplate.gameObject.SetActive(false);
            group.alpha = 0f;
        }

        void Update()
        {
            TrackWalk();
            if (Input.GetKeyDown(interactKey))
            {
                if (_shown == Hint.Interact) _interactDone = true;
                if (_shown == Hint.Advance) _advanceDone = true;
            }

            Hint want = Pick();
            float step = Time.unscaledDeltaTime / _fadeTime;
            if (want != _shown)
            {
                group.alpha = Mathf.MoveTowards(group.alpha, 0f, step);
                if (group.alpha <= 0f)
                {
                    _shown = want;
                    if (want != Hint.None) Fill(want);
                }
            }
            else
            {
                group.alpha = Mathf.MoveTowards(group.alpha, want == Hint.None ? 0f : 1f, step);
            }
        }

        void TrackWalk()
        {
            if (player == null) return;
            if (Cutscene.Playing)
            {
                _lastPos = player.position;
                return;
            }
            Vector3 d = player.position - _lastPos;
            d.y = 0f;
            _lastPos = player.position;
            if (d.magnitude < 1f) _walked += d.magnitude;
            if (_walked >= _moveDistance) _moveDone = true;
        }

        Hint Pick()
        {
            if (Cutscene.Playing)
            {
                _startTime = Time.time;
                return Hint.None;
            }
            DialogueManager dm = DialogueManager.Instance;
            if (!_advanceDone && dm != null && dm.IsWaitingForInput) return Hint.Advance;
            if (Time.time - _startTime < _startDelay) return Hint.None;
            if (!_moveDone) return Hint.Move;
            if (dm != null && dm.IsBusy) return Hint.None;
            if (!_interactDone && interactor != null && interactor.isActiveAndEnabled && interactor.HasTarget) return Hint.Interact;
            return Hint.None;
        }

        void Fill(Hint hint)
        {
            foreach (GameObject k in _keys) Destroy(k);
            _keys.Clear();

            string key = interactKey.ToString();
            switch (hint)
            {
                case Hint.Move:
                    AddKey("W"); AddKey("A"); AddKey("S"); AddKey("D");
                    label.text = "andar";
                    break;
                case Hint.Interact:
                    AddKey(key);
                    label.text = "interagir";
                    break;
                case Hint.Advance:
                    AddKey(key);
                    label.text = "continuar";
                    break;
            }
        }

        void AddKey(string letter)
        {
            Image k = Instantiate(keyTemplate, keysRoot);
            k.gameObject.SetActive(true);
            TextMeshProUGUI t = k.GetComponentInChildren<TextMeshProUGUI>(true);
            if (t != null) t.text = letter;
            _keys.Add(k.gameObject);
        }
    }
}
