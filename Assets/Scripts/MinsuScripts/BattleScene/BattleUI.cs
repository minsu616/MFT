using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour
{
    [Header("텍스트 UI")]
    public TextMeshProUGUI apText;       // AP 표시
    public TextMeshProUGUI turnText;     // 턴 표시
    public TextMeshProUGUI phaseText;    // 단계 표시

    [Header("버튼")]
    public Button commandCompleteButton; // 명령 완료 버튼

    private TurnManager turnManager;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();

        // 명령 완료 버튼 클릭 이벤트 연결
        commandCompleteButton.onClick.AddListener(OnCommandComplete);

        UpdateUI();
    }

    void Update()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (turnManager == null) return;

        // AP 표시
        apText.text = $"AP: {turnManager.GetCurrentAP()} / 5";

        // 턴 표시
        turnText.text = turnManager.currentTurn == TurnManager.Turn.Player1
            ? "플레이어 1 턴"
            : "플레이어 2 턴";

        // 단계 표시
        switch (turnManager.currentPhase)
        {
            case TurnManager.Phase.Command:
                phaseText.text = "명령";
                commandCompleteButton.interactable = true;
                break;
            case TurnManager.Phase.Move:
                phaseText.text = "이동";
                commandCompleteButton.interactable = false;
                break;
            case TurnManager.Phase.Execute:
                phaseText.text = "공격";
                commandCompleteButton.interactable = false;
                break;
        }
    }

    // 명령 완료 버튼 클릭
    void OnCommandComplete()
    {
        FindObjectOfType<PhotonBattleSync>().SendReady();
    }
}