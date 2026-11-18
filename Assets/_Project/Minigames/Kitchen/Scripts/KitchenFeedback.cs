using System.Collections;
using UnityEngine;

namespace MovingIn.Minigames.Kitchen{
    public class KitchenFeedback : MonoBehaviour
    {
        [SerializeField] KitchenManager manager;

        [Header("Erro: tremer e piscar")]
        [SerializeField] float _shakeDuration = 0.3f;
        [SerializeField] float _shakeAngle = 15f;
        [SerializeField] float _blinkDuration = 0.15f;
        [SerializeField] Color _errorColor = new Color(1f, 0.3f, 0.3f);

        [Header("Acerto: pulinho")]
        [SerializeField] float _popDuration = 0.2f;
        [SerializeField] float _popAmount = 0.2f;

        [Header("Som (opcional)")]
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip hitClip;
        [SerializeField] AudioClip missClip;

        void OnEnable()
        {
            manager.Hit += OnHit;
            manager.Miss += OnMiss;
        }

        void OnDisable()
        {
            manager.Hit -= OnHit;
            manager.Miss -= OnMiss;
        }

        void OnHit(ItemVisual item)
        {
            PlaySound(hitClip);
            StartCoroutine(Pop(item));
        }

        void OnMiss(ItemVisual item)
        {
            PlaySound(missClip);
            StartCoroutine(Shake(item));
            StartCoroutine(Blink(item));
        }

        void PlaySound(AudioClip clip)
        {
            if (audioSource != null && clip != null) audioSource.PlayOneShot(clip);
        }

        IEnumerator Shake(ItemVisual item)
        {
            float t = 0f;
            while (t < _shakeDuration)
            {
                if (item == null) yield break;  
                float fade = 1f - t / _shakeDuration;              
                float angle = Mathf.Sin(t * 60f) * _shakeAngle * fade;
                item.transform.rotation = Quaternion.Euler(0f, 0f, angle);
                t += Time.deltaTime;
                yield return null;
            }
            if (item != null) item.transform.rotation = Quaternion.identity;
        }

        IEnumerator Blink(ItemVisual item)
        {
            SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
            sr.color = _errorColor;
            yield return new WaitForSeconds(_blinkDuration);
            if (item != null) sr.color = item.BaseColor;  
        }

        IEnumerator Pop(ItemVisual item)
        {
            if (item == null) yield break;
            Vector3 baseScale = item.transform.localScale;
            float t = 0f;
            while (t < _popDuration)
            {
                if (item == null) yield break;
                float k = Mathf.Sin(t / _popDuration * Mathf.PI);  
                item.transform.localScale = baseScale * (1f + _popAmount * k);
                t += Time.deltaTime;
                yield return null;
            }
            if (item != null) item.transform.localScale = baseScale;
        }
    }
}
