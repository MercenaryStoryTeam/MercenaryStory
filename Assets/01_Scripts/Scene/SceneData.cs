using UnityEngine;

[CreateAssetMenu(fileName = "SceneData", menuName = "SceneData")]
public class SceneData : ScriptableObject
{
	public string bgmName;
	public int monsterCount;
	public string nextSceneName;
}