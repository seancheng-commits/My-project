using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGF_Manager : MonoBehaviour
{
    [Header("題目區 17 張牌")]
    public List<Image> tileSlots; // Inspector 指定 17 個 Image

    [Header("滑動選牌區")]
    public Transform scrollContent;      // ScrollView Content
    public GameObject tileSlotPrefab;    // TileSlot Prefab

    [Header("玩家答案區，每列 17 張，共 4 列")]
    public List<Image> row0; // 第一列
    public List<Image> row1; // 第二列
    public List<Image> row2; // 第三列
    public List<Image> row3; // 第四列

    private List<List<Image>> allRows; // 整合四列，方便操作

    private List<Tile> currentQuestion;                  // 目前題目牌
    private List<Tile> playerAnswers = new List<Tile>(); // 玩家已選答案
    private Dictionary<string, int> playerTileCount = new Dictionary<string, int>(); // 同一張牌最多 4 張

    private int currentRowIndex = 0; // 當前答案列

    const int MAX_ANSWER = 17; // 答案區上限 17 張

    // ================= 麻將資料 =================
    public enum TileSuit { Man, Pin, Sou, Honor }
    public enum HonorType { Ton = 1, Nan, Shaa, Pei, Front, Hatsu, Chun }

    public class Tile
    {
        public TileSuit Suit;
        public int Value;

        public Tile(TileSuit suit, int value)
        {
            Suit = suit;
            Value = value;
        }

        public string GetSpriteName()
        {
            switch (Suit)
            {
                case TileSuit.Man: return $"Man{Value}";
                case TileSuit.Pin: return $"Pin{Value}";
                case TileSuit.Sou: return $"Sou{Value}";
                case TileSuit.Honor: return ((HonorType)Value).ToString();
            }
            return "";
        }
    }

    // ================= Start =================
    void Start()
    {
        // 整合四列
        allRows = new List<List<Image>>() { row0, row1, row2, row3 };

        ShowNewQuestion();   // 題目區
        InitAllTiles();      // 滑動選牌區
        InitAnswerArea();    // 清空答案區
    }

    public void OnResButtonClick()
    {
        ShowNewQuestion();   // 重製題目
        InitAnswerArea();    // 清空答案區
    }

    // ================= 題目生成 =================
    static int TileCompare(Tile a, Tile b)
    {
        if (a.Suit != b.Suit) return a.Suit.CompareTo(b.Suit);
        return a.Value.CompareTo(b.Value);
    }

    public void ShowNewQuestion()
    {
        currentQuestion = new List<Tile>();
        List<Tile> tilePool = new List<Tile>();

        // ===== 建立完整牌池（每種牌 4 張） =====
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Man, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Pin, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Sou, i));
        for (int i = 1; i <= 7; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Honor, i));

        // ===== 1 對將 =====
        int pairIndex = Random.Range(0, tilePool.Count);
        Tile pair = tilePool[pairIndex];
        currentQuestion.Add(pair);
        currentQuestion.Add(new Tile(pair.Suit, pair.Value));
        tilePool.RemoveAll(t => t.Suit == pair.Suit && t.Value == pair.Value);

        // ===== 5 組面子 =====
        while (currentQuestion.Count < 17)
        {
            if (Random.value < 0.5f)
            {
                // 嘗試生成順子
                TileSuit suit = (TileSuit)Random.Range(0, 3); // Man, Pin, Sou
                int start = Random.Range(1, 8);
                bool canMakeSeq = true;
                List<Tile> seqTiles = new List<Tile>();
                for (int i = 0; i < 3; i++)
                {
                    Tile t = tilePool.Find(x => x.Suit == suit && x.Value == start + i);
                    if (t != null) seqTiles.Add(t);
                    else { canMakeSeq = false; break; }
                }
                if (canMakeSeq)
                {
                    foreach (Tile t in seqTiles)
                    {
                        currentQuestion.Add(t);
                        tilePool.Remove(t);
                    }
                }
            }
            else
            {
                // 嘗試生成刻子
                int idx = Random.Range(0, tilePool.Count);
                Tile t = tilePool[idx];
                List<Tile> triplet = tilePool.FindAll(x => x.Suit == t.Suit && x.Value == t.Value);
                if (triplet.Count >= 3)
                {
                    currentQuestion.Add(triplet[0]);
                    currentQuestion.Add(triplet[1]);
                    currentQuestion.Add(triplet[2]);
                    tilePool.Remove(triplet[0]);
                    tilePool.Remove(triplet[1]);
                    tilePool.Remove(triplet[2]);
                }
            }
        }

        currentQuestion.Sort(TileCompare);

        // ===== 顯示題目區 =====
        for (int i = 0; i < tileSlots.Count; i++)
        {
            if (i >= currentQuestion.Count)
            {
                tileSlots[i].enabled = false;
                continue;
            }

            Sprite sprite = Resources.Load<Sprite>("Tiles/" + currentQuestion[i].GetSpriteName());
            tileSlots[i].sprite = sprite;
            tileSlots[i].enabled = true;
        }

        Debug.Log("合法題目生成完成：" + string.Join(" ", currentQuestion.ConvertAll(t => t.GetSpriteName())));
    }

    // ================= 滑動選牌區 =================
    List<Tile> GetAllMahjongTiles()
    {
        List<Tile> list = new List<Tile>();
        for (int i = 1; i <= 9; i++) list.Add(new Tile(TileSuit.Man, i));
        for (int i = 1; i <= 9; i++) list.Add(new Tile(TileSuit.Pin, i));
        for (int i = 1; i <= 9; i++) list.Add(new Tile(TileSuit.Sou, i));
        for (int i = 1; i <= 7; i++) list.Add(new Tile(TileSuit.Honor, i));
        return list;
    }

    public void InitAllTiles()
    {
        foreach (Transform c in scrollContent)
            Destroy(c.gameObject);

        foreach (Tile tile in GetAllMahjongTiles())
        {
            GameObject slot = Instantiate(tileSlotPrefab, scrollContent);

            Image img = slot.transform.Find("TileImage").GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("Tiles/" + tile.GetSpriteName());
            img.preserveAspect = true;

            Button btn = img.GetComponent<Button>();
            Tile captured = tile;

            // 點擊滑動區牌，加入答案區
            btn.onClick.AddListener(() => OnScrollTileClicked(captured));
        }
    }

    // ================= 顯示答案區 =================
    void InitAnswerArea()
    {
        playerAnswers.Clear();
        playerTileCount.Clear();
        currentRowIndex = 0;

        // 清空四列答案區圖片
        foreach (var row in allRows)
        {
            foreach (var img in row)
            {
                img.sprite = null;
                img.enabled = false;
                // Mask 透明
                if (img.transform.childCount > 0)
                    img.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    void OnScrollTileClicked(Tile tile)
    {
        string name = tile.GetSpriteName();

        // ===== 限制同一張牌最多 4 張 =====
        if (!playerTileCount.ContainsKey(name)) playerTileCount[name] = 0;
        if (playerTileCount[name] >= 4)
        {
            Debug.Log("此牌已選滿 4 張：" + name);
            return;
        }

        // ===== 限制答案區最多 17 張 =====
        if (playerAnswers.Count >= MAX_ANSWER)
        {
            Debug.Log("答案已滿 17 張");
            return;
        }

        playerAnswers.Add(tile);
        playerTileCount[name]++;

        // ===== 顯示答案在當前列 =====
        RefreshAnswerRow(currentRowIndex, playerAnswers);
    }

    // ===== 顯示答案列函式 =====
    void RefreshAnswerRow(int rowIndex, List<Tile> answers)
    {
        if (rowIndex < 0 || rowIndex >= allRows.Count) return;

        List<Image> row = allRows[rowIndex];
        for (int i = 0; i < row.Count; i++)
        {
            if (i >= answers.Count)
            {
                row[i].enabled = false;
                if (row[i].transform.childCount > 0)
                    row[i].transform.GetChild(0).gameObject.SetActive(false);
                continue;
            }

            Tile tile = answers[i];
            Sprite sprite = Resources.Load<Sprite>("Tiles/" + tile.GetSpriteName());
            row[i].sprite = sprite;
            row[i].enabled = true;

            // 隱藏提示遮罩
            if (row[i].transform.childCount > 0)
                row[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    // ================= 確認答案按鈕 =================
    public void OnSubmitAnswer()
    {
        // ===== 檢查答案長度 =====
        if (playerAnswers.Count != currentQuestion.Count)
        {
            Debug.Log("答案長度不符合，無法提交！");
            return;
        }

        // ===== 檢查答案能否胡牌（可擴充） =====
        if (!CanHu(playerAnswers))
        {
            Debug.Log("答案無法胡牌！");
            return;
        }

        // ===== 開始比對答案 =====
        CompareAnswer();
    }

    // ===== 範例胡牌檢查（簡單版，可再擴充） =====
    bool CanHu(List<Tile> answers)
    {
        // 這裡先暫時回傳 true
        return true;
    }

    // ===== 答案比對 =====
    void CompareAnswer()
    {
        if (currentRowIndex >= allRows.Count) return;

        List<Image> row = allRows[currentRowIndex];

        for (int i = 0; i < playerAnswers.Count; i++)
        {
            Tile playerTile = playerAnswers[i];
            Tile correctTile = currentQuestion[i];

            if (playerTile.Suit == correctTile.Suit && playerTile.Value == correctTile.Value)
            {
                // 完全正確，顯示答案
                row[i].sprite = Resources.Load<Sprite>("Tiles/" + playerTile.GetSpriteName());
                row[i].enabled = true;
            }
            else
            {
                // 牌對但位置不對 → 顯示遮罩提示
                row[i].enabled = true;
                if (row[i].transform.childCount > 0)
                {
                    row[i].transform.GetChild(0).gameObject.SetActive(true);
                    // 可用不同顏色區分提示
                    row[i].transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                }
            }
        }

        // 下一列
        currentRowIndex++;

        // 清空玩家答案區，準備下一次作答
        playerAnswers.Clear();
        playerTileCount.Clear();
    }
}