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
    private List<Tile> playerAnswers = new List<Tile>(); // 玩家當前作答
    private Dictionary<string, int> playerTileCount = new Dictionary<string, int>(); // 同牌最多 4 張

    private int currentRowIndex = 0; // 當前答案列
    const int MAX_ANSWER = 17;

    public GameObject MessageBox;
    public Text Messagetext;

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
        allRows = new List<List<Image>>() { row0, row1, row2, row3 };

        ShowNewQuestion();   // 題目
        InitAllTiles();      // 滑動選牌
        InitAnswerArea();    // 清空答案
        MessageBox.SetActive(false);
        Messagetext.text = "";
    }

    public void OnResButtonClick()
    {
        ShowNewQuestion();
        InitAnswerArea();
    }

    public void OnCloseMessageBoxClick()
    {
        MessageBox.SetActive(false);
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

        // 建立完整牌池（每種 4 張）
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Man, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Pin, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Sou, i));
        for (int i = 1; i <= 7; i++) for (int j = 0; j < 4; j++) tilePool.Add(new Tile(TileSuit.Honor, i));

        // ===== 取一對將 =====
        Tile pair = tilePool[Random.Range(0, tilePool.Count)];
        currentQuestion.Add(pair);
        currentQuestion.Add(new Tile(pair.Suit, pair.Value));
        tilePool.RemoveAll(t => t.Suit == pair.Suit && t.Value == pair.Value);

        // ===== 組 5 面子 =====
        while (currentQuestion.Count < 17)
        {
            if (Random.value < 0.5f)
            {
                // 順子
                TileSuit suit = (TileSuit)Random.Range(0, 3);
                int start = Random.Range(1, 8);

                Tile a = tilePool.Find(t => t.Suit == suit && t.Value == start);
                Tile b = tilePool.Find(t => t.Suit == suit && t.Value == start + 1);
                Tile c = tilePool.Find(t => t.Suit == suit && t.Value == start + 2);

                if (a != null && b != null && c != null)
                {
                    currentQuestion.Add(a);
                    currentQuestion.Add(b);
                    currentQuestion.Add(c);
                    tilePool.Remove(a);
                    tilePool.Remove(b);
                    tilePool.Remove(c);
                }
            }
            else
            {
                // 刻子
                Tile t = tilePool[Random.Range(0, tilePool.Count)];
                List<Tile> same = tilePool.FindAll(x => x.Suit == t.Suit && x.Value == t.Value);
                if (same.Count >= 3)
                {
                    currentQuestion.Add(same[0]);
                    currentQuestion.Add(same[1]);
                    currentQuestion.Add(same[2]);
                    tilePool.Remove(same[0]);
                    tilePool.Remove(same[1]);
                    tilePool.Remove(same[2]);
                }
            }
        }

        currentQuestion.Sort(TileCompare);

        // 顯示題目
        for (int i = 0; i < tileSlots.Count; i++)
        {
            tileSlots[i].sprite = Resources.Load<Sprite>("Tiles/" + currentQuestion[i].GetSpriteName());
            tileSlots[i].enabled = true;
        }
    }

    // ================= 滑動選牌 =================
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

            Tile captured = tile;
            img.GetComponent<Button>().onClick.AddListener(() => OnScrollTileClicked(captured));
        }
    }

    // ================= 作答 =================
    void InitAnswerArea()
    {
        playerAnswers.Clear();
        playerTileCount.Clear();
        currentRowIndex = 0;

        foreach (var row in allRows)
        {
            foreach (var img in row)
            {
                img.enabled = false;
                if (img.transform.childCount > 0)
                    img.transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    void OnScrollTileClicked(Tile tile)
    {
        string key = tile.GetSpriteName();
        if (!playerTileCount.ContainsKey(key)) playerTileCount[key] = 0;
        if (playerTileCount[key] >= 4 || playerAnswers.Count >= MAX_ANSWER) return;

        playerTileCount[key]++;
        playerAnswers.Add(tile);

        RefreshAnswerRow(currentRowIndex);
    }

    void RefreshAnswerRow(int rowIndex)
    {
        List<Image> row = allRows[rowIndex];
        for (int i = 0; i < row.Count; i++)
        {
            if (i >= playerAnswers.Count)
            {
                row[i].enabled = false;
                continue;
            }

            row[i].sprite = Resources.Load<Sprite>("Tiles/" + playerAnswers[i].GetSpriteName());
            row[i].enabled = true;
        }
    }

    // ================= 提交答案 =================
    public void OnSubmitAnswer()
    {
        if (playerAnswers.Count != 17)
        {
            Debug.Log("答案數量錯誤");
            MessageBox.SetActive(true);
            Messagetext.text = "你確定作答完了嗎?";
            return;
        }

        if (!CanHu(playerAnswers))
        {
            Debug.Log("此答案無法胡牌");
            MessageBox.SetActive(true);
            Messagetext.text = "無法胡牌!!!";
            return;
        }

        CompareAnswer();
    }

    // ================= 胡牌檢查 =================
    bool CanHu(List<Tile> tiles)
    {
        List<Tile> sorted = new List<Tile>(tiles);
        sorted.Sort(TileCompare);

        for (int i = 0; i < sorted.Count - 1; i++)
        {
            if (sorted[i].Suit == sorted[i + 1].Suit &&
                sorted[i].Value == sorted[i + 1].Value)
            {
                List<Tile> remain = new List<Tile>(sorted);
                remain.RemoveAt(i + 1);
                remain.RemoveAt(i);
                if (CanFormMelds(remain)) return true;
            }
        }
        return false;
    }

    bool CanFormMelds(List<Tile> tiles)
    {
        if (tiles.Count == 0) return true;

        tiles.Sort(TileCompare);
        Tile first = tiles[0];

        // 刻子
        int count = tiles.FindAll(t => t.Suit == first.Suit && t.Value == first.Value).Count;
        if (count >= 3)
        {
            List<Tile> next = new List<Tile>(tiles);
            for (int i = 0; i < 3; i++)
                next.Remove(next.Find(t => t.Suit == first.Suit && t.Value == first.Value));
            if (CanFormMelds(next)) return true;
        }

        // 順子
        if (first.Suit != TileSuit.Honor)
        {
            Tile b = tiles.Find(t => t.Suit == first.Suit && t.Value == first.Value + 1);
            Tile c = tiles.Find(t => t.Suit == first.Suit && t.Value == first.Value + 2);
            if (b != null && c != null)
            {
                List<Tile> next = new List<Tile>(tiles);
                next.Remove(first);
                next.Remove(b);
                next.Remove(c);
                if (CanFormMelds(next)) return true;
            }
        }
        return false;
    }

    // ================= 答案比對 =================
    void CompareAnswer()
    {
        List<Image> row = allRows[currentRowIndex];

        for (int i = 0; i < 17; i++)
        {
            if (playerAnswers[i].Suit == currentQuestion[i].Suit &&
                playerAnswers[i].Value == currentQuestion[i].Value)
                continue;

            // 牌對但位置錯 → 半透明遮罩
            bool exist = currentQuestion.Exists(t =>
                t.Suit == playerAnswers[i].Suit && t.Value == playerAnswers[i].Value);

            if (exist && row[i].transform.childCount > 0)
            {
                Image mask = row[i].transform.GetChild(0).GetComponent<Image>();
                mask.color = new Color(1f, 1f, 0f, 0.5f);
                mask.gameObject.SetActive(true);
            }
        }

        currentRowIndex++;
        playerAnswers.Clear();
        playerTileCount.Clear();
    }
}