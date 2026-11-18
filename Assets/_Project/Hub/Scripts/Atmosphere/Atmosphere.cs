using UnityEngine;

namespace MovingIn.Hub{
    public enum AtmosphereMode { Always, Sunlit, Dark, RoomLamps, Blackout, Powered }

    public static class Atmosphere
    {
        public static float Weight(AtmosphereMode mode)
        {
            TimeOfDay t = TimeOfDay.Instance;
            if (t == null || !Application.isPlaying)
                return mode == AtmosphereMode.Always || mode == AtmosphereMode.Sunlit || mode == AtmosphereMode.Powered ? 1f : 0f;
            switch (mode)
            {
                case AtmosphereMode.Sunlit: return Mathf.InverseLerp(0.3f, 0.9f, t.SunIntensity);
                case AtmosphereMode.Dark: return 1f - Mathf.InverseLerp(0.15f, 0.5f, t.SunIntensity);
                case AtmosphereMode.RoomLamps: return Mathf.Clamp01(t.RoomFactor);
                case AtmosphereMode.Blackout: return t.Current == "Blackout" ? 1f : 0f;
                case AtmosphereMode.Powered: return t.Current == "Blackout" ? 0f : 1f;
                default: return 1f;
            }
        }
    }
}
