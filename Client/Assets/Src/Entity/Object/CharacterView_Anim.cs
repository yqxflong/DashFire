using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public enum IdleState
  {
    kNotIdle, //未进入
    kReady, //空闲开始计时
    kBegin, //开始播放空闲动画
  }

  public sealed class CharacterAnimationInfo
  {
    public bool IsPlayBeAttacked;
    public bool IsPlayReload;
    public bool IsPlayChangeWeapon;
    public bool IsPlayDead;

    public bool IsMoving;
    public MovementMode MoveMode;
    public RelMoveDir MoveDir;

    public void Reset()
    {
      IsPlayBeAttacked = false;
      IsPlayReload = false;
      IsPlayChangeWeapon = false;
      IsPlayDead = false;
      IsMoving = false;
    }
    public bool IsIdle()
    {
      return !IsMoving && !IsPlayBeAttacked && !IsPlayReload && !IsPlayChangeWeapon && !IsPlayDead;
    }
  }

  public delegate void AnimationStopCallback(string anim_name);

  public partial class CharacterView
  {
    public void PlayAnimation(Animation_Type type)
    {
      PlayAnimation(type, 1.0f);
    }

    public void PlayAnimation(Animation_Type type, float speed)
    {
      string name = GetAnimationNameByType(type);
      if (string.IsNullOrEmpty(name)) {
        return;
      }

      GfxSystem.CrossFadeAnimation(m_Actor, name);
      GfxSystem.SetAnimationSpeed(m_Actor, name, speed);
    }

    public void PlayQueuedAnimation(Animation_Type type) 
    {
      PlayQueuedAnimation(type, 1.0f);
    }

    public void PlayQueuedAnimation(Animation_Type type, float speed) 
    {
      string name = GetAnimationNameByType(type);
      if (string.IsNullOrEmpty(name)) {
        return;
      }

      GfxSystem.PlayQueuedAnimation(m_Actor, name);
      GfxSystem.SetAnimationSpeed(m_Actor, name, speed);
    }

    public void StopAnimation(Animation_Type type)
    {
      string name = GetAnimationNameByType(type);
      if (string.IsNullOrEmpty(name)) {
        return;
      }
      GfxSystem.StopAnimation(m_Actor, name);
    }

    public void ResetShootAnimation()
    {
      m_resetShootAnimation = true;
    }

    public void SetBlendTime(long move_blend_time, long blend2stand_time)
    {
      m_TotalBlendTime = move_blend_time;
      m_StandBlendTime = blend2stand_time;
    }

    public void UpdateWeaponModel(CharacterInfo entity)
    {
      if (null == entity) return;
      if (null == entity.GetShootStateInfo().GetAllWeapon()) {
        //LogSystem.Debug("--weapon: no weapon!");
        return;
      }

      if (0 != m_GunActor) {
        GfxSystem.DetachGameObject(m_GunActor);
        GfxSystem.DestroyGameObject(m_GunActor);
      }

      List<WeaponInfo> weapons = entity.GetShootStateInfo().GetAllWeapon();
      if (null == weapons || weapons.Count == 0) {
        //LogSystem.Debug("---weapon attach: no weapon!");
        return;
      }
      foreach (WeaponInfo weapon in weapons) {
        string weaponResName = weapon.ResourceName;
        string[] wrn_peer = weaponResName.Split(new string[] { "|", "," }, StringSplitOptions.None);
        if (wrn_peer.Length < 2) {
          continue;
        }
        string wrnResName = wrn_peer[0];
        string wrnSkeleton = wrn_peer[1];
        bool ret = false;
        for (int j = 0; j < CurWeaponList.Count; ++j) {
          if (CurWeaponList[j].Equals(wrnResName)) {
            ret = true;
          }
        }
        if (ret) {
          //LogSystem.Debug("---weapon change: weapon is same");
          return;
        }
        GetCurActionConfig();
        CurWeaponList.Add(wrnResName);
        m_GunActor = GameObjectIdManager.Instance.GenNextId();
        GfxSystem.CreateGameObjectForAttach(m_GunActor, wrnResName);
        GfxSystem.AttachGameObject(m_GunActor, m_Actor, wrnSkeleton);
        GfxSystem.SetGameObjectVisible(m_GunActor, true);

        LogSystem.Debug("User {0} Weapon {1} Res {2} Bind {3}", m_Actor, m_GunActor, wrnResName, wrnSkeleton);
      }
    }
    
    protected void InitAnimationSets()
    {
      List<int> action_list = GetOwner().GetActionList();
      for (int i = 0; i < action_list.Count; i++) {
        m_CurActionConfig = ActionConfigProvider.Instance.GetDataById(action_list[i]);
        InitAnimationInfo();
      }
      if (m_CurActionConfig != null) {
        m_IsUpperBodyDepart = m_CurActionConfig.m_IsUpperDepart;
      }
    }

    protected void InitAnimationInfo()
    {
      InitAnimation(Animation_Type.AT_NoGunRun, false);
      InitAnimation(Animation_Type.AT_NoGunStand, false);
      InitAnimation(Animation_Type.AT_SLEEP, false);
      InitAnimation(Animation_Type.AT_Stand, false);
      InitAnimation(Animation_Type.AT_Hold, true);
      InitAnimation(Animation_Type.AT_Idle0, false);
      InitAnimation(Animation_Type.AT_Idle1, false);

      InitAnimation(Animation_Type.AT_SlowMove, false);
      InitAnimation(Animation_Type.AT_FastMove, false);

      InitAnimation(Animation_Type.AT_RunForward, false);
      InitAnimation(Animation_Type.AT_RunBackward, false);
      InitAnimation(Animation_Type.AT_RunLeft, false);
      InitAnimation(Animation_Type.AT_RunRight, false);
      InitAnimation(Animation_Type.AT_RunForwardLeft, false);
      InitAnimation(Animation_Type.AT_RunForwardRight, false);
      InitAnimation(Animation_Type.AT_RunBackwardLeft, false);
      InitAnimation(Animation_Type.AT_RunBackwardRight, false);

      InitAnimation(Animation_Type.AT_Hurt, true);
      InitAnimation(Animation_Type.AT_HitHigh, false);

      InitAnimation(Animation_Type.AT_Attack, true);
      InitAnimation(Animation_Type.AT_EquipWeapon, true);
      InitAnimation(Animation_Type.AT_UnequipWeapon, true);
      InitAnimation(Animation_Type.AT_Reload, true);

      InitAnimation(Animation_Type.AT_Dead, false);

      InitAnimation(Animation_Type.AT_GetUp1, false);
      InitAnimation(Animation_Type.AT_GetUp2, false);
    }

    protected void InitAnimation(Animation_Type type, bool isUpperBody)
    {
      if (m_CurActionConfig == null) { return; }
      List<Data_ActionConfig.Data_ActionInfo> list = m_CurActionConfig.GetAllActionByType(type);
      for (int i = 0; i < list.Count; i++) {
        string animName = list[i].m_AnimName;
        if (null == animName || "" == animName) continue;
        if (m_InitedAnimation.Contains(animName)) { continue; }
        m_InitedAnimation.Add(animName);
      }
    }

    protected void UpdateMoveAnimation()
    {
      CharacterInfo charObj = GetOwner();
      if (null == charObj)
        return;
      if (charObj.GetMovementStateInfo().IsMoving) {
        if (!m_CharacterAnimationInfo.IsMoving) {
          m_CharacterAnimationInfo.IsMoving = true;
          m_CharacterAnimationInfo.MoveMode = charObj.GetMovementStateInfo().MovementMode;
          m_CharacterAnimationInfo.MoveDir = charObj.GetMovementStateInfo().RelativeMoveDir;
          StartMoveAnimation();
        } else if (m_CharacterAnimationInfo.MoveDir != charObj.GetMovementStateInfo().RelativeMoveDir) {
          StopMoveAnimation();
          m_CharacterAnimationInfo.MoveMode = charObj.GetMovementStateInfo().MovementMode;
          m_CharacterAnimationInfo.MoveDir = charObj.GetMovementStateInfo().RelativeMoveDir;
          StartMoveAnimation();
        } else if (m_CharacterAnimationInfo.MoveMode != charObj.GetMovementStateInfo().MovementMode) {
          StopMoveAnimation();
          m_CharacterAnimationInfo.MoveMode = charObj.GetMovementStateInfo().MovementMode;
          m_CharacterAnimationInfo.MoveDir = charObj.GetMovementStateInfo().RelativeMoveDir;
          StartMoveAnimation();
        }
      } else {
        if (m_CharacterAnimationInfo.IsMoving) {
          m_CharacterAnimationInfo.IsMoving = false;
          StopMoveAnimation();
        }
      }
    }

    protected void UpdateBeAttacked()
    {
      CharacterInfo charObj = GetOwner();
      if (null == charObj)
        return;
      if (charObj.GetBeAttack()) {
        charObj.SetBeAttack(false);
        m_CharacterAnimationInfo.IsPlayBeAttacked = true;
        string name = GetAnimationNameByType(Animation_Type.AT_Hurt);
        if (!string.IsNullOrEmpty(name)) {
          RecordAnim(name, false);
          GfxSystem.BlendAnimation(m_Actor, name, 1.0f, 0.3f);
        }
      } else {
        if (m_CharacterAnimationInfo.IsPlayBeAttacked) {
          m_CharacterAnimationInfo.IsPlayBeAttacked = false;
          /*string name = GetAnimationNameByType(Animation_Type.AT_Hurt);
          if(!string.IsNullOrEmpty(name)){
            GfxSystem.StopAnimation(m_Actor, name);
          }*/
          FadeToMoveOrHold();
        }
      }
    }

    protected void UpdateChangeWeapon()
    {
      CharacterInfo charObj = GetOwner();
      if (null == charObj)
        return;
      if (charObj.GetShootStateInfo().IsChangingWeapon()) {
        if (!m_CharacterAnimationInfo.IsPlayChangeWeapon) {
          m_CharacterAnimationInfo.IsPlayChangeWeapon = true;
          string unequipName = GetAnimationNameByType(Animation_Type.AT_UnequipWeapon);
          string equipName = GetAnimationNameByType(Animation_Type.AT_EquipWeapon);
          if (!string.IsNullOrEmpty(unequipName) && !string.IsNullOrEmpty(equipName)) {
            RecordAnim(unequipName, false);
            GfxSystem.CrossFadeQueuedAnimation(m_Actor, unequipName);
            GfxSystem.CrossFadeQueuedAnimation(m_Actor, equipName);
          }
        }
      } else {
        if (m_CharacterAnimationInfo.IsPlayChangeWeapon) {
          m_CharacterAnimationInfo.IsPlayChangeWeapon = false;
          /*string unequipName = GetAnimationNameByType(Animation_Type.AT_UnequipWeapon);
          string equipName = GetAnimationNameByType(Animation_Type.AT_EquipWeapon);
          if (!string.IsNullOrEmpty(unequipName) && !string.IsNullOrEmpty(equipName)) {
            GfxSystem.StopAnimation(m_Actor, unequipName);
            GfxSystem.StopAnimation(m_Actor, equipName);
          }*/
          FadeToMoveOrHold();
        }
      }
    }
    
    protected void UpdateDead()
    {
      CharacterInfo charObj = GetOwner();
      if (null == charObj)
        return;
      if (charObj.IsDead()) {
        if (!m_CharacterAnimationInfo.IsPlayDead) {
          m_CharacterAnimationInfo.IsPlayDead = true;
          string name = GetAnimationNameByType(Animation_Type.AT_Dead);
          if (!string.IsNullOrEmpty(name)) {
            RecordAnim(name, false);
            GfxSystem.CrossFadeAnimation(m_Actor, name);
          }
        }
      } else {
        if (m_CharacterAnimationInfo.IsPlayDead) {
          m_CharacterAnimationInfo.IsPlayDead = false;
          /*string name = GetAnimationNameByType(Animation_Type.AT_Dead);
          if (!string.IsNullOrEmpty(name)) {
            GfxSystem.StopAnimation(m_Actor, name);
          }*/
        }
      }
    }

    protected virtual void UpdateIdle()
    {
      if (!GetOwner().IsDead() && m_CharacterAnimationInfo.IsIdle()) {
        if (m_IdleState == IdleState.kNotIdle) {
          //PlayAnimation(Animation_Type.AT_Hold);
          string name = GetAnimationNameByType(Animation_Type.AT_Hold);
          if (!string.IsNullOrEmpty(name)) {
            GfxSystem.CrossFadeAnimation(m_Actor, name, 0.5f);
          }

          m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
          m_IdleState = IdleState.kReady;
          m_IdleInterval = new Random().Next(1, 3) * 1000;
        } else if (m_IdleState == IdleState.kReady) {
          if (TimeUtility.GetServerMilliseconds() - m_BeginIdleTime > m_IdleInterval) {
            string name = GetAnimationNameByType(Animation_Type.AT_Stand);
            if (!string.IsNullOrEmpty(name)) {
              GfxSystem.CrossFadeAnimation(m_Actor, name, 0.5f);
            }
            //PlayAnimation(Animation_Type.AT_Stand);
            m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
          }
        }
      } else {
        m_IdleState = IdleState.kNotIdle;
      }
    }
    
    protected virtual CharacterInfo GetOwner() { return null; }

    protected void GetAnimationDirAndSpeed(MovementMode mode, RelMoveDir rmd, float move_speed, out Animation_Type at, out float speed_factor)
    {
      Data_ActionConfig action_config = m_CurActionConfig;

      if (mode == MovementMode.LowSpeed) {
        at = Animation_Type.AT_SlowMove;
      } else if (mode == MovementMode.HighSpeed) {
        at = Animation_Type.AT_FastMove;
      } else {
        at = new Animation_Type[] {
          Animation_Type.AT_Idle0, // N
          Animation_Type.AT_RunForward, // F
          Animation_Type.AT_RunBackward, // B
          Animation_Type.AT_Idle0, // N
          Animation_Type.AT_RunLeft, // L
          Animation_Type.AT_RunForwardLeft, // LF
          Animation_Type.AT_RunBackwardLeft, // BL
          Animation_Type.AT_Idle0, // N
          Animation_Type.AT_RunRight, // R
          Animation_Type.AT_RunForwardRight, // FR
          Animation_Type.AT_RunBackwardRight, // BR
        }[(int)rmd];
      }
      //                           N    F   B       N   L       LF      BL      N   R       FR      BR
      //speed_factor = new float[] { 0f, 1f, 1 / 2f, 0f, 3 / 4f, 7 / 8f, 5 / 8f, 0f, 3 / 4f, 7 / 8f, 5 / 8f }[(int)rmd];
      if (action_config != null) {
        if (mode == MovementMode.LowSpeed) {
          speed_factor = move_speed / action_config.m_SlowStdSpeed;
        } else if (mode == MovementMode.HighSpeed) {
          speed_factor = move_speed / action_config.m_FastStdSpeed;
        } else {
          speed_factor = new float[] {
            0f,             //N
            move_speed / action_config.m_ForwardStdSpeed,  //F
            move_speed / action_config.m_BackStdSpeed,     //B
            0f,             //N
            move_speed / action_config.m_LeftRightStdSpeed, //L
            move_speed / action_config.m_LeftRightForwardStdSpeed, // LF
            move_speed / action_config.m_LeftRightBackStdSpeed, //LB
            0f,
            move_speed / action_config.m_LeftRightStdSpeed,//R
            move_speed / action_config.m_LeftRightForwardStdSpeed, //RF
            move_speed / action_config.m_LeftRightBackStdSpeed  //RB
          }[(int)rmd];
        }
      } else {
        speed_factor = 1.0f;
      }
    }

    protected Data_ActionConfig GetCurActionConfig()
    {
      int weapon_type = -1;
      if (GetOwner().GetShootStateInfo().GetCurWeaponInfo() != null) {
        weapon_type = GetOwner().GetShootStateInfo().GetCurWeaponInfo().WeaponType;
      }
      if (weapon_type != m_CurWeaponType) {
        m_LastActionConfig = m_CurActionConfig;
        m_CurWeaponType = weapon_type;
        m_CurActionConfig = ActionConfigProvider.Instance.GetCharacterCurActionConfig(GetOwner().GetActionList(), m_CurWeaponType);
      }
      return m_CurActionConfig;
    }
    
    protected Data_ActionConfig.Data_ActionInfo GetRandomActionByType(Animation_Type type)
    {
      if (m_CurActionConfig != null) {
        return m_CurActionConfig.GetRandomActionByType(type);
      }
      return null;
    }

    protected string GetAnimationNameByType(Animation_Type type)
    {
      if (m_CurActionConfig != null) {
        Data_ActionConfig.Data_ActionInfo action = m_CurActionConfig.GetRandomActionByType(type);
        if (action != null) {
          return action.m_AnimName;
        }
      }
      return null;
    }
    
    protected string GetAnimationSoundByType(Animation_Type type)
    {
      if (m_CurActionConfig != null)
      {
        Data_ActionConfig.Data_ActionInfo action = m_CurActionConfig.GetRandomActionByType(type);
        if (action != null)
        {
          return action.m_SoundId;
        }
      }
      return null;
    }
    
    protected void RecordAnim(string name, bool is_uppper = false)
    {
      if (string.IsNullOrEmpty(name)) {
        return;
      }
      if (m_CurActionConfig == null) {
        m_CurWholeBodyAnimName = name;
        return;
      }
      if (m_CurActionConfig.m_IsUpperDepart && is_uppper) {
        m_CurUpperBodyAnimName = name;
      } else {
        m_CurWholeBodyAnimName = name;
      }
    }
        
    protected bool IsAnimationInited(Animation_Type anim_type)
    {
      string name = GetAnimationNameByType(anim_type);
      if (string.IsNullOrEmpty(name)) {
        return true;
      }
      return m_InitedAnimation.Contains(name);
    }

    protected void UpdateHitRecoverSpeed()
    {
      if (!string.IsNullOrEmpty(m_CurWholeBodyAnimName)) {
        GfxSystem.SetAnimationSpeed(m_Actor, m_CurWholeBodyAnimName, GetOwner().HitRecoverActionSpeed);
      }
      if (!string.IsNullOrEmpty(m_CurUpperBodyAnimName)) {
        GfxSystem.SetAnimationSpeed(m_Actor, m_CurUpperBodyAnimName, GetOwner().HitRecoverActionSpeed);
      }
      if (!string.IsNullOrEmpty(m_CurMoveAnimation)) {
        GfxSystem.SetAnimationSpeed(m_Actor, m_CurMoveAnimation, GetOwner().HitRecoverActionSpeed);
      }
      if (!string.IsNullOrEmpty(m_LastMoveAnimation)) {
        GfxSystem.SetAnimationSpeed(m_Actor, m_LastMoveAnimation, GetOwner().HitRecoverActionSpeed);
      }
    }

    private void StartMoveAnimation()
    {
      Animation_Type type = Animation_Type.AT_None;
      float speed_factor;
      float move_speed = GetOwner().GetActualProperty().MoveSpeed;
      GetAnimationDirAndSpeed(m_CharacterAnimationInfo.MoveMode,
        m_CharacterAnimationInfo.MoveDir,
        move_speed, out type, out speed_factor);
      string name = GetAnimationNameByType(type);
      if (!string.IsNullOrEmpty(name)) {
        RecordAnim(name, false);
        GfxSystem.CrossFadeAnimation(m_Actor, name, 0.05f);
        //GfxSystem.SetAnimationSpeed(m_Actor, name, (float)speed_factor);
      }
    }

    private void StopMoveAnimation()
    {
      /*Animation_Type type = Animation_Type.AT_None;
      float speed_factor;
      float move_speed = GetOwner().GetActualProperty().MoveSpeed;
      GetAnimationDirAndSpeed(m_CharacterAnimationInfo.MoveMode,
        m_CharacterAnimationInfo.MoveDir,
        move_speed, out type, out speed_factor);
      string name = GetAnimationNameByType(type);
      if (!string.IsNullOrEmpty(name)) {
        GfxSystem.StopAnimation(m_Actor, name);
      }*/
      FadeToHold();
    }

    private void FadeToHold()
    {
      string name = GetAnimationNameByType(Animation_Type.AT_Hold);
      if (!string.IsNullOrEmpty(name)) {
        GfxSystem.CrossFadeAnimation(m_Actor, name);
      }
    }

    private void FadeToMoveOrHold()
    {
      if (m_CharacterAnimationInfo.IsMoving) {
        MovementMode moveMode = GetOwner().GetMovementStateInfo().MovementMode;
        RelMoveDir moveDir = GetOwner().GetMovementStateInfo().RelativeMoveDir;
        StartMoveAnimation();
      } else {
        FadeToHold();
      }
    }
    
    protected bool m_resetShootAnimation = false;
    private int m_CurWeaponType = -2;
    private Data_ActionConfig m_LastActionConfig = null;
    protected Data_ActionConfig m_CurActionConfig = null;

    protected CharacterAnimationInfo m_CharacterAnimationInfo = new CharacterAnimationInfo();
    protected HashSet<string> m_InitedAnimation = new HashSet<string>();
    protected bool m_IsUpperBodyDepart = false;
    protected long m_TotalBlendTime = 150;
    protected long m_StandBlendTime = 250;
    protected string m_LastUpperBodyAnimName = "";
    protected string m_LastWholeBodyAnimName = "";
    protected string m_CurUpperBodyAnimName = "";
    protected string m_CurWholeBodyAnimName = "";
    protected string m_LastMoveAnimation = "";
    protected string m_CurMoveAnimation = "";

    protected string m_LastSkillAction = "";
    protected string m_LastImpactAction = "";
    protected string m_LastInterAction = "";
    protected string m_LastWeaponAction = "";

    protected IdleState m_IdleState = IdleState.kNotIdle;
    protected long m_BeginIdleTime = 0;
    protected long m_IdleInterval = 0;
  }
}
