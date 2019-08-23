using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class EquipmentConfig : IData
  {
    public int m_EquipmentId = 0;
    public string m_EquipmentName = "";
    public string m_UiModel = "";
    public AttrDataConfig m_AttrData = new AttrDataConfig();

    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_EquipmentId = DBCUtil.ExtractNumeric<int>(node, "EquipmentId", 0, true);
      m_EquipmentName = DBCUtil.ExtractString(node, "EquipmentName", "", true);
      m_UiModel = DBCUtil.ExtractString(node, "UiModel", "", false);
      m_AttrData.CollectDataFromDBC(node);
      return true;
    }

    public int GetId()
    {
      return m_EquipmentId;
    }
  }
  public class EquipmentLevelupConfig : IData
  {
    public int m_LevelupId = 0;
    public int m_EquipmentId = 0;
    public string m_EquipmentName = "";
    public int m_Type = 0;
    public int m_Level = 0;
    public string m_UiModel = "";
    public int m_ConsumeItem = 0;
    public int m_ConsumeMoney = 0;
    public int m_ConsumeYuanbao = 0;
    public AttrDataConfig m_AttrData = new AttrDataConfig();

    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_LevelupId = DBCUtil.ExtractNumeric<int>(node, "LevelupId", 0, true);
      m_EquipmentId = DBCUtil.ExtractNumeric<int>(node, "EquipmentId", 0, true);
      m_EquipmentName = DBCUtil.ExtractString(node, "EquipmentName", "", true);
      m_Type = DBCUtil.ExtractNumeric<int>(node, "Type", 0, true);
      m_Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, true);
      m_UiModel = DBCUtil.ExtractString(node, "UiModel", "", false);
      m_ConsumeItem = DBCUtil.ExtractNumeric<int>(node, "ConsumeItem", 0, false);
      m_ConsumeMoney = DBCUtil.ExtractNumeric<int>(node, "ConsumeMoney", 0, false);
      m_ConsumeYuanbao = DBCUtil.ExtractNumeric<int>(node, "ConsumeYuanbao", 0, false);
      m_AttrData.CollectDataFromDBC(node);
      return true;
    }

    public int GetId()
    {
      return m_LevelupId;
    }
  }
  public class EquipmentConfigProvider
  {
    public DataTemplateMgr<EquipmentConfig> EquipmentConfigMgr
    {
      get { return m_EquipmentConfigMgr; }
    }
    public EquipmentConfig GetEquipmentConfigById(int id)
    {
      return m_EquipmentConfigMgr.GetDataById(id);
    }
    public void LoadEquipmentConfig(string file, string root)
    {
      m_EquipmentConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<EquipmentConfig> m_EquipmentConfigMgr = new DataTemplateMgr<EquipmentConfig>();

    public DataTemplateMgr<EquipmentLevelupConfig> EquipmentLevelupConfigMgr
    {
      get { return m_EquipmentLevelupConfigMgr; }
    }
    public EquipmentLevelupConfig GetEquipmentLevelupConfigById(int id)
    {
      return m_EquipmentLevelupConfigMgr.GetDataById(id);
    }
    public EquipmentLevelupConfig GetEquipmentLevelupConfigByTypeLevel(int type, int level)
    {
      EquipmentLevelupConfig cfg = null;
      if (m_EquipmentLevelupConfigByTypeLevel.ContainsKey(type)) {
        MyDictionary<int, EquipmentLevelupConfig> cfgDict = m_EquipmentLevelupConfigByTypeLevel[type];
        if (cfgDict.ContainsKey(level)) {
          cfg = cfgDict[level];
        }
      }
      return cfg;
    }
    public void LoadEquipmentLevelupConfig(string file, string root)
    {
      m_EquipmentLevelupConfigMgr.CollectDataFromDBC(file, root);
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfigMgr.GetData().Values) {
        if (m_EquipmentLevelupConfigByTypeLevel.ContainsKey(cfg.m_Type)) {
          MyDictionary<int, EquipmentLevelupConfig> cfgDict = m_EquipmentLevelupConfigByTypeLevel[cfg.m_Type];
          if (cfgDict.ContainsKey(cfg.m_Level)) {
            cfgDict[cfg.m_Level] = cfg;
          } else {
            cfgDict.Add(cfg.m_Level, cfg);
          }
        } else {
          MyDictionary<int, EquipmentLevelupConfig> cfgDict = new MyDictionary<int, EquipmentLevelupConfig>();
          cfgDict.Add(cfg.m_Level, cfg);
          m_EquipmentLevelupConfigByTypeLevel.Add(cfg.m_Type, cfgDict);
        }
      }
    }

    private DataTemplateMgr<EquipmentLevelupConfig> m_EquipmentLevelupConfigMgr = new DataTemplateMgr<EquipmentLevelupConfig>();
    private MyDictionary<int, MyDictionary<int, EquipmentLevelupConfig>> m_EquipmentLevelupConfigByTypeLevel = new MyDictionary<int, MyDictionary<int, EquipmentLevelupConfig>>();

    public static EquipmentConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static EquipmentConfigProvider s_Instance = new EquipmentConfigProvider();
  }
}
