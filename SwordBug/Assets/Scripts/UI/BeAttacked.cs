using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeAttacked : MonoBehaviour
{
    private SpriteRenderer sr;
    public Sprite[] sprites;
    private int cnt;
    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = sprites[cnt];
        cnt = 1;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        if (collision.tag == "Attack"&&cnt<3)
        {
            sr.sprite=sprites[cnt++];
        }
        if (cnt == 3) Destroy(gameObject);
    }
}
