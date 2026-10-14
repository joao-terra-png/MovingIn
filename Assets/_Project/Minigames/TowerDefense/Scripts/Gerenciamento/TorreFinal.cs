using System;
using UnityEngine;

// A torre que os inimigos tentam alcançar.
public class TorreFinal : MonoBehaviour
{
    [SerializeField] private int vidaMaxima = 10;

    public int VidaAtual { get; private set; }

    public event Action<int, int> VidaMudou;
    public event Action Destruida;

    private void Awake()
    {
        VidaAtual = vidaMaxima;
    }

    private void Start()
    {
        VidaMudou?.Invoke(VidaAtual, vidaMaxima);
    }

    public void ReceberDano(int dano)
    {
        if (VidaAtual <= 0) return;

        VidaAtual = Mathf.Max(0, VidaAtual - dano);
        VidaMudou?.Invoke(VidaAtual, vidaMaxima);

        if (VidaAtual == 0) Destruida?.Invoke();
    }
}
