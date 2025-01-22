using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{

    [SerializeField] private StageData[] stageDatas;
    public int dieMonsterCount;
    public PlayerFsm hostPlayerFsm;
    public bool StageClear { get; private set; }
    public Transform spawnPoint;
    public int currentStage = 0;

    private void Start()
    {
        ServerManager.PlayerSpawn(spawnPoint);
        PlayStageBGM();
    }

    private void PlayStageBGM()
    {
        if (currentStage < stageDatas.Length)
        {
            SoundManager.Instance.PlayBGM(stageDatas[currentStage].bgmName);
        }
    }

    public void ChangeStage(int stageIndex)
    {
        if (stageIndex < stageDatas.Length)
        {
            currentStage = stageIndex;
            PlayStageBGM();
        }
    }

    public void Update()
    {
        if (dieMonsterCount == stageDatas[currentStage].monsterCount)
        {
            StageClear = true;
        }
    }
}