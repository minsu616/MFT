using UnityEngine;

public class APManager : MonoBehaviour
{
    [Header("AP 설정")]
    public int maxAP = 5;         // 턴당 최대 AP
    public int currentAP;         // 현재 AP

    // AP 비용
    public const int MOVE_COST = 1;
    public const int ATTACK_COST = 2;
    public const int SKILL_COST = 3;

    void Start()
    {
        ResetAP();
    }

    // 턴 시작시 AP 초기화
    public void ResetAP()
    {
        currentAP = maxAP;
        Debug.Log($"AP 초기화! 현재 AP: {currentAP}");
    }

    // AP 사용 가능한지 체크
    public bool CanUse(int cost)
    {
        return currentAP >= cost;
    }

    // AP 소모
    public bool UseAP(int cost)
    {
        if (!CanUse(cost))
        {
            Debug.Log($"AP 부족! 현재 AP: {currentAP}, 필요 AP: {cost}");
            return false;
        }
        currentAP -= cost;
        Debug.Log($"AP 사용! 남은 AP: {currentAP}");
        return true;
    }

    // 이동 가능?
    public bool CanMove() => CanUse(MOVE_COST);

    // 공격 가능?
    public bool CanAttack() => CanUse(ATTACK_COST);

    // 스킬 가능?
    public bool CanSkill() => CanUse(SKILL_COST);
}