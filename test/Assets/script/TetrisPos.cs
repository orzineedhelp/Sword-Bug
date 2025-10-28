using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//单个俄罗斯方块组中各方块的位置和状态信息
public class TetrisPos : MonoBehaviour{
    public string Name;
    public Vector3Int[] Positions;//origin positions of every single block in Tetris block.
}
