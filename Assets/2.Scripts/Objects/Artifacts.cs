using System.Collections;
using UnityEngine;

/// <summary>
/// 아티팩트들의 효과
/// </summary>

// ──────────────────────────────────────────────────────────
// 1. 채찍과 당근
//    대쉬 후 2초 동안 공격력 15% 증가
// ──────────────────────────────────────────────────────────
public class Artifact_WhipAndCarrot : ArtifactEffect
{
    private const float _atkBonus = 0.15f;
    private const float _duration = 2f;
    private Coroutine _routine;

    protected override void OnActivate()
    {
        Player.OnDashStarted += HandleDash;
    }

    protected override void OnDeactivate()
    {
        Player.OnDashStarted -= HandleDash;
        if (_routine != null) Player.StopCoroutine(_routine);
        Player.RemoveAtkMultiplier(_atkBonus);
    }

    private void HandleDash()
    {
        if (_routine != null) Player.StopCoroutine(_routine);
        _routine = Player.StartCoroutine(AtkBoostRoutine());
    }

    private IEnumerator AtkBoostRoutine()
    {
        Player.AddAtkMultiplier(_atkBonus);
        yield return new WaitForSeconds(_duration);
        Player.RemoveAtkMultiplier(_atkBonus);
    }
}

// ──────────────────────────────────────────────────────────
// 2. 가속의 회중시계
//    기본 공격 적중 시 Q스킬 쿨타임 0.2초 감소
// ──────────────────────────────────────────────────────────
public class Artifact_AccelerationWatch : ArtifactEffect
{
    private const float _cooldownReduce = 0.2f;

    protected override void OnActivate()
    {
        Player.OnBasicAttackHit += HandleAttackHit;
    }

    protected override void OnDeactivate()
    {
        Player.OnBasicAttackHit -= HandleAttackHit;
    }

    private void HandleAttackHit()
    {
        Player.ReduceSuperSkillCooldown(_cooldownReduce);
    }
}

// ──────────────────────────────────────────────────────────
// 3. 당근 바주카
//    필살기 데미지 25% 증가
// ──────────────────────────────────────────────────────────
public class Artifact_CarrotBazooka : ArtifactEffect
{
    private const float _bonus = 0.25f;

    protected override void OnActivate()
    {
        Player.AddSpecialDamageBonus(_bonus);
    }

    protected override void OnDeactivate()
    {
        Player.AddSpecialDamageBonus(-_bonus);
    }
}

// ──────────────────────────────────────────────────────────
// 4. 작은 토끼 친구
//    3초에 한 번씩, 적 공격 시 공격력 100% 추가 데미지
// ──────────────────────────────────────────────────────────
public class Artifact_SmallRabbitFriend : ArtifactEffect
{
    private const float _cooldown = 3f;
    private bool _ready = true;
    private Coroutine _cooldownRoutine;

    protected override void OnActivate()
    {
        _ready = true;
        Player.OnBasicAttackHit += HandleAttackHit;
    }

    protected override void OnDeactivate()
    {
        Player.OnBasicAttackHit -= HandleAttackHit;
        if (_cooldownRoutine != null)
            Player.StopCoroutine(_cooldownRoutine);
        _ready = true;
    }

    private void HandleAttackHit()
    {
        if (!_ready) return;

        var targetEnemy = ArtifactManager.Instance.LastHitEnemy;
        if (targetEnemy == null || !targetEnemy.gameObject.activeSelf) return;

        int bonusDamage = Mathf.Max(1, Player.GetBaseAtk()); // 최소 1 보장

        targetEnemy.DamageTaken(bonusDamage);

        // 데미지 텍스트 표시 (토끼 친구임을 알 수 있게 다른 색상)
        DamageTextManager.Instance.Show(bonusDamage, false,
            targetEnemy.transform.position + Vector3.up * 0.8f);

        _ready = false;
        _cooldownRoutine = Player.StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        yield return new UnityEngine.WaitForSeconds(_cooldown);
        _ready = true;
    }
}

// ──────────────────────────────────────────────────────────
// 5. 빛나는 당근
//    대쉬 중 무적 지속시간 0.5초 증가
// ──────────────────────────────────────────────────────────
public class Artifact_ShiningCarrot : ArtifactEffect
{
    private const float _extraTime = 0.5f;

    protected override void OnActivate()
    {
        Player.AddDashInvincibleTime(_extraTime);
    }

    protected override void OnDeactivate()
    {
        Player.AddDashInvincibleTime(-_extraTime);
    }
}