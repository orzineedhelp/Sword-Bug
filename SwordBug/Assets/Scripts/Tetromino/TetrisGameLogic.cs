using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TetrisGameLogic : MonoBehaviour
{
    [Header("生成设置")]
    [Tooltip("The Current Tetris User can control")]
    public TetrisOp CurrentTetris;
    [Tooltip("The Tetris moving down speed-DownSpeed X 1 Cells per Second")]
    public float DownSpeed = 1.0f;
    private float LastDownTime;
    // private bool IsNextPositionsCleared = true;//true-clear last transformed positions before move the tetris.false won't clear it.
    public TetrisClearLine TCL;
    public TetrisGameOver TGO;
    public TetrisOp[] AllTetrisCanUse;
    Vector3Int _Offset;
    public bool isOver;

    [Header("游戏启动设置")]
    public float startDelay = 3f; // 游戏开始后延迟几秒开始生成方块
    private bool gameStarted = false;
    private float gameStartTime;
    [Header("audio")]
    public AudioDefination audioHit;


    [Header("碰撞设置")]
    public float horizontalMoveCooldown = 0.5f;
    public float verticalMoveCooldown = 0.2f;
    private float lastHorizontalCollisionTime = 0f;
    private float lastVerticalCollisionTime = 0f;
    private Vector3Int lastCollisionCell = Vector3Int.zero;

    [Header("旋转设置")]
    public bool enableRandomRotation = true;
    public int maxRandomRotations = 4; // 最大随机旋转次数
    [Header("特殊方块设置")]
    public int specialTetrisIndex = 2;  // 特殊方块的索引（j=2）
    public bool requireNoRotation = true; // 是否需要无旋转状态


    void Start()
    {
        gameStartTime = Time.time;//等待延迟后生成方块
        //RandomChooseCurrentTetris();
        LastDownTime = Time.time;
    }

    void RandomChooseCurrentTetris()
    {
        int i = AllTetrisCanUse.Length;
        int j = Random.Range(0, i);
        CurrentTetris = AllTetrisCanUse[j];
        // 检查是否是特殊方块
        bool isSpecialTetris = (j == specialTetrisIndex);

        // 在生成前随机旋转（特殊方块可能不需要旋转）
        if (enableRandomRotation)
        {
            if (isSpecialTetris && requireNoRotation)
            {
                CurrentTetris.SetCollectable(true);
                Debug.Log($"生成了可收集的特殊方块: {CurrentTetris.name}");
                return;
            }
            else
            {
                ApplyRandomRotation(CurrentTetris);
            }
        }

        CurrentTetris.Reset();
        EnsureValidSpawnPosition();
    }


    // 应用随机旋转
    private void ApplyRandomRotation(TetrisOp tetris)
    {
        if (tetris.TransformPositions.Length <= 1) return;

        int randomRotations = Random.Range(0, maxRandomRotations + 1);

        for (int i = 0; i < randomRotations; i++)
        {
            tetris.TransformToNext();
        }

        //  Debug.Log($"俄罗斯方块生成时旋转了 {randomRotations} 次");
    }

    // 确保生成位置有效
    private void EnsureValidSpawnPosition()
    {
        // 检查当前旋转状态下的生成位置是否有效
        if (!CurrentTetris.IsTransformedPositionsCleared(
            CurrentTetris.OffsetAtTileMap,
            CurrentTetris.OriginPositionAtTileMap))
        {
            // 如果无效，尝试其他旋转状态
            TryFindValidRotation();
        }
    }
    // 尝试找到有效的旋转状态
    private void TryFindValidRotation()
    {
        TetrisOp tetris = CurrentTetris;
        int originalRotation = tetris.CurrentTransformPos;

        // 尝试所有可能的旋转状态
        for (int i = 0; i < tetris.TransformPositions.Length; i++)
        {
            if (tetris.IsTransformedPositionsCleared(
                tetris.OffsetAtTileMap,
                tetris.OriginPositionAtTileMap))
            {
                Debug.Log($"找到有效的旋转状态: {tetris.CurrentTransformPos}");
                return;
            }

            // 尝试下一个旋转状态
            tetris.TransformToNext();
        }

        // 如果没有找到有效的旋转状态，回到原始状态
        tetris.CurrentTransformPos = originalRotation;
        Debug.LogWarning("无法找到有效的旋转状态，使用默认状态");
    }


    void Update()
    {
        // 检查游戏是否应该开始
        if (!gameStarted && Time.time - gameStartTime >= startDelay)
        {
            gameStarted = true;
            RandomChooseCurrentTetris();
            LastDownTime = Time.time;
            Debug.Log("游戏开始，生成第一个方块");
            return;
        }

        // 如果游戏还没开始，不执行游戏逻辑
        if (!gameStarted) return;

        bool TransformedAtThisFrame = false;
        float Now = Time.time;
        float ElapsedTime = Now - LastDownTime;//Get elapsed time from last down one cell

        _Offset = CurrentTetris.OffsetAtTileMap;
        float DefaultDownSpeed = DownSpeed;


        if (CurrentTetris.IsTransformedPositionsCleared(_Offset, CurrentTetris.OriginPositionAtTileMap))
        {
            CurrentTetris.OffsetAtTileMap = _Offset;
        }
        else
        {
            _Offset = CurrentTetris.OffsetAtTileMap;
            if (TransformedAtThisFrame)
            {
                CurrentTetris.TransformToPrevious();
            }
        }


        if (ElapsedTime >= 1.0f / DownSpeed)//达到可以下落一格的时间
        {
            _Offset.y -= 1;
            LastDownTime = Time.time;
        }

        //Test if next transformed Positions are cleared,if not,paint tetris at last position and reset it.
        if (CurrentTetris.IsTransformedPositionsCleared(_Offset, CurrentTetris.OriginPositionAtTileMap))
        {
            CurrentTetris.OffsetAtTileMap = _Offset;//save the new offset
            CurrentTetris.ComputeCurrentTransformedPositions();
            CurrentTetris.MoveTetris();
            CurrentTetris.SaveLastTransformedPositions();
        }
        else
        {
            CurrentTetris.FillLastTransformedTetrisLocked();
            LastDownTime = Now;
            RandomChooseCurrentTetris();
            CurrentTetris.Reset();
        }
        // 检查当前活动方块是否被收集
        if (CurrentTetris != null && CurrentTetris.isCollected&&CurrentTetris.isCollectable)
        {
            // 如果当前活动方块被收集，立即生成新方块
            Debug.Log("当前方块被收集，生成新方块");
            CurrentTetris.ClearFromTilemap();
            RandomChooseCurrentTetris();
            LastDownTime = Time.time;
        }
        TCL.ClearLockedTiles();
        if (TGO.IsGameOver())
        {
            DownSpeed = 0.0f;
            isOver = true;
        }


    }



    // 检查是否是当前活动俄罗斯方块的Tile
    public bool IsActiveTetrisTile(Vector3Int cellPosition, TileBase hitTile)
    {
        if (CurrentTetris == null) return false;

        // 首先检查Tile类型（如果使用了不同的Tile）
        TilesManual tilesManual = CurrentTetris.TilesManualObject;
        if (tilesManual != null && hitTile != tilesManual.TB[0])
        {
            return false;
        }

        // 然后检查位置是否匹配
        CurrentTetris.ComputeCurrentTransformedPositions();
        TetrisPos currentPos = CurrentTetris.GetTransformedTetrisPos();

        foreach (Vector3Int tetrisCell in currentPos.Positions)
        {
            if (tetrisCell == cellPosition)
                return true;
        }

        return false;
    }

    // 处理玩家与俄罗斯方块的碰撞
    public void HandlePlayerTetrisCollision(Vector3Int cellPosition, Vector2 collisionNormal, PlayerControl player)
    {
        audioHit.PlayAudioClip();
        // 避免重复处理同一单元格的碰撞
        if (cellPosition == lastCollisionCell &&
            Time.time - lastHorizontalCollisionTime < 0.1f)
            return;

        Vector3Int moveDirection = GetMoveDirectionFromCollision(collisionNormal, player);

        if (moveDirection != Vector3Int.zero)
        {
            ApplyTetrisMovement(moveDirection);
            lastCollisionCell = cellPosition;

            if (moveDirection.x != 0)
                lastHorizontalCollisionTime = Time.time;
            else if (moveDirection.y != 0)
                lastVerticalCollisionTime = Time.time;
        }
    }

    // 根据碰撞方向和玩家状态获取移动方向
    private Vector3Int GetMoveDirectionFromCollision(Vector2 collisionNormal, PlayerControl player)
    {
        // 水平移动
        if (Mathf.Abs(collisionNormal.x) > 0.7f)
        {
            // 检查冷却时间
            if (Time.time - lastHorizontalCollisionTime < horizontalMoveCooldown)
                return Vector3Int.zero;

            // 根据玩家朝向决定移动方向
            if (collisionNormal.x > 0 && player.moveDirection.x < 0)
            {
                // 玩家从左侧碰撞且向左移动，方块向左移动
                return Vector3Int.left;
            }
            else if (collisionNormal.x < 0 && player.moveDirection.x > 0)
            {
                // 玩家从右侧碰撞且向右移动，方块向右移动
                return Vector3Int.right;
            }
        }
        // 垂直移动（加速下落）
        else if (collisionNormal.y > 0.7f)
        {
            // 检查冷却时间
            if (Time.time - lastVerticalCollisionTime < verticalMoveCooldown)
                return Vector3Int.zero;

            // 玩家从上方碰撞，加速下落
            return Vector3Int.down;
        }

        return Vector3Int.zero;
    }

    // 应用俄罗斯方块移动
    private void ApplyTetrisMovement(Vector3Int moveDirection)
    {
        Vector3Int newOffset = CurrentTetris.OffsetAtTileMap + moveDirection;

        // 测试新位置是否可用
        if (CurrentTetris.IsTransformedPositionsCleared(newOffset, CurrentTetris.OriginPositionAtTileMap))
        {
            CurrentTetris.OffsetAtTileMap = newOffset;

            // 立即更新显示
            CurrentTetris.ComputeCurrentTransformedPositions();
            CurrentTetris.MoveTetris();
            CurrentTetris.SaveLastTransformedPositions();

            Debug.Log($"俄罗斯方块移动: {moveDirection}");

            // 如果是加速下落，重置下落计时器
            if (moveDirection.y < 0)
            {
                LastDownTime = Time.time;
            }
        }
    }

}