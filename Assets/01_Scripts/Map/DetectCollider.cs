using System.Collections.Generic;
using UnityEngine;

public class DetectCollider : MonoBehaviour
{
    public BossMonster boss;
    public List<Minion> minions;
    public List<Player> players;

    private void Update()
    {
        boss.minionList = minions;
        boss.playerList = players;
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
        }
        if (other.CompareTag("Player"))
        {
            print(other.gameObject.name);
            players.Add(other.gameObject.GetComponent<Player>());
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {   
            players.Remove(other.gameObject.GetComponent<Player>());
        }
    }
}
