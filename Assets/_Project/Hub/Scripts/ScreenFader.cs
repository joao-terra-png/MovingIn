using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MovingIn.Hub{
    ///<summary>
    /// tela preta que escurece e clareia (fade)
    /// vai numa Image preta que cobre a tela inteira, junto com um CanvasGroup
    /// o RoomManager usa isso na troca de comodo
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class ScreenFader : MonoBehaviour
    {
        CanvasGroup _group;

        void Awake()
        {
            _group = GetComponent<CanvasGroup>();
            _group.alpha = 0f;              
            _group.blocksRaycasts = false;  
        }

        // escurece a tela ate ficar toda preta
        public IEnumerator FadeOut(float duration) => Fade(1f, duration);

        // clareia a tela ate ficar transparente
        public IEnumerator FadeIn(float duration) => Fade(0f, duration);

        IEnumerator Fade(float targetAlpha, float duration)
        {
            if (duration <= 0f)
            {
                _group.alpha = targetAlpha;
                yield break;
            }

            float startAlpha = _group.alpha;
            float time = 0f;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                _group.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                yield return null;  
            }
            _group.alpha = targetAlpha;
        }
    }
}
