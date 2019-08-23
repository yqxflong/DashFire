using System;
using System.Collections.Generic;
using System.Text;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public class Data_NpcConfig : IData
  {
    // 基础属性
    public int m_Id;
    public string m_Name;
    public int m_NpcType;
    public int m_Level;
    public float m_Scale = 1.0f;
    public int m_DropCount = 0;
    public int m_DropExp = 0;
    public int m_DropMoney = 0;
    public int[] m_DropProbabilities = null;
    public int[] m_DropNpcs = null;
    public int[] m_InteractSourceActions = null;
    public int[] m_InteractTargetActions = null;
    public int m_InteractionLogic;
    public string[] m_InteractResultData;
    public bool m_CanMove;
    public bool m_CanRotate;
    public bool m_IsRange;
    public bool m_IsShine;

    // 战斗属性
    public AttrDataConfig m_AttrData = new AttrDataConfig();

    public float m_ViewRange = 10;
    public float m_GohomeRange = 10;
    public long m_ReleaseTime = 1000;
    public int m_HeadUiPos = 0;
    // 技能列表
    public List<int> m_SkillList = new List<int>();
    public List<int> m_ActionList = new List<int>();

    // 动画
    public string m_Model;
    public string m_DeathModel;
    public string m_DeathEffect;
    public string m_DeathSound;
    public int m_DeadType;

    public int m_BornTimeMs;
    public string m_BornEffect;
    public bool m_IsAttachControler;
    public string m_AttachNodeName;

    public ScriptRuntime.Vector3 m_GunEndRelativePos;

    public bool m_IsHurtComa;
    public bool m_isBlaze;
    public int m_Barrage;
    public int m_AvoidanceRadius;
    public List<int> m_WeaponList = new List<int>();
    public Shape m_Shape = null;

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
      m_NpcType = DBCUtil.ExtractNumeric<int>(node, "NpcType", 0, true);
      m_Level = DBCUtil.ExtractNumeric<int>(node, "Level", 0, true);
      m_Scale = DBCUtil.ExtractNumeric<float>(node, "Scale", 1.0f, true);

      m_DropCount = DBCUtil.ExtractNumeric<int>(node, "DropCount", 0, false);
      m_DropExp = DBCUtil.ExtractNumeric<int>(node, "DropExp", 0, false);
      m_DropMoney = DBCUtil.ExtractNumeric<int>(node, "DropMoney", 0, false);

      List<int> list = DBCUtil.ExtractNumericList<int>(node, "DropProbabilities", 0, false);
      if (list.Count > 0) {
        m_DropProbabilities = list.ToArray();
      }

      list = DBCUtil.ExtractNumericList<int>(node, "DropNpcs", 0, false);
      if (list.Count > 0) {
        m_DropNpcs = list.ToArray();
      }

      list = DBCUtil.ExtractNumericList<int>(node, "InteractSourceActions", 0, false);
      if (list.Count > 0) {
        m_InteractSourceActions = list.ToArray();
      } else {
        m_InteractSourceActions = new int[] { 0, 0 };
      }

      list = DBCUtil.ExtractNumericList<int>(node, "InteractTargetActions", 0, false);
      if (list.Count > 0) {
        m_InteractTargetActions = list.ToArray();
      } else {
        m_InteractTargetActions = new int[] { 0, 0 };
      }

      m_InteractionLogic = DBCUtil.ExtractNumeric<int>(node, "InteractionLogic", 0, false);
      List<string> strList = DBCUtil.ExtractStringList(node, "InteractResultData", "", false);
      if (strList.Count > 0)
        m_InteractResultData = strList.ToArray();

      m_AttrData.CollectDataFromDBC(node);
      m_ViewRange = DBCUtil.ExtractNumeric<float>(node, "ViewRange", -1, true);
      m_GohomeRange = DBCUtil.ExtractNumeric<float>(node, "GohomeRange", -1, true);
      m_ReleaseTime = DBCUtil.ExtractNumeric<long>(node, "ReleaseTime", 0, true);
      m_HeadUiPos = DBCUtil.ExtractNumeric<int>(node, "HeadUiPos", 0, true);

      m_SkillList = DBCUtil.ExtractNumericList<int>(node, "SkillList", 0, false);
      m_ActionList = DBCUtil.ExtractNumericList<int>(node, "ActionId", 0, false);

      m_Model = DBCUtil.ExtractString(node, "Model", "", false);
      m_DeathModel = DBCUtil.ExtractString(node, "DeathModel", "", false);
      m_DeathEffect = DBCUtil.ExtractString(node, "DeathEffect", "", false);
      m_DeathSound = DBCUtil.ExtractString(node, "DeathSound", "", false);
      m_DeadType = DBCUtil.ExtractNumeric<int>(node, "DeadType", 0, false);
      m_Barrage = DBCUtil.ExtractNumeric<int>(node, "Barrage", 0, false);

      m_AvoidanceRadius = DBCUtil.ExtractNumeric<int>(node, "AvoidanceRadius", 1, false);
      m_CanMove = DBCUtil.ExtractBool(node, "CanMove", false, false);
      m_CanRotate = DBCUtil.ExtractBool(node, "CanRotate", true, false);
      m_IsRange = DBCUtil.ExtractBool(node, "IsRange", false, false);
      m_IsShine = DBCUtil.ExtractBool(node, "IsShine", false, false);
      m_isBlaze = DBCUtil.ExtractBool(node, "IsBlaze", false, false);
      m_IsHurtComa = DBCUtil.ExtractBool(node, "IsHurtComa", false, false);

      m_BornTimeMs = DBCUtil.ExtractNumeric<int>(node, "BornTimeMs", 0, false);
      m_BornEffect = DBCUtil.ExtractString(node, "BornEffect", "", false);
      m_IsAttachControler = DBCUtil.ExtractBool(node, "IsAttachControler", false, false);
      m_AttachNodeName = DBCUtil.ExtractString(node, "AttachNodeName", "", false);

      m_GunEndRelativePos = Converter.ConvertVector3D(DBCUtil.ExtractString(node, "GunEndRelativePos", "0.0,0.0,0.0", false));

      m_WeaponList = DBCUtil.ExtractNumericList<int>(node, "WeaponId", 0, false);

      string shapeType = DBCUtil.ExtractString(node, "ShapeType", "", true);
      int shapeParamNum = DBCUtil.ExtractNumeric<int>(node, "ShapeParamNum", 0, true);
      if (shapeParamNum > 0) {
        string[] shapeParams = new string[shapeParamNum];
        for (int i = 0; i < shapeParamNum; ++i) {
          shapeParams[i] = DBCUtil.ExtractString(node, "ShapeParam" + i, "", false);
        }

        if (0 == string.Compare("Circle", shapeType, true)) {
          m_Shape = new Circle(new Vector3(0, 0, 0), float.Parse(shapeParams[0]));
        } else if (0 == string.Compare("Line", shapeType, true)) {
          Vector3 start=Converter.ConvertVector3D(shapeParams[0]);
          Vector3 end = Converter.ConvertVector3D(shapeParams[1]);
          m_Shape = new Line(start, end);
        } else if (0 == string.Compare("Rect", shapeType, true)) {
          float width=float.Parse(shapeParams[0]);
          float height = float.Parse(shapeParams[1]);
          m_Shape = new Rect(width, height);
        } else if (0 == string.Compare("Polygon", shapeType, true)) {
          Polygon polygon = new Polygon();
          foreach (string s in shapeParams) {
            Vector3 pt = Converter.ConvertVector3D(s);
            polygon.AddVertex(pt);
          }
          m_Shape = polygon;
        }
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
  
  public class NpcConfigProvider
  {
    public DataTemplateMgr<Data_NpcConfig> NpcConfigMgr
    {
      get { return m_NpcConfigMgr; }
    }
    public Data_NpcConfig GetNpcConfigById(int id)
    {
      return m_NpcConfigMgr.GetDataById(id);
    }
    public void LoadNpcConfig(string file, string root)
    {
      m_NpcConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<Data_NpcConfig> m_NpcConfigMgr = new DataTemplateMgr<Data_NpcConfig>();

    public DataTemplateMgr<LevelupConfig> NpcLevelupConfigMgr
    {
      get { return m_NpcLevelupConfigMgr; }
    }
    public LevelupConfig GetNpcLevelupConfigById(int id)
    {
      return m_NpcLevelupConfigMgr.GetDataById(id);
    }
    public void LoadNpcLevelupConfig(string file, string root)
    {
      m_NpcLevelupConfigMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<LevelupConfig> m_NpcLevelupConfigMgr = new DataTemplateMgr<LevelupConfig>();

    public static NpcConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static NpcConfigProvider s_Instance = new NpcConfigProvider();
  }
}
