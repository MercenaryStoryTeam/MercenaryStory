using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DetectCollider : MonoBehaviour
{
    public BossMonster boss;
    public List<Minion> minions;

    private void Update()
    {
        minions = boss.minionList;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print(other.gameObject.name);
            boss.playerList.Add(other.GetComponent<Player>());
            foreach (Minion minion in minions)
            {
                minion.playerList.Add(other.GetComponent<Player>());
            }
        }
        if (other.CompareTag("Minion"))
        {
            print(other.gameObject.name);
            boss.minionList.Add(other.GetComponent<Minion>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            boss.playerList.Remove(other.GetComponent<Player>());
            foreach (Minion minion in minions)
            {
                minion.playerList.Remove(other.GetComponent<Player>());
            }
        }
    }
}
