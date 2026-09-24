using UnityEngine;
using Mudanca.Core;

namespace Mudanca.Minigames.Exemplo
{
    /// <summary>
    /// Minigame "de mentira" para testar o hub antes dos minigames reais ficarem prontos.
    /// Também serve de modelo: todo minigame real faz exatamente isto —
    /// chama MinigameBridge.Finish(true) ao vencer e Finish(false) ao sair.
    /// </summary>
    public class PlaceholderMinigame : MonoBehaviour
    {
        void Update()
        {
            // Convenção do grupo: ESC sempre sai do minigame sem concluir
            if (Input.GetKeyDown(KeyCode.Escape))
                MinigameBridge.Finish(false);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20f, 20f, 360f, 220f));
            GUILayout.Label("MINIGAME DE EXEMPLO");
            GUILayout.Label(MinigameBridge.IsInsideHub ? "(aberto pelo hub)" : "(rodando sozinho, modo teste)");

            if (GUILayout.Button("Vencer (cômodo fica arrumado)", GUILayout.Height(40f)))
                MinigameBridge.Finish(true);

            if (GUILayout.Button("Sair sem terminar (ESC)", GUILayout.Height(40f)))
                MinigameBridge.Finish(false);

            GUILayout.EndArea();
        }
    }
}
