using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipPlacer : MonoBehaviour
{
    [Header("배치할 함선 목록")]
    public ShipController.ShipType[] shipTypes = new ShipController.ShipType[]
    {
        ShipController.ShipType.Battleship,   // 전함 5칸
        ShipController.ShipType.Carrier,      // 항공모함 5칸
        ShipController.ShipType.Cruiser,      // 순양함 4칸
        ShipController.ShipType.Destroyer,    // 구축함 3칸
        ShipController.ShipType.Submarine,    // 잠수함 3칸
        ShipController.ShipType.SpeedBoat,    // 고속정 2칸
    };

    [Header("함선 색상")]
    public Color shipColor = new Color(0.2f, 0.8f, 0.2f); // 초록색
    public Color previewColor = new Color(0.2f, 0.8f, 0.2f, 0.5f); // 반투명 초록

    private TileSelector tileSelector;
    private int currentShipIndex = 0;      // 현재 배치할 함선 인덱스
    private bool isHorizontal = true;      // 가로/세로 방향
    private List<GameObject> placedShips = new List<GameObject>(); // 배치된 함선들
    private GameObject previewShip;        // 미리보기 함선

    // 함선 크기 매핑
    private Dictionary<ShipController.ShipType, int> shipSizes
        = new Dictionary<ShipController.ShipType, int>()
    {
        { ShipController.ShipType.Battleship, 5 },
        { ShipController.ShipType.Carrier,    5 },
        { ShipController.ShipType.Cruiser,    4 },
        { ShipController.ShipType.Destroyer,  3 },
        { ShipController.ShipType.Submarine,  3 },
        { ShipController.ShipType.SpeedBoat,  2 },
    };

    void Start()
    {
        //바로 아랫줄 테스트
        Debug.Log("ShipPlacer 시작됨");
        tileSelector = FindObjectOfType<TileSelector>();
        CreatePreview();
    }

    void Update()
    {
        // 모든 함선 배치 완료
        if (currentShipIndex >= shipTypes.Length) return;

        // R키로 방향 전환
        if (Input.GetKeyDown(KeyCode.R))
        {
            isHorizontal = !isHorizontal;
            Debug.Log($"방향: {(isHorizontal ? "가로" : "세로")}");
        }

        // 미리보기 업데이트
        UpdatePreview();

        // 클릭시 배치
        if (Input.GetMouseButtonDown(0))
        {
            //바로 아랫줄 테스트
            Debug.Log("클릭됨");
            TryPlaceShip();
        }
    }

    // 미리보기 오브젝트 생성
    void CreatePreview()
    {
        if (currentShipIndex >= shipTypes.Length) return;

        if (previewShip != null) Destroy(previewShip);

        int size = shipSizes[shipTypes[currentShipIndex]];
        previewShip = CreateShipObject(size, previewColor, "Preview");
    }

    // 미리보기 위치 업데이트
    void UpdatePreview()
    {
        if (previewShip == null) return;

        Vector2Int coord = tileSelector.GetSelectedCoord();
        if (coord.x == -1) return;

        int size = shipSizes[shipTypes[currentShipIndex]];
        Vector3 pos = GetShipPosition(coord, size);
        Vector3 scale = GetShipScale(size);

        previewShip.transform.position = pos;
        previewShip.transform.localScale = scale;
    }

    // 함선 배치 시도
    void TryPlaceShip()
    {
        Vector2Int coord = tileSelector.GetSelectedCoord();
        Debug.Log($"좌표: {coord}");
        if (coord.x == -1) return;

        int size = shipSizes[shipTypes[currentShipIndex]];

        // 맵 범위 체크
        if (!IsValidPlacement(coord, size))
        {
            Debug.Log("배치 불가! 맵 범위를 벗어났습니다.");
            return;
        }

        // 함선 생성
        GameObject ship = CreateShipObject(size, shipColor,
            shipTypes[currentShipIndex].ToString());
        ship.transform.position = GetShipPosition(coord, size);
        ship.transform.localScale = GetShipScale(size);

        // ShipController 붙이기
        ShipController sc = ship.AddComponent<ShipController>();
        sc.shipType = shipTypes[currentShipIndex];

        placedShips.Add(ship);

        Debug.Log($"{shipTypes[currentShipIndex]} 배치완료! 좌표: ({coord.x}, {coord.y})");

        // 다음 함선으로
        currentShipIndex++;

        if (currentShipIndex >= shipTypes.Length)
        {
            Debug.Log("모든 함선 배치 완료!");
            if (previewShip != null) Destroy(previewShip);
            // TODO: 배치 완료 버튼 활성화
        }
        else
        {
            CreatePreview();
            Debug.Log($"다음 배치할 함선: {shipTypes[currentShipIndex]}");
        }
    }

    // 맵 범위 체크
    bool IsValidPlacement(Vector2Int coord, int size)
    {
        if (isHorizontal)
            return coord.x + size <= 30;
        else
            return coord.y + size <= 30;
    }

    // 함선 위치 계산
    Vector3 GetShipPosition(Vector2Int coord, int size)
    {
        if (isHorizontal)
            return new Vector3(coord.x + (size - 1) * 0.5f, 0.3f, coord.y);
        else
            return new Vector3(coord.x, 0.3f, coord.y + (size - 1) * 0.5f);
    }

    // 함선 크기 계산
    Vector3 GetShipScale(int size)
    {
        if (isHorizontal)
            return new Vector3(size, 0.3f, 0.8f);
        else
            return new Vector3(0.8f, 0.3f, size);
    }

    // 박스 오브젝트 생성
    GameObject CreateShipObject(int size, Color color, string shipName)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = shipName;
        obj.GetComponent<Renderer>().material.color = color;
        return obj;
    }
}

