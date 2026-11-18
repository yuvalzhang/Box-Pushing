using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IMovable
{
    // public Vector2 currentWorldPosition;

    public Vector2 WorldPosition { get; set; }
    public Vector2Int GridPosition { get; set; }
    public Vector2Int gridPosition;
    public Vector2 worldPosition;
    
    public bool IsMoving { get; set; }
    public float moveDuration = 0.4f;
    
    private void Update()
    {
        gridPosition = GridPosition;
        worldPosition = WorldPosition;
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

        IMovable entity = null;
        //增加步数
        MoveCommand cmd = new MoveCommand(GridPosition, stepInfo, WorldPosition);
        if (GameManager.Instance.dynamicEntities.ContainsKey(stepInfo))
        {
            entity = GameManager.Instance.dynamicEntities[stepInfo];
            Vector2Int newTargetPos = stepInfo + dir;
            if (!entity.CanMove(newTargetPos, dir))
            {
                //看对应方向还有没有箱子
                return false;
            }
            cmd = new MoveCommand(GridPosition, stepInfo, WorldPosition, entity, stepInfo, stepInfo + dir, entity.WorldPosition);
            Vector2Int worldDir = GameManager.Instance.GetWorldPos(dir);
            entity.MoveNext(entity.WorldPosition + worldDir, newTargetPos);
        }
        
        StepManager.Instance.RecordStep(cmd);

        return true;
    }

    public void UndoMove(Vector2 targetPos, Vector2Int stepInfo)
    {
        MoveNext(targetPos, stepInfo);
        //减去步数
    }

    #region Move Methods
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
                // GameManager.Instance.CheckLevelComplete();
                
            }
        );
        
        //移动成功，更新位置
        GridPosition = stepInfo;
        
        //注册新的位置
        GameManager.Instance.dynamicEntities[GridPosition] = this; 
    }
    
    public void MoveUp()
    {
        Vector2 targetPosition = new Vector2(WorldPosition.x, WorldPosition.y + 1);
        //检验目标位置是否合法
        Vector2Int targetStep = new Vector2Int(GridPosition.x - 1, GridPosition.y);
        if (CanMove(targetStep, new Vector2Int(-1, 0))) 
        //如果合法，是否可以推动箱子
            MoveNext(targetPosition, targetStep);
    }

    public void MoveDown()
    {
        Vector2 targetPosition = new Vector2(WorldPosition.x, WorldPosition.y - 1);
        Vector2Int targetStep = new Vector2Int(GridPosition.x + 1, GridPosition.y);
        
        if (CanMove(targetStep, new Vector2Int(1, 0))) 
            //如果合法，是否可以推动箱子
            MoveNext(targetPosition, targetStep);
    }

    public void MoveLeft()
    {
        Vector2 targetPosition = new Vector2(WorldPosition.x - 1, WorldPosition.y);
        Vector2Int targetStep = new Vector2Int(GridPosition.x, GridPosition.y - 1);
        
        if (CanMove(targetStep, new Vector2Int(0,-1))) 
            //如果合法，是否可以推动箱子
            MoveNext(targetPosition, targetStep);
    }

    public void MoveRight()
    {
        Vector2 targetPosition = new Vector2(WorldPosition.x + 1, WorldPosition.y);
        Vector2Int targetStep = new Vector2Int(GridPosition.x, GridPosition.y + 1);
        
        if (CanMove(targetStep, new Vector2Int(0,1))) 
            //如果合法，是否可以推动箱子
            MoveNext(targetPosition, targetStep);
    }
    #endregion
}
