using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionButtonUI : MonoBehaviour
{
    [Header("버튼")]
    public Button moveButton;
    public Button attackButton;
    public Button skillButton;

    [Header("버튼 이미지 (나중에 교체용)")]
    public Image moveButtonImage;
    public Image attackButtonImage;
    public Image skillButtonImage;

    [Header("선택됐을 때 색상")]
    public Color selectedColor = new Color(1f, 0.8f, 0f);   // 노란색
    public Color normalColor = new Color(1f, 1f, 1f);        // 흰색
    public Color disabledColor = new Color(0.5f, 0.5f, 0.5f); // 회색 (AP부족)

    private ShipSelector shipSelector;
    private TurnManager turnManager;

    // 현재 선택된 행동들 (중복 선택 가능)
    public bool moveSelected { get; set; }
    public bool attackSelected { get; set; }
    public bool skillSelected { get; private set; }

    void Start()
    {
        shipSelector = FindObjectOfType<ShipSelector>();
        turnManager = FindObjectOfType<TurnManager>();

        // 버튼 클릭 이벤트 연결
        moveButton.onClick.AddListener(OnMoveButton);
        attackButton.onClick.AddListener(OnAttackButton);
        skillButton.onClick.AddListener(OnSkillButton);

        // 처음엔 전부 숨기기
        HideAllButtons();
    }

    // 함선 선택시 버튼 표시
    public void ShowButtons()
    {
        moveSelected = false;
        attackSelected = false;
        skillSelected = false;

        moveButton.gameObject.SetActive(true);
        attackButton.gameObject.SetActive(true);
        skillButton.gameObject.SetActive(true);

        UpdateButtonColors();
    }

    // 버튼 숨기기
    public void HideAllButtons()
    {
        moveButton.gameObject.SetActive(false);
        attackButton.gameObject.SetActive(false);
        skillButton.gameObject.SetActive(false);
    }

    // 이동 버튼 클릭
    void OnMoveButton()
    {
        if (!turnManager.CanUse(APManager.MOVE_COST))
        {
            Debug.Log("AP 부족! 이동 불가");
            return;
        }
        moveSelected = !moveSelected; // 토글
        Debug.Log($"이동 선택: {moveSelected}");
        UpdateButtonColors();
    }

    // 공격 버튼 클릭
    void OnAttackButton()
    {
        if (!turnManager.CanUse(APManager.ATTACK_COST))
        {
            Debug.Log("AP 부족! 공격 불가");
            return;
        }
        attackSelected = !attackSelected; // 토글
        Debug.Log($"공격 선택: {attackSelected}");
        UpdateButtonColors();
    }

    // 스킬 버튼 클릭
    void OnSkillButton()
    {
        if (!turnManager.CanUse(APManager.SKILL_COST))
        {
            Debug.Log("AP 부족! 스킬 불가");
            return;
        }
        skillSelected = !skillSelected; // 토글
        Debug.Log($"스킬 선택: {skillSelected}");
        UpdateButtonColors();
    }

    // 버튼 색상 업데이트
    void UpdateButtonColors()
    {
        // 이동 버튼
        SetButtonColor(moveButton,
            moveSelected ? selectedColor :
            turnManager.CanUse(APManager.MOVE_COST) ? normalColor : disabledColor);

        // 공격 버튼
        SetButtonColor(attackButton,
            attackSelected ? selectedColor :
            turnManager.CanUse(APManager.ATTACK_COST) ? normalColor : disabledColor);

        // 스킬 버튼
        SetButtonColor(skillButton,
            skillSelected ? selectedColor :
            turnManager.CanUse(APManager.SKILL_COST) ? normalColor : disabledColor);
    }

    void SetButtonColor(Button btn, Color color)
    {
        // 텍스트 버튼 색상 변경
        btn.GetComponent<Image>().color = color;
    }

    // 선택 초기화 (턴 넘어갈 때)
    public void ResetSelection()
    {
        moveSelected = false;
        attackSelected = false;
        skillSelected = false;
        HideAllButtons();
    }

    // 이미지 교체용 함수 (나중에 사용)
    public void SetMoveImage(Sprite sprite) => moveButtonImage.sprite = sprite;
    public void SetAttackImage(Sprite sprite) => attackButtonImage.sprite = sprite;
    public void SetSkillImage(Sprite sprite) => skillButtonImage.sprite = sprite;
}
