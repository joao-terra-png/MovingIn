using UnityEngine;

[CreateAssetMenu(fileName = "NovoAliado", menuName = "TowerDefense/Aliado")]
public class AliadoData : ScriptableObject
{
    public string nome = "Fusível";
    public Sprite icone;
    public Aliado prefab;
    public int custo = 50;

    [Header("Ataque (Fusível)")]
    public float alcance = 3f;
    public int dano = 2;

    [Header("Tempo entre ações (ataque ou geração)")]
    public float intervaloAcao = 1f;

    [Header("Geração (Bateria)")]
    public int energiaGerada = 25;
}
