using System.Collections.Generic;
using UnityEngine;
using static TurnManager;

public class FogOfWar : MonoBehaviour
{
    private float updateInterval = 0.1f; // 0.1초마다 업데이트
    private float lastUpdateTime = 0f;

    private BattleSetup battleSetup;
    private TurnManager turnManager;

    void Start()
    {
        battleSetup = FindObjectOfType<BattleSetup>();
        turnManager = FindObjectOfType<TurnManager>();
        
    }

    void Update()
    {
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            lastUpdateTime = Time.time;
            UpdateFogOfWar();
        }
    }

    public void ForceUpdate()
    {
        UpdateFogOfWar();
    }

    void UpdateFogOfWar()
    {
        if (battleSetup == null) return;

        List<GameObject> myShips = battleSetup.GetMyShips();
        List<GameObject> enemyShips = battleSetup.GetEnemyShips();

        if (myShips == null || enemyShips == null) return;

        foreach (GameObject enemy in enemyShips)
        {
            // null 체크 강화
            if (enemy == null) continue;

            bool detected = false;

            foreach (GameObject myShip in myShips)
            {
                // null 체크 강화
                if (myShip == null) continue;
                if (!myShip.activeSelf) continue;

                ShipController sc = myShip.GetComponent<ShipController>();
                if (sc == null) continue; // ShipController null 체크

                ShipData data = sc.GetData();
                if (data == null) continue; // ShipData null 체크

                int detectRange = data.DetectRange;

                Vector2Int myCoord = GetShipCenterCoord(myShip);
                Vector2Int enemyCoord = GetShipCenterCoord(enemy);

                int distX = Mathf.Abs(enemyCoord.x - myCoord.x);
                int distZ = Mathf.Abs(enemyCoord.y - myCoord.y);

                if (distX <= detectRange && distZ <= detectRange)
                {
                    detected = true;
                    break;
                }
            }

            // enemy 활성화 여부 상관없이 자식 셀 체크
            // 탐지됐으면 보이게, 아니면 숨기기
            foreach (Transform cell in enemy.transform)
            {
                if (cell == null) continue;

                // HPBar는 따로 처리
                if (cell.name == "HPBar")
                {
                    cell.gameObject.SetActive(detected);
                    continue;
                }

                Renderer rend = cell.GetComponent<Renderer>();
                if (rend != null)
                    rend.enabled = detected;
            }
        }
    }

    Vector2Int GetShipCenterCoord(GameObject ship)
    {
        if (ship == null) return Vector2Int.zero;

        ShipController sc = ship.GetComponent<ShipController>();
        if (sc == null) return Vector2Int.zero; // null 체크

        ShipData data = sc.GetData();
        if (data == null) return Vector2Int.zero; // null 체크

        int size = data.Size;
        int centerIndex = (size - 1) / 2;

        if (ship.transform.childCount <= centerIndex)
            return new Vector2Int(
                Mathf.RoundToInt(ship.transform.position.x),
                Mathf.RoundToInt(ship.transform.position.z));

        Transform centerCell = ship.transform.GetChild(centerIndex);
        if (centerCell == null) return Vector2Int.zero; // null 체크

        return new Vector2Int(
            Mathf.RoundToInt(centerCell.position.x),
            Mathf.RoundToInt(centerCell.position.z));
    }
}