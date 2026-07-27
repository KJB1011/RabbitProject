using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데미지를 제외한 HUD 텍스트들을 관리합니다.
/// </summary>
public class FloatingTextManager : MonoBehaviour
{
    public static FloatingTextManager Instance { get; private set; }

    [SerializeField] private GameObject _prefab;
    [SerializeField] private int _prewarmCount = 15;

    [Header("플레이어 피격")]
    [SerializeField] private Color _damageColor = Color.red;
    [SerializeField] private float _damageFontSize = 3f;

    [Header("아이템 획득")]
    [SerializeField] private Color _itemColor = new Color(0.4f, 1f, 0.4f);
    [SerializeField] private float _itemFontSize = 2.5f;

    private Queue<FloatingText> _pool = new();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (_prefab == null)
        {
            Debug.LogError("[FloatingTextManager] _prefab이 연결되지 않았습니다.");
            return;
        }
        if (_prefab.GetComponent<FloatingText>() == null)
        {
            Debug.LogError($"[FloatingTextManager] '{_prefab.name}'에 FloatingText 컴포넌트가 없습니다.");
            return;
        }

        for (int i = 0; i < _prewarmCount; i++)
            _pool.Enqueue(CreateNew());
    }

    private FloatingText CreateNew()
    {
        var go = Instantiate(_prefab, transform);
        go.SetActive(false);
        return go.GetComponent<FloatingText>();
    }

    private FloatingText Get()
    {
        var ft = _pool.Count > 0 ? _pool.Dequeue() : CreateNew();
        ft.gameObject.SetActive(true);
        return ft;
    }

    private void Release(FloatingText ft)
    {
        ft.ReturnCleanup();
        _pool.Enqueue(ft);
    }

    /// <summary>
    /// 플레이어 피격 "-1"로 표시
    /// </summary>
    public void ShowDamage(Vector3 worldPos)
    {
        var ft = Get();
        ft.Play("-1", _damageColor, _damageFontSize, worldPos, Release);
    }

    /// <summary>
    /// 아이템 획득 효과
    /// </summary>
    public void ShowItemEffect(string content, Vector3 worldPos)
    {
        var ft = Get();
        ft.Play(content, _itemColor, _itemFontSize, worldPos, Release);
    }

    /// <summary>
    /// 색상/크기 직접 지정 (상점 알림 등)
    /// </summary>
    public void ShowCustom(string content, Color color, float fontSize, Vector3 worldPos)
    {
        var ft = Get();
        ft.Play(content, color, fontSize, worldPos, Release);
    }
}