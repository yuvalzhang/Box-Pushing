using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class LevelSlot : UIBindable, IPointerClickHandler
{
    public int levelIndex;
    public LevelSO levelSo;
    public bool isLocked = false;
    private TMP_Text levelIndexText;

    protected override void Awake()
    {
        base.Awake();
        levelIndexText = GetControl<TMP_Text>("LevelIndexText");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //根据编号从关卡编辑器读取关卡地图数据
        levelSo = LevelManager.Instance.GetLevelData(levelIndex);
        if (levelSo == null)
        {
            Debug.LogError("Level not found");
            return;
        }
        
        GameManager.Instance.GameBegin(levelSo);
        //显示ui面板
        UIManager.Instance.ShowGamePanel();
    }

    public void SetLevelIndex(int index)
    {
        levelIndex = index;
        levelIndexText.text = levelIndex.ToString();
    }
}
