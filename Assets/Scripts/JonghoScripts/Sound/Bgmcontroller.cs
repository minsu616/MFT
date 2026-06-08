using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬(Scene) 이름에 따라 BGM을 자동으로 전환합니다.
/// 각 씬의 루트 오브젝트 혹은 GameManager에 붙여서 사용하세요.
/// </summary>
public class BGMController : MonoBehaviour
{
    [System.Serializable]
    public class SceneBGMEntry
    {
        [Tooltip("씬 이름 (Build Settings의 씬 이름과 정확히 일치해야 합니다)")]
        public string sceneName;
        public SoundID bgmID;
    }

    [Header("씬별 BGM 매핑")]
    [SerializeField] private SceneBGMEntry[] sceneBGMMap;

    [Header("기본 BGM (매핑 없는 씬에서 재생)")]
    [SerializeField] private SoundID defaultBGM = SoundID.BGM_Lobby;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // 최초 씬에서도 BGM 재생
        PlayBGMForScene(SceneManager.GetActiveScene().name);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayBGMForScene(scene.name);
    }

    private void PlayBGMForScene(string sceneName)
    {
        if (SoundManager.Instance == null) return;

        SoundID bgmID = defaultBGM;

        foreach (var entry in sceneBGMMap)
        {
            if (entry.sceneName == sceneName)
            {
                bgmID = entry.bgmID;
                break;
            }
        }

        // 이미 같은 BGM이 재생 중이면 끊지 않고 그대로 이어감
        if (SoundManager.Instance.CurrentBgmId == bgmID) return;

        SoundManager.Instance.ChangeBGM(bgmID);
    }
}