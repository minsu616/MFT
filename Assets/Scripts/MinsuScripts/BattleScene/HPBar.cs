using UnityEngine;
using TMPro;

public class HPBar : MonoBehaviour
{
    [Header("HP바 설정")]
    public float barHeight = 0.1f;    // HP바 높이
    public float heightOffset = 0.5f; // 함선 위 높이

    private GameObject bgBar;
    private GameObject hpBar;
    private ShipController shipController;
    private Camera mainCamera;
    private float barWidth;           // 함선 크기에 따라 자동 설정

    [Header("폰트")]
    public TMP_FontAsset koreanFont;

    void Start()
    {
        shipController = GetComponentInParent<ShipController>();
        mainCamera = Camera.main;

        // 함선 크기만큼 바 너비 설정
        barWidth = shipController.GetData().Size;

        CreateHPBar();
    }

    void CreateHPBar()
    {
        GameObject nameObj = new GameObject("ShipNameText");
        nameObj.transform.parent = this.transform;

        TextMeshPro tmp = nameObj.AddComponent<TextMeshPro>();
        
        if (koreanFont != null)
            tmp.font = koreanFont;

        tmp.text = shipController.GetData().ShipName;
        tmp.fontSize = 2f;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Right;
        tmp.rectTransform.sizeDelta = new Vector2(3f, 0.5f);

        // HP바 왼쪽에 배치
        nameObj.transform.localPosition = new Vector3(
            -(barWidth / 2) - 1.8f, 0, -0.01f);

        // 배경 바 (회색)
        bgBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bgBar.name = "HPBar_BG";
        bgBar.transform.parent = this.transform;
        bgBar.transform.localScale = new Vector3(barWidth, barHeight, 0.05f);
        bgBar.GetComponent<Renderer>().material.color = new Color(0.3f, 0.3f, 0.3f);
        Destroy(bgBar.GetComponent<BoxCollider>());

        // HP 바
        hpBar = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hpBar.name = "HPBar_HP";
        hpBar.transform.parent = this.transform;
        hpBar.transform.localScale = new Vector3(barWidth, barHeight, 0.06f);
        hpBar.GetComponent<Renderer>().material.color = Color.green;
        Destroy(hpBar.GetComponent<BoxCollider>());
    }

    void Update()
    {
        if (shipController == null || shipController.GetData() == null) return;

        // 항상 카메라를 향하도록 회전
        transform.LookAt(transform.position + mainCamera.transform.forward);

        // 함선 위에 위치
        transform.position = GetShipTopPosition();

        UpdateHPBar();
    }

    void UpdateHPBar()
    {
        ShipData data = shipController.GetData();
        float ratio = (float)data.CurrentHP / data.MaxHP;
        ratio = Mathf.Clamp01(ratio);

        // HP바 크기 조정
        hpBar.transform.localScale = new Vector3(
            barWidth * ratio, barHeight, 0.06f);
        hpBar.transform.localPosition = new Vector3(
            -(barWidth * (1 - ratio)) / 2, 0, -0.01f);

        // 색상 변경 (녹색 → 노란색 → 빨간색)
        hpBar.GetComponent<Renderer>().material.color = Color.Lerp(
            Color.red, Color.green, ratio);

        // HP 0이면 숨기기
        if (data.CurrentHP <= 0)
        {
            bgBar.SetActive(false);
            hpBar.SetActive(false);
        }
    }

    Vector3 GetShipTopPosition()
    {
        ShipController sc = shipController;
        int size = sc.GetData().Size;
        int centerIndex = (size - 1) / 2;

        // 함선 중앙 셀 기준으로 위치 설정
        Transform shipParent = transform.parent;
        if (shipParent != null && shipParent.childCount > centerIndex)
        {
            Transform centerCell = shipParent.GetChild(centerIndex);
            return new Vector3(
                centerCell.position.x,
                centerCell.position.y + heightOffset,
                centerCell.position.z);
        }

        return transform.parent.position + Vector3.up * heightOffset;
    }
}