using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class Data_PlayerConfig : IData
  {
    // 基础属性
    public int m_Id;
    public string m_Name;
    public float m_Scale = 1.0f;

    // 战斗属性
    public AttrDataConfig m_AttrData = new AttrDataConfig();

    public float m_ViewRange = 10;
    public long m_ReleaseTime = 1000;
    public int m_HeadUiPos = 0;
    public int m_CostType = 0;
    public float m_ShootBuffLifeTime = 1.0f;
    public long m_NoGunRunEnterTimeMs = 2000;

    public List<int> m_SkillList_1 = new List<int>();
    public List<int> m_SkillList_2 = new List<int>();
    public List<int> m_SkillList_3 = new List<int>();
    public List<int> m_SkillList_4 = new List<int>();
	public List<int> m_SkillList_5 = new List<int>();
    public List<int> m_SkillList_6 = new List<int>();
    public int m_RollSkill = 0;
    public List<int> m_WeaponList = new List<int>();
    public List<int> m_ActionList = new List<int>();

    // 动画
    public string m_Model;
    public string m_DeathModel;
    public string m_ActionFile;
    public string m_AnimPath;

    public float m_Radius;
    public int m_AvoidanceRadius;
    public ScriptRuntime.Vector3 m_GunEndRelativePos;

    public int m_AiLogic;
    //推荐装备
    public int[] m_RecommendEquipment = null;
    //电脑ai用到的数据
    public int[] m_AiEquipment = null;
    public int[] m_AiAttackSkill = null;
    public int[] m_AiMoveSkill = null;
    public int[] m_AiControlSkill = null;
    public int[] m_AiSelfAssitSkill = null;
    public int[] m_AiTeamAssitSkill = null;

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
      m_Name = DBCUtil.ExtractString(node, "Name", "", true);
      m_Scale = DBCUtil.ExtractNumeric<float>(node, "Scale", 1.0f, true);
      m_AiLogic = DBCUtil.ExtractNumeric<int>(node, "AiLogic", 0, false);

      m_AttrData.CollectDataFromDBC(node);
      m_ViewRange = DBCUtil.ExtractNumeric<float>(node, "ViewRange", -1, true);
      m_ReleaseTime = DBCUtil.ExtractNumeric<long>(node, "ReleaseTime", 0, false);
      m_HeadUiPos = DBCUtil.ExtractNumeric<int>(node, "HeadUiPos", 0, false);
      m_CostType = DBCUtil.ExtractNumeric<int>(node, "CostType", 0, false);
      m_ShootBuffLifeTime = DBCUtil.ExtractNumeric<float>(node, "ShootBuffLifeTime", 1.0f, false);
      m_NoGunRunEnterTimeMs = DBCUtil.ExtractNumeric<long>(node, "NoGunRunEnterTimeMs", 2000, false);

      m_SkillList_1 = DBCUtil.ExtractNumericList<int>(node, "Skill_1", 0, false);
      m_SkillList_2 = DBCUtil.ExtractNumericList<int>(node, "Skill_2", 0, false);
      m_SkillList_3 = DBCUtil.ExtractNumericList<int>(node, "Skill_3", 0, false);
      m_SkillList_4 = DBCUtil.ExtractNumericList<int>(node, "Skill_4", 0, false);
	  m_SkillList_5 = DBCUtil.ExtractNumericList<int>(node, "Skill_5", 0, false);
	  m_SkillList_6 = DBCUtil.ExtractNumericList<int>(node, "Skill_6", 0, false);

      m_RollSkill = DBCUtil.ExtractNumeric<int>(node, "RollSkill", 0, false);

      m_WeaponList = DBCUtil.ExtractNumericList<int>(node, "WeaponList", 0, false);
      m_ActionList = DBCUtil.ExtractNumericList<int>(node, "ActionId", 0, false);

      m_Model = DBCUtil.ExtractString(node, "Model", "", false);
      m_DeathModel = DBCUtil.ExtractString(node, "DeathModel", "", false);
      m_ActionFile = DBCUtil.ExtractString(node, "ActionFile", "", false);
      m_AnimPath = DBCUtil.ExtractString(node, "AnimPath", "", false);

      m_Radius = DBCUtil.ExtractNumeric<float>(node, "Radius", 1.0f, false);
      m_AvoidanceRadius = DBCUtil.ExtractNumeric<int>(node, "AvoidanceRadius", 1, false);
      m_GunEndRelativePos = Converter.ConvertVector3D(DBCUtil.ExtractString(node, "GunEndRelativePos", "0.0,0.0,0.0", false));

      List<int> list = DBCUtil.ExtractNumericList<int>(node, "RecommendEquipment", 0, false);
      if (list.Count == 6) {
        m_RecommendEquipment = list.ToArray();
      } else {
        m_RecommendEquipment = new int[] { 0, 0, 0, 0, 0, 0 };
      }

      list = DBCUtil.ExtractNumericList<int>(node, "AiEquipment", 0, false);
      if (list.Count == 6)
        m_AiEquipment = list.ToArray();
      else
        m_AiEquipment = new int[] { 0, 0, 0, 0, 0, 0 };
      list = DBCUtil.ExtractNumericList<int>(node, "AiAttackSkill", 0, false);
      if (list.Count > 0)
        m_AiAttackSkill = list.ToArray();
      list = DBCUtil.ExtractNumericList<int>(node, "AiMoveSkill", 0, false);
      if (list.Count > 0)
        m_AiMoveSkill = list.ToArray();
      list = DBCUtil.ExtractNumericList<int>(node, "AiControlSkill", 0, false);
      if (list.Count > 0)
        m_AiControlSkill = list.ToArray();
      list = DBCUtil.ExtractNumericList<int>(node, "AiSelfAssitSkill", 0, false);
      if (list.Count > 0)
        m_AiSelfAssitSkill = list.ToArray();
      list = DBCUtil.ExtractNumericList<int>(node, "AiTeamAssitSkill", 0, false);
      if (list.Count > 0)
        m_AiTeamAssitSkill = list.ToArray();

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

    public int GetSkillIndex(int skillId)
    {
      int findIndex = findFromList(m_SkillList_1, skillId);
       if (findIndex != -1) return 0;

      findIndex = findFromList(m_SkillList_2, skillId);
      if (findIndex != -1) return 1;

      findIndex = findFromList(m_SkillList_3, skillId);
      if (findIndex != -1) return 2;

      findIndex = findFromList(m_SkillList_4, skillId);
      if (findIndex != -1) return 3;

      return -1;
    }

    private int findFromList(List<int> l, int id)
    {
      return l.FindIndex(
        delegate(int v)
        {
          return v == id;
        }
        );
    }
  }
  
  public class PlayerLevelupExpConfig : IData
  {
    public int m_Level = 0;
    public int m_ConsumeExp = 0;
    public int m_RebornTime = 0;

    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, true);
      m_ConsumeExp = DBCUtil.ExtractNumeric<int>(node, "ConsumeExp", 0, true);
      m_RebornTime = DBCUtil.ExtractNumeric<int>(node, "RebornTime", 0, true);
      return true;
    }

    public int GetId()
    {
      return m_Level;
    }
  }

  public class PlayerConfigProvider
  {
    public DataTemplateMgr<Data_PlayerConfig> PlayerConfigMgr
    {
      get { return m_PlayerConfigMgr; }
    }
    public Data_PlayerConfig GetPlayerConfigById(int id)
    {
      return m_PlayerConfigMgr.GetDataById(id);
    }
    public void LoadPlayerConfig(string file, string root)
    {
      m_PlayerConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<Data_PlayerConfig> m_PlayerConfigMgr = new DataTemplateMgr<Data_PlayerConfig>();
    
    public DataTemplateMgr<LevelupConfig> PlayerLevelupConfigMgr
    {
      get { return m_PlayerLevelupConfigMgr; }
    }
    public LevelupConfig GetPlayerLevelupConfigById(int id)
    {
      return m_PlayerLevelupConfigMgr.GetDataById(id);
    }
    public void LoadPlayerLevelupConfig(string file, string root)
    {
      m_PlayerLevelupConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<LevelupConfig> m_PlayerLevelupConfigMgr = new DataTemplateMgr<LevelupConfig>();

    public DataTemplateMgr<PlayerLevelupExpConfig> PlayerLevelupExpConfigMgr
    {
      get { return m_PlayerLevelupExpConfigMgr; }
    }
    public PlayerLevelupExpConfig GetPlayerLevelupExpConfigById(int id)
    {
      return m_PlayerLevelupExpConfigMgr.GetDataById(id);
    }
    public void LoadPlayerLevelupExpConfig(string file, string root)
    {
      m_PlayerLevelupExpConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<PlayerLevelupExpConfig> m_PlayerLevelupExpConfigMgr = new DataTemplateMgr<PlayerLevelupExpConfig>();

    public static PlayerConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static PlayerConfigProvider s_Instance = new PlayerConfigProvider();
  }
}
