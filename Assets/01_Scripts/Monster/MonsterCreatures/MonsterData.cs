using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "MonsterData")]
public class MonsterData : ScriptableObject
{
    [Header("몬스터 스텟")]
    public int hp;
    public int maxHp;
    public int damage;
    public float moveSpeed;
    public float rotationSpeed;
    public float attackSpeed;
    
    [Header("범위")]
    public float patrolRange;
    public float detectionRange;
    public float attackRange;
    public float returnRange;

    [Header("보상 골드")]
    public float goldReward;
    [Header("보상 아이템")]
    public List<ItemBase> dropItems;
}
