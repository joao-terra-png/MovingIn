using TMPro;
using UnityEngine;

// Mostra saldo de energia, vida da torre, onda atual e mensagens.
public class HudTowerDefense : MonoBehaviour
{
    [Header("Sistemas")]
    [SerializeField] private GerenciadorEnergia energia;
    [SerializeField] private TorreFinal torre;
    [SerializeField] private GerenciadorOndas ondas;
    [SerializeField] private GerenciadorTowerDefense partida;
    [SerializeField] private LojaAliados loja;

    [Header("Textos (TextMeshPro)")]
    [SerializeField] private TMP_Text textoSaldo;
    [SerializeField] private TMP_Text textoVida;
    [SerializeField] private TMP_Text textoOnda;
    [SerializeField] private TMP_Text textoMensagem;

    private bool partidaFinalizada;

    private void OnEnable()
    {
        energia.SaldoMudou += AoSaldoMudar;
        torre.VidaMudou += AoVidaMudar;
        ondas.OndaIniciada += AoOndaIniciar;
        partida.Finalizado += AoFinalizar;
        loja.SelecaoMudou += AoSelecaoMudar;
        loja.EnergiaInsuficiente += AoEnergiaInsuficiente;
    }

    private void OnDisable()
    {
        energia.SaldoMudou -= AoSaldoMudar;
        torre.VidaMudou -= AoVidaMudar;
        ondas.OndaIniciada -= AoOndaIniciar;
        partida.Finalizado -= AoFinalizar;
        loja.SelecaoMudou -= AoSelecaoMudar;
        loja.EnergiaInsuficiente -= AoEnergiaInsuficiente;
    }

    private void Start()
    {
        AoOndaIniciar(0, ondas.TotalOndas);
        Definir(textoMensagem, "");
    }

    private void AoSaldoMudar(int saldo)
    {
        Definir(textoSaldo, "Energia: " + saldo);
    }

    private void AoVidaMudar(int atual, int maxima)
    {
        Definir(textoVida, "Torre: " + atual + "/" + maxima);
    }

    private void AoOndaIniciar(int atual, int total)
    {
        Definir(textoOnda, "Onda " + atual + "/" + total);
    }

    private void AoSelecaoMudar(AliadoData dados)
    {
        if (partidaFinalizada) return;

        CancelInvoke(nameof(LimparMensagem));
        if (dados == null)
            Definir(textoMensagem, "");
        else
            Definir(textoMensagem, "Colocando " + dados.nome + ": clique em um slot (botão direito cancela)");
    }

    private void AoEnergiaInsuficiente()
    {
        if (partidaFinalizada) return;

        Definir(textoMensagem, "Energia insuficiente!");
        CancelInvoke(nameof(LimparMensagem));
        Invoke(nameof(LimparMensagem), 2f);
    }

    private void LimparMensagem()
    {
        if (partidaFinalizada) return;
        Definir(textoMensagem, "");
    }

    private void AoFinalizar(bool venceu)
    {
        partidaFinalizada = true;
        CancelInvoke(nameof(LimparMensagem));
        Definir(textoMensagem, venceu ? "VITÓRIA! A energia foi estabilizada." : "DERROTA! O circuito falhou.");
    }

    private void Definir(TMP_Text campo, string texto)
    {
        if (campo != null) campo.text = texto;
    }
}
