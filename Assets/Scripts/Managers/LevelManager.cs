using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : SingletonMono<LevelManager>
{
    [SerializeField]private List<LevelSO> levelList = new ();
    [SerializeField]private int currentLevelIndex;

    public LevelSO GetLevelData(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levelList.Count) return null;
        currentLevelIndex = levelIndex;
        return levelList[levelIndex];
    }

    public LevelSO LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levelList.Count) return null;
        return levelList[currentLevelIndex];
    }
}
