using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MovingIn.Core;

namespace MovingIn.Minigames.Exemplo{
    ///<summary>
    /// minigame "de mentira" pra testar o hub antes dos minigames reais
    /// serve de modelo: todo minigame chama MinigameBridge.Finish(true) ao vencer
    /// e Finish(false) ao sair
    /// </summary>
    public class PlaceholderMinigame : MonoBehaviour
    {
        void Update()
        {
            // esc para sair
            if (Input.GetKeyDown(KeyCode.Escape))
                MinigameBridge.Finish(false);
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(20f, 20f, 360f, 220f));
            GUILayout.Label("MINIGAME DE EXEMPLO");
            GUILayout.Label(MinigameBridge.IsInsideHub ? "(aberto pelo hub)" : "(rodando sozinho, modo teste)");

            if (GUILayout.Button("Vencer (comodo fica arrumado)", GUILayout.Height(40f)))
                MinigameBridge.Finish(true);

            if (GUILayout.Button("Sair sem terminar (ESC)", GUILayout.Height(40f)))
                MinigameBridge.Finish(false);

            GUILayout.EndArea();
        }
    }
}
