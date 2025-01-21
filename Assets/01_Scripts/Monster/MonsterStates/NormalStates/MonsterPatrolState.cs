using UnityEngine;
using UnityEngine.AI;

public class MonsterPatrolState : MonsterState
{
    private Vector3 currentPatrolPoint;

    public override void EnterState(Monster monster)
    {
        monster.Agent.isStopped = false;
        monster.Animator.SetBool("IsMoving", true);
        monster.Agent.speed = monster.MoveSpeed;
        monster.Agent.stoppingDistance = 0.1f;
        SetNewPatrolPoint(monster);
    }

    public override void ExecuteState(Monster monster)
    {
        if (DetectPlayer(monster))
        {
            monster.ChangeState(MonsterStateType.Chase);
            return;
        }

        if (!monster.Agent.pathPending && 
            monster.Agent.remainingDistance < monster.Agent.stoppingDistance)
        {
            SetNewPatrolPoint(monster);
        }
    }
    
    public override void ExitState(Monster monster)
    {
        monster.Agent.isStopped = true;
        monster.Animator.SetBool("IsMoving", false);
        monster.Agent.ResetPath();
    }

    private void SetNewPatrolPoint(Monster monster)
    {
        Vector3 center = monster.patrolPoint;
        float range = monster.PatrolRange;
        
        Vector3 randomPoint = center + Random.insideUnitSphere * range;
        
        if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, range, NavMesh.AllAreas))
        {
            currentPatrolPoint = hit.position;
            monster.Agent.SetDestination(currentPatrolPoint);
        }
    }

    private bool DetectPlayer(Monster monster)
    {
        Collider[] playerColliders = Physics.OverlapSphere
            (monster.transform.position, monster.DetectionRange, monster.PlayerLayer);

        if (playerColliders.Length > 0)
        {
            monster.playerTransform = playerColliders[0].transform;
            return true;
        }
    
        return false;
    }
} 