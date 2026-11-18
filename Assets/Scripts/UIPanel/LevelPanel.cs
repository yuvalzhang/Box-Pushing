using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPanel : BasePanel
{
    //当前关卡页数
    public int currentTab;
    //如果关卡数量过多，需要复用当前的LevelSlot物体
    protected override void OnClick(string objName)
    {
        //读取关卡配置的数据，跳转到对应的关卡
        switch (objName)
        {
            case "Quit":
                //返回到开始界面
                UIManager.Instance.ShowStartPanel();
                break;
        }
    }
}
