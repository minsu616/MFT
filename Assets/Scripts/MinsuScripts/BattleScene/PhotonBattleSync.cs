using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections;
using System.Collections.Generic;

public class PhotonBattleSync : MonoBehaviourPunCallbacks, IOnEventCallback
{
    const byte READY_EVENT = 10;
    const byte MOVE_EVENT = 11;
    const byte ATTACK_EVENT = 12;

    private TurnManager turnManager;
    private ShipSelector shipSelector;
    private AttackSystem attackSystem;
    private BattleSetup battleSetup;

    private bool myReady = false;
    private bool enemyReady = false;

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        shipSelector = FindObjectOfType<ShipSelector>();
        attackSystem = FindObjectOfType<AttackSystem>();
        battleSetup = FindObjectOfType<BattleSetup>();
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // ──────────────────────────────────────────────
    // 1. 턴 동기화
    // ──────────────────────────────────────────────
    public void SendReady()
    {
        if (!PhotonNetwork.InRoom)
        {
            turnManager.EndCommandPhase();
            return;
        }

        myReady = true;
        Debug.Log("내 명령 완료 전송!");

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };
        PhotonNetwork.RaiseEvent(READY_EVENT, null, options,
            SendOptions.SendReliable);

        CheckBothReady();
    }

    void CheckBothReady()
    {
        if (myReady && enemyReady)
        {
            myReady = false;
            enemyReady = false;
            Debug.Log("둘 다 준비 완료! 이동 단계 시작!");
            turnManager.EndCommandPhase();
        }
    }

    // ──────────────────────────────────────────────
    // 2. 이동 동기화
    // ──────────────────────────────────────────────
    public void SendMoveData(List<ShipMoveData> moveDataList)
    {
        if (!PhotonNetwork.InRoom) return;

        int[] data = new int[moveDataList.Count * 4];
        string[] names = new string[moveDataList.Count];

        for (int i = 0; i < moveDataList.Count; i++)
        {
            names[i] = moveDataList[i].shipName;
            data[i * 4 + 0] = moveDataList[i].targetX;
            data[i * 4 + 1] = moveDataList[i].targetZ;
            data[i * 4 + 2] = moveDataList[i].isHorizontal ? 1 : 0;
            data[i * 4 + 3] = i;
        }

        object[] sendData = new object[] { names, data };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };
        PhotonNetwork.RaiseEvent(MOVE_EVENT, sendData, options,
            SendOptions.SendReliable);

        Debug.Log("이동 데이터 전송 완료!");
    }

    // ──────────────────────────────────────────────
    // 3. 공격 동기화
    // ──────────────────────────────────────────────
    public void SendAttackData(string attackerName, int attackX, int attackZ, int damage)
    {
        if (!PhotonNetwork.InRoom) return;

        object[] data = new object[] { attackerName, attackX, attackZ, damage };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };
        PhotonNetwork.RaiseEvent(ATTACK_EVENT, data, options,
            SendOptions.SendReliable);

        Debug.Log($"공격 데이터 전송! {attackerName} → ({attackX},{attackZ})");
    }

    // ──────────────────────────────────────────────
    // 이벤트 수신
    // ──────────────────────────────────────────────
    public void OnEvent(EventData photonEvent)
    {
        // 상대방 명령 완료 수신
        if (photonEvent.Code == READY_EVENT)
        {
            enemyReady = true;
            Debug.Log("상대방 명령 완료!");
            CheckBothReady();
        }

        // 상대방 이동 데이터 수신
        if (photonEvent.Code == MOVE_EVENT)
        {
            object[] received = (object[])photonEvent.CustomData;
            string[] names = (string[])received[0];
            int[] data = (int[])received[1];

            for (int i = 0; i < names.Length; i++)
            {
                string shipName = names[i].Replace("My_", "Enemy_");
                int targetX = data[i * 4 + 0];
                int targetZ = data[i * 4 + 1];
                bool isHorizontal = data[i * 4 + 2] == 1;

                MoveEnemyShip(shipName, targetX, targetZ, isHorizontal);
            }
            Debug.Log("상대방 이동 데이터 수신!");
        }

        // 상대방 공격 데이터 수신
        if (photonEvent.Code == ATTACK_EVENT)
        {
            object[] data = (object[])photonEvent.CustomData;
            string attackerName = (string)data[0];
            int attackX = (int)data[1];
            int attackZ = (int)data[2];
            int damage = (int)data[3];

            Debug.Log($"상대방 공격 수신! 공격함선:{attackerName} 좌표:({attackX},{attackZ}) 데미지:{damage}");

            // attackerName 추가
            StartCoroutine(ReceiveAttack(attackerName, attackX, attackZ, damage));
        }
    }

    // ──────────────────────────────────────────────
    // 공격 수신 처리
    // ──────────────────────────────────────────────
    IEnumerator ReceiveAttack(string attackerName, int attackX, int attackZ, int damage)
    {
        // 함선 이름으로 정확한 적 함선 찾기
        string enemyShipName = attackerName.Replace("My_", "Enemy_");
        GameObject attacker = GameObject.Find(enemyShipName);

        if (attacker != null)
        {
            ShipController sc = attacker.GetComponent<ShipController>();
            Vector3 spawnPos = new Vector3(
                attacker.transform.position.x, 1.5f,
                attacker.transform.position.z);
            Vector3 targetPos = new Vector3(attackX, 0f, attackZ);

            GameObject missileObj = MissileFactory.CreateMissile(sc.shipType);
            missileObj.transform.position = spawnPos;

            Missile missile = missileObj.GetComponent<Missile>();
            missile.damage = damage;
            missile.attackCount = 1;

            missile.OnArrived += (m) =>
            {
                ApplyDamageToMyShip(attackX, attackZ, damage);
            };

            missile.Launch(spawnPos, targetPos);
        }
        else
        {
            Debug.Log($"공격한 적 함선 못찾음: {enemyShipName} 바로 데미지 적용");
            ApplyDamageToMyShip(attackX, attackZ, damage);
        }

        yield return null;
    }

    // ──────────────────────────────────────────────
    // 적 함선 이동
    // ──────────────────────────────────────────────
    void MoveEnemyShip(string shipName, int targetX, int targetZ, bool isHorizontal)
    {
        GameObject enemyShip = GameObject.Find(shipName);
        if (enemyShip == null)
        {
            Debug.Log($"적 함선 못찾음: {shipName}");
            return;
        }

        StartCoroutine(MoveEnemyShipCoroutine(enemyShip, targetX, targetZ, isHorizontal));
    }

    IEnumerator MoveEnemyShipCoroutine(GameObject enemyShip, int targetX, int targetZ, bool isHorizontal)
    {
        WaitForSeconds wait = new WaitForSeconds(0.5f);

        ShipController sc = enemyShip.GetComponent<ShipController>();
        int size = sc.GetData().Size;
        int centerIndex = (size - 1) / 2;

        Transform centerCell = enemyShip.transform.GetChild(centerIndex);
        int startX = Mathf.RoundToInt(centerCell.position.x);
        int startZ = Mathf.RoundToInt(centerCell.position.z);
        int endX = targetX + centerIndex;
        int endZ = targetZ;

        // X축 이동
        int stepX = endX > startX ? 1 : -1;
        for (int x = startX; x != endX; x += stepX)
        {
            enemyShip.transform.position += new Vector3(stepX, 0, 0);

            for (int i = 0; i < enemyShip.transform.childCount; i++)
            {
                Transform cell = enemyShip.transform.GetChild(i);
                if (cell.name == "HPBar") continue;
                if (isHorizontal)
                    cell.position = new Vector3(
                        enemyShip.transform.position.x + i, 0.3f,
                        enemyShip.transform.position.z);
                else
                    cell.position = new Vector3(
                        enemyShip.transform.position.x, 0.3f,
                        enemyShip.transform.position.z + i);
            }
            yield return wait;
        }

        // Z축 이동
        int stepZ = endZ > startZ ? 1 : -1;
        for (int z = startZ; z != endZ; z += stepZ)
        {
            enemyShip.transform.position += new Vector3(0, 0, stepZ);

            for (int i = 0; i < enemyShip.transform.childCount; i++)
            {
                Transform cell = enemyShip.transform.GetChild(i);
                if (cell.name == "HPBar") continue;
                if (isHorizontal)
                    cell.position = new Vector3(
                        enemyShip.transform.position.x + i, 0.3f,
                        enemyShip.transform.position.z);
                else
                    cell.position = new Vector3(
                        enemyShip.transform.position.x, 0.3f,
                        enemyShip.transform.position.z + i);
            }
            yield return wait;
        }

        // 최종 위치 확정
        enemyShip.transform.position = new Vector3(targetX, 0.3f, targetZ);
        for (int i = 0; i < enemyShip.transform.childCount; i++)
        {
            Transform cell = enemyShip.transform.GetChild(i);
            if (cell.name == "HPBar") continue;
            if (isHorizontal)
                cell.position = new Vector3(targetX + i, 0.3f, targetZ);
            else
                cell.position = new Vector3(targetX, 0.3f, targetZ + i);
        }

        Debug.Log($"{enemyShip.name} 이동 완료! → ({targetX},{targetZ})");
    }

    // ──────────────────────────────────────────────
    // 내 함선에 데미지 적용
    // ──────────────────────────────────────────────
    void ApplyDamageToMyShip(int attackX, int attackZ, int damage)
    {
        List<GameObject> myShips = battleSetup.GetMyShips();

        foreach (GameObject ship in myShips)
        {
            if (ship == null || !ship.activeSelf) continue;

            ShipController sc = ship.GetComponent<ShipController>();
            int size = sc.GetData().Size;
            int centerIndex = (size - 1) / 2;

            Transform centerCell = ship.transform.GetChild(centerIndex);
            int shipX = Mathf.RoundToInt(centerCell.position.x);
            int shipZ = Mathf.RoundToInt(centerCell.position.z);

            int distX = Mathf.Abs(shipX - attackX);
            int distZ = Mathf.Abs(shipZ - attackZ);

            if (distX <= 1 && distZ <= 1)
            {
                sc.TakeDamage(damage);
                Debug.Log($"내 {sc.GetData().ShipName} 피격! 데미지:{damage} 남은HP:{sc.GetData().CurrentHP}");
                break;
            }
        }
    }
}

// 이동 데이터 구조체
[System.Serializable]
public class ShipMoveData
{
    public string shipName;
    public int targetX;
    public int targetZ;
    public bool isHorizontal;
}