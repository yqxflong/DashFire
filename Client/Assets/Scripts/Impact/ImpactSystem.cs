using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ImpactSystem {

  /*
  public ImpactInfo impactInfo;
  public ImpactInfo impactInfo1;
  public GameObject target;
   */
  #region Singleton
  private static ImpactSystem m_Instance = new ImpactSystem();
  public static ImpactSystem Instance {
    get { return m_Instance; }
  }
  #endregion
  void Start() {
  }

  // Update is called once per frame
  void Update() {
    /*
    if (Input.GetMouseButtonDown(0)) {
      SendImpact(target, impactInfo.Clone() as ImpactInfo);
    }
    if (Input.GetMouseButtonDown(1)) {
      SendImpact(target, impactInfo1.Clone() as ImpactInfo);
    }
     */
  }

  public ImpactType GetImpactState(GameObject obj) {
    BaseImpact[] impacts = obj.GetComponentsInChildren<BaseImpact>();
    if (impacts != null && impacts.Length > 0) {
      foreach (BaseImpact impact in impacts) {
        if (impact.IsAcitve) {
          return impact.GetImpactType();
        }
      }
    }

    return ImpactType.None;
  }

  private bool CanBreakSkill(GameObject obj) {
    BaseSkillManager bsm = obj.GetComponent<BaseSkillManager>();
    if (null != bsm) {
      SkillControllerInterface sc = bsm.GetSkillController();
      if (null != sc) {
        return sc.IsCurSkillCanBreakByImpact();
      }
    }
    return true;
  }

  private void BreakCurSkill(GameObject obj) {
    BaseSkillManager bsm = obj.GetComponent<BaseSkillManager>();
    if (null != bsm) {
      if (!bsm.IsUsingSkill()) {
        return;
      }
      SkillControllerInterface sc = bsm.GetSkillController();
      if (null != sc) {
        sc.ForceStopCurSkill();
      }
    }
  }

  private bool IsBreakEndure(ImpactInfo impactInfo) {
    if (ImpactType.MicroGHitFly == impactInfo.m_Type) {
      return true;
    }
    return false;
  }

  private void SendImpactWithOutDamage(GameObject target, ImpactInfo impactInfo) {
    SharedGameObjectInfo targetInfo = LogicSystem.GetSharedGameObjectInfo(target);
    CharacterCamp info = target.GetComponent<CharacterCamp>();
    if (null != info && info.m_IsEndure && IsBreakEndure(impactInfo)) {
      info.SetEndure(false);
    }
    if (null != info && info.m_IsEndure) {
    } else {
      if (CanBreakSkill(target)) {
        BreakCurSkill(target);
        if (null != targetInfo && targetInfo.Blood > 0) {
          switch (impactInfo.m_Type) {
            case ImpactType.HitFly:
              SendHitFlyImpact(target, impactInfo);
              break;
            case ImpactType.Stiffness:
              SendStiffnessImpact(target, impactInfo);
              break;
            case ImpactType.KnockDown:
              SendKnockDownImpact(target, impactInfo);
              break;
            case ImpactType.Grab:
              SendGrabImpact(target, impactInfo);
              break;
            case ImpactType.MicroGHitFly:
              SendMicroGHitFly(target, impactInfo);
              break;
          }
        }
      }
    }
  }

  public void SendImpact(GameObject sender, GameObject target, ImpactInfo impactInfo, int hitCount = 1) {
    SendImpactWithOutDamage(target, impactInfo);
    SharedGameObjectInfo targetInfo = LogicSystem.GetSharedGameObjectInfo(target);
    LogicSystem.NotifyGfxHitTarget(sender, impactInfo.m_ImpactId, target, hitCount);
  }

  public void SendImpact(GameObject obj, ImpactInfo impactInfo) {
    SendImpactWithOutDamage(obj, impactInfo);
  }

  private void SendHitFlyImpact(GameObject obj, ImpactInfo impactInfo) {
    ImpactMicroGHitFly mircoGHitFly = obj.GetComponent<ImpactMicroGHitFly>();
    ImpactHitFly hitFlyLogic = obj.GetComponent<ImpactHitFly>();
    if (null != mircoGHitFly && mircoGHitFly.IsAcitve) {
      if (ImpactType.HitFly != mircoGHitFly.GetImpactType()) {
        mircoGHitFly.StopImpact();
        if (null != hitFlyLogic) {
          hitFlyLogic.StartImpact(impactInfo);
          return;
        }
      } else {
        mircoGHitFly.OnHitInFly();
      }
      return;
    }
    ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
    if (null != grabLogic && grabLogic.IsAcitve) {
      return;
    }
    ImpactKnockDown knockDownLogic = obj.GetComponent<ImpactKnockDown>();
    if (null != knockDownLogic && knockDownLogic.IsAcitve) {
      knockDownLogic.StopImpact();
    }
    ImpactStiffness stiffnessLogic = obj.GetComponent<ImpactStiffness>();
    if (null != stiffnessLogic && stiffnessLogic.IsAcitve) {
      stiffnessLogic.StopImpact();
    }
    if (null != hitFlyLogic) {
      /*if (hitFlyLogic.IsAcitve) {
        hitFlyLogic.OnHitInFly();
        return;
      }*/
      hitFlyLogic.StopImpact();
      hitFlyLogic.StartImpact(impactInfo);
    }
  }

  private void SendStiffnessImpact(GameObject obj, ImpactInfo impactInfo) {
    ImpactMicroGHitFly mircoGHitFly = obj.GetComponent<ImpactMicroGHitFly>();
    ImpactStiffness StiffnessLogic = obj.GetComponent<ImpactStiffness>();
    if (null != mircoGHitFly && mircoGHitFly.IsAcitve) {
      if (ImpactType.HitFly != mircoGHitFly.GetImpactType()) {
        mircoGHitFly.StopImpact();
        if (null != StiffnessLogic) {
          StiffnessLogic.StartImpact(impactInfo);
        }
        return;
      } else {
        mircoGHitFly.OnStiffness(impactInfo);
      }
      return;
    }
    ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
    if (null != grabLogic && grabLogic.IsAcitve) {
      return;
    }
    ImpactHitFly hitFlyLogic = obj.GetComponent<ImpactHitFly>();
    ImpactKnockDown KnockDownLogic = obj.GetComponent<ImpactKnockDown>();
    // 击飞后端，停止击飞做硬直
    if (null != hitFlyLogic && hitFlyLogic.IsAcitve && ImpactType.UnKnown == hitFlyLogic.GetImpactType()) {
      hitFlyLogic.StopImpact();
      if (null != StiffnessLogic) {
        StiffnessLogic.StartImpact(impactInfo);
      }
      return;
    }
    // 如果被击飞，只做受击
    if (null != hitFlyLogic && hitFlyLogic.IsAcitve && ImpactType.UnKnown != hitFlyLogic.GetImpactType()) {
      if (null != StiffnessLogic) {
        hitFlyLogic.OnHitInFly();
        return;
      }
    }
    // 击倒前段， 不处理
    if (null != KnockDownLogic && KnockDownLogic.IsAcitve && ImpactType.UnKnown != KnockDownLogic.GetImpactType()) {
      return;
    }
    if (null != KnockDownLogic && KnockDownLogic.IsAcitve && ImpactType.UnKnown == KnockDownLogic.GetImpactType()) {
      KnockDownLogic.StopImpact();
      if (null != StiffnessLogic) {
        StiffnessLogic.StartImpact(impactInfo);
        return;
      }
    }

    if (null != StiffnessLogic) {
      StiffnessLogic.StartImpact(impactInfo);
    }
  }

  private void SendKnockDownImpact(GameObject obj, ImpactInfo impactInfo) {
    ImpactMicroGHitFly mircoGHitFly = obj.GetComponent<ImpactMicroGHitFly>();
    ImpactKnockDown knockDownLogic = obj.GetComponent<ImpactKnockDown>();
    if (null != mircoGHitFly && mircoGHitFly.IsAcitve) {
      if (ImpactType.HitFly != mircoGHitFly.GetImpactType()) {
        mircoGHitFly.StopImpact();
        if (null != knockDownLogic) {
          knockDownLogic.StartImpact(impactInfo);
          return;
        }
      }
      return;
    }
    ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
    if (null != grabLogic && grabLogic.IsAcitve) {
      return;
    }
    ImpactHitFly hitFlyLogic = obj.GetComponent<ImpactHitFly>();
    if (null != hitFlyLogic && hitFlyLogic.IsAcitve && ImpactType.UnKnown == hitFlyLogic.GetImpactType()) {
      hitFlyLogic.StopImpact();
      if (null != knockDownLogic) {
        knockDownLogic.StartImpact(impactInfo);
        return;
      }
    }
    if (null != hitFlyLogic && hitFlyLogic.IsAcitve && ImpactType.UnKnown != hitFlyLogic.GetImpactType()) {
      hitFlyLogic.OnKnockDown();
      return;
    }
    if (null != knockDownLogic && !knockDownLogic.IsAcitve) {
      knockDownLogic.StartImpact(impactInfo);
    }
  }

  private void SendThunderStrokeImpact(GameObject obj, ImpactInfo impactInfo) {
    ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
    if (null != grabLogic && grabLogic.IsAcitve) {
      return;
    }
    ImpactThunderStroke thunderStrokeLogic = obj.GetComponent<ImpactThunderStroke>();
    if (null != thunderStrokeLogic) {
      thunderStrokeLogic.StartImpact(impactInfo);
    }
  }
  private void SendGrabImpact(GameObject obj, ImpactInfo impactInfo) {
    ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
    if (null != grabLogic) {
      if (!grabLogic.IsAcitve) {
        StopImpactExcept(obj, ImpactType.Grab);
        grabLogic.StartImpact(impactInfo);
      }
    }
  }

  private void SendMicroGHitFly(GameObject obj, ImpactInfo impactInfo) {
    ImpactMicroGHitFly microGHitFlyLogic = obj.GetComponent<ImpactMicroGHitFly>();
    if (null != microGHitFlyLogic) {
      if (!microGHitFlyLogic.IsAcitve) {
        StopImpactExcept(obj, ImpactType.MicroGHitFly);
        microGHitFlyLogic.StartImpact(impactInfo);
      } else {
        if (ImpactType.HitFly != microGHitFlyLogic.GetImpactType()) {
          StopImpactExcept(obj, ImpactType.MicroGHitFly);
          microGHitFlyLogic.StopImpact();
          microGHitFlyLogic.StartImpact(impactInfo);
        }
      }
    }
  }
  public void StopImpact(GameObject obj, ImpactType impactType) {
    switch (impactType) {
      case ImpactType.HitFly:
        ImpactHitFly hitFlyLogic = obj.GetComponent<ImpactHitFly>();
        if (null != hitFlyLogic && hitFlyLogic.IsAcitve) {
          hitFlyLogic.StopImpact();
        }
        break;
      case ImpactType.Stiffness:
        ImpactStiffness stiffnessLogic = obj.GetComponent<ImpactStiffness>();
        if (null != stiffnessLogic && stiffnessLogic.IsAcitve) {
          stiffnessLogic.StopImpact();
        }
        break;
      case ImpactType.KnockDown:
        ImpactKnockDown knockDownLogic = obj.GetComponent<ImpactKnockDown>();
        if (null != knockDownLogic && knockDownLogic.IsAcitve) {
          knockDownLogic.StopImpact();
        }
        break;
      case ImpactType.ThunderStroke:
        break;
      case ImpactType.Grab:
        ImpactGrab grabLogic = obj.GetComponent<ImpactGrab>();
        if (null != grabLogic && grabLogic.IsAcitve) {
          grabLogic.StopImpact();
        }
        break;
    }
  }

  public void StopImpactExcept(GameObject obj, ImpactType impactType) {
    if (ImpactType.Grab != impactType) {
      StopImpact(obj, ImpactType.Grab);
    }
    if (ImpactType.HitFly != impactType) {
      StopImpact(obj, ImpactType.HitFly);
    }
    if (ImpactType.KnockDown != impactType) {
      StopImpact(obj, ImpactType.KnockDown);
    }
    if (ImpactType.Stiffness != impactType) {
      StopImpact(obj, ImpactType.Stiffness);
    }
  }
  public void StopAllImpacts(GameObject obj) {
    BaseImpact[] bis = obj.GetComponents<BaseImpact>();
    foreach (BaseImpact bi in bis) {
      if (bi.IsAcitve) {
        bi.StopImpact();
      }
    }
  }
}



[System.Serializable]
public class ImpactInfo {
  public ImpactType m_Type;
  public int m_ImpactId = 1;
  public GameObject m_Attacker;
  public float m_Offset = 0;
  public ImpactDirectionType m_DirectionType = ImpactDirectionType.VelocityDir;
  public Vector3 m_DirectionPoint;
  public Vector3 m_ForcePoint;
  public Vector3 m_VelocityFactorA;
  public Vector3 m_VelocityFactorB;
  public Vector3 m_AccelFactorA;
  public Vector3 m_AccelFactorB;
  public float m_HorizontalSpeed;
  public float m_VerticalSpeed;
  public Vector3 m_Velocity;
  public Vector3 m_Acceleration;
  public bool m_IsHitHighLight = false;
  public float m_HitHighLightTime = 0.1f;
  public List<AudioClip> m_AudioClips = new List<AudioClip>();
  public List<EffectData> m_EffectDatas = new List<EffectData>();
  public ImpactStiffnessConfig m_StiffnessConfig = new ImpactStiffnessConfig();
  public ImpactKnockDownConfig m_KnockDownConfig = new ImpactKnockDownConfig();
  public ImpactHitFlyConfig m_HitFlyConfig = new ImpactHitFlyConfig();
  public ImpactThunderStrokeConfig m_ThunderStrokeConfig = new ImpactThunderStrokeConfig();
  public ImpactGrabConfig m_GrabConfig = new ImpactGrabConfig();
  public ImpactMicroGHitFlyConfig m_MicroHitFlyConfig = new ImpactMicroGHitFlyConfig();

  public GameObject Attacker {
    get { return m_Attacker; }
    set { m_Attacker = value; }
  }

  public float HorizontalSpeed {
    get { return m_HorizontalSpeed; }
    set { m_HorizontalSpeed = value; }
  }

  public float VerticalSpeed {
    get { return m_VerticalSpeed; }
    set { m_VerticalSpeed = value; }
  }

  public ImpactDirectionType DirectionType {
    get { return m_DirectionType; }
    set { m_DirectionType = value; }
  }
  public void SetMovementInfo(ImpactDirectionType dirType) {
    m_DirectionType = dirType;
  }

  public object Clone() {
    ImpactInfo ii = new ImpactInfo();
    ii.m_Type = m_Type;
    ii.m_Attacker = m_Attacker;
    ii.m_Velocity = m_Velocity;
    ii.m_ImpactId = m_ImpactId;
    ii.m_Acceleration = m_Acceleration;
    ii.m_IsHitHighLight = m_IsHitHighLight;
    ii.m_HitHighLightTime = m_HitHighLightTime;
    foreach (AudioClip ac in m_AudioClips) {
      if (!ii.m_AudioClips.Contains(ac)) {
        ii.m_AudioClips.Add(ac);
      }
    }
    foreach (EffectData effect in m_EffectDatas) {
      if (!ii.m_EffectDatas.Contains(effect)) {
        ii.m_EffectDatas.Add(new EffectData(effect));
      }
    }
    ii.m_Offset = m_Offset;
    ii.m_DirectionType = m_DirectionType;
    ii.m_DirectionPoint = m_DirectionPoint;
    ii.m_ForcePoint = m_ForcePoint;
    ii.m_VelocityFactorA = m_VelocityFactorA;
    ii.m_VelocityFactorB = m_VelocityFactorB;
    ii.m_AccelFactorA = m_AccelFactorA;
    ii.m_AccelFactorB = m_AccelFactorB;
    ii.HorizontalSpeed = m_HorizontalSpeed;
    ii.VerticalSpeed = m_VerticalSpeed;
    ii.m_StiffnessConfig = new ImpactStiffnessConfig(m_StiffnessConfig);
    ii.m_KnockDownConfig = new ImpactKnockDownConfig(m_KnockDownConfig);
    ii.m_HitFlyConfig = new ImpactHitFlyConfig(m_HitFlyConfig);
    ii.m_ThunderStrokeConfig = new ImpactThunderStrokeConfig(m_ThunderStrokeConfig);
    ii.m_GrabConfig = new ImpactGrabConfig(m_GrabConfig);
    ii.m_MicroHitFlyConfig = new ImpactMicroGHitFlyConfig(m_MicroHitFlyConfig);
    return ii;
  }

  public void ApplyOffset(float offset) {
    m_Velocity = m_Velocity * Random.Range(1 - offset, 1 + offset);
    m_StiffnessConfig.ApplyOffset(offset);
    m_KnockDownConfig.ApplyOffset(offset);
    m_HitFlyConfig.ApplyOffset(offset);
    m_ThunderStrokeConfig.ApplyOffset(offset);
    m_GrabConfig.ApplyOffset(offset);
    m_MicroHitFlyConfig.ApplyOffset(offset);
  }
}

[System.Serializable]
public class ImpactStiffnessConfig {
  public bool m_UseCustom = true;
  public float m_StiffTime = -1;
  public float m_StiffHoldTime = -1;
  public float m_MovingTime = -1;

  public ImpactStiffnessConfig() {
  }
  public ImpactStiffnessConfig(ImpactStiffnessConfig config) {
    m_UseCustom = config.m_UseCustom;
    m_StiffTime = config.m_StiffTime;
    m_StiffHoldTime = config.m_StiffHoldTime;
    m_MovingTime = config.m_MovingTime;
  }

  public void ApplyOffset(float offset) {
    m_StiffTime = m_StiffTime * Random.Range(1 - offset, 1 + offset);
    m_StiffHoldTime = m_StiffHoldTime * Random.Range(1 - offset, 1 + offset);
    m_MovingTime = m_MovingTime * Random.Range(1 - offset, 1 + offset);
  }
}
[System.Serializable]
public class ImpactKnockDownConfig {
  public bool m_UseCustom = true;
  public float m_MovingTime;
  public float m_FallDownTime;
  public float m_OnLandHoldTime;

  public ImpactKnockDownConfig() {
  }
  public ImpactKnockDownConfig(ImpactKnockDownConfig config) {
    m_UseCustom = config.m_UseCustom;
    m_MovingTime = config.m_MovingTime;
    m_FallDownTime = config.m_FallDownTime;
    m_OnLandHoldTime = config.m_OnLandHoldTime;
  }

  public void ApplyOffset(float offset) {
    m_MovingTime = m_MovingTime * Random.Range(1 - offset, 1 + offset);
    m_FallDownTime = m_FallDownTime * Random.Range(1 - offset, 1 + offset);
    m_OnLandHoldTime = m_OnLandHoldTime * Random.Range(1 - offset, 1 + offset);
  }
}

[System.Serializable]
public class ImpactHitFlyConfig {
  public bool m_UseCustom = true;
  public float m_MovingTime = -1;
  public float m_CrossFadeTime = -1;
  public float m_OnLandHoldTime = -1;

  public ImpactHitFlyConfig() {
  }
  public ImpactHitFlyConfig(ImpactHitFlyConfig config) {
    m_UseCustom = config.m_UseCustom;
    m_MovingTime = config.m_MovingTime;
    m_CrossFadeTime = config.m_CrossFadeTime;
    m_OnLandHoldTime = config.m_OnLandHoldTime;
  }
  public void ApplyOffset(float offset) {
    m_MovingTime = m_MovingTime * Random.Range(1 - offset, 1 + offset);
    m_CrossFadeTime = m_CrossFadeTime * Random.Range(1 - offset, 1 + offset);
    m_OnLandHoldTime = m_OnLandHoldTime * Random.Range(1 - offset, 1 + offset);
  }

}
[System.Serializable]
public class ImpactThunderStrokeConfig {
  public bool m_UseCustom = true;
  public float m_StrokeInterval = 0.5f;
  public int m_StrokeCount = 1;

  public ImpactThunderStrokeConfig() {
  }
  public ImpactThunderStrokeConfig(ImpactThunderStrokeConfig config) {
    m_UseCustom = config.m_UseCustom;
    m_StrokeInterval = config.m_StrokeInterval;
    m_StrokeCount = config.m_StrokeCount;
  }

  public void ApplyOffset(float offset) {
  }
}
[System.Serializable]
public class ImpactGrabConfig{
  public bool m_UseCustom = true;
  public float m_GrabTime = 0.0f;
  public AnimationClip m_GrabEndAnim;

  public ImpactGrabConfig(){
  }

  public ImpactGrabConfig(ImpactGrabConfig config){
    m_UseCustom = config.m_UseCustom;
    m_GrabTime = config.m_GrabTime;
    m_GrabEndAnim = config.m_GrabEndAnim;
  }
  public void ApplyOffset(float offset) {
  }
}

[System.Serializable]
public class ImpactMicroGHitFlyConfig {
  public bool m_UseCustom = true;
  public float m_Scaler = 0.1f;
  public float m_ScaleDelay = 0.3f;
  public float m_EndSpeedY = 100.0f;

  public ImpactMicroGHitFlyConfig() {
  }

  public ImpactMicroGHitFlyConfig(ImpactMicroGHitFlyConfig config) {
    m_UseCustom = config.m_UseCustom;
    m_Scaler = config.m_Scaler;
    m_ScaleDelay = config.m_ScaleDelay;
    m_EndSpeedY = config.m_EndSpeedY;
  }
  public void ApplyOffset(float offset) {
    m_EndSpeedY = m_EndSpeedY * Random.Range(1 - offset, 1 + offset);
  }
}
[System.Serializable]
public enum ImpactType {
  Stiffness,
  HitFly,
  KnockDown,
  ThunderStroke,
  UnKnown,
  None,
  Grab,
  MicroGHitFly,
}

public enum ImpactDirectionType {
  SenderDir,
  SenderToTargetDir,
  VelocityDir,
  Converge,
  Diverge,
}

[System.Serializable]
public enum EffectPositionType {
  Position,
  Bone,
  BoneWithPosition,
  PositionWithDirection,
}
[System.Serializable]
public class EffectData {
  public EffectData(EffectData effectData) {
    m_EffectPrefab = effectData.m_EffectPrefab;
    m_Position = effectData.m_Position;
    m_Bone = effectData.m_Bone;
    m_Position = new Vector3(effectData.m_Position.x, effectData.m_Position.y, effectData.m_Position.z);
    m_LifeTime = effectData.m_LifeTime;
    m_DelayTime = effectData.m_DelayTime;
    m_PositionType = effectData.m_PositionType;
    m_Rotation = effectData.m_Rotation;
  }
  public GameObject m_EffectPrefab;
  public EffectPositionType m_PositionType;
  public string m_Bone;
  public Vector3 m_Position;
  public Vector3 m_Rotation;
  public float m_LifeTime;
  public float m_DelayTime;
}