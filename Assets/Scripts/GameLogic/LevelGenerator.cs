using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject levelSlotPrefab;

    public void OnEnable()
    {
        if (transform.childCount > 0) return;
        GameObject child = null;
        for (int i = 0; i < 9; i++)
        {
            child = Instantiate(levelSlotPrefab, transform);
            int index = i + 1;
            child.name = levelSlotPrefab.name + "_" + index;
            child.GetComponent<LevelSlot>().SetLevelIndex(index);
        }
    }

    public void OnDisable()
    {
        
    }
}
