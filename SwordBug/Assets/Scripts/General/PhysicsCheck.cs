using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PhysicsCheck : MonoBehaviour
{//进行物理检查
    public CapsuleCollider2D coll;
    [Header("检测参数")]
    public bool manual;
    public Vector2 bottomOffset;
    public Vector2 topOffset;
    public Vector2 leftOffset;
    public Vector2 rightOffset;
    [Header("检测是否触碰")]
    public bool isGround;
    public bool isCelling;
    public bool touchLeftWall;
    public bool touchRightWall;
    [Header("检测位置")]
    [SerializeField]private Transform ground;
    [SerializeField]private Transform celling;
    [SerializeField] private Transform left;

    public LayerMask groundLayer;
    public LayerMask cellingLayer;
    public float checkRaduis=0.2f;
    public float checkRaduis_Celling = 0.2f;

    
    private void InitializeCheckPoints()
    {
        // 如果检测点未在Inspector中赋值，则自动获取子物体
        if (ground == null && transform.childCount > 0)
        {
            ground = transform.GetChild(0);
        }

        if (celling == null && transform.childCount > 1)
        {
            celling = transform.GetChild(1);
        }

        if(left==null && transform.childCount>2)
        {
            left = transform.GetChild(2);
        }

      

        // 如果仍然为空，记录警告
        if (ground == null)
        {
            Debug.LogWarning("地面检测点未设置，请在Inspector中赋值或确保有子物体", this);
        }
        if (celling == null)
        {
            Debug.LogWarning("天花板检测点未设置，请在Inspector中赋值或确保有子物体", this);
        }
        if (left == null)
        {
            Debug.LogWarning("左右检测点未设置，请在Inspector中赋值或确保有子物体", this);
        }
        
    }
    private void Awake()
    {
        InitializeCheckPoints();
        coll=GetComponent<CapsuleCollider2D>();
        if (!manual)
        {
            rightOffset = new Vector2((coll.bounds.size.x + coll.offset.x / 2), coll.bounds.size.y / 2);
            leftOffset = new Vector2(-rightOffset.x, rightOffset.y);
        }
    }
   

    void Update()
    {
        Check();
    }

    public void Check()
    {
        isGround = Physics2D.OverlapCircle((Vector2)ground.position+bottomOffset, checkRaduis,groundLayer);
        isCelling = Physics2D.OverlapCircle((Vector2)celling.position+topOffset, checkRaduis,cellingLayer);
        touchLeftWall=Physics2D.OverlapCircle((Vector2)left.position+leftOffset, checkRaduis,groundLayer);
        touchRightWall=Physics2D.OverlapCircle((Vector2)left.position+rightOffset, checkRaduis,groundLayer);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere((Vector2)ground.position + bottomOffset, checkRaduis);
        Gizmos.DrawWireSphere((Vector2)celling.position + topOffset, checkRaduis_Celling);
        Gizmos.DrawWireSphere((Vector2)left.position + leftOffset, checkRaduis);
        Gizmos.DrawWireSphere((Vector2)left.position + rightOffset, checkRaduis);
    }
}
