using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGF_Manager : MonoBehaviour
{
    public List<Image> tileSlots; // Inspector 指定 16 個 Image
    private List<MGF_Manager.Tile> currentQuestion;

    public void OnResButtonClick()  // 重製題目按鈕
    {
        ShowNewQuestion();
    }

    public enum TileSuit  // 定義花色
    {
        Man,    // 萬
        Pin,    // 筒
        Sou,    // 條
        Honor   // 字牌
    }

    public enum HonorType  // 定義字牌
    {
        Ton = 1,    // 東
        Nan = 2,    // 南
        Shaa = 3,   // 西
        Pei = 4,    // 北
        Front = 5,  // 白
        Hatsu = 6,  // 發
        Chun = 7    // 中
    }

    public class Tile // 單一麻將
    {
        public TileSuit Suit; // 花色
        public int Value;     // 牌面數字

        public Tile(TileSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        public string GetSpriteName()  // 取得圖片名稱方法
        {
            switch (Suit)
            {
                case TileSuit.Man:
                    return $"Man{Value}";
                case TileSuit.Pin:
                    return $"Pin{Value}";
                case TileSuit.Sou:
                    return $"Sou{Value}";
                case TileSuit.Honor:
                    return GetHonorSpriteName();
            }
            return "";
        }

        string GetHonorSpriteName()  // 取得字牌名稱
        {
            switch ((HonorType)Value)
            {
                case HonorType.Ton: return "Ton";
                case HonorType.Nan: return "Nan";
                case HonorType.Shaa: return "Shaa";
                case HonorType.Pei: return "Pei";
                case HonorType.Front: return "Front";
                case HonorType.Hatsu: return "Hatsu";
                case HonorType.Chun: return "Chun";
                default: return "unknown";
            }
        }
    }

    // 隨機生成一張牌
    static Tile RandomTile()
    {
        TileSuit suit = (TileSuit)Random.Range(0, 4);
        int value;

        if (suit == TileSuit.Honor)
        {
            HonorType[] honorPool = new HonorType[]
            {
                HonorType.Ton,
                HonorType.Nan,
                HonorType.Shaa,
                HonorType.Pei,
                HonorType.Front,
                HonorType.Hatsu,
                HonorType.Chun
            };
            int index = Random.Range(0, honorPool.Length);
            value = (int)honorPool[index];
        }
        else
        {
            value = Random.Range(1, 10); // 數字牌 1~9
        }

        return new Tile(suit, value);
    }

    // 順子生成
    static void AddSequence(List<Tile> hand, Dictionary<string, int> tileCount)
    {
        TileSuit suit = (TileSuit)Random.Range(0, 3); // Man, Pin, Sou
        int start = Random.Range(1, 8);

        for (int i = 0; i < 3; i++)
        {
            Tile t = new Tile(suit, start + i);
            string name = t.GetSpriteName();
            if (!tileCount.ContainsKey(name))
                tileCount[name] = 0;

            if (tileCount[name] < 4)
            {
                hand.Add(t);
                tileCount[name]++;
            }
            else
            {
                // 如果這張牌已經有4張，重選一張同花色不同數字的牌
                int tries = 0;
                while (tileCount[name] >= 4 && tries < 10)
                {
                    start = Random.Range(1, 8);
                    t = new Tile(suit, start + i);
                    name = t.GetSpriteName();
                    if (!tileCount.ContainsKey(name))
                        tileCount[name] = 0;
                    tries++;
                }
                if (tileCount[name] < 4)
                {
                    hand.Add(t);
                    tileCount[name]++;
                }
            }
        }
    }

    // 比較牌大小（排序用）
    static int TileCompare(Tile a, Tile b)
    {
        if (a.Suit != b.Suit)
            return a.Suit.CompareTo(b.Suit);
        return a.Value.CompareTo(b.Value);
    }

    void Start()
    {
        ShowNewQuestion(); // 點 Play 自動生成題目
    }

    public void ShowNewQuestion()
    {
        currentQuestion = new List<Tile>();
        Dictionary<string, int> tileCount = new Dictionary<string, int>();

        // 1 對將
        Tile pair = RandomTile();
        string pairName = pair.GetSpriteName();
        tileCount[pairName] = 2;
        currentQuestion.Add(pair);
        currentQuestion.Add(new Tile(pair.Suit, pair.Value));

        // 5 組面子 → 固定 17 張牌
        for (int i = 0; i < 5; i++)
        {
            if (Random.value < 0.5f)
            {
                AddSequence(currentQuestion, tileCount); // 順子
            }
            else
            {
                // 刻子，保證三張
                Tile t;
                string name;
                int tries = 0;
                do
                {
                    t = RandomTile();
                    name = t.GetSpriteName();
                    if (!tileCount.ContainsKey(name))
                        tileCount[name] = 0;
                    tries++;
                } while (tileCount[name] >= 4 && tries < 10);

                for (int j = 0; j < 3; j++)
                {
                    currentQuestion.Add(new Tile(t.Suit, t.Value));
                    tileCount[name]++;
                }
            }
        }

        // 排序
        currentQuestion.Sort(TileCompare);

        // LOG 顯示牌型
        string log = "本次題目：";
        foreach (Tile t in currentQuestion)
        {
            log += t.GetSpriteName() + " ";
        }
        Debug.Log(log);

        // 顯示到 UI
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (i >= currentQuestion.Count)
            {
                tileSlots[i].enabled = false;
                continue;
            }

            Sprite sprite = Resources.Load<Sprite>("Tiles/" + currentQuestion[i].GetSpriteName());
            if (sprite != null)
                tileSlots[i].sprite = sprite;
            else
                Debug.LogWarning("找不到圖片: " + currentQuestion[i].GetSpriteName());

            tileSlots[i].enabled = true;
        }
    }
}