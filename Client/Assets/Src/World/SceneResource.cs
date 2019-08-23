using System;
using System.Collections.Generic;
using DashFire.Network;
using ScriptRuntime;

namespace DashFire
{
  public class SceneResource
  {
    public int ResId
    {
      get
      {
        return m_SceneResId;
      }
    }
    public MapDataProvider StaticData
    {
      get
      {
        return m_SceneStaticData;
      }
    }
    public Data_SceneConfig SceneConfig
    {
      get
      {
        return m_SceneConfig;
      }
    }
    public bool IsPve
    {
      get
      {
        if (GlobalVariables.Instance.IsFullClient)
          return true;
        return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVE;
      }
    }
    public bool IsPvp
    {
      get
      {
        if (GlobalVariables.Instance.IsFullClient)
          return false;
        return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_PVP;
      }
    }
    public bool IsMultiPve
    {
      get
      {
        if (GlobalVariables.Instance.IsFullClient)
          return false;
        return m_SceneConfig.m_Type == (int)SceneTypeEnum.TYPE_MULTI_PVE;
      }
    }
    public bool IsWaitSceneLoad
    {
      get { return m_IsWaitSceneLoad; }
    }
    public bool IsSuccessEnter
    {
      get { return m_IsSuccessEnter; }
    }
    public void Init(int resId)
    {
      m_SceneResId = resId;
      LoadSceneData(resId);
      WorldSystem.Instance.SceneContext.SceneResId = resId;
      m_IsWaitSceneLoad = true;

      Data_Unit unit = m_SceneStaticData.ExtractData(DataMap_Type.DT_Unit, GlobalVariables.GetUnitIdByCampId(NetworkSystem.Instance.CampId)) as Data_Unit;
      if (null != unit) {
        m_CameraLookAtHeight = unit.m_Pos.Y;
      }

      GfxSystem.GfxLog("SceneResource.Init {0}", resId);
    }
    public void Release()
    {
      GfxSystem.GfxLog("SceneResource.Release");
    }
    public void NotifyUserEnter()
    {
      m_IsSuccessEnter = true;
      GfxSystem.GfxLog("SceneResource.NotifyUserEnter");
    }
    public void UpdateObserverCamera(float x, float y)
    {
      /*const float c_drag_velocity = 0.5f;
      if (m_IsSuccessEnter) {
        Vector3 pos = GfxSystemExt.GfxSystem.Instance.MainCamera.Position;
        if (x <= 0.2f) {
          pos.X -= c_drag_velocity; 
        } else if (x >= 0.8f) {
          pos.X += c_drag_velocity;
        }
        if (y <= 0.2f) {
          pos.Z -= c_drag_velocity;
        } else if (y >= 0.8f) {
          pos.Z += c_drag_velocity;
        }
        if (pos.X < 0)
          pos.X = 0;
        if (pos.Z < 0)
          pos.Z = 0;
        float xsize = StaticData.m_MapInfo.m_MapSize.X;
        float ysize = StaticData.m_MapInfo.m_MapSize.Y;
        if (pos.X > xsize)
          pos.X = xsize;
        if (pos.Z > ysize)
          pos.Z = ysize;
        pos.Y = m_CameraLookAtHeight;
        GfxSystem.SendMessage("GfxGameRoot", "CameraLookat", new float[]{ pos.X, pos.Y, pos.Z });
      }*/
    }
    private void OnLoadFinish()
    {
      if (WorldSystem.Instance.IsObserver) {
        UserInfo myself = WorldSystem.Instance.CreatePlayerSelf(0x0ffffffe, 1);
        if (null != myself) {//观战客户端创建一个虚拟玩家（不关联view，血量不要为0，主要目的是为了适应客户端代码里对主角的判断）
          myself.SetLevel(16);
          myself.SetHp(Operate_Type.OT_Absolute, 999999);
        }
        NotifyUserEnter();
      }

      if (WorldSystem.Instance.IsObserver) {
        DashFireMessage.Msg_CR_Observer build = new DashFireMessage.Msg_CR_Observer();
        NetworkSystem.Instance.SendMessage(build);
        LogSystem.Debug("send Msg_CR_Observer to roomserver");
      } else if(WorldSystem.Instance.IsPveScene()) {
        //单机游戏逻辑启动
        WorldSystem.Instance.StartGame();
        NotifyUserEnter();
      } else {
        DashFireMessage.Msg_CRC_Create build = new DashFireMessage.Msg_CRC_Create();
        NetworkSystem.Instance.SendMessage(build);
        LogSystem.Debug("send Msg_CRC_Create to roomserver");
      }
      GfxSystem.GfxLog("SceneResource.OnLoadFinish");
      m_IsWaitSceneLoad = false;

      GfxSystem.PublishGfxEvent("ge_loading_finish", "ui");
    }
    private bool LoadSceneData(int sceneResId)
    {
      bool result = true;
      m_SceneResId = sceneResId;
      // 加载场景配置数据
      m_SceneConfig = SceneConfigProvider.Instance.GetSceneConfigById(m_SceneResId);
      if (null == m_SceneConfig)
        GfxSystem.GfxLog("LoadSceneData {0} failed!", sceneResId);
      // 加载本场景xml数据
      m_SceneStaticData = SceneConfigProvider.Instance.GetMapDataBySceneResId(m_SceneResId);

      GfxSystem.LoadScene(m_SceneConfig.m_ClientSceneFile, OnLoadFinish);
      return result;
    }

    private int m_SceneResId;
    private MapDataProvider m_SceneStaticData;
    private Data_SceneConfig m_SceneConfig;

    private bool m_IsWaitSceneLoad = true;
    private bool m_IsSuccessEnter = false;
    private float m_CameraLookAtHeight = 0;
  }
}
