using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class PhotonShipSync : MonoBehaviourPunCallbacks, IOnEventCallback
{
    // 이벤트 코드
    const byte SHIP_PLACEMENT_EVENT = 1;
    const byte READY_EVENT = 2;

    private bool myReady = false;
    private bool enemyReady = false;

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
    }

    // 배치 완료 버튼 눌렀을 때 호출
    public void SendShipPlacement()
    {
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("싱글 테스트 모드 - 바로 Battle 씬으로 이동");
            UnityEngine.SceneManagement.SceneManager.LoadScene("Battle");
            return;
        }

        List<ShipInfo> myFleet = GameData.Instance.myFleet;

        int[] data = new int[myFleet.Count * 4];
        for (int i = 0; i < myFleet.Count; i++)
        {
            data[i * 4 + 0] = (int)myFleet[i].shipType;
            data[i * 4 + 1] = myFleet[i].coordinate.x;
            data[i * 4 + 2] = myFleet[i].coordinate.y;
            data[i * 4 + 3] = myFleet[i].isHorizontal ? 1 : 0;
        }

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };
        PhotonNetwork.RaiseEvent(SHIP_PLACEMENT_EVENT, data, options,
            SendOptions.SendReliable);

        Debug.Log("내 배치 데이터 전송 완료!");

        // myReady만 true, READY_EVENT 보내지 않음
        myReady = true;
        Debug.Log("상대방 배치 완료 기다리는 중...");
    }

    // Photon 이벤트 수신
    public void OnEvent(EventData photonEvent)
    {
        // 상대방 배치 데이터 수신
        if (photonEvent.Code == SHIP_PLACEMENT_EVENT)
        {
            int[] data = (int[])photonEvent.CustomData;

            GameData.Instance.enemyFleet.Clear();
            for (int i = 0; i < data.Length / 4; i++)
            {
                ShipInfo info = new ShipInfo();
                info.shipType = (ShipController.ShipType)data[i * 4 + 0];
                info.coordinate = new Vector2Int(data[i * 4 + 1], data[i * 4 + 2]);
                info.isHorizontal = data[i * 4 + 3] == 1;
                GameData.Instance.enemyFleet.Add(info);
            }

            Debug.Log("상대방 배치 데이터 수신 완료!");

            // 상대방 데이터 받으면 enemyReady = true
            enemyReady = true;
            CheckBothReady();
        }

        // 상대방 준비 완료 수신 (미사용)
        if (photonEvent.Code == READY_EVENT)
        {
            Debug.Log("READY_EVENT 수신 (미사용)");
        }
    }

    // 둘 다 준비됐으면 Battle 씬으로
    void CheckBothReady()
    {
        if (myReady && enemyReady)
        {
            Debug.Log("둘 다 준비 완료! Battle 씬으로 이동!");
            PhotonNetwork.LoadLevel("Battle");
        }
    }
}