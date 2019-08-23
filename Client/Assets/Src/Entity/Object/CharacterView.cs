﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public partial class CharacterView
  {
    public int Actor
    {
      get { return m_Actor; }
    }
    public int ObjId
    {
      get { return m_ObjId; }
    }
    public SharedGameObjectInfo ObjectInfo
    {
      get { return m_ObjectInfo; }
    }
    
    public int GunActor
    {
      get { return m_GunActor; }
      set { m_GunActor = value; }
    }
      
    public bool Visible
    {
      get { return m_Visible; }
      set
      {        
        m_Visible = UpdateVisible(value);
      }
    }

    public bool CanAffectPlayerSelf
    {
      get { return m_CanAffectPlayerSelf || WorldSystem.Instance.IsObserver; }
      set { m_CanAffectPlayerSelf = value; }
    }
    
    public void PlayParticle(string particlename, bool enable)
    {
      if (enable && !CanAffectPlayerSelf) return;
    }

    public void PlayParticle(string particlename, float x, float y, float z, float duration)
    {
      if (!CanAffectPlayerSelf) return;
      TemporaryEffectArgs args = new TemporaryEffectArgs(particlename, duration, x, y, z);
      GfxSystem.SendMessage("GfxGameRoot", "AddTemporaryEffect", args);
    }

    public void AddAttachedObject(int objectId, string path) {
      GfxSystem.AttachGameObject(objectId, m_Actor, path);
    }

    public void RemoveAttachedObject(int objectId, bool isDestroy = false) {
      GfxSystem.DetachGameObject(objectId);
      if (isDestroy) {
        GfxSystem.DestroyGameObject(objectId);
      }
    }
    public void PlaySound(string filename, ScriptRuntime.Vector3 pos)
    {
      if (!CanAffectPlayerSelf) return;
    }

    public void SetVisible(bool bVis, string model = null)
    {
      if (m_Actor == 0) {
        return;
      }
    }

    public void SetMaterial(string material_name)
    {
      if (!CanAffectPlayerSelf) return;
    }

    public void ResetMaterial()
    {
    }
    
    protected void CreateActor(int objId, string model, Vector3 pos, float dir, float scale = 1.0f)
    {
      Init();

      m_ObjId = objId;
      m_Actor = GameObjectIdManager.Instance.GenNextId();
      m_ObjectInfo.m_LogicObjectId = objId;
      m_ObjectInfo.X = pos.X;
      m_ObjectInfo.Y = pos.Y;
      m_ObjectInfo.Z = pos.Z;
      m_ObjectInfo.FaceDir = dir;
      m_ObjectInfo.Sx = scale;
      m_ObjectInfo.Sy = scale;
      m_ObjectInfo.Sz = scale;
      GfxSystem.CreateGameObject(m_Actor, model, m_ObjectInfo);
    }

    protected void DestroyActor()
    {
      if (0 != m_GunActor) {
        GfxSystem.DetachGameObject(m_GunActor);
        GfxSystem.DestroyGameObject(m_GunActor);
      }
      GfxSystem.DestroyGameObject(m_Actor);
      Release();
    }

    protected virtual bool UpdateVisible(bool visible)
    {
      GfxSystem.SetGameObjectVisible(m_Actor, visible);
      return visible;
    }

    protected void UpdateMovement()
    {
      CharacterInfo obj = GetOwner();
      if (null != obj && !obj.IsDead() && null != ObjectInfo) {
        MovementStateInfo msi = obj.GetMovementStateInfo();
        ObjectInfo.FaceDir = msi.GetFaceDir();
        if (msi.IsMoving) {          
          ScriptRuntime.Vector3 pos = msi.GetPosition3D();
          ObjectInfo.MoveCos = (float)msi.MoveDirCosAngle;
          ObjectInfo.MoveSin = (float)msi.MoveDirSinAngle;
          ObjectInfo.MoveSpeed = (float)obj.GetActualProperty().MoveSpeed * (float)obj.VelocityCoefficient;
          if (obj is UserInfo) {
            if (msi.TargetPosition.LengthSquared() < Geometry.c_FloatPrecision) {
              ObjectInfo.MoveTargetDistanceSqr = 100.0f;
            } else {
              ObjectInfo.MoveTargetDistanceSqr = msi.CalcDistancSquareToTarget();
            }
          } else {
            ObjectInfo.MoveTargetDistanceSqr = msi.CalcDistancSquareToTarget();
          }

          ObjectInfo.IsLogicMoving = true;
        } else {
          ObjectInfo.IsLogicMoving = false;
        }
      } else {
        ObjectInfo.IsLogicMoving = false;
      }
    }

    protected NpcDeadType GetDeadType()
    {
      return NpcDeadType.Animation;
    }

    protected void Hilight(ScriptRuntime.Vector4 color)
    {
      //Actor.SetColor(color);
    }

    protected void Hilight(HilightType hi)
    {
      switch (hi) {
        case HilightType.kNone:
          //Actor.SetColor(m_NormalColor);
          break;
        case HilightType.kBurnType:
          //Actor.SetColor(m_BurnColor);
          break;
        case HilightType.kFrozenType:
          //Actor.SetColor(m_FrozonColor);
          break;
        case HilightType.kShineType:
          //Actor.SetColor(m_ShineColor);
          break;
        default:
          break;
      }
    }

    protected void UpdateAffectPlayerSelf(ScriptRuntime.Vector3 pos)
    {
      if (null != WorldSystem.Instance.GetPlayerSelf()) {
        ScriptRuntime.Vector3 myselfPos = WorldSystem.Instance.GetPlayerSelf().GetMovementStateInfo().GetPosition3D();
        if (Geometry.DistanceSquare(pos, myselfPos) < c_AffectPlayerSelfDistanceSquare) {
          CanAffectPlayerSelf = true;
        } else {
          CanAffectPlayerSelf = false;
        }
      } else {
        CanAffectPlayerSelf = false;
      }
    }

    private void Init()
    {
      m_NormalColor = new ScriptRuntime.Vector4(1, 1, 1, 1);
      m_BurnColor = new ScriptRuntime.Vector4(0.75f, 0.2f, 0.2f, 1);
      m_FrozonColor = new ScriptRuntime.Vector4(0.2f, 0.2f, 0.75f, 1);
      m_ShineColor = new ScriptRuntime.Vector4(0.2f, 0.75f, 0.2f, 1);
      m_Actor = 0;

      m_CurActionConfig = null;
      m_CurWeaponType = -2;
    }

    private void Release()
    {
      List<string> keyList = effect_map_.Keys.ToList();
      if (keyList != null && keyList.Count > 0)
      {
        foreach (string model in keyList)
        {
          //DetachActor(model);
        }
      }
      CurWeaponList.Clear();
    }

    public List<string> CurWeaponList
    {
      get
      {
        return m_CurWeaponName;
      }
    }

    public string Cylinder
    {
      get { return c_CylinderName; }
    }

    private int m_Actor = 0;
    private int m_GunActor = 0;
    private int m_ObjId = 0;
    private SharedGameObjectInfo m_ObjectInfo = new SharedGameObjectInfo();

    private const string c_CylinderName = "1_Cylinder";
    private const float c_AffectPlayerSelfDistanceSquare = 900;
    private List<string> m_CurWeaponName = new List<string>();

    private bool m_Visible = true;
    private bool m_CanAffectPlayerSelf = true;
  
    private ScriptRuntime.Vector4 m_NormalColor = new ScriptRuntime.Vector4(1,1,1,1);
    private ScriptRuntime.Vector4 m_BurnColor = new ScriptRuntime.Vector4(0.75f, 0.2f, 0.2f, 1);
    private ScriptRuntime.Vector4 m_FrozonColor = new ScriptRuntime.Vector4(0.2f, 0.2f, 0.75f, 1);
    private ScriptRuntime.Vector4 m_ShineColor = new ScriptRuntime.Vector4(0.2f, 0.75f, 0.2f, 1);
    private Dictionary<string, uint> effect_map_ = new Dictionary<string, uint>();
  }
}
