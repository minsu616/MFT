using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipSelector : MonoBehaviour
{
    // ActionButtonUI에서 호출용
    public void ClearHighlightsPublic() => ClearHighlights();
    public void ShowMoveRangePublic(GameObject ship) => ShowMoveRange(ship);

    private ActionButtonUI actionButtonUI;
    private ErrorPopup errorPopup;


    [Header("색상")]
    public Color selectedColor = new Color(1f, 1f, 0f);
    public Color moveRangeColor = new Color(0f, 1f, 1f);
    public Color attackRangeColor = new Color(1f, 0f, 0f);
    public Color commandedColor = new Color(1f, 0.5f, 0f);  // 명령받은 배 주황색

    private GameObject selectedShip;
    private Color originalShipColor;
    private TurnManager turnManager;
    private List<GameObject> highlightedTiles = new List<GameObject>();

    public enum ActionMode { None, Move, Attack }
    public ActionMode currentAction = ActionMode.None;

    // 명령 저장 목록
    private List<ShipCommand> commandList = new List<ShipCommand>();

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        actionButtonUI = FindObjectOfType<ActionButtonUI>();
        errorPopup = FindObjectOfType<ErrorPopup>();
    }

    void Update()
    {
        if (turnManager.currentPhase != TurnManager.Phase.Command) return;

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject target = hit.collider.gameObject;

            // 자식 셀 클릭시 부모로 올라가기
            if (target.name.Contains("_cell"))
            {
                target = target.transform.parent.gameObject;
            }

            // 내 함선 클릭
            if (target.name.StartsWith("My_"))
            {
                SelectShip(target);
            }
            // 타일 클릭
            else if (target.name.Contains("Tile") && selectedShip != null)
            {
                HandleTileClick(target);
            }
        }
    }


    void SelectShip(GameObject ship)
    {
        if (selectedShip != null)
        {
            if (!HasCommand(selectedShip))
            {
                //자식 셀들 색상 복구
                foreach (Transform cell in selectedShip.transform)
                    cell.GetComponent<Renderer>().material.color = originalShipColor;
            }
            ClearHighlights();
        }

        selectedShip = ship;
        //자식 첫번째 셀에서 원래 색상 가져오기
        originalShipColor = ship.transform.GetChild(0).GetComponent<Renderer>().material.color;

        //자식셀들 전체 노란색으로
        foreach (Transform cell in ship.transform)
            cell.GetComponent<Renderer>().material.color = selectedColor;

        ShipController sc = ship.GetComponent<ShipController>();
        Debug.Log($"{sc.GetData().ShipName} 선택!");

        actionButtonUI.ShowButtons();
        ShowMoveRange(ship); // 이동범위만 표시 (공격범위 제거)
    }

    void ShowMoveRange(GameObject ship)
    {
        ClearHighlights();

        ShipController sc = ship.GetComponent<ShipController>();
        int moveRange = sc.GetData().MoveRange;
        Vector2Int shipCoord = GetShipCoord(ship);

        if (turnManager.CanUse(APManager.MOVE_COST))
        {
            for (int x = -moveRange; x <= moveRange; x++)
            {
                for (int z = -moveRange; z <= moveRange; z++)
                {
                    int tx = shipCoord.x + x;
                    int tz = shipCoord.y + z;
                    if (tx < 0 || tx >= 30 || tz < 0 || tz >= 30) continue;

                    GameObject tile = GameObject.Find($"Tile ({tx},{tz})");
                    if (tile != null)
                    {
                        tile.GetComponent<Renderer>().material.color = moveRangeColor;
                        highlightedTiles.Add(tile);
                    }
                }
            }
        }
    }


    //  이동 명령 저장
    void SaveMoveCommand(GameObject ship, Vector2Int targetCoord)
    {
        ShipController sc = ship.GetComponent<ShipController>();

        ShipCommand existing = commandList.Find(c => c.ship == ship);
        if (existing != null)
        {
            existing.moveTarget = targetCoord;
            existing.hasMoveCommand = true;
        }
        else
        {
            ShipCommand cmd = new ShipCommand();
            cmd.ship = ship;
            cmd.moveTarget = targetCoord;
            cmd.hasMoveCommand = true;

            // 가로/세로 판별 (자식이 1개 이상일 때)
            if (ship.transform.childCount >= 2)
                cmd.isHorizontal = ship.transform.GetChild(1).position.x
                    > ship.transform.GetChild(0).position.x;
            else
                cmd.isHorizontal = true;

            commandList.Add(cmd);
        }

        turnManager.UseAP(APManager.MOVE_COST);

        // 자식 셀 전체 주황색으로
        foreach (Transform cell in ship.transform)
            cell.GetComponent<Renderer>().material.color = commandedColor;

        Debug.Log($"{sc.GetData().ShipName} 이동 명령 저장! → ({targetCoord.x}, {targetCoord.y}) 남은 AP: {turnManager.GetCurrentAP()}");
        ClearHighlights();
    }

    // 이동 단계에서 TurnManager가 호출 - 실제 이동 실행
    public void ExecuteMoveCommands()
    {
        StartCoroutine(ExecuteMoveCoroutine());
    }

    IEnumerator ExecuteMoveCoroutine()
    {
        foreach (ShipCommand cmd in commandList)
        {
            if (cmd.hasMoveCommand)
            {
                ShipController sc = cmd.ship.GetComponent<ShipController>();
                int size = sc.GetData().Size;

                // 현재 위치 (중앙 셀 기준)
                int centerIndex = (size - 1) / 2;
                Vector3 currentPos = cmd.ship.transform.GetChild(centerIndex).position;

                int startX = Mathf.RoundToInt(currentPos.x);
                int startZ = Mathf.RoundToInt(currentPos.z);
                int targetX = cmd.moveTarget.x + centerIndex; // 중앙 기준 목표
                int targetZ = cmd.moveTarget.y;

                // X축 이동
                int stepX = (targetX > startX) ? 1 : -1;
                for (int x = startX; x != targetX; x += stepX)
                {
                    // 부모 이동
                    cmd.ship.transform.position += new Vector3(stepX, 0, 0);

                    // 자식 셀 위치 재정렬
                    for (int i = 0; i < cmd.ship.transform.childCount; i++)
                    {
                        Transform cell = cmd.ship.transform.GetChild(i);
                        if (cmd.isHorizontal)
                            cell.position = new Vector3(
                                cmd.ship.transform.position.x + i, 0.3f,
                                cmd.ship.transform.position.z);
                        else
                            cell.position = new Vector3(
                                cmd.ship.transform.position.x, 0.3f,
                                cmd.ship.transform.position.z + i);
                    }

                    yield return new WaitForSeconds(0.5f); // 0.5초 대기
                }

                // Z축 이동
                int stepZ = (targetZ > startZ) ? 1 : -1;
                for (int z = startZ; z != targetZ; z += stepZ)
                {
                    cmd.ship.transform.position += new Vector3(0, 0, stepZ);

                    for (int i = 0; i < cmd.ship.transform.childCount; i++)
                    {
                        Transform cell = cmd.ship.transform.GetChild(i);
                        if (cmd.isHorizontal)
                            cell.position = new Vector3(
                                cmd.ship.transform.position.x + i, 0.3f,
                                cmd.ship.transform.position.z);
                        else
                            cell.position = new Vector3(
                                cmd.ship.transform.position.x, 0.3f,
                                cmd.ship.transform.position.z + i);
                    }

                    yield return new WaitForSeconds(0.5f);
                }

                // 최종 위치 확정
                cmd.ship.transform.position = new Vector3(
                    cmd.moveTarget.x, 0.3f, cmd.moveTarget.y);

                for (int i = 0; i < cmd.ship.transform.childCount; i++)
                {
                    Transform cell = cmd.ship.transform.GetChild(i);
                    if (cmd.isHorizontal)
                        cell.position = new Vector3(cmd.moveTarget.x + i, 0.3f, cmd.moveTarget.y);
                    else
                        cell.position = new Vector3(cmd.moveTarget.x, 0.3f, cmd.moveTarget.y + i);
                }

                // 색상 복구
                foreach (Transform cell in cmd.ship.transform)
                    cell.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.2f);

                Debug.Log($"{sc.GetData().ShipName} 이동 완료! → ({cmd.moveTarget.x}, {cmd.moveTarget.y})");
            }
        }

        commandList.Clear();

        // 이동 완료 후 하이라이트 초기화
        AttackSystem attackSystem = FindObjectOfType<AttackSystem>();
        attackSystem.ClearHighlightsPublic();

        // 이동 완료 후 턴 진행 (TurnManager에 알려줌)
        FindObjectOfType<TurnManager>().OnMoveComplete();
    }

    bool HasCommand(GameObject ship)
    {
        return commandList.Find(c => c.ship == ship) != null;
    }

    bool IsInMoveRange(Vector2Int targetCoord)
    {
        if (selectedShip == null) return false;

        ShipController sc = selectedShip.GetComponent<ShipController>();
        int moveRange = sc.GetData().MoveRange;
        Vector2Int shipCoord = GetShipCoord(selectedShip);

        int distX = Mathf.Abs(targetCoord.x - shipCoord.x);
        int distZ = Mathf.Abs(targetCoord.y - shipCoord.y);

        return distX <= moveRange && distZ <= moveRange;
    }

    Vector2Int GetShipCoord(GameObject ship)
    {
        ShipController sc = ship.GetComponent<ShipController>();
        int size = sc.GetData().Size;

        // 홀수만 쓰므로 정중앙 인덱스 = (size-1)/2
        int centerIndex = (size - 1) / 2;

        // 고속정(1칸)은 자식이 1개라 무조건 0번
        Transform centerCell = ship.transform.GetChild(centerIndex);
        int x = Mathf.RoundToInt(centerCell.position.x);
        int z = Mathf.RoundToInt(centerCell.position.z);

        return new Vector2Int(x, z);
    }

    void ClearHighlights()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            if (tile != null)
            {
                string tileName = tile.name.Replace("Tile (", "").Replace(")", "");
                string[] coords = tileName.Split(',');
                int x = int.Parse(coords[0]);
                int z = int.Parse(coords[1]);

                if ((x + z) % 2 == 0)
                    tile.GetComponent<Renderer>().material.color = new Color(0.2f, 0.5f, 0.8f);
                else
                    tile.GetComponent<Renderer>().material.color = new Color(0.1f, 0.3f, 0.6f);
            }
        }
        highlightedTiles.Clear();
    }

    public GameObject GetSelectedShip() => selectedShip;

    bool IsPathClear(GameObject ship, Vector2Int targetCoord)
    {
        ShipController sc = ship.GetComponent<ShipController>();
        int size = sc.GetData().Size;
        int centerIndex = (size - 1) / 2;

        // 현재 중앙 좌표
        Vector2Int currentCoord = GetShipCoord(ship);

        int startX = currentCoord.x;
        int startZ = currentCoord.y;
        int endX = targetCoord.x + centerIndex;
        int endZ = targetCoord.y;

        // X축 경로 체크
        int stepX = (endX > startX) ? 1 : -1;
        for (int x = startX + stepX; x != endX + stepX; x += stepX)
        {
            if (IsTileOccupied(new Vector2Int(x, startZ), ship))
            {
                Debug.Log($"경로 ({x}, {startZ})에 함선이 있습니다!");
                return false;
            }
        }

        // Z축 경로 체크
        int stepZ = (endZ > startZ) ? 1 : -1;
        for (int z = startZ + stepZ; z != endZ + stepZ; z += stepZ)
        {
            if (IsTileOccupied(new Vector2Int(endX, z), ship))
            {
                Debug.Log($"경로 ({endX}, {z})에 함선이 있습니다!");
                return false;
            }
        }

        return true;
    }

    // 해당 좌표에 함선이 있는지 체크
    bool IsTileOccupied(Vector2Int coord, GameObject ignorShip)
    {
        BattleSetup battleSetup = FindObjectOfType<BattleSetup>();

        // 내 함선 체크
        foreach (GameObject ship in battleSetup.GetMyShips())
        {
            if (ship == ignorShip || !ship.activeSelf) continue;

            ShipController sc = ship.GetComponent<ShipController>();
            int size = sc.GetData().Size;

            // 함선이 차지하는 모든 칸 체크
            for (int i = 0; i < size; i++)
            {
                Transform cell = ship.transform.GetChild(i);
                int cx = Mathf.RoundToInt(cell.position.x);
                int cz = Mathf.RoundToInt(cell.position.z);

                if (cx == coord.x && cz == coord.y)
                    return true;
            }
        }

        // 적 함선 체크
        foreach (GameObject ship in battleSetup.GetEnemyShips())
        {
            if (!ship.activeSelf) continue;

            ShipController sc = ship.GetComponent<ShipController>();
            int size = sc.GetData().Size;

            for (int i = 0; i < size; i++)
            {
                Transform cell = ship.transform.GetChild(i);
                int cx = Mathf.RoundToInt(cell.position.x);
                int cz = Mathf.RoundToInt(cell.position.z);

                if (cx == coord.x && cz == coord.y)
                    return true;
            }
        }

        return false;
    }

    void HandleTileClick(GameObject tile)
    {
        if (!actionButtonUI.moveSelected)
        {
            Debug.Log("이동 버튼을 먼저 선택해주세요!");
            return;
        }

        if (!turnManager.CanUse(APManager.MOVE_COST))
        {
            Debug.Log("AP 부족! 이동 불가");
            return;
        }

        string tileName = tile.name.Replace("Tile (", "").Replace(")", "");
        string[] coords = tileName.Split(',');
        int x = int.Parse(coords[0]);
        int z = int.Parse(coords[1]);

        if (!IsInMoveRange(new Vector2Int(x, z)))
        {
            Debug.Log("이동 범위 밖입니다!");
            return;
        }

        //  경로 충돌 체크 추가
        if (!IsPathClear(selectedShip, new Vector2Int(x, z)))
        {
            Debug.Log("경로에 함선이 있어 이동할 수 없습니다!");
            errorPopup.ShowError();
            return;
        }

        SaveMoveCommand(selectedShip, new Vector2Int(x, z));
        actionButtonUI.moveSelected = false;
        actionButtonUI.ShowButtons();
    }
}