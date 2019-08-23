using System;
using System.Collections.Generic;
using System.Text;

namespace DashFire
{
  public enum WeaponActionSectionNum
  {
    ASN_PREFIRE = 0,
    ASN_FIRE = 1,
    ASN_ENDFIRE = 2,
  }
  public class ActionLogicData : IData
  {
    public class ActionEffectInfo
    {
      public float EffectTime;
      public string SkeletonNodeName;
      public string EffectName;
    }

    public class ActionSection
    {
      public int ActionType;      // ActionType
      public string WapMode;      // anim play mode LOOP, ONCE, CLAMP
      public bool IsUpperBody;
      public float PlaySpeed;     // animation playing speed
      public float PlayTime;      // animation playing time (s)
      public float MoveSpeed;     // move speed （m/s）
      public int MoveTowards;     // move direction(0-forwards, 90-left, 180-backwards, 270-right)
      public float AccelerateY;

      public ActionSection()
      {
        ActionType = -1;
        WapMode = "ONCE";
        IsUpperBody = false;
        PlaySpeed = 1.0f;
        PlayTime = 0.0f;
        MoveSpeed = 0.0f;
        MoveTowards = 0;
        AccelerateY = 0.0f;
      }
    }

    public int ActionId;                  // 动作Id
    public int ActionLogicId;             // 动作对应的逻辑Id
    public int BreakLevel;                // 中断级别，0为不可中断，数字小的可以中断数字大的动作
    public int CallbackSection;           // 回调段数
    public float CallbackPoint;           // 回调的时间点
    public int SectionNumber;             // 有效的动作数量，最大为6
    public List<ActionSection> SectionList = new List<ActionSection>();  // 动作列表

    public int EffectNum;
    public List<ActionEffectInfo> EffectList = new List<ActionEffectInfo>();

    public int ParamNum = 0;
    public List<string> ExtraParams = new List<string>();

    public ActionLogicData()
    {
      SectionList.Clear();
    }

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      ActionId = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      ActionLogicId = DBCUtil.ExtractNumeric<int>(node, "LogicId", 0, true);
      BreakLevel = DBCUtil.ExtractNumeric<int>(node, "BreakLevel", 0, true);
      CallbackSection = DBCUtil.ExtractNumeric<int>(node, "CallbackSection", 0, true);
      CallbackPoint = DBCUtil.ExtractNumeric<float>(node, "CallbackPoint", 0, true);
      SectionNumber = DBCUtil.ExtractNumeric<int>(node, "SectionNum", 0, true);

      EffectNum = DBCUtil.ExtractNumeric<int>(node, "EffectNum", 0, true);
      EffectList.Clear();
      for (int i = 0; i < EffectNum; i++) {
        string keySuffix = i.ToString();
        ActionEffectInfo effect_info = new ActionEffectInfo();
        effect_info.EffectTime = DBCUtil.ExtractNumeric<float>(node, "EffectTime" + keySuffix, 0, false);
        effect_info.SkeletonNodeName = DBCUtil.ExtractString(node, "SkeletonNodeName" + keySuffix, "", false);
        effect_info.EffectName = DBCUtil.ExtractString(node, "EffectName" + keySuffix, "", false);
        EffectList.Insert(i, effect_info);
      }

      for (int i = 0; i < SectionNumber; i++)
      {
        string keySuffix = i.ToString();
        ActionSection actionSection = new ActionSection();
        actionSection.ActionType = DBCUtil.ExtractNumeric<int>(node, "ActionType" + keySuffix, -1, true);
        actionSection.WapMode = DBCUtil.ExtractString(node, "WapMode" + keySuffix, "ONCE", false);
        actionSection.IsUpperBody = DBCUtil.ExtractBool(node, "IsUpperBody" + keySuffix, false, false);
        actionSection.PlaySpeed = DBCUtil.ExtractNumeric<float>(node, "PlaySpeed" + keySuffix, 1.0f, false);
        actionSection.PlayTime = DBCUtil.ExtractNumeric<float>(node, "PlayTime" + keySuffix, 0, false);
        actionSection.MoveSpeed = DBCUtil.ExtractNumeric<float>(node, "MoveSpeed" + keySuffix, 0, false);
        actionSection.MoveTowards = DBCUtil.ExtractNumeric<int>(node, "MoveTowards" + keySuffix, 0, false);
        SectionList.Insert(i, actionSection);
      }

      ParamNum = DBCUtil.ExtractNumeric<int>(node, "ParamNum", 0, false);
      if (ParamNum > 0)
      {
        for (int i = 0; i < ParamNum; ++i)
        {
          string key = "Param" + i.ToString();
          ExtraParams.Insert(i, DBCUtil.ExtractString(node, key, "", false));
        }
      }
      return true;
    }

    /**
     * @brief
     *   get action ID
     *
     * @return 
     */
    public int GetId()
    {
      return ActionId;
    }

    /**
     * @brief
     *   获取动作的移动总距离
     * @return
     *   距离
     */
    public float GetMoveDistance()
    {
      float distance = 0.0f;
      for (int i = 0; i < SectionNumber; i++)
      {
        if (SectionList [i].MoveSpeed > 0)
        {
          distance += SectionList [i].MoveSpeed * SectionList [i].PlayTime;
        }
      }
      return distance;
    }

    /**
     *  @brief
     *    dump all data
     */
    public void Dump()
    {
      LogSystem.Debug("ActionId={0}", ActionId);
      LogSystem.Debug("ActionLogicId={0}", ActionLogicId);
      LogSystem.Debug("BreakLevel={0}", BreakLevel);
      LogSystem.Debug("CallbackPoint={0}", CallbackPoint);
      LogSystem.Debug("SectionNum={0}", SectionNumber);
      for (int i = 0; i < SectionNumber; i++)
      {
        LogSystem.Debug("Section No.{0}:", i + 1);
        LogSystem.Debug("---ActionType={0}", SectionList [i].ActionType);
        LogSystem.Debug("---PlaySpeed={0}", SectionList [i].PlaySpeed);
        LogSystem.Debug("---PlayTime={0}", SectionList [i].PlayTime);
        LogSystem.Debug("---MoveSpeed={0}", SectionList [i].MoveSpeed);
        LogSystem.Debug("---MoveTowards={0}", SectionList [i].MoveTowards);
        LogSystem.Debug("---AccelerateY={0}", SectionList [i].AccelerateY);
      }
      for (int i = 0; i < ParamNum; i++)
      {
        LogSystem.Debug("ExtraParam{0}={1}", i, ExtraParams [i]);
      }
    }
  }
  public class Data_ActionConfig : IData
  {
    public class Data_ActionInfo
    {
      public string m_AnimName;
      public string m_SoundId;
    }

    public int m_ModelId;
    public int m_AnimSetType;
    public Data_ActionInfo m_Default;
    public Dictionary<Animation_Type, List<Data_ActionInfo>> m_ActionContainer;
    public float m_NoGunStdSpeed;
    public float m_ForwardStdSpeed;
    public float m_BackStdSpeed;
    public float m_LeftRightStdSpeed;
    public float m_LeftRightForwardStdSpeed;
    public float m_LeftRightBackStdSpeed;
    public float m_SlowStdSpeed;
    public float m_FastStdSpeed;
    public long m_ReloadStdTimeMs;
    public long m_FireStdTimeMs;
    public string m_BornAnim;
    public string m_ActionPrefix;
    public bool m_IsUpperDepart;

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_ModelId = DBCUtil.ExtractNumeric<int>(node, "ModelId", 0, true);

      m_AnimSetType = DBCUtil.ExtractNumeric<int>(node, "AnimSetType", 0, false);

      m_NoGunStdSpeed = DBCUtil.ExtractNumeric<float>(node, "NoGunStdSpeed", 3.0f, false);
      m_ForwardStdSpeed = DBCUtil.ExtractNumeric<float>(node, "ForwardStdSpeed", 3.0f, true);
      m_BackStdSpeed = DBCUtil.ExtractNumeric<float>(node, "BackStdSpeed", 3.0f, true);
      m_LeftRightStdSpeed = DBCUtil.ExtractNumeric<float>(node, "LeftRightStdSpeed", 3.0f, true);
      m_LeftRightForwardStdSpeed = DBCUtil.ExtractNumeric<float>(node, "LeftRightForwardStdSpeed", 3.0f, true);
      m_LeftRightBackStdSpeed = DBCUtil.ExtractNumeric<float>(node, "LeftRightBackStdSpeed", 3.0f, true);

      m_SlowStdSpeed = DBCUtil.ExtractNumeric<float>(node, "SlowStdSpeed", 3.0f, false);
      m_FastStdSpeed = DBCUtil.ExtractNumeric<float>(node, "FastStdSpeed", 3.0f, false);

      m_IsUpperDepart = DBCUtil.ExtractBool(node, "IsUpperDepart", false, false);
      m_ActionPrefix = DBCUtil.ExtractString(node, "ActionPrefix", "", false);

      m_ActionContainer = new Dictionary<Animation_Type, List<Data_ActionInfo>>();

      m_ActionContainer[Animation_Type.AT_SLEEP] = ExtractAction(node, "Sleep");
      m_ActionContainer[Animation_Type.AT_Stand] = ExtractAction(node, "Stand_0");
      m_ActionContainer[Animation_Type.AT_Hold] = ExtractAction(node, "Hold_0");
      m_ActionContainer[Animation_Type.AT_RIDE] = ExtractAction(node, "Ride_0");
      m_ActionContainer[Animation_Type.AT_Idle0] = ExtractAction(node, "Idle_0");
      m_ActionContainer[Animation_Type.AT_Idle1] = ExtractAction(node, "Idle_1");
      m_ActionContainer[Animation_Type.AT_Idle2] = ExtractAction(node, "Idle_2");

      m_ActionContainer[Animation_Type.AT_SlowMove] = ExtractAction(node, "SlowMove");
      m_ActionContainer[Animation_Type.AT_FastMove] = ExtractAction(node, "FastMove");
      m_ActionContainer[Animation_Type.AT_NoGunRun] = ExtractAction(node, "NoGunRun");
      m_ActionContainer[Animation_Type.AT_NoGunStand] = ExtractAction(node, "NoGunStand");

      m_ActionContainer[Animation_Type.AT_RunForward] = ExtractAction(node, "RunForward_0");
      m_ActionContainer[Animation_Type.AT_RunBackward] = ExtractAction(node, "RunBackward_0");
      m_ActionContainer[Animation_Type.AT_RunLeft] = ExtractAction(node, "RunLeft_0");
      m_ActionContainer[Animation_Type.AT_RunRight] = ExtractAction(node, "RunRight_0");
      m_ActionContainer[Animation_Type.AT_RunForwardLeft] = ExtractAction(node, "RunForwardLeft_0");
      m_ActionContainer[Animation_Type.AT_RunForwardRight] = ExtractAction(node, "RunForwardRight_0");
      m_ActionContainer[Animation_Type.AT_RunBackwardLeft] = ExtractAction(node, "RunBackwardLeft_0");
      m_ActionContainer[Animation_Type.AT_RunBackwardRight] = ExtractAction(node, "RunBackwardRight_0");

      m_ActionContainer[Animation_Type.AT_EquipWeapon] = ExtractAction(node, "EquipWeapon");
      m_ActionContainer[Animation_Type.AT_UnequipWeapon] = ExtractAction(node, "UnequipWeapon");
      m_ActionContainer[Animation_Type.AT_Reload] = ExtractAction(node, "Reload");

      m_ActionContainer[Animation_Type.AT_JumpBegin] = ExtractAction(node, "JumpBegin");
      m_ActionContainer[Animation_Type.AT_Jumping] = ExtractAction(node, "Jumping");
      m_ActionContainer[Animation_Type.AT_JumpEnd] = ExtractAction(node, "JumpEnd");
      m_ActionContainer[Animation_Type.AT_Born] = ExtractAction(node, "Born");

      m_ActionContainer[Animation_Type.AT_Attack] = ExtractAction(node, "Attack_");
      m_ActionContainer[Animation_Type.AT_Hurt] = ExtractAction(node, "Hurt");
      m_ActionContainer[Animation_Type.AT_Dead] = ExtractAction(node, "Dead");
      m_ActionContainer[Animation_Type.AT_PostDead] = ExtractAction(node, "PostDead");
      m_ActionContainer[Animation_Type.AT_HitHigh] = ExtractAction(node, "HitHigh");

      m_ActionContainer[Animation_Type.AT_GetUp1] = ExtractAction(node, "GetUp_0");
      m_ActionContainer[Animation_Type.AT_GetUp2] = ExtractAction(node, "GetUp_1");
      m_ActionContainer[Animation_Type.AT_SkillSection1] = ExtractAction(node, "SkillSection01");
      m_ActionContainer[Animation_Type.AT_SkillSection2] = ExtractAction(node, "SkillSection02");
      m_ActionContainer[Animation_Type.AT_SkillSection3] = ExtractAction(node, "SkillSection03");
      m_ActionContainer[Animation_Type.AT_SkillSection4] = ExtractAction(node, "SkillSection04");
      m_ActionContainer[Animation_Type.AT_SkillSection5] = ExtractAction(node, "SkillSection05");
      m_ActionContainer[Animation_Type.AT_SkillSection6] = ExtractAction(node, "SkillSection06");
      m_ActionContainer[Animation_Type.AT_SkillSection7] = ExtractAction(node, "SkillSection07");
      m_ActionContainer[Animation_Type.AT_SkillSection8] = ExtractAction(node, "SkillSection08");
      m_ActionContainer[Animation_Type.AT_SkillSection9] = ExtractAction(node, "SkillSection09");
      m_ActionContainer[Animation_Type.AT_SkillSection10] = ExtractAction(node, "SkillSection10");
      m_ActionContainer[Animation_Type.AT_SkillSection11] = ExtractAction(node, "SkillSection11");
      m_ActionContainer[Animation_Type.AT_SkillSection12] = ExtractAction(node, "SkillSection12");
      m_ActionContainer[Animation_Type.AT_SkillSection13] = ExtractAction(node, "SkillSection13");
      m_ActionContainer[Animation_Type.AT_SkillSection14] = ExtractAction(node, "SkillSection14");
      m_ActionContainer[Animation_Type.AT_SkillSection15] = ExtractAction(node, "SkillSection15");
      m_ActionContainer[Animation_Type.AT_SkillSection16] = ExtractAction(node, "SkillSection16");
      m_ActionContainer[Animation_Type.AT_SkillSection17] = ExtractAction(node, "SkillSection17");
      m_ActionContainer[Animation_Type.AT_SkillSection18] = ExtractAction(node, "SkillSection18");
      m_ActionContainer[Animation_Type.AT_SkillSection19] = ExtractAction(node, "SkillSection19");
      m_ActionContainer[Animation_Type.AT_SkillSection20] = ExtractAction(node, "SkillSection20");
      return true;
    }

    /**
     * @brief 获取数据ID
     *
     * @return 
     */
    public virtual int GetId()
    {
      return m_ModelId;
    }

    /**
     * @brief 获取动作数量
     *
     * @return 
     */
    public int GetActionCountByType(Animation_Type type)
    {
      if (!m_ActionContainer.ContainsKey(type)) {
        return 0;
      }

      return m_ActionContainer[type].Count;
    }

    /**
     * @brief 获取随即动作
     *
     * @return 
     */
    public Data_ActionInfo GetRandomActionByType(Animation_Type type)
    {
      int count = GetActionCountByType(type);
      if (count > 0) {
        int randIndex = Helper.Random.Next(count);
        return m_ActionContainer[type][randIndex];
      }

      return null;
    }

    public List<Data_ActionInfo> GetAllActionByType(Animation_Type type)
    {
      if (m_ActionContainer.ContainsKey(type)) {
        return m_ActionContainer[type];
      } else {
        return new List<Data_ActionInfo>();
      }
    }
    
    /**
     * @brief 私有提取函数
     *
     * @return 
     */
    private List<Data_ActionInfo> ExtractAction(DBC_Row node, string prefix)
    {
      List<Data_ActionInfo> data = new List<Data_ActionInfo>();

      List<string> childList = DBCUtil.ExtractNodeByPrefix(node, prefix);

      if (childList.Count == 0)
        return data;

      foreach (string child in childList) {
        if (string.IsNullOrEmpty(child)) {
          continue;
        }

        string outModelPath;
        string outSoundId;
        if (!_ParseModelPath(child, out outModelPath, out outSoundId))
        {
          string info = "[Err]:ActionConfigProvider.ExtractAction anim name error:" + child;
          throw new Exception(info);
        }

        Data_ActionInfo action = new Data_ActionInfo();
        action.m_AnimName = m_ActionPrefix + outModelPath;
        action.m_SoundId = outSoundId;
        data.Add(action);
      }

      return data;
    }

    private static bool _ParseModelPath(string path, out string outModelPath, out string outSoundId)
    {
      string[] resut = path.Split(new String[] { "@" }, StringSplitOptions.None);
      if (resut != null)
      {
        outModelPath = (resut.Length > 0) ? resut[0] : "";
        outSoundId = (resut.Length > 1) ? resut[1] : "";
      }
      else
      {
        outModelPath = "";
        outSoundId = "";
      }
      return true;
    }
  }

  public class ActionConfigProvider
  {
    public DataTemplateMgr<Data_ActionConfig> ActionConfigMgr
    {
      get { return m_ActionConfigMgr; }
    }
    public DataTemplateMgr<ActionLogicData> ActionLogicDataMgr
    {
      get { return m_ActionLogicDataMgr; }
    }
    public Data_ActionConfig GetDataById(int id)
    {
      return m_ActionConfigMgr.GetDataById(id);
    }

    public Data_ActionConfig GetCharacterCurActionConfig(List<int> action_list, int anim_set_type)
    {
      for (int i = 0; i < action_list.Count; ++i) {
        Data_ActionConfig action_config = GetDataById(action_list[i]);
        if (action_config != null && (anim_set_type == -1 || action_config.m_AnimSetType == anim_set_type)) {
          return action_config;
        }
      }
      return null;
    }

    public ActionLogicData GetActionDataById(int id)
    {
      return m_ActionLogicDataMgr.GetDataById(id);
    }

    public void Load(string file, string root)
    {
      m_ActionConfigMgr.CollectDataFromDBC(file, root);
    }
    public void LoadActionData(string file, string root)
    {
      m_ActionLogicDataMgr.CollectDataFromDBC(file, root);
    }

    private DataTemplateMgr<Data_ActionConfig> m_ActionConfigMgr = new DataTemplateMgr<Data_ActionConfig>();
    private DataTemplateMgr<ActionLogicData> m_ActionLogicDataMgr = new DataTemplateMgr<ActionLogicData>();

    public static ActionConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static ActionConfigProvider s_Instance = new ActionConfigProvider();
  }
}
