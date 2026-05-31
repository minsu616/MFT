using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class VictoryManager : MonoBehaviour
{
    private BattleSetup battleSetup;
    private TurnManager turnManager;

    [Header("최후의 1대1 설정")]
    public int lastStandMaxTurns = 9; // 최후의 1대1 최대 턴
    private int lastStandTurnCount = 0; // 현재 1대1 턴 카운트
    private bool isLastStand = false;   // 1대1 상태인지

    void Start()
    {
        battleSetup = FindObjectOfType<BattleSetup>();
        turnManager = FindObjectOfType<TurnManager>();
    }

    // ──────────────────────────────────────────────
    // 수행 단계 끝날 때마다 호출
    // ──────────────────────────────────────────────
    public void CheckVictoryCondition()
    {
        List<GameObject> myShips = battleSetup.GetMyShips();
        List<GameObject> enemyShips = battleSetup.GetEnemyShips();

        // 살아있는 함선 수 계산
        int myAlive = CountAliveShips(myShips);
        int enemyAlive = CountAliveShips(enemyShips);

        Debug.Log($"내 함선: {myAlive}척 / 적 함선: {enemyAlive}척");

        // 승리 조건
        if (enemyAlive <= 0)
        {
            StartCoroutine(EndGame(true));
            return;
        }

        // 패배 조건
        if (myAlive <= 0)
        {
            StartCoroutine(EndGame(false));
            return;
        }

        // 최후의 1대1 조건
        if (myAlive == 1 && enemyAlive == 1)
        {
            if (!isLastStand)
            {
                isLastStand = true;
                lastStandTurnCount = 0;
                Debug.Log("최후의 1대1 시작! 9턴 카운트!");
            }
            else
            {
                lastStandTurnCount++;
                Debug.Log($"1대1 턴: {lastStandTurnCount} / {lastStandMaxTurns}");

                // 9턴 종료
                if (lastStandTurnCount >= lastStandMaxTurns)
                {
                    Debug.Log("9턴 종료! 체력 비교!");
                    CompareHP(myShips, enemyShips);
                }
            }
        }
        else
        {
            // 1대1 아닌 경우 초기화
            isLastStand = false;
            lastStandTurnCount = 0;
        }
    }

    // ──────────────────────────────────────────────
    // 살아있는 함선 수 계산
    // ──────────────────────────────────────────────
    int CountAliveShips(List<GameObject> ships)
    {
        int count = 0;
        foreach (GameObject ship in ships)
        {
            if (ship != null && ship.activeSelf)
                count++;
        }
        return count;
    }

    // ──────────────────────────────────────────────
    // 9턴 후 체력 비교
    // ──────────────────────────────────────────────
    void CompareHP(List<GameObject> myShips, List<GameObject> enemyShips)
    {
        int myHP = 0;
        int enemyHP = 0;

        foreach (GameObject ship in myShips)
        {
            if (ship != null && ship.activeSelf)
            {
                ShipController sc = ship.GetComponent<ShipController>();
                myHP += sc.GetData().CurrentHP;
            }
        }

        foreach (GameObject ship in enemyShips)
        {
            if (ship != null && ship.activeSelf)
            {
                ShipController sc = ship.GetComponent<ShipController>();
                enemyHP += sc.GetData().CurrentHP;
            }
        }

        Debug.Log($"내 HP: {myHP} / 적 HP: {enemyHP}");

        if (myHP >= enemyHP)
            StartCoroutine(EndGame(true));
        else
            StartCoroutine(EndGame(false));
    }

    // ──────────────────────────────────────────────
    // 게임 종료
    // ──────────────────────────────────────────────
    IEnumerator EndGame(bool isVictory)
    {
        if (isVictory)
        {
            Debug.Log("승리!");
            GameData.Instance.isVictory = true;
        }
        else
        {
            Debug.Log("패배!");
            GameData.Instance.isVictory = false;
        }

        yield return new WaitForSeconds(2f);

        // Result 씬으로 이동
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LoadLevel("Result");
        else
            SceneManager.LoadScene("Result");
    }
}