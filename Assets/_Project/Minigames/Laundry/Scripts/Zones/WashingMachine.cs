using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    public class WashingMachine : LaundryZone
    {
        [SerializeField] ClothColor color;

        public ClothColor MachineColor => color;
        public override LaundryPhase ActivePhase => LaundryPhase.Wash;
        public override bool Accepts(ClothPiece piece) => piece.ClothColor == color;
    }
}
