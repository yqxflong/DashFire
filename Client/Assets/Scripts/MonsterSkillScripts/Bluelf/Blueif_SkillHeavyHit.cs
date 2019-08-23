using UnityEngine;
using DashFire;
using System.Collections;
using System.Collections.Generic;

public class Blueif_SkillHeavyHit : MonsterBaseSkill {

  public AnimationClip m_MoveAnim;
  public AnimationClip m_HitAnim;
  public AnimationClip m_IdleAnim;
  public GameObject m_AttackEffect;
  public float m_MoveSpeed;
  public float m_DamageRange = 2.0f;
  public float m_DamageAngle = 30.0f;
  private bool m_HasDamageSend = false;
  private bool m_HasMove = false;
  private CharacterController m_MoveController;
  public ImpactInfo m_ImpactInfo;
  public float m_DamageDelay = 1.0f;

	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
      if (null == m_AnimationPlayer) {
        Debug.LogError("Skill must have AnimationManager");
      }
    }
    m_MoveController = gameObject.GetComponent<CharacterController>();
	}
  public override void StartSkill() {
    GeneralStartSkill();
    m_HasDamageSend = false;
    m_HasMove = false;
    if (null != m_MoveAnim) {
      m_AnimationPlayer.Play(m_MoveAnim.name);
    }
    if (null != m_AttackEffect) {
      GameObject obj = ResourceSystem.NewObject(m_AttackEffect, 2.0f) as GameObject;
      if (null != obj) {
        obj.transform.position = this.transform.position;
        obj.transform.rotation = this.transform.rotation;
      }
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (Time.time < m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_MoveAnim.name)) {
        m_MoveController.Move(Time.deltaTime * m_MoveSpeed * this.transform.forward);
      LogicSystem.NotifyGfxUpdatePosition(gameObject, this.transform.position.x, this.transform.position.y, this.transform.position.z, 0, this.transform.rotation.eulerAngles.y * Mathf.PI / 180f, 0);
      } else if(Time.time < m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_MoveAnim.name) + m_AnimationPlayer.AnimationLenth(m_HitAnim.name)) {
        if (!m_AnimationPlayer.IsPlaying(m_HitAnim.name)) {
          m_AnimationPlayer.CrossFade(m_HitAnim.name);
        }
      } else {
        StopSkill();
      }
      if (Time.time > m_LastTriggerTime + m_DamageDelay && !m_HasDamageSend) {
        m_HasDamageSend = true;
        if (m_DamageAngle <= 0.1 || m_DamageRange <= 0.1)
          return;
        foreach(GameObject obj in GetObjInSector(this.transform.position - this.transform.forward * 0.0f, m_DamageRange, m_DamageAngle))
        {
          if (obj.tag == "Player") {
            ImpactInfo impact = m_ImpactInfo.Clone() as ImpactInfo;
            impact.Attacker = gameObject;
            impact.m_Velocity = this.transform.forward * impact.m_Velocity.z;
            ImpactSystem.Instance.SendImpact(gameObject, obj, impact as ImpactInfo);
          }
        }
      }
    }
	}

  public override void StopSkill() {
    GeneralStopSkill();
    if (null != m_IdleAnim) {
      m_AnimationPlayer.CrossFade(m_IdleAnim.name);
    }
  }
}

