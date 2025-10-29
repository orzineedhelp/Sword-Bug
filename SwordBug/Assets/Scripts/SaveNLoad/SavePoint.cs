using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour
{
    //进入则自动存档
    [Header("广播")]
    public VoidEventSO saveGameEvent;
    public bool isSaved;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isSaved&& collision.tag == "Player")
        {
          
                //保存数据
                Debug.Log(saveGameEvent.name);
            saveGameEvent.RaiseEvent();
            isSaved = true;
        }   
    }
}
