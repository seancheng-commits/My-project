using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySystem
{
    public EnemyData[] enemies;
    private BulletPool _pool;
    private float _spawnTimer;
    private const float SpawnInterval = 1.0f;
    private const float EnemySpeed = 2f;
    private const float BoundaryX = 10f;
    private const float BoundaryY = 6f;

    public EnemySystem(int capacity)
    {
        enemies = new EnemyData[capacity];
        _pool = new BulletPool(capacity);   // 借用同一個 Pool 結構管理 enemy index

        // 設定每個敵人的半身大小
        for (int i = 0; i < capacity; i++)
            enemies[i].halfSize = 0.4f;
    }

    public void Tick(float deltaTime)
    {
        _spawnTimer += deltaTime;
        if (_spawnTimer >= SpawnInterval)
        {
            _spawnTimer -= SpawnInterval;
            Spawn();
        }

        for (int i = 0; i < enemies.Length; i++)
        {
            if (!enemies[i].isAlive) continue;

            enemies[i].x += enemies[i].vx * deltaTime;
            enemies[i].y += enemies[i].vy * deltaTime;

            // 到中央附近就消失
            float dx = enemies[i].x;
            float dy = enemies[i].y;
            if (dx * dx + dy * dy < 0.2f)
            {
                enemies[i].isAlive = false;
                _pool.Return(i);
            }
        }
    }

    private void Spawn() //生成系統
    {
        int idx = _pool.Get();
        if (idx == -1) return;

        // 從四邊隨機出現
        int side = Random.Range(0, 4);
        float px, py;
        switch (side)
        {
            case 0: px = Random.Range(-BoundaryX, BoundaryX); py =  BoundaryY; break;
            case 1: px = Random.Range(-BoundaryX, BoundaryX); py = -BoundaryY; break;
            case 2: px =  BoundaryX; py = Random.Range(-BoundaryY, BoundaryY); break;
            default: px = -BoundaryX; py = Random.Range(-BoundaryY, BoundaryY); break;
        }

        // 朝中央移動
        float len = Mathf.Sqrt(px * px + py * py);
        enemies[idx].x = px;
        enemies[idx].y = py;
        enemies[idx].vx = (-px / len) * EnemySpeed;
        enemies[idx].vy = (-py / len) * EnemySpeed;
        enemies[idx].isAlive = true;
    }

    public void Kill(int index)
    {
        enemies[index].isAlive = false;
        _pool.Return(index);
    }
}
