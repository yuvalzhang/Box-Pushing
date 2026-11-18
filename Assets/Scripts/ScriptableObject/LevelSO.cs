using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_Index", menuName = "SO/LevelSO")]
public class LevelSO : ScriptableObject
{
    public int levelIndex;
    public string levelName;
    [HideInInspector] public int width = 10;
    [HideInInspector] public int height = 10;
    [HideInInspector] public int mapDataWidth;
    [HideInInspector] public int mapDataHeight;
    public int[] staticMapData = new int[100];
    public List<DynamicElements> dynamicMapData = new ();
}

[Serializable]
public class DynamicElements
{
    public Vector2Int GridPosition;
    public int elementIndex;
}
