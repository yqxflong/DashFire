using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ShooterSkillLogic_ExShoot : ShooterSkillLogic_Base {
  enum SkillSection : int {
    None,
    ShootStart,
    Shooting,
    ShootEnd,
  }
  private Animation m_AnimationCom = null;
  private SkillSceneObjManager m_SceneObjMgr = null;
  private bool m_IsPrepareToEnd = false;
  private float m_StartEndTs = 0;
  private float m_HoldEndTs = 0;
  private float m_MissleEndTs = 0;
  private bool m_IsEnterMissle = false;
  private float m_CreateMissleLastTs = 0;
  private float m_InitSkillDuration = 0;

  //Config
  public SkillAnimInfo ShootStartAnimInfo = null;
  public SkillAnimInfo ShootIdleAnimInfo = null;
  public SkillAnimInfo ShootAnimInfo = null;
  public SkillAnimInfo ShootEndAnimInfo = null;

  public int TargetPosGuildEffectId = -1;
  public int MissleId = -1;
  public float MissleInterval = 0.4f;
  public float ShootHoldTime = 2.0f;

  /// <summary>
  /// Unity inner api
  /// </summary>
  internal void Awake() {
    OnInitialize();
    ResetSkill();
  }
  // Update is called once per frame
  void Update() {
    ProcessInput();
    SkillComponent.ExecuteSection();
  }

  /// <summary>
  /// Skill logic
  /// </summary>
  public override void OnInitialize() {
    SkillComponent = new SkillSectionCom();
    SkillComponent.RegisterSection((int)SkillSection.None, null);
    SkillComponent.RegisterSection((int)SkillSection.ShootStart, OnShootStart);
    SkillComponent.RegisterSection((int)SkillSection.Shooting, OnShooting);
    SkillComponent.RegisterSection((int)SkillSection.ShootEnd, OnShootEnd);

    TouchManager.OnFingerEvent += OnSkillFingerEvent;
  }
  public override bool StartSkill(Vector3 targetPos) {
    ResetSkill();
    CDBeginTime = Time.time;
    Vector3 curTargetPos;
    if (UseCurTargetPos) {
      if (TriggerImpl.GetCurTargetPos(this.gameObject, out curTargetPos)) {
        targetPos = curTargetPos;
      } else {
        return StopSkill();
      }
    }
    if (UseFaceTargetPos || targetPos == Vector3.zero) {
      TargetPos = Script_Util.GetRoleFaceTargetPos(this.gameObject);
    } else {
      TargetPos = targetPos;
      TriggerImpl.SetFacePos(this.gameObject, TargetPos);
    }
    TriggerImpl.ResetTarget(this.gameObject);
    SkillComponent.ChangeSection((int)SkillSection.ShootStart);
    return true;
  }
  public override bool StopSkill() {
    if (IsExecuting()) {
      SkillComponent.ChangeSection((int)SkillSection.ShootEnd, true);
    }
    return true;
  }
  public override void ResetSkill() {
    m_AnimationCom = gameObject.animation;
    m_SceneObjMgr = gameObject.GetComponent<SkillSceneObjManager>();
    SkillElapsed = 0;
    IsCanBreak = false;
    IsCanBreakBySkill = false;
    TargetPos = Vector3.zero;
    m_HoldEndTs = 0;
    m_MissleEndTs = 0;
    m_CreateMissleLastTs = 0;
    m_IsEnterMissle = false;
    m_IsPrepareToEnd = false;
    m_InitSkillDuration = SkillDuration;
    SkillComponent.ChangeSection((int)SkillSection.None);
  }

  /// <summary>
  /// Sections
  /// </summary>
  private void OnShootStart() {
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_ex_skill", "ui", "GunWoman", true);

    // Notify
    TriggerImpl.StopAllAnim(this.gameObject);
    LogicSystem.NotifyGfxAnimationStart(gameObject);
    if (ControlMove) {
      LogicSystem.NotifyGfxMoveControlStart(gameObject);
    }

    // Weapon
    TriggerImpl.ChangeWeaponById(this.gameObject, WeaponId);
    TriggerImpl.ChangeWeaponById(this.gameObject, SubWeaponId);

    // Animation
    m_StartEndTs = TriggerImpl.GetAnimTime(this.gameObject, ShootStartAnimInfo);
    m_HoldEndTs = ShootHoldTime + m_StartEndTs;
    m_MissleEndTs = m_HoldEndTs + SkillDuration;
    SkillDuration = m_MissleEndTs + TriggerImpl.GetAnimTime(this.gameObject, ShootEndAnimInfo);
    
    TriggerImpl.PlayAnims(this.gameObject, ShootStartAnimInfo);
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", true);

    SkillComponent.ChangeNextSection();
  }
  private void OnShooting() {
    if (IsSkillOver()) {
      SkillComponent.ChangeNextSection();
      return;
    } else if (SkillElapsed >= m_MissleEndTs) {
      if (!m_IsPrepareToEnd) {
        m_IsPrepareToEnd = true;
        m_IsEnterMissle = false;
        TriggerImpl.StopAllAnim(this.gameObject);
        TriggerImpl.PlayAnim(this.gameObject, ShootEndAnimInfo);
      }
    } else if (SkillElapsed >= m_HoldEndTs) {
      if (!m_IsEnterMissle) {
        m_IsEnterMissle = true;
        TriggerImpl.StopAllAnim(this.gameObject);
        TriggerImpl.PlayAnim(this.gameObject, ShootIdleAnimInfo);
      }
    } else if (SkillElapsed >= m_StartEndTs) {
      if (!TriggerImpl.IsAnimPlaying(this.gameObject, ShootAnimInfo)) {
        TriggerImpl.StopAllAnim(this.gameObject);
        TriggerImpl.PlayAnim(this.gameObject, ShootAnimInfo);
      }
    }
    SkillElapsed += Time.deltaTime;
  }
  private void OnShootEnd() {
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_ex_skill", "ui", "GunWoman", false);

    TriggerImpl.StopAnim(this.gameObject, ShootStartAnimInfo);
    TriggerImpl.StopAnim(this.gameObject, ShootIdleAnimInfo);
    TriggerImpl.StopAnim(this.gameObject, ShootAnimInfo);
    TriggerImpl.StopAnim(this.gameObject, ShootEndAnimInfo);
    // Weapon
    TriggerImpl.ChangeWeaponById(this.gameObject, EndWeaponId);
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", false);
    SkillDuration = m_InitSkillDuration;
    ResetSkill();
    //Debug.Log("ShooterSkillLogic_ExShoot OnShootEnd!");

    LogicSystem.NotifyGfxAnimationFinish(gameObject);
    if (ControlMove) {
      TriggerImpl.StopMove(this.gameObject);
    }
  }
  private void ProcessInput() {
    if (Input.GetKeyDown(TestKeyCode)) {
      Vector3 posOff = new Vector3(0, 0, 2.0f);
      Vector3 targetPos = this.gameObject.transform.position + this.gameObject.transform.rotation * posOff;
      StartSkill(targetPos);
    }
  }
  private void OnSkillFingerEvent(FingerEvent finger) {
    if (m_IsEnterMissle && IsExecuting() && "OnFingerMove" == finger.Name) {
      //Debug.Log("OnSkillFingerEvent OnFingerMove!");
      TargetPos = finger.GetTouchToWorldPoint();
      //TriggerImpl.SetFacePos(this.gameObject, TargetPos);
      CreateMissle(TargetPos, true);
    }
    if (m_IsEnterMissle && IsExecuting() && "OnFingerDown" == finger.Name) {
      //Debug.Log("OnSkillFingerEvent SupportHoldSkillByFinger OnFingerDown!");
      TargetPos = finger.GetTouchToWorldPoint();
      //TriggerImpl.SetFacePos(this.gameObject, TargetPos);
      CreateMissle(TargetPos, false);
    }
  }
  private void CreateTargetGuilde(Vector3 targetPos) {
    if (m_SceneObjMgr == null) {
      Debug.Log("ShooterSkillLogic_ExShoot CreateTargetGuilde m_SceneObjMgr miss!");
      return;
    }
    SkillEffectInfo effectInfo = m_SceneObjMgr.TryGetSkillEffectInfo(TargetPosGuildEffectId);
    if (effectInfo != null) {
      effectInfo.EffectPos = TargetPos + new Vector3(0, 0.2f, 0);
      effectInfo.EffectRot = Vector3.zero;
      if (effectInfo != null) {
        TriggerImpl.PlayEffect(effectInfo);
      }
    }
  }
  private void CreateMissleInternal(Vector3 targetPos) {
    if (m_SceneObjMgr == null) {
      Debug.Log("ShooterSkillLogic_ExShoot CreateMissle m_SceneObjMgr miss!");
      return;
    }

    SceneObject_MissleInfo bulletInfo = m_SceneObjMgr.TryGetMissleInfo(MissleId);
    if (bulletInfo != null && bulletInfo.SceneObjInfo != null) {
      bulletInfo = bulletInfo.Clone();
      bulletInfo.Attacker = this.gameObject;
      bulletInfo.SceneObjInfo.EffectPos += targetPos;
      bulletInfo.MoveStartPos += targetPos;
      bulletInfo.MoveTargetPos += targetPos;

      GameObject tBulletObj = TriggerImpl.PlayEffect(bulletInfo.SceneObjInfo);
      tBulletObj.SendMessage("Active", bulletInfo);
    }
  }

  private void CreateMissle(Vector3 targetPos, bool isNeedInterval = false) {
    if (m_IsPrepareToEnd) { return; }

    if (isNeedInterval) {
      if (SkillElapsed - m_CreateMissleLastTs >= MissleInterval) {
        m_CreateMissleLastTs = SkillElapsed;
      } else {
        return;
      }
    }

    CreateTargetGuilde(TargetPos);
    CreateMissleInternal(TargetPos);
  }
}
