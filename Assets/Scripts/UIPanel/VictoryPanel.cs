using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryPanel :  BasePanel
{
    protected override void OnClick(string objName)
    {
        //保存已通关的数据（使用的步数）
        
        //读取关卡配置的数据，跳转到对应的关卡
        switch (objName)
        {
            case "QuitToMenu":
                //卸载地图
                MapGenerator.Instance.UnloadCurrentMap();
                //清空步数
                StepManager.Instance.CleanStepCount();
                //返回到开始界面
                UIManager.Instance.ShowLevelPanel();
                break;
            case "RestartLevel":
                //清空步数
                StepManager.Instance.CleanStepCount();
                GameManager.Instance.RestartLevel();
                UIManager.Instance.ShowGamePanel();
                break;
            case "NextLevel":
                LevelSO nextLevel = LevelManager.Instance.LoadNextLevel();
                if (nextLevel == null) return;
                //卸载地图
                MapGenerator.Instance.UnloadCurrentMap();
                //清空步数
                StepManager.Instance.CleanStepCount();
                //加载下一关
                GameManager.Instance.GameBegin(nextLevel);
                UIManager.Instance.ShowGamePanel();
                break;
        }
    }
}
