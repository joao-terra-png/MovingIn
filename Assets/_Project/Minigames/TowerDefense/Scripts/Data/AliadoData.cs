using UnityEngine;

namespace MovingIn.Minigames.TowerDefense
{
    [CreateAssetMenu(fileName = "NovoAliado", menuName = "TowerDefense/Aliado")]
    public class AliadoData : ScriptableObject
    {
        public string nome = "Fusível";
        public Aliado prefab;
        public int custo = 50;

        [Header("Ataque (Fusível)")]
        public float alcance = 3f;
        public int dano = 2;

        [Header("Tempo entre ações (ataque ou geração)")]
        public float intervaloAcao = 1f;

        [Header("Geração (Bateria)")]
        public int energiaGerada = 25;

        [Header("Visual")]
        public Sprite icone;
        public Color cor = Color.white;
        public float escala = 1f;
        public Color corDoRaio = new Color(1f, 0.9f, 0.2f);
    }
}
