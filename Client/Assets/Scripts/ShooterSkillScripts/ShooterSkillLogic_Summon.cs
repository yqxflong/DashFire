using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class ShooterSkillLogic_Summon : ShooterSkillLogic_Base {
  enum SkillSection : int {
    None,
    SummonStart,
    Summoning,
    SummonEnd,
  }
  private Animation m_AnimationCom = null;

  //Config
  public SkillAnimInfo SummonAnimInfo = null;

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
    SkillComponent.RegisterSection((int)SkillSection.SummonStart, OnSummonStart);
    SkillComponent.RegisterSection((int)SkillSection.Summoning, OnSummoning);
    SkillComponent.RegisterSection((int)SkillSection.SummonEnd, OnSummonEnd);
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
    SkillComponent.ChangeSection((int)SkillSection.SummonStart);
    return true;
  }
  public override bool StopSkill() {
    if (IsExecuting()) {
      SkillComponent.ChangeSection((int)SkillSection.SummonEnd, true);
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
  private void OnSummonStart() {
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
    TriggerImpl.PlayAnim(this.gameObject, SummonAnimInfo);
    SkillDuration = TriggerImpl.GetAnimTime(this.gameObject, SummonAnimInfo);

    if (SupportRotateByHold) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", true);
    }
    if (SupportRotateByJoyStick) {
      TriggerImpl.ShowJoyStick(this.gameObject);
    }

    SkillComponent.ChangeNextSection();
  }
  private void OnSummoning() {
    if (IsSkillOver()) {
      SkillComponent.ChangeNextSection();
    } else {
      SkillElapsed += Time.deltaTime;
    }
  }
  private void OnSummonEnd() {
    TriggerImpl.StopAnim(this.gameObject, SummonAnimInfo);
    TriggerImpl.ChangeWeaponById(this.gameObject, EndWeaponId);
    if (SupportRotateByHold) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_touch_dir", "ui", false);
    }
    if (SupportRotateByJoyStick) {
      TriggerImpl.HideJoyStick(this.gameObject);
    }
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
}
