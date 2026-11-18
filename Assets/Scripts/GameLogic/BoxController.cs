using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class BoxController : MonoBehaviour, IMovable
{
    public Vector2 WorldPosition { get; set; }
    public Vector2Int GridPosition { get; set; }
    //用作调试
    public Vector2Int gridPosition;
    public Vector2 worldPosition;
    
    public bool IsMoving { get; set; }
    public float moveDuration = 0.4f;
    
    private void Update()
    {
        gridPosition = GridPosition;
        worldPosition = WorldPosition;
    }

    public void MoveNext(Vector2 targetPos, Vector2Int stepInfo)
    {
        if (IsMoving) return;
        IsMoving = true;
        GameManager.Instance.dynamicEntities.Remove(GridPosition);
        //Dotween移动
        transform.DOMove(targetPos, moveDuration).OnComplete(() =>
            {
                WorldPosition = targetPos;
                IsMoving = false;
                GameManager.Instance.CheckLevelComplete();
            }
        );
        //移动成功，更新位置
        GridPosition = stepInfo;
        //注册新的位置
        GameManager.Instance.dynamicEntities[GridPosition] = this; 
    }

    public bool CanMove(Vector2Int stepInfo, Vector2Int dir)
    {
        if (IsMoving) return false;
        
        int data = GameManager.Instance.GetStaticGridData(stepInfo.x, stepInfo.y);
        switch (data)
        {
            case -1:
                Debug.Log("空地无法行走");return false;
            case 1:
                Debug.Log("遇到Block");return false;
        }

        if (GameManager.Instance.dynamicEntities.ContainsKey(stepInfo))
        {
            // IMovable entity = GameManager.Instance.dynamicEntities[stepInfo];
            // Vector2Int newTargetPos = stepInfo + dir;
            // if (entity.CanMove(newTargetPos, dir))
            // {
            //     //看对应方向还有没有箱子
            //     Vector2Int worldDir = GameManager.Instance.GetWorldPos(dir);
            //     entity.MoveNext(entity.WorldPosition + worldDir, newTargetPos);
            //     return true;
            // }
            return false;
            
        }

        return true;
    }

    public void UndoMove(Vector2 targetPos, Vector2Int stepInfo)
    {
        MoveNext(targetPos, stepInfo);
    }
    
}
