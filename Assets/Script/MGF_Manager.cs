using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGF_Manager : MonoBehaviour
{
    [Header("題目區 17 張牌")]
    public List<Image> tileSlots; // Inspector 指定 17 個 Image

    [Header("答案區")]
    public List<Image> answerSlots; // Inspector 指定 17 個 Image

    [Header("滑動選牌區")]
    public Transform scrollContent;      // ScrollView Content
    public GameObject tileSlotPrefab;    // 指定你的 TitleSlot Prefab

    private List<Tile> currentQuestion;                  // 目前題目牌
    private List<Tile> selectedTiles = new List<Tile>(); // 玩家已選答案
    private Dictionary<string, int> selectedTileCount = new Dictionary<string, int>();

    // 麻將花色
    public enum TileSuit { Man, Pin, Sou, Honor }

    // 字牌種類
    public enum HonorType { Ton = 1, Nan, Shaa, Pei, Front, Hatsu, Chun }

    // 單張麻將
    public class Tile
    {
        public TileSuit Suit;
        public int Value;

        public Tile(TileSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        // 取得 Sprite 名稱
        public string GetSpriteName()
        {
            switch (Suit)
            {
                case TileSuit.Man: return $"Man{Value}";
                case TileSuit.Pin: return $"Pin{Value}";
                case TileSuit.Sou: return $"Sou{Value}";
                case TileSuit.Honor: return GetHonorSpriteName();
            }
            return "";
        }

        // 字牌名稱
        string GetHonorSpriteName()
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

    void Start()
    {
        ShowNewQuestion(); // 生成題目
        InitAllTiles();    // 初始化滑動選牌區
    }

    public void OnResButtonClick() => ShowNewQuestion(); // 重製題目

    // 隨機生成單張麻將
    static Tile RandomTile()
    {
        TileSuit suit = (TileSuit)Random.Range(0, 4);
        int value;

        if (suit == TileSuit.Honor)
        {
            HonorType[] honorPool = new HonorType[]
            {
                HonorType.Ton, HonorType.Nan, HonorType.Shaa, HonorType.Pei,
                HonorType.Front, HonorType.Hatsu, HonorType.Chun
            };
            value = (int)honorPool[Random.Range(0, honorPool.Length)];
        }
        else
        {
            value = Random.Range(1, 10); // 數字牌 1~9
        }

        return new Tile(suit, value);
    }

    // 生成順子
    static void AddSequence(List<Tile> hand, Dictionary<string, int> tileCount)
    {
        TileSuit suit = (TileSuit)Random.Range(0, 3); // Man, Pin, Sou
        int start = Random.Range(1, 8); // 順子起始數字 1~7

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
        }
    }

    // 排序函式：先花色，再數字
    static int TileCompare(Tile a, Tile b)
    {
        if (a.Suit != b.Suit)
            return a.Suit.CompareTo(b.Suit);
        return a.Value.CompareTo(b.Value);
    }

    // 生成題目牌
    public void ShowNewQuestion()
    {
        // 重置答案區
        selectedTiles.Clear();
        selectedTileCount.Clear();
        foreach (var img in answerSlots)
            img.enabled = false;

        currentQuestion = new List<Tile>();
        Dictionary<string, int> tileCount = new Dictionary<string, int>();

        // 1 對將
        Tile pair = RandomTile();
        string pairName = pair.GetSpriteName();
        tileCount[pairName] = 2;
        currentQuestion.Add(pair);
        currentQuestion.Add(new Tile(pair.Suit, pair.Value));

        // 5 組面子 → 固定 17 張牌
        while (currentQuestion.Count < 17)
        {
            if (Random.value < 0.5f)
                AddSequence(currentQuestion, tileCount);
            else
            {
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

                for (int j = 0; j < 3 && currentQuestion.Count < 17; j++)
                {
                    currentQuestion.Add(new Tile(t.Suit, t.Value));
                    tileCount[name]++;
                }
            }
        }

        // 排序
        currentQuestion.Sort(TileCompare);

        // LOG
        string log = "本次題目：";
        foreach (var t in currentQuestion)
            log += t.GetSpriteName() + " ";
        Debug.Log(log);

        // 顯示題目牌
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (i >= currentQuestion.Count) { tileSlots[i].enabled = false; continue; }

            Sprite sprite = Resources.Load<Sprite>("Tiles/" + currentQuestion[i].GetSpriteName());
            if (sprite != null)
                tileSlots[i].sprite = sprite;
            tileSlots[i].enabled = true;
        }
    }

    // 取得滑動選牌區完整列表（萬>筒>條>字）
    List<Tile> GetAllMahjongTiles()
    {
        List<Tile> tiles = new List<Tile>();
        for (int i = 1; i <= 9; i++)
            tiles.Add(new Tile(TileSuit.Man, i));
        for (int i = 1; i <= 9; i++)
            tiles.Add(new Tile(TileSuit.Pin, i));
        for (int i = 1; i <= 9; i++)
            tiles.Add(new Tile(TileSuit.Sou, i));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Ton));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Nan));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Shaa));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Pei));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Front));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Hatsu));
        tiles.Add(new Tile(TileSuit.Honor, (int)HonorType.Chun));
        return tiles;
    }

    // 初始化滑動選牌區
    public void InitAllTiles()
    {
        if (scrollContent == null || tileSlotPrefab == null)
        {
            Debug.LogError("ScrollContent 或 TileSlotPrefab 沒有指定！");
            return;
        }

        // 清空舊子物件
        foreach (Transform child in scrollContent)
            Destroy(child.gameObject);

        List<Tile> allTiles = GetAllMahjongTiles();

        foreach (var tile in allTiles)
        {
            GameObject slot = Instantiate(tileSlotPrefab, scrollContent);

            // 找 TileImage 子物件
            Transform tileImageTransform = slot.transform.Find("TileImage");
            if (tileImageTransform != null)
            {
                Image tileImage = tileImageTransform.GetComponent<Image>();
                if (tileImage != null)
                {
                    tileImage.sprite = Resources.Load<Sprite>("Tiles/" + tile.GetSpriteName());
                    tileImage.preserveAspect = true;

                    // Button 在 TileImage 上
                    Button btn = tileImageTransform.GetComponent<Button>();
                    if (btn != null)
                    {
                        Tile capturedTile = tile; // Lambda 捕捉
                        btn.onClick.AddListener(() =>
                        {
                            OnTileClicked(capturedTile);
                        });
                    }
                    else
                    {
                        Debug.LogWarning("TileImage 上沒有 Button 組件！");
                    }
                }
            }
            else
            {
                Debug.LogWarning("TitleSlot 沒有 TileImage 子物件！");
            }
        }
    }

    // 點滑動區麻將
    void OnTileClicked(Tile tile)
    {
        Debug.Log("滑動區牌點擊: " + tile.GetSpriteName());

        string name = tile.GetSpriteName();
        if (!selectedTileCount.ContainsKey(name))
            selectedTileCount[name] = 0;

        if (selectedTileCount[name] >= 4)
        {
            Debug.Log("此牌已選滿 4 張：" + name);
            return;
        }

        if (selectedTiles.Count >= answerSlots.Count)
        {
            Debug.Log("答案區已滿");
            return;
        }

        selectedTiles.Add(tile);
        selectedTileCount[name]++;
        RefreshAnswerUI();
    }

    // 更新答案區 UI
    void RefreshAnswerUI()
    {
        // 先清空答案區
        for (int i = 0; i < answerSlots.Count; i++)
        {
            if (answerSlots[i] != null)
            {
                answerSlots[i].enabled = false;
                Button oldBtn = answerSlots[i].GetComponent<Button>();
                if (oldBtn != null)
                    oldBtn.onClick.RemoveAllListeners();
            }
        }

        // 顯示已選牌
        for (int index = 0; index < selectedTiles.Count; index++)
        {
            if (index >= answerSlots.Count) break;

            Tile tile = selectedTiles[index];            // 局部 tile 避免 lambda 捕捉 i
            Image slotImg = answerSlots[index];          // 局部 Image 避免 lambda 捕捉

            if (slotImg == null) continue;

            Sprite sprite = Resources.Load<Sprite>("Tiles/" + tile.GetSpriteName());
            if (sprite != null)
            {
                slotImg.sprite = sprite;
                slotImg.preserveAspect = true;
                slotImg.enabled = true;

                Button btn = slotImg.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() =>
                    {
                        if (slotImg != null) // 安全檢查
                            Debug.Log("答案牌點擊: " + tile.GetSpriteName());
                    });
                }
            }
        }
    }
}