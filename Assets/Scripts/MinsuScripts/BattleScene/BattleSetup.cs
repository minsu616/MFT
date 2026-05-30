using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BattleSetup : MonoBehaviour
{
    [Header("함선 색상")]
    public Color myShipColor = new Color(0.2f, 0.8f, 0.2f);      // 내 배 초록색
    public Color enemyShipColor = new Color(0.8f, 0.2f, 0.2f);   // 상대 배 빨간색

    private List<GameObject> myShips = new List<GameObject>();
    private List<GameObject> enemyShips = new List<GameObject>();

    [Header("폰트")]
    public TMP_FontAsset koreanFont;



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

        StartCoroutine(InitFogOfWar());
    }

    IEnumerator InitFogOfWar()
    {
        yield return null;
        FogOfWar fogOfWar = FindObjectOfType<FogOfWar>();
        if (fogOfWar != null)
            fogOfWar.ForceUpdate();
    }

    //테스트용 적 함선 생성 코드 start
    void Update()
    {
        // T키 누르면 테스트용 적 함선 1척 생성
        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnTestEnemy();
        }
    }
    void SpawnTestEnemy()
    {
        // 이미 테스트 적 있으면 삭제 후 재생성
        GameObject existing = GameObject.Find("Enemy_Test");
        if (existing != null) Destroy(existing);

        ShipInfo info = new ShipInfo();
        info.shipType = ShipController.ShipType.SpeedBoat; // 1칸짜리 고속정
        info.coordinate = new Vector2Int(15, 15);           // 맵 중앙
        info.isHorizontal = true;

        GameObject testEnemy = CreateShip(info, enemyShipColor, "Enemy_Test");

        // 처음엔 숨기기 (Fog of War 테스트용)
        foreach (Transform cell in testEnemy.transform)
        {
            if (cell.name == "HPBar")
            {
                cell.gameObject.SetActive(false);
                continue;
            }
            Renderer rend = cell.GetComponent<Renderer>();
            if (rend != null) rend.enabled = false;
        }

        enemyShips.Add(testEnemy);
        Debug.Log("테스트 적 함선 생성! 좌표: (15, 15)");
    }
    //테스트용 함선 생성코드 end


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

        if (shipName.StartsWith("My_")) //내 함선 HP바
        {
            GameObject hpBarObj = new GameObject("HPBar");
            hpBarObj.transform.parent = shipParent.transform;
            HPBar hpBarComp = hpBarObj.AddComponent<HPBar>();
            hpBarComp.koreanFont = koreanFont;
        }

        if (shipName.StartsWith("Enemy_")) //적 함선 HP바
        {
            GameObject hpBarObj = new GameObject("HPBar");
            hpBarObj.transform.parent = shipParent.transform;
            HPBar hpBarComp = hpBarObj.AddComponent<HPBar>();
            hpBarComp.koreanFont = koreanFont;
            // 처음엔 숨기기 (Fog of War)
            hpBarObj.SetActive(false);
        }

        return shipParent;
    }

    // 외부에서 함선 목록 가져올 때
    public List<GameObject> GetMyShips() => myShips;
    public List<GameObject> GetEnemyShips() => enemyShips;

}
