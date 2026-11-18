using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : SingletonMono<MapGenerator>
{
    public GameObject[] mapElements;

    private Transform floor;
    private Transform block;
    private Transform flag;
    private Transform box;

    private GameObject tempGo;

    protected override void Awake()
    {
        base.Awake();
        floor = GameObject.Find("Floor").transform;
        block = GameObject.Find("Block").transform;
        flag = GameObject.Find("Flag").transform;
        box = GameObject.Find("Box").transform;
    }

    public void GenerateStaticMap(int[][] mapData, Vector2 originPoint)
    {
        //OriginPoint 决定哪个点在屏幕正中央(0, 0, 0)
        //双层for循环遍历
        for (int i = 0; i < mapData.Length; i++)
        {
            for (int j = 0; j < mapData[i].Length; j++)
            {
                int data = mapData[i][j];
                if (data == -1) continue;

                Vector2 targetPosition = new Vector2(originPoint.y + j, originPoint.x - i);
                GenerateItem(data, targetPosition);
                
            }
        }

        //生成玩家，允许输入
    }

    public IMovable GenerateDynamicEntities(Dictionary<Vector2Int, int> dynamicMap, Vector2 originPoint)
    {
        IMovable playerEntity = null; // 用于返回玩家
        foreach (KeyValuePair<Vector2Int, int> pair in dynamicMap)
        {
            int i = pair.Key.x;
            int j = pair.Key.y;
            int data = pair.Value;
            Vector2 targetPosition = new Vector2(originPoint.y + j, originPoint.x - i);
            
            IMovable movable = GenerateItem(data, targetPosition);
            
            GameManager.Instance.dynamicEntities[pair.Key] = movable;
            movable.GridPosition = new Vector2Int(i, j);
            movable.WorldPosition = targetPosition;
            if(data == 3)
                GameManager.Instance.boxList.Add(movable);
            if (data == 4)
                playerEntity = movable;

        }

        return playerEntity;
    }

    private IMovable GenerateItem(int data, Vector2 targetPosition)
    {
        //方法里局部变量如果是引用类型？
        Transform targetFather = transform;
        switch (data)
        {
            case 0: targetFather = floor;break;
            case 1: targetFather = block;break;
            case 2: targetFather = flag;break;
            case 3: targetFather = box;break;
            case 4: targetFather = box;break;
            
        }

        //声明到自己子物体下
        tempGo = Instantiate(mapElements[data], targetFather);
        tempGo.name = mapElements[data].name;
        tempGo.transform.position = targetPosition;
        if (data == 0) return null;
        if (data > 2)
        {
            return tempGo.GetComponent<IMovable>();
        }

        //再创建一个地形
        tempGo = Instantiate(mapElements[0], floor);
        tempGo.name = mapElements[0].name;
        tempGo.transform.position = targetPosition;
        return null;
    }

    public void ResetDynamicEntities()
    {
        
    }

    public void UnloadCurrentMap()
    {
        ClearChildren(floor);
        ClearChildren(block);
        ClearChildren(flag);
        ClearChildren(box);
    }
    
    public void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }



}
