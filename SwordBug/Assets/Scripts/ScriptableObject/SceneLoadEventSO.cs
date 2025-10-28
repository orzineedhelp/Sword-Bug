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
    /// <param name="isAni">是否要加入转场动画</param>
    public void RaisedLoadRequestEvent(GameSceneSO gameScene, Vector3 position, bool isAni,bool isChangePos)
    {
        LoadRequestEvent?.Invoke(gameScene, position, isAni,isChangePos);
    }
}
