using UnityEngine;

namespace MovingIn.Minigames.Laundry{
    public class Drawer : LaundryZone
    {
        [SerializeField] ClothType type;

        public ClothType DrawerType => type;
        public override LaundryPhase ActivePhase => LaundryPhase.Store;
        public override bool Accepts(ClothPiece piece) => piece.ClothType == type;
    }
}
