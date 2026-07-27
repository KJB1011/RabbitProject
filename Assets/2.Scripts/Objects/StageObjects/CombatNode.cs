using UnityEngine;

/// <summary>
/// 일반 적이 나오는 노드
/// </summary>
public class CombatNode : NodeBase
{
    [Header("적 프리팹 및 스폰 위치")]
    [SerializeField] private GameObject[] _enemyPrefabs;
    [SerializeField] private Transform[] _spawnPoints; // 없으면 (0,0,0)에 스폰

    private EnemyController[] _enemies;
    private int _aliveCount;
    private UIHPBar _hpBar;

    protected override void OnNodeStart()
    {
        IngameManager.Instance._isBattle = true;

        _aliveCount = _enemyPrefabs.Length;
        _enemies = new EnemyController[_enemyPrefabs.Length];

        for (int i = 0; i < _enemyPrefabs.Length; i++)
        {
            if (_enemyPrefabs[i] == null)
            {
                Debug.LogError($"[CombatNode] _enemyPrefabs[{i}]가 null입니다.");
                _aliveCount--;
                continue;
            }

            Vector3 pos = (_spawnPoints != null && i < _spawnPoints.Length)
                ? _spawnPoints[i].position
                : Vector3.zero;

            var go = Instantiate(_enemyPrefabs[i], pos, Quaternion.identity);
            go.SetActive(true);

            _enemies[i] = go.GetComponent<EnemyController>();
            if (_enemies[i] == null)
            {
                Debug.LogError($"[CombatNode] {go.name}에 EnemyController가 없습니다.");
                _aliveCount--;
                continue;
            }

            _enemies[i].OnDied += HandleEnemyDied;
        }

        // HPBar 연결
        _hpBar = IngameManager.Instance.GetHPBar();
        if (_hpBar != null && _enemies.Length > 0 && _enemies[0] != null)
            _hpBar.SetEnemy(_enemies[0]);

        if (_aliveCount <= 0)
            Complete();
    }

    private void HandleEnemyDied()
    {
        _aliveCount--;

        if (_aliveCount > 0 && _hpBar != null)
        {
            foreach (var e in _enemies)
            {
                if (e != null && e.gameObject.activeSelf)
                {
                    _hpBar.SetEnemy(e);
                    break;
                }
            }
        }
        else if (_aliveCount <= 0)
        {
            Complete();
        }
    }

    private void OnDisable()
    {
        if (_enemies != null)
            foreach (var e in _enemies)
                if (e != null) e.OnDied -= HandleEnemyDied;

        _hpBar?.Hide();
    }
}