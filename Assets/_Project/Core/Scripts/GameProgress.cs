using System.Collections.Generic;

namespace Mudanca.Core
{
    /// <summary>
    /// Guarda quais minigames (tarefas de arrumação) já foram concluídos.
    ///
    /// OBS. PARA A DISCIPLINA: aqui usamos o HashSet pronto do C# só para a base funcionar.
    /// Este é um ótimo lugar para trocar pela Lista Cadastral (TAD) implementada pelo grupo,
    /// mostrando uma estrutura da disciplina sendo usada no próprio hub.
    /// </summary>
    public static class GameProgress
    {
        static readonly HashSet<string> completed = new HashSet<string>();

        public static int CompletedCount => completed.Count;

        /*
         * Nome: IsCompleted
         * Objetivo: verificar se um minigame já foi concluído.
         * Parâmetros: id (entrada) - identificador único do minigame/interagível.
         * Retorno: true se já foi concluído; false caso contrário.
         */
        public static bool IsCompleted(string id) => completed.Contains(id);

        /*
         * Nome: MarkCompleted
         * Objetivo: registrar que um minigame foi concluído.
         * Parâmetros: id (entrada) - identificador único do minigame/interagível.
         * Retorno: nenhum.
         */
        public static void MarkCompleted(string id) => completed.Add(id);

        /*
         * Nome: ResetAll
         * Objetivo: apagar todo o progresso (ex.: botão "Novo jogo").
         * Parâmetros: nenhum.
         * Retorno: nenhum.
         */
        public static void ResetAll() => completed.Clear();
    }
}
