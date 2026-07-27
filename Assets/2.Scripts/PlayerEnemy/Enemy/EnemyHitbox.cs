using UnityEngine;

/// <summary>
/// Enemy의 히트박스
/// 플레이어가 Enemy를 공격할 때의 판정 검사를 담당한다.
/// </summary>
public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private EnemyController owner;
    [SerializeField] private Transform _spriteTF;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerAttack")) return;

        var player = other.GetComponentInParent<PlayerController>();
        if (player == null) return;

        int damage = player.DamageReturn(out bool isCrit);
        owner.DamageTaken(damage);

        // 아티팩트4(작은 토끼 친구)가 참조할 마지막 피격 적 등록
        ArtifactManager.Instance.LastHitEnemy = owner;

        // 아티팩트2,4에게 기본 공격 적중 알림
        player.NotifyBasicAttackHit();

        DamageTextManager.Instance.Show(damage, isCrit, owner.transform.position);
        Utils.Shake(_spriteTF, this);
        VFXManager.Instance.Play("Hit", transform.position);
    }
}