using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Monster : MonoBehaviour
{
    private int hp;
    private int maxHp;
    private int damage;
    private float speed;
    private float patrolRange;
    private float detectRange;
    private float attackRange;
    
    public int Hp
    {
        set => hp = Mathf.Max(0, value);
        get => hp;
    }
    public int MaxHp
    {
        set => maxHp = Mathf.Max(0, value);
        get => maxHp;
    }
    public int Damage
    {
        set => damage = Mathf.Max(0, value);
        get => damage;
    }
    public float Speed
    {
        set => speed = Mathf.Max(0, value);
        get => speed;
    }
    public float PatrolRange
    {
        set => patrolRange = Mathf.Max(0, value);
        get => patrolRange;
    }
    public float DetectRange
    {
        set => detectRange = Mathf.Max(0, value);
        get => detectRange;
    }
    public float AttackRange
    {
        set => attackRange = Mathf.Max(0, value);
        get => attackRange;
    }

    public abstract void Idle();

    public abstract void Patrol();

    public abstract void Detect();

    public abstract void Chase();

    public abstract void Attack();

    public abstract void GetBack();

    public abstract void Die();
    
    public void TakeDamage(int damage)
    {
        Hp -= damage;
    }

    
}