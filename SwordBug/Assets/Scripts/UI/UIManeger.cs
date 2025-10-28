using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIManeger : MonoBehaviour
{
    [Header("组件")]
    public PlayerHealth playerHealth;
    public GameObject mainCanvas;
    public GameObject gameOverPanel;
    public GameObject restartButton;

    [Header("事件监听")]
    public CharacterEventSO HealthEvent;//由character利用事件传递信息，之后UIManeger接受并统一处理UI
    public SceneLoadEventSO unloadedSceneEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO gameOverEvent;
    public VoidEventSO backToMenuEvent;


    private void OnEnable()
    {
        HealthEvent.OnEventRaised += OnHealthEvent;//注册事件
        unloadedSceneEvent.LoadRequestEvent += OnUnloadSceneEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        gameOverEvent.OnEventRaised += OnGameOverEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;

    }
    private void OnDisable()
    {
        HealthEvent.OnEventRaised -= OnHealthEvent;//注销事件
        unloadedSceneEvent.LoadRequestEvent -= OnUnloadSceneEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        gameOverEvent.OnEventRaised -= OnGameOverEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;


    }

    private void OnGameOverEvent()
    {
       gameOverPanel.SetActive(true);
       EventSystem.current.SetSelectedGameObject(restartButton);
    }

    private void OnLoadDataEvent()
    {
     gameOverPanel.SetActive(false);
    }

    private void OnUnloadSceneEvent(GameSceneSO arg0, Vector3 arg1, bool arg2, bool i)
    {
        var isMenu = arg0.sceneType == SceneType.Menu;
        mainCanvas.SetActive(!isMenu);
        playerHealth.gameObject.SetActive(!isMenu);
       

        
    }

    private void OnHealthEvent(Character character)
    {
        //具体扣血UI显示逻辑执行
        int num = character.maxHealth- character.currentHealth;
        Debug.Log("扣除血量为" + num+"isheal"+ character.isheal+"isdead"+ character.isDead);
        playerHealth.OnHealthChange(num,character.isheal,character.isDead);

    }
}
