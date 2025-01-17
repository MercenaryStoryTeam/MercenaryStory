using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class DetectCollider : MonoBehaviour
{
    public BossMonster boss;
    public List<Minion> minions;
    public List<Player> players;

    private void Update()
    {
        foreach (Minion minion in minions)
        {
            minion.playerList = players;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minion"))
             {
                 print(other.gameObject.name);
                 minions.Add(other.GetComponent<Minion>());
                 boss.minionList.Add(other.GetComponent<Minion>());
             }
        if (other.CompareTag("Player"))
        {
            print(other.gameObject.name);
            players.Add(other.gameObject.GetComponent<Player>());
            boss.playerList.Add(other.GetComponent<Player>());
            foreach (Minion minion in minions)
            {
                minion.playerList.Add(other.GetComponent<Player>());
            }
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
