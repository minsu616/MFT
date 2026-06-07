using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AttackSystem : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  외부 참조
    // ──────────────────────────────────────────────
    private TurnManager turnManager;
    private ShipSelector shipSelector;
    private BattleSetup battleSetup;
    private ActionButtonUI actionButtonUI;
    private ErrorPopup errorPopup;

    private FogOfWar fogOfWar;

    /*
    // ──────────────────────────────────────────────
    //  미사일 설정 (Inspector)
    // ──────────────────────────────────────────────
    [Header("미사일 프리펩")]
    [Tooltip("Project 탭에서 미사일 프리펩을 여기에 드래그")]
    public GameObject missilePrefab;

    [Tooltip("미사일이 발사되는 오프셋 높이 (함선 위 몇 유닛)")]
    public float missileSpawnHeight = 1.5f;

    [Tooltip("미사일 여러 발을 쏠 때 발사 간격 (초)")]
    public float missileInterval = 0.3f;
    */

    // ──────────────────────────────────────────────
    //  공격 명령 데이터
    // ──────────────────────────────────────────────
    [System.Serializable]
    public class AttackCommand
    {
        public GameObject attacker;
        public Vector2Int attackCoord;
        public int attackCount;    // 공격만: 2, 이동+공격: 1
        public Vector2Int fromCoord;      // 이동+공격 시 이동 후 위치
    }

    private List<AttackCommand> attackCommandList = new List<AttackCommand>();
    private List<GameObject> highlightedTiles = new List<GameObject>();

    // ──────────────────────────────────────────────
    //  초기화
    // ──────────────────────────────────────────────
    void Start()
    {
        turnManager = FindObjectOfType<TurnManager>();
        shipSelector = FindObjectOfType<ShipSelector>();
        battleSetup = FindObjectOfType<BattleSetup>();
        actionButtonUI = FindObjectOfType<ActionButtonUI>();
        errorPopup = FindObjectOfType<ErrorPopup>();
        fogOfWar = FindObjectOfType<FogOfWar>();
    }

    // ──────────────────────────────────────────────
    //  Update — 클릭 입력
    // ──────────────────────────────────────────────
    void Update()
    {
        if (turnManager.currentPhase != TurnManager.Phase.Command) return;

        if (Input.GetMouseButtonDown(0) && actionButtonUI.attackSelected)
            HandleAttackClick();
    }

    // ──────────────────────────────────────────────
    //  클릭 → 좌표 계산
    // ──────────────────────────────────────────────
    void HandleAttackClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (!Physics.Raycast(ray, out hit)) return;

        GameObject target = hit.collider.gameObject;
        if (target.name.Contains("_cell"))
            target = target.transform.parent.gameObject;

        Vector2Int clickCoord = Vector2Int.zero;

        if (target.name.Contains("Tile"))
        {
            string tileName = target.name.Replace("Tile (", "").Replace(")", "");
            string[] coords = tileName.Split(',');
            clickCoord = new Vector2Int(int.Parse(coords[0]), int.Parse(coords[1]));
        }
        else if (target.name.StartsWith("Enemy_"))
        {
            clickCoord = new Vector2Int(
                Mathf.RoundToInt(target.transform.position.x),
                Mathf.RoundToInt(target.transform.position.z));
        }
        else return;

        TryAttack(clickCoord);
    }

    // ──────────────────────────────────────────────
    //  공격 시도 (명령 저장)
    // ──────────────────────────────────────────────
    void TryAttack(Vector2Int clickCoord)
    {
        GameObject selectedShip = shipSelector.GetSelectedShip();
        if (selectedShip == null)
        {
            Debug.Log("함선을 먼저 선택해주세요!");
            return;
        }

        if (attackCommandList.Find(c => c.attacker == selectedShip) != null)
        {
            Debug.Log("이미 공격 명령이 저장된 함선입니다!");
            errorPopup?.ShowError();
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
        Vector2Int myCoord = GetShipCenterCoord(selectedShip);

        int distX = Mathf.Abs(clickCoord.x - myCoord.x);
        int distZ = Mathf.Abs(clickCoord.y - myCoord.y);
        if (distX > attackRange || distZ > attackRange)
        {
            Debug.Log("공격 범위 밖입니다!");
            return;
        }

        GameObject targetEnemy = FindTargetEnemy(myCoord, clickCoord, detectRange, attackRange);
        if (targetEnemy == null)
        {
            Debug.Log("탐지/공격 범위 안에 적이 없습니다!");
            return;
        }

        SaveAttackCommand(selectedShip, clickCoord, 1);
        turnManager.UseAP(APManager.ATTACK_COST);

        Debug.Log($"{sc.GetData().ShipName} 공격 명령 저장! → ({clickCoord.x},{clickCoord.y}) 대상: {targetEnemy.name}");

        actionButtonUI.attackSelected = false;
        actionButtonUI.ShowButtons();
        ClearHighlights();
    }

    // ──────────────────────────────────────────────
    //  탐지+공격 범위 안 타겟 탐색
    // ──────────────────────────────────────────────
    GameObject FindTargetEnemy(
        Vector2Int myCoord,
        Vector2Int clickCoord,
        int detectRange,
        int attackRange)
    {
        List<GameObject> enemyShips = battleSetup.GetEnemyShips();
        GameObject closest = null;
        float closestDist = float.MaxValue;

        foreach (GameObject enemy in enemyShips)
        {
            if (enemy == null || !enemy.activeSelf) continue;

            Vector2Int ec = new Vector2Int(
                Mathf.RoundToInt(enemy.transform.position.x),
                Mathf.RoundToInt(enemy.transform.position.z));

            int dx = Mathf.Abs(ec.x - myCoord.x);
            int dz = Mathf.Abs(ec.y - myCoord.y);

            if (dx > detectRange || dz > detectRange) continue;
            if (dx > attackRange || dz > attackRange) continue;

            float distToClick = Vector2Int.Distance(ec, clickCoord);
            if (distToClick < closestDist)
            {
                closestDist = distToClick;
                closest = enemy;
            }
        }
        return closest;
    }

    // ──────────────────────────────────────────────
    //  명령 저장 헬퍼
    // ──────────────────────────────────────────────
    void SaveAttackCommand(GameObject attacker, Vector2Int coord, int count)
    {
        attackCommandList.Add(new AttackCommand
        {
            attacker = attacker,
            attackCoord = coord,
            attackCount = count
        });
    }

    public void ExecuteAttackCommands()
    {
        StartCoroutine(ExecuteAllMissiles());
    }

    IEnumerator ExecuteAllMissiles()
    {
        // WaitForSeconds 캐싱
        WaitForSeconds waitInterval = new WaitForSeconds(0.3f);
        WaitForSeconds waitArrival = new WaitForSeconds(2.5f);

        foreach (AttackCommand cmd in attackCommandList)
        {
            ShipController attackerSC = cmd.attacker.GetComponent<ShipController>();
            int detectRange = attackerSC.GetData().DetectRange;
            int attackRange = attackerSC.GetData().AttackRange;

            Vector2Int baseCoord = (cmd.fromCoord != Vector2Int.zero)
                ? cmd.fromCoord
                : GetShipCenterCoord(cmd.attacker);

            GameObject targetEnemy = FindTargetEnemy(
                baseCoord, cmd.attackCoord, detectRange, attackRange);

            if (targetEnemy == null)
            {
                Debug.Log($"{attackerSC.GetData().ShipName} 공격 실패!");
                continue;
            }

            int missileCount = GetMissileCount(attackerSC.shipType);
            int damagePerMissile = attackerSC.GetData().Attack / missileCount;

            for (int i = 0; i < missileCount; i++)
            {
                LaunchMissile(cmd.attacker, targetEnemy,
                    cmd.attackCoord, damagePerMissile,
                    attackerSC.shipType);

                yield return waitInterval; // 캐싱된 객체 사용
            }

            yield return waitArrival; // 캐싱된 객체 사용
        }

        attackCommandList.Clear();
        turnManager.OnAttackComplete();
    }

    void LaunchMissile(
    GameObject attacker,
    GameObject targetEnemy,
    Vector2Int attackCoord,
    int damage,
    ShipController.ShipType shipType)
    {
        Vector2Int shipCoord = GetShipCenterCoord(attacker);
        Vector3 spawnPos = new Vector3(shipCoord.x, 1.5f, shipCoord.y);
        Vector3 targetPos = new Vector3(attackCoord.x, 0f, attackCoord.y);

        GameObject missileObj = MissileFactory.CreateMissile(shipType);
        missileObj.transform.position = spawnPos;

        Missile missile = missileObj.GetComponent<Missile>();
        missile.damage = damage;
        missile.attackCount = 1;
        missile.targetEnemyName = targetEnemy.name;

        string enemyName = targetEnemy.name;
        missile.OnArrived += (m) =>
        {
            GameObject enemy = GameObject.Find(enemyName);
            if (enemy != null && enemy.activeSelf)
            {
                ShipController enemySC = enemy.GetComponent<ShipController>();
                enemySC.TakeDamage(m.damage);
                Debug.Log($"데미지: {m.damage} 남은HP: {enemySC.GetData().CurrentHP}");

                // 공격 데이터 상대방에게 전송
                FindObjectOfType<PhotonBattleSync>().SendAttackData(
                    attacker.name,
                    attackCoord.x,
                    attackCoord.y,
                    m.damage,
                    enemySC.GetData().CurrentHP);
            }
        };

        missile.Launch(spawnPos, targetPos);
    }

    // 함선 타입별 발사체 개수
    int GetMissileCount(ShipController.ShipType shipType)
    {
        switch (shipType)
        {
            case ShipController.ShipType.Battleship: return 1; // 전함: 1발
            case ShipController.ShipType.Carrier: return 3; // 항모: 3발
            case ShipController.ShipType.Destroyer: return 2; // 구축함: 2발
            case ShipController.ShipType.Cruiser: return 2; // 순양함: 2발
            case ShipController.ShipType.Submarine: return 1; // 잠수함: 1발
            case ShipController.ShipType.SpeedBoat: return 3; // 고속정: 3발
            default: return 1;
        }
    }

    // ──────────────────────────────────────────────
    //  하이라이트 (공격 범위 표시)
    // ──────────────────────────────────────────────
    public void ShowAttackRange(GameObject ship)
    {
        ClearHighlights();
        ShipController sc = ship.GetComponent<ShipController>();
        int attackRange = sc.GetData().AttackRange;
        Vector2Int shipCoord = GetShipCenterCoord(ship);
        HighlightRange(shipCoord, attackRange);
    }

    public void ShowAttackRangeFromCoord(GameObject ship, Vector2Int fromCoord)
    {
        ClearHighlights();
        int attackRange = ship.GetComponent<ShipController>().GetData().AttackRange;
        HighlightRange(fromCoord, attackRange);
    }

    void HighlightRange(Vector2Int center, int range)
    {
        for (int x = -range; x <= range; x++)
        {
            for (int z = -range; z <= range; z++)
            {
                int tx = center.x + x;
                int tz = center.y + z;
                if (tx < 0 || tx >= 30 || tz < 0 || tz >= 30) continue;

                GameObject tile = GameObject.Find($"Tile ({tx},{tz})");
                if (tile != null)
                {
                    tile.GetComponent<Renderer>().material.color = new Color(1f, 0.3f, 0.3f);
                    highlightedTiles.Add(tile);
                }
            }
        }
    }

    public void ClearHighlightsPublic() => ClearHighlights();

    void ClearHighlights()
    {
        foreach (GameObject tile in highlightedTiles)
        {
            if (tile == null) continue;
            string name = tile.name.Replace("Tile (", "").Replace(")", "");
            string[] coords = name.Split(',');
            int x = int.Parse(coords[0]);
            int z = int.Parse(coords[1]);

            tile.GetComponent<Renderer>().material.color =
                (x + z) % 2 == 0
                    ? new Color(0.2f, 0.5f, 0.8f)
                    : new Color(0.1f, 0.3f, 0.6f);
        }
        highlightedTiles.Clear();
    }

    // ──────────────────────────────────────────────
    //  유틸 / 외부 공개 API
    // ──────────────────────────────────────────────
    Vector2Int GetShipCenterCoord(GameObject ship)
    {
        ShipController sc = ship.GetComponent<ShipController>();
        int centerIndex = (sc.GetData().Size - 1) / 2;
        Transform centerCell = ship.transform.GetChild(centerIndex);
        return new Vector2Int(
            Mathf.RoundToInt(centerCell.position.x),
            Mathf.RoundToInt(centerCell.position.z));
    }

    public bool HasAttackCommand(GameObject ship) =>
        attackCommandList.Find(c => c.attacker == ship) != null;

    public void TryAttackPublic(Vector2Int clickCoord) => TryAttack(clickCoord);

    public void SaveAttackCommandExternal(
        GameObject attacker, Vector2Int coord, int count, Vector2Int fromCoord)
    {
        attackCommandList.Add(new AttackCommand
        {
            attacker = attacker,
            attackCoord = coord,
            attackCount = count,
            fromCoord = fromCoord
        });
        Debug.Log($"이동+공격 명령 저장! 이동위치:({fromCoord.x},{fromCoord.y}) 공격좌표:({coord.x},{coord.y})");
    }
}