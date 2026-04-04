using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [Header("카메라 설정")]
    public float moveSpeed = 10f;    // 카메라 이동 속도
    public float zoomSpeed = 5f;     // 줌 속도
    public float minZoom = 5f;       // 최소 줌
    public float maxZoom = 30f;      // 최대 줌
    public float verticalRotationSpeed = 3f; // 상하 회전 속도
    private float currentXRotation = 55f;    // 초기 카메라 X축 회전각(고개 숙인 정도)

    void Start()
    {
        // 쿼터뷰 각도 세팅
        // 30x30 맵 중앙 위치 (14.5, 14.5)
        transform.position = new Vector3(14.5f, 20f, 14.5f);
        currentXRotation = 55f;
        transform.rotation = Quaternion.Euler(currentXRotation, 0f, 0f);

        //    //나중에 Photon(멀티플레이 추가할시 이걸 예시로 시점 변경)
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    //플레이어 1시점
        //    transform.rotation = Quaternion.Euler(55f, 45f, 0f);
        //}
        //else
        //{
        //    //플레이어 2시점
        //    transform.rotation = Quaternion.Euler(55f, 225, 0f);
        //}

    }

    void Update()
    {
        CameraMove();
        CameraZoom();
        CameraRotateByMouse();
    }

    void CameraMove()
    {
        // WASD 또는 방향키로 카메라 이동
        float h = Input.GetAxis("Horizontal"); // A,D
        float v = Input.GetAxis("Vertical");   // W,S

        Vector3 move = new Vector3(h, 0, v) * moveSpeed * Time.deltaTime;
        transform.position += move;
    }

    void CameraZoom()
    {
        // 마우스 휠로 줌 인/아웃
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize -= scroll * zoomSpeed;
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize, minZoom, maxZoom);
    }
    /*플레이 버튼 누를시
     * 쿼터뷰 45도 시점으로 맵 전체 보임
     * WASD로 카메라 이동
     * 마우스 휠로 줌 인/아웃
     */

    void CameraRotateByMouse()
    {
        if (Input.GetMouseButton(1)) // 우클릭 누를 때만
        {
            float mouseY = Input.GetAxis("Mouse Y");
            // 수직 이동에 따라 각도 변경 (마우스 위로 이동 시 고개 숙임)
            currentXRotation -= mouseY * verticalRotationSpeed;
            currentXRotation = Mathf.Clamp(currentXRotation, 20f, 80f); // 각도 제한 (너무 아래로 숙이거나 너무 올리지 않도록)

            Vector3 currentEuler = transform.rotation.eulerAngles;
            transform.rotation = Quaternion.Euler(currentXRotation, currentEuler.y, currentEuler.z);
        }

    }
}
