using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ImpactHitFly : BaseImpact {

  public AnimationClip m_FrontRisingAnim;
  public AnimationClip m_FrontDropAnim;
  public AnimationClip m_FrontHitGroundAnim;
  public AnimationClip m_FrontOnGroundHoldAnim;
  public AnimationClip m_FrontStandUpAnim;
  public AnimationClip m_BackRisingAnim;
  public AnimationClip m_BackDropAnim;
  public AnimationClip m_BackHitGroundAnim;
  public AnimationClip m_BackStandUpAnim;
  public AnimationClip m_BackOnGroundHoldAnim;
  public AnimationClip m_StandAnim;
  public AnimationClip m_FlyHitAnim;
  public Transform m_TestTransform;
  public float m_Gravity = -10.0f;
  public Vector3 m_Velocity;

  public GameObject m_HitGroundEffect;
  public bool m_IsGetUpEndure = false;
  private bool m_HasSetEndure = false;
  private float m_CurCrossFadeTime;
  private float m_OnLandHoldTime;
  private float m_HitGroundTime;

  class AnimationDict {
    public AnimationClip RisingAnim;
    public AnimationClip DropAnim;
    public AnimationClip HitGroundAnim;
    public AnimationClip StandUpAnim;
    public AnimationClip OnLandHoldAnim;
    public AnimationClip StandAnim;
    public AnimationClip FlyHitAnim;
  }
  AnimationDict m_AnimationDict = new AnimationDict();
  private enum HitFlyState
  {
    Rise,
    Fall,
    OnGround,
    Stand,
  }
  private HitFlyState m_State;
	
	// Update is called once per frame
	void Update () {
    if (IsAcitve)
      TickImpact(Time.deltaTime);
	}

  public override ImpactType GetImpactType()
  {
    if (HitFlyState.Fall == m_State || HitFlyState.Rise == m_State) {
      return ImpactType.HitFly;
    }
    if (HitFlyState.OnGround == m_State) {
      return ImpactType.KnockDown;
    }
    if (HitFlyState.Stand == m_State) {
      return ImpactType.UnKnown;
    }
    return ImpactType.HitFly;
  }

  public void StartImpact(ImpactInfo impactInfo)
  {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactHitFly::StartImpact -- impactInfo is null");
    }
    GeneralStartImpact(impactInfo);
    if (m_ImpactInfo.m_HitFlyConfig.m_UseCustom) {
      m_CurCrossFadeTime = m_ImpactInfo.m_HitFlyConfig.m_CrossFadeTime;
      if (m_CurCrossFadeTime > Mathf.Abs(m_ImpactInfo.m_Velocity.y / m_Gravity)) {
        m_CurCrossFadeTime = Mathf.Abs(m_ImpactInfo.m_Velocity.y / m_Gravity);
      }
      m_MovingTime = m_ImpactInfo.m_HitFlyConfig.m_MovingTime;
      m_OnLandHoldTime = m_ImpactInfo.m_HitFlyConfig.m_OnLandHoldTime;
    } else {
      m_CurCrossFadeTime = Mathf.Abs(m_ImpactInfo.m_Velocity.y / m_Gravity);
      m_MovingTime = -1;
      m_OnLandHoldTime = 0;
    }
    SetAnimation(m_HitDirection);
    m_State = HitFlyState.Rise;
    m_HasSetEndure = false;
    m_Velocity = m_ImpactInfo.m_Velocity;
    //SetFaceDir(new Vector3(-impactInfo.m_Velocity.x, 0, - impactInfo.m_Velocity.z));
    m_AnimationPlayer.CrossFade(m_AnimationDict.RisingAnim.name, m_CurCrossFadeTime);
  }

  private void SetAnimation(HitDirection direction) {
    if (HitDirection.FRONT == direction) {
      m_AnimationDict.DropAnim = m_FrontDropAnim;
      m_AnimationDict.HitGroundAnim = m_FrontHitGroundAnim;
      m_AnimationDict.StandAnim = m_StandAnim;
      m_AnimationDict.RisingAnim = m_FrontRisingAnim;
      m_AnimationDict.StandUpAnim = m_FrontStandUpAnim;
      m_AnimationDict.OnLandHoldAnim = m_FrontOnGroundHoldAnim;
      m_AnimationDict.FlyHitAnim = m_FlyHitAnim;
    } else if (HitDirection.BACK == direction) {
      m_AnimationDict.DropAnim = m_BackDropAnim;
      m_AnimationDict.HitGroundAnim = m_BackHitGroundAnim;
      m_AnimationDict.StandAnim = m_StandAnim;
      m_AnimationDict.RisingAnim = m_BackRisingAnim;
      m_AnimationDict.OnLandHoldAnim = m_BackOnGroundHoldAnim;
      m_AnimationDict.StandUpAnim = m_BackStandUpAnim;
      m_AnimationDict.FlyHitAnim = m_FlyHitAnim;
    }

    m_AnimationPlayer.AddMixingTranform(m_AnimationDict.FlyHitAnim.name, m_TestTransform, true);
  }
  public void TickImpact(float deltaTime) {
    GeneralTickImpact(deltaTime);
  }

  protected override void TickAnimation(float deltaTime) {
    if (HitFlyState.Rise == m_State) {
      if (m_Velocity.y <= 0) {
        m_State = HitFlyState.Fall;
        m_AnimationPlayer.CrossFade(m_AnimationDict.DropAnim.name, Mathf.Sqrt(Mathf.Abs((this.transform.position.y - GetTerrainHeight(this.transform.position)) * 2 / m_Gravity)));
      }
    } else if (HitFlyState.Fall == m_State) {

      if (m_CharacterController.isGrounded) {
        if (null != m_HitGroundEffect) {
          GameObject obj = ResourceSystem.NewObject(m_HitGroundEffect) as GameObject;
          if (null != obj) {
            obj.transform.position = this.transform.position;
            obj.transform.rotation = Quaternion.identity;
            GameObject.Destroy(obj, 2.0f);
          }

        }
        m_State = HitFlyState.OnGround;
        m_AnimationPlayer.Play(m_AnimationDict.HitGroundAnim.name);
        m_HitGroundTime = Time.time;
      }
    } else if (HitFlyState.OnGround == m_State) {
      if (!m_HasSetEndure && m_IsGetUpEndure) {
        SetEndure(gameObject, true);
        m_HasSetEndure = true;
      }
      if (!m_AnimationPlayer.IsPlaying(m_AnimationDict.HitGroundAnim.name) ||
          Time.time > m_HitGroundTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.HitGroundAnim)) {
        if (m_OnLandHoldTime > 0) {
          m_AnimationPlayer.CrossFade(m_AnimationDict.OnLandHoldAnim.name);
          m_OnLandHoldTime -= deltaTime;
        } else {
          m_State = HitFlyState.Stand;
          if (IsLogicDead()) {
            NotifyNpcDead(gameObject);
            StopImpact();
          } else {
            m_AnimationPlayer.Play(m_AnimationDict.StandUpAnim.name);
          }
        }
      }
    } else if (HitFlyState.Stand == m_State) {
      if (!m_AnimationPlayer.IsPlaying(m_AnimationDict.StandUpAnim.name) ||
        Time.time > m_HitGroundTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.HitGroundAnim) + m_OnLandHoldTime + m_AnimationPlayer.AnimationLenth(m_AnimationDict.StandUpAnim)) {
        StopImpact();
      }
    }
  }

  protected override void TickMovement(float deltaTime) {
    // tick vertical
    float verticalSpeed = 0.0f;
    if (m_State == HitFlyState.Rise || m_State == HitFlyState.Fall) {
      verticalSpeed = m_Velocity.y;
      m_Velocity.y += m_Gravity * deltaTime;
    }
    Vector3 motion = new Vector3();
    // tick horizontal
    if (m_MovingTime < 0) {
      if (m_State == HitFlyState.Rise || m_State == HitFlyState.Fall) {
        motion = m_CurVelocity * deltaTime + m_ImpactInfo.m_Acceleration * deltaTime * deltaTime / 2;
        m_CurVelocity = m_CurVelocity + m_ImpactInfo.m_Acceleration * deltaTime;
      }
    } else if(Time.time < m_StartTime + m_MovingTime){
      motion = m_CurVelocity * deltaTime + m_ImpactInfo.m_Acceleration * deltaTime * deltaTime / 2;
      m_CurVelocity = m_CurVelocity + m_ImpactInfo.m_Acceleration * deltaTime;
    }
    motion.y = verticalSpeed * deltaTime;
    m_CharacterController.Move(motion);
  }

  public override void StopImpact() {
    GeneralStopImpact();
    if (m_HasSetEndure && m_IsGetUpEndure) {
      SetEndure(gameObject, false);
      m_HasSetEndure = false;
    }
    if (!IsLogicDead()) {
      if (null != m_StandAnim.name) {
        m_AnimationPlayer.CrossFade(m_StandAnim.name);
      }
    } else {
      NotifyNpcDead(gameObject);
      m_AnimationPlayer.CrossFade(m_AnimationDict.OnLandHoldAnim);
    }
  }

  private void SetFaceDir(Vector3 direction)
  {
    direction.Normalize();
    this.transform.forward = new Vector3(direction.x, 0, direction.z);
  }

  public void OnHitInFly() {
    if (m_IsActive) {
      m_AnimationPlayer.Play(m_AnimationDict.FlyHitAnim.name);
    }
  }

  public void OnKnockDown() {
    if (HitFlyState.Rise == m_State || HitFlyState.Fall == m_State) {
      m_State = HitFlyState.Fall;
      m_Velocity.y -= 5;
    }
  }

  public void OnStiffness(ImpactInfo impactInfo) {
  }
}
