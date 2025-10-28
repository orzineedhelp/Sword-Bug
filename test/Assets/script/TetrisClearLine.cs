using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//清理指定TileMap中已经填满FixBlock的行，并记录清除行的总数
public class TetrisClearLine : MonoBehaviour {
    public string Name;
    [Tooltip("由此对象获取TileMap,并对TileMap的单元格进行操作")]
    public TilesManual TilesManualObject;
    [Tooltip("Filled lines with this Tile will be set of locked,could pass the ClearLineTest.")]
    public TileBase TBLockedBlock;
    [Tooltip("Left Up corner of the region tetris could placed in TileMap.")]
    public Vector2Int LT;
    [Tooltip("Right Down corner of the region tetris could placed in TileMap.")]
    public Vector2Int RD;
    public TileBase TBClear;
    private int LineCleared = 0;
    private bool NeedClear = false;//此值为true时LineNeedClear中保存自下方第一行需要被清除的LockedTile的y值，即行坐标。
    private int LineNeedClear = 0;
    private Vector3Int[] LockedTilesPositions; //use to save  positions of locked tiles. 
    private int ValidInLockedTilesPositions = 0;
	// Use this for initialization
	void Start () {
        LockedTilesPositions = new Vector3Int[300];
	}

    //清除被锁定的Tiles
    public bool ClearLockedTiles()
    {
        int ClearedAtThisTime = 0;
        while(ClearLineTest())
        {
            if(NeedClear)
            {
                MoveLockedDownTo(LineNeedClear);
                ClearedAtThisTime++;
            }
        }

        if (ClearedAtThisTime > 0)
            return true;

        return false;
    }

    //清除一行，并将本行上面的LockedTile顺序下移一行，清除被下移行原来位置上的LockedTile。
    public void MoveLockedDownTo(int LineNum)
    {
        ClearLine(LineNum);//清除指定行
        GetLockedPositions(LineNum);//保存当前的LockedTiles的位置
        ClearSavedLockedPositions();//清除指定行上方的所有Locked Tiles.
        MoveDownLockedTilesInSavedPositions();//将剩余的Locked Tiles向下移动一行.
    }

    public int GetTotalClearedLines()
    {
        return this.LineCleared;
    }

    //用TBClear填充指定行，实现清除指定行，每调用一次，清除行数计数变量LineCleared+1
    public void ClearLine(int LineNum)
    {
        for (int j = LT.x; j < RD.x+1; j++)
        {
            TilesManualObject.ClearTile(new Vector3Int(j, LineNum, 0),TBClear);
        }

        LineCleared++;
    }

    //将位于行LineNum上方的Locked tiles的位置保存到LockedTilesPositions变量
    public void GetLockedPositions(int LineNum)
    {
        ValidInLockedTilesPositions = 0;

        for (int i = LineNum; i < LT.y + 1; i++)
        {
            for (int j = LT.x; j < RD.x + 1; j++)
            {
                if (TilesManualObject.TileMapObject.GetTile(new Vector3Int(j, i, 0)) == TBLockedBlock)//Has locked tile,save its position
                {
                    LockedTilesPositions[ValidInLockedTilesPositions].x = j;
                    LockedTilesPositions[ValidInLockedTilesPositions].y =i;
                    LockedTilesPositions[ValidInLockedTilesPositions].z =0;

                    //Debug.Log(LockedTilesPositions[ValidInLockedTilesPositions].ToString());
                    //Debug.Log(LockedTilesPositions[ValidInLockedTilesPositions].x.ToString());

                    ValidInLockedTilesPositions++;//The total locked tiles positions in LockedTilesPositions From LockedTilesPositions[0].

                    //Debug.Log(LockedTilesPositions[ValidInLockedTilesPositions].ToString());
                }
            }
        }
    }

    //清除保存在LockedTilesPositions中指定位置的LockedTiles
    public void ClearSavedLockedPositions()
    {
        for(int i=0; i<ValidInLockedTilesPositions;i++)
        {
            TilesManualObject.ClearTile(LockedTilesPositions[i], TBClear);
        }
    }

    //将保存在LockedTilesPositions中指定位置的LockedTiles向下移动一行.然后将NeedClear置为False，表明被标记的需要清除的行LineNeedClear已被清除。
    public void MoveDownLockedTilesInSavedPositions()
    {
        Vector3Int v3i;

        for (int i = 0; i < ValidInLockedTilesPositions; i++)
        {
            v3i = LockedTilesPositions[i];
            v3i.y -= 1;
            TilesManualObject.LockTile(v3i, TBLockedBlock);
        }

        NeedClear = false;
    }

    //从下到上测试是否有可清除的LockedTile行，测试得出有可清除的LockedTile行将保存这行的坐标至，即y值，并立即返回true.
    public bool ClearLineTest()
    {
        for(int i=RD.y;i<LT.y+1;i++)
        {
            for(int j=LT.x;j<RD.x+1;j++)
            {
                if(TilesManualObject.TileMapObject.GetTile(new Vector3Int(j,i,0))!=TBLockedBlock)//Has unlocked tile in this line, don't clear and go next line.
                {
                    break;
                }

                if(j==RD.x)//本行所有tile皆通过上步测试，本行将被设置为clear状态。
                {
                    LineNeedClear = i;
                    NeedClear = true;
                    Debug.Log("There is a line need to clear!");
                    return true;
                }
            }
        }
        return false;
    }

    private void LateUpdate()
    {
        
    }

    // Update is called once per frame
    void Update () {
		
	}
}
