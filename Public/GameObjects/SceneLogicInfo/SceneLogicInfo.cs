using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public enum SceneLogicId
  {
    GRENADE = 11001,
    AIRATTACK = 11002,
    BOSSMISSILE = 11003,
    LIGHTNINGSHOT = 11004,
    ENERGYSHOT = 11005,
    PIERCESHOT = 11006,
    SOURBULLET = 11007,
    SOURPATH = 11008,
    FLYBOMB = 11009,
    SOURSHOOT = 11010,
    FIREBOMB = 11011,
    HUOXIYISHIELD = 11012,
    ROCKET = 11013,
    BOSSTRACESHOOT = 11014,
    SkillShootBarrage = 11015,
    SUMMON = 11016,
    SimulateBarrage = 11017,
    ConditionBarrage = 11018,
    UCABossMissle = 11019,
    UCABossLandmine = 11020,
    IceEjector = 11021,

    MEDICINECAB = 12001,
    LASER = 12002,
    ICEHOLE = 12003,
    EXPLOSIVE_OBJECT = 12004,
    BLINDAGE = 12005,

    REVIVE_POINT = 13001,
    PVP_BARRACKS = 13002,
    WORM_LAIR = 13003,
    SHIPDECK_FIGHT = 13004,
    SHIPDECK_BOSS = 13005,
    PVP_HEADQUARTERS = 13006,
    PVE_MISSIONCHECK = 13007,
    PVE_MISSIONFAILED = 13008,

    SEARCH_LIGHT = 14001,

    GUNNER_ATTACK = 15001,
    GUNNER_ARCATTACK = 15002,
  }

  public class SceneLogicInfo
  {
    public int DataId
    {
      get { return m_DataId; }
      set { m_DataId = value; }
    }
    public int GetId()
    {
      return m_Id;
    }
    public int ConfigId
    {
      get
      {
        int id = 0;
        if (null != m_SceneLogicConfig) {
          id = m_SceneLogicConfig.GetId();
        }
        return id;
      }
    }
    public int LogicId
    {
      get
      {
        int id = 0;
        if (null != m_SceneLogicConfig) {
          id = m_SceneLogicConfig.m_LogicId;
        }
        return id;
      }
    }
    public SceneLogicConfig SceneLogicConfig
    {
      get { return m_SceneLogicConfig; }
      set { m_SceneLogicConfig = value; }
    }
    public bool IsLogicFinished
    {
      get { return m_IsLogicFinished; }
      set { m_IsLogicFinished = value; }
    }
    public long CreateTime
    {
      get { return m_CreateTime; }
      set { m_CreateTime = value; }
    }
    public SceneContextInfo SceneContext
    {
      get { return m_SceneContext; }
      set { m_SceneContext = value; }
    }
    public DashFireSpatial.ISpatialSystem SpatialSystem
    {
      get
      {
        DashFireSpatial.ISpatialSystem sys = null;
        if (null != m_SceneContext) {
          sys = m_SceneContext.SpatialSystem;
        }
        return sys;
      }
    }
    public SceneLogicInfoManager SceneLogicInfoManager
    {
      get
      {
        SceneLogicInfoManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.SceneLogicInfoManager;
        }
        return mgr;
      }
    }
    public NpcManager NpcManager
    {
      get
      {
        NpcManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.NpcManager;
        }
        return mgr;
      }
    }
    public UserManager UserManager
    {
      get
      {
        UserManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.UserManager;
        }
        return mgr;
      }
    }
    public BlackBoard BlackBoard
    {
      get
      {
        BlackBoard blackBoard = null;
        if (null != m_SceneContext) {
          blackBoard = m_SceneContext.BlackBoard;
        }
        return blackBoard;
      }
    }
    public long Time
    {
      get { return m_Time; }
      set { m_Time = value; }
    }
    public TypedDataCollection LogicDatas
    {
      get { return m_LogicDatas; }
    }
    public SceneLogicInfo(int id)
    {
      m_Id = id;
      m_IsLogicFinished = false;
      m_CreateTime = TimeUtility.GetServerMilliseconds();
    }
    public void InitId(int id)
    {
      m_Id = id;
    }
    public void Reset()
    {
      m_IsLogicFinished = false;
      m_LogicDatas.Clear();
    }
    private int m_Id = 0;
    private int m_DataId = 0;
    private SceneLogicConfig m_SceneLogicConfig = null;
    private bool m_IsLogicFinished = false;
    private long m_CreateTime;
    private SceneContextInfo m_SceneContext = null;
    private long m_Time = 0;//由于场景逻辑主要在Tick里工作，通常需要限制工作的频率，这一数据用于此目的（由于LogicDatas的读取比较费，所以抽出来放公共里）
    private TypedDataCollection m_LogicDatas = new TypedDataCollection();
  }
}
