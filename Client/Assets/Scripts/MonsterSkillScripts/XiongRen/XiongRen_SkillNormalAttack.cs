using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XiongRen_SkillNormalAttack : MonsterBaseSkill {

  public AnimationClip m_NormalAttackAnim;
  public AnimationClip m_IdleAnim;
  public float m_DamageRange = 1.0f;
  public float m_DamageAngle = 30.0f;
  public ImpactInfo m_ImpactInfo;

	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      Debug.LogError("Skill must have AnimationManager");
    }
	}
  public override void StartSkill() {
    GeneralStartSkill();
    if (null != m_NormalAttackAnim) {
      m_AnimationPlayer.Play(m_NormalAttackAnim.name);
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (Time.time > m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_NormalAttackAnim.name)) {
        StopSkill();
      }
    }
	}

  public override void StopSkill() {
    GeneralStopSkill();
    if (null != m_IdleAnim) {
      m_AnimationPlayer.Play(m_IdleAnim.name);
    }
    
  }
  public void OnAnimationEvent_NormalAttackHitCheck() {
  }
}


