using System.Collections.Generic;

namespace MovingIn.Core{
    ///<summary>
    /// guarda quais minigames (tarefas de arrumacao) ja foram concluidos
    ///
    /// aqui eh uma estrutura de dados basica da unity, da pra fazer um TAD disso dps
    /// </summary>
    public static class GameProgress
    {
        static readonly HashSet<string> _completed = new HashSet<string>();

        public static int CompletedCount => _completed.Count;

        // retorna true se o minigame com esse id ja foi concluido
        public static bool IsCompleted(string id) => _completed.Contains(id);

        // registra que o minigame com esse id foi concluido
        public static void MarkCompleted(string id) => _completed.Add(id);

        // apaga todo o progresso 
        public static void ResetAll() => _completed.Clear();
    }
}
