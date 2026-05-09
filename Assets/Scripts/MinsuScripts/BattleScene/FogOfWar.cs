using UnityEngine;
using System.Collections.Generic;

public class FogOfWar : MonoBehaviour
{
    private BattleSetup battleSetup;
    private TurnManager turnManager;

    void Start()
    {
        battleSetup = FindObjectOfType<BattleSetup>();
        turnManager = FindObjectOfType<TurnManager>();
    }

    void Update()
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
            foreach (Transform cell in enemy.transform)
            {
                if (cell == null) continue;
                Renderer rend = cell.GetComponent<Renderer>();
                if (rend != null)
                    rend.enabled = detected;
            }
        }
    }

    Vector2Int GetShipCenterCoord(GameObject ship)
    {
        ShipController sc = ship.GetComponent<ShipController>();
        int size = sc.GetData().Size;
        int centerIndex = (size - 1) / 2;

        Transform centerCell = ship.transform.GetChild(centerIndex);
        return new Vector2Int(
            Mathf.RoundToInt(centerCell.position.x),
            Mathf.RoundToInt(centerCell.position.z));
    }
}