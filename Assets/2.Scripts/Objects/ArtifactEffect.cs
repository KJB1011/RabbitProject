/// <summary>
/// 모든 아티팩트 효과의 추상 기반.
/// PlayerController 이벤트를 구독해서 효과를 적용하고,
/// Deactivate 시 구독을 해제합니다.
/// </summary>
public abstract class ArtifactEffect
{
    protected PlayerController Player { get; set; }

    public void Activate(PlayerController player)
    {
        Player = player;
        OnActivate();
    }

    public void Deactivate()
    {
        OnDeactivate();
        Player = null;
    }

    protected abstract void OnActivate();
    protected abstract void OnDeactivate();
}