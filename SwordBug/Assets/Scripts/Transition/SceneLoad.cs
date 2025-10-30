using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoad : MonoBehaviour,ISaveable
{
    public Transform playerPosition;
    public Vector3 firstPosition;
    public Vector3 menuPosition;
    public bool needChangePos;
    public GameObject player;
    public Vector3 offsetpos;

    [Header("事件监听")]
    public SceneLoadEventSO loadEventSO;//装载要加载的场景、玩家位置、是否需要转场动画、是否需要改变玩家位置
    public VoidEventSO newGameEvent;//用于直接加载第一场景
    public VoidEventSO backToMenuEvent;


    private GameSceneSO nextScene;//记录之后要加载的场景
    private Vector3 positionNext;//之后玩家的位置
    private bool needReset;
    [Header("记录场景")]
    public GameSceneSO menuLoadScene;
    public GameSceneSO firstLoadScene;
    public GameSceneSO currentScene;

    [Header("广播")]
    public VoidEventSO afterSceneLoadedEvent;//加载完场景后通知
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unLoadedSceneEvent;
    public VoidEventSO onMap0Loaded; // 进入map0时触发的事件


    public float fadeDuration;

   
    private void Start()
    {
       // NewGame();
        needChangePos = true;
        loadEventSO.RaisedLoadRequestEvent(menuLoadScene,menuPosition,true,true);//需重置
    }
    private void OnEnable()
    {
        loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
        newGameEvent.OnEventRaised += NewGame;
        backToMenuEvent.OnEventRaised += OnBackToMenuEvent;
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }

    private void OnBackToMenuEvent()
    {
        nextScene = menuLoadScene;
        loadEventSO.RaisedLoadRequestEvent(nextScene,menuPosition,true,true);   //返回菜单需重置
    }

    private void NewGame()
    {
        nextScene = firstLoadScene;
       
        // OnLoadRequestEvent(nextScene, firstPosition, true);
        loadEventSO.RaisedLoadRequestEvent(nextScene, firstPosition, true,true);//新游戏需重置
        Debug.Log("NewGame需重置！");
         
        
    }

    /// <summary>
    /// 场景加载事件请求
    /// </summary>
    /// <param name="location"></param>
    /// <param name="position"></param>
    /// <param name="isAni"></param>
    private void OnLoadRequestEvent(GameSceneSO location, Vector3 position, bool isReset,bool isChangePos)
    {
     
        nextScene=location;
        positionNext=position;
        needReset=isReset;
        needChangePos=isChangePos;
         // 如果要加载菜单场景，强制重置对话
        if (location.sceneType == SceneType.Menu)
        {
            ForceResetDialogues();
        }
        
        if (currentScene != null)
            StartCoroutine(UnLoadPreScene());
        else LoadScene();

    }
    /// <summary>
    /// 强制重置所有对话状态
    /// </summary>
    private void ForceResetDialogues()
    {
        var playerControl = player.GetComponent<PlayerControl>();
        if (playerControl != null)
        {
            playerControl.ForceCloseAllDialogues();
        }

    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }
    /// <summary>
    /// 卸载当前场景
    /// </summary>
    /// <returns></returns>
    private IEnumerator UnLoadPreScene()
    {
        
            //实现转场动画
            fadeEvent.FadeIn(fadeDuration);
            
        
        yield return new WaitForSeconds(fadeDuration);
        //广播事件调整血量
        unLoadedSceneEvent.RaisedLoadRequestEvent(nextScene,positionNext,true,true);//卸载场景去下一个场景
        yield return currentScene.sceneReference.UnLoadScene();//等待操作完成后

        playerPosition.gameObject.SetActive(false);
       // player.SetActive(true);

        LoadScene();

    }
    public void LoadScene()
    {
        if ((needReset))
        {
            player.GetComponent<PlayerControl>().ResetPlayer();
            Debug.Log("LoadScene need RESET");
        }
        var loadingOption=nextScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive,true);
        loadingOption.Completed += OnLoadCompleted;
        
    }
    /// <summary>
    /// 场景加载完后使用
    /// </summary>
    /// <param name="handle"></param>
    private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
    {
        currentScene=nextScene;
        if(needChangePos) playerPosition.position=positionNext+offsetpos;
        needChangePos=false;
        playerPosition.gameObject.SetActive(true);
         //执行转场动画
            fadeEvent.FadeOut(fadeDuration);
        
        if(currentScene.sceneType!=SceneType.Menu)
        { 
            afterSceneLoadedEvent?.RaiseEvent();//广播告诉大家场景加载完了
                                                // 如果是第一次加载 map0 场景，触发 onMap0Loaded 事件
                                                // 改进的场景检测逻辑
            CheckAndTriggerMap0Event();
        }
    }

    /// <summary>
    /// 检查并触发Map0事件
    /// </summary>
    private void CheckAndTriggerMap0Event()
    {
        // 方法1：通过场景名称比较
        string currentSceneName = currentScene.name;
        string firstSceneName = firstLoadScene != null ? firstLoadScene.name : "null";

        Debug.Log($"当前场景: {currentSceneName}, 第一场景: {firstSceneName}");

        // 方法2：通过场景引用比较
        bool isSceneReferenceEqual = currentScene.sceneReference == firstLoadScene.sceneReference;
        Debug.Log($"场景引用相等: {isSceneReferenceEqual}");

        // 方法3：通过场景类型或名称包含判断
        bool isMap0Scene = currentScene.sceneType == SceneType.MapUp; // 根据你的场景类型调整

        if (isMap0Scene)
        {
            Debug.Log("检测到Map0场景，触发onMap0Loaded事件");
            onMap0Loaded?.RaiseEvent();
        }
        else
        {
            Debug.Log($"当前不是Map0场景，跳过事件触发。场景名: {currentSceneName}");
        }
    }

    DataDefinition ISaveable.GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    void ISaveable.GetSaveData(Data data)
    {
        data.SaveGameScene(currentScene);
       // Debug.Log("获得场景" + currentScene);
    }

    /// <summary>
    /// 回到原来场景保存数据
    /// </summary>
    /// <param name="data"></param>
    void ISaveable.LoadData(Data data)
    {
        var playeID=playerPosition.GetComponent<DataDefinition>().ID;
        if (data.characterPosDict.ContainsKey(playeID))
        {
            positionNext=data.characterPosDict[playeID];
            Debug.Log("回到原来场景位置！");
            nextScene = data.GetSaveScene();
            positionNext += offsetpos;
            OnLoadRequestEvent(nextScene, positionNext, true, true);
        }
    }
    //public bool CheckCurrentScene(GameSceneSO scence)
    //{
    //    return currentScene==scence;
    //}
}
