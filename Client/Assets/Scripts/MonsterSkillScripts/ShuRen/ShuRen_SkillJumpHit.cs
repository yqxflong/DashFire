using UnityEngine;
using System.Collections;

public class ShuRen_SkillJumpHit : MonsterBaseSkill {

  public AnimationClip m_JumpHitAnim;
  public AnimationClip m_IdleAnim;


	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      Debug.LogError("Skill must have AnimationManager");
    }
	}
  public override void StartSkill() {
    GeneralStartSkill();
    if (null != m_JumpHitAnim) {
      m_AnimationPlayer.Play(m_JumpHitAnim.name);
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (Time.time > m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_JumpHitAnim.name)) {
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


