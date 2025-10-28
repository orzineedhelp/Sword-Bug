using System.Collections;
using UnityEngine;

public class DialogueEffects : MonoBehaviour
{
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.position;
    }

    // 修改方法，添加强度参数
    public IEnumerator ShakeDialogueBox(float intensity)
    {
        float elapsed = 0f;
        float duration = 0.5f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            transform.position = originalPosition + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }
}