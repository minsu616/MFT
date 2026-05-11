using UnityEngine;
using System.Collections;

public class Missile : MonoBehaviour
{
    // ──────────────────────────────────────────────
    //  Inspector에서 조정 가능한 설정값
    // ──────────────────────────────────────────────
    [Header("비행 설정")]
    [Tooltip("포물선 최고 높이 (타일 단위)")]
    public float arcHeight = 8f;

    [Tooltip("비행 총 시간 (초)")]
    public float flightDuration = 1.2f;

    [Header("이펙트")]
    [Tooltip("폭발 파티클 프리펩 (없으면 스킵)")]
    public GameObject explosionPrefab;

    [Tooltip("폭발 이펙트가 사라지기까지의 시간 (초)")]
    public float explosionLifetime = 2f;

    [Tooltip("미사일 자체 트레일/파티클 오브젝트 (있으면 분리 후 자동 소멸)")]
    public GameObject trailObject;

    // ──────────────────────────────────────────────
    //  내부 상태
    // ──────────────────────────────────────────────
    private Vector3 _startPos;
    private Vector3 _endPos;
    private float _elapsed;
    private bool _launched;

    // 공격 데이터 (AttackSystem이 설정)
    [HideInInspector] public int damage;
    [HideInInspector] public int attackCount;   // 이 미사일 한 발이 입힐 공격 횟수
    [HideInInspector] public string targetEnemyName;

    // 도착 이벤트 콜백 (AttackSystem에서 구독)
    public System.Action<Missile> OnArrived;

    // ──────────────────────────────────────────────
    //  외부 호출: 발사 시작
    // ──────────────────────────────────────────────
    /// <summary>
    /// AttackSystem.cs 에서 호출. 발사체 시작 위치와 목표 위치를 받아 비행을 시작한다.
    /// </summary>
    public void Launch(Vector3 start, Vector3 end)
    {
        _startPos = start;
        _endPos = end;
        _elapsed = 0f;
        _launched = true;
    }

    // ──────────────────────────────────────────────
    //  매 프레임 포물선 이동
    // ──────────────────────────────────────────────
    void Update()
    {
        if (!_launched) return;

        _elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(_elapsed / flightDuration);

        // 포물선 공식
        // P(t) = Lerp(start, end, t)  +  UP * arcHeight * sin(π*t)
        Vector3 linearPos = Vector3.Lerp(_startPos, _endPos, t);
        float arc = arcHeight * Mathf.Sin(Mathf.PI * t);
        transform.position = linearPos + Vector3.up * arc;

        // 미사일 머리가 진행 방향을 향하도록 회전
        if (t < 0.99f)
        {
            Vector3 nextLinear = Vector3.Lerp(_startPos, _endPos, t + 0.02f);
            float nextArc = arcHeight * Mathf.Sin(Mathf.PI * (t + 0.02f));
            Vector3 nextPos = nextLinear + Vector3.up * nextArc;
            Vector3 dir = (nextPos - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);
        }

        // 도착 판정
        if (t >= 1f)
        {
            _launched = false;
            Arrive();
        }
    }

    // ──────────────────────────────────────────────
    //  도착 처리
    // ──────────────────────────────────────────────
    void Arrive()
    {
        // 폭발 이펙트 생성
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, _endPos, Quaternion.identity);
            Destroy(fx, explosionLifetime);
        }

        // 트레일 오브젝트가 있으면 부모에서 분리 후 페이드 소멸
        if (trailObject != null)
        {
            trailObject.transform.SetParent(null);
            Destroy(trailObject, 1.5f);
        }

        // 콜백 → AttackSystem이 실제 데미지 처리
        OnArrived?.Invoke(this);

        Destroy(gameObject);
    }
}