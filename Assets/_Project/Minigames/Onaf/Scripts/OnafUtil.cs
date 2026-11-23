using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MovingIn.Minigames.Onaf
{
    public static class OnafUtil
    {
        private static readonly Dictionary<int, string> nomes = new Dictionary<int, string>
        {
            { 10, "Sótão" },
            { 20, "Corredor" },
            { 30, "Porão" },
            { 40, "Escada" },
            { 50, "Lavanderia" },
            { 60, "Seu quarto" },
            { 70, "Sala" },
            { 80, "Frente" },
            { 90, "Cozinha" },
            { 100, "Garagem" }
        };

        private static readonly Dictionary<string, int> idsDosBotoes = new Dictionary<string, int>
        {
            { "quarto", 60 },
            { "corredor", 20 },
            { "frente", 80 },
            { "sala", 70 },
            { "escada", 40 },
            { "garagem", 100 },
            { "sotao", 10 },
            { "cozinha", 90 },
            { "lavanderia", 50 },
            { "porao", 30 }
        };

        public static string NomeDoComodo(int id)
        {
            return nomes.TryGetValue(id, out string nome) ? nome : "Cômodo " + id;
        }

        public static int IdDoBotao(string nomeDoObjeto)
        {
            string chave = nomeDoObjeto.Replace("Button", "").Trim().ToLowerInvariant();
            return idsDosBotoes.TryGetValue(chave, out int id) ? id : -1;
        }

        public static T EncontrarNaCena<T>(Scene cena) where T : Component
        {
            foreach (GameObject raiz in cena.GetRootGameObjects())
            {
                T achado = raiz.GetComponentInChildren<T>(true);
                if (achado != null) return achado;
            }

            return null;
        }
    }
}
