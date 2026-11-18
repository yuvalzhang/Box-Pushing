using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameManager : SingletonMono<GameManager>
{
    public Vector2 mapOffset;
    
    public int[][] staticMap;
    public Dictionary<Vector2Int, int> dynamicMap = new();
    public Dictionary<Vector2Int, IMovable> dynamicEntities = new();
    public IMovable currentPlayer;
    public LevelSO currentLevel;

    private Dictionary<Vector2Int, Vector2Int> gridWorldDirTable = new();
    private Dictionary<Vector2Int, Vector2Int> worldGridDirTable = new();

    public HashSet<Vector2Int> targetFlag = new();
    public List<IMovable> boxList = new();
    

    protected override void Awake()
    {
        base.Awake();
        InputManager.Instance.InitInputAction();
        InitGridWorldDirTable();
    }

    public void OnEnable()
    {
        InputManager.Instance.EnableUIInputMap();
        
    }

    public void GameBegin(LevelSO levelSo)
    {
        InputManager.Instance.EnablePlayerInputMap();
        currentLevel = levelSo;
        GenerateLevelMap(levelSo);
    }

    #region GameMap
    public void UpdateCurrentMap()
    {
        
    }

    private void InitGridWorldDirTable()
    {
        gridWorldDirTable = new()
        {
            {new Vector2Int(0, 1), new Vector2Int(1, 0)},
            {new Vector2Int(0,-1), new Vector2Int(-1, 0)},
            {new Vector2Int(-1, 0), new Vector2Int(0, 1)},
            {new Vector2Int(1, 0), new Vector2Int(0, -1)},
            
        };
        worldGridDirTable = new()
        {
            { new Vector2Int(1, 0), new Vector2Int(0, 1) },
            { new Vector2Int(-1, 0), new Vector2Int(0, -1) },
            { new Vector2Int(0, 1), new Vector2Int(-1, 0) },
            { new Vector2Int(0, -1), new Vector2Int(1, 0) },
        };
    }

    public Vector2Int GetGridPos(Vector2Int worldDir)
    {
        return worldGridDirTable[worldDir];
    }

    public Vector2Int GetWorldPos(Vector2Int gridDir)
    {
        return gridWorldDirTable[gridDir];
    }
    
    private void OrganizeMapData(LevelSO levelSo)
    {
        // 1. 清理上一关的残留数据 (极其重要！)
        targetFlag.Clear();
        boxList.Clear();
        dynamicEntities.Clear();
        dynamicMap.Clear(); // 如果你还需要用它的话
        MapGenerator.Instance.UnloadCurrentMap(); // 假设这个方法能清空场景里的预制体
        
        int w = levelSo.width;
        int h = levelSo.height;

        staticMap = new int[h][];
        for (int i = 0; i < h; i++)
        {
            staticMap[i] = new int[w];
        }

        for (int i = 0; i < levelSo.staticMapData.Length; i++)
        {
            int x = i % w;
            int y = i / w;
            int data = levelSo.staticMapData[i];
            staticMap[y][x] = data;
            if (data == 2)
            {
                targetFlag.Add(new Vector2Int(y, x));
            }
        }

        foreach (var element in levelSo.dynamicMapData)
        {
            Vector2Int invertedPos = new Vector2Int(element.GridPosition.y, element.GridPosition.x);
            dynamicMap[invertedPos] = element.elementIndex;
        }
    }

    private void GenerateLevelMap(LevelSO levelSo)
    {
        OrganizeMapData(levelSo);
        
        int rows = staticMap.Length;
        int columns = staticMap[0].Length;
        
        Vector2 originPoint = new Vector2(rows * mapOffset.x, columns * mapOffset.y);
        MapGenerator.Instance.GenerateStaticMap(staticMap, originPoint);
        currentPlayer = MapGenerator.Instance.GenerateDynamicEntities(dynamicMap, originPoint);
    }
    #endregion

    #region Level Complete
    public void CheckLevelComplete()
    {
        if(IsLevelComplete())
            LevelDone();
    }

    private bool IsLevelComplete()
    {
        boxList.RemoveAll(box => targetFlag.Contains(box.GridPosition));
        return boxList.Count == 0;
    }

    private void LevelDone()
    {
        Debug.Log("Level done");
        //保存通关进度，通关所耗费的步数
        
        //禁止玩家输入
        InputManager.Instance.DisablePlayerInputMap();
        //弹出UI
        UIManager.Instance.ShowVictoryPanel();
    }
    #endregion
    
    public int GetStaticGridData(int x, int y)
    {
        return staticMap[x][y];
    }

    public void RestartLevel()
    {
        if (currentLevel == null) return;
        
        Debug.Log("Restart Successful!");
        InputManager.Instance.DisablePlayerInputMap();
        MapGenerator.Instance.UnloadCurrentMap();
        StepManager.Instance.CleanStepCount();
        GenerateLevelMap(currentLevel);
        InputManager.Instance.EnablePlayerInputMap();
    }
}

