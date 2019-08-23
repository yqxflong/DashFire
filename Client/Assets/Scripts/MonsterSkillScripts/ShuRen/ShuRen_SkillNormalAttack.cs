using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShuRen_SkillNormalAttack : MonsterBaseSkill {

  public AnimationClip m_NormalAttackAnim;
  public AnimationClip m_IdleAnim;
  public float m_DamageRange = 2.0f;
  public float m_DamageAngle = 30.0f;
  private bool m_HasDamageSend = false;
  public ImpactInfo m_ImpactInfo;
  public float m_DamageDelay = 1.0f;

	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
      if (null != m_AnimationPlayer) {
        Debug.LogError("Skill must have AnimationManager");
      }
    }
	}
  public override void StartSkill() {
    GeneralStartSkill();
    m_HasDamageSend = false;
    if (null != m_NormalAttackAnim) {
      m_AnimationPlayer.Play(m_NormalAttackAnim.name);
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (Time.time > m_LastTriggerTime + m_DamageDelay && !m_HasDamageSend) {
        m_HasDamageSend = true;
        foreach(GameObject obj in GetObjInSector(this.transform.position, m_DamageRange, m_DamageAngle))
        {
          if (obj.tag == "Player") {
            ImpactSystem.Instance.SendImpact(gameObject, obj, m_ImpactInfo.Clone() as ImpactInfo);
          }
        }
      }
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
}

