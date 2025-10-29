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
    public TeleportPoint teleport;
    public GameObject mainUI;
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private void Awake()
    {
        pa=player.GetComponent<PlayerAnimation>();
        teleport = GetComponent<TeleportPoint>();

    }
    private void Update()
    {
        if (endDialogue.isEnd)
        {
            player.GetComponent<PlayerControl>().enabled=false;
            StartCoroutine(FlyToSky(11, 5f));
            if(mainUI != null)
            mainUI.SetActive(false);
            
        }
    }
   public IEnumerator FlyToSky(float targetY, float duration)
    {
        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = new Vector3(startPosition.x, targetY, startPosition.z);
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration;

            // 使用 AnimationCurve 控制缓动
            float curveValue = moveCurve.Evaluate(progress);

            player.transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);

            yield return null;
        }

        player.transform.position = targetPosition;
        pa.EndAni();
        yield return 1f;
        Addressables.LoadSceneAsync("END");
        //teleport.TriggerAction();
    }
}
