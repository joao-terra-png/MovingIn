using System.Collections.Generic;
using System.Reflection;
using MovingIn.Hub;
using UnityEngine;

namespace MovingIn.Minigames.Onaf
{
    public class EntranceMonsters : MonoBehaviour
    {
        [Header("Comodos (ids da arvore)")]
        [SerializeField] int _playerRoomId = 60;
        [Tooltip("Quem esta neste comodo aparece na porta")]
        [SerializeField] int _doorRoomId = 20;
        [Tooltip("Quem esta neste comodo aparece na janela")]
        [SerializeField] int _windowRoomId = 80;

        [Header("Posicoes (usa as do DefesaManager quando marcado)")]
        [SerializeField] bool _readFromDefense = true;
        [SerializeField] Vector3 _doorPosition = new Vector3(9.85f, 21.1f, 4f);
        [SerializeField] Vector3 _doorRotation = new Vector3(0f, 90f, 0f);
        [SerializeField] float _doorHeight = 2.2f;
        [SerializeField] Vector3 _windowPosition = new Vector3(6.8f, 21.7f, 7.83f);
        [SerializeField] Vector3 _windowRotation = Vector3.zero;
        [SerializeField] float _windowHeight = 1.2f;

        [Header("Monstro")]
        [Tooltip("Altura do monstro parado no vao da porta")]
        [SerializeField] float _doorMonsterHeight = 1.9f;
        [Tooltip("Quanto o monstro fica para fora do quarto, no corredor")]
        [SerializeField] float _doorOutside = 0.35f;
        [Tooltip("Altura do monstro colado na janela")]
        [SerializeField] float _windowMonsterHeight = 1.25f;
        [Tooltip("Quanto o monstro fica para dentro do vidro (evita sumir atras da parede)")]
        [SerializeField] float _windowInside = 0.06f;
        [SerializeField] Color _tint = new Color(0.62f, 0.62f, 0.7f, 1f);
        [SerializeField] float _fadeIn = 0.4f;
        [Tooltip("Esconde o boneco no meio do corredor enquanto ele aparece na porta (visao do quarto)")]
        [SerializeField] bool _hideInCorridor = true;

        [Header("Som (opcional)")]
        [SerializeField] AudioClip _appearSound;
        [SerializeField, Range(0f, 1f)] float _volume = 0.7f;

        GameManager _game;
        DefesaManager _defense;
        CameraManager _cameras;
        Boneco[] _monsters = new Boneco[0];
        Slot _door;
        Slot _window;
        AudioSource _audio;

        class Slot
        {
            public SpriteRenderer renderer;
            public Boneco current;
            public float alpha;
            public float height;
            public Vector3 basePosition;
            public bool feetOnFloor;
        }

        void Start()
        {
            _game = OnafUtil.EncontrarNaCena<GameManager>(gameObject.scene);
            _defense = OnafUtil.EncontrarNaCena<DefesaManager>(gameObject.scene);
            _cameras = OnafUtil.EncontrarNaCena<CameraManager>(gameObject.scene);

            var found = new List<Boneco>();
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                found.AddRange(root.GetComponentsInChildren<Boneco>(true));
            _monsters = found.ToArray();

            if (_readFromDefense && _defense != null)
            {
                _doorPosition = Read(_defense, "portaSpritePosicao", _doorPosition);
                _doorRotation = Read(_defense, "portaSpriteRotacao", _doorRotation);
                _doorHeight = Read(_defense, "portaSpriteAltura", _doorHeight);
                _windowPosition = Read(_defense, "janelaSpritePosicao", _windowPosition);
                _windowRotation = Read(_defense, "janelaSpriteRotacao", _windowRotation);
                _windowHeight = Read(_defense, "janelaSpriteAltura", _windowHeight);
            }

            Room playerRoom = _game != null && _game.GetTree() != null ? _game.GetTree().GetRoom(_playerRoomId) : null;
            Transform parent = playerRoom != null ? playerRoom.transform : transform;
            Vector3 center = playerRoom != null ? playerRoom.FocusPoint : _doorPosition;

            Vector3 doorOut = Flat(_doorPosition - center);
            Vector3 windowOut = Flat(_windowPosition - center);

            Vector3 doorFeet = _doorPosition - Vector3.up * (_doorHeight * 0.5f) + doorOut * _doorOutside;
            _door = MakeSlot("MonsterAtDoor", parent, doorFeet, _doorRotation, _doorMonsterHeight, true);
            _window = MakeSlot("MonsterAtWindow", parent, _windowPosition - windowOut * _windowInside, _windowRotation, _windowMonsterHeight, false);

            if (_appearSound != null)
            {
                _audio = gameObject.AddComponent<AudioSource>();
                _audio.playOnAwake = false;
                _audio.spatialBlend = 0f;
            }
        }

        void LateUpdate()
        {
            bool playerView = _cameras == null || !_cameras.CameraEstaLigada();
            bool doorOpen = _defense == null || !_defense.PortaEstaFechada();
            bool windowOpen = _defense == null || !_defense.JanelaEstaFechada();

            Boneco atDoor = MonsterIn(_doorRoomId);
            Boneco atWindow = MonsterIn(_windowRoomId);

            UpdateSlot(_door, atDoor, doorOpen);
            UpdateSlot(_window, atWindow, windowOpen);

            if (_hideInCorridor && playerView && atDoor != null)
                foreach (Renderer r in atDoor.GetComponentsInChildren<Renderer>(true))
                    if (r != _door.renderer) r.enabled = false;
        }

        Boneco MonsterIn(int roomId)
        {
            foreach (Boneco b in _monsters)
                if (b != null && b.isActiveAndEnabled && b.GetIDRoom() == roomId && b.GetSprite() != null) return b;
            return null;
        }

        void UpdateSlot(Slot slot, Boneco monster, bool open)
        {
            if (slot == null) return;

            if (monster != slot.current)
            {
                slot.current = monster;
                slot.alpha = 0f;
                if (monster != null)
                {
                    SetSprite(slot, monster.GetSprite());
                    if (open && _audio != null) _audio.PlayOneShot(_appearSound, _volume);
                }
            }

            bool show = monster != null && open;
            float target = show ? 1f : 0f;
            slot.alpha = _fadeIn <= 0f ? target : Mathf.MoveTowards(slot.alpha, target, Time.deltaTime / _fadeIn);

            slot.renderer.enabled = slot.alpha > 0.01f;
            Color c = _tint;
            c.a = _tint.a * slot.alpha;
            slot.renderer.color = c;
        }

        Slot MakeSlot(string name, Transform parent, Vector3 position, Vector3 rotation, float height, bool feetOnFloor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, true);
            go.transform.position = position;
            go.transform.rotation = Quaternion.Euler(rotation);
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 4;
            sr.enabled = false;
            return new Slot { renderer = sr, height = height, basePosition = position, feetOnFloor = feetOnFloor };
        }

        static void SetSprite(Slot slot, Sprite sprite)
        {
            slot.renderer.sprite = sprite;
            float size = sprite.bounds.size.y;
            float scale = size > 0f ? slot.height / size : 1f;
            Transform t = slot.renderer.transform;
            t.localScale = new Vector3(scale, scale, 1f);
            Vector3 offset = slot.feetOnFloor ? -sprite.bounds.min.y * scale * Vector3.up : -sprite.bounds.center.y * scale * Vector3.up;
            t.position = slot.basePosition + offset;
        }

        static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.zero;
        }

        static T Read<T>(object owner, string field, T fallback)
        {
            FieldInfo info = owner.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            return info != null && info.GetValue(owner) is T value ? value : fallback;
        }
    }
}
