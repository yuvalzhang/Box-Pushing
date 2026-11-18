
using UnityEngine;

public interface IMovable
{
    public Vector2 WorldPosition { get; set; }
    public Vector2Int GridPosition { get; set; }
    public bool IsMoving { get; set; }

    //使用dotween移动到目标位置
    public void MoveNext(Vector2 targetPos, Vector2Int stepInfo);
    public bool CanMove(Vector2Int stepInfo, Vector2Int dir);
    public void UndoMove(Vector2 targetPos, Vector2Int stepInfo);
}
