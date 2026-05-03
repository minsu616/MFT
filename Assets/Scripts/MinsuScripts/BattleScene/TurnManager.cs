using UnityEngine;

public class TurnManager : MonoBehaviour
{
    // 턴 단계
    public enum Phase
    {
        Command,    // 명령 단계
        Move,       // 이동 단계
        Execute     // 수행 단계
    }

    // 플레이어 턴
    public enum Turn
    {
        Player1,
        Player2
    }

    public Phase currentPhase { get; private set; }
    public Turn currentTurn { get; private set; }

    private APManager apManager;
    public int turnCount = 1; // 현재 턴 수

    void Start()
    {
        apManager = GetComponent<APManager>();
        StartGame();
    }

    // 게임 시작
    void StartGame()
    {
        currentTurn = Turn.Player1;
        StartCommandPhase();
    }

    // ─── 명령 단계 ───
    public void StartCommandPhase()
    {
        currentPhase = Phase.Command;
        apManager.ResetAP();
        Debug.Log($"[턴 {turnCount}] {currentTurn} 명령 단계 시작! AP: {apManager.currentAP}");
    }

    // 명령 완료 → 이동 단계로
    public void EndCommandPhase()
    {
        if (currentPhase != Phase.Command) return;
        Debug.Log($"{currentTurn} 명령 완료!");
        StartMovePhase();
    }

    // ─── 이동 단계 ───
    void StartMovePhase()
    {
        currentPhase = Phase.Move;
        Debug.Log($"[턴 {turnCount}] 이동 단계 시작!");

        //  코루틴이라 완료될때까지 기다려야 함
        // ExecuteMoveCommands 안에서 OnMoveComplete 호출
        ShipSelector shipSelector = FindObjectOfType<ShipSelector>();
        shipSelector.ExecuteMoveCommands();

        // 바로 수행단계로 안넘어감! OnMoveComplete에서 넘어감
    }

    //  이동 완료 후 ShipSelector가 호출
    public void OnMoveComplete()
    {
        StartExecutePhase();
    }

    // ─── 수행 단계 ───
    void StartExecutePhase()
    {
        currentPhase = Phase.Execute;
        Debug.Log($"[턴 {turnCount}] 수행 단계 시작!");
        // 나중에 공격 애니메이션 끝나면 자동으로 턴 종료
        
        //공격 실행 추가
        AttackSystem attackSystem = FindObjectOfType<AttackSystem>();
        attackSystem.ExecuteAttackCommands();

        // 지금은 바로 턴 종료
        EndTurn();
    }

    // ─── 턴 종료 ───
    void EndTurn()
    {
        // 플레이어 교체
        if (currentTurn == Turn.Player1)
            currentTurn = Turn.Player2;
        else
        {
            currentTurn = Turn.Player1;
            turnCount++; // 양측 다 했으면 턴 카운트 증가
        }

        Debug.Log($"턴 종료! 다음 턴: {currentTurn}");
        StartCommandPhase();
    }

    // 외부에서 명령 완료 버튼 누를 때 호출
    public void OnCommandComplete()
    {
        EndCommandPhase();
    }

    // AP 사용
    public bool UseAP(int cost) => apManager.UseAP(cost);
    public bool CanUse(int cost) => apManager.CanUse(cost);
    public int GetCurrentAP() => apManager.currentAP;
}
