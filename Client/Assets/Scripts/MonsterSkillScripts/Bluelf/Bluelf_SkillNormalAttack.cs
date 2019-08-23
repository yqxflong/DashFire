using UnityEngine;
using DashFire;
using System.Collections;
using System.Collections.Generic;

public class Bluelf_SkillNormalAttack : MonsterBaseSkill {

  public AnimationClip m_NormalAttackAnim;
  public AnimationClip m_IdleAnim;
  public GameObject m_BladeEffect;
  public float m_DamageRange = 2.0f;
  public float m_DamageAngle = 30.0f;
  private bool m_HasDamageSend = false;
  private bool m_HasMove = false;
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
    m_HasMove = false;
    if (null != m_BladeEffect) {
      GameObject bladeEffect = ResourceSystem.NewObject(m_BladeEffect, 2.0f) as GameObject;
      if (null != bladeEffect) {
        bladeEffect.transform.position = this.transform.position;
        bladeEffect.transform.rotation = this.transform.rotation;
      }
    }
    if (null != m_NormalAttackAnim) {
      m_AnimationPlayer.Play(m_NormalAttackAnim.name);
    } else {
      Debug.LogError("Can't find NormalAttackAnim, stop the skill");
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (Time.time > m_LastTriggerTime + m_DamageDelay && !m_HasDamageSend) {
        m_HasDamageSend = true;
        if (m_DamageAngle <= 0.1 || m_DamageRange <= 0.1)
          return;
        foreach(GameObject obj in GetObjInSector(this.transform.position, m_DamageRange, m_DamageAngle))
        {
          if (obj.tag == "Player") {
            ImpactInfo impact = m_ImpactInfo.Clone() as ImpactInfo;
            impact.Attacker = gameObject;
            impact.m_Velocity = this.transform.forward * impact.m_Velocity.z;
            ImpactSystem.Instance.SendImpact(gameObject, obj, impact as ImpactInfo);
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
