using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TetrisCollideAt
{
    public bool Left=false;
    public bool Right = false;
    public bool Down = false;
}

//用于操作俄罗斯方块的旋转，位移，位置锁定和删除
public class TetrisOp : MonoBehaviour {
    public string Name;
    public TetrisPos[] TransformPositions;//所有可变换位置
    public TilesManual TilesManualObject;
    public int CurrentTransformPos=1;//指定当前的变换位置为TransformPositions[CurrentTransformPos]
    public Vector3Int OriginPositionAtTileMap;
    public Vector3Int OffsetAtTileMap;
    public TetrisPos TransformedPositions;//变换后的位置，即Tetris移动后的位置，Tetris移动后，这里将保存移动量与当前TransformPositions相加的结果.
    public TetrisPos LastTransformedPositions;
    private bool Locked = false;//锁定的方块不可移动
    [Tooltip("Use to save result of temped transform positions")]
    public TetrisPos TempTransformedPositions;
    public TetrisCollideAt mTetrisCollideAt;
    public TileBase LockTile;
    // Use this for initialization
    void Start () {
        //TransformedPositions = new TetrisPos();
        mTetrisCollideAt = new TetrisCollideAt();
	}

    private void Init()
    {
        OffsetAtTileMap = Vector3Int.zero;
    }

    //Move Tetris to _Position at TileMap
    public void MoveTetris(Vector3Int[] _Position)
    {
        TilesManualObject.SetTilesManual(TilesManualObject.TB, _Position);
    }

    public void SaveLastTransformedPositions()
    {
        for (int a = 0; a < TransformedPositions.Positions.Length; a++)
        {
            LastTransformedPositions.Positions[a] = TransformedPositions.Positions[a];
        }

    }

    //Compute current transformed position.
    public void ComputeCurrentTransformedPositions()
    {
        //获取Tetris的变换状态，与当前位置，初始位置做加法
        for (int i = 0; i < TransformPositions[CurrentTransformPos].Positions.Length; i++)
        {
            TransformedPositions.Positions[i].x = TransformPositions[CurrentTransformPos].Positions[i].x + OffsetAtTileMap.x+OriginPositionAtTileMap.x;
            TransformedPositions.Positions[i].y = TransformPositions[CurrentTransformPos].Positions[i].y + OffsetAtTileMap.y + OriginPositionAtTileMap.y;
            TransformedPositions.Positions[i].z = TransformPositions[CurrentTransformPos].Positions[i].z + OffsetAtTileMap.z + OriginPositionAtTileMap.z;
        }

    }

    //Compute transformed positions about _Offfset and _OriginPositionAtTileMap,the results will filled in member _TransformedPositions.
    public void ComputeTransformedPositions(Vector3Int _Offset, Vector3Int _OriginPositionAtTileMap)
    {
        //获取Tetris的变换状态，与当前位置，初始位置做加法
        for (int i = 0; i < TransformPositions[CurrentTransformPos].Positions.Length; i++)
        {
            TempTransformedPositions.Positions[i].x = TransformPositions[CurrentTransformPos].Positions[i].x + _Offset.x + _OriginPositionAtTileMap.x;
            TempTransformedPositions.Positions[i].y = TransformPositions[CurrentTransformPos].Positions[i].y + _Offset.y + _OriginPositionAtTileMap.y;
            TempTransformedPositions.Positions[i].z = TransformPositions[CurrentTransformPos].Positions[i].z + _Offset.z + _OriginPositionAtTileMap.z;
        }

    }

    public TetrisPos GetTransformedTetrisPos() { return this.TransformedPositions; }

    //Before move the tetris use this method to clear the path that tetris moved over.
    //If the cell color is _Color,it will not be cleared.
    public void ClearTetrisAtLastTransformedPositions()
    {
        for (int i = 0; i < LastTransformedPositions.Positions.Length; i++)
        {
            if(TilesManualObject.TileMapObject.GetTile(LastTransformedPositions.Positions[i])!= TilesManualObject.TBLocked[0])
            {
                TilesManualObject.TileMapObject.SetTile(LastTransformedPositions.Positions[i], TilesManualObject.TBClear[0]);
                //Debug.Log("Not Locked!");
            }
           
        }
        //TM.ClearTilesManual(TM.TBClear, LastTransformedPositions.Positions);
    }

    //Compute TransformedPositions about _Offset and _OriginPositionAtTileMap,then test if these positions are cleared.
    //The method get collide info of the next position tetris will go and the current tetris.  
    //The detail collide info saved in mTetrisCollideAt
    //Note:This method will clear tetris at last transformed positions.
    public bool IsTransformedPositionsCleared(Vector3Int _Offset,Vector3Int _OriginPositionAtTileMap)
    {
        ClearTetrisAtLastTransformedPositions();
        //Get transformed positions,and put it in _TransformedPositions.
        ComputeTransformedPositions(_Offset, _OriginPositionAtTileMap);

        for(int i=0;i< TempTransformedPositions.Positions.Length;i++)
        {
            if(TilesManualObject.TileMapObject.HasTile(TempTransformedPositions.Positions[i]))
            {
                //Debug.Log("There is a tile in here!");
                return false;
            }
        }
        return true;
    }

    //It will compute the transformed position.
    //Note:This method will clear tetris at last transformed positions.
    public bool IsNextTransformedPositionsCleared(Color _ColorNotBeCleared)
    {
        ClearTetrisAtLastTransformedPositions();
        ComputeCurrentTransformedPositions();
        foreach(Vector3Int v3i in TransformedPositions.Positions)
        {
            if(TilesManualObject.TileMapObject.HasTile(v3i))
            {
                //Debug.Log("There is a tile in here!");
                return false;
            }
        }
        return true;
    }

    //Move Tetris to current transformed position.It will compute the transformed positions.
    public void MoveTetrisToTransformedPosition()
    {
        ComputeCurrentTransformedPositions();
        TilesManualObject.SetTilesManual(TilesManualObject.TB, TransformedPositions.Positions);
    }

    public void MoveTetrisToLastTransformedPosition()
    {
        TilesManualObject.SetTilesManual(TilesManualObject.TB, LastTransformedPositions.Positions);
    }

    //Move Tetris to current transformed position.It won't compute the transformed position.
    public void MoveTetris()
    {
        TilesManualObject.SetTilesManual(TilesManualObject.TB, TransformedPositions.Positions);
    }

    public void FillLastTransformedTetrisLocked()
    {
        foreach (Vector3Int v3i in LastTransformedPositions.Positions)
        {
            //Debug.Log("Fill Locked!!!");
            TilesManualObject.LockTile(v3i,LockTile);
            if(TilesManualObject.TileMapObject.GetTile(v3i)== LockTile)
            {
                //Debug.Log("LockTile Set Successfully!");
                ;
            }
            else
            {
                Debug.Log("LockTile Set Failed!");
            }
        }
    }

    public void SetCurrentTetrisColor(Color _Color)
    {
        foreach (Vector3Int v3i in TransformedPositions.Positions)
        {
            TilesManualObject.TileMapObject.SetColor(v3i, _Color);
        }
    }

    public void SetLastTransformedTetrisColor(Color _Color)
    {
        foreach (Vector3Int v3i in LastTransformedPositions.Positions)
        {
            TilesManualObject.TileMapObject.SetColor(v3i, _Color);
            Debug.Log(TilesManualObject.TileMapObject.GetColor(v3i).ToString());
        }
    }

    public void TransformToNext()
    {
        if(CurrentTransformPos<TransformPositions.Length-1)
        {
            CurrentTransformPos++;
        }
        else
        {
            CurrentTransformPos = 0;
        }
    }

    public void TransformToPrevious()
    {
        if (CurrentTransformPos >0)
        {
            CurrentTransformPos--;
        }
        else
        {
            CurrentTransformPos = TransformPositions.Length-1;
        }
    }

    public void Reset()
    {
        CurrentTransformPos = 0;
        OffsetAtTileMap.x = 0;
        OffsetAtTileMap.y = 0;
        OffsetAtTileMap.z = 0;
    }

    // Update is called once per frame
    void Update () {
        //TM.SetTilesManual(TM.TB, TransformPositions[CurrentTransformPos].Positions);
        //MoveTetrisToTransformedPosition();
	}
}
