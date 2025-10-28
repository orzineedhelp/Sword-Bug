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
   
    [Header("事件监听")]
    public SceneLoadEventSO loadEventSO;
    public VoidEventSO newGameEvent;
    private GameSceneSO nextScene;
    private Vector3 positionNext;
    private bool needAni;
    [Header("场景")]
    public GameSceneSO menuLoadScene;
    public GameSceneSO firstLoadScene;
    public GameSceneSO currentScene;
    public VoidEventSO onMap0Loaded; // 进入map0时触发的事件

    [Header("广播")]
    public VoidEventSO afterSceneLoadedEvent;
    public FadeEventSO fadeEvent;
    public SceneLoadEventSO unLoadedSceneEvent;
    public VoidEventSO backToMenuEvent;
    public float fadeDuration;

   
    private void Start()
    {
       // NewGame();
        needChangePos = true;
        loadEventSO.RaisedLoadRequestEvent(menuLoadScene,menuPosition,true,true);
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
        loadEventSO.RaisedLoadRequestEvent(nextScene,menuPosition,true,true);   
    }

    private void NewGame()
    {
        nextScene = firstLoadScene;
       
        // OnLoadRequestEvent(nextScene, firstPosition, true);
        loadEventSO.RaisedLoadRequestEvent(nextScene, firstPosition, true,true);
          onMap0Loaded?.RaiseEvent();
        
    }

    /// <summary>
    /// 场景加载事件请求
    /// </summary>
    /// <param name="location"></param>
    /// <param name="position"></param>
    /// <param name="isAni"></param>
    private void OnLoadRequestEvent(GameSceneSO location, Vector3 position, bool isAni,bool isChangePos)
    {
     
        nextScene=location;
        positionNext=position;
        needAni=isAni;
        needChangePos=isChangePos;
        if (currentScene != null)
            StartCoroutine(UnLoadPreScene());
        else LoadScene();

    }

    private void OnDisable()
    {
        loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
        newGameEvent.OnEventRaised -= NewGame;
        backToMenuEvent.OnEventRaised -= OnBackToMenuEvent;

        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }
    private IEnumerator UnLoadPreScene()
    {
        if(needAni)
        {
            //实现转场动画
            fadeEvent.FadeIn(fadeDuration);
            
        }
        yield return new WaitForSeconds(fadeDuration);
        //广播事件调整血量
        unLoadedSceneEvent.RaisedLoadRequestEvent(nextScene,positionNext,true,true);
        yield return currentScene.sceneReference.UnLoadScene();//等待操作完成后

        playerPosition.gameObject.SetActive(false);
       // player.SetActive(true);

        LoadScene();

    }
    public void LoadScene()
    {
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
        if(needChangePos) playerPosition.position=positionNext;
        needChangePos=false;
        playerPosition.gameObject.SetActive(true);
        if (needAni)
        {
            //执行转场动画
            fadeEvent.FadeOut(fadeDuration);
        }
        if(currentScene.sceneType!=SceneType.Menu) 
        afterSceneLoadedEvent?.RaiseEvent();
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
            nextScene = data.GetSaveScene();
            OnLoadRequestEvent(nextScene, positionNext, true, true);
        }
    }
    //public bool CheckCurrentScene(GameSceneSO scence)
    //{
    //    return currentScene==scence;
    //}
}
