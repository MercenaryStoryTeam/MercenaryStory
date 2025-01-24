using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CooldownBar : MonoBehaviour
{
    // 스킬 타입 열거형
    public enum SkillType
    {
        Rush,
        Parry,
        Skill1,
        Skill2
    }

    // 스킬과 관련된 이미지 묶음 클래스
    [System.Serializable]
    public class SkillCooldown
    {
        public SkillType skillType; // 스킬 타입
        public Image cooldownImage; // 쿨타임 이미지
    }

    // 스킬-이미지 리스트
    [SerializeField] private List<SkillCooldown> skillCooldowns;
}
