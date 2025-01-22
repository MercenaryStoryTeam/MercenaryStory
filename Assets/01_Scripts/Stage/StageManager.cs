using System.Collections.Generic;
using UnityEngine;

public class StageManager : SingletonManager<StageManager>
{

    public StageData[] stageDatas;
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
    
    public void Update()
    {
        if (dieMonsterCount == stageDatas[currentStage].monsterCount)
        {
            StageClear = true;
        }
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
            dieMonsterCount = 0;
            StageClear = false;
            currentStage = stageIndex;
            PlayStageBGM();
        }
    }
}