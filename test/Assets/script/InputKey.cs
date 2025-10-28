using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputKey : MonoBehaviour {
    [Tooltip("The Name Of this Object")]
    public string OperatingName;
    [Tooltip("The Funtion will get input data from this key")]
    public string KeyName;
    private float PressTime;
    private bool Pressing = false;
	// Use this for initialization
	void Start () {
		
	}

    //将按下按键的时间置为当前时间
    public void ResetPressTime()
    {
        PressTime = Time.time;
    }

    //获取按键被连续按下的时间。
    public float PressedTimes() { return Time.time - PressTime; }

    //Returns true while the user holds down the key identified by KeyName.Think auto fire.
    public bool IsDown()
    {
        return Input.GetKey(KeyName);
    }

    //Returns true during the frame the user starts pressing down the key identified by KeyName.
    public bool IsKeyClick()
    {
        return Input.GetKeyDown(KeyName);
    }
	
	// Update is called once per frame
	void Update () {
      if(Input.GetKey(KeyName))
        {
            if(!Pressing)
            {
                PressTime = Time.time;//Record the time start press the key.
                Pressing = true;
            }
        }
	}
}
