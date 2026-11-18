using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPanel : BasePanel
{
    protected override void OnClick(string objName)
    {
        switch (objName)
        {
            case "StartGame":
                UIManager.Instance.ShowLevelPanel();
                break;
                
            case "QuitGame":
                Application.Quit();
                break;
        }
    }
}
