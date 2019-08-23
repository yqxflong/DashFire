using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class LockFrameInfo
{
  public LockFrameInfo(string str)
  {
    string[] param_list = str.Split(' ');
    try {
      if (param_list.Length >= 3) {
        m_IsNeedCollide = Convert.ToBoolean(param_list[0]);
        m_RemainTime = (float)Convert.ToDouble(param_list[1]);
        m_AfterLockAnimSpeed = (float)Convert.ToDouble(param_list[2]);
      }
    } catch (Exception ex) {
      Debug.Log("parse lock frame info error, use default value! " + ex.Message);
    }
  }

  public AnimationClip m_LockedAnim = null;
  public bool m_IsNeedCollide = false;
  public float m_RemainTime = 0.1f;
  public float m_AfterLockAnimSpeed = 0.5f;
  public float m_AnimTime = 0;
  public bool m_IsPaused = false;
}

public class LockFrame : MonoBehaviour {
  private Animation m_Animation;
  private SkillManager m_SkillManager;
  private LockFrameInfo m_LockFramInfo;

	// Use this for initialization
	void Start () {
    m_Animation = gameObject.GetComponent<Animation>();
    m_SkillManager = gameObject.GetComponent<SkillManager>();
	}

	// Update is called once per frame
	void Update () {
    UpdateLockFrame();
	}

  public void LockCurAnimsFrame(string lockinfoStr)
  {
    m_LockFramInfo = new LockFrameInfo(lockinfoStr);
    if (m_LockFramInfo.m_IsNeedCollide) {
      SkillColliderManager manager = gameObject.GetComponent<SkillColliderManager>();
      if (manager == null || manager.GetColliders().Count <= 0) {
        return;
      }
    }
    AnimationClip cur_anim = GetCurPlayingAnims();
    if (cur_anim == null) {
      return;
    }
    m_LockFramInfo.m_LockedAnim = cur_anim;
    m_LockFramInfo.m_AnimTime = m_Animation[cur_anim.name].time;
    m_LockFramInfo.m_IsPaused = false;
    if (m_LockFramInfo.m_RemainTime == 0) {
      SetLockAnimsSpeed(m_LockFramInfo.m_AfterLockAnimSpeed);
      m_LockFramInfo = null;
    }
  }

  public void SetCurAnimSpeed(float speed) {
    AnimationClip cur_anim = GetCurPlayingAnims();
    if (cur_anim == null) {
      return;
    }
    m_Animation[cur_anim.name].speed = speed;
  }

  private AnimationClip GetCurPlayingAnims()
  {
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss != null) {
      return ss.GetCurAnimationClip();
    }
    return null;
  }

  private void UpdateLockFrame()
  {
    if (m_LockFramInfo == null) {
      return;
    }
    if (m_LockFramInfo.m_RemainTime == 0) {
      return;
    }
    if (!m_LockFramInfo.m_IsPaused) {
      SetLockAnimsSpeed(0);
      m_LockFramInfo.m_IsPaused = true;
    }
    m_LockFramInfo.m_RemainTime -= Time.deltaTime;
    if (m_LockFramInfo.m_RemainTime <= 0) {
      SetLockAnimsSpeed(m_LockFramInfo.m_AfterLockAnimSpeed);
      m_LockFramInfo.m_RemainTime = 0;
      m_LockFramInfo = null;
    }
  }

  private void SetLockAnimsSpeed(float speed)
  {
    if (m_LockFramInfo.m_LockedAnim != null) {
      m_Animation[m_LockFramInfo.m_LockedAnim.name].speed = speed;
    }
  }
}
