using System;
using System.Collections.Generic;
using System.Text;

namespace DashFire
{
  [Serializable]
  public class ItemConfig : IData
  {
    public int m_ItemId = 0;
    public string m_ItemName = "";
    public string m_ItemTrueName = "";
    public string m_ItemType = "";
    public bool m_IsLobbyItem = false;
    public int m_Grade = 0;
    public int m_UseLogicId = 0;
    public string[] m_UseResultData = null;
    public int[] m_AddBuffOnEquiping = null;
    public int[] m_AddSkillOnEquiping = null;
    public bool m_ShowInShop = false;
    public string m_Model = "";
    public string m_UiModel = "";
    public int m_MaxStack = 1;
    public int[] m_ConsumeItems = null;
    public int m_ConsumeMoney = 0;
    public int[] m_PropertyRank = null;
    public int m_AddExp = 0;
    public int m_AddMoney = 0;
    public int m_AddBuff = 0;
    public string m_Introduce = "";
    public int m_ItemSkillFirst = 0;
    public int m_ItemSkillSecond = 0;
    public int m_ItemSkillThird = 0;
    public string m_NormalIcon = "";
    public string m_BigIcon = "";
    public AttrDataConfig m_AttrData = new AttrDataConfig();

    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_ItemId = DBCUtil.ExtractNumeric<int>(node, "ItemId", 0, true);
      m_ItemName = DBCUtil.ExtractString(node, "ItemName", "", true);
      m_ItemTrueName = DBCUtil.ExtractString(node, "ItemTrueName", "", true);
      m_ItemType = DBCUtil.ExtractString(node, "ItemType", "", true);
      m_IsLobbyItem = DBCUtil.ExtractBool(node, "IsLobbyItem", false, true);
      m_Grade = DBCUtil.ExtractNumeric<int>(node, "Grade", 0, true);
      m_UseLogicId = DBCUtil.ExtractNumeric<int>(node, "UseLogicId", 0, true);
      List<string> strList = DBCUtil.ExtractStringList(node, "UseResultData", "", true);
      if(strList.Count>0)
        m_UseResultData = strList.ToArray();
      List<int> list = DBCUtil.ExtractNumericList<int>(node, "AddBuffOnEquiping", 0, true);
      if (list.Count>0) {
        m_AddBuffOnEquiping=list.ToArray();
      }
      list = DBCUtil.ExtractNumericList<int>(node, "AddSkillOnEquiping", 0, true);
      if (list.Count > 0) {
        m_AddSkillOnEquiping = list.ToArray();
      }
      m_ShowInShop = (0 != DBCUtil.ExtractNumeric<int>(node, "ShowInShop", 0, true));
      m_Model = DBCUtil.ExtractString(node, "Model", "", true);
      m_UiModel = DBCUtil.ExtractString(node, "UiModel", "", true);
      m_MaxStack = DBCUtil.ExtractNumeric<int>(node, "MaxStack", 1, true);
      list = DBCUtil.ExtractNumericList<int>(node, "ConsumeItems", 0, true);
      if (list.Count > 0) {
        m_ConsumeItems = list.ToArray();
      } else {
        m_ConsumeItems = new int[] { 0, 0, 0 };
      }
      m_ConsumeMoney = DBCUtil.ExtractNumeric<int>(node, "ConsumeMoney", 0, true);
      list = DBCUtil.ExtractNumericList<int>(node, "PropertyRank", 0, true);
      if (list.Count > 0) {
        m_PropertyRank = list.ToArray();
      } else {
        m_PropertyRank = new int[] { 0, 0, 0, 0, 0 };
      }
      m_AddExp = DBCUtil.ExtractNumeric<int>(node, "AddExp", 0, true);
      m_AddMoney = DBCUtil.ExtractNumeric<int>(node, "AddMoney", 0, true);
      m_AddBuff = DBCUtil.ExtractNumeric<int>(node, "AddBuffer", 0, true);
      m_Introduce = DBCUtil.ExtractString(node, "Introduce", "", true);
      m_ItemSkillFirst = DBCUtil.ExtractNumeric<int>(node, "ItemSkillFirst", 0, true);
      m_ItemSkillSecond = DBCUtil.ExtractNumeric<int>(node, "ItemSkillSecond", 0, true);
      m_ItemSkillThird = DBCUtil.ExtractNumeric<int>(node, "ItemSkillThird", 0, true);
      m_NormalIcon = DBCUtil.ExtractString(node, "NormalIcon", "", true);
      m_BigIcon = DBCUtil.ExtractString(node, "BigIcon", "", true);
      m_AttrData.CollectDataFromDBC(node);
      return true;
    }

    public int GetId()
    {
      return m_ItemId;
    }
  }
  public class ItemConfigProvider
  {
    public DataTemplateMgr<ItemConfig> ItemConfigMgr
    {
      get { return m_ItemConfigMgr; }
    }
    public ItemConfig GetDataById(int id)
    {
      return m_ItemConfigMgr.GetDataById(id);
    }
    public int GetDataCount()
    {
      return m_ItemConfigMgr.GetDataCount();
    }
    public void Load(string file, string root)
    {
      m_ItemConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<ItemConfig> m_ItemConfigMgr = new DataTemplateMgr<ItemConfig>();

    public static ItemConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static ItemConfigProvider s_Instance = new ItemConfigProvider();
  }
}
