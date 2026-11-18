using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    [RequireComponent(typeof(Collider2D))]
    public class SlotAliado : MonoBehaviour
    {
        public bool Ocupado { get; private set; }

        public void Ocupar() { Ocupado = true; }
        public void Liberar() { Ocupado = false; }
    }
}
