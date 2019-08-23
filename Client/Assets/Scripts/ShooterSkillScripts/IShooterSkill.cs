#define SHOOTER_LOG

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public enum ShooterSkillId : int {
  SkillLogic_None = -1,
  SkillLogic_TribleRifle,
  SkillLogic_FlightShoot,
  SkillLogic_TerminalShoot,
  SkillLogic_SummonChest,
  SkillLogic_SprintGrenade,
  SkillLogic_Flashbomb,
  SkillLogic_ThrowMine,
  SkillLogic_SummonObstruction,
  SkillLogic_FlameFire,
  SkillLogic_SummonMissle,
  SkillLogic_RushDig,
  SkillLogic_SummonEbomb,
  SkillLogic_SprintGrenadeSecond,
  SkillLogic_TribleRifle2,
  SkillLogic_SwitchWeapon,
  SkillLogic_MachineGun,
  SkillLogic_AttractShoot,
  SkillLogic_GroundShoot,
  SkillLogic_TribleRifle3,
  SkillLogic_TribleRifle4,
  SkillLogic_TribleRifle5,
  SkillLogic_TribleRifle6,
  SkillLogic_HoldShoot1,
  SkillLogic_HoldShoot2,
  SkillLogic_HoldShoot3,
  SkillLogic_HoldShoot4,
  SkillLogic_HoldShoot5,
  SkillLogic_HoldShoot6,
  SkillLogic_ExShoot,
  SkillLogic_GroundShoot2,
  SkillLogic_GroundShoot3,
  SkillLogic_GroundShoot4,
  SkillLogic_GroundShoot5,
  SkillLogic_GroundShoot6,
  SkillLogic_Test1,
  SkillLogic_Test2,
  SkillLogic_Test3,
  SkillLogic_Test4,
  SkillLogic_Test5,
  SkillLogic_Test6,
  SkillLogic_Test7,
  SkillLogic_Test8,
  SkillLogic_Test9,
  SkillLogic_Test10,
  SkillLogic_Test11,
  SkillLogic_Test12,
  SkillLogic_Test13,
  SkillLogic_Test14,
  SkillLogic_Test15,
  SkillLogic_Test16,
  SkillLogic_Test17,
  SkillLogic_Test18,
  SkillLogic_Test19,
  SkillLogic_Test20,
  SkillLogic_Test21,
  SkillLogic_Test22,
  SkillLogic_Test23,
  SkillLogic_Test24,
  SkillLogic_Test25,
  SkillLogic_Test26,
  SkillLogic_Test27,
  SkillLogic_Test28,
  SkillLogic_Test29,
  SkillLogic_Test30,
  Max,
}

public interface IShooterSkill {
  void OnInitialize();
  bool StartSkill(Vector3 targetPos);
  bool StopSkill();
  void ResetSkill();

  bool CanBreak();
  void SetCanBreak(bool val);
  bool CanBreakBySkill();
  void SetCanBreakBySkill(bool val);
  bool CanStart();

  void BeginCD();
  float GetCD();
  bool IsInCD();

  float GetNextInputTime();
  float GetLockInputTime();

  bool IsExecuting();
  bool IsSkillOver();
  bool IsDefaultCategory();

  Vector3 GetTargetPos();

  int GetSkillId();
  int GetNextSkillId();
  void SetNextSkillId(int id);
  int GetQSkillId();
  int GetESkillId();
  SkillCategory GetCategory();
  List<ShooterSkillId> GetNextSkillIdList();
  int GetWeaponId(MasterWeaponType master = MasterWeaponType.Master);

  float GetSkillCastRange();
  float GetTargetChooseRange();

  bool IsAttackAfterSkill();

  void OnSkillTriggerEnter(Object param);
}

public abstract class ShooterSkillLogic_Base : MonoBehaviour, IShooterSkill {
  protected SkillSectionCom SkillComponent { get; set; }
  protected float SkillElapsed { get; set; }
  protected float CDBeginTime { get; set; }
  protected bool IsCanBreak { get; set; }
  protected bool IsCanBreakBySkill { get; set; }
  protected Vector3 TargetPos { get; set; }
  protected float StartTime { get; set; }

  // Config
  public ShooterSkillId SkillId = ShooterSkillId.SkillLogic_None;
  public ShooterSkillId NextSkillId = ShooterSkillId.SkillLogic_None;
  public List<ShooterSkillId> NextSkillIdList = new List<ShooterSkillId>();
  public ShooterSkillId QSkillId = ShooterSkillId.SkillLogic_None;
  public ShooterSkillId ESkillId = ShooterSkillId.SkillLogic_None;
  public SkillCategory CategoryId = SkillCategory.kNone;
  public int WeaponId = -1;
  public int SubWeaponId = -1;
  public int EndWeaponId = -1;
  public bool IsCategoryDefault = false;

  public float m_NextInputTime = 0.0f;
  public float m_LockInputTime = 0.0f;

  public float m_CD = 0;
  public float ChargeTime = 0;
  public float SkillDuration = float.MinValue;
  public bool ControlMove = true;
  public bool UseFaceTargetPos = false;
  public bool SupportRotateByJoyStick = false;
  public bool SupportRotateByHold = false;
  public bool UseCurTargetPos = false;
  public bool m_IsAttackAfterSkill = false;

  public float m_SkillCastRange = 10.0f;
  public float m_TargetChooseRange = 0.5f;

  public KeyCode TestKeyCode = KeyCode.Keypad1;

  public abstract void OnInitialize();
  public abstract bool StartSkill(Vector3 targetPos);
  public abstract bool StopSkill();
  public abstract void ResetSkill();
  public virtual void OnSkillTriggerEnter(Object param) { }

  public bool CanBreak() { return IsCanBreak || !IsExecuting(); }
  public void SetCanBreak(bool val) { IsCanBreak = val; }
  public bool CanBreakBySkill() { return IsCanBreakBySkill || !IsExecuting(); }
  public void SetCanBreakBySkill(bool val) { IsCanBreakBySkill = val; }
  public bool CanStart() { return !IsInCD(); }

  public void BeginCD() { CDBeginTime = Time.time; }
  public float GetCD() { return m_CD; }
  public bool IsInCD() { return Time.time <= (CDBeginTime + m_CD); }

  public float GetNextInputTime() { return m_NextInputTime; }
  public float GetLockInputTime() { return m_LockInputTime; }

  public bool IsExecuting() { return SkillComponent.CurSectionId != (int)0; }
  public bool IsSkillOver() { return SkillElapsed >= SkillDuration; }
  public bool IsDefaultCategory() { return IsCategoryDefault; }

  public Vector3 GetTargetPos() { return TargetPos; }

  public int GetSkillId() { return (int)SkillId; }
  public int GetNextSkillId() { return (int)NextSkillId; }
  public void SetNextSkillId(int id) { NextSkillId = (ShooterSkillId)id; }
  public int GetQSkillId() { return (int)QSkillId; }
  public int GetESkillId() { return (int)ESkillId; }
  public SkillCategory GetCategory() { return CategoryId; }
  public List<ShooterSkillId> GetNextSkillIdList() { return NextSkillIdList;}

  public int GetWeaponId(MasterWeaponType master = MasterWeaponType.Master) { 
    return (master == MasterWeaponType.Master)? WeaponId : SubWeaponId; 
  }

  public float GetSkillCastRange() { return m_SkillCastRange; }
  public float GetTargetChooseRange() { return m_TargetChooseRange; }

  public bool IsAttackAfterSkill() { return m_IsAttackAfterSkill; }
}
