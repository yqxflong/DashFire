using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class XiongRen_SkillThrowSpear : MonsterBaseSkill {

  public AnimationClip m_ThrowAnim;
  public AnimationClip m_IdleAnim;
  public float m_ThrowDelay;
  public GameObject SpearPrefab;
  public ImpactInfo m_ImpactInfo;

  private bool m_HasThrowSpear = false;
	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
      if (null == m_AnimationPlayer) {
        Debug.LogError("Skill must have AnimationManager");
      }
    }
	}
  public override void StartSkill() {
    GeneralStartSkill();
    if (null != m_ThrowAnim) {
      m_AnimationPlayer.Play(m_ThrowAnim.name);
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (!m_HasThrowSpear && Time.time > m_LastTriggerTime + m_ThrowDelay) {
        ThrowSpear();
        m_HasThrowSpear = true;
      }
      if (Time.time > m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_ThrowAnim.name)) {
        StopSkill();
      }
    }
	}

  private void ThrowSpear() {
    GameObject player = GameObject.FindGameObjectWithTag("Player");
    if (null != player) {
      Vector3 direction = (player.transform.position + new Vector3(0f, 0.5f, 0f)) - SpearPrefab.transform.position;
      SpearPrefab.transform.forward = -1 * direction.normalized;
      XiongRen_SkillSpear spearLogic = SpearPrefab.GetComponent<XiongRen_SkillSpear>();
      spearLogic.m_Sender = gameObject;
      spearLogic.m_ImpactInfo = m_ImpactInfo;
      spearLogic.m_IsFly = true;
      spearLogic.m_FlyStartTime = Time.time;
      spearLogic.m_FlyTime = direction.magnitude / spearLogic.m_ThrowSpeed;
      spearLogic.m_LocalRotation = SpearPrefab.transform.localRotation;
      if (null != SpearPrefab.transform.parent) {
        SpearPrefab.transform.parent.DetachChildren();
      }
    }
  }
  public override void StopSkill() {
    GeneralStopSkill();
    m_HasThrowSpear = false;
    if (null != m_IdleAnim) {
      m_AnimationPlayer.Play(m_IdleAnim.name);
    }
    
  }
}



