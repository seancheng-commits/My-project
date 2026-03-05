using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private Player _myPlayer;
    public Text Debugtext;

    public void Start()
    {
        // 這裡才是「建立玩家」的地方
        _myPlayer = new Player("勇者小明", 100f);
        
        Debug.Log("遊戲開始，玩家物件已在記憶體中建立！");
        
        // 測試看看
        _myPlayer.TakeDamage(10);
        _myPlayer.GainXp(120);

        Debugtext.text = "";
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.D))
        {
           Debug.Log($"目前生命值：{_myPlayer.Hp}");
        }

        if(Input.GetKeyDown(KeyCode.A))
        {
            _myPlayer.TakeDamage(10f);
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            _myPlayer.GainXp(10);
        }

        Debugtext.text = "玩家目前生命值 : "+ _myPlayer.Hp.ToString() + "目前經驗值 :" + _myPlayer.Xp.ToString() + "目前玩家等級 :" + -_myPlayer.Level;
    }
}
