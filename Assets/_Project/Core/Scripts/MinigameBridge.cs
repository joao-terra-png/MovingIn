using System;
using UnityEngine;

namespace MovingIn.Core{
    ///<summary>
    /// ponte entre o minigame e o hub
    ///
    /// O minigame NAO conhece o hub, ele so chama:
    ///   MinigameBridge.Finish(true)  -> jogador venceu (comodo fica arrumado)
    ///   MinigameBridge.Finish(false) -> jogador saiu (comodo continua baguncado)
    /// Assim da pra testar cada minigame sozinho
    /// </summary>
    public static class MinigameBridge
    {
        // disparado quando o minigame acaba, o MinigameManager escuta isso
        public static event Action<bool> Finished;

        // true se tem um hub esperando o resultado
        // false quando a cena do minigame foi aberta sozinha (modo teste)
        public static bool IsInsideHub => Finished != null;

        public static void Finish(bool success)
        {
            if (Finished == null)
            {
                Debug.Log($"[MinigameBridge] Minigame terminou (vitoria = {success}). " +
                          "Nenhum hub carregado, entao nada acontece (normal em teste).");
                return;
            }
            Finished.Invoke(success);
        }
    }
}
