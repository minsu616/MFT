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
        { ShipController.ShipType.Cruiser,    4 },
        { ShipController.ShipType.Destroyer,  3 },
        { ShipController.ShipType.Submarine,  3 },
        { ShipController.ShipType.SpeedBoat,  2 },
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
        // 테스트용 임시 배치 (상대방 배치 데이터가 없으니 맵 반대편에 고정 배치)
        int[] sizes = { 5, 5, 4, 3, 3, 2 };
        ShipController.ShipType[] types = {
            ShipController.ShipType.Battleship,
            ShipController.ShipType.Carrier,
            ShipController.ShipType.Cruiser,
            ShipController.ShipType.Destroyer,
            ShipController.ShipType.Submarine,
            ShipController.ShipType.SpeedBoat
        };

        for (int i = 0; i < types.Length; i++)
        {
            ShipInfo info = new ShipInfo();
            info.shipType = types[i];
            info.coordinate = new Vector2Int(20, i * 4); // 맵 반대편 고정 배치
            info.isHorizontal = true;

            GameObject ship = CreateShip(info, enemyShipColor, "Enemy_" + types[i].ToString());

            // 상대 배 안보이게
            ship.GetComponent<Renderer>().enabled = false;

            enemyShips.Add(ship);
        }
        Debug.Log($"상대 함선 {enemyShips.Count}척 생성완료! (숨김 상태)");
    }

    // 함선 오브젝트 생성
    GameObject CreateShip(ShipInfo info, Color color, string shipName)
    {
        int size = shipSizes[info.shipType];

        GameObject ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ship.name = shipName;
        ship.GetComponent<Renderer>().material.color = color;

        // 위치
        if (info.isHorizontal)
        {
            ship.transform.position = new Vector3(
                info.coordinate.x + (size - 1) * 0.5f, 0.3f, info.coordinate.y);
            ship.transform.localScale = new Vector3(size, 0.3f, 0.8f);
        }
        else
        {
            ship.transform.position = new Vector3(
                info.coordinate.x, 0.3f, info.coordinate.y + (size - 1) * 0.5f);
            ship.transform.localScale = new Vector3(0.8f, 0.3f, size);
        }

        // ShipController 붙이기
        ShipController sc = ship.AddComponent<ShipController>();
        sc.shipType = info.shipType;

        return ship;
    }

    // 외부에서 함선 목록 가져올 때
    public List<GameObject> GetMyShips() => myShips;
    public List<GameObject> GetEnemyShips() => enemyShips;
}
