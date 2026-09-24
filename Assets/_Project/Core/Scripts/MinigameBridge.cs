using System;
using UnityEngine;

namespace Mudanca.Core
{
    /// <summary>
    /// Ponte entre um minigame e o hub 3D.
    /// O minigame NÃO conhece o hub: a única coisa que ele precisa fazer
    /// é chamar MinigameBridge.Finish(true) quando o jogador vencer
    /// (ou Finish(false) quando o jogador desistir / sair).
    /// Isso deixa cada minigame independente: dá para testar a cena dele sozinha.
    /// </summary>
    public static class MinigameBridge
    {
        /// Disparado quando o minigame termina. O MinigameManager (hub) escuta isso.
        public static event Action<bool> Finished;

        /// true quando existe um hub esperando o resultado;
        /// false quando você deu Play direto na cena do minigame (modo teste).
        public static bool IsInsideHub => Finished != null;

        /*
         * Nome: Finish
         * Objetivo: avisar ao hub que o minigame terminou.
         * Parâmetros:
         *   success (entrada) - true se o jogador venceu (cômodo fica arrumado),
         *                       false se saiu sem terminar (cômodo continua bagunçado).
         * Retorno: nenhum.
         */
        public static void Finish(bool success)
        {
            if (Finished == null)
            {
                Debug.Log($"[MinigameBridge] Minigame terminou (vitória = {success}). " +
                          "Nenhum hub carregado, então nada acontece — normal quando você testa a cena sozinha.");
                return;
            }
            Finished.Invoke(success);
        }
    }
}
