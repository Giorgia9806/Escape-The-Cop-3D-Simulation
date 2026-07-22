using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public sealed class AudioManager : MonoBehaviour
{
    /// <summary>
    /// Simple persistent singleton. Access via AudioManager.I
    /// </summary>
    public static AudioManager I { get; private set; }

    [Header("Music Clips")]
    [SerializeField] private AudioClip musicMenuStart;   // Menu + early gameplay
    [SerializeField] private AudioClip musicChase;       // Chase / cop follows you

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxCoin;
    [SerializeField] private AudioClip sfxTrampoline;
    [SerializeField] private AudioClip sfxWin;
    [SerializeField] private AudioClip sfxFail;

    [Header("Volumes")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1.0f;

    [Header("Duck settings (Win/Fail)")]
    [Range(0f, 1f)]
    [SerializeField] private float duckToVolume = 0.2f;     // Target music volume during duck
    [SerializeField] private float duckDuration = 0.25f;    // How fast we duck

    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private Coroutine _duckRoutine;
    private bool _playMenuMusicAfterReload;

    private void Awake()
    {
        // Singleton setup
        if (I != null && I != this)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        // Create and configure audio sources
        _musicSource = gameObject.AddComponent<AudioSource>();
        _sfxSource = gameObject.AddComponent<AudioSource>();

        _musicSource.playOnAwake = false;
        _musicSource.loop = true;

        _sfxSource.playOnAwake = false;
        _sfxSource.loop = false;

        ApplyVolumes();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // =========================
    // MUSIC
    // =========================

    /// <summary>
    /// Plays a music clip on the dedicated music AudioSource.
    /// Prevents unnecessary restarts when the same clip is already playing.
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (clip == null) return;

        if (_musicSource.clip == clip && _musicSource.isPlaying)
            return;

        _musicSource.Stop();
        _musicSource.clip = clip;
        _musicSource.loop = loop;
        _musicSource.volume = musicVolume;
        _musicSource.Play();
    }

    public void StopMusic()
    {
        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void SetMusicVolume(float value01)
    {
        musicVolume = Mathf.Clamp01(value01);
        ApplyVolumes();
    }

    // Convenience shortcuts
    public void MusicMenuStart() => PlayMusic(musicMenuStart, loop: true);
    public void MusicChase() => PlayMusic(musicChase, loop: true);

    // =========================
    // SFX
    // =========================

    /// <summary>
    /// Plays a one-shot SFX on the dedicated SFX AudioSource.
    /// </summary>
    public void PlaySfx(AudioClip clip, float volumeMultiplier = 1f, bool stopPreviousSfx = false)
    {
        if (clip == null) return;

        if (stopPreviousSfx)
            _sfxSource.Stop();

        _sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume) * volumeMultiplier);
    }

    public void SfxCoin() => PlaySfx(sfxCoin, volumeMultiplier: 0.9f, stopPreviousSfx: false);
    public void SfxTrampoline() => PlaySfx(sfxTrampoline, volumeMultiplier: 1.0f, stopPreviousSfx: false);

    // =========================
    // WIN/FAIL (duck music + play SFX)
    // =========================

    public void WinDuckAndPlay() => StartDuckRoutine(sfxWin);
    public void FailDuckAndPlay() => StartDuckRoutine(sfxFail);

    private void StartDuckRoutine(AudioClip sfxClip)
    {
        if (_duckRoutine != null)
            StopCoroutine(_duckRoutine);

        _duckRoutine = StartCoroutine(DuckAndPlayCoroutine(sfxClip));
    }

    private IEnumerator DuckAndPlayCoroutine(AudioClip sfxClip)
    {
        // If there is no music running, just play the SFX.
        if (!_musicSource.isPlaying)
        {
            PlaySfx(sfxClip, volumeMultiplier: 1f, stopPreviousSfx: true);
            yield break;
        }

        float startVol = _musicSource.volume;
        float t = 0f;

        // Duck the music volume over duckDuration
        while (t < duckDuration)
        {
            t += Time.deltaTime;
            float alpha = duckDuration <= 0f ? 1f : (t / duckDuration);
            _musicSource.volume = Mathf.Lerp(startVol, duckToVolume, alpha);
            yield return null;
        }

        // Stop music completely (as in your original logic)
        _musicSource.Stop();

        // Play win/fail SFX (optionally stopping any previous SFX)
        PlaySfx(sfxClip, volumeMultiplier: 1f, stopPreviousSfx: true);

        _duckRoutine = null;
    }

    // =========================
    // SCENE HOOKS
    // =========================

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Safety: ensure time scale is reset when loading scenes
        Time.timeScale = 1f;

        // If requested (e.g., after reload), start menu music once
        if (_playMenuMusicAfterReload)
        {
            _playMenuMusicAfterReload = false;
            MusicMenuStart();
        }

        // Optional: you can auto-switch music by scene name here if you want.
        // Example:
        // if (scene.name == "Game") MusicMenuStart();
    }

    public void RequestMenuMusicAfterReload()
    {
        _playMenuMusicAfterReload = true;
    }

    private void ApplyVolumes()
    {
        if (_musicSource != null)
            _musicSource.volume = musicVolume;

        if (_sfxSource != null)
            _sfxSource.volume = sfxVolume;
    }
}

