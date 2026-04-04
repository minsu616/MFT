using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 함선 1척의 명령 저장
[System.Serializable]
public class ShipCommand
{
    public GameObject ship;           // 명령받은 함선
    public Vector2Int moveTarget;     // 이동할 좌표
    public bool hasMoveCommand;       // 이동 명령 있는지
    public Vector2Int attackTarget;   // 공격할 좌표
    public bool hasAttackCommand;     // 공격 명령 있는지
}
