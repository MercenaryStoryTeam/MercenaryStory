using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }
    
    public List<Monster> monster;
    public PlayerFsm playerFsm;
    public bool StageClear { get; private set; }
    public Transform spawnPoint;
    
    protected void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    public void Update()
    {
        if (monster.Count <= 0)
        {
            StageClear = true;
        }
    }
}
