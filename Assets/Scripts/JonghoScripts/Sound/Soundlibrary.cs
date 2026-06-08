using System;
using System.Collections.Generic;
using UnityEngine;

// ─────────────────────────────────────────────────────────────────────────────
// 1. 사운드 ID 열거형
//    ? 새 사운드 추가 시 여기에 항목만 추가하세요.
// ─────────────────────────────────────────────────────────────────────────────
public enum SoundID
{
    // ── BGM ──────────────────────────
    BGM_Lobby,          // 로비/메인화면 배경음
    BGM_Battle,         // 전투 배경음
    BGM_Victory,        // 승리 배경음
    BGM_Defeat,         // 패배 배경음

    // ── UI 효과음 ─────────────────────
    UI_Click,           // 일반 버튼 클릭
    UI_Hover,           // 버튼 호버
    UI_Confirm,         // 확인/OK
    UI_Cancel,          // 취소/뒤로가기
    UI_Popup_Open,      // 팝업 열림
    UI_Popup_Close,     // 팝업 닫힘

    // ── 전투 효과음 ───────────────────
    Attack_Swing,       // 근접 공격 휘두름
    Attack_Hit,         // 공격 적중
    Attack_Miss,        // 공격 빗나감
    Attack_Critical,    // 크리티컬 히트
    Attack_Magic,       // 마법 공격
    Attack_Arrow,       // 원거리(화살)
    Skill_Use,          // 스킬 사용

    // ── 미사일 효과음 ─────────────────
    Missile_Launch,     // 미사일 발사
    Missile_Hit,        // 미사일 착탄/폭발

    // ── 캐릭터/상태 효과음 ────────────
    Char_LevelUp,       // 레벨업
    Char_Heal,          // 회복
    Char_Death,         // 사망
    Char_Jump,          // 점프

    // ── 환경/기타 ─────────────────────
    Env_Coin,           // 코인 획득
    Env_ItemGet,        // 아이템 획득
    Env_DoorOpen,       // 문 열림
}

// ─────────────────────────────────────────────────────────────────────────────
// 2. ScriptableObject ? SoundID ↔ AudioClip 매핑
//    ? Assets > Create > Sound > SoundLibrary 로 생성
// ─────────────────────────────────────────────────────────────────────────────
[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Sound/SoundLibrary")]
public class SoundLibrary : ScriptableObject
{
    [Serializable]
    public class SoundEntry
    {
        public SoundID id;
        public AudioClip clip;
    }

    [SerializeField] private List<SoundEntry> entries = new();

    // 빠른 조회를 위해 Dictionary로 캐싱
    private Dictionary<SoundID, AudioClip> _cache;

    private void OnEnable() => BuildCache();

    private void BuildCache()
    {
        _cache = new Dictionary<SoundID, AudioClip>(entries.Count);
        foreach (var e in entries)
        {
            if (e.clip != null)
                _cache[e.id] = e.clip;
        }
    }

    /// <summary>SoundID에 해당하는 AudioClip을 반환합니다.</summary>
    public AudioClip GetClip(SoundID id)
    {
        if (_cache == null) BuildCache();
        return _cache.TryGetValue(id, out var clip) ? clip : null;
    }
}