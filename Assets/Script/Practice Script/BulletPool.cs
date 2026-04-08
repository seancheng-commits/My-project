using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPool
{
    private int[] _freeIndices;
    private int _top;        // stack 的頂端 index
    public int Capacity { get; }

    public BulletPool(int capacity)
    {
        Capacity = capacity;
        _freeIndices = new int[capacity];
        _top = 0;

        // 初始化時把所有 index 都放進去
        for (int i = 0; i < capacity; i++)
        {
            _freeIndices[_top++] = i;
        }
    }

    // 取出一個可用的 index，回傳 -1 表示池子滿了
    public int Get()
    {
        if (_top == 0) return -1;
        return _freeIndices[--_top];
    }

    // 歸還 index
    public void Return(int index)
    {
        _freeIndices[_top++] = index;
    }
}
