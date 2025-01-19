using UnityEngine;
using System.Collections.Generic;

public class ThiefRat : Monster
{
    [Header("몬스터 스텟")]
    public int _hp = 10;
    public int _maxHp = 10;
    public int _damage = 3;
    public float _moveSpeed = 1;
    public float _rotationSpeed = 1;
    public float _attackSpeed = 1;
    
    [Header("순찰")]
    public Vector3 _patrolPoint = new Vector3(1, 1, 1);
    public float _patrolRange = 3;
    
    [Header("범위")]
    public float _detectionRange = 3;
    public float _attackRange = 2;
    public float _returnRange = 10;
    
    public void Awake()
    {
        Hp = _hp;
        MaxHp = _maxHp;
        Damage = _damage;
        MoveSpeed = _moveSpeed;
        RotationSpeed = _rotationSpeed;
        AttackSpeed = _attackSpeed;
        PatrolRange = _patrolRange;
        DetectionRange = _detectionRange;
        AttackRange = _attackRange;
        ReturnRange = _returnRange;
        PatrolPoint = _patrolPoint;
    }
}
