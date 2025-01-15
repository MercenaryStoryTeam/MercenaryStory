using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class BossStageManager : MonoBehaviour
{
    public BossMonster bossMonster;
    private List<Player> Players = new List<Player>();

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        Gizmos.DrawWireCube(transform.position, new Vector3(20,3,20));
    }
}
