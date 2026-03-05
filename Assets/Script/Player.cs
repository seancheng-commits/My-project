using UnityEngine;

public class Player
{
    public string Name { get; set; }
    public int Level { get; private set; }
    public float Hp { get; private set; }
    public int Xp { get; private set; }

    public Player(string name, float startHp)
    {
        Name = name;
        Hp = startHp;
        Level = 1;
        Xp = 0;
    }

    public void TakeDamage(float damage)
    {
        Hp -= damage;
        if (Hp <= 0)
        {
            Hp = 0; // 確保血量不為負
            Debug.Log($"{Name} 已經陣亡了！");
        }
    }

    public void GainXp(int amount)
    {
        Xp += amount; 

        if (Xp >= 100)
        {
            Xp -= 100; // ✅ 升級要扣掉 100 經驗
            Level++;
            
            // ✅ 這裡要印的是「等級」，而不是「傷害」
            Debug.Log($"{Name} 升級了！目前等級：{Level}，剩餘經驗：{Xp}");
        }
    }
}
