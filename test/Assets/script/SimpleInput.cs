using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//简单的输入系统
//注意：相关方法的调用需放在游戏逻辑的Update中。否则输入的处理会出现不连续的情况。
[RequireComponent(typeof(InputKey))]
//[RequireComponent(typeof(InputKey))]
public class SimpleInput : MonoBehaviour {
    [Tooltip("The name of this Input Object.It's optional.")]
    public string Name;
    public InputKey MoveRight;
    public InputKey MoveLeft;
    public InputKey Up;
    public InputKey Down;
	// Use this for initialization
	void Start () {
		
	}
    
	// Update is called once per frame
	void Update () {
		
	}
}
