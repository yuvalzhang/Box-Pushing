using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEntrance : MonoBehaviour
{
     public bool debugMod;
    public bool skipPrepare;
    
    // 集中管理所有需要动态创建的 Manager 类型
    // 使用 Type 比字符串更安全，且使用 IDE 重命名类时这里会自动同步
    private readonly Type[] managerTypes = new Type[]
    {
        typeof(GameManager),
        typeof(InputManager),
        // typeof(SceneMgr),
        typeof(UIManager),
        // typeof(TimeMgr),
        // typeof(ResourceMgr),
        // typeof(AudioMgr),
        // typeof(PoolManager),
        // typeof(VFXManager),
        typeof(EventCenter),
        // typeof(GlobalEventLibrary),
        // typeof(MobileInputBridge),
        
    };

    void Awake()
    {
        // InitializeManagers();
        // InitializeSpecialObjects();
    }

    private void Start()
    {
        // GameManager.Instance.OnAllManagersReady(); 
        UIManager.Instance.ShowStartPanel();
    }

    private void InitializeManagers()
    {
        foreach (Type type in managerTypes)
        {
            GameObject managerObj = new GameObject(type.Name);
            Component comp = managerObj.AddComponent(type);
            managerObj.transform.SetParent(this.transform); 

            // 在 GameManager 刚被挂载时，立刻修改它的状态
            // 这样它在下一帧执行自己的 Start() 时，就不会去加载 MainScene 了
            // if (type == typeof(GameManager) && debugMod)
            // {
            //     GameManager gm = (GameManager)comp;
            //     gm.isTesting = true;
            //     gm.skipPrepare = skipPrepare; // 新增一个标志位，告诉它跳过倒计时
            // }
        }
    }

    private void InitializeSpecialObjects()
    {
        // 1. 处理 Debugger 和 特殊的子模块
        if (debugMod)
        {
            // GameObject debuggerObj = new GameObject("Debugger");
            //
            // GameObject gpuCheck = new GameObject("GPUCheck");
            // gpuCheck.AddComponent<GPUCheck>();
            // gpuCheck.transform.SetParent(debuggerObj.transform); // 归类存放
        }

        // 2. 处理 AudioListener 
        // new GameObject("AudioListener").AddComponent<AudioListener>().transform.SetParent(this.transform);

        // 3. 处理 FX 节点 (假设它只是个空节点用来挂载特效)
        new GameObject("FX").transform.SetParent(this.transform);
    }
}
