using System.Collections;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// tela preta que escurece e clareia (fade)
    /// vai numa Image preta que cobre a tela inteira, junto com um CanvasGroup
    /// o RoomManager usa isso na troca de comodo
    /// no editor deve ficar com Alpha 0 no CanvasGroup (invisivel); o Awake garante isso no Play
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        CanvasGroup _group;

        void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            SetAlpha(0f);
        }

        // chamado pela Unity quando o componente e adicionado (ou no "Reset" do menu):
        // ja deixa o CanvasGroup transparente, pra tela nao ficar preta no editor
        void Reset()
        {
            var group = GetComponent<CanvasGroup>();
            group.alpha = 0f;
            group.blocksRaycasts = false;
            group.interactable = false;
        }

        // escurece a tela ate ficar toda preta
        public IEnumerator FadeOut(float duration) => Fade(1f, duration);

        // clareia a tela ate ficar transparente
        public IEnumerator FadeIn(float duration) => Fade(0f, duration);

        IEnumerator Fade(float targetAlpha, float duration)
        {
            // durante a transicao (e com a tela preta) bloqueia cliques no que esta atras
            _group.blocksRaycasts = true;

            if (duration > 0f)
            {
                float startAlpha = _group.alpha;
                float time = 0f;

                while (time < duration)
                {
                    time += Time.unscaledDeltaTime;
                    _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                    yield return null;
                }
            }

            SetAlpha(targetAlpha);
        }

        // transparente = nao bloqueia cliques; preto = bloqueia
        void SetAlpha(float alpha)
        {
            _group.alpha = alpha;
            _group.blocksRaycasts = alpha > 0.01f;
        }

#if UNITY_EDITOR
        // clique nos tres pontinhos do componente no Inspector para ver/esconder o fade enquanto edita
        [ContextMenu("Preview: tela preta")]
        void PreviewPreto() => SetPreview(1f);

        [ContextMenu("Preview: transparente")]
        void PreviewTransparente() => SetPreview(0f);

        void SetPreview(float alpha)
        {
            var group = GetComponent<CanvasGroup>();
            UnityEditor.Undo.RecordObject(group, "Preview do fade");
            group.alpha = alpha;
        }
#endif
    }
}