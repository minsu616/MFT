using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI 버튼에 부착하면 클릭/호버 시 효과음이 재생됩니다.
/// Button 컴포넌트가 있는 오브젝트에 Add Component로 추가하세요.
/// 
/// [사용법]
/// 1. Button 오브젝트에 이 컴포넌트 추가
/// 2. 인스펙터에서 원하는 사운드 ID 선택
/// </summary>
[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler
{
    [Header("사운드 설정")]
    [SerializeField] private SoundID clickSound = SoundID.UI_Click;
    [SerializeField] private SoundID hoverSound = SoundID.UI_Hover;

    [Header("옵션")]
    [SerializeField] private bool playHoverSound = false;   // 호버 사운드 활성화 여부
    [SerializeField] private bool playOnlyIfInteractable = true;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (playOnlyIfInteractable && !_button.interactable) return;
        SoundManager.Instance?.PlaySFX(clickSound);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!playHoverSound) return;
        if (playOnlyIfInteractable && !_button.interactable) return;
        SoundManager.Instance?.PlaySFX(hoverSound);
    }
}


/// <summary>
/// Toggle, Dropdown 등 Button 이외의 UI에 사운드를 붙일 때 사용합니다.
/// (RequireComponent 없는 범용 버전)
/// </summary>
public class UIGenericSound : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private SoundID soundID = SoundID.UI_Click;

    public void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance?.PlaySFX(soundID);
    }

    /// <summary>코드에서 직접 호출할 때 사용 (예: UnityEvent 연결)</summary>
    public void PlaySound()
    {
        SoundManager.Instance?.PlaySFX(soundID);
    }

    public void PlaySound(SoundID id)
    {
        SoundManager.Instance?.PlaySFX(id);
    }
}