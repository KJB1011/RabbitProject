using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enemy의 공격을 담당하는 스크립트
/// 1. 방사형 탄막 - 주변에 흩뿌리는 형식의 탄막
/// 2. 단발 탄막 - 특정 위치로 빠르게 발사하는 적은 수의 탄막
/// 3. 범위 공격 - 원모양의 넓은 범위에 데미지를 입히는 공격
/// </summary>
public class EnemyAttack : MonoBehaviour
{
    [Header("방사형 탄막")]
    [SerializeField] public int bulletCount = 16;
    [SerializeField] private float bulletSpeed = 4f;

    [Header("단발 탄막")]
    [SerializeField] private float fastBulletSpeed = 12f;

    [Header("범위 공격")]
    [SerializeField] private GameObject _areaDamageZonePrefab;
    [SerializeField] private float _areaTelegraphDuration = 1.5f;

    [Header("탄막 궤적 전조")]
    [SerializeField] private GameObject _bulletTelegraphPrefab; // BulletTelegraph 프리팹
    [SerializeField] private float _telegraphDuration = 0.8f;  // 전조 표시 시간

    // 궤적 및 범위공격 인스턴스 풀
    private List<BulletTelegraph> _telegraphPool = new();
    private Queue<AreaDamageZone> _areaPool = new();
    // ── 방사형 탄막 (전조 없음) ───────────────────────────────

    public void FireRadialAttack()
    {
        float angleStep = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));
            VFXManager.Instance.FireBullet("VFX/TestBullet", transform.position,
                dir * bulletSpeed);
        }
    }

    // ── 방사형 탄막 (궤적 전조 있음) ─────────────────────────

    public void FireRadialAttackWithTelegraph(System.Action onComplete = null)
    {
        StartCoroutine(RadialTelegraphRoutine(onComplete));
    }

    private IEnumerator RadialTelegraphRoutine(System.Action onComplete)
    {
        // 1) 방향별 궤적 표시
        float angleStep = 360f / bulletCount;
        var telegraphs = new List<BulletTelegraph>();

        for (int i = 0; i < bulletCount; i++)
        {
            float angle = angleStep * i;
            Vector2 dir = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad));

            var telegraph = GetTelegraph();
            telegraph.Show(transform.position, dir);
            telegraphs.Add(telegraph);
        }

        // 2) 전조 시간 대기
        yield return new WaitForSeconds(_telegraphDuration);

        // 3) 궤적 페이드 아웃 + 동시에 탄막 발사
        foreach (var t in telegraphs)
            t.Hide(() => ReturnTelegraph(t));

        SoundManager.Instance.PlaySFX("SFX/BulletFire");
        FireRadialAttack();

        onComplete?.Invoke();
    }

    // ── 단발 발사 ─────────────────────────────────────────────

    public void FireSingleShot(Vector2 targetPosition)
    {
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;
        VFXManager.Instance.FireBullet("VFX/TestBullet", transform.position,
            dir * fastBulletSpeed);
    }

    // ── 단발 발사 (궤적 전조 있음) ───────────────────────────

    public void FireSingleShotWithTelegraph(Vector2 targetPosition,
                                             System.Action onComplete = null)
    {
        StartCoroutine(SingleTelegraphRoutine(targetPosition, onComplete));
    }

    private IEnumerator SingleTelegraphRoutine(Vector2 targetPosition,
                                                System.Action onComplete)
    {
        Vector2 dir = (targetPosition - (Vector2)transform.position).normalized;

        var telegraph = GetTelegraph();
        telegraph.Show(transform.position, dir);

        yield return new WaitForSeconds(_telegraphDuration);

        telegraph.Hide(() => ReturnTelegraph(telegraph));
        SoundManager.Instance.PlaySFX("SFX/BulletFire");
        FireSingleShot(targetPosition);

        onComplete?.Invoke();
    }

    // ── 범위 공격 ─────────────────────────────────────────────

    public void FireAreaAttack(Vector3 pos, Vector2 size, float angle = 0f,
                               System.Action onComplete = null)
    {
        var zone = GetAreaZone();
        if (zone == null) return;

        // 반납 콜백을 onComplete에 추가
        zone.Fire(pos, size, _areaTelegraphDuration, angle, () =>
        {
            ReturnAreaZone(zone);
            onComplete?.Invoke();
        });
    }

    public void FireAreaAttackAtPlayer(Vector2 size, System.Action onComplete = null)
    {
        var playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null) return;
        FireAreaAttack(playerObj.transform.position, size, 0f, onComplete);
    }

    public void FireMultiAreaAttack(Vector3[] positions, Vector2 size,
                                    System.Action onComplete = null)
    {
        int remaining = positions.Length;
        foreach (var pos in positions)
        {
            FireAreaAttack(pos, size, 0f, () =>
            {
                remaining--;
                if (remaining <= 0) onComplete?.Invoke();
            });
        }
    }

    // ── BulletTelegraph 풀 ────────────────────────────────────

    private BulletTelegraph GetTelegraph()
    {
        // 비활성 상태인 풀 항목 재사용
        foreach (var t in _telegraphPool)
            if (!t.gameObject.activeSelf) return t;

        // 없으면 새로 생성
        if (_bulletTelegraphPrefab == null)
        {
            Debug.LogWarning("[EnemyAttack] _bulletTelegraphPrefab이 연결되지 않았습니다.");
            return null;
        }

        var go = Instantiate(_bulletTelegraphPrefab, transform);
        var bt = go.GetComponent<BulletTelegraph>();
        _telegraphPool.Add(bt);
        return bt;
    }

    private void ReturnTelegraph(BulletTelegraph telegraph)
    {
        if (telegraph != null)
            telegraph.gameObject.SetActive(false);
    }
    // ── AreaDamageZone 풀 ────────────────────────────────────
    private AreaDamageZone GetAreaZone()
    {
        // 비활성 상태인 항목 재사용
        if (_areaPool.Count > 0)
        {
            var zone = _areaPool.Dequeue();
            if (zone != null)
            {
                zone.gameObject.SetActive(true);
                return zone;
            }
        }

        // 풀 소진 시 새로 생성
        if (_areaDamageZonePrefab == null) return null;
        var go = Instantiate(_areaDamageZonePrefab, transform);
        return go.GetComponent<AreaDamageZone>();
    }

    private void ReturnAreaZone(AreaDamageZone zone)
    {
        if (zone == null) return;
        zone.gameObject.SetActive(false);
        _areaPool.Enqueue(zone);
    }
}