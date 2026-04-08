using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSystem
{
    public BulletData[] bullets;        // 固定大小，不會再 new
    private BulletPool _pool;
    private float _fireTimer;
    private const float FireInterval = 0.3f;
    private const float BulletSpeed = 8f;
    private const float BoundaryX = 10f; //X軸的邊界距離
    private const float BoundaryY = 6f; //Y軸的邊界距離

    public BulletSystem(int capacity)
    {
        bullets = new BulletData[capacity];
        _pool = new BulletPool(capacity);
    }

    public void Tick(float deltaTime)
    {
        // 計時發射
        _fireTimer += deltaTime;
        if (_fireTimer >= FireInterval)
        {
            _fireTimer -= FireInterval; // 控制射擊速度
            Fire();
        }

        // 更新所有子彈位置
        for (int i = 0; i < bullets.Length; i++)
        {
            if (!bullets[i].isAlive) continue; // 跳過沒再用的子彈

            //物理公式 : 位移 = 速度 * 時間
            bullets[i].x += bullets[i].vx * deltaTime;
            bullets[i].y += bullets[i].vy * deltaTime;

            // 出界就回收
            if (bullets[i].x < -BoundaryX || bullets[i].x > BoundaryX ||
                bullets[i].y < -BoundaryY || bullets[i].y > BoundaryY)
            {
                bullets[i].isAlive = false;
                _pool.Return(i);  //消除子彈將子彈收回
            }
        }
    }

    private void Fire()
    {
        int idx = _pool.Get();
        if (idx == -1) return;   // 池子滿了就不射

        // 隨機方向
        float angle = Random.Range(0f, Mathf.PI * 2f);
        bullets[idx].x = 0f;
        bullets[idx].y = 0f;
        bullets[idx].vx = Mathf.Cos(angle) * BulletSpeed;
        bullets[idx].vy = Mathf.Sin(angle) * BulletSpeed;
        bullets[idx].isAlive = true;
    }

    // 從外部（碰撞系統）呼叫，回收某顆子彈
    public void Kill(int index)
    {
        bullets[index].isAlive = false;
        _pool.Return(index);
    }
}
