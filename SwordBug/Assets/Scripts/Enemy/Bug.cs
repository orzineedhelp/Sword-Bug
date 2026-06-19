using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyFSM))]
public class Bug : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        isFSMControlled = true;
    }

    public override void Move()
    {
        base.Move();
    }
}