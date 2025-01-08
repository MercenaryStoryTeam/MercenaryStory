using UnityEngine;

public class MonsterPatrolState : MonsterState
{
    private Vector3 currentPatrolPoint;

    public override void EnterState(Monster monster)
    {
        SetNewPatrolPoint(monster);
    }

    public override void ExecuteState(Monster monster)
    {
        // 플레이어 감지
        if (DetectPlayer(monster))
        {
            monster.ChangeState(MonsterStateType.Chase);
            return;
        }

        // 순찰
        float distanceToTarget = Vector3.Distance(monster.transform.position, currentPatrolPoint);
        if (distanceToTarget < 0.5f)
        {
            SetNewPatrolPoint(monster);
        }

        Vector3 direction = (currentPatrolPoint - monster.transform.position).normalized;
        monster.transform.position += direction * (monster.MoveSpeed * Time.deltaTime);
        monster.transform.rotation = Quaternion.Lerp(
            monster.transform.rotation,
            Quaternion.LookRotation(direction),
            monster.RotationSpeed * Time.deltaTime
        );
    }

    public override void ExitState(Monster monster)
    {
    }

    private void SetNewPatrolPoint(Monster monster)
    {
        Vector3 center = monster.PatrolPoint;
        float range = monster.PatrolRange;
        
        float randomX = Random.Range(center.x - range, center.x + range);
        float randomZ = Random.Range(center.z - range, center.z + range);
        
        currentPatrolPoint = new Vector3(randomX, monster.transform.position.y, randomZ);
    }

    private bool DetectPlayer(Monster monster)
    {
        // 플레이어 감지 로직 구현
        // Physics.OverlapSphere 등을 사용
        return false;
    }
} 