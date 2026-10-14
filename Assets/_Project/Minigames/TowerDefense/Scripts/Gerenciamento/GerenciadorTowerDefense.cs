using System;
using UnityEngine;

public enum EstadoPartida { Aguardando, EmAndamento, Vitoria, Derrota }

// Coordena o minigame: liga os sistemas entre si e decide vitória e derrota.
public class GerenciadorTowerDefense : MonoBehaviour
{
    [SerializeField] private GerenciadorOndas ondas;
    [SerializeField] private TorreFinal torre;
    [SerializeField] private GerenciadorEnergia energia;
    [SerializeField] private GerenciadorPosicionamento posicionamento;

    [Header("Início")]
    [Tooltip("Desmarque se o jogo principal for chamar Iniciar() (ex.: ao religar a energia).")]
    [SerializeField] private bool iniciarAutomaticamente = true;

    [Header("UI (opcional)")]
    [SerializeField] private GameObject painelVitoria;
    [SerializeField] private GameObject painelDerrota;

    public EstadoPartida Estado { get; private set; } = EstadoPartida.Aguardando;

    public event Action<bool> Finalizado;

    private void OnEnable()
    {
        ondas.InimigoDerrotado += AoInimigoDerrotado;
        ondas.InimigoChegouAoFim += AoInimigoChegarAoFim;
        ondas.TodasOndasConcluidas += AoConcluirOndas;
        torre.Destruida += AoTorreDestruida;
    }

    private void OnDisable()
    {
        ondas.InimigoDerrotado -= AoInimigoDerrotado;
        ondas.InimigoChegouAoFim -= AoInimigoChegarAoFim;
        ondas.TodasOndasConcluidas -= AoConcluirOndas;
        torre.Destruida -= AoTorreDestruida;
    }

    private void Start()
    {
        if (painelVitoria != null) painelVitoria.SetActive(false);
        if (painelDerrota != null) painelDerrota.SetActive(false);

        if (iniciarAutomaticamente) Iniciar();
    }

    public void Iniciar()
    {
        if (Estado != EstadoPartida.Aguardando) return;

        Estado = EstadoPartida.EmAndamento;
        ondas.Iniciar();
    }

    private void AoInimigoDerrotado(Inimigo inimigo)
    {
        if (Estado != EstadoPartida.EmAndamento) return;
        energia.Adicionar(inimigo.Dados.recompensa);
    }

    private void AoInimigoChegarAoFim(Inimigo inimigo)
    {
        if (Estado != EstadoPartida.EmAndamento) return;
        torre.ReceberDano(inimigo.Dados.danoNaTorre);
    }

    private void AoTorreDestruida()
    {
        Encerrar(false);
    }

    private void AoConcluirOndas()
    {
        Encerrar(true);
    }

    private void Encerrar(bool venceu)
    {
        if (Estado != EstadoPartida.EmAndamento) return;

        Estado = venceu ? EstadoPartida.Vitoria : EstadoPartida.Derrota;

        ondas.Parar();
        posicionamento.Encerrar();

        if (venceu && painelVitoria != null) painelVitoria.SetActive(true);
        if (!venceu && painelDerrota != null) painelDerrota.SetActive(true);

        Finalizado?.Invoke(venceu);
    }
}
