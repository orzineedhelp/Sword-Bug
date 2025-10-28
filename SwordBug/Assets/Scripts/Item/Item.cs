using UnityEngine;

public class Item : MonoBehaviour
{
    public Tools itemType;

    private Collider2D itemCollider;
    public  AudioDefination audioUse;
    public AudioDefination audioPick;
    private SpriteRenderer spriteRenderer;
    private Vector3 originalPosition;
    private Transform originalParent;

    public GameObject lightbg;

    private void Awake()
    {
        itemCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalPosition = transform.position;
        originalParent = transform.parent;

     
    }
  
    public void Pickup(Transform newParent)
    {
        
        //记录原位置
        originalPosition = transform.position;
        originalParent = transform.parent;
        //将物体设为子物体与玩家一起行动
        transform.SetParent(newParent);
        transform.localPosition = Vector3.zero;
        if (lightbg != null)
        {
            lightbg.SetActive(false);
        }
        audioPick.PlayAudioClip();
        transform.localRotation = Quaternion.identity;

        if (itemCollider != null) itemCollider.enabled = false;

        Debug.Log($"拾取道具: {itemType}");
    }

    public void Use()
    {
        Debug.Log($"使用道具: {itemType}");

        if (itemType == Tools.Sword)
        {
            audioUse.PlayAudioClip();

            ReturnToWorld();
        }
       
    }

    public void ReturnToWorld()
    {
        Transform player = transform.parent;
        if (player != null)
        {
            transform.position = player.position;
        }
        else
        {
            transform.position = originalPosition;
        }

        transform.SetParent(originalParent);

        if (itemCollider != null) itemCollider.enabled = true;


        Debug.Log($"道具已放回场景");
    }
    public void DestoryCurrentItem()
    {
        Debug.Log("销毁！");
        
        Destroy(this.gameObject);
    }

}