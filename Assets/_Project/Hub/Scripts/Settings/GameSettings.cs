using UnityEngine;

namespace MovingIn.Hub{
    public static class GameSettings
    {
        const string VolumeKey = "movingin_volume";
        const string TextKey = "movingin_text_speed";
        const string FullscreenKey = "movingin_fullscreen";
        const string PixelKey = "movingin_pixel";

        public static readonly string[] TextSpeedNames = { "Lenta", "Normal", "Rápida", "Instantânea" };
        static readonly float[] TextSpeeds = { 22f, 40f, 75f, 2000f };

        public static int Volume
        {
            get => PlayerPrefs.GetInt(VolumeKey, 80);
            set { PlayerPrefs.SetInt(VolumeKey, Mathf.Clamp(value, 0, 100)); Apply(); }
        }

        public static int TextSpeed
        {
            get => Mathf.Clamp(PlayerPrefs.GetInt(TextKey, 1), 0, TextSpeeds.Length - 1);
            set { PlayerPrefs.SetInt(TextKey, (value % TextSpeeds.Length + TextSpeeds.Length) % TextSpeeds.Length); Apply(); }
        }

        public static bool Fullscreen
        {
            get => PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) == 1;
            set { PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0); Apply(); }
        }

        public static bool Pixel
        {
            get => PlayerPrefs.GetInt(PixelKey, 1) == 1;
            set { PlayerPrefs.SetInt(PixelKey, value ? 1 : 0); Apply(); }
        }

        public static void Apply()
        {
            AudioListener.volume = Volume / 100f;
            if (DialogueManager.Instance != null) DialogueManager.Instance.CharsPerSecond = TextSpeeds[TextSpeed];
            if (!Application.isEditor && Screen.fullScreen != Fullscreen) Screen.fullScreen = Fullscreen;
            foreach (PixelCamera pc in Object.FindObjectsOfType<PixelCamera>(true))
                pc.Pixelate = Pixel;
            PlayerPrefs.Save();
        }
    }
}
