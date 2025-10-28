using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetrisGameLogic : MonoBehaviour {
    //public Vector3Int CurrentTetrisPos;
    [Tooltip("The Current Tetris User can control")]
    public TetrisOp CurrentTetris;
    [Tooltip("The Tetris moving down speed-DownSpeed X 1 Cells per Second")]
    public float DownSpeed=1.0f;
    private float LastDownTime;
    private bool IsNextPositionsCleared = true;//true-clear last transformed positions before move the tetris.false won't clear it.
    public SimpleInput SI;
    public TetrisClearLine TCL;
    public TetrisGameOver TGO;
    public TextMesh GameOverText;
    public TetrisOp[] AllTetrisCanUse;
    [Tooltip("按下左或右移动键移动一格所需时间")]
    public float MoveTimesPerBlock = 0.5f;
	// Use this for initialization
	void Start () {
        RandomChooseCurrentTetris();
        GameOverText.gameObject.SetActive(false);
        LastDownTime = Time.time;
	}

    void RandomChooseCurrentTetris()
    {
        int i = AllTetrisCanUse.Length;
        int j = Random.Range(0, i);
        CurrentTetris = AllTetrisCanUse[j];
    }

    private void FixedUpdate()
    {
       
    }

    private void LateUpdate()
    {
        
    }

    // Update is called once per frame
    void Update () {
        bool TransformedAtThisFrame = false;
        float Now = Time.time;
        float ElapsedTime = Now-LastDownTime;//Get elapsed time from last down one cell
        Vector3Int _Offset;
        _Offset = CurrentTetris.OffsetAtTileMap;
        float DefaultDownSpeed = DownSpeed;

        //if ((SI.MoveRight.IsKeyClick())||(SI.MoveRight.IsDown()&&(SI.MoveRight.PressedTimes()>=MoveTimesPerBlock)))
        //{
        //    //Debug.Log("MoveRight button down!");
        //    SI.MoveRight.ResetPressTime();
        //    _Offset.x += 1;
        //}
        //if ((SI.MoveLeft.IsKeyClick()) || (SI.MoveLeft.IsDown() && (SI.MoveLeft.PressedTimes() >= MoveTimesPerBlock)))
        //{
        //    SI.MoveLeft.ResetPressTime();
        //    _Offset.x -= 1;
        //}

        //if ((SI.Up.IsKeyClick()) || (SI.Up.IsDown() && (SI.Up.PressedTimes() >= MoveTimesPerBlock)))
        //{
        //    SI.Up.ResetPressTime();
        //    CurrentTetris.TransformToNext();
        //    TransformedAtThisFrame = true;//save this state,if the transformation is failed,we will use this to detect if we should transform to previous.
        //}

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

        //if ((SI.Down.IsKeyClick()) || (SI.Down.IsDown() && (SI.Down.PressedTimes() >= MoveTimesPerBlock)))
        //{
        //    SI.Down.ResetPressTime();
        //    _Offset.y -= 1;
        //}

        if (ElapsedTime>=1.0f/DownSpeed)//达到可以下落一格的时间
        {
            _Offset.y -= 1;
            LastDownTime = Time.time;
        }

        //Test if next transformed Positions are cleared,if not,paint tetris at last position and reset it.
        if (CurrentTetris.IsTransformedPositionsCleared(_Offset,CurrentTetris.OriginPositionAtTileMap))
        {
            CurrentTetris.OffsetAtTileMap = _Offset;//save the new offset
            CurrentTetris.ComputeCurrentTransformedPositions();
            CurrentTetris.MoveTetris();
            CurrentTetris.SaveLastTransformedPositions();
        }
        else
        {
            //CurrentTetris.MoveTetrisToLastTransformedPosition();
            //Debug.Log("Fill Locked!");
            CurrentTetris.FillLastTransformedTetrisLocked();
            //CurrentTetris.MoveTetrisToLastTransformedPosition();
            LastDownTime = Now;
            RandomChooseCurrentTetris();
            CurrentTetris.Reset();
        }

        TCL.ClearLockedTiles();
        if(TGO.IsGameOver())
        {
            GameOverText.gameObject.SetActive(true);
            DownSpeed = 0.0f;
        }

        //CurrentTetris.TM.LockTile(new Vector3Int(-6,9,0),CurrentTetris.LockTile);
        //CurrentTetris.TM.LockTile(new Vector3Int(5, -9, 0), CurrentTetris.LockTile);
    }
}
