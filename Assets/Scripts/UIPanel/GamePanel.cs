using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamePanel : BasePanel
{
    private TMP_Text stepCountText;
    public void OnEnable()
    {
        stepCountText = GetControl<TMP_Text>("CountNumber");
        stepCountText.text = "0";
    }

    protected override void OnClick(string objName)
    {
        switch (objName)
        {
            case "Withdraw": 
                StepManager.Instance.UndoStep();
                break;
            
            case "Restart":
                GameManager.Instance.RestartLevel();
                break;
                
            case "Forward":
                Debug.Log("Forward");
                break;
            
            case "Tip":
                Debug.Log("Tip");
                break;
            
            case "Quit":
                //重置关卡
                MapGenerator.Instance.UnloadCurrentMap();
                //清空步数
                StepManager.Instance.CleanStepCount();
                //返回到选择关卡界面
                UIManager.Instance.ShowLevelPanel();
                break;
        }
    }

    public void ShowStepCountText(int stepCount)
    {
        stepCountText.text = stepCount.ToString();
    }
}