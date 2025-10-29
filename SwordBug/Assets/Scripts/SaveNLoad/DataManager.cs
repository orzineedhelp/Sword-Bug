using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class DataManager : MonoBehaviour
{
   //使用单例模式
    public static DataManager instance;
    private List<ISaveable> saveableList=new List<ISaveable>();
    [Header("事件监听")]
    public VoidEventSO saveDataEvent;
    public VoidEventSO loadDataEvent;

    private Data saveData;
    private void Awake()
    {
        if(instance == null) instance=this;
        else Destroy(this.gameObject);
        saveData=new Data();
    }
    private void OnEnable()
    {
        saveDataEvent.OnEventRaised += Save;
        loadDataEvent.OnEventRaised += Load;
    }
    private void OnDisable()
    {
        saveDataEvent.OnEventRaised -= Save;
        loadDataEvent.OnEventRaised-= Load;

    }
    //private void Update()
    //{
    //    if (Keyboard.current.lKey.wasPressedThisFrame)
    //    {
    //        Load();
    //    }
    //}
    public void RegisterSaveData(ISaveable saveable)
    {
        if(!saveableList.Contains(saveable))
        {
            saveableList.Add(saveable);
        }
    }

    public void UnRegisterSaveData(ISaveable saveable)
    {
        saveableList.Remove(saveable);
    }
    public void Save()
    {//实现保存
        foreach (var saveable in saveableList)
        {
            saveable.GetSaveData(saveData);
        }
        foreach (var item in saveData.characterPosDict)
        {
            Debug.Log(item.Key + "      " + item.Value);
        }
    }
    public void Load()
    {
        foreach (var saveable in saveableList)
        {
            saveable.LoadData(saveData);
            Debug.LogWarning("开始加载存储点");
        }
    }
}
