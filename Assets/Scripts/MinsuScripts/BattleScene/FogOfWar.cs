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
        List<GameObject> myShips = battleSetup.GetMyShips();
        List<GameObject> enemyShips = battleSetup.GetEnemyShips();

        foreach (GameObject enemy in enemyShips)
        {
            if (enemy == null || !enemy.activeSelf) continue;

            bool detected = false;

            // 내 함선 중 하나라도 탐지 범위 안에 적이 있으면 보이게
            foreach (GameObject myShip in myShips)
            {
                if (myShip == null || !myShip.activeSelf) continue;

                ShipController sc = myShip.GetComponent<ShipController>();
                int detectRange = sc.GetData().DetectRange;

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

            // 탐지됐으면 보이게, 아니면 숨기기
            foreach (Transform cell in enemy.transform)
            {
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