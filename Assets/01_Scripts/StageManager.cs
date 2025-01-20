using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{
    public Monster monster;
    public BossMonster bossMonster;


    private void Awake()
    {
        monster = GetComponent<Monster>();
        bossMonster = GetComponent<BossMonster>();
    }
}
