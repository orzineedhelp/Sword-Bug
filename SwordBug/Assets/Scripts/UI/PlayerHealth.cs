using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public List<Image> hearts;
    private int[] isFull=new int[3];//1满0半-1空
 

    private void Awake()
    {
        // 确保 hearts 列表已初始化
        if (hearts == null)
            hearts = new List<Image>();
        // 清空列表并重新添加
        hearts.Clear();

        for (int i = 0; i < 3; i++)
        {
            Transform heartTransform = transform.GetChild(i+3);
            if (heartTransform != null)
            {
                Image heartImage = heartTransform.GetComponent<Image>();
                if (heartImage != null)
                {
                    hearts.Add(heartImage);
                    // 初始化每颗心为满状态
                    heartImage.fillAmount = 1f;
                }
            }
        }
        for (int i = 0; i < 3; i++)
        {
            isFull[i] = 1;
        }


    }
    public void OnHealthChange(int num,bool isheal)
    {
        if (isheal)
        {
            Heal();
        }
       else
        {
            //共有6血，扣一则减半心，扣二则减全心
            switch (num)
            {
                case 0:

                    foreach (Image heart in hearts) heart.fillAmount = 1;
                    for (int i = 0; i < 3; i++) isFull[i] = 1;
                    break;
                case 1://扣半滴
                    hearts[0].fillAmount = 0.5f;
                    isFull[0] = 0;
                    break;
                case 2://扣一滴
                    hearts[0].fillAmount = 0;
                    isFull[0] = -1;
                    break;
                case 3:
                    hearts[1].fillAmount = 0.5f;
                    isFull[1] = 0;
                    break;
                case 4:
                    hearts[1].fillAmount = 0;
                    isFull[1] = -1;
                    break;
                case 5:
                    hearts[2].fillAmount = 0.5f;
                    isFull[2] = 0;
                    break;
                case 6:
                    hearts[2].fillAmount = 0;
                    isFull[2] = -1;
                    break;
                default:
                    break;
            }
        }
        
    }

    public void Heal()
    {
           
        // 从右向左寻找需要恢复的心形
        for (int i = 2; i >= 0; i--)
        {
            if (isFull[i] < 1)
            {
                if (isFull[i] == -1) // 空的心
                {
                    hearts[i].fillAmount = 0.5f;
                    isFull[i] = 0;
                }
                else if (isFull[i] == 0) // 半心
                {
                    hearts[i].fillAmount = 1f;
                    isFull[i] = 1;
                }
                break; // 只恢复半格
            }
        }

    }
}
