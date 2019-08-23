using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire;
using UnityEngine;

public class ShooterSkillTrigger : MonoBehaviour {
  private SkillSceneObjManager m_SceneObjMgr = null;
  internal void Start() {
    m_SceneObjMgr = gameObject.GetComponent<SkillSceneObjManager>();
    if (m_SceneObjMgr == null) {
      LogicSystem.GfxLog("ShooterSkillTrigger miss!");
      return;
    }

    TriggerImpl.OnGetSkillSoundInfo = m_SceneObjMgr.TryGetSkillSoundInfo;
  }
  /// <summary>
  /// Triggers
  /// </summary>
  /// param:Id[对应到SkillSceneObjManager中的SkillEffectId]
  /// example:2
  public void Trigger_PlayEffectAtPosByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_PlayEffectByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    if (m_SceneObjMgr != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        TriggerImpl.PlayEffect(effectInfo);
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SkillEffectId]
  /// example:2
  public void Trigger_PlayEffectAtRoleByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_PlayEffectAtRoleByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    if (m_SceneObjMgr != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        effectInfo = effectInfo.Clone();
        Vector3 pos = this.gameObject.transform.position;
        Vector3 rot = this.gameObject.transform.rotation.eulerAngles;
        if (!string.IsNullOrEmpty(effectInfo.EffectParentBone)) {
          Transform t = LogicSystem.FindChildRecursive(this.gameObject.transform, effectInfo.EffectParentBone);
          if (null != t) {
            pos = t.position;
          }
        }
        effectInfo.EffectPos += pos;
        effectInfo.EffectRot += rot;
        TriggerImpl.PlayEffect(effectInfo);
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SkillEffectId]
  /// example:2
  public void Trigger_PlayEffectAtWeaponByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_PlayEffectAtWeaponByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    MasterWeaponType masterType = (MasterWeaponType)(sobjInfo.ExtractNumeric<int>(0, 0));
    GameObject curMainWeapon = TriggerImpl.GetCurMainWeapon(this.gameObject, masterType);
    if (m_SceneObjMgr != null && curMainWeapon != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        effectInfo = effectInfo.Clone();
        Vector3 pos = curMainWeapon.transform.position;
        Vector3 rot = curMainWeapon.transform.rotation.eulerAngles;
        if (!string.IsNullOrEmpty(effectInfo.EffectParentBone)) {
          Transform t = LogicSystem.FindChildRecursive(curMainWeapon.transform, effectInfo.EffectParentBone);
          if (null != t) {
            pos = t.position;
          }
        }
        effectInfo.EffectPos += pos;
        effectInfo.EffectRot += rot;
        TriggerImpl.PlayEffect(effectInfo);
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SkillEffectId]
  /// example:2
  public void Trigger_AttachEffectAtWeaponByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_AttachEffectAtWeaponByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    MasterWeaponType masterType = (MasterWeaponType)(sobjInfo.ExtractNumeric<int>(0, 0));
    GameObject curMainWeapon = TriggerImpl.GetCurMainWeapon(this.gameObject, masterType);
    if (m_SceneObjMgr != null && curMainWeapon != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        effectInfo = effectInfo.Clone();
        effectInfo.EffectParent = curMainWeapon.transform;
        TriggerImpl.AttachEffect(effectInfo);
      }
    } else {
      Debug.Log("Trigger_AttachEffectAtWeaponByEffectId null");
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SkillEffect Id]
  /// example:2
  public void Trigger_AttachEffectAtRoleByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_AttachEffectAtRoleByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    if (m_SceneObjMgr != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        effectInfo = effectInfo.Clone();
        effectInfo.EffectParent = this.gameObject.transform;
        TriggerImpl.AttachEffect(effectInfo);
      }
    } else {
      Debug.Log("Trigger_AttachEffectAtRoleByEffectId null");
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SkillEffect Id]
  /// example:2
  public void Trigger_AttachEffectDirectAtRoleByEffectId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_AttachEffectDirectAtRoleByEffectId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    if (m_SceneObjMgr != null) {
      SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(sobjInfo.Id);
      if (effectInfo != null) {
        effectInfo = effectInfo.Clone();
        effectInfo.EffectParent = this.gameObject.transform;
        TriggerImpl.AttachEffectDirect(effectInfo);
      }
    } else {
      Debug.Log("Trigger_AttachEffectDirectAtRoleByEffectId null");
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SoundEffect Id]
  /// example:2
  public void Trigger_PlaySoundAtRoleBySoundId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_PlaySoundAtRoleBySoundId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    if (m_SceneObjMgr != null) {
      SkillSoundInfo soundInfo = m_SceneObjMgr.TryGetSkillSoundInfo(sobjInfo.Id);
      if (soundInfo != null) {
        TriggerImpl.PlaySoundAtTarget(this.gameObject, soundInfo);
      }
    } else {
      Debug.Log("Trigger_PlaySoundAtRoleBySoundId null");
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的SoundEffect Id], pos[场景绝对位置]
  /// example:2,0 2 3
  public void Trigger_PlaySoundAtPosBySoundId(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_PlaySoundAtPosBySoundId param:" + param);
#endif
    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    Vector3 targetPos = Vector3.zero;
    try {
      if (sobjInfo.paramOther.Count > 0) {
        targetPos = Script_Util.ToVector3(sobjInfo.paramOther[0]);
      } else {
        Debug.Log("Trigger_PlaySoundAtPosBySoundId miss pos");
        return;
      }
    } catch (System.Exception ex) {
      Debug.Log("Trigger_PlaySoundAtPosBySoundId pos convert error");
      return;
    }

    if (m_SceneObjMgr != null) {
      SkillSoundInfo soundInfo = m_SceneObjMgr.TryGetSkillSoundInfo(sobjInfo.Id);
      if (soundInfo != null) {
        TriggerImpl.PlaySoundAtPos(soundInfo, targetPos);
      }
    } else {
      Debug.Log("Trigger_PlaySoundAtPosBySoundId null");
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的LinearBullet Id]
  /// example:1
  public void Trigger_Shoot(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_Shoot param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SceneObject_LinearBulletInfo bulletInfo = m_SceneObjMgr.TryGetLinearBulletInfo(sobjInfo.Id);
      MasterWeaponType masterType = (MasterWeaponType)(sobjInfo.ExtractNumeric<int>(0, 0));
      GameObject curMainWeapon = TriggerImpl.GetCurMainWeapon(this.gameObject, masterType);
      if (bulletInfo != null && bulletInfo.SceneObjInfo != null && curMainWeapon != null) {
        Transform efSpark = LogicSystem.FindChildRecursive(curMainWeapon.transform,
          Script_Util.ForceNotifyEffectBone(bulletInfo.SceneObjInfo.EffectParentBone));
        if (null == efSpark) {
          Debug.Log("Trigger_Shoot bone miss! id:" + bulletInfo.Id);
          return;
        }
        bulletInfo = bulletInfo.Clone();
        bulletInfo.Attacker = this.gameObject;
        bulletInfo.SceneObjInfo.EffectPos += efSpark.transform.position;
        bulletInfo.MoveStartPos += efSpark.transform.position;
        bulletInfo.MoveDir = Quaternion.Euler(efSpark.transform.rotation.eulerAngles + bulletInfo.MoveDir) * Vector3.forward;
        bulletInfo.ImpactSrcPos += new Vector3(gameObject.transform.position.x,
          efSpark.transform.position.y, gameObject.transform.position.z);
        GameObject tBulletObj = TriggerImpl.PlayEffect(bulletInfo.SceneObjInfo);
        tBulletObj.SendMessage("Active", bulletInfo);
      } else {
        Debug.Log("Trigger_Shoot null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的TerminalBullet Id]
  /// example:1
  public void Trigger_TerminalShoot(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_TerminalShoot param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SceneObject_TerminalBulletInfo bulletInfo = m_SceneObjMgr.TryGetTerminalBulletInfo(sobjInfo.Id);
      MasterWeaponType masterType = (MasterWeaponType)(sobjInfo.ExtractNumeric<int>(0, 0));
      GameObject curMainWeapon = TriggerImpl.GetCurMainWeapon(this.gameObject, masterType);
      if (bulletInfo != null && bulletInfo.SceneObjInfo != null && curMainWeapon != null) {
        Transform efSpark = LogicSystem.FindChildRecursive(curMainWeapon.transform,
          Script_Util.ForceNotifyEffectBone(bulletInfo.SceneObjInfo.EffectParentBone));
        if (null == efSpark) {
          Debug.Log("Trigger_Shoot bone miss!");
          return;
        }
        bulletInfo = bulletInfo.Clone();
        bulletInfo.Attacker = this.gameObject;
        bulletInfo.SceneObjInfo.EffectPos += efSpark.transform.position;
        bulletInfo.MoveStartPos += efSpark.transform.position;
        bulletInfo.MoveDir = Quaternion.Euler(efSpark.transform.rotation.eulerAngles + bulletInfo.MoveDir) * Vector3.forward;
        bulletInfo.ImpactSrcPos += new Vector3(gameObject.transform.position.x,
          efSpark.transform.position.y, gameObject.transform.position.z);
        GameObject tBulletObj = TriggerImpl.PlayEffect(bulletInfo.SceneObjInfo);
        tBulletObj.SendMessage("Active", bulletInfo);
      } else {
        Debug.Log("Trigger_TerminalShoot null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的AttractBullet Id]
  /// example:1
  public void Trigger_AttractShoot(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_AttractShoot param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SceneObject_AttractBulletInfo bulletInfo = m_SceneObjMgr.TryGetAttractBulletInfo(sobjInfo.Id);
      MasterWeaponType masterType = (MasterWeaponType)(sobjInfo.ExtractNumeric<int>(0, 0));
      GameObject curMainWeapon = TriggerImpl.GetCurMainWeapon(this.gameObject, masterType);
      if (bulletInfo != null && bulletInfo.SceneObjInfo != null && curMainWeapon != null) {
        Transform efSpark = LogicSystem.FindChildRecursive(curMainWeapon.transform,
          Script_Util.ForceNotifyEffectBone(bulletInfo.SceneObjInfo.EffectParentBone));
        if (null == efSpark) {
          Debug.Log("Trigger_AttractShoot bone miss!");
          return;
        }
        bulletInfo = bulletInfo.Clone();
        bulletInfo.Attacker = this.gameObject;
        bulletInfo.SceneObjInfo.EffectPos += efSpark.transform.position;
        bulletInfo.MoveStartPos += efSpark.transform.position;
        bulletInfo.MoveDir = Quaternion.Euler(efSpark.transform.rotation.eulerAngles + bulletInfo.MoveDir) * Vector3.forward;
        bulletInfo.SceneObjInfo.EffectLiftTime = float.MaxValue;
        bulletInfo.ImpactSrcPos += new Vector3(gameObject.transform.position.x,
          efSpark.transform.position.y, gameObject.transform.position.z);
        GameObject tBulletObj = TriggerImpl.PlayEffect(bulletInfo.SceneObjInfo);
        tBulletObj.SendMessage("Active", bulletInfo);
      } else {
        Debug.Log("Trigger_AttractShoot null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的Missle Id]
  /// example:1
  public void Trigger_SummonMissle(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_SummonMissle param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SceneObject_MissleInfo bulletInfo = m_SceneObjMgr.TryGetMissleInfo(sobjInfo.Id);
      IShooterSkill curSkill = TriggerImpl.GetCurSkill(this.gameObject);
      if (bulletInfo != null && bulletInfo.SceneObjInfo != null && curSkill != null) {
        bulletInfo = bulletInfo.Clone();
        bulletInfo.Attacker = this.gameObject;
        bulletInfo.SceneObjInfo.EffectPos += this.gameObject.transform.position;
        bulletInfo.MoveStartPos += this.gameObject.transform.position;
        bulletInfo.MoveTargetPos += curSkill.GetTargetPos();
        bulletInfo.MoveVelocity = this.gameObject.transform.rotation * bulletInfo.MoveVelocity;

        GameObject tBulletObj = TriggerImpl.PlayEffect(bulletInfo.SceneObjInfo);
        tBulletObj.SendMessage("Active", bulletInfo);
      } else {
        Debug.Log("Trigger_SummonMissle null!");
      }
    }
  }
  /// param:
  /// example:
  public void Trigger_SwitchWeapon() {
#if SHOOTER_LOG
    Debug.Log("Trigger_SwitchWeapon param:");
#endif
    TriggerImpl.SwitchNextWeapon(this.gameObject);
  }
  /// param:
  /// example:
  public void Trigger_CanBreak() {
#if SHOOTER_LOG
    Debug.Log("Trigger_CanBreak");
#endif
    IShooterSkill skillLogic = TriggerImpl.GetCurSkill(this.gameObject);
    if (null != skillLogic) {
      skillLogic.SetCanBreak(true);
    }
  }
  /// param:
  /// example:
  public void Trigger_CanBreakBySkill() {
#if SHOOTER_LOG
    Debug.Log("Trigger_CanBreakBySkill");
#endif
    IShooterSkill skillLogic = TriggerImpl.GetCurSkill(this.gameObject);
    if (null != skillLogic) {
      skillLogic.SetCanBreakBySkill(true);
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的Impact Id]
  /// example:1
  public void Trigger_SendImpact(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_SendImpact param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SkillImpactInfos bulletInfo = m_SceneObjMgr.TryGetSkillImpactInfo(sobjInfo.Id);
      IShooterSkill curSkill = TriggerImpl.GetCurSkill(this.gameObject);
      if (bulletInfo != null) {
        bulletInfo = bulletInfo.Clone();
        TriggerImpl.ProcessImpact(this.gameObject, this.gameObject, bulletInfo);
      } else {
        Debug.Log("Trigger_SendImpact null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的Impact Id],verticalHeight[垂直高度],verticalHeightStart[底面起始高度偏移],sectorDegree[角度范围]
  /// example:1,10,-1,60
  public void Trigger_SendVerticalImpact(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_SendVerticalImpact param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      float verticalHeight = sobjInfo.ExtractNumeric<float>(0, 10.0f);
      float verticalHeightStart = sobjInfo.ExtractNumeric<float>(1, 0.0f);
      float sectorDegree = sobjInfo.ExtractNumeric<float>(2, 360.0f);
      SkillImpactInfos bulletInfo = m_SceneObjMgr.TryGetSkillImpactInfo(sobjInfo.Id);
      IShooterSkill curSkill = TriggerImpl.GetCurSkill(this.gameObject);
      if (bulletInfo != null) {
        bulletInfo = bulletInfo.Clone();
        TriggerImpl.ProcessVerticalImpact(this.gameObject, this.gameObject, bulletInfo,
          verticalHeight, verticalHeightStart, sectorDegree);
      } else {
        Debug.Log("Trigger_SendVerticalImpact null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的Impact Id],center[box中心],size[box大小]
  /// example:602,0 1 1.5,1 3 3
  public void Trigger_SendImpactByBoxCollider(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_SendImpactByBoxCollider param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      Vector3 center = sobjInfo.ExtractVector3(0);
      Vector3 size = sobjInfo.ExtractVector3(1);
      SkillImpactInfos bulletInfo = m_SceneObjMgr.TryGetSkillImpactInfo(sobjInfo.Id);
      IShooterSkill curSkill = TriggerImpl.GetCurSkill(this.gameObject);
      if (bulletInfo != null) {
        bulletInfo = bulletInfo.Clone();
        TriggerImpl.ProcessImpactByBoxCollider(this.gameObject, this.gameObject, bulletInfo,
          center, size);
      } else {
        Debug.Log("Trigger_SendImpact null!");
      }
    }
  }
  //功能：开始移动
  //参数：{speed} 移动速度
  public void Trigger_StartMove(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_StartMove param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SkillMovementInfo moveInfo = m_SceneObjMgr.TryGetSkillMovementInfo(sobjInfo.Id);
      if (moveInfo != null) {
        TriggerImpl.StartMove(this.gameObject, moveInfo);
      } else {
        Debug.Log("Trigger_StartMove null!");
      }
    }
  }
  //功能：停止StartMove开始的移动，无法停止CurveMove
  public void Trigger_StopMove() {
#if SHOOTER_LOG
    Debug.Log("Trigger_StopMove");
#endif
    TriggerImpl.StopMove(this.gameObject);
  }
  /// param:Id[对应到SkillSceneObjManager中的WeaponInfo Id]
  /// example:1
  public void Trigger_ChangeWeapon(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_ChangeWeapon param:" + param);
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SkillWeaponInfo moveInfo = m_SceneObjMgr.TryGetSkillWeaponInfo(sobjInfo.Id);
      if (moveInfo != null) {
        TriggerImpl.ChangeWeapon(this.gameObject, moveInfo);
      } else {
        Debug.Log("Trigger_ChangeWeapon null!");
      }
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的WeaponInfo Id]
  /// example:1
  public void Trigger_ChangeMainWeapon() {
#if SHOOTER_LOG
    Debug.Log("Trigger_ChangeWeapon param:" + param);
#endif
    SkillWeaponInfo masterWeaponInfo = TriggerImpl.GetCurWeaponInfo(this.gameObject, MasterWeaponType.Master);
    SkillWeaponInfo subMasterWeaponInfo = TriggerImpl.GetCurWeaponInfo(this.gameObject, MasterWeaponType.SubMaster);
    if (masterWeaponInfo != null) {
      TriggerImpl.ChangeWeapon(this.gameObject, masterWeaponInfo);
    }
    if (subMasterWeaponInfo != null) {
      TriggerImpl.ChangeWeapon(this.gameObject, subMasterWeaponInfo);
    }
  }
  /// param:Id[对应到SkillSceneObjManager中的WeaponInfo Id]
  /// example:1
  public void Trigger_NotifyIngoreGravity(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_NotifyIngoreGravity param:" + param);
#endif
    bool ingore = false;
    try {
      ingore = Convert.ToBoolean(param);
    } catch (System.Exception ex) {
      Debug.Log("Trigger_NotifyIngoreGravity err");
      return;
    }
    TriggerImpl.NotifyIngoreGravity(this.gameObject, ingore);
  }
  /// param:Id[对应到SkillSceneObjManager中的WeaponInfo Id]
  /// example:1
  public void Trigger_ShowRole() {
#if SHOOTER_LOG
    Debug.Log("Trigger_ShowRole param:");
#endif
    TriggerImpl.ShowGameObject(this.gameObject, true);
  }
  /// param:Id[对应到SkillSceneObjManager中的WeaponInfo Id]
  /// example:1
  public void Trigger_HideRole() {
#if SHOOTER_LOG
    Debug.Log("Trigger_HideRole param:");
#endif
    TriggerImpl.ShowGameObject(this.gameObject, false);
  }
  /// param:Id[忽略],duration[震屏持续时间],frequency[震屏频率],randomPercent[震屏位移产生概率],amplitude[震屏位移幅度]
  /// example:0,1,0.02,80,0.03
  /// example:0
  public void Trigger_StartShakeCamera(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_StartShakeCamera param:");
#endif
    ShakeCamera tShakeCameraObj = this.gameObject.GetComponent<ShakeCamera>();
    if (tShakeCameraObj == null) {
      LogicSystem.GfxLog("Trigger_StartShakeCamera miss!");
      return;
    }

    SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
    float duration = sobjInfo.ExtractNumeric<float>(0, 1.0f);
    float frequency = sobjInfo.ExtractNumeric<float>(1, 0.02f);
    float randomPercent = sobjInfo.ExtractNumeric<float>(2, 80.0f);
    float amplitude = sobjInfo.ExtractNumeric<float>(3, 0.03f);
    string formatParam = string.Format("{0} {1} {2} {3}", duration, frequency, randomPercent, amplitude);
    tShakeCameraObj.StartShakeCamera(formatParam);
  }
  /// param:Duration[持续时间],BoneName[骨骼名字],AnimName[动画名字]
  /// example:0.5,ef_righthand,QiangShou_Test09
  public void Trigger_StartAnimProcedure(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_StartAnimProcedure param:");
#endif
    SkillAnimProcedure tAniProce = this.gameObject.GetComponent<SkillAnimProcedure>();
    if (tAniProce == null) {
      LogicSystem.GfxLog("Trigger_StartAnimProcedure SkillAnimProcedure miss!");
      return;
    }
    IShooterSkill curSkill = TriggerImpl.GetCurSkill(this.gameObject);
    if (curSkill == null) {
      Debug.Log("Trigger_StartAnimProcedure curSkill null!");
      return;
    }
    SkillAnimProcedureInfo info = new SkillAnimProcedureInfo(param);
    info.TargetPos = curSkill.GetTargetPos();
    tAniProce.StartAnimProcedure(info);
  }
  /// param:ID[对应到SkillSceneObjManager中的LockFrameInfo Id]
  /// example:101
  public void Trigger_LockFrame(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_LockFrame param:");
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SkillLockFrameInfo lockInfo = m_SceneObjMgr.TryGetSkillLockFrameInfo(sobjInfo.Id);
      if (lockInfo != null) {
        lockInfo = lockInfo.Clone();
        TriggerImpl.StartLockFrame(this.gameObject, lockInfo);
      } else {
        Debug.Log("Trigger_LockFrame null!");
      }
    }
  }
  /// param:ID[对应到SkillSceneObjManager中的LockFrameInfo Id]
  /// example:101
  public void Trigger_LockMoveScale(string param) {
#if SHOOTER_LOG
    Debug.Log("Trigger_LockMoveScale param:");
#endif
    if (m_SceneObjMgr != null) {
      SkillSceneObjInfo sobjInfo = new SkillSceneObjInfo(param);
      SkillMoveScaleInfo lockInfo = m_SceneObjMgr.TryGetSkillMoveScaleInfo(sobjInfo.Id);
      if (lockInfo != null) {
        lockInfo = lockInfo.Clone();
        TriggerImpl.StartMoveScale(this.gameObject, lockInfo);
      } else {
        Debug.Log("Trigger_LockFrame null!");
      }
    }
  }
}
