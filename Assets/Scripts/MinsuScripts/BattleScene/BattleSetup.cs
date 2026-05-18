using UnityEngine;
using System.Collections.Generic;

public class BattleSetup : MonoBehaviour
{
    [Header("함선 색상")]
    public Color myShipColor = new Color(0.2f, 0.8f, 0.2f);      // 내 배 초록색
    public Color enemyShipColor = new Color(0.8f, 0.2f, 0.2f);   // 상대 배 빨간색

    private List<GameObject> myShips = new List<GameObject>();
    private List<GameObject> enemyShips = new List<GameObject>();



    // 함선 크기 매핑
    private Dictionary<ShipController.ShipType, int> shipSizes
        = new Dictionary<ShipController.ShipType, int>()
    {
        { ShipController.ShipType.Battleship, 5 },
        { ShipController.ShipType.Carrier,    5 },
        { ShipController.ShipType.Cruiser,    3 },
        { ShipController.ShipType.Destroyer,  3 },
        { ShipController.ShipType.Submarine,  3 },
        { ShipController.ShipType.SpeedBoat,  1 },
    };

    void Start()
    {
        if (GameData.Instance == null)
        {
            Debug.Log("GameData 없음! ShipPlacement 씬부터 시작해주세요.");
            return;
        }

        SpawnMyFleet();
        SpawnEnemyFleet();
    }


    // 내 함선 생성
    void SpawnMyFleet()
    {
        foreach (ShipInfo info in GameData.Instance.myFleet)
        {
            GameObject ship = CreateShip(info, myShipColor, "My_" + info.shipType.ToString());
            myShips.Add(ship);
        }
        Debug.Log($"내 함선 {myShips.Count}척 생성완료!");
    }

    // 상대 함선 생성 (나중에 Photon으로 받아올 예정, 지금은 테스트용 임시 배치)
    void SpawnEnemyFleet()
    {
        if (GameData.Instance.enemyFleet.Count == 0)
        {
            Debug.Log("상대방 배치 데이터 없음!");
            return;
        }

        foreach (ShipInfo info in GameData.Instance.enemyFleet)
        {
            GameObject ship = CreateShip(info, enemyShipColor,
                "Enemy_" + info.shipType.ToString());

            // 상대 배 숨기기
            foreach (Transform cell in ship.transform)
                cell.GetComponent<Renderer>().enabled = false;

            enemyShips.Add(ship);
        }
        Debug.Log($"상대 함선 {enemyShips.Count}척 생성완료!");
    }

    // 함선 오브젝트 생성
    GameObject CreateShip(ShipInfo info, Color color, string shipName)
    {
        int size = shipSizes[info.shipType];

        // 부모 오브젝트
        GameObject shipParent = new GameObject(shipName);
        shipParent.transform.position = new Vector3(info.coordinate.x, 0.3f, info.coordinate.y);

        // 칸마다 박스 생성
        for (int i = 0; i < size; i++)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.name = shipName + $"_cell{i}";
            cell.GetComponent<Renderer>().material.color = color;
            cell.transform.parent = shipParent.transform;
            cell.transform.localScale = new Vector3(0.9f, 0.3f, 0.9f);

            if (info.isHorizontal)
                cell.transform.position = new Vector3(info.coordinate.x + i, 0.3f, info.coordinate.y);
            else
                cell.transform.position = new Vector3(info.coordinate.x, 0.3f, info.coordinate.y + i);
        }

        ShipController sc = shipParent.AddComponent<ShipController>();
        sc.shipType = info.shipType;

        return shipParent;
    }

    // 외부에서 함선 목록 가져올 때
    public List<GameObject> GetMyShips() => myShips;
    public List<GameObject> GetEnemyShips() => enemyShips;

}
