using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct EnemyData
{
    public float x;
    public float y;
    public float vx;
    public float vy;
    public bool isAlive;
    public float halfSize; // 用於 AABB 碰撞
}
