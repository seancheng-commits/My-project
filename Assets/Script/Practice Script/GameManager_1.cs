using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_1 : MonoBehaviour
{
    private BulletSystem _bulletSystem;
    private EnemySystem  _enemySystem;

    // 用來渲染的 GameObject 陣列，預先建好不再 new
    private Transform[] _bulletTransforms;
    private Transform[] _enemyTransforms;

    private const int MaxBullets = 50;
    private const int MaxEnemies = 20;

    void Start()
    {
        _bulletSystem = new BulletSystem(MaxBullets);
        _enemySystem  = new EnemySystem(MaxEnemies);

        // 預先建好所有 GameObject
        _bulletTransforms = new Transform[MaxBullets];
        for (int i = 0; i < MaxBullets; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.transform.localScale = Vector3.one * 0.2f;
            _bulletTransforms[i] = go.transform;
        }

        _enemyTransforms = new Transform[MaxEnemies];
        for (int i = 0; i < MaxEnemies; i++)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.localScale = Vector3.one * 0.8f;
            _enemyTransforms[i] = go.transform;
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // 邏輯更新
        _bulletSystem.Tick(dt);
        _enemySystem.Tick(dt);

        // AABB 碰撞偵測
        CheckCollisions();

        // 同步位置到 GameObject（這裡是唯一跟 Unity API 互動的地方）
        SyncTransforms();
    }

    private void CheckCollisions()
    {
        for (int b = 0; b < MaxBullets; b++)
        {
            if (!_bulletSystem.bullets[b].isAlive) continue;

            for (int e = 0; e < MaxEnemies; e++)
            {
                if (!_enemySystem.enemies[e].isAlive) continue;

                // AABB：子彈半徑 0.1，敵人半徑 0.4
                float dx = _bulletSystem.bullets[b].x - _enemySystem.enemies[e].x;
                float dy = _bulletSystem.bullets[b].y - _enemySystem.enemies[e].y;
                float combinedHalf = 0.1f + _enemySystem.enemies[e].halfSize;

                if (Mathf.Abs(dx) < combinedHalf && Mathf.Abs(dy) < combinedHalf)
                {
                    _bulletSystem.Kill(b);
                    _enemySystem.Kill(e);
                    break;
                }
            }
        }
    }

    // 用預先宣告的 Vector3 避免每幀 new
    private Vector3 _pos = Vector3.zero;

    private void SyncTransforms()
    {
        for (int i = 0; i < MaxBullets; i++)
        {
            ref BulletData b = ref _bulletSystem.bullets[i];
            if (b.isAlive)
            {
                _pos.x = b.x;
                _pos.y = b.y;
                _pos.z = 0f;
                _bulletTransforms[i].localPosition = _pos;
                _bulletTransforms[i].gameObject.SetActive(true);
            }
            else
            {
                _bulletTransforms[i].gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < MaxEnemies; i++)
        {
            ref EnemyData e = ref _enemySystem.enemies[i];
            if (e.isAlive)
            {
                _pos.x = e.x;
                _pos.y = e.y;
                _pos.z = 0f;
                _enemyTransforms[i].localPosition = _pos;
                _enemyTransforms[i].gameObject.SetActive(true);
            }
            else
            {
                _enemyTransforms[i].gameObject.SetActive(false);
            }
        }
    }
}
