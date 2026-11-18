using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    [CreateAssetMenu(fileName = "NovoInimigo", menuName = "TowerDefense/Inimigo")]
    public class InimigoData : ScriptableObject
    {
        public string nome = "Carga";
        public Inimigo prefab;
        public int vidaMaxima = 10;
        public int danoNaTorre = 1;
        public int recompensa = 5;

        [Header("Visual")]
        public Sprite sprite;
        public Color cor = Color.white;
        public float escala = 1f;
    }
}
