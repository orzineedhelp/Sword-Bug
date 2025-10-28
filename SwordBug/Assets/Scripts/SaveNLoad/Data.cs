using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

public class Data 
{
    public string sceneToSave;
    public Dictionary<string, Vector3> characterPosDict=new Dictionary<string, Vector3>();//保存坐标
    public Dictionary<string,float> floatSavedData=new Dictionary<string,float>();//保存血量与scale值
    public Dictionary<string,bool> boolSaveData=new Dictionary<string,bool>();//保存布尔值，清除玩家状态

    public void SaveGameScene(GameSceneSO savedScene)
    {

        sceneToSave=JsonUtility.ToJson(savedScene);//将object转换为json文件
    }

    public GameSceneSO GetSaveScene()//反序列化，将json文件转换回so
    {
        var newScene=ScriptableObject.CreateInstance<GameSceneSO>();
        JsonUtility.FromJsonOverwrite(sceneToSave, newScene);
        return newScene;
    }

}
