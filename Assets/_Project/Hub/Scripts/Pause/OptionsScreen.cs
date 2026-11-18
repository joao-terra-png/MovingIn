using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MovingIn.Hub{
    public class OptionsScreen : MonoBehaviour
    {
        [SerializeField] PauseMenu menu;
        [SerializeField] Button volumeRow;
        [SerializeField] Button textRow;
        [SerializeField] Button fullscreenRow;
        [SerializeField] Button pixelRow;
        [SerializeField] Button backButton;
        [SerializeField] TextMeshProUGUI volumeValue;
        [SerializeField] TextMeshProUGUI textValue;
        [SerializeField] TextMeshProUGUI fullscreenValue;
        [SerializeField] TextMeshProUGUI pixelValue;

        void Awake()
        {
            volumeRow.onClick.AddListener(() => Change(volumeRow, 1, true));
            textRow.onClick.AddListener(() => Change(textRow, 1, true));
            fullscreenRow.onClick.AddListener(() => Change(fullscreenRow, 1, true));
            pixelRow.onClick.AddListener(() => Change(pixelRow, 1, true));
            backButton.onClick.AddListener(menu.Back);
        }

        void OnEnable()
        {
            Refresh();
        }

        void Update()
        {
            int dir = 0;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) dir = -1;
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) dir = 1;
            if (dir == 0 || EventSystem.current == null) return;

            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected == volumeRow.gameObject) Change(volumeRow, dir, false);
            else if (selected == textRow.gameObject) Change(textRow, dir, false);
            else if (selected == fullscreenRow.gameObject) Change(fullscreenRow, dir, false);
            else if (selected == pixelRow.gameObject) Change(pixelRow, dir, false);
        }

        void Change(Button row, int dir, bool wrap)
        {
            if (row == volumeRow)
            {
                int v = GameSettings.Volume + 10 * dir;
                if (wrap && v > 100) v = 0;
                GameSettings.Volume = Mathf.Clamp(v, 0, 100);
            }
            else if (row == textRow) GameSettings.TextSpeed += dir;
            else if (row == fullscreenRow) GameSettings.Fullscreen = !GameSettings.Fullscreen;
            else if (row == pixelRow) GameSettings.Pixel = !GameSettings.Pixel;
            Refresh();
        }

        void Refresh()
        {
            volumeValue.text = $"<  {GameSettings.Volume}%  >";
            textValue.text = $"<  {GameSettings.TextSpeedNames[GameSettings.TextSpeed]}  >";
            fullscreenValue.text = $"<  {(GameSettings.Fullscreen ? "Sim" : "Não")}  >";
            pixelValue.text = $"<  {(GameSettings.Pixel ? "Sim" : "Não")}  >";
        }
    }
}
