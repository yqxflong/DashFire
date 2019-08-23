using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire.Debug;
using ScriptRuntime;

namespace DashFire
{
  enum JumpState
  {
    kNotJump,
    kJumpBeginStart,
    kJumpBeginPlaying,
    kJumpBeginOver,
    kJumpingPlaying,
    kJumpEndStart,
    kJumpEndPlaying,
    kJumpEndOver,
  };

  public class NpcView : CharacterView
  {
    public NpcInfo Npc
    {
      get { return m_Npc; }
    }

    protected override CharacterInfo GetOwner()
    {
      return m_Npc;
    }

    public void Create(NpcInfo npc)
    {
      Init();
      if (null != npc) {
        m_Npc = npc;
        m_Npc.OnBeginAttack = ResetShootAnimation;
        MovementStateInfo msi = m_Npc.GetMovementStateInfo();
        Vector3 pos = msi.GetPosition3D();
        float dir = msi.GetFaceDir();
        CreateActor(m_Npc.GetId(), m_Npc.GetModel(), pos, dir, m_Npc.Scale);
        InitAnimationSets();
        UpdateWeaponModel(m_Npc);
      }
    }
    public void Destroy()
    {
      Release();
      DestroyActor();
    }

    public void Update()
    {
      UpdateAttr();
      UpdateSpatial();
      UpdateModel();
      UpdateAnimation();
    }

    public void UpdateModel()
    {
      /*
      if (m_Npc.ControllerObject != null && m_Npc.IsAttachControler && !m_IsControlerModelAttached) {
        m_ControlerActor = GfxSystemExt.GfxSystem.Instance.CreateGfxActor(m_Npc.ControllerObject.GetModel());
        if (m_ControlerActor != null && m_ControlerActor.IsValid && !string.IsNullOrEmpty(m_Npc.AttachNodeName)) {
          Actor.AddAttachedActor(m_ControlerActor, m_Npc.AttachNodeName);
          m_ControlerActor.SetVisible(true);
          m_ControlerActor.PlayParticle(true);
          Data_ActionConfig.Data_ActionInfo ride_action = m_Npc.ControllerObject.GetCurActionConfig().GetRandomActionByType(Animation_Type.AT_RIDE);
          if (ride_action != null) {
            LogSystem.Debug("---npc control: play controler anim is {0}", ride_action.m_AnimName);
            m_ControlerActor.SetWrapMode(ride_action.m_AnimName, GfxAnimWrapMode.LOOP);
            m_ControlerActor.PlayAnimation(ride_action.m_AnimName, GfxAnimPlayType.CROSS);
          } else {
            LogSystem.Debug("---npc control: play controler anim is null");
          }
        }
        m_IsControlerModelAttached = true;
      } else if (m_Npc.ControllerObject == null && m_IsControlerModelAttached) {
        if (m_ControlerActor != null) {
          Actor.RemoveAttachedActor(m_ControlerActor, true);
        }
        m_IsControlerModelAttached = false;
      }*/
    }

    protected override bool UpdateVisible(bool visible)
    {
      bool ret = visible;
      if (null != m_Npc) {
        if (null != WorldSystem.Instance.GetPlayerSelf() && m_Npc.OwnerId > 0) {
          if (m_Npc.OwnerId == WorldSystem.Instance.PlayerSelfId) {
            ret = true;
          } else {
            ret = false;
          }
          SetVisible(ret);
        } else {
          if (visible) {
            if (m_Npc.IsDead()) {
              SetVisible(false);
              if (!string.IsNullOrEmpty(m_Npc.DeathModel))
                SetVisible(true, m_Npc.DeathModel);
            } else {
              SetVisible(true);
              if (!string.IsNullOrEmpty(m_Npc.DeathModel))
                SetVisible(false, m_Npc.DeathModel);
            }
          } else {
            SetVisible(false);
          }
        }
      } else {
        SetVisible(false);
        ret = false;
      }
      return ret;
    }

    private void UpdateAttr()
    {
      if (null != m_Npc) {
        ObjectInfo.Blood = m_Npc.Hp;
        ObjectInfo.MaxBlood = m_Npc.GetActualProperty().HpMax;
      }
    }

    private void UpdateSpatial()
    {
      if (null != m_Npc) {
        if (ObjectInfo.IsGfxMoveControl) {
          if (ObjectInfo.DataChangedByGfx) {
            MovementStateInfo msi = m_Npc.GetMovementStateInfo();
            msi.PositionX = ObjectInfo.X;
            msi.PositionY = ObjectInfo.Y;
            msi.PositionZ = ObjectInfo.Z;
            msi.SetFaceDir(ObjectInfo.FaceDir);

            ObjectInfo.DataChangedByGfx = false;
          }
        } else {
          if (ObjectInfo.DataChangedByGfx) {
            MovementStateInfo msi = m_Npc.GetMovementStateInfo();
            msi.PositionX = ObjectInfo.X;
            msi.PositionY = ObjectInfo.Y;
            msi.PositionZ = ObjectInfo.Z;
            msi.SetFaceDir(ObjectInfo.FaceDir);

            ObjectInfo.DataChangedByGfx = false;
          }
          UpdateMovement();
        }
      }
    }

    private void UpdateAnimation()
    {
      if (!CanAffectPlayerSelf) return;
      if (null != m_Npc) {
        if (ObjectInfo.IsGfxAnimation) {
          m_CharacterAnimationInfo.Reset();
          return;
        }

        if (m_Npc.CanControl && m_Npc.ControllerObject == null) {
          PlayAnimation(Animation_Type.AT_SLEEP);
          return;
        }

        if (m_Npc.NpcType == (int)NpcTypeEnum.InteractiveNpc && m_Npc.IsHaveStateFlag(CharacterState_Type.CST_Opened)) {
          if (!IsAnimationInited(Animation_Type.AT_SkillSection2)) {
            InitAnimation(Animation_Type.AT_SkillSection2, false);
          }
          PlayAnimation(Animation_Type.AT_SkillSection2);
          return;
        }

        if (m_Npc.IsBorning) {
          UpdateBornAnimation();
        }

        UpdateMoveAnimation();
        //UpdateBeAttacked();
        //UpdateDead();
        UpdateIdle();

        HilightType hi = m_Npc.GetHilightType();
        if (hi != m_LastHilightType) {
          Hilight(hi);
          m_LastHilightType = hi;
        }

        if (!m_Npc.IsHilightColorShowed) {
          ScriptRuntime.Vector4 color = m_Npc.GetHilightColor();
          Hilight(color);
          m_Npc.IsHilightColorShowed = true;
        }
      }
    }

    protected override void UpdateIdle(){
      if((int)AiStateId.Idle == m_Npc.GetAiStateInfo().CurState){
        if (!GetOwner().IsDead() && m_CharacterAnimationInfo.IsIdle()) {
          if (m_IdleState == IdleState.kNotIdle) {
            PlayAnimation(Animation_Type.AT_Stand);
            m_IdleState = IdleState.kBegin;
            m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
            m_IdleInterval = Helper.Random.Next(3, 7) * 1000;
          }else if(m_IdleState == IdleState.kBegin){
            if (TimeUtility.GetServerMilliseconds() - m_BeginIdleTime > m_IdleInterval) {
              PlayAnimation(GetNextIdleAnim());
              PlayQueuedAnimation(Animation_Type.AT_Stand);
              m_BeginIdleTime = TimeUtility.GetServerMilliseconds();
              m_IdleInterval = Helper.Random.Next(3, 7) * 1000;
            }
          } else {
            m_IdleState = IdleState.kNotIdle;
          }
        }
      } else {
        base.UpdateIdle();
      }
    }

    private Animation_Type GetNextIdleAnim() {
      if (m_IdleAnimDict.Count > 0) {
        return m_IdleAnimDict[Helper.Random.Next(0, m_IdleAnimDict.Count)];
      } else {
        return Animation_Type.AT_None;
      }
    }

    public void SetIdleAnim(List<Animation_Type> anims) {
      m_IdleAnimDict = anims;
    }
    
    private void UpdateBornAnimation()
    {
      if (m_CurActionConfig == null || Actor == 0) {
        return;
      }
      if (m_Npc.BornAnimTimeMs == 0) {
        return;
      }
      PlayAnimation(Animation_Type.AT_Born);
      RecordAnim(GetAnimationNameByType(Animation_Type.AT_Born), false);

    }

    private void UpdateHitHighAnimation()
    {
      PlayAnimation(Animation_Type.AT_HitHigh);
      RecordAnim(GetAnimationNameByType(Animation_Type.AT_HitHigh), false);
    }
    
    private void Init()
    {
      m_resetShootAnimation = false;
      m_CurActionConfig = null;
      m_LastHilightType = HilightType.kNone;
      m_IdleAnimDict.Clear();
      m_Npc = null;
    }

    private void Release()
    {
    }
    
    private HilightType m_LastHilightType = HilightType.kNone;
    private NpcInfo m_Npc = null;
    private List<Animation_Type> m_IdleAnimDict = new List<Animation_Type>();
    //private GfxActor m_ControlerActor;
  }
}
