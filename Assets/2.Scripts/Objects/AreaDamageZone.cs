using System.Collections;
using UnityEngine;

/// <summary>
/// Enemy - 원형범위에 데미지를 넣는 공격
/// </summary>
public class AreaDamageZone : MonoBehaviour
{
    [Header("컴포넌트 참조")]
    [SerializeField] TelegraphIndicator _telegraph;

    [Header("연출")]
    [SerializeField] float _lingerDuration = 0.2f;

    System.Action _onComplete;

    public void Fire(Vector3 pos, Vector2 size, float telegraphDuration,
                     float angle = 0f, System.Action onComplete = null)
    {
        _onComplete = onComplete;
        gameObject.SetActive(true);
        StartCoroutine(FireRoutine(pos, size, telegraphDuration, angle));
        SoundManager.Instance.PlaySFX("SFX/Telegraph", SoundManager.Instance.SFXVolume, telegraphDuration);
    }

    private IEnumerator FireRoutine(Vector3 pos, Vector2 size,
                                     float telegraphDuration, float angle)
    {
        _telegraph.Show(pos, size, angle);
        yield return new WaitForSeconds(telegraphDuration);
        _telegraph.Hide();

        DealDamage(pos, size);
        VFXManager.Instance.Play("AreaHit", pos);
        SoundManager.Instance.PlaySFX("SFX/AreaDamage");

        yield return new WaitForSeconds(_lingerDuration);

        _onComplete?.Invoke();
    }

    private void DealDamage(Vector3 pos, Vector2 size)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, size.x * 0.5f);

        foreach (var hit in hits)
        {
            var player = hit.GetComponentInParent<PlayerController>();
            if (player == null) continue;
            player.DamageTaken();
            break;
        }
    }
}