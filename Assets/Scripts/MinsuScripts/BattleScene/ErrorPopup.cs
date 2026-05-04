using UnityEngine;
using TMPro;
using System.Collections;

public class ErrorPopup : MonoBehaviour
{
    public TextMeshProUGUI errorText;
    public float displayTime = 1.5f; // 표시 시간

    private Coroutine hideCoroutine;

    void Start()
    {
        errorText.gameObject.SetActive(false);
    }

    public void ShowError()
    {
        errorText.gameObject.SetActive(true);

        // 이미 실행 중이면 취소하고 다시 시작
        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(displayTime);
        errorText.gameObject.SetActive(false);
    }
}
