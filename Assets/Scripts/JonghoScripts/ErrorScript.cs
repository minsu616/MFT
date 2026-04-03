using UnityEngine;
using System.Collections;

public class ErrorPopupManager : MonoBehaviour
{
    // 어디서든 접근할 수 있도록 싱글톤 인스턴스 생성
    public static ErrorPopupManager Instance;

    [Header("에러 이미지 오브젝트를 연결하세요")]
    public GameObject errorImage;

    private void Awake()
    {
        Instance = this;
        errorImage.SetActive(false); // 게임 시작 시 이미지는 숨김 처리
    }

    // 다른 스크립트에서 호출할 간결한 함수
    public static void ShowError()
    {
        if (Instance != null)
        {
            Instance.StopAllCoroutines(); // 이미지가 켜져있는 상태에서 또 에러가 나면 시간 초기화
            Instance.StartCoroutine(Instance.ShowAndHideRoutine());
        }
    }

    private IEnumerator ShowAndHideRoutine()
    {
        errorImage.SetActive(true);            // 이미지 켜기
        yield return new WaitForSeconds(2.0f); // 2초 대기 (원하는 시간으로 수정 가능)
        errorImage.SetActive(false);           // 이미지 끄기
    }
}