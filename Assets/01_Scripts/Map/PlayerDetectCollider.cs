using UnityEngine;

public class PlayerDetectCollider : MonoBehaviour
{
    public BossMonster bossMonster;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            print(other.gameObject.name);
            bossMonster.playerList.Add(other.GetComponent<Player>());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            bossMonster.playerList.Remove(other.GetComponent<Player>());
        }
    }
}
