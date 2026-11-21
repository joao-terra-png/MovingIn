using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class EndingDirector : MonoBehaviour
    {
        public static EndingDirector Instance { get; private set; }

        [Header("Scene")]
        [SerializeField] Camera cam;
        [SerializeField] Transform player;
        [SerializeField] Speaker playerSpeaker;
        [SerializeField] Behaviour[] cameraFollowers;
        [SerializeField] Room frontRoom;
        [SerializeField] Transform frontSpawn;
        [SerializeField] Transform car;
        [SerializeField] Vector3 _carDrive = new Vector3(26f, 0f, 0f);
        [SerializeField] float _carTime = 4f;
        [SerializeField] NpcWalker mom;
        [SerializeField] NpcWalker dad;
        [SerializeField] Transform momSpot;
        [SerializeField] Transform dadSpot;
        [SerializeField] Transform kidSpot;
        [SerializeField] Transform carDoor;
        [SerializeField] Transform[] arrivalPath;
        [SerializeField] GameObject photoExtras;

        [Header("Camera")]
        [SerializeField] Vector3 _wideFocus = new Vector3(7.5f, 1f, -7.5f);
        [SerializeField] float _wideSize = 7.5f;
        [SerializeField] Vector3 _photoFocus = new Vector3(10f, 1f, -2.6f);
        [SerializeField] float _photoSize = 3.2f;
        [SerializeField] float _distance = 30f;

        [Header("UI")]
        [SerializeField] CanvasGroup black;
        [SerializeField] CanvasGroup flash;
        [SerializeField] RectTransform polaroid;
        [SerializeField] RawImage photo;
        [SerializeField] TextMeshProUGUI caption;
        [SerializeField] CanvasGroup credits;
        [SerializeField] RectTransform creditsContent;
        [SerializeField] TextMeshProUGUI creditsText;
        [SerializeField] TextMeshProUGUI continueHint;
        [SerializeField, TextArea(3, 12)] string _creditsBody = "Projeto final de AED1 · UFSCar\n\n<size=120%><b>Equipe</b></size>\nDavi Filipe Del Bianco\nFelipe Gabriel Rabello\nJoão Gabriel Azevedo Terra\nNicolas Catini Ramalho dos Santos\n\n<size=120%><b>Minigames</b></size>\n<b>Hamsters' Pipe</b>\nDavi Filipe Del Bianco\nFelipe Gabriel Rabello\n\n<b>Fuse Defense</b>\nNicolas Catini Ramalho dos Santos\n\n<b>Kitchen Organizer · Laundry · Box Tetris</b>\nJoão Gabriel Azevedo Terra\n\n<size=120%><b>Arte</b></size>\n@krol.psd (Kitchen Organizer)\nJoão Gabriel Azevedo Terra\nFelipe Gabriel Rabello\n\n<size=120%><b>Músicas</b></size>\n\"Main Title Theme\"\n\"Night Walk\"\nJonathan Morali\nLife is Strange - Original Soundtrack\nDONTNOD Entertainment / Square Enix, 2015\n\n<size=120%><b>Efeitos sonoros</b></size>\nInterface Sounds\nKenney (kenney.nl) - CC0\n\n<size=120%><b>Assets</b></size>\nModelos 3D: Furniture Kit\nKenney (kenney.nl) - CC0\n\nFonte: Pixelify Sans\nThe Pixelify Sans Project Authors\nSIL Open Font License 1.1\n\n<size=120%><b>Ferramentas</b></size>\nUnity 2022.3 · C#\nTextMesh Pro\n\nObrigado por jogar!";
        [SerializeField] float _creditsSpeed = 60f;
        [SerializeField] float _creditsBottomMargin = 160f;
        [Tooltip("Fala depois dos creditos avisando que da para rejogar. Vazio = sem fala")]
        [SerializeField] string _replayTip = "Agora dá pra jogar os minigames de novo! É só apertar ESC e escolher Minigames.";

        public static bool Playing { get; private set; }
        public static bool Finished { get; private set; }

        Texture2D _shot;
        Behaviour[] _followers;
        bool _holdCamera;
        Vector3 _camFrom;
        float _sizeFrom;
        Vector3 _camTo;
        float _sizeTo;
        float _camT;

        void Awake()
        {
            Instance = this;
            if (cam == null) cam = Camera.main;
            black.alpha = 0f;
            flash.alpha = 0f;
            credits.alpha = 0f;
            polaroid.gameObject.SetActive(false);
            continueHint.gameObject.SetActive(false);
            if (car != null) car.gameObject.SetActive(false);
            if (mom != null) mom.Show(false);
            if (dad != null) dad.Show(false);
            if (photoExtras != null) photoExtras.SetActive(false);
        }

        void LateUpdate()
        {
            if (!_holdCamera || cam == null) return;
            float s = Mathf.SmoothStep(0f, 1f, _camT);
            cam.transform.rotation = Quaternion.Euler(30f, 45f, 0f);
            cam.transform.position = Vector3.Lerp(_camFrom, _camTo, s);
            cam.orthographicSize = Mathf.Lerp(_sizeFrom, _sizeTo, s);
        }

        public IEnumerator Play()
        {
            Playing = true;
            Cutscene.Playing = true;
            DialogueManager dm = DialogueManager.Instance;

            yield return Cutscene.Fade(black, 1f, 1f);

            RoomManager rm = RoomManager.Instance;
            if (rm != null && frontRoom != null && rm.CurrentRoom != frontRoom)
            {
                rm.GoToRoom(frontRoom, frontSpawn);
                float limit = Time.time + 3f;
                while (Time.time < limit && (rm.IsTransitioning || rm.CurrentRoom != frontRoom)) yield return null;
            }
            if (player != null && kidSpot != null)
                Cutscene.Teleport(player, new Vector3(kidSpot.position.x, player.position.y, kidSpot.position.z + 0.6f));

            _followers = Array.FindAll(cameraFollowers, b => b != null && b.enabled);
            Cutscene.SetBehaviours(_followers, false);
            AimCamera(_wideFocus, _wideSize, true);

            Vector3 carHome = car != null ? car.position : Vector3.zero;
            if (car != null)
            {
                car.position = carHome - _carDrive;
                car.gameObject.SetActive(true);
            }

            yield return new WaitForSeconds(0.6f);
            yield return Cutscene.Fade(black, 0f, 1.2f);

            if (car != null) yield return Cutscene.Move(car, carHome, _carTime, false, true);
            yield return new WaitForSeconds(0.6f);

            if (dm != null) dm.ShowEmote(playerSpeaker, Emote.Exclaim, 1.5f);
            yield return Arrive(mom, momSpot, 0f);
            yield return Arrive(dad, dadSpot, 1f);
            float waitParents = Time.time + 8f;
            while (Time.time < waitParents && (Busy(mom) || Busy(dad))) yield return null;

            if (player != null && kidSpot != null)
            {
                CharacterController cc = player.GetComponent<CharacterController>();
                if (cc != null) cc.enabled = false;
                yield return Cutscene.Walk(player, kidSpot.position, 2f);
                if (cc != null) cc.enabled = true;
            }
            yield return Nudge(mom);
            yield return Nudge(dad);
            if (photoExtras != null) photoExtras.SetActive(true);

            if (dm != null)
            {
                bool done = false;
                dm.Play(new[]
                {
                    new DialogueLine(mom != null ? mom.GetComponent<Speaker>() : null, "Chegamos!"),
                    new DialogueLine(dad != null ? dad.GetComponent<Speaker>() : null, "Olha só, a casa tá prontinha!"),
                    new DialogueLine(null, "Bem-vindos à casa nova!"),
                    new DialogueLine(mom != null ? mom.GetComponent<Speaker>() : null, "Todo mundo junto pra foto!"),
                }, true, () => done = true);
                while (!done) yield return null;
            }

            FaceFront(player);
            if (mom != null) FaceFront(mom.transform);
            if (dad != null) FaceFront(dad.transform);

            AimCamera(_photoFocus, _photoSize, false);
            while (_camT < 1f)
            {
                _camT += Time.deltaTime / 1.5f;
                yield return null;
            }
            yield return new WaitForSeconds(0.6f);

            yield return new WaitForEndOfFrame();
            if (_shot != null) Destroy(_shot);
            _shot = ScreenCapture.CaptureScreenshotAsTexture();
            flash.alpha = 1f;
            yield return Cutscene.Fade(flash, 0f, 0.8f);

            photo.texture = _shot;
            caption.text = $"Casa nova · {DateTime.Now.Year}";
            polaroid.gameObject.SetActive(true);
            Vector2 shown = polaroid.anchoredPosition;
            Vector2 hidden = shown + Vector2.down * 900f;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / 0.7f;
                polaroid.anchoredPosition = Vector2.Lerp(hidden, shown, 1f - (1f - t) * (1f - t));
                yield return null;
            }
            polaroid.anchoredPosition = shown;
            yield return new WaitForSeconds(3.5f);

            yield return Cutscene.Fade(black, 1f, 1.2f);
            polaroid.gameObject.SetActive(false);
            yield return RollCredits();

            continueHint.gameObject.SetActive(true);
            while (!Input.GetKeyDown(KeyCode.E) && !Input.GetKeyDown(KeyCode.Return)) yield return null;
            continueHint.gameObject.SetActive(false);

            yield return Cutscene.Fade(credits, 0f, 0.6f);
            _holdCamera = false;
            cam.orthographicSize = 4.5f;
            Cutscene.SetBehaviours(_followers, true);
            yield return Cutscene.Fade(black, 0f, 1f);

            Finished = true;
            if (dm != null && !string.IsNullOrEmpty(_replayTip))
            {
                bool tipDone = false;
                dm.Play(new[] { new DialogueLine(null, _replayTip) }, true, () => tipDone = true);
                while (!tipDone) yield return null;
            }

            Playing = false;
            Cutscene.Playing = false;
        }

        void AimCamera(Vector3 focus, float size, bool instant)
        {
            _camFrom = cam.transform.position;
            _sizeFrom = cam.orthographicSize;
            _camTo = focus - Quaternion.Euler(30f, 45f, 0f) * Vector3.forward * _distance;
            _sizeTo = size;
            _camT = instant ? 1f : 0f;
            _holdCamera = true;
        }

        IEnumerator RollCredits()
        {
            creditsText.text = $"<size=160%>Moving In</size>\n\n{_creditsBody}";
            Canvas.ForceUpdateCanvases();
            float height = creditsText.preferredHeight;
            float view = ((RectTransform)creditsContent.parent).rect.height;
            creditsContent.anchoredPosition = new Vector2(0f, -view / 2f - height / 2f);
            yield return Cutscene.Fade(credits, 1f, 0.8f);

            float stop = Mathf.Max(0f, (height - view) / 2f + _creditsBottomMargin);
            while (creditsContent.anchoredPosition.y < stop)
            {
                creditsContent.anchoredPosition += Vector2.up * (_creditsSpeed * Time.unscaledDeltaTime);
                if (Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Return))
                    creditsContent.anchoredPosition += Vector2.up * (_creditsSpeed * 3f * Time.unscaledDeltaTime);
                yield return null;
            }
            creditsContent.anchoredPosition = new Vector2(0f, stop);
        }

        IEnumerator Arrive(NpcWalker npc, Transform spot, float delay)
        {
            if (npc == null || spot == null || carDoor == null) yield break;
            yield return new WaitForSeconds(delay);
            npc.transform.position = new Vector3(carDoor.position.x, npc.transform.position.y, carDoor.position.z);
            npc.Show(true);
            StartCoroutine(Route(npc, spot));
        }

        IEnumerator Route(NpcWalker npc, Transform spot)
        {
            if (arrivalPath != null)
                foreach (Transform p in arrivalPath)
                    if (p != null) yield return npc.WalkTo(p.position);
            yield return npc.WalkTo(spot.position);
        }

        static void FaceFront(Transform t)
        {
            if (t == null) return;
            foreach (PlayerSpriteVisual v in t.GetComponentsInChildren<PlayerSpriteVisual>(true)) v.FaceFront();
        }

        IEnumerator Nudge(NpcWalker npc)
        {
            if (npc == null) yield break;
            Vector3 towardCamera = Quaternion.Euler(0f, 45f, 0f) * Vector3.back * 0.15f;
            yield return Cutscene.Walk(npc.transform, npc.transform.position + towardCamera, 1f);
        }

        bool Busy(NpcWalker npc)
        {
            if (npc == null) return false;
            Transform spot = npc == mom ? momSpot : dadSpot;
            if (spot == null) return false;
            Vector3 a = npc.transform.position;
            Vector3 b = spot.position;
            a.y = b.y = 0f;
            return (a - b).sqrMagnitude > 0.01f;
        }
    }
}
