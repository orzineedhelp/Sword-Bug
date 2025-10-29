using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class GameOver : MonoBehaviour
{
    public DialogueTrigger endDialogue;
    public GameObject player;
    public PlayerAnimation pa;

    private void Awake()
    {
        pa=player.GetComponent<PlayerAnimation>();
    }
    private void Update()
    {
        if (endDialogue.isEnd)
        {
            player.GetComponent<PlayerControl>().enabled=false;
            StartCoroutine(FlyToSky());
            Addressables.LoadSceneAsync("END");
        }
    }
   public IEnumerator FlyToSky()
    {
        player.transform.DOMoveY(9, 2f);
        pa.EndAni();
        yield return 3f;

    }
}
