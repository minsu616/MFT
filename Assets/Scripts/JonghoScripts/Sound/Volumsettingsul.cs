using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정 화면의 볼륨 슬라이더 UI와 SoundManager를 연결합니다.
/// Canvas의 설정 패널에 붙여서 사용하세요.
/// </summary>
public class VolumeSettingsUI : MonoBehaviour
{
    [Header("BGM 볼륨")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private TextMeshProUGUI bgmValueText;  // 선택사항

    [Header("SFX 볼륨")]
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private TextMeshProUGUI sfxValueText;  // 선택사항

    private void Start()
    {
        if (SoundManager.Instance == null) return;

        // 슬라이더 초기값을 현재 볼륨으로 설정
        if (bgmSlider != null)
        {
            bgmSlider.value = SoundManager.Instance.BgmVolume;
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }

        if (sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.SfxVolume;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }

        UpdateLabels();
    }

    private void OnDestroy()
    {
        bgmSlider?.onValueChanged.RemoveListener(OnBGMVolumeChanged);
        sfxSlider?.onValueChanged.RemoveListener(OnSFXVolumeChanged);
    }

    // ─────────────────────────────────────────
    // 슬라이더 콜백
    // ─────────────────────────────────────────

    private void OnBGMVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.BgmVolume = value;
        UpdateLabels();
    }

    private void OnSFXVolumeChanged(float value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.SfxVolume = value;
        UpdateLabels();
    }

    // ─────────────────────────────────────────
    // 버튼 이벤트 (UnityEvent용)
    // ─────────────────────────────────────────

    /// <summary>뮤트 토글 (BGM)</summary>
    public void ToggleBGMMute()
    {
        if (SoundManager.Instance == null) return;
        float newVol = SoundManager.Instance.BgmVolume > 0 ? 0f : 0.7f;
        SoundManager.Instance.BgmVolume = newVol;
        if (bgmSlider) bgmSlider.value = newVol;
        UpdateLabels();
    }

    /// <summary>뮤트 토글 (SFX)</summary>
    public void ToggleSFXMute()
    {
        if (SoundManager.Instance == null) return;
        float newVol = SoundManager.Instance.SfxVolume > 0 ? 0f : 1f;
        SoundManager.Instance.SfxVolume = newVol;
        if (sfxSlider) sfxSlider.value = newVol;
        UpdateLabels();
    }

    // ─────────────────────────────────────────
    // 레이블 업데이트
    // ─────────────────────────────────────────
    private void UpdateLabels()
    {
        if (SoundManager.Instance == null) return;
        if (bgmValueText) bgmValueText.text = $"{(int)(SoundManager.Instance.BgmVolume * 100)}%";
        if (sfxValueText) sfxValueText.text = $"{(int)(SoundManager.Instance.SfxVolume * 100)}%";
    }
}