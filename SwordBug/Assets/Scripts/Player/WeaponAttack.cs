using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
        [Header("武器引用")]
        [SerializeField] private Transform weaponPivot;
        [SerializeField] private SpriteRenderer playerSpriteRenderer; // 玩家的SpriteRenderer

        [Header("攻击位移设置")]
        [SerializeField] private float attackMoveDistance = 0.5f;
        [SerializeField] private float attackMoveDuration = 0.2f;
        [SerializeField] private AnimationCurve attackCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private Vector3 weaponOriginalLocalPosition;
        private bool isAttacking = false;
        private Coroutine attackMoveCoroutine;

        private void Start()
        {
            if (weaponPivot != null)
            {
                weaponOriginalLocalPosition = weaponPivot.localPosition;
            }
            else
            {
                Debug.LogError("WeaponPivot 未分配！");
            }

            // 如果没有手动分配，尝试自动获取玩家的SpriteRenderer
            if (playerSpriteRenderer == null)
            {
                playerSpriteRenderer = GetComponent<SpriteRenderer>();
            }
        }

        // 动画事件调用的方法 - 攻击开始
        public void OnAttackStart()
        {
            if (weaponPivot == null) return;

            isAttacking = true;

            // 停止之前的协程
            if (attackMoveCoroutine != null)
                StopCoroutine(attackMoveCoroutine);

            attackMoveCoroutine = StartCoroutine(AttackMoveRoutine());
        }

        // 动画事件调用的方法 - 攻击结束
        public void OnAttackEnd()
        {
            if (weaponPivot == null) return;

            isAttacking = false;

            if (attackMoveCoroutine != null)
                StopCoroutine(attackMoveCoroutine);

            // 立即回到原位
            weaponPivot.localPosition = weaponOriginalLocalPosition;
        }

        // 攻击位移协程
        private IEnumerator AttackMoveRoutine()
        {
            Vector3 startPos = weaponPivot.localPosition;

            // 根据玩家朝向决定攻击方向
            bool isFacingLeft = IsPlayerFacingLeft();
            Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
            Vector3 targetPos = weaponOriginalLocalPosition + attackDirection * attackMoveDistance;

            float elapsedTime = 0f;

            // 向前移动阶段
            while (elapsedTime < attackMoveDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / attackMoveDuration;
                float curveValue = attackCurve.Evaluate(t);

                weaponPivot.localPosition = Vector3.Lerp(startPos, targetPos, curveValue);
                yield return null;
            }

            // 确保到达目标位置
            weaponPivot.localPosition = targetPos;

            // 短暂停留
            yield return new WaitForSeconds(0.05f);

            // 返回原位
            elapsedTime = 0f;
            startPos = weaponPivot.localPosition;

            while (elapsedTime < attackMoveDuration * 0.3f)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / (attackMoveDuration * 0.3f);
                weaponPivot.localPosition = Vector3.Lerp(startPos, weaponOriginalLocalPosition, t);
                yield return null;
            }

            weaponPivot.localPosition = weaponOriginalLocalPosition;
        }

        // 判断玩家朝向
        private bool IsPlayerFacingLeft()
        {
            // 方法1: 通过SpriteRenderer的flipX判断
            if (playerSpriteRenderer != null)
            {
                return playerSpriteRenderer.flipX;
            }

            // 方法2: 通过localScale判断（如果你同时使用scale翻转）
            // 如果使用scale翻转，可以取消注释下面的代码
            // return transform.localScale.x < 0;

            // 默认朝向右边
            return false;
        }

        // 调试方法
        private void OnDrawGizmosSelected()
        {
            if (weaponPivot != null)
            {
                bool isFacingLeft = IsPlayerFacingLeft();
                Vector3 attackDirection = isFacingLeft ? Vector3.left : Vector3.right;
                Vector3 worldStartPos = weaponPivot.parent.TransformPoint(weaponOriginalLocalPosition);
                Vector3 worldEndPos = worldStartPos + transform.TransformDirection(attackDirection) * attackMoveDistance;

                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldStartPos, worldEndPos);
                Gizmos.DrawWireSphere(worldEndPos, 0.1f);
            }
        }
    }
