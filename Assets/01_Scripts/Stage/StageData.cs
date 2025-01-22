using UnityEngine;


[CreateAssetMenu(fileName = "StageData", menuName = "Stage Data")]
public class StageData : ScriptableObject
{
    public string stageName;
    public string bgmName;
    public int monsterCount;
    public string nextStageName;
}