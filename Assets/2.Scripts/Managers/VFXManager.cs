using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using static Defines;

/// <summary>
/// VFX / 탄환 재생을 담당하는 매니저.
/// Resources/VFX/ 폴더에서 프리팹을 로드합니다.
/// </summary>
public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("풀 설정")]
    [SerializeField] private int _vfxDefaultCapacity = 10;
    [SerializeField] private int _vfxMaxSize = 50;
    [SerializeField] private int _bulletDefaultCapacity = 32;
    [SerializeField] private int _bulletMaxSize = 200;

    // 프리팹 캐싱 (SoundManager의 clipCache와 동일한 구조)
    private Dictionary<string, GameObject> _prefabCache = new();

    // 풀
    private Dictionary<string, ObjectPool<PooledVFX>> _vfxPools = new();
    private Dictionary<string, ObjectPool<PooledBullet>> _bulletPools = new();

    // 이펙트 최대 지속 시간
    float _lifetime = 3f;


    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── VFX ──────────────────────────────────────────────────

    public PooledVFX Play(string id, Vector3 position, Quaternion rotation = default)
    {
        var pool = GetOrCreateVFXPool(id);
        if (pool == null) return null;

        var instance = pool.Get();
        if (instance == null) return null;

        instance.transform.SetPositionAndRotation(position, rotation);

        instance.PlayAndScheduleReturn(_lifetime, () => pool.Release(instance));

        return instance;
    }

    private ObjectPool<PooledVFX> GetOrCreateVFXPool(string id)
    {
        if (_vfxPools.TryGetValue(id, out var existing)) return existing;

        string path = $"VFX/{id}";
        var prefab = LoadPrefab(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[VFXManager] 프리팹을 찾을 수 없습니다: Resources/VFX/{path}");
            return null;
        }

        var pool = new ObjectPool<PooledVFX>(
            createFunc: () => Instantiate(prefab, transform).GetComponent<PooledVFX>(),
            actionOnGet: vfx => vfx.gameObject.SetActive(true),
            actionOnRelease: vfx => vfx.gameObject.SetActive(false),
            actionOnDestroy: vfx => Destroy(vfx.gameObject),
            defaultCapacity: _vfxDefaultCapacity,
            maxSize: _vfxMaxSize
        );

        _vfxPools[id] = pool;
        return pool;
    }

    /// <summary>
    /// 플레이어에게 붙어서 재생되는 VFX
    /// </summary>
    public PooledVFX PlayAttached(string id, Transform target, Vector3 localOffset = default)
    {
        var pool = GetOrCreateVFXPool(id);
        if (pool == null) return null;

        var instance = pool.Get();
        if (instance == null) return null;

        // target의 자식으로 붙이기
        instance.transform.SetParent(target);
        instance.transform.localPosition = localOffset;
        instance.transform.localRotation = Quaternion.identity;

        // 파티클 재시작
        var systems = instance.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in systems)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        instance.PlayAndScheduleReturn(_lifetime, () =>
        {
            // 반납 전 부모 해제 (VFXManager 자식으로 복귀)
            instance.transform.SetParent(transform);
            pool.Release(instance);
        });

        return instance;
    }
    // ── 탄환 ─────────────────────────────────────────────────

    public PooledBullet FireBullet(string id, Vector3 position, Vector2 velocity,
                                    float lifetime = 5f)
    {
        var pool = GetOrCreateBulletPool(id);
        if (pool == null) return null;

        var bullet = pool.Get();
        if (bullet == null) return null;

        bullet.transform.position = position;
        bullet.Launch(velocity, lifetime, b => pool.Release(b));
        return bullet;
    }

    private ObjectPool<PooledBullet> GetOrCreateBulletPool(string id)
    {
        if (_bulletPools.TryGetValue(id, out var existing)) return existing;

        string path = id;
        var prefab = LoadPrefab(path);
        if (prefab == null)
        {
            Debug.LogWarning($"[VFXManager] 탄환 프리팹을 찾을 수 없습니다: Resources/VFX/{path}");
            return null;
        }

        var pool = new ObjectPool<PooledBullet>(
            createFunc: () => Instantiate(prefab, transform).GetComponent<PooledBullet>(),
            actionOnGet: b => b.gameObject.SetActive(true),
            actionOnRelease: b => b.gameObject.SetActive(false),
            actionOnDestroy: b => Destroy(b.gameObject),
            defaultCapacity: _bulletDefaultCapacity,
            maxSize: _bulletMaxSize
        );

        _bulletPools[id] = pool;
        return pool;
    }

    // ── 유틸 ─────────────────────────────────────────────────

    private GameObject LoadPrefab(string path)
    {
        if (_prefabCache.TryGetValue(path, out var cached)) return cached;

        var prefab = Resources.Load<GameObject>(path);
        if (prefab != null)
            _prefabCache[path] = prefab;

        return prefab;
    }
}