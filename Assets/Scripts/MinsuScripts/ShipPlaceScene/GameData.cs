// GameData.cs
using UnityEngine;
using System.Collections.Generic;

// 함선 1척의 정보
[System.Serializable]
public class ShipInfo
{
    public ShipController.ShipType shipType;  // 함선 종류
    public Vector2Int coordinate;             // 배치 좌표
    public bool isHorizontal;                 // 가로/세로
}

public class GameData : MonoBehaviour
{
    public static GameData Instance;

    public List<ShipInfo> myFleet = new List<ShipInfo>(); // 내 함선 데이터
    public List<ShipInfo> enemyFleet = new List<ShipInfo>();//적 함선 데이터

    public bool isVictory = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 씬 바뀌어도 안사라짐
        }
        else Destroy(gameObject);
    }

    // 함선 추가
    public void AddShip(ShipController.ShipType type, Vector2Int coord, bool horizontal)
    {
        ShipInfo info = new ShipInfo();
        info.shipType = type;
        info.coordinate = coord;
        info.isHorizontal = horizontal;
        myFleet.Add(info);
    }

    // 데이터 초기화 (게임 재시작 시)
    public void ClearFleet()
    {
        myFleet.Clear();
        enemyFleet.Clear();
    }
}
