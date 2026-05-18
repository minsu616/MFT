using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public TextMeshProUGUI statusText;
    public Button createRoomButton;
    public Button joinRoomButton;

    void Start()
    {
        // 서버 연결
        if (!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
            statusText.text = "서버 연결 중...";
        }
        else
        {
            statusText.text = "서버 연결됨!";
        }

        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);

        // 처음엔 버튼 비활성화
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
    }

    // 서버 연결 성공
    public override void OnConnectedToMaster()
    {
        statusText.text = "서버 연결됨!";
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        Debug.Log("서버 연결 성공!");
    }

    // 방 만들기
    void CreateRoom()
    {
        statusText.text = "방 생성 중...";
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;

        RoomOptions options = new RoomOptions();
        options.MaxPlayers = 2; // 최대 2명
        PhotonNetwork.CreateRoom("MTFRoom", options);
    }

    // 방 참가
    void JoinRoom()
    {
        statusText.text = "방 참가 중...";
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;

        PhotonNetwork.JoinRoom("MTFRoom");
    }

    // 방 생성 성공
    public override void OnCreatedRoom()
    {
        statusText.text = "방 생성 완료! 상대방 기다리는 중...";
        Debug.Log("방 생성 완료!");
    }

    // 방 참가 성공
    public override void OnJoinedRoom()
    {
        statusText.text = $"방 참가 완료! ({PhotonNetwork.CurrentRoom.PlayerCount}/2)";
        Debug.Log($"방 참가! 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // 2명 모이면 ShipPlacement로 이동
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            statusText.text = "2명 모임! 게임 시작!";
            // 방장만 씬 이동 (다른 플레이어는 자동으로 따라옴)
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("ShipPlacement");
            }
        }
    }

    // 다른 플레이어 입장
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        statusText.text = $"상대방 입장! ({PhotonNetwork.CurrentRoom.PlayerCount}/2)";
        Debug.Log($"플레이어 입장! 현재 인원: {PhotonNetwork.CurrentRoom.PlayerCount}");

        // 2명 모이면 ShipPlacement로 이동
        if (PhotonNetwork.CurrentRoom.PlayerCount == 2)
        {
            statusText.text = "2명 모임! 게임 시작!";
            if (PhotonNetwork.IsMasterClient)
            {
                PhotonNetwork.LoadLevel("ShipPlacement");
            }
        }
    }

    // 방 생성 실패
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        statusText.text = "방 생성 실패! 다시 시도해주세요.";
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        Debug.Log($"방 생성 실패: {message}");
    }

    // 방 참가 실패
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        statusText.text = "방 참가 실패! 방이 없거나 꽉 찼어요.";
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        Debug.Log($"방 참가 실패: {message}");
    }
}