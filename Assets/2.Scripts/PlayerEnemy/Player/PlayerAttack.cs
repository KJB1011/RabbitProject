using System.Collections;
using UnityEngine;
using System;

/// <summary>
/// Player의 기본공격, Q스킬의 범위 및 연출을 담당하는 스크립트
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    float _rangeMultiplier = 1.0f;

    [SerializeField] float _baseAttackDistance = 1.2f;
    [SerializeField] float _baseSuperDistance = 2.5f;

    [Header("기본 공격")]
    [SerializeField] BoxCollider2D _attackArea;
    [SerializeField] Transform _attackAreaPos;
    [SerializeField] float _basicActiveTime = 0.1f;

    [Header("Q 슈퍼스킬")]
    [SerializeField] BoxCollider2D _attackArea2;
    [SerializeField] Transform _attackAreaPos2;
    [SerializeField] float _superRangeMultiplier = 4f;
    [SerializeField] float _superActiveTime = 0.1f;
    [SerializeField] Color _superVFXColor = new Color(1f, 0.5f, 0f);

    Vector2 _defaultColSize;
    Vector2 _defaultColSize2;

    void Awake()
    {
        _attackArea.enabled = false;
        _attackArea2.enabled = false;
        _defaultColSize = new Vector2(2f, 1.5f);
        _defaultColSize2 = new Vector2(5f, 4.5f);
    }
    public void SetAttackRange(float multiplier) => _rangeMultiplier = multiplier;


    /// <summary>
    /// 노드 전환 시 진행 중인 공격을 즉시 중단합니다.
    /// </summary>
    public void ForceStop()
    {
        StopAllCoroutines();
        _attackArea.enabled = false;
        _attackArea2.enabled = false;
    }
    // ── 기본 공격 ─────────────────────────────────────────────
    public void AttackStart(Action onComplete)
    {
        StartCoroutine(CoBasicAttack(onComplete));
    }
    IEnumerator CoBasicAttack(Action onComplete)
    {
        _attackArea.enabled = true;

        float multiplier = _rangeMultiplier;
        _attackAreaPos.position = transform.position + transform.right * (_baseAttackDistance * multiplier);
        _attackArea.size = _defaultColSize * multiplier;

        SoundManager.Instance.PlaySFX("SFX/Sword");
        GameObject prefab = Resources.Load<GameObject>("VFX/SwordVFX");
        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab, (Vector2)_attackAreaPos.position, transform.rotation, _attackAreaPos);
            go.transform.localScale = Vector3.one * multiplier;
        }

        yield return new WaitForSeconds(_basicActiveTime);
        _attackArea.enabled = false;
        if (go != null) Destroy(go);
        onComplete?.Invoke();
    }
    // ── Q 스킬 ─────────────────────────────────────────────
    public void SuperAttackStart(Action onComplete)
    {
        StartCoroutine(CoSuperAttack(onComplete));
    }

    IEnumerator CoSuperAttack(Action onComplete)
    {
        _attackArea2.enabled = true;

        float multiplier = _rangeMultiplier;
        _attackAreaPos2.position = transform.position + transform.right * (_baseSuperDistance * multiplier);
        _attackArea2.size = _defaultColSize2 * multiplier;

        SoundManager.Instance.PlaySFX("SFX/StrongSword");
        GameObject prefab = Resources.Load<GameObject>("VFX/SwordVFX2");
        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab, (Vector2)_attackAreaPos2.position, transform.rotation, _attackAreaPos2);
            go.transform.localScale = Vector3.one * _superRangeMultiplier;

            var sr = go.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) sr.color = _superVFXColor;
        }

        yield return new WaitForSeconds(_superActiveTime);
        _attackArea2.enabled = false;
        if (go != null) Destroy(go);
        onComplete?.Invoke();
    }
}