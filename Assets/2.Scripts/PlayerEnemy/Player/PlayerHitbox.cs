using UnityEngine;
using static Defines;

/// <summary>
/// Player의 히트박스 판정
/// 플레이어가 Enemy의 공격에 맞는 판정
/// </summary>
public class PlayerHitbox : MonoBehaviour
{
    [SerializeField] PlayerController owner;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("EnemyBullet")) return;

        // 무적 상태면 이펙트도 포함해서 전부 무시
        if (owner._isInvincible) return;

        owner.DamageTaken();
        VFXManager.Instance.Play("PlayerHit", transform.position);
    }
}