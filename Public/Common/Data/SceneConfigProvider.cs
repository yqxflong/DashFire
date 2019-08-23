using System;
using System.Collections.Generic;
using ScriptRuntime;
//using System.Diagnostics;

namespace DashFire
{
  public enum SceneTypeEnum : int
  {
    TYPE_PVE = 0,
    TYPE_PVP,
    TYPE_MULTI_PVE,
    TYPE_NUM,
  }
  /**
   * @brief 场景配置数据
   * @remarks 这里应该只放场景逻辑相关配表数据，不要把阻挡、地形等二进制数据放在这里（因为这些数据通常只有空间或渲染系统要用，不需要共用）
   */
  public class Data_SceneConfig : IData
  {

    /**
     * @brief 地图id
     */
    public int m_Id;

    public int m_Type;

    public int m_IntroTime;

    public string m_ScenePath;
    
    /**
     * @brief 单元数据文件
     */
    public string m_UnitFile;
    
    /**
     * @brief 碰撞数据文件
     */
    public string m_ObstacleFile;

    public string m_SceneLogicFile;

    public string m_BlockInfoFile;

    public string m_ClientSceneFile;

    public float m_TiledDataScale;

    public Vector3[] m_ReachableSet;

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 0, true);
      m_IntroTime = DBCUtil.ExtractNumeric<int>(node, "IntroTime", 0, true);
      m_ScenePath = DBCUtil.ExtractString(node, "ScenePath", "", true);
      m_UnitFile = DBCUtil.ExtractString(node, "UnitFile", "", true);
      m_ObstacleFile = DBCUtil.ExtractString(node, "ObstacleFile", "", true);
      m_SceneLogicFile = DBCUtil.ExtractString(node, "SceneLogicFile", "", true);
      m_BlockInfoFile = DBCUtil.ExtractString(node, "BlockInfoFile", "", true);
      m_ClientSceneFile = DBCUtil.ExtractString(node, "ClientSceneFile", "", true);
      m_TiledDataScale = DBCUtil.ExtractNumeric<float>(node, "TiledDataScale", 0, true);

      List<float> coords = DBCUtil.ExtractNumericList<float>(node, "ReachableSet", 0, false);
      if (coords.Count > 0) {
        m_ReachableSet = new Vector3[coords.Count / 2];
        for (int i = 0; i < coords.Count - 1; i += 2) {
          m_ReachableSet[i / 2] = new Vector3(coords[i], 0, coords[i + 1]);
        }
      } else {
        m_ReachableSet = null;
      }
      return true;
    }

    /**
     * @brief 获取数据ID
     *
     * @return 
     */
    public int GetId()
    {
      return m_Id;
    }
  }

  public class SceneConfigProvider
  {
    public DataTemplateMgr<Data_SceneConfig> SceneConfigMgr
    {
      get { return m_SceneConfigMgr; }
    }
    public MyDictionary<int, MapDataProvider> MapDataProviders
    {
      get { return m_MapDataProviders; }
    }
    public Data_SceneConfig GetSceneConfigById(int id)
    {
      return m_SceneConfigMgr.GetDataById(id);
    }
    public MapDataProvider GetMapDataBySceneResId(int resId)
    {
      MapDataProvider data = null;
      if (m_MapDataProviders.ContainsKey(resId)) {
        data = m_MapDataProviders[resId];
      }
      return data;
    }
    public void Load(string file, string root)
    {
      m_SceneConfigMgr.CollectDataFromDBC(file, root);
    }
    public MapDataProvider LoadSceneConfig(int id,string rootPath)
    {
      MapDataProvider provider = null;
      Data_SceneConfig sceneCfg = m_SceneConfigMgr.GetDataById(id);
      if (null != sceneCfg) {
        provider = new MapDataProvider();
        provider.CollectData(DataMap_Type.DT_Unit, rootPath + sceneCfg.m_UnitFile, "UnitInfo");
        provider.CollectData(DataMap_Type.DT_SceneLogic, rootPath + sceneCfg.m_SceneLogicFile, "SceneLogic");
      }
      return provider;
    }
    public void LoadAllSceneConfig(string rootPath)
    {
      m_MapDataProviders.Clear();
      foreach (int id in m_SceneConfigMgr.GetData().Keys) {
        MapDataProvider data = LoadSceneConfig(id,rootPath);
        if (null != data) {
          m_MapDataProviders.Add(id, data);
        }
      }
    }
    private DataTemplateMgr<Data_SceneConfig> m_SceneConfigMgr = new DataTemplateMgr<Data_SceneConfig>();
    private MyDictionary<int, MapDataProvider> m_MapDataProviders = new MyDictionary<int, MapDataProvider>();

    public static SceneConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static SceneConfigProvider s_Instance = new SceneConfigProvider();
  }
}
