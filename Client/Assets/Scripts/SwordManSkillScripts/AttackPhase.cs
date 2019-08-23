using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum MyCharacterStatus {
  kIdle = 0,
  kWalk = 1,
  kTrotting = 2,
  kRuning = 3,
  kJumping = 4,
  kSkilling = 5
}

public class AttackPhase : SkillScript{
  public AnimationClip m_skillAnim;
  public float m_AnimSpeed = 1.0f;

  // Use this for initialization
  void Start () {
    base.Init();
    if (m_ObjAnimation != null && m_skillAnim != null) {
      m_ObjAnimation[m_skillAnim.name].speed = m_AnimSpeed;
    }
    m_Status = MyCharacterStatus.kIdle;
  }

  // Update is called once per frame
  void Update()
  {
    base.UpdateSkill();
  }

  public override bool StartSkill()
  {
    if (!base.StartSkill()) {
      return false;
    }
    m_ObjAnimation[m_skillAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_skillAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_skillAnim.name].wrapMode = WrapMode.ClampForever;
    if (m_CrossFadeTime == 0) {
      m_ObjAnimation.Play(m_skillAnim.name);
    } else {
      m_ObjAnimation[m_skillAnim.name].weight = m_StartWeight;
      m_ObjAnimation.CrossFade(m_skillAnim.name, m_CrossFadeTime);
    }
    return true;
  }

  public override AnimationClip GetCurAnimationClip() {
    return m_skillAnim;
  }

  protected override bool CheckSkillOver()
  {
    if (m_Status != MyCharacterStatus.kSkilling) {
      return true;
    }
    if (m_ObjAnimation[m_skillAnim.name].normalizedTime >= 1) {
      m_ClampedTime += Time.deltaTime;
      if (m_ClampedTime > m_ClampTime) {
        return true;
      }
    }
    return false;
  }
}

