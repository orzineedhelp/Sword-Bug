using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private Vector3 rightPosition;
    [SerializeField] private Vector3 leftPosition;

    private SpriteRenderer playerSprite;

    private void Start()
    {
        playerSprite = GetComponentInParent<SpriteRenderer>();
    }

    private void Update()
    {
        // 根据玩家朝向调整武器位置
        if (playerSprite.flipX)
        {
            weaponPivot.localPosition = leftPosition;
        }
        else
        {
            weaponPivot.localPosition = rightPosition;
        }
    }
}
