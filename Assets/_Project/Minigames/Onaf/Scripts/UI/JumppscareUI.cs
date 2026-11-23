using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace MovingIn.Minigames.Onaf
{
    // Jumpscare da derrota: tela preta, o sprite do boneco vindo na direção do jogador (rápido, com tremor)
    // e um grito de fundo.
    //   - O GameManager cria este componente sozinho se não houver um na cena.
    //   - Para usar o seu próprio grito, adicione o componente em qualquer objeto da cena e
    //     arraste o áudio em "Som Do Grito". Vazio = usa um grito gerado por código.
    public class JumpscareUI : MonoBehaviour
    {
        [Header("Áudio")]
        [Tooltip("Grito tocado durante o jumpscare. Vazio = usa um grito gerado por código.")]
        [SerializeField] private AudioClip somDoGrito;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;

        [Header("Animação")]
        [Tooltip("Duração total do jumpscare, em segundos.")]
        [SerializeField] private float duracaoTotal = 1.6f;
        [Tooltip("Tempo que o boneco leva para chegar até a tela (quanto menor, mais rápido).")]
        [SerializeField] private float tempoDeAproximacao = 0.35f;
        [Tooltip("Tamanho do boneco no começo (1 = altura da tela).")]
        [SerializeField] private float escalaInicial = 0.12f;
        [Tooltip("Tamanho do boneco no fim (1 = altura da tela).")]
        [SerializeField] private float escalaFinal = 1.8f;
        [Tooltip("Quanto o boneco treme depois de chegar, em pixels.")]
        [SerializeField] private float intensidadeDoTremor = 30f;
        [Tooltip("Usado se o boneco que atacou não tiver sprite (aparece um bloco vermelho se estiver vazio).")]
        [SerializeField] private Sprite spritePadrao;

        private const float AlturaBase = 1080f;

        private GameObject canvasGO;
        private Image fundo;
        private Image flash;
        private Image monstro;
        private RectTransform monstroRect;
        private AudioSource fonte;
        private AudioClip gritoGerado;
        private bool tocando;

        public bool Tocando
        {
            get { return tocando; }
        }

        private void Awake()
        {
            ConstruirUI();

            fonte = gameObject.AddComponent<AudioSource>();
            fonte.playOnAwake = false;
            fonte.spatialBlend = 0f;
        }

        // Toca o jumpscare e chama aoTerminar quando acabar.
        public void Tocar(Sprite sprite, Action aoTerminar)
        {
            if (tocando)
                return;

            StartCoroutine(Rotina(sprite != null ? sprite : spritePadrao, aoTerminar));
        }

        private IEnumerator Rotina(Sprite sprite, Action aoTerminar)
        {
            tocando = true;
            canvasGO.SetActive(true);

            fundo.color = Color.black;

            if (sprite != null)
            {
                float aspecto = sprite.rect.width / Mathf.Max(1f, sprite.rect.height);
                monstro.sprite = sprite;
                monstro.color = Color.white;
                monstroRect.sizeDelta = new Vector2(AlturaBase * aspecto, AlturaBase);
            }
            else
            {
                monstro.sprite = null;
                monstro.color = new Color(0.7f, 0f, 0f, 1f);
                monstroRect.sizeDelta = new Vector2(AlturaBase * 0.6f, AlturaBase);
            }

            AudioClip clip = somDoGrito != null ? somDoGrito : PegarGritoGerado();
            fonte.PlayOneShot(clip, volume);

            float t = 0f;

            while (t < duracaoTotal)
            {
                t += Time.unscaledDeltaTime;

                // o boneco acelera na direção da tela
                float aproximacao = Mathf.Clamp01(t / Mathf.Max(0.01f, tempoDeAproximacao));
                float escala = Mathf.Lerp(escalaInicial, escalaFinal, Mathf.Pow(aproximacao, 2.5f));

                Vector2 tremor = Vector2.zero;

                if (aproximacao > 0.6f)
                {
                    float forca = aproximacao >= 1f ? 1f : 0.4f;
                    tremor = UnityEngine.Random.insideUnitCircle * intensidadeDoTremor * forca;
                }

                if (aproximacao >= 1f)
                    escala *= 1f + 0.03f * Mathf.Sin(t * 55f);

                monstroRect.localScale = new Vector3(escala, escala, 1f);
                monstroRect.anchoredPosition = tremor;

                // clarão branco rápido no começo
                Color cor = flash.color;
                cor.a = Mathf.Clamp01(1f - t / 0.12f) * 0.85f;
                flash.color = cor;

                yield return null;
            }

            if (aoTerminar != null)
                aoTerminar();

            canvasGO.SetActive(false);
            tocando = false;
        }

        // ------------------------------------------------------------------ UI

        private void ConstruirUI()
        {
            canvasGO = new GameObject("JumpscareCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvasGO.transform.SetParent(transform, false);

            Canvas canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 500;

            CanvasScaler escala = canvasGO.GetComponent<CanvasScaler>();
            escala.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            escala.referenceResolution = new Vector2(1920f, 1080f);
            escala.matchWidthOrHeight = 0.5f;

            fundo = CriarImagem("Fundo", canvasGO.transform, Color.black);
            Esticar(fundo.rectTransform);
            fundo.raycastTarget = true;

            monstro = CriarImagem("Monstro", canvasGO.transform, Color.white);
            monstroRect = monstro.rectTransform;
            monstroRect.anchorMin = new Vector2(0.5f, 0.5f);
            monstroRect.anchorMax = new Vector2(0.5f, 0.5f);
            monstroRect.pivot = new Vector2(0.5f, 0.5f);
            monstro.preserveAspect = true;

            flash = CriarImagem("Flash", canvasGO.transform, new Color(1f, 1f, 1f, 0f));
            Esticar(flash.rectTransform);

            canvasGO.SetActive(false);
        }

        private static Image CriarImagem(string nome, Transform pai, Color cor)
        {
            GameObject go = new GameObject(nome, typeof(RectTransform));
            go.transform.SetParent(pai, false);

            Image imagem = go.AddComponent<Image>();
            imagem.color = cor;
            imagem.raycastTarget = false;
            return imagem;
        }

        private static void Esticar(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        // ------------------------------------------------------------------ grito gerado por código

        private AudioClip PegarGritoGerado()
        {
            if (gritoGerado == null)
                gritoGerado = GerarGrito();

            return gritoGerado;
        }

        // Grito sintético: tom áspero que sobe rápido e treme, ruído, distorção e uma batida grave no início.
        private static AudioClip GerarGrito()
        {
            const int taxa = 44100;
            const float duracao = 1.8f;
            const float doisPi = 2f * Mathf.PI;

            int total = (int)(taxa * duracao);
            float[] dados = new float[total];

            System.Random sorteio = new System.Random(7);
            float fase = 0f;
            float faseVibrato = 0f;
            float faseGrave = 0f;

            for (int i = 0; i < total; i++)
            {
                float t = i / (float)taxa;

                float frequencia = Mathf.Lerp(500f, 1100f, Mathf.Clamp01(t / 0.25f))
                                   * (1f + 0.04f * Mathf.Sin(faseVibrato));

                fase = (fase + doisPi * frequencia / taxa) % doisPi;
                faseVibrato = (faseVibrato + doisPi * 9f / taxa) % doisPi;
                faseGrave = (faseGrave + doisPi * 55f / taxa) % doisPi;

                float serra = fase / Mathf.PI - 1f;
                float tom = serra * 0.6f + Mathf.Sin(fase * 2f) * 0.3f + Mathf.Sin(fase * 3f) * 0.2f;
                float ruido = ((float)sorteio.NextDouble() * 2f - 1f) * 0.3f;
                float grave = Mathf.Sin(faseGrave) * Mathf.Exp(-t * 6f);

                float entrada = Mathf.Clamp01(t / 0.02f);
                float saida = 1f - Mathf.SmoothStep(0f, 1f, Mathf.Clamp01((t - (duracao - 0.5f)) / 0.5f));

                float amostra = (tom + ruido) * entrada * saida + grave * 0.9f;
                dados[i] = (float)Math.Tanh(amostra * 2.2f) * 0.9f;
            }

            AudioClip clip = AudioClip.Create("GritoGerado", total, 1, taxa, false);
            clip.SetData(dados, 0);
            return clip;
        }
    }
}
