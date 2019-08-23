﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class SpaceInfoView
  {
    public int Id
    {
      get
      {
        return m_ObjId;
      }
    }
    public bool NeedDestroy    
    {
      get { return m_NeedDestroy; }
      set { m_NeedDestroy = value; }
    }
    public SharedGameObjectInfo ObjectInfo
    {
      get { return m_ObjectInfo; }
    }

    public void Create(int objId,bool isPlayer)
    {
      m_Actor = GameObjectIdManager.Instance.GenNextId();
      if(isPlayer)
        GfxSystem.CreateGameObject(m_Actor, "Arrow_Red", m_ObjectInfo);
      else
        GfxSystem.CreateGameObject(m_Actor, "Arrow_Blue", m_ObjectInfo);
    }

    public void Destroy()
    {
      GfxSystem.DestroyGameObject(m_Actor);
    }

    public void Update(float x, float y, float z, float dir)
    {
      m_ObjectInfo.X = x;
      m_ObjectInfo.Y = 0;
      m_ObjectInfo.Z = z;
      m_ObjectInfo.FaceDir = dir;
      m_ObjectInfo.DataChangedByLogic = true;
    }

    private int m_Actor = 0;
    private int m_ObjId = 0;
    private bool m_NeedDestroy = false;
    private SharedGameObjectInfo m_ObjectInfo = new SharedGameObjectInfo();
  }
}
