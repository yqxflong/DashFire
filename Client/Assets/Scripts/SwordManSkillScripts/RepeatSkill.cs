using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum TargetChoosePriority {
  kChooseMostForward,
  kChooseNearest,
}

public class RepeatSkill : SkillScript{
  public AnimationClip m_SkillAnim;
  public float m_AnimSpeed = 1.0f;
  public TargetChoosePriority m_TargetChoosePriority = TargetChoosePriority.kChooseMostForward;
  public float m_SectorRadius = 3;
  public float m_SectorDegree = 60;
  public int m_RepeatTimes = 1;

  private int m_CurTime = 0;
  private GameObject m_Target = null;

  // Use this for initialization
  void Start () {
    base.Init();
    if (m_ObjAnimation != null && m_SkillAnim != null) {
      m_ObjAnimation[m_SkillAnim.name].speed = m_AnimSpeed;
    }
    m_Status = MyCharacterStatus.kIdle;
  }

  // Update is called once per frame
  void Update()
  {
    if (!m_IsSkillActive) {
      return;
    }
    base.UpdateSkill();
    if (m_CurTime < m_RepeatTimes) {
      if (CanStopBackSwing()) {
        StopSkill();
      }
    }
  }

  public override bool CanStop() {
    if (IsActive()) {
      if (m_CurTime == m_RepeatTimes) {
        return base.CanStop();
      } else {
        return false;
      }
    }
    return base.CanStop();
  }

  public override bool StartSkill()
  {
    if (!CanStart()) {
      m_CurTime = m_RepeatTimes;
      return false;
    }
    m_CurTime += 1;
    if (m_CurTime > m_RepeatTimes) {
      m_CurTime = m_CurTime % m_RepeatTimes;
    }
    if (m_CurTime == 1) {
      List<GameObject> allobj = TargetChooser.FindTargetInSector(transform.position, m_SectorRadius,
                                                                 transform.forward, transform.position,
                                                                 m_SectorDegree);
      if (m_TargetChoosePriority == TargetChoosePriority.kChooseMostForward) {
        m_Target = TargetChooser.GetMostForwardObj(transform.position, transform.forward, FiltEnimy(allobj));
      } else {
        m_Target = TargetChooser.GetNearestObj(transform.position, FiltEnimy(allobj));
      }
    }
    if (m_Target == null) {
      m_CurTime = m_RepeatTimes;
      ForceStopSkill();
      return false;
    } else {
      m_SkillMovement.SetFacePos(m_Target.transform.position);
    }
    if (!base.StartSkill()) {
      m_CurTime = m_RepeatTimes;
      return false;
    }
    m_ObjAnimation[m_SkillAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_SkillAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_SkillAnim.name].wrapMode = WrapMode.ClampForever;
    if (m_CrossFadeTime == 0) {
      m_ObjAnimation.Play(m_SkillAnim.name);
    } else {
      m_ObjAnimation[m_SkillAnim.name].weight = m_StartWeight;
      m_ObjAnimation.CrossFade(m_SkillAnim.name, m_CrossFadeTime);
    }
    return true;
  }

  public override void StopSkill()
  {
    base.StopSkill();
    if (m_CurTime < m_RepeatTimes) {
      StartSkill();
    }
  }

  public override AnimationClip GetCurAnimationClip() {
    return m_SkillAnim;
  }

  protected override bool CheckSkillOver()
  {
    if (m_Status != MyCharacterStatus.kSkilling) {
      return true;
    }
    if (m_ObjAnimation[m_SkillAnim.name].normalizedTime >= 1) {
      m_ClampedTime += Time.deltaTime;
      if (m_ClampedTime > m_ClampTime) {
        return true;
      }
    }
    return false;
  }

  private List<GameObject> FiltEnimy(List<GameObject> list) {
    List<GameObject> result = new List<GameObject>();
    foreach (GameObject obj in list) {
      if (m_ColliderManager.IsEnimy(obj)) {
        result.Add(obj);
      }
    }
    return result;
  }
}

