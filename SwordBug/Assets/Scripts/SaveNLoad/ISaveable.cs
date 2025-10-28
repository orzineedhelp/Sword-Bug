using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISaveable //接口类，但是新c#可实现写方法
{
    DataDefinition GetDataID();

    void RegisterSaveData() 
    {
        DataManager.instance.RegisterSaveData(this);
    }
    void UnRegistSaveData() => DataManager.instance.UnRegisterSaveData(this);
    void GetSaveData(Data data);
    void LoadData(Data data);
}
