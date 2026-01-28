using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MGF_Manager : MonoBehaviour
{
    [Header("題目區 17 張牌")]
    public List<Image> tileSlots;

    [Header("滑動選牌區")]
    public Transform scrollContent;
    public GameObject tileSlotPrefab;

    [Header("玩家答案區，每列 17 張，共 4 列")]
    public List<Image> row0;
    public List<Image> row1;
    public List<Image> row2;
    public List<Image> row3;

    private List<List<Image>> allRows;

    private List<Tile> currentQuestion;
    private Tile[] playerAnswers = new Tile[17];
    private Dictionary<string, int> playerTileCount = new Dictionary<string, int>();

    //記住「牌 + 位置都正確」的答案，下一列會自動帶入
    private Tile[] lockedCorrectTiles = new Tile[17];

    private int currentRowIndex = 0;
    const int MAX_ANSWER = 17;

    [Header("提示視窗")]
    public GameObject MessageBox;
    public Text Messagetext;

    [Header("清除按鈕")]
    public Button ClearCurrentAnswerButton;

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

    void Start()
    {
        allRows = new List<List<Image>>() { row0, row1, row2, row3 };

        // ★ 綁定點擊刪除答案事件
        BindAnswerSlotClick(row0);
        BindAnswerSlotClick(row1);
        BindAnswerSlotClick(row2);
        BindAnswerSlotClick(row3);

        ShowNewQuestion();
        InitAllTiles();
        InitAnswerArea();

        MessageBox.SetActive(false);
        Messagetext.text = "";

        if (ClearCurrentAnswerButton != null)
        ClearCurrentAnswerButton.onClick.AddListener(ClearCurrentAnswer);
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

    //題目生成
    static int TileCompare(Tile a, Tile b)
    {
        if (a.Suit != b.Suit) return a.Suit.CompareTo(b.Suit);
        return a.Value.CompareTo(b.Value);
    }

    public void ShowNewQuestion()
    {
        currentQuestion = new List<Tile>();
        List<Tile> pool = new List<Tile>();

        // 建立牌池
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) pool.Add(new Tile(TileSuit.Man, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) pool.Add(new Tile(TileSuit.Pin, i));
        for (int i = 1; i <= 9; i++) for (int j = 0; j < 4; j++) pool.Add(new Tile(TileSuit.Sou, i));
        for (int i = 1; i <= 7; i++) for (int j = 0; j < 4; j++) pool.Add(new Tile(TileSuit.Honor, i));

        //組成將
        Tile pair = pool[Random.Range(0, pool.Count)];
        currentQuestion.Add(pair);
        currentQuestion.Add(new Tile(pair.Suit, pair.Value));
        pool.RemoveAll(t => t.Suit == pair.Suit && t.Value == pair.Value);

        //組成面子
        while (currentQuestion.Count < 17)
        {
            if (Random.value < 0.5f)
            {
                TileSuit suit = (TileSuit)Random.Range(0, 3);
                int start = Random.Range(1, 8);

                Tile a = pool.Find(t => t.Suit == suit && t.Value == start);
                Tile b = pool.Find(t => t.Suit == suit && t.Value == start + 1);
                Tile c = pool.Find(t => t.Suit == suit && t.Value == start + 2);

                if (a != null && b != null && c != null)
                {
                    currentQuestion.Add(a);
                    currentQuestion.Add(b);
                    currentQuestion.Add(c);
                    pool.Remove(a);
                    pool.Remove(b);
                    pool.Remove(c);
                }
            }
            else
            {
                Tile t = pool[Random.Range(0, pool.Count)];
                List<Tile> same = pool.FindAll(x => x.Suit == t.Suit && x.Value == t.Value);
                if (same.Count >= 3)
                {
                    currentQuestion.Add(same[0]);
                    currentQuestion.Add(same[1]);
                    currentQuestion.Add(same[2]);
                    pool.Remove(same[0]);
                    pool.Remove(same[1]);
                    pool.Remove(same[2]);
                }
            }
        }

        currentQuestion.Sort(TileCompare);

        for (int i = 0; i < 17; i++)
        {
            tileSlots[i].sprite = Resources.Load<Sprite>("Tiles/" + currentQuestion[i].GetSpriteName());
            tileSlots[i].enabled = true;
        }

        Debug.Log("題目：" + string.Join(" ", currentQuestion.ConvertAll(t => t.GetSpriteName())));
    }

    //滑動選牌
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
        foreach (Transform c in scrollContent) Destroy(c.gameObject);

        foreach (Tile tile in GetAllMahjongTiles())
        {
            GameObject slot = Instantiate(tileSlotPrefab, scrollContent);
            Image img = slot.transform.Find("TileImage").GetComponent<Image>();
            img.sprite = Resources.Load<Sprite>("Tiles/" + tile.GetSpriteName());

            Tile captured = tile;
            img.GetComponent<Button>().onClick.AddListener(() => OnScrollTileClicked(captured));
        }
    }

    //作答
    void InitAnswerArea()
    {
        //將上一次完全正確的答案自動填回
        for (int i = 0; i < 17; i++)
            playerAnswers[i] = lockedCorrectTiles[i];

        //重新計算牌數量
        playerTileCount.Clear();
        for (int i = 0; i < 17; i++)
            if (playerAnswers[i] != null)
            {
                string key = playerAnswers[i].GetSpriteName();
                if (!playerTileCount.ContainsKey(key)) playerTileCount[key] = 0;
                playerTileCount[key]++;
            }

        currentRowIndex = 0;

        foreach (var row in allRows)
            foreach (var img in row)
            {
                img.enabled = false;
                if (img.transform.childCount > 0)
                    img.transform.GetChild(0).gameObject.SetActive(false);
            }

        RefreshAnswerRow(currentRowIndex);
    }

    void OnScrollTileClicked(Tile tile)
    {
        string key = tile.GetSpriteName();

        // 同牌最多 4 張
        if (!playerTileCount.ContainsKey(key)) playerTileCount[key] = 0;
        if (playerTileCount[key] >= 4) return;

        // 找第一個空位
        for (int i = 0; i < 17; i++)
        {
            if (playerAnswers[i] == null)
            {
                playerAnswers[i] = tile;
                playerTileCount[key]++;
                RefreshAnswerRow(currentRowIndex);
                return;
            }
        }

        Debug.Log("答案已滿");
    }

    void RefreshAnswerRow(int rowIndex)
    {
        List<Image> row = allRows[rowIndex];

        for (int i = 0; i < 17; i++)
        {
            if (playerAnswers[i] == null)
            {
                row[i].enabled = false;
                if (row[i].transform.childCount > 0)
                    row[i].transform.GetChild(0).gameObject.SetActive(false);
                continue;
            }

            row[i].sprite = Resources.Load<Sprite>("Tiles/" + playerAnswers[i].GetSpriteName());
            row[i].enabled = true;

            // ★ 隱藏遮罩
            if (row[i].transform.childCount > 0)
                row[i].transform.GetChild(0).gameObject.SetActive(false);
        }
    }

    //提交答案
    public void OnSubmitAnswer()
    {
        for (int i = 0; i < 17; i++)
            if (playerAnswers[i] == null)
            {
                ShowMessage("答案尚未填滿");
                return;
            }

        if (!CanHu(new List<Tile>(playerAnswers)))
        {
            ShowMessage("無法胡牌！");
            return;
        }

        CompareAnswer();
    }

    void ShowMessage(string msg)
    {
        MessageBox.SetActive(true);
        Messagetext.text = msg;
    }

    //答案比對
    void CompareAnswer()
    {
         if (currentRowIndex >= allRows.Count)
        {
            ShowMessage("已達最大作答次數");
            return;
        }

        List<Image> row = allRows[currentRowIndex];
        Dictionary<string, int> count = new Dictionary<string, int>();
        foreach (Tile t in currentQuestion)
        {
            string k = t.GetSpriteName();
            if (!count.ContainsKey(k)) count[k] = 0;
                count[k]++;
        }

        //完全正確（藍色）
        for (int i = 0; i < 17; i++)
        {
            Tile p = playerAnswers[i];
            Tile q = currentQuestion[i];
            if (p.Suit == q.Suit && p.Value == q.Value)
            {
                lockedCorrectTiles[i] = p; // ★ 鎖定格子
                if (row[i].transform.childCount > 0)
                {
                    Image mask = row[i].transform.GetChild(0).GetComponent<Image>();
                    mask.color = new Color(0f, 0.5f, 1f, 0.4f);
                    mask.gameObject.SetActive(true);
                }
                count[p.GetSpriteName()]--;
            }
        }

        //牌對位置錯（黃色）
        for (int i = 0; i < 17; i++)
        {
            if (lockedCorrectTiles[i] != null) continue;

            string key = playerAnswers[i].GetSpriteName();
            if (count.ContainsKey(key) && count[key] > 0)
            {
                if (row[i].transform.childCount > 0)
                {
                    Image mask = row[i].transform.GetChild(0).GetComponent<Image>();
                    mask.color = new Color(1f, 1f, 0f, 0.4f);
                    mask.gameObject.SetActive(true);
                }
                count[key]--;
            }
        }

        //下一列立即生成答案區
        currentRowIndex++;
        if (currentRowIndex < allRows.Count)
        {
            //將已鎖定的藍色牌直接填入下一列
            for (int i = 0; i < 17; i++)
                playerAnswers[i] = lockedCorrectTiles[i];

            //計算每張牌剩餘可選數量
            playerTileCount.Clear();
            for (int i = 0; i < 17; i++)
            {
                if (playerAnswers[i] != null)
                {
                    string key = playerAnswers[i].GetSpriteName();
                    if (!playerTileCount.ContainsKey(key)) playerTileCount[key] = 0;
                    playerTileCount[key]++;
                }
            }

            RefreshAnswerRow(currentRowIndex);
        }
    }

    //胡牌判定
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
        Tile f = tiles[0];

        int same = tiles.FindAll(t => t.Suit == f.Suit && t.Value == f.Value).Count;
        if (same >= 3)
        {
            List<Tile> next = new List<Tile>(tiles);
            for (int i = 0; i < 3; i++)
                next.Remove(next.Find(t => t.Suit == f.Suit && t.Value == f.Value));
            if (CanFormMelds(next)) return true;
        }

        if (f.Suit != TileSuit.Honor)
        {
            Tile b = tiles.Find(t => t.Suit == f.Suit && t.Value == f.Value + 1);
            Tile c = tiles.Find(t => t.Suit == f.Suit && t.Value == f.Value + 2);
            if (b != null && c != null)
            {
                List<Tile> next = new List<Tile>(tiles);
                next.Remove(f);
                next.Remove(b);
                next.Remove(c);
                if (CanFormMelds(next)) return true;
            }
        }
        return false;
    }

    // 點擊刪除答案
    void BindAnswerSlotClick(List<Image> row)
    {
        for (int i = 0; i < row.Count; i++)
        {
            int index = i; // closure 保護
            Button btn = row[i].GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => OnAnswerSlotClicked(index));
        }
    }

    void OnAnswerSlotClicked(int index)
    {
        if (currentRowIndex >= allRows.Count) return;

        // 鎖定格不能刪
        if (lockedCorrectTiles[index] != null) return;

        if (playerAnswers[index] == null) return;

        string key = playerAnswers[index].GetSpriteName();

        if (playerTileCount.ContainsKey(key))
            playerTileCount[key]--;

        playerAnswers[index] = null;

        RefreshAnswerRow(currentRowIndex);
    }

    void ClearCurrentAnswer()
    {
        if (currentRowIndex >= allRows.Count) return;

        //先保留藍色鎖定牌
        for (int i = 0; i < 17; i++)
        {
            if (lockedCorrectTiles[i] != null)
                playerAnswers[i] = lockedCorrectTiles[i];
            else
                playerAnswers[i] = null;
        }

        //重新計算牌數量
        playerTileCount.Clear();
        for (int i = 0; i < 17; i++)
        {
            if (playerAnswers[i] != null)
            {
                string key = playerAnswers[i].GetSpriteName();
                if (!playerTileCount.ContainsKey(key)) playerTileCount[key] = 0;
                playerTileCount[key]++;
            }
        }

        //更新 UI
        RefreshAnswerRow(currentRowIndex);
    }
}