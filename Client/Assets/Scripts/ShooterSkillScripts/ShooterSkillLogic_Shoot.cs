using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ShooterSkillLogic_Shoot : ShooterSkillLogic_Base {
  enum SkillSection : int {
    None,
    ShootStart,
    Shooting,
    ShootEnd,
  }
  private Animation m_AnimationCom = null;

  //Config
  public SkillAnimInfo ShootAnimInfo = null;
  public MathRange YRotateRange = null;
  public Vector3 GunRotate = Vector3.zero;
  public int StartMovementId = -1;
  public int EndMovementId = -1;

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
    //TouchManager.OnGestureEvent += OnSkillGestureEvent;
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
      AdjustTargetPos();
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
    SkillElapsed = 0;
    IsCanBreak = false;
    IsCanBreakBySkill = false;
    TargetPos = Vector3.zero;
    SkillComponent.ChangeSection((int)SkillSection.None);
  }

  /// <summary>
  /// Sections
  /// </summary>
  private void OnShootStart() {
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
    TriggerImpl.PlayAnim(this.gameObject, ShootAnimInfo);
    SkillDuration = Mathf.Max(TriggerImpl.GetAnimTime(this.gameObject, ShootAnimInfo), SkillDuration);

    if (SupportRotateByHold) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", true);
    }
    if (SupportRotateByJoyStick) {
      TriggerImpl.ShowJoyStick(this.gameObject);
    }

    TriggerImpl.StartMoveById(this.gameObject, StartMovementId);

    SkillComponent.ChangeNextSection();
  }
  private void OnShooting() {
    if (IsSkillOver()) {
      SkillComponent.ChangeNextSection();
    } else {
      SkillElapsed += Time.deltaTime;
    }
  }
  private void OnShootEnd() {
    TriggerImpl.StopAnim(this.gameObject, ShootAnimInfo);
    // Weapon
    TriggerImpl.ChangeWeaponById(this.gameObject, EndWeaponId);
    if (SupportRotateByHold) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", false);
    }
    if (SupportRotateByJoyStick) {
      TriggerImpl.HideJoyStick(this.gameObject);
    }

    TriggerImpl.StartMoveById(this.gameObject, EndMovementId);
    ResetSkill();

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
    if (SupportRotateByHold && IsExecuting() && "OnTwoFingerMove" == finger.Name) {
      TargetPos = finger.GetTouchToWorldPoint();
      AdjustTargetPos();
    }
    if (SupportRotateByHold && IsExecuting() && "OnTwoFingerDown" == finger.Name) {
      TargetPos = finger.GetTouchToWorldPoint();
      AdjustTargetPos();
    }
  }
  private void AdjustTargetPos() {
    float weaponHeight = TriggerImpl.GetWeaponHeight(this.gameObject);
    Vector3 adjustTargetPos = Script_Util.AdjustTargetPos(
      this.gameObject.transform,
      TargetPos,
      Camera.main.transform,
      weaponHeight,
      GunRotate);
    //Debug.Log(string.Format("AdjustTargetPos TargetPos:{0} adjustTargetPos:{1} weaponHeight:{2}", 
    //  TargetPos, adjustTargetPos, weaponHeight));
    TriggerImpl.SetFacePos(this.gameObject, adjustTargetPos, YRotateRange);
  }
}
