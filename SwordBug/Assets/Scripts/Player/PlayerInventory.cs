using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("库存设置")]
    public Transform itemHolder;
    public Item currentItem;

    public bool HasItem()
    {
        return currentItem != null;
    }

    public bool HasItem(Tools itemType)
    {
        return currentItem != null && currentItem.itemType == itemType;
    }

    public void PickupItem(Item item)
    {
        if (currentItem != null)
        {
            Debug.Log("手中已有道具，无法拾取新的");
            return;
        }

        currentItem = item;
        item.Pickup(itemHolder);
        Debug.Log($"拾取道具: {item.itemType}");
    }

    public void UseCurrentItem()
    {
        if (currentItem == null)
        {
            Debug.Log("没有道具可使用");
            return;
        }

        currentItem.Use();
        currentItem = null;
    }

    public void DropCurrentItem()
    {
        if (currentItem == null) return;

        currentItem.ReturnToWorld();
        currentItem = null;

        Debug.Log("丢弃当前道具");
    }

    public void DestoryCurrentItem()
    {
        if (currentItem != null) return;
        currentItem.DestoryCurrentItem();
    }
}