using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 전체 사운드를 관리하는 싱글턴 매니저.
/// BGM(배경음악)과 SFX(효과음)를 독립적으로 제어합니다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    // ─────────────────────────────────────────
    // 싱글턴
    // ─────────────────────────────────────────
    public static SoundManager Instance { get; private set; }

    // ─────────────────────────────────────────
    // 인스펙터 설정
    // ─────────────────────────────────────────
    [Header("BGM 설정")]
    [SerializeField] private AudioSource bgmSource;          // BGM 전용 AudioSource
    [SerializeField][Range(0f, 1f)] private float bgmVolume = 0.7f;
    [SerializeField] private float bgmFadeDuration = 1.0f;   // 크로스페이드 시간(초)

    [Header("SFX 설정")]
    [SerializeField] private AudioSource sfxSource;          // SFX 기본 AudioSource
    [SerializeField][Range(0f, 1f)] private float sfxVolume = 1.0f;
    [SerializeField] private int sfxPoolSize = 8;            // 동시 재생 최대 채널 수

    [Header("사운드 클립 등록")]
    [SerializeField] private SoundLibrary soundLibrary;

    // ─────────────────────────────────────────
    // 내부 상태
    // ─────────────────────────────────────────
    private List<AudioSource> sfxPool = new();   // SFX 오브젝트 풀
    private Coroutine bgmFadeCoroutine;
    private SoundID? _currentBgmId;              // 현재 재생 중인 BGM ID

    /// <summary>현재 재생 중인 BGM ID (외부에서 읽기 전용)</summary>
    public SoundID? CurrentBgmId => _currentBgmId;

    // ─────────────────────────────────────────
    // 볼륨 프로퍼티 (저장/불러오기 포함)
    // ─────────────────────────────────────────
    public float BgmVolume
    {
        get => bgmVolume;
        set
        {
            bgmVolume = Mathf.Clamp01(value);
            bgmSource.volume = bgmVolume;
            PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        }
    }

    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        }
    }

    // ─────────────────────────────────────────
    // 초기화
    // ─────────────────────────────────────────
    private void Awake()
    {
        // 싱글턴 처리
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadVolumeSettings();
        BuildSfxPool();
    }

    /// <summary>PlayerPrefs에서 볼륨 불러오기</summary>
    private void LoadVolumeSettings()
    {
        bgmVolume = PlayerPrefs.GetFloat("BGMVolume", bgmVolume);
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", sfxVolume);
        bgmSource.volume = bgmVolume;
    }

    /// <summary>SFX 오브젝트 풀 생성</summary>
    private void BuildSfxPool()
    {
        for (int i = 0; i < sfxPoolSize; i++)
        {
            var go = new GameObject($"SFX_Pool_{i}");
            go.transform.SetParent(transform);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            sfxPool.Add(source);
        }
    }

    // ─────────────────────────────────────────
    // BGM API
    // ─────────────────────────────────────────

    /// <summary>BGM을 즉시 재생합니다.</summary>
    public void PlayBGM(SoundID id, bool loop = true)
    {
        AudioClip clip = soundLibrary.GetClip(id);
        if (clip == null) { Debug.LogWarning($"[SoundManager] 클립 없음: {id}"); return; }

        _currentBgmId = id;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    /// <summary>페이드 아웃 → 새 BGM 페이드 인 (크로스페이드)</summary>
    public void ChangeBGM(SoundID id, bool loop = true)
    {
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(CrossFadeBGM(id, loop));
    }

    /// <summary>BGM을 페이드 아웃 후 정지합니다.</summary>
    public void StopBGM()
    {
        _currentBgmId = null;
        if (bgmFadeCoroutine != null) StopCoroutine(bgmFadeCoroutine);
        bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
    }

    public void PauseBGM() => bgmSource.Pause();
    public void ResumeBGM() => bgmSource.UnPause();

    // ─────────────────────────────────────────
    // SFX API
    // ─────────────────────────────────────────

    /// <summary>효과음을 재생합니다. (오브젝트 풀 사용)</summary>
    public void PlaySFX(SoundID id, float pitchVariance = 0f)
    {
        AudioClip clip = soundLibrary.GetClip(id);
        if (clip == null) { Debug.LogWarning($"[SoundManager] 클립 없음: {id}"); return; }

        AudioSource source = GetAvailableSource();
        source.clip = clip;
        source.volume = sfxVolume;
        source.pitch = 1f + UnityEngine.Random.Range(-pitchVariance, pitchVariance);
        source.Play();
    }

    /// <summary>월드 공간의 특정 위치에서 3D 효과음을 재생합니다.</summary>
    public void PlaySFXAtPoint(SoundID id, Vector3 worldPosition)
    {
        AudioClip clip = soundLibrary.GetClip(id);
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, worldPosition, sfxVolume);
    }

    // ─────────────────────────────────────────
    // 내부 유틸
    // ─────────────────────────────────────────

    /// <summary>풀에서 재생 중이 아닌 AudioSource를 반환합니다.</summary>
    private AudioSource GetAvailableSource()
    {
        foreach (var src in sfxPool)
            if (!src.isPlaying) return src;

        // 모두 사용 중이면 가장 오래된 것을 재사용
        return sfxPool[0];
    }

    private IEnumerator CrossFadeBGM(SoundID id, bool loop)
    {
        // 1) 현재 BGM 페이드 아웃
        float elapsed = 0f;
        float startVol = bgmSource.volume;
        while (elapsed < bgmFadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgmSource.Stop();

        // 2) 새 BGM 페이드 인
        PlayBGM(id, loop);
        bgmSource.volume = 0f;
        elapsed = 0f;
        while (elapsed < bgmFadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(0f, bgmVolume, elapsed / bgmFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgmSource.volume = bgmVolume;
    }

    private IEnumerator FadeOutBGM()
    {
        float elapsed = 0f;
        float startVol = bgmSource.volume;
        while (elapsed < bgmFadeDuration)
        {
            bgmSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        bgmSource.Stop();
        bgmSource.volume = bgmVolume; // 볼륨값 복원 (다음 재생을 위해)
    }
}