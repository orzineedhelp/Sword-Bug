using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "GameScene/GameSceneSO")]

public class GameSceneSO : ScriptableObject
{
    public AssetReference sceneReference;
    public SceneType sceneType;
}
