using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class PhotonBattleSync : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // 이벤트 코드
    const byte READY_EVENT = 10;        // 명령 완료
    const byte MOVE_EVENT = 11;         // 이동 데이터
    const byte ATTACK_EVENT = 12;       // 공격 데이터

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

    // 명령 완료 버튼 눌렀을 때 호출
    public void SendReady()
    {
        // 싱글 테스트면 그냥 다음 단계로
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

    // 이동 명령 상대방에게 전송
    public void SendMoveData(List<ShipMoveData> moveDataList)
    {
        if (!PhotonNetwork.InRoom) return;

        // 직렬화: [함선이름길이, 함선이름, x, z, isHorizontal]
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

    // 공격 데이터 상대방에게 전송
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
                string shipName = "My_" + names[i]
                    .Replace("Enemy_", ""); // 상대방의 My_ = 내 화면의 Enemy_

                int targetX = data[i * 4 + 0];
                int targetZ = data[i * 4 + 1];
                bool isHorizontal = data[i * 4 + 2] == 1;

                // 적 함선 이동
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

            // 내 함선 중 해당 좌표에 있는 함선에 데미지
            ApplyDamageToMyShip(attackX, attackZ, damage);
            Debug.Log($"상대방 공격 수신! 좌표:({attackX},{attackZ}) 데미지:{damage}");
        }
    }

    // ──────────────────────────────────────────────
    // 적 함선 이동 적용
    // ──────────────────────────────────────────────
    void MoveEnemyShip(string shipName, int targetX, int targetZ, bool isHorizontal)
    {
        GameObject enemyShip = GameObject.Find(shipName);
        if (enemyShip == null)
        {
            Debug.Log($"적 함선 못찾음: {shipName}");
            return;
        }

        ShipController sc = enemyShip.GetComponent<ShipController>();
        int size = sc.GetData().Size;

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

        Debug.Log($"{shipName} 이동 완료! → ({targetX},{targetZ})");
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

            // 공격 좌표 근처에 내 함선 있으면 데미지
            int distX = Mathf.Abs(shipX - attackX);
            int distZ = Mathf.Abs(shipZ - attackZ);

            if (distX <= size && distZ <= size)
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