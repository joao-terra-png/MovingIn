using System;
using System.Collections;
using UnityEngine;

// Cria os inimigos e os remove
public class GerenciadorOndas : MonoBehaviour
{
    [Header("Ondas")]
    [SerializeField] private OndaData[] ondas;

    [Header("Caminho (waypoints na ordem; o primeiro é o ponto de spawn)")]
    [SerializeField] private Transform[] caminho;

    [Header("Configuração")]
    [Tooltip("Igual para todos os inimigos, para a ordem da fila continuar válida.")]
    [SerializeField] private float velocidadeInimigos = 2f;
    [SerializeField] private Transform paiDosInimigos;

    private FilaEncadeada fila;
    private Coroutine rotina;
    private bool geracaoConcluida;
    private bool concluidaNotificada;
    private bool encerrado;

    public int OndaAtual { get; private set; }
    public int TotalOndas => ondas != null ? ondas.Length : 0;

    // Sempre o primeiro da fila.
    public Inimigo AlvoAtual
    {
        get
        {
            Node no = fila.Primeiro();
            return no == null ? null : (Inimigo)no.GetInfo();
        }
    }

    public event Action<int, int> OndaIniciada;
    public event Action<Inimigo> InimigoDerrotado;
    public event Action<Inimigo> InimigoChegouAoFim;
    public event Action TodasOndasConcluidas;

    private void Awake()
    {
        fila = new FilaEncadeada();
    }

    public void Iniciar()
    {
        if (rotina != null || encerrado) return;
        rotina = StartCoroutine(RotinaOndas());
    }

    public void Parar()
    {
        encerrado = true;
        if (rotina != null) StopCoroutine(rotina);

        while (!fila.Vazia())
        {
            Inimigo restante = AlvoAtual;
            fila.Desenf(null);
            if (restante != null)
            {
                Desligar(restante);
                Destroy(restante.gameObject);
            }
        }
    }

    private IEnumerator RotinaOndas()
    {
        for (int i = 0; i < ondas.Length; i++)
        {
            OndaData onda = ondas[i];
            OndaAtual = i + 1;

            yield return new WaitForSeconds(onda.pausaAntesDaOnda);
            OndaIniciada?.Invoke(OndaAtual, ondas.Length);

            foreach (GrupoInimigos grupo in onda.grupos)
            {
                for (int q = 0; q < grupo.quantidade; q++)
                {
                    Spawnar(grupo.tipo);
                    yield return new WaitForSeconds(grupo.intervalo);
                }
            }
        }

        geracaoConcluida = true;
        VerificarFim();
    }

    private void Spawnar(InimigoData dados)
    {
        if (dados == null || dados.prefab == null)
        {
            Debug.LogError("InimigoData sem prefab: " + (dados != null ? dados.name : "(grupo sem Tipo)"), dados);
            return;
        }

        Inimigo inimigo = Instantiate(dados.prefab, caminho[0].position, Quaternion.identity, paiDosInimigos);

        fila.Enf(inimigo);

        inimigo.Morreu += AoInimigoMorrer;
        inimigo.ChegouAoFim += AoInimigoChegar;
        inimigo.Inicializar(dados, caminho, velocidadeInimigos);
    }

    private void AoInimigoMorrer(Inimigo inimigo)
    {
        if (encerrado) return;

        RetirarDaFila(inimigo);
        InimigoDerrotado?.Invoke(inimigo);
        Destroy(inimigo.gameObject);
        VerificarFim();
    }

    private void AoInimigoChegar(Inimigo inimigo)
    {
        if (encerrado) return;

        RetirarDaFila(inimigo);
        InimigoChegouAoFim?.Invoke(inimigo);
        Destroy(inimigo.gameObject);
        VerificarFim();
    }

    private void RetirarDaFila(Inimigo esperado)
    {
        Inimigo daFrente = AlvoAtual;
        bool deuCerto = fila.Desenf(null);
        Desligar(esperado);

        if (deuCerto && daFrente != esperado)
            Debug.LogWarning("A ordem da fila foi quebrada: saiu um inimigo diferente do esperado.");
    }

    private void Desligar(Inimigo inimigo)
    {
        inimigo.Morreu -= AoInimigoMorrer;
        inimigo.ChegouAoFim -= AoInimigoChegar;
    }

    private void VerificarFim()
    {
        if (encerrado || concluidaNotificada) return;
        if (geracaoConcluida && fila.Vazia())
        {
            concluidaNotificada = true;
            TodasOndasConcluidas?.Invoke();
        }
    }
}
