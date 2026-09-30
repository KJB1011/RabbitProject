using System.Collections;
using UnityEngine;

/// <summary>
/// 풀에서 꺼내져 사용되는 탄환 하나를 담당하는 스크립트
/// 이동, 수명 관리, 플레이어 충돌 시 데미지를 처리합니다.
/// </summary>
public class PooledBullet : MonoBehaviour
{
    private Vector2 _velocity;
    private System.Action<PooledBullet> _releaseCallback;
    private Coroutine _returnRoutine;

    public void Launch(Vector2 velocity, float lifetime,
                       System.Action<PooledBullet> releaseCallback)
    {
        _velocity = velocity;
        _releaseCallback = releaseCallback;

        // 이동 방향으로 오브젝트 회전
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        if (_returnRoutine != null) StopCoroutine(_returnRoutine);
        _returnRoutine = StartCoroutine(ReturnAfter(lifetime));
    }

    void Update()
    {
        transform.Translate(_velocity * Time.deltaTime, Space.World);
    }

    // 수명이 다하거나 맞았을 때 풀로 반납
    public void ReleaseSelf()
    {
        if (_returnRoutine != null)
        {
            StopCoroutine(_returnRoutine);
            _returnRoutine = null;
        }
        _releaseCallback?.Invoke(this);
    }

    private IEnumerator ReturnAfter(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        ReleaseSelf();
    }

    private void OnDisable()
    {
        _returnRoutine = null;
    }
}