using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
   public UnityAction<GameSceneSO,Vector3,bool,bool> LoadRequestEvent;
    /// <summary>
    /// 场景请求加载
    /// </summary>
    /// <param name="gameScene">下一个地点</param>
    /// <param name="position">传送位置</param>
    /// <param name="isReset">是否要重置玩家数值</param>
    public void RaisedLoadRequestEvent(GameSceneSO gameScene, Vector3 position, bool isReset,bool isChangePos)
    {
        LoadRequestEvent?.Invoke(gameScene, position, isReset,isChangePos);
    }
}
