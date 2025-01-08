using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ThiefRat : Monster
{
    public int _hp = 10;
    public int _maxHp = 10;
    public int _damage = 3;
    public float _moveSpeed = 1;
    public float _rotationSpeed = 1;
    public float _attackSpeed = 1;
    public Vector3 _patrolPoint = new Vector3(1, 1, 1);
    public float _patrolRange = 3;
    public float _detectionRange = 3;
    public float _attackRange = 2;
    public Transform _patrolArea;
    private MonsterState currentState;
    private ThiefRat _thiefRat;

    public void Awake()
    {
        _thiefRat = gameObject.GetComponent<ThiefRat>();
        Hp = _hp;
        MaxHp = _maxHp;
        Damage = _damage;
        MoveSpeed = _moveSpeed;
        RotationSpeed = _rotationSpeed;
        AttackSpeed = _attackSpeed;
        PatrolRange = _patrolRange;
        DetectionRange = _detectionRange;
        AttackRange = _attackRange;
        PatrolPoint = _patrolPoint;
        _patrolArea.position = PatrolPoint;
        _patrolArea.localScale = new Vector3(PatrolRange, PatrolRange, 1);
    }

}
