using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour
{
    public Vector3 TeleportPosition;
    public GameSceneSO nextScene;
    public SceneLoadEventSO loadEventSO;
    public void TriggerAction()
    {
        loadEventSO.RaisedLoadRequestEvent(nextScene, TeleportPosition, true, false);
        Debug.Log("change scene");
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((collision.tag=="Player"))
        {
            TriggerAction();
        }
    }
}
