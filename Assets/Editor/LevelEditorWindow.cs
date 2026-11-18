using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private LevelSO currentLevel;
    private int currentBrush = 0; 
    private readonly string[] brushNames = { "空地(0)", "墙壁(1)", "目标点(2)", "箱子(3)", "玩家(4)", "橡皮擦(-1)" };
    
    private readonly Color[] brushColors = { 
        new Color(0.6f, 0.8f, 1f),  // 0: 空地 (蓝色)
        new Color(0.7f, 0.4f, 1f),  // 1: 墙壁 (紫色)
        new Color(0.5f, 1f, 0.5f),  // 2: 目标点 (浅绿)
        new Color(1f, 0.8f, 0f),    // 3: 箱子 (橘黄)
        new Color(1f, 0.4f, 0.4f)   // 4: 玩家 (红色)
    };
    

    [MenuItem("Level Tools/关卡编辑器")]
    public static void ShowWindow() => GetWindow<LevelEditorWindow>("关卡编辑器");

    //窗口失去焦点时自动保存
    private void OnLostFocus()
    {
        if (currentLevel != null)
        {
            EditorUtility.SetDirty(currentLevel);
            AssetDatabase.SaveAssets();
        }
    }

    //核心的绘制方法
    private void OnGUI()
    {
        GUILayout.Label("1. 关卡数据文件", EditorStyles.boldLabel);
        
        EditorGUI.BeginChangeCheck();
        currentLevel = (LevelSO)EditorGUILayout.ObjectField("Level SO", currentLevel, typeof(LevelSO), false);
        if (EditorGUI.EndChangeCheck()) GUI.FocusControl(null); 

        if (currentLevel == null)
        {
            EditorGUILayout.HelpBox("请挂载一个 LevelSO 文件", MessageType.Warning);
            return;
        }
        
        //记录地图尺寸的旧关卡数据
        if ((currentLevel.mapDataWidth <= 0 || currentLevel.mapDataHeight <= 0) &&
            currentLevel.staticMapData != null &&
            currentLevel.staticMapData.Length == currentLevel.width * currentLevel.height)
        {
            currentLevel.mapDataWidth = currentLevel.width;
            currentLevel.mapDataHeight = currentLevel.height;
            EditorUtility.SetDirty(currentLevel);
        }

        //地图尺寸设置
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        currentLevel.width = EditorGUILayout.IntSlider("地图宽度", currentLevel.width, 5, 20);
        currentLevel.height = EditorGUILayout.IntSlider("地图高度", currentLevel.height, 5, 20);
        GUILayout.EndHorizontal();

        int expectedSize = currentLevel.width * currentLevel.height;
        bool dimensionsChanged =
            currentLevel.width != currentLevel.mapDataWidth ||
            currentLevel.height != currentLevel.mapDataHeight;

        if (dimensionsChanged ||
            currentLevel.staticMapData == null ||
            currentLevel.staticMapData.Length != expectedSize)
        {
            EditorGUILayout.HelpBox("地图尺寸已改变，必须重置网格数据！旧数据将被清空", MessageType.Error);

            if (GUILayout.Button("重置网格数据", GUILayout.Height(30)))
            {
                Undo.RecordObject(currentLevel, "Reset Grid Data");

                currentLevel.staticMapData = new int[expectedSize];
                for (int i = 0; i < currentLevel.staticMapData.Length; i++)
                    currentLevel.staticMapData[i] = -1;

                currentLevel.dynamicMapData.Clear();

                currentLevel.mapDataWidth = currentLevel.width;
                currentLevel.mapDataHeight = currentLevel.height;

                EditorUtility.SetDirty(currentLevel);
                AssetDatabase.SaveAssets();
            }

            return;
        }

        //功能区
        GUILayout.Space(10);
        GUILayout.Label("2. 功能区", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        for (int i = 0; i < brushNames.Length; i++)
        {
            GUI.backgroundColor = currentBrush == i ? Color.green : Color.white;
            if (GUILayout.Button(brushNames[i])) currentBrush = i;
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        
        
        GUILayout.BeginVertical("box");
        
        //撤销按钮
        GUI.backgroundColor = new Color(0.8f, 0.8f, 1f);
        if (GUILayout.Button("撤销上一步操作", GUILayout.Height(25)))
        {
            //主动触发 Unity 全局撤销
            Undo.PerformUndo(); 
            //撤销后必须重绘窗口，否则画面不会立刻更新
            Repaint(); 
        }
        
        //智能墙壁按钮
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("智能生成紧密包围墙壁", GUILayout.Height(25)))
        {
            GenerateSmartWalls();
        }
        // 一键补充空地按钮
        GUI.backgroundColor = Color.white;
        if (GUILayout.Button("一键补充所有空地", GUILayout.Height(25)))
        {
            FillEmptyCells();
        }
        //强制清空按钮
        GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); 
        if (GUILayout.Button("强制清空整个地图", GUILayout.Height(25)))
        {
            if (EditorUtility.DisplayDialog("确认清空", "确定要删除地图上的所有元素吗？清空后可使用 Ctrl+Z 撤销。", "确定", "取消"))
            {
                ClearAllData();
            }
        }
        
        GUILayout.EndVertical();
        GUI.backgroundColor = Color.white;
        

        //网格绘制区
        GUILayout.Space(15);
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace(); 
        GUILayout.BeginVertical("box");
        
        Event e = Event.current;
        
        for (int y = 0; y < currentLevel.height; y++)
        {
            GUILayout.BeginHorizontal();
            for (int x = 0; x < currentLevel.width; x++)
            {
                int index = y * currentLevel.width + x;
                Vector2Int pos = new Vector2Int(x, y);
                
                int displayData = currentLevel.staticMapData[index];
                foreach (var d in currentLevel.dynamicMapData)
                    if (d.GridPosition == pos) { displayData = d.elementIndex; break; }
                // 1. 预留出格子位置 (这里依然用 Rect 获取位置)
                Rect rect = GUILayoutUtility.GetRect(30, 30);
                
                //留出间隙
                float padding = 2f;
                Rect innerRect = new Rect(
                    rect.x + padding, 
                    rect.y + padding, 
                    rect.width - padding * 2, 
                    rect.height - padding * 2
                );
                // --- 2. 提取底层(静态)和上层(动态)数据 ---
                int staticData = currentLevel.staticMapData[index];
                int dynamicData = -1; // 默认没有动态物体
                foreach (var d in currentLevel.dynamicMapData)
                {
                    if (d.GridPosition == pos) 
                    { 
                        dynamicData = d.elementIndex; 
                        break; 
                    }
                }
                
                //双层绘制
                //画静态背景
                Color staticColor = (staticData == -1) ? Color.black : brushColors[staticData];
                EditorGUI.DrawRect(innerRect, staticColor);
                //如果这上面有玩家或箱子等动态元素，在内部画一个稍微小一点的色块，露出底层的边缘
                if (dynamicData != -1)
                {
                    //动态物体比底下的地砖小一圈
                    float dynamicPadding = 4f; 
                    Rect dynamicRect = new Rect(
                        innerRect.x + dynamicPadding, 
                        innerRect.y + dynamicPadding, 
                        innerRect.width - dynamicPadding * 2, 
                        innerRect.height - dynamicPadding * 2
                    );
                    
                    Color dynamicColor = brushColors[dynamicData];
                    EditorGUI.DrawRect(dynamicRect, dynamicColor);
                }
                
                if ((e.type == EventType.MouseDown || e.type == EventType.MouseDrag) && rect.Contains(e.mousePosition))
                {
                    if (e.button == 0)
                    {
                        PaintGrid(index, pos);
                        e.Use();
                    }
                }
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
        GUILayout.FlexibleSpace(); 
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;

        //校验
        GUILayout.Space(15);
        ValidateLevel();
    }

    private void PaintGrid(int index, Vector2Int pos)
    {
        Undo.RecordObject(currentLevel, "Paint Grid"); 
        
        int brushValue = (currentBrush == 5) ? -1 : currentBrush; 
        //分层擦除
        if (brushValue == -1)
        {
            //先尝试移除动态层
            int removedCount = currentLevel.dynamicMapData.RemoveAll(d => d.GridPosition == pos);
            
            //如果没有动态物体，才去擦静态层
            if (removedCount == 0)
            {
                currentLevel.staticMapData[index] = -1; // 彻底清除
            }
        }
        //静态笔刷
        else if (brushValue <= 2) 
        {
            //静态笔刷强制所有碍事的动态物体
            currentLevel.dynamicMapData.RemoveAll(d => d.GridPosition == pos);
            currentLevel.staticMapData[index] = brushValue;
        }
        //动态笔刷
        else 
        {
            //先清空旧的动态物体
            currentLevel.dynamicMapData.RemoveAll(d => d.GridPosition == pos);
            
            // 检查脚下。如果是墙(1) 或 虚无(-1)，强行垫一块平地(0)。保留目标点(2)和平地(0)
            if (currentLevel.staticMapData[index] == 1 || currentLevel.staticMapData[index] == -1) 
            {
                currentLevel.staticMapData[index] = 0;
            }
            
            //添加入动态层
            currentLevel.dynamicMapData.Add(new DynamicElements() { GridPosition = pos, elementIndex = brushValue });
        }
        EditorUtility.SetDirty(currentLevel);
    }

    //智能紧密包围
    private void GenerateSmartWalls()
    {
        //记录生成墙壁前的状态
        Undo.RecordObject(currentLevel, "Generate Smart Walls"); 
        
        int w = currentLevel.width;
        int h = currentLevel.height;
        bool hasAnyElement = false;

        //扫描地图，寻找所有有效元素（包含静态和动态，只要不是-1和墙壁）
        HashSet<Vector2Int> validElements = new HashSet<Vector2Int>();
        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                int index = y * w + x;
                //如果是墙壁，则忽略
                if (currentLevel.staticMapData[index] == 1) 
                {
                    currentLevel.staticMapData[index] = -1;
                }
                
                int data = currentLevel.staticMapData[index];
                if (data != -1)
                {
                    validElements.Add(new Vector2Int(x, y));
                    hasAnyElement = true;
                }
            }
        }

        //也把动态层元素算作有效元素
        foreach (var d in currentLevel.dynamicMapData)
        {
            validElements.Add(d.GridPosition);
            hasAnyElement = true;
        }

        //如果地图没有任何元素，就在最外围生成一圈墙壁兜底
        if (!hasAnyElement)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x == 0 || x == w - 1 || y == 0 || y == h - 1)
                    {
                        currentLevel.staticMapData[y * w + x] = 1;
                    }
                }
            }
            EditorUtility.SetDirty(currentLevel);
            return;
        }

        //如果有元素，寻找紧密边缘（八向膨胀）
        HashSet<Vector2Int> wallPositions = new HashSet<Vector2Int>();
        //八向膨胀用到的八个方向偏移量
        Vector2Int[] dirs = { 
            new Vector2Int(0,1), new Vector2Int(0,-1), new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(1,1), new Vector2Int(1,-1), new Vector2Int(-1,1), new Vector2Int(-1,-1)
        };

        foreach (var pos in validElements)
        {
            foreach (var dir in dirs)
            {
                Vector2Int neighbor = pos + dir;
                //确保邻居在地图范围内
                if (neighbor.x >= 0 && neighbor.x < w && neighbor.y >= 0 && neighbor.y < h)
                {
                    //如果邻居不是有效元素，就应该变成墙壁
                    if (!validElements.Contains(neighbor))
                    {
                        wallPositions.Add(neighbor);
                    }
                }
            }
            
            //如果有效元素在地图边缘，强行在它所在的位置覆盖一面墙壁
            if (pos.x == 0 || pos.x == w - 1 || pos.y == 0 || pos.y == h - 1)
            {
                wallPositions.Add(pos); 
            }
        }

        //写入墙壁
        foreach (var wp in wallPositions)
        {
            int index = wp.y * w + wp.x;
            currentLevel.staticMapData[index] = 1; 
            //如果填墙壁的位置原本有动态物体，必须清理掉
            currentLevel.dynamicMapData.RemoveAll(d => d.GridPosition == wp);
        }

        EditorUtility.SetDirty(currentLevel);
        AssetDatabase.SaveAssets(); // 立即保存，防止丢失
    }
    
    private void FillEmptyCells()
    {
        int[] mapData = currentLevel.staticMapData;
        int emptyCount = 0;

        // 先确认是否有需要填充的格子，避免产生无意义的撤销记录。
        for (int i = 0; i < mapData.Length; i++)
        {
            if (mapData[i] == -1)
            {
                emptyCount++;
            }
        }

        if (emptyCount == 0)
        {
            return;
        }

        Undo.RecordObject(currentLevel, "Fill Empty Cells");

        for (int i = 0; i < mapData.Length; i++)
        {
            if (mapData[i] == -1)
            {
                mapData[i] = 0;
            }
        }

        EditorUtility.SetDirty(currentLevel);
        AssetDatabase.SaveAssets();
    }

    //检查关卡是否合法
    private void ValidateLevel()
    {
        int playerCount = 0;
        HashSet<Vector2Int> boxPositions = new HashSet<Vector2Int>();
        
        foreach (var d in currentLevel.dynamicMapData)
        {
            if (d.elementIndex == 4) playerCount++;
            if (d.elementIndex == 3) boxPositions.Add(d.GridPosition); // 放入 HashSet 去重
        }
        int targetCount = 0;
        for (int i = 0; i < currentLevel.staticMapData.Length; i++)
        {
            if (currentLevel.staticMapData[i] == 2) targetCount++;
        }
        //核心判断
        if (playerCount != 1) 
            EditorGUILayout.HelpBox($"错误：需要 1 个玩家 (当前: {playerCount})", MessageType.Error);
        else if (boxPositions.Count == 0 || targetCount == 0) 
            EditorGUILayout.HelpBox("错误：箱子和目标点数量不能为 0！", MessageType.Error);
        else if (boxPositions.Count != targetCount) 
            EditorGUILayout.HelpBox($"警告：箱子数({boxPositions.Count})与目标点数({targetCount})不一致！", MessageType.Warning);
        else 
            EditorGUILayout.HelpBox("关卡状态：合法，随时可以运行！", MessageType.Info);
    }
    
    //清空所有数据
    private void ClearAllData()
    {
        //记录清空前的状态
        Undo.RecordObject(currentLevel, "Clear All Data");
        
        int size = currentLevel.width * currentLevel.height;
        currentLevel.staticMapData = new int[size];
        for (int i = 0; i < size; i++) currentLevel.staticMapData[i] = -1;
        currentLevel.dynamicMapData.Clear();
        
        EditorUtility.SetDirty(currentLevel);
        AssetDatabase.SaveAssets();
    }
}