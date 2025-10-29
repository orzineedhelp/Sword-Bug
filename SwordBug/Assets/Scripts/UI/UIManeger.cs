using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManeger : MonoBehaviour
{
    [Header("组件")]
    private PlayerInputControl inputActions;
    public PlayerHealth playerHealth;
    public GameObject mainCanvas;
    public GameObject gameOverPanel;
    public GameObject restartButton;

    public Button settingButton;
    public GameObject pausePanel;
    public Slider volumeSlider_Master;
    public Slider volumeSlider_BGM;
    public Slider volumeSlider_FX;


    [Header("事件监听")]
    public CharacterEventSO HealthEvent;//由character利用事件传递信息，之后UIManeger接受并统一处理UI
    public SceneLoadEventSO unloadedSceneEvent;
    public VoidEventSO loadDataEvent;//加载保存数据
    public VoidEventSO gameOverEvent;
    public VoidEventSO backToMenuEvent;
    public FloatEventSO syncVolumeEvent;
    public FloatEventSO syncVolumeEventBGM;
    public FloatEventSO syncVolumeEventFX;


    [Header("广播")]
    public VoidEventSO pauseEvent;

    private void Awake()
    {
        settingButton.onClick.AddListener(TogglePausePanel);
        inputActions = new PlayerInputControl();
    }
    private void OnEnable()
    {
        inputActions.Enable();
        HealthEvent.OnEventRaised += OnHealthEvent;//注册事件
        unloadedSceneEvent.LoadRequestEvent += OnUnloadSceneEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        gameOverEvent.OnEventRaised += OnGameOverEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised += OnSyncVolumeEvent;
        syncVolumeEventBGM.OnEventRaised += OnSyncVolumeEventBGM;
        syncVolumeEventFX.OnEventRaised += OnSyncVolumeEventFX;
        inputActions.UI.Setting.started += ActivePausePanel;
    }
    private void OnDisable()
    {
        HealthEvent.OnEventRaised -= OnHealthEvent;//注销事件
        unloadedSceneEvent.LoadRequestEvent -= OnUnloadSceneEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        gameOverEvent.OnEventRaised -= OnGameOverEvent;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
        syncVolumeEvent.OnEventRaised -= OnSyncVolumeEvent;
        syncVolumeEventBGM.OnEventRaised -= OnSyncVolumeEventBGM;
        syncVolumeEventFX.OnEventRaised -= OnSyncVolumeEventFX;

        inputActions.UI.Setting.started -= ActivePausePanel;


    }

    private void ActivePausePanel(InputAction.CallbackContext context)
    {
        TogglePausePanel();
    }

    private void OnSyncVolumeEventBGM(float amount_B)
    {
        volumeSlider_BGM.value = (amount_B + 80) / 100;
    }

    private void OnSyncVolumeEventFX(float amount_F)
    {
        volumeSlider_FX.value = (amount_F + 80) / 100;
    }

    private void OnSyncVolumeEvent(float amount)
    {
        volumeSlider_Master.value = (amount+80)/100;
    }

    public void TogglePausePanel()
    {
        if (pausePanel.activeInHierarchy)
        {
            pausePanel.SetActive(false);
            Time.timeScale = 1.0f;
        }
        else
        {
            pausePanel.SetActive(true);
            pauseEvent.RaiseEvent();
            Time.timeScale = 0.0f;
        }
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
    //    Debug.Log("扣除血量为" + num+"isheal"+ character.isheal+"isdead"+ character.isDead);
        playerHealth.OnHealthChange(num,character.isheal,character.isDead);

    }
}
