using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SettingsUIManager : MonoBehaviour
{
    [Header("Main UI")]
    public GameObject settingsPanel; // 환경설정 최상위 UI 패널 (ESC로 켜고 꺼지는 대상)
    public Button closeXButton;      // 우측 하단 취소/확인 또는 우측 상단 X 버튼

    [Header("탭 버튼 (Tab Buttons)")]
    public Button screenTabButton;   // 화면 탭
    public Button audioTabButton;    // 음향 탭
    public Button controlTabButton;  // 조작 탭
    public Button optionTabButton;   // 옵션 탭

    [Header("설정 패널 (Settings Panels)")]
    public GameObject screenPanel;   // 화면 설정 내용이 들어갈 패널
    public GameObject audioPanel;    // 음향 설정 내용이 들어갈 패널
    public GameObject controlPanel;  // 조작 설정 내용이 들어갈 패널
    public GameObject optionPanel;   // 옵션 설정 내용이 들어갈 패널

    [Header("Screen Settings (화면)")]
    public Toggle fullscreenToggle;     // 전체화면 토글 버튼
    public Dropdown resolutionDropdown; // 해상도 드롭다운
    public Slider brightnessSlider;     // 밝기 조절 슬라이더

    [Header("Audio Settings (음향)")]
    public Slider masterVolumeSlider;   // 전체 음량 슬라이더
    public Slider bgmVolumeSlider;      // 배경음악 슬라이더
    public Slider sfxVolumeSlider;      // 효과음 슬라이더

    [Header("Control Settings (조작)")]
    public Slider mouseSensitivitySlider; // 마우스 감도 슬라이더

    void Start()
    {
        // 1. 초기화 및 이벤트 연결
        InitializeSettings();

        // 2. 저장된 데이터 불러오기 (게임을 껐다 켜도 유지됨)
        LoadSavedSettings();

        // 3. 처음 설정 창을 열었을 때는 '화면' 탭이 기본으로 보이게 설정
        SwitchTab(screenPanel);
    }

    void Update()
    {
        // [기능] ESC 키를 눌렀을 때 환경설정 창을 켜거나 끄는 기능
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettingsUI();
        }
    }

    /// <summary>
    /// [기능] 버튼 클릭 및 슬라이더 값 변경 시 실행될 함수들을 연결(Listening)해주는 기능입니다.
    /// </summary>
    private void InitializeSettings()
    {
        // --- 탭 전환 이벤트 연결 ---
        if (screenTabButton != null) screenTabButton.onClick.AddListener(() => SwitchTab(screenPanel));
        if (audioTabButton != null) audioTabButton.onClick.AddListener(() => SwitchTab(audioPanel));
        if (controlTabButton != null) controlTabButton.onClick.AddListener(() => SwitchTab(controlPanel));
        if (optionTabButton != null) optionTabButton.onClick.AddListener(() => SwitchTab(optionPanel));

        // --- 닫기 버튼 이벤트 연결 ---
        if (closeXButton != null) closeXButton.onClick.AddListener(CloseSettingsUI);

        // --- 해상도 드롭다운 옵션 설정 ---
        if (resolutionDropdown != null)
        {
            resolutionDropdown.ClearOptions();
            List<string> options = new List<string> { "1920 x 1080", "1280 x 720" };
            resolutionDropdown.AddOptions(options);
            resolutionDropdown.onValueChanged.AddListener(SetResolution);
        }

        // --- 설정 UI 값 변경 이벤트 연결 ---
        if (fullscreenToggle != null) fullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        if (brightnessSlider != null) brightnessSlider.onValueChanged.AddListener(SetBrightness);

        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(SetBGMVolume);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (mouseSensitivitySlider != null) mouseSensitivitySlider.onValueChanged.AddListener(SetMouseSensitivity);
    }

    /// <summary>
    /// [기능] 로컬에 저장된 환경설정 데이터를 불러와서 UI에 적용하는 기능입니다.
    /// </summary>
    private void LoadSavedSettings()
    {
        if (fullscreenToggle != null) fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (resolutionDropdown != null) resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex", 0);
        if (brightnessSlider != null) brightnessSlider.value = PlayerPrefs.GetFloat("Brightness", 1f);

        if (masterVolumeSlider != null) masterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 1f);
        if (bgmVolumeSlider != null) bgmVolumeSlider.value = PlayerPrefs.GetFloat("BGMVolume", 1f);
        if (sfxVolumeSlider != null) sfxVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 1f);

        if (mouseSensitivitySlider != null) mouseSensitivitySlider.value = PlayerPrefs.GetFloat("MouseSensitivity", 1f);
    }

    // ==========================================
    // UI 활성화 / 비활성화 및 탭 전환 제어
    // ==========================================

    public void ToggleSettingsUI()
    {
        if (settingsPanel != null)
        {
            bool isActive = settingsPanel.activeSelf;
            settingsPanel.SetActive(!isActive);
        }
    }

    public void OpenSettingsUI() { if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void CloseSettingsUI() { if (settingsPanel != null) settingsPanel.SetActive(false); }

    private void SwitchTab(GameObject activePanel)
    {
        // 모든 패널을 먼저 끄고 매개변수로 받은 패널만 켭니다.
        if (screenPanel != null) screenPanel.SetActive(false);
        if (audioPanel != null) audioPanel.SetActive(false);
        if (controlPanel != null) controlPanel.SetActive(false);
        if (optionPanel != null) optionPanel.SetActive(false);

        if (activePanel != null) activePanel.SetActive(true);
    }

    // ==========================================
    // 설정 적용 및 저장 로직
    // ==========================================

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetResolution(int resolutionIndex)
    {
        int width = 1920;
        int height = 1080;

        if (resolutionIndex == 1)
        {
            width = 1280;
            height = 720;
        }

        Screen.SetResolution(width, height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    public void SetBrightness(float value)
    {
        Debug.Log("밝기 조절됨: " + value);
        PlayerPrefs.SetFloat("Brightness", value);
        PlayerPrefs.Save();
    }

    public void SetMasterVolume(float volume)
    {
        Debug.Log("전체 음량 조절됨: " + volume);
        PlayerPrefs.SetFloat("MasterVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetBGMVolume(float volume)
    {
        Debug.Log("배경음악 조절됨: " + volume);
        PlayerPrefs.SetFloat("BGMVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float volume)
    {
        Debug.Log("효과음 조절됨: " + volume);
        PlayerPrefs.SetFloat("SFXVolume", volume);
        PlayerPrefs.Save();
    }

    public void SetMouseSensitivity(float sensitivity)
    {
        Debug.Log("마우스 감도 조절됨: " + sensitivity);
        PlayerPrefs.SetFloat("MouseSensitivity", sensitivity);
        PlayerPrefs.Save();
    }
}