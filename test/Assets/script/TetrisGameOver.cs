using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//测试是Tetris是否达成GameOver条件，并作出处理
public class TetrisGameOver : MonoBehaviour {
    public string Name;
    [Tooltip("由此对象获取TileMap,并对TileMap的单元格进行操作")]
    public TilesManual TilesManualObject;
    [Tooltip("Filled lines with this Tile will be set of locked,could pass the ClearLineTest.")]
    public TileBase TBLockedBlock;
    [Tooltip("Left Up corner of the region tetris could placed in TileMap.")]
    public Vector2Int LT;
    [Tooltip("Right Down corner of the region tetris could placed in TileMap.")]
    public Vector2Int RD;
    // Use this for initialization
    void Start () {
		
	}

    public bool IsGameOver()
    {
        for (int j = LT.x; j < RD.x + 1; j++)
        {
            if(TilesManualObject.TileMapObject.GetTile(new Vector3Int(j, LT.y+1, 0))==TBLockedBlock)
            {
                return true;
            }
        }

        return false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
