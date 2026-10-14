using System;
using UnityEngine;

[Serializable]
public class GrupoInimigos
{
    public InimigoData tipo;
    public int quantidade = 5;
    public float intervalo = 1f;
}

[CreateAssetMenu(fileName = "NovaOnda", menuName = "TowerDefense/Onda")]
public class OndaData : ScriptableObject
{
    public float pausaAntesDaOnda = 5f;
    public GrupoInimigos[] grupos;
}
