using UnityEngine;

namespace MovingIn.Hub{
    [RequireComponent(typeof(ParticleSystem))]
    public class AmbientParticles : MonoBehaviour
    {
        public enum Kind { Dust, Motes, Leaves, Fireflies, Sparks, Drips, Glints }

        [SerializeField] Kind kind = Kind.Dust;
        [SerializeField] AtmosphereMode mode = AtmosphereMode.Always;
        [SerializeField] Vector3 area = Vector3.one;
        [SerializeField] float rate = 4f;

        ParticleSystem _ps;
        float _weight;
        float _nextBurst;

        public void Setup(Kind k, AtmosphereMode m, Vector3 size, float perSecond)
        {
            kind = k;
            mode = m;
            area = size;
            rate = perSecond;
            Configure();
        }

        void Awake()
        {
            Configure();
        }

        void OnEnable()
        {
            _weight = Atmosphere.Weight(mode);
            SetRate();
        }

        void Update()
        {
            _weight = Mathf.MoveTowards(_weight, Atmosphere.Weight(mode), Time.deltaTime);
            SetRate();
            if (kind == Kind.Sparks && _weight > 0.5f && Time.time >= _nextBurst)
            {
                _ps.Emit(Random.Range(6, 14));
                _nextBurst = Time.time + Random.Range(1.2f, 3.5f);
            }
        }

        void SetRate()
        {
            if (_ps == null) _ps = GetComponent<ParticleSystem>();
            ParticleSystem.EmissionModule e = _ps.emission;
            e.rateOverTimeMultiplier = kind == Kind.Sparks ? 0f : rate * _weight;
        }

        public void Configure()
        {
            _ps = GetComponent<ParticleSystem>();
            bool playing = _ps.isPlaying;
            if (playing) _ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ParticleSystem.MainModule main = _ps.main;
            main.loop = true;
            main.playOnAwake = true;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startSpeed = 0f;
            main.gravityModifier = 0f;
            main.startRotation = 0f;

            ParticleSystem.ShapeModule shape = _ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = area;

            ParticleSystem.NoiseModule noise = _ps.noise;
            noise.enabled = false;
            ParticleSystem.VelocityOverLifetimeModule vel = _ps.velocityOverLifetime;
            vel.enabled = false;
            ParticleSystem.RotationOverLifetimeModule rot = _ps.rotationOverLifetime;
            rot.enabled = false;
            ParticleSystem.ColorOverLifetimeModule col = _ps.colorOverLifetime;
            col.enabled = true;

            switch (kind)
            {
                case Kind.Dust:
                case Kind.Motes:
                    bool dust = kind == Kind.Dust;
                    main.prewarm = true;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
                    main.startColor = dust ? new Color(1f, 0.93f, 0.75f, 0.7f) : new Color(0.85f, 0.8f, 0.72f, 0.45f);
                    main.maxParticles = dust ? 60 : 40;
                    noise.enabled = true;
                    noise.strength = 0.06f;
                    noise.frequency = 0.3f;
                    noise.scrollSpeed = 0.1f;
                    col.color = Fade(Color.white, 0.25f, 0.75f);
                    break;
                case Kind.Leaves:
                    main.prewarm = true;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 8f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.11f);
                    main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
                    main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.53f, 0.71f, 0.43f), new Color(0.91f, 0.63f, 0.29f));
                    main.maxParticles = 30;
                    vel.enabled = true;
                    vel.space = ParticleSystemSimulationSpace.World;
                    vel.x = new ParticleSystem.MinMaxCurve(0.05f, 0.25f);
                    vel.y = new ParticleSystem.MinMaxCurve(-0.45f, -0.3f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
                    noise.enabled = true;
                    noise.strength = 0.35f;
                    noise.frequency = 0.5f;
                    rot.enabled = true;
                    rot.z = new ParticleSystem.MinMaxCurve(-2f, 2f);
                    col.color = Fade(Color.white, 0.05f, 0.85f);
                    break;
                case Kind.Fireflies:
                    main.prewarm = true;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(4f, 7f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.07f, 0.09f);
                    main.startColor = new Color(0.92f, 0.97f, 0.6f, 1f);
                    main.maxParticles = 40;
                    noise.enabled = true;
                    noise.strength = 0.35f;
                    noise.frequency = 0.4f;
                    noise.scrollSpeed = 0.2f;
                    col.color = Blink();
                    break;
                case Kind.Sparks:
                    main.prewarm = false;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.6f);
                    main.startSpeed = new ParticleSystem.MinMaxCurve(1f, 2.5f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.06f);
                    main.startColor = new Color(1f, 0.88f, 0.55f, 1f);
                    main.gravityModifier = 1f;
                    main.maxParticles = 40;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.angle = 35f;
                    shape.radius = 0.05f;
                    shape.scale = Vector3.one;
                    col.color = Fade(Color.white, 0f, 0.6f);
                    break;
                case Kind.Drips:
                    main.prewarm = false;
                    main.startLifetime = 0.75f;
                    main.startSize = 0.05f;
                    main.startColor = new Color(0.66f, 0.8f, 0.88f, 0.9f);
                    main.gravityModifier = 1f;
                    main.maxParticles = 6;
                    col.color = Fade(Color.white, 0f, 0.95f);
                    break;
                case Kind.Glints:
                    main.prewarm = true;
                    main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 1.1f);
                    main.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.07f);
                    main.startColor = new Color(1f, 1f, 0.95f, 0.9f);
                    main.maxParticles = 20;
                    col.color = Fade(Color.white, 0.3f, 0.7f);
                    break;
            }

            ParticleSystemRenderer r = GetComponent<ParticleSystemRenderer>();
            r.renderMode = ParticleSystemRenderMode.Billboard;
            r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            r.receiveShadows = false;
            SetRate();
            if (playing || Application.isPlaying) _ps.Play();
        }

        static ParticleSystem.MinMaxGradient Fade(Color c, float inEnd, float outStart)
        {
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(c, 0f), new GradientColorKey(c, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, Mathf.Max(0.01f, inEnd)), new GradientAlphaKey(1f, outStart), new GradientAlphaKey(0f, 1f) });
            return new ParticleSystem.MinMaxGradient(g);
        }

        static ParticleSystem.MinMaxGradient Blink()
        {
            Gradient g = new Gradient();
            g.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, 0.15f), new GradientAlphaKey(0.15f, 0.35f), new GradientAlphaKey(1f, 0.55f), new GradientAlphaKey(0.2f, 0.75f), new GradientAlphaKey(0f, 1f) });
            return new ParticleSystem.MinMaxGradient(g);
        }
    }
}
