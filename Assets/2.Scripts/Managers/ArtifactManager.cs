using System.Collections.Generic;
using UnityEngine;
using static Defines;

/// <summary>
/// 플레이어가 보유한 아티팩트를 관리합니다.
/// 아티팩트 획득/제거, 효과 활성화/비활성화를 담당합니다.
/// </summary>
public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    private readonly Dictionary<ArtifactId, ArtifactEffect> _activeEffects = new();
    private PlayerController _player;

    // 아이템4(작은 토끼 친구)가 참조할 마지막 피격 적
    public EnemyBase LastHitEnemy { get; set; }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void SetPlayer(PlayerController player)
    {
        _player = player;

        foreach (var effect in _activeEffects.Values)
        {
            effect.Deactivate();
            effect.Activate(_player);
        }
    }

    /// <summary>
    /// 아티팩트를 획득합니다. (상점 구매 or 보스 드롭)
    /// </summary>
    public bool Acquire(ArtifactId id)
    {
        if (_activeEffects.ContainsKey(id))
        {
            Debug.Log($"[Artifact] 이미 보유 중: {id}");
            return false;
        }

        var effect = CreateEffect(id);
        if (effect == null) return false;

        effect.Activate(_player);
        _activeEffects[id] = effect;

        Debug.Log($"[Artifact] 획득: {id}");
        return true;
    }
    /// <summary>
    /// 아티팩트를 제거합니다.
    /// </summary>
    public void Remove(ArtifactId id)
    {
        if (!_activeEffects.TryGetValue(id, out var effect)) return;

        effect.Deactivate();
        _activeEffects.Remove(id);
        Debug.Log($"[Artifact] 제거: {id}");
    }

    public bool HasArtifact(ArtifactId id) => _activeEffects.ContainsKey(id);

    private ArtifactEffect CreateEffect(ArtifactId id)
    {
        switch (id)
        {
            case ArtifactId.WhipAndCarrot:
                return new Artifact_WhipAndCarrot();
            case ArtifactId.AccelerationWatch:
                return new Artifact_AccelerationWatch();
            case ArtifactId.CarrotBazooka:
                return new Artifact_CarrotBazooka();
            case ArtifactId.SmallRabbitFriend:
                return new Artifact_SmallRabbitFriend();
            case ArtifactId.ShiningCarrot:
                return new Artifact_ShiningCarrot();
            default:
                return null;
        }
    }
}