using System.Collections.Generic;
using MovingIn.Hub;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace MovingIn.Minigames.Onaf
{
    public class OnafNightLook : MonoBehaviour
    {
        [Header("Ceu e ambiente")]
        [SerializeField] Color _ambient = new Color(0.11f, 0.12f, 0.2f);
        [SerializeField] Color _background = new Color(0.02f, 0.025f, 0.06f);

        [Header("Lua (luz direcional)")]
        [SerializeField] Color _moonColor = new Color(0.55f, 0.65f, 1f);
        [SerializeField] float _moonIntensity = 0.18f;

        [Header("Luzes da casa (visao do quarto)")]
        [Tooltip("Quanto as luzes de teto continuam acesas nos outros comodos (0 = apagadas)")]
        [SerializeField, Range(0f, 1f)] float _roomLights = 0.3f;
        [SerializeField] int _playerRoomId = 60;
        [Tooltip("Luz do quarto do jogador (abajur)")]
        [SerializeField, Range(0f, 1f)] float _playerRoomLight = 0.35f;
        [SerializeField] Color _playerRoomColor = new Color(1f, 0.72f, 0.45f);
        [Tooltip("Mantem a luz da varanda acesa (ilumina quem chega pela janela)")]
        [SerializeField] bool _keepPorchLight = true;
        [Tooltip("Desliga as luzes vermelhas de emergencia copiadas do apagao")]
        [SerializeField] bool _hideEmergencyLights = true;

        [Header("Cameras de seguranca (mais claras para enxergar os bonecos)")]
        [SerializeField] Color _cameraAmbient = new Color(0.34f, 0.37f, 0.44f);
        [SerializeField, Range(0f, 1.5f)] float _cameraRoomLights = 0.9f;
        [SerializeField] float _cameraMoonIntensity = 0.45f;

        class Lamp
        {
            public Light light;
            public float baseIntensity;
            public bool playerRoom;
            public bool animated;
        }

        readonly List<Lamp> _lamps = new List<Lamp>();
        readonly List<Light> _moons = new List<Light>();
        CameraManager _cameras;
        Camera _playerCamera;

        void Start()
        {
            _cameras = OnafUtil.EncontrarNaCena<CameraManager>(gameObject.scene);
            _playerCamera = _cameras != null ? _cameras.GetCameraJogador() : null;
            CollectLights();
            ApplyCameras();
        }

        void LateUpdate()
        {
            bool security = _playerCamera != null && !_playerCamera.enabled;

            if (SceneManager.GetActiveScene() == gameObject.scene)
            {
                RenderSettings.ambientMode = AmbientMode.Flat;
                RenderSettings.ambientLight = security ? _cameraAmbient : _ambient;
                RenderSettings.fog = false;
            }

            foreach (Light moon in _moons)
                if (moon != null) moon.intensity = security ? _cameraMoonIntensity : _moonIntensity;

            float others = security ? _cameraRoomLights : _roomLights;
            foreach (Lamp lamp in _lamps)
            {
                if (lamp.light == null) continue;
                float factor = lamp.playerRoom ? _playerRoomLight : others;
                if (lamp.animated)
                {
                    lamp.light.intensity *= factor;
                }
                else
                {
                    lamp.light.intensity = lamp.baseIntensity * factor;
                    lamp.light.enabled = factor > 0f;
                }
            }
        }

        void CollectLights()
        {
            foreach (Light light in FindInScene<Light>())
            {
                if (light.type == LightType.Directional)
                {
                    light.color = _moonColor;
                    _moons.Add(light);
                    continue;
                }

                if (_hideEmergencyLights && light.GetComponentInParent<EmergencyLight>(true) != null)
                {
                    light.enabled = false;
                    continue;
                }
                if (light.name.Contains("Porch"))
                {
                    if (!_keepPorchLight) light.enabled = false;
                    continue;
                }
                if (!light.enabled && light.GetComponentInParent<LampFx>(true) == null) continue;

                Room room = light.GetComponentInParent<Room>(true);
                bool playerRoom = room != null && room.GetRoomId() == _playerRoomId;
                if (playerRoom) light.color = _playerRoomColor;

                _lamps.Add(new Lamp
                {
                    light = light,
                    baseIntensity = light.intensity,
                    playerRoom = playerRoom,
                    animated = light.GetComponentInParent<LampFx>(true) != null
                });
            }

            if (_hideEmergencyLights)
                foreach (EmergencyLight e in FindInScene<EmergencyLight>())
                    e.enabled = false;
        }

        void ApplyCameras()
        {
            foreach (Camera cam in FindInScene<Camera>())
            {
                if (cam.targetTexture != null) continue;
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = _background;
            }
        }

        T[] FindInScene<T>() where T : Component
        {
            var list = new List<T>();
            foreach (GameObject root in gameObject.scene.GetRootGameObjects())
                list.AddRange(root.GetComponentsInChildren<T>(true));
            return list.ToArray();
        }
    }
}
