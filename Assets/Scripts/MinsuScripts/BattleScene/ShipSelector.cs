using UnityEngine;
using System.Collections.Generic;

public class ShipSelector : MonoBehaviour
{
    private ActionButtonUI actionButtonUI; //추가

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
        actionButtonUI = FindObjectOfType<ActionButtonUI>(); //추가

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
                // 자식 셀들 색상 복구
                foreach (Transform cell in selectedShip.transform)
                    cell.GetComponent<Renderer>().material.color = originalShipColor;
            }
            ClearHighlights();
        }

        selectedShip = ship;

        // 자식 첫번째 셀에서 원래 색상 가져오기
        originalShipColor = ship.transform.GetChild(0).GetComponent<Renderer>().material.color;

        // 자식 셀들 전체 노란색으로
        foreach (Transform cell in ship.transform)
            cell.GetComponent<Renderer>().material.color = selectedColor;

        ShipController sc = ship.GetComponent<ShipController>();
        Debug.Log($"{sc.GetData().ShipName} 선택!");

        actionButtonUI.ShowButtons(); //추가

        ShowMoveRange(ship);
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

    void HandleTileClick(GameObject tile)
    {
        // 이동 버튼 선택됐는지 체크
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

        SaveMoveCommand(selectedShip, new Vector2Int(x, z));

        // 이동 명령 저장 후 이동 버튼 해제
        actionButtonUI.moveSelected = false;
        actionButtonUI.ShowButtons();
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
        foreach (ShipCommand cmd in commandList)
        {
            if (cmd.hasMoveCommand)
            {
                ShipController sc = cmd.ship.GetComponent<ShipController>();
                int size = sc.GetData().Size;

                // 부모 위치 이동
                cmd.ship.transform.position = new Vector3(
                    cmd.moveTarget.x, 0.3f, cmd.moveTarget.y);

                // 자식 셀 위치 재정렬
                for (int i = 0; i < cmd.ship.transform.childCount; i++)
                {
                    Transform cell = cmd.ship.transform.GetChild(i);
                    if (cmd.isHorizontal)
                        cell.position = new Vector3(cmd.moveTarget.x + i, 0.3f, cmd.moveTarget.y);
                    else
                        cell.position = new Vector3(cmd.moveTarget.x, 0.3f, cmd.moveTarget.y + i);
                }

                // 자식 셀 초록색으로 복구
                foreach (Transform cell in cmd.ship.transform)
                    cell.GetComponent<Renderer>().material.color = new Color(0.2f, 0.8f, 0.2f);

                Debug.Log($"{sc.GetData().ShipName} 이동 실행! → ({cmd.moveTarget.x}, {cmd.moveTarget.y})");
            }
        }
        commandList.Clear();
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
        int x = Mathf.RoundToInt(ship.transform.position.x);
        int z = Mathf.RoundToInt(ship.transform.position.z);
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
}