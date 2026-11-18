using TMPro;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject pausePanel;
        [SerializeField] TMP_Text pauseText;
        [SerializeField] float _pulseSpeed = 4f;
        [SerializeField] float _pulseAmount = 0.1f;

        public bool IsPaused { get; private set; }

        void Start()
        {
            SetPaused(false);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.P)) SetPaused(!IsPaused);

            if (IsPaused && pauseText != null)
            {
                float s = 1f + _pulseAmount * Mathf.Sin(Time.unscaledTime * _pulseSpeed);
                pauseText.transform.localScale = Vector3.one * s;
            }
        }

        public void SetPaused(bool value)
        {
            IsPaused = value;
            Time.timeScale = value ? 0f : 1f;
            if (pausePanel != null) pausePanel.SetActive(value);
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }
    }
}
