using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : SingletonMono<UIManager>
{
    [Header("UI 面板引用")]
    public BasePanel startPanel;
    public BasePanel levelPanel;
    public BasePanel gamePanel;
    public BasePanel victoryPanel;
   
    private void Start()
    {
        // 游戏一开始，只显示主菜单
        // ShowStartPanel();
    }
    
    public void ShowStartPanel()
    {
        startPanel.ShowMe();
        levelPanel.HideMe();
        gamePanel.HideMe();
        victoryPanel.HideMe();
    }
    public void ShowLevelPanel()
    {
        startPanel.HideMe();
        levelPanel.ShowMe();
        gamePanel.HideMe();
        victoryPanel.HideMe();
        
    }
    public void ShowGamePanel()
    {
        startPanel.HideMe();
        levelPanel.HideMe();
        gamePanel.ShowMe();
        victoryPanel.HideMe();
        
    }

    public void ShowVictoryPanel()
    {
        startPanel.HideMe();
        levelPanel.HideMe();
        // gamePanel.HideMe();
        victoryPanel.ShowMe();
    }
    
    #region GetPanel

    public GamePanel GetGamePanel()
    {
        if (gamePanel == null)
            return null;
        return gamePanel as GamePanel;
    }

    #endregion
}
