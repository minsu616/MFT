using UnityEngine;
using System.Collections.Generic;

public class AttackSystem : MonoBehaviour
{
    private TurnManager turnManager;
    private ShipSelector shipSelector;
    private BattleSetup battleSetup;
    private ActionButtonUI actionButtonUI;
    private ErrorPopup errorPopup;

    // ActionButtonUI에서 호출용
    public void ClearHighlightsPublic() => ClearHighlights();

    // 공격 명령 저장
    [System.Serializable]
    public class AttackCommand
    {
        public GameObject attacker;      // 공격하는 내 함선
        public Vector2Int attackCoord;   // 공격 좌표
        public int attackCount;          // 공격 횟수 (공격만: 2회, 이동+공격: 1회)
    }

    private List<AttackCommand> attackCommandList = new List<AttackCommand>();
    private List<GameObject> highlightedTiles = new List<GameObject>();

    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        shipSelector = FindObjectOfType<ShipSelector>();
        battleSetup = FindObjectOfType<BattleSetup>();
        actionButtonUI = FindObjectOfType<ActionButtonUI>();
        errorPopup = FindObjectOfType<ErrorPopup>();
    }

    void Update()
    {
        if (turnManager.currentPhase != TurnManager.Phase.Command) return;

        if (Input.GetMouseButtonDown(0) && actionButtonUI.attackSelected)
        {
            HandleAttackClick();
        }
    }

    void HandleAttackClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit)) return;

        GameObject target = hit.collider.gameObject;

        // 자식 셀 클릭시 부모로
        if (target.name.Contains("_cell"))
            target = target.transform.parent.gameObject;

        // 타일 또는 적 함선 클릭
        Vector2Int clickCoord = Vector2Int.zero;

        if (target.name.Contains("Tile"))
        {
            string tileName = target.name.Replace("Tile (", "").Replace(")", "");
            string[] coords = tileName.Split(',');
            clickCoord = new Vector2Int(int.Parse(coords[0]), int.Parse(coords[1]));
        }
        else if (target.name.StartsWith("Enemy_"))
        {
            // 적 함선 직접 클릭시 해당 좌표
            clickCoord = new Vector2Int(
                Mathf.RoundToInt(target.transform.position.x),
                Mathf.RoundToInt(target.transform.position.z));
        }
        else return;

        TryAttack(clickCoord);
    }

    void TryAttack(Vector2Int clickCoord)
    {
        GameObject selectedShip = shipSelector.GetSelectedShip();
        if (selectedShip == null)
        {
            Debug.Log("함선을 먼저 선택해주세요!");
            return;
        }

        AttackCommand existing = attackCommandList.Find(c => c.attacker == selectedShip);
        if (existing != null)
        {
            Debug.Log("이미 공격 명령이 저장된 함선입니다!");
            errorPopup.ShowError(); // ErrorPopup 있으면 표시
            return;
        }

        if (!turnManager.CanUse(APManager.ATTACK_COST))
        {
            Debug.Log("AP 부족! 공격 불가");
            return;
        }

        ShipController sc = selectedShip.GetComponent<ShipController>();
        int detectRange = sc.GetData().DetectRange;
        int attackRange = sc.GetData().AttackRange;

        // 내 함선 중앙 좌표
        Vector2Int myCoord = GetShipCenterCoord(selectedShip);

        // 클릭한 좌표가 공격 범위 안인지 체크
        int distX = Mathf.Abs(clickCoord.x - myCoord.x);
        int distZ = Mathf.Abs(clickCoord.y - myCoord.y);

        if (distX > attackRange || distZ > attackRange)
        {
            Debug.Log("공격 범위 밖입니다!");
            return;
        }

        // 탐지 범위 + 공격 범위 안에 있는 적 찾기
        GameObject targetEnemy = FindTargetEnemy(myCoord, clickCoord, detectRange, attackRange);

        if (targetEnemy == null)
        {
            Debug.Log("탐지/공격 범위 안에 적이 없습니다!");
            return;
        }

        // 공격 명령 저장 (공격만: 2회)
        SaveAttackCommand(selectedShip, clickCoord, 2);

        // AP 소모
        turnManager.UseAP(APManager.ATTACK_COST);

        Debug.Log($"{sc.GetData().ShipName} 공격 명령 저장! → ({clickCoord.x}, {clickCoord.y}) 대상: {targetEnemy.name}");

        // 공격 버튼 해제
        actionButtonUI.attackSelected = false;
        actionButtonUI.ShowButtons();

        ClearHighlights();
    }

    // 탐지+공격 범위 안에서 타겟 적 찾기
    GameObject FindTargetEnemy(Vector2Int myCoord, Vector2Int clickCoord, int detectRange, int attackRange)
    {
        List<GameObject> enemyShips = battleSetup.GetEnemyShips();
        GameObject closestEnemy = null;
        float closestDist = float.MaxValue;

        foreach (GameObject enemy in enemyShips)
        {
            if (enemy == null || !enemy.activeSelf) continue;

            Vector2Int enemyCoord = new Vector2Int(
                Mathf.RoundToInt(enemy.transform.position.x),
                Mathf.RoundToInt(enemy.transform.position.z));

            // 탐지 범위 체크
            int detectDistX = Mathf.Abs(enemyCoord.x - myCoord.x);
            int detectDistZ = Mathf.Abs(enemyCoord.y - myCoord.y);
            if (detectDistX > detectRange || detectDistZ > detectRange) continue;

            // 공격 범위 체크
            int attackDistX = Mathf.Abs(enemyCoord.x - myCoord.x);
            int attackDistZ = Mathf.Abs(enemyCoord.y - myCoord.y);
            if (attackDistX > attackRange || attackDistZ > attackRange) continue;

            // 클릭한 좌표와 가장 가까운 적 찾기
            float distToClick = Vector2Int.Distance(enemyCoord, clickCoord);
            if (distToClick < closestDist)
            {
                closestDist = distToClick;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

    // 공격 명령 저장
    void SaveAttackCommand(GameObject attacker, Vector2Int coord, int count)
    {
        AttackCommand cmd = new AttackCommand();
        cmd.attacker = attacker;
        cmd.attackCoord = coord;
        cmd.attackCount = count;
        attackCommandList.Add(cmd);
    }

    // 수행 단계에서 TurnManager가 호출 - 실제 공격 실행
    public void ExecuteAttackCommands()
    {
        foreach (AttackCommand cmd in attackCommandList)
        {
            ShipController attackerSC = cmd.attacker.GetComponent<ShipController>();
            int detectRange = attackerSC.GetData().DetectRange;
            int attackRange = attackerSC.GetData().AttackRange;
            Vector2Int myCoord = GetShipCenterCoord(cmd.attacker);

            // 타겟 다시 찾기 (이동 후 위치 기준)
            GameObject targetEnemy = FindTargetEnemy(myCoord, cmd.attackCoord, detectRange, attackRange);

            if (targetEnemy == null)
            {
                Debug.Log($"{attackerSC.GetData().ShipName} 공격 실패! 범위 안에 적 없음");
                continue;
            }

            // 공격 횟수만큼 데미지
            ShipController enemySC = targetEnemy.GetComponent<ShipController>();
            for (int i = 0; i < cmd.attackCount; i++)
            {
                enemySC.TakeDamage(attackerSC.GetData().Attack);
                Debug.Log($"{attackerSC.GetData().ShipName} → {enemySC.GetData().ShipName} 공격! " +
                          $"데미지: {attackerSC.GetData().Attack} " +
                          $"남은HP: {enemySC.GetData().CurrentHP}/{enemySC.GetData().MaxHP}");
            }
        }

        attackCommandList.Clear();
    }

    // 공격 범위 하이라이트 표시
    public void ShowAttackRange(GameObject ship)
    {
        ClearHighlights();

        ShipController sc = ship.GetComponent<ShipController>();
        int attackRange = sc.GetData().AttackRange;
        Vector2Int shipCoord = GetShipCenterCoord(ship);

        for (int x = -attackRange; x <= attackRange; x++)
        {
            for (int z = -attackRange; z <= attackRange; z++)
            {
                int tx = shipCoord.x + x;
                int tz = shipCoord.y + z;
                if (tx < 0 || tx >= 30 || tz < 0 || tz >= 30) continue;

                GameObject tile = GameObject.Find($"Tile ({tx},{tz})");
                if (tile != null)
                {
                    tile.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0.3f); // 빨간색
                    highlightedTiles.Add(tile);
                }
            }
        }
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

    Vector2Int GetShipCenterCoord(GameObject ship)
    {
        ShipController sc = ship.GetComponent<ShipController>();
        int size = sc.GetData().Size;
        int centerIndex = (size - 1) / 2;

        Transform centerCell = ship.transform.GetChild(centerIndex);
        return new Vector2Int(
            Mathf.RoundToInt(centerCell.position.x),
            Mathf.RoundToInt(centerCell.position.z));
    }

    //외부에서 공격 명령 여부 확인
    public bool HasAttackCommand(GameObject ship)
    {
        return attackCommandList.Find(c => c.attacker == ship) != null;
    }
}