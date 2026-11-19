using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    public abstract class LaundryZone : MonoBehaviour
    {
        public abstract LaundryPhase ActivePhase { get; }

        public abstract bool Accepts(ClothPiece piece);
    }
}
