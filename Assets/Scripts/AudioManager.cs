using UnityEngine;

// Central audio dispatcher. Subscribes to the global event bus and plays a sound
// for each gameplay event. Each slot has two ways to source audio:
//   1. Drag an AudioClip into the inspector slot — that clip will play.
//   2. Leave it empty — a simple procedural fallback (generated in code) plays
//      instead, so the game has audible feedback out of the box.
//
// Recommended drop-in pack: https://kenney.nl/assets/sci-fi-sounds (CC0).
public class AudioManager : MonoBehaviour
{
    [Header("Audio clips (optional — overrides procedural fallback)")]
    [SerializeField] AudioClip gunFireClip;
    [SerializeField] AudioClip enemyDeathClip;
    [SerializeField] AudioClip playerDamageClip;
    [SerializeField] AudioClip gameOverClip;

    [Header("Music")]
    [Tooltip("Optional looping music track. Leave empty for no music.")]
    [SerializeField] AudioClip musicClip;

    [Header("Volume")]
    [Range(0f, 1f)] [SerializeField] float sfxVolume = 0.6f;
    [Range(0f, 1f)] [SerializeField] float musicVolume = 0.25f;

    EventManagerSO eventManager;
    AudioSource sfxSource;
    AudioSource musicSource;

    // Procedural fallback clips — generated once in Awake.
    AudioClip _proceduralGunshot;
    AudioClip _proceduralEnemyDeath;
    AudioClip _proceduralPlayerDamage;
    AudioClip _proceduralGameOver;

    // Track last-known player health so we only play the hurt sound on a decrease,
    // not on the initial PlayerHealthChanged event (which fires at full health).
    float _lastPlayerHealth = -1f;

    private void Awake()
    {
        eventManager = Resources.Load<EventManagerSO>("EventManager");

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.spatialBlend = 0f; // 2D (UI-style, no falloff)

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        _proceduralGunshot = MakeNoiseBurst(0.1f, 0.55f);
        _proceduralEnemyDeath = MakeDescendingTone(0.25f, 600f, 180f, 0.5f);
        _proceduralPlayerDamage = MakeDescendingTone(0.18f, 320f, 110f, 0.55f);
        _proceduralGameOver = MakeDescendingTone(0.9f, 220f, 60f, 0.45f);
    }

    private void OnEnable()
    {
        if (eventManager == null) return;
        eventManager.onGunFired += OnGunFired;
        eventManager.onEnemyDefeated += OnEnemyDefeated;
        eventManager.onPlayerHealthChanged += OnPlayerHealthChanged;
        eventManager.onGameOver += OnGameOver;
    }

    private void OnDisable()
    {
        if (eventManager == null) return;
        eventManager.onGunFired -= OnGunFired;
        eventManager.onEnemyDefeated -= OnEnemyDefeated;
        eventManager.onPlayerHealthChanged -= OnPlayerHealthChanged;
        eventManager.onGameOver -= OnGameOver;
    }

    private void Start()
    {
        if (musicClip != null)
        {
            musicSource.clip = musicClip;
            musicSource.Play();
        }
    }

    // === Event handlers ===

    private void OnGunFired()
    {
        PlayOneShot(gunFireClip, _proceduralGunshot);
    }

    private void OnEnemyDefeated(int _)
    {
        PlayOneShot(enemyDeathClip, _proceduralEnemyDeath);
    }

    private void OnPlayerHealthChanged(float current, float max)
    {
        // Skip the very first event (game start, full health).
        if (_lastPlayerHealth < 0f)
        {
            _lastPlayerHealth = current;
            return;
        }

        // Only react to decreases. Skip if the hit would also trigger GameOver
        // (we have a dedicated gameOver sound for that).
        if (current < _lastPlayerHealth && current > 0f)
        {
            PlayOneShot(playerDamageClip, _proceduralPlayerDamage);
        }

        _lastPlayerHealth = current;
    }

    private void OnGameOver()
    {
        PlayOneShot(gameOverClip, _proceduralGameOver);
    }

    // === Helpers ===

    private void PlayOneShot(AudioClip preferred, AudioClip fallback)
    {
        var clip = preferred != null ? preferred : fallback;
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // === Procedural audio generation ===
    // These build short AudioClips from raw PCM samples. Quality is intentionally
    // basic — a placeholder until real audio assets are dropped in.

    static AudioClip MakeNoiseBurst(float durationSeconds, float amplitude)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
        float[] data = new float[sampleCount];
        var rng = new System.Random(1);

        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            float envelope = Mathf.Exp(-t * 8f); // sharp decay
            float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
            data[i] = noise * envelope * amplitude;
        }

        var clip = AudioClip.Create("proc_noise_burst", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    static AudioClip MakeDescendingTone(float durationSeconds, float startHz, float endHz, float amplitude)
    {
        const int sampleRate = 44100;
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
        float[] data = new float[sampleCount];

        float phase = 0f;
        for (int i = 0; i < sampleCount; i++)
        {
            float t = (float)i / sampleCount;
            // Linear frequency sweep from startHz → endHz.
            float frequency = Mathf.Lerp(startHz, endHz, t);
            phase += frequency * Mathf.PI * 2f / sampleRate;
            // Triangle-ish wave: clamp-folded sine for a fuller body than pure sine.
            float wave = Mathf.Sin(phase);
            // Asymmetric attack/decay envelope: quick rise, slower fall.
            float envelope = (t < 0.05f)
                ? t / 0.05f
                : Mathf.Exp(-(t - 0.05f) * 3.5f);
            data[i] = wave * envelope * amplitude;
        }

        var clip = AudioClip.Create("proc_descending_tone", sampleCount, 1, sampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
