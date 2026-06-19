using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//基于unity Tilemap，在指定位置填充特定Tile \Scripts\comm
public class TilesManual : MonoBehaviour {
    public TileBase[] TB;
    public TileBase[] TBClear;
    public Tilemap TileMapObject;
    public Vector3Int[] Positions;
    public TileBase[] TBLocked;
	// Use this for initialization
	void Start () {
        //Matrix4x4 mtx =Matrix4x4.TRS(Vector4.zero, Quaternion.Euler(0.0f, 0.0f, 90.0f), Vector4.one);
        //TM.SetTiles(Positions, TB);
        //TM.SetTransformMatrix(Positions[0], mtx);
	}
	
    //在指定位置填充Tile
    public void SetTiles() { TileMapObject.SetTiles(Positions, TB); }

    public void ClearTiles() { TileMapObject.SetTiles(Positions, TBClear); }

    public void ClearTile(Vector3Int _Position, TileBase _TBClear) { TileMapObject.SetTile(_Position, _TBClear); }

    public void SetTilesManual(TileBase[] _TB,Vector3Int[] _Positions) { TileMapObject.SetTiles(_Positions, _TB); }

    public void ClearTilesManual(TileBase[] _TB, Vector3Int[] _Positions) { TileMapObject.SetTiles(_Positions, _TB); }

    public void LockTiles() { TileMapObject.SetTiles(Positions, TBLocked); }

    public void LockTilesManual(TileBase[] _TBLock, Vector3Int[] _Positions) { TileMapObject.SetTiles(_Positions, _TBLock); }

    public void LockTile(Vector3Int _Position,TileBase _LockedTile) { TileMapObject.SetTile(_Position, _LockedTile); }

  
}
