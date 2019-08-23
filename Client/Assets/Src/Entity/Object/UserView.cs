using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class UserView : CharacterView
  {
    public UserInfo User
    {
      get { return m_User; }
    }

    public void Create(UserInfo user)
    {
      Init();
      if (null != user) {
        m_User=user;
        m_User.OnBeginAttack = ResetShootAnimation;
        MovementStateInfo msi = m_User.GetMovementStateInfo();
        Vector3 pos = msi.GetPosition3D();
        float dir = msi.GetFaceDir();
        CreateActor(m_User.GetId(), m_User.GetModel(), pos, dir, m_User.Scale);
        InitAnimationSets();
        UpdateWeaponModel(m_User);
        if (user.GetId() == WorldSystem.Instance.PlayerSelfId) {
          GfxSystem.MarkPlayerSelf(Actor);
        }
      }
    }
    public void Destroy()
    {
      DestroyActor();
      Release();
    }
    public void Update()
    {
      UpdateAttr();
      UpdateSpatial();
      UpdateAnimation();
    }

    protected override bool UpdateVisible(bool visible)
    {
      SetVisible(visible);
      return visible;
    }

    private void UpdateAttr()
    {
      if (null != m_User) {
        ObjectInfo.Blood = m_User.Hp;
        ObjectInfo.MaxBlood = m_User.GetActualProperty().HpMax;
        ObjectInfo.Rage = m_User.Rage;
        ObjectInfo.MaxRage = m_User.GetActualProperty().RageMax;
      }
    }
    
    private void UpdateSpatial()
    {
      if (null != m_User) {
        if (ObjectInfo.IsGfxMoveControl) {
          if (ObjectInfo.DataChangedByGfx) {
            MovementStateInfo msi = m_User.GetMovementStateInfo();
            msi.PositionX = ObjectInfo.X;
            msi.PositionY = ObjectInfo.Y;
            msi.PositionZ = ObjectInfo.Z;
            msi.SetFaceDir(ObjectInfo.FaceDir);

            ObjectInfo.DataChangedByGfx = false;
          }
        } else {
          if (ObjectInfo.DataChangedByGfx) {
            MovementStateInfo msi = m_User.GetMovementStateInfo();
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
      if (null != m_User) {
        if (ObjectInfo.IsGfxAnimation) {
          m_CharacterAnimationInfo.Reset();
          m_IdleState = IdleState.kNotIdle;
          return;
        }

        UpdateMoveAnimation();
        //UpdateBeAttacked();
        UpdateDead();
        UpdateIdle();

        UpdateHilight();
      }
    }
    
    private void UpdateHilight()
    {
      HilightType hi = m_User.GetHilightType();
      if (hi != m_LastHilightType) {
        Hilight(hi);
        m_LastHilightType = hi;
      }

      if (!m_User.IsHilightColorShowed) {
        Vector4 color = m_User.GetHilightColor();
        Hilight(color);
        m_User.IsHilightColorShowed = true;
      }
    }

    private void Init()
    {
      m_resetShootAnimation = false;

      m_User = null;
      m_LastHilightType = HilightType.kNone;
      old_color_ = Vector4.Zero;
    }

    private void Release()
    {

    }

    protected override CharacterInfo GetOwner()
    {
      return m_User;
    }
    
    private UserInfo m_User = null;
    private HilightType m_LastHilightType = HilightType.kNone;
    private Vector4 old_color_;
  }
}
