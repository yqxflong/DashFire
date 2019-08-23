using UnityEngine;
using System.Collections;

public class ImpactKnockDown : BaseImpact {

	// Use this for initialization
  public float m_FallDownTime = 0.3f;
  public float m_OnLandHoldTime = 0.0f;
  private float m_CurFallDownTime;
  private float m_CurOnLandHoldTime;
  public AnimationClip m_FrontFallDownAnim;
  public AnimationClip m_FrontHitGroundAnim;
  public AnimationClip m_FrontOnLandHoldAnim;
  public AnimationClip m_FrontStandUpAnim;
  public AnimationClip m_BackFallDownAnim;
  public AnimationClip m_BackHitGroundAnim;
  public AnimationClip m_BackOnLandHoldAnim;
  public AnimationClip m_BackStandUpAnim;
  public AnimationClip m_IdleAnim;
  public bool m_IsGetUpEndure = false;
  private bool m_HasSetEndure = false;

  class AnimationDict {
    public AnimationClip FallDownAnim;
    public AnimationClip HitGroundAnim;
    public AnimationClip OnLandHoldAnim;
    public AnimationClip StandUpAnim;
    public AnimationClip IdleAnim;
  }

  private AnimationDict m_AnimationDict = new AnimationDict();
  private State m_State;

  enum State {
    FallDown,
    HitGround,
    OnLand,
    StandUp,
  }
  public override ImpactType GetImpactType(){
    if (State.StandUp == m_State) {
      return ImpactType.UnKnown;
    }
    return ImpactType.KnockDown;
  }

  public void StartImpact(ImpactInfo impactInfo) {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactKnockDown::StartImpact -- impactInfo is null");
    }
    GeneralStartImpact(impactInfo);
    SetAnimation(m_HitDirection);
    if (impactInfo.m_KnockDownConfig.m_UseCustom) {
      m_CurFallDownTime = impactInfo.m_KnockDownConfig.m_FallDownTime;
      m_CurOnLandHoldTime = impactInfo.m_KnockDownConfig.m_OnLandHoldTime;
      m_CurMovingTime = impactInfo.m_KnockDownConfig.m_MovingTime;
    } else {
      m_CurFallDownTime = m_FallDownTime;
      m_CurOnLandHoldTime = m_OnLandHoldTime;
      m_CurMovingTime = m_MovingTime;
    }
    m_AnimationPlayer.CrossFade(m_AnimationDict.FallDownAnim.name, m_CurFallDownTime);
    m_State = State.FallDown;
    m_HasSetEndure = false;
  }

  private void SetAnimation(HitDirection direction) {
    if (HitDirection.FRONT == direction) {
      m_AnimationDict.FallDownAnim = m_FrontFallDownAnim;
      m_AnimationDict.HitGroundAnim = m_FrontHitGroundAnim;
      m_AnimationDict.OnLandHoldAnim = m_FrontOnLandHoldAnim;
      m_AnimationDict.StandUpAnim = m_FrontStandUpAnim;
      m_AnimationDict.IdleAnim = m_IdleAnim;
    } else if (HitDirection.BACK == direction) {
      m_AnimationDict.FallDownAnim = m_BackFallDownAnim;
      m_AnimationDict.HitGroundAnim = m_BackHitGroundAnim;
      m_AnimationDict.OnLandHoldAnim = m_BackOnLandHoldAnim;
      m_AnimationDict.StandUpAnim = m_BackStandUpAnim;
      m_AnimationDict.IdleAnim = m_IdleAnim;
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsAcitve) {
      TickEffect(Time.deltaTime);
      TickMovement(Time.deltaTime);
      TickAnimation(Time.deltaTime);
    }
	}

  protected override void TickAnimation(float deltaTime) {
    if (State.FallDown == m_State && Time.time > m_StartTime + m_CurFallDownTime) {
      m_AnimationPlayer.Play(m_AnimationDict.HitGroundAnim.name);
      m_State = State.HitGround;
    }
    if (State.HitGround == m_State && Time.time > m_StartTime + m_CurFallDownTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.HitGroundAnim.name)) {
      m_AnimationPlayer.CrossFade(m_AnimationDict.OnLandHoldAnim.name);
      m_State = State.OnLand;
    }
    if (State.OnLand == m_State && Time.time > m_StartTime + m_CurFallDownTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.HitGroundAnim.name) + m_CurOnLandHoldTime) {
      if (!m_HasSetEndure && m_IsGetUpEndure) {
        SetEndure(gameObject, true);
        m_HasSetEndure = true;
      }
      if (IsLogicDead()) {
        NotifyNpcDead(gameObject);
        StopImpact();
      } else {
        m_AnimationPlayer.Play(m_AnimationDict.StandUpAnim.name);
        m_State = State.StandUp;
      }
    }
    if (State.StandUp == m_State && Time.time > m_StartTime + m_CurFallDownTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.HitGroundAnim.name) + m_CurOnLandHoldTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.StandUpAnim.name)) {
      StopImpact();
    }
  }

  public override void StopImpact() {
    m_State = State.FallDown;
    GeneralStopImpact();
    if (m_HasSetEndure && m_IsGetUpEndure) {
      SetEndure(gameObject, false);
      m_HasSetEndure = false;
    }
    if (!IsLogicDead()) {
      if (null != m_IdleAnim) {
        m_AnimationPlayer.CrossFade(m_IdleAnim);
      }
    } else {
      m_AnimationPlayer.CrossFade(m_AnimationDict.OnLandHoldAnim);
      NotifyNpcDead(gameObject);
    }
  }
}
