using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImpactStiffness : BaseImpact{

  // 僵直状态 
  public float m_StiffTime = 0.1f;  // 从正常动作到受击动作的过度时间。
  public float m_HitHoldTime = 0.1f;  // 受击保持时间。
  public float m_RecoverTime = 0.3f;  // 从受击动作动回复的时间。

  private float m_CurStiffTime = 0.1f;
  private float m_CurHitHoldTime = 0.1f;
  private float m_CurRecoverTime = 0.3f;
  private System.Random m_Random = new System.Random();
  public Vector3 m_Velocity = new Vector3();

  public AnimationClip m_FrontStiffStartAnim;  // 僵直动作
  public AnimationClip m_FrontStiffEndAnim;
  public AnimationClip m_FrontStiffStartAnimExtra;
  public AnimationClip m_FrontStiffEndAnimExtra;
  public AnimationClip m_BackStiffStartAnim;
  public AnimationClip m_BackStiffEndAnim;
  public AnimationClip m_BackStiffStartAnimExtra;
  public AnimationClip m_BackStiffEndAnimExtra;

  public AnimationClip m_IdleAnim;  // 站立动作
  public AnimationClip m_DeadAnim;  // 死亡动作。

  class AnimationDict {
    public AnimationClip StiffStartAnim;
    public AnimationClip StiffEndAnim;
    public AnimationClip IdleAnim;
  }

  private AnimationDict m_AnimationDict = new AnimationDict();
  public override ImpactType GetImpactType()
  {
    return ImpactType.Stiffness;
  }

	// Update is called once per frame
	void Update () {
    if (IsAcitve) {
      GeneralTickImpact(Time.deltaTime);
      if (true) {
        if (Time.time > m_StartTime + m_AnimationPlayer.AnimationLenth(m_FrontStiffStartAnim.name)) {
          StopImpact();
        }
      } else {
        if (Time.time > m_StartTime + m_CurStiffTime + m_CurHitHoldTime) {
          StopImpact();
        }
      }
      if (IsLogicDead()) {
        StopImpact();
      }
    }
	}

  public void StartImpact(ImpactInfo impactInfo) {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactStiffness::StartImpact -- impactInfo is null");
    }
    GeneralStartImpact(impactInfo);
    m_ImpactInfo.ApplyOffset(m_ImpactInfo.m_Offset);
    if (impactInfo.m_StiffnessConfig.m_UseCustom) {
      m_CurHitHoldTime = impactInfo.m_StiffnessConfig.m_StiffHoldTime;
      m_CurMovingTime = impactInfo.m_StiffnessConfig.m_MovingTime;
      m_CurStiffTime = impactInfo.m_StiffnessConfig.m_StiffTime;
    } else {
      m_CurRecoverTime = m_RecoverTime;
      m_CurStiffTime = m_StiffTime;
      m_CurHitHoldTime = m_HitHoldTime;
      m_CurMovingTime = m_CurRecoverTime + m_CurStiffTime + m_CurHitHoldTime;
    }
    SetAnimation(m_HitDirection);
    if (!gameObject.CompareTag("Player")) {
      SetFaceDir(new Vector3(-impactInfo.m_Velocity.x, 0, - impactInfo.m_Velocity.z));
    }
    if (true) {
      m_AnimationPlayer.StopAnimation(m_AnimationDict.StiffStartAnim.name);
      m_AnimationPlayer.Play(m_AnimationDict.StiffStartAnim.name);
    } else {
      if (null != m_AnimationDict.StiffStartAnim) {
        m_AnimationPlayer.Play(m_AnimationDict.StiffStartAnim.name);
      }
      if (null != m_AnimationDict.StiffEndAnim) {
        m_AnimationPlayer.CrossFade(m_AnimationDict.StiffEndAnim.name, m_CurStiffTime);
      }
    }
  }

  private void SetAnimation(HitDirection direction) {
    if (0 == m_Random.Next(0, 2)) {
      if (HitDirection.FRONT == direction) {
        m_AnimationDict.StiffStartAnim = m_FrontStiffStartAnim;
        m_AnimationDict.StiffEndAnim = m_FrontStiffEndAnim;
        m_AnimationDict.IdleAnim = m_IdleAnim;
      } else if (HitDirection.BACK == direction) {
        m_AnimationDict.StiffStartAnim = m_BackStiffStartAnim;
        m_AnimationDict.StiffEndAnim = m_BackStiffEndAnim;
        m_AnimationDict.IdleAnim = m_IdleAnim;
      }
    } else {
      if (HitDirection.FRONT == direction) {
        m_AnimationDict.StiffStartAnim = m_FrontStiffStartAnimExtra;
        m_AnimationDict.StiffEndAnim = m_FrontStiffEndAnimExtra;
        m_AnimationDict.IdleAnim = m_IdleAnim;
      } else if (HitDirection.BACK == direction) {
        m_AnimationDict.StiffStartAnim = m_BackStiffStartAnimExtra;
        m_AnimationDict.StiffEndAnim = m_BackStiffEndAnimExtra;
        m_AnimationDict.IdleAnim = m_IdleAnim;
      }
    }
  }
  public override void StopImpact() {
    GeneralStopImpact();
    if (IsLogicDead()) {
      NotifyNpcDead(gameObject);
      if (null != m_DeadAnim) {
        m_AnimationPlayer.CrossFade(m_DeadAnim.name);
      }
    } else {
      if (null != m_AnimationDict.IdleAnim) {
        m_AnimationPlayer.CrossFade(m_AnimationDict.IdleAnim.name, m_CurRecoverTime);
      }
    }
  }
  private void SetFaceDir(Vector3 direction)
  {
    direction.Normalize();
    this.transform.forward = new Vector3(direction.x, 0, direction.z);
  }
}
