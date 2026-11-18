using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCommand
{
    public Vector2Int playerFrom;
    public Vector2Int playerTo;
    public Vector2 playerWorldFrom;

    public IMovable pushedBox;
    public Vector2Int boxFrom;
    public Vector2Int boxTo;
    public Vector2 boxWorldFrom;

    public MoveCommand(Vector2Int pFrom, Vector2Int pTo, 
        Vector2 pWorldFrom, IMovable box = null, 
        Vector2Int bFrom = default, Vector2Int bTo = default, Vector2 bWorldFrom = default)
    {
        playerFrom = pFrom;
        playerTo = pTo;
        playerWorldFrom = pWorldFrom;
        pushedBox = box;
        boxFrom = bFrom;
        boxTo = bTo;
        boxWorldFrom = bWorldFrom;
    }
}

public class StepManager : SingletonMono<StepManager>
{
    public int currentPlayerStepCount = 0;
    public Stack<MoveCommand> commandStack = new ();
    
    public void RecordStep(MoveCommand cmd)
    {
        commandStack.Push(cmd);
        currentPlayerStepCount++;
        //检查当前的cmd有没有推开箱子，boxFrom是不是目标点
        if (cmd.pushedBox != null)
        {
            var gameManager = GameManager.Instance;

            if (gameManager.targetFlag.Contains(cmd.boxTo))
            {
                gameManager.boxList.Remove(cmd.pushedBox);
            }
            else if (!gameManager.boxList.Contains(cmd.pushedBox))
            {
                gameManager.boxList.Add(cmd.pushedBox);
            }
        }

        UIManager.Instance.GetGamePanel().ShowStepCountText(currentPlayerStepCount);
    }

    public void UndoStep()
    {
        if (commandStack.Count == 0) return;
        
        MoveCommand lastCmd = commandStack.Pop();
        Debug.Log(lastCmd.playerWorldFrom);
        GameManager.Instance.currentPlayer.UndoMove(lastCmd.playerWorldFrom, lastCmd.playerFrom);

        if (lastCmd.pushedBox != null)
        {
            lastCmd.pushedBox.UndoMove(lastCmd.boxWorldFrom, lastCmd.boxFrom);

            var gameManager = GameManager.Instance;

            if (gameManager.targetFlag.Contains(lastCmd.boxFrom))
            {
                gameManager.boxList.Remove(lastCmd.pushedBox);
            }
            else if (!gameManager.boxList.Contains(lastCmd.pushedBox))
            {
                gameManager.boxList.Add(lastCmd.pushedBox);
            }
        }
        
        //更新地图网格
        
        currentPlayerStepCount--;
        UIManager.Instance.GetGamePanel().ShowStepCountText(currentPlayerStepCount);
    }

    public void CleanStepCount()
    {
        currentPlayerStepCount = 0;
        //清空栈
        commandStack.Clear();

        UIManager.Instance.GetGamePanel().ShowStepCountText(currentPlayerStepCount);
    }
}
