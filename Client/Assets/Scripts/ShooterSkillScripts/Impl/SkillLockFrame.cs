using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class SkillLockFrame : MonoBehaviour {
  private Animation m_AnimCom = null;
  private SkillLockFrameInfo m_Info = null;
  private float m_Elapsed = 0;

  internal void Update() {
    if (CheckInfoValid()) {
      m_Elapsed += Time.deltaTime;
      if (m_Info.AnimUseCurve && m_Info.AnimSpeedCurve != null && m_Info.Duration > 0) {
        float percent = Mathf.Min(m_Elapsed / m_Info.Duration, 1.0f);
        float curSpeed = m_Info.AnimSpeedCurve.Evaluate(percent);
        SetAnimSpeed(curSpeed);
      }
      if (m_Elapsed > m_Info.Duration) {
        SetAnimSpeed(m_Info.AnimSpeedReset);
      }
    } else {
      m_Elapsed = 0;
      m_Info = null;
    }
  }
  public void StartLockFrame(SkillLockFrameInfo info) {
    m_Info = info;
    m_Elapsed = 0;
    if (m_Info.AnimAsset != null) {
      m_Info.AnimName = m_Info.AnimAsset.name;
    }
    if (CheckInfoValid()) {
      float curSpeed = m_Info.AnimSpeed;
      if (m_Info.AnimUseCurve && m_Info.AnimSpeedCurve != null) {
        curSpeed = m_Info.AnimSpeedCurve.Evaluate(0);
      }
      SetAnimSpeed(curSpeed);
    }
  }
  private bool CheckInfoValid() {
    UpdateAnimCom();
    if (m_Info == null || m_Info.Duration <= 0 || string.IsNullOrEmpty(m_Info.AnimName)) {
      return false;
    }
    if (m_Elapsed > m_Info.Duration) {
      return false;
    }
    if (m_AnimCom == null) {
      m_AnimCom = this.gameObject.GetComponent<Animation>();
    }
    if (m_AnimCom == null || m_AnimCom[m_Info.AnimName] == null) {
      return false;
    }
    if (!(m_AnimCom.IsPlaying(m_Info.AnimName) && m_AnimCom[m_Info.AnimName].normalizedTime < 1.0f)) {
      return false;
    }

    return true;
  }
  public void UpdateAnimCom() {
    if (m_AnimCom == null) {
      m_AnimCom = this.gameObject.GetComponent<Animation>();
    }
  }
  public void SetAnimSpeed(float speed) {
    if (m_AnimCom == null || m_AnimCom[m_Info.AnimName] == null) {
      return;
    }
    m_AnimCom[m_Info.AnimName].speed = speed;
  }
}
