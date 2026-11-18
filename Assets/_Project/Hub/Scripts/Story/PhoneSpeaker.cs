using UnityEngine;

namespace MovingIn.Hub{
    public class PhoneSpeaker : Speaker
    {
        [SerializeField] RectTransform anchor;
        [SerializeField] Camera cam;

        readonly Vector3[] _corners = new Vector3[4];

        public override Vector3 HeadPosition
        {
            get
            {
                if (anchor == null) return base.HeadPosition;
                if (cam == null) cam = Camera.main;
                if (cam == null) return base.HeadPosition;
                anchor.GetWorldCorners(_corners);
                Vector3 top = (_corners[1] + _corners[2]) / 2f;
                return cam.ScreenToWorldPoint(new Vector3(top.x, top.y + 6f, 10f));
            }
        }
    }
}
