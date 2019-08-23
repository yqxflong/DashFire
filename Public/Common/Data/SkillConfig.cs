using System;
using System.Collections.Generic;
using System.Text;

namespace DashFire
{
  /**
 * @brief
 *   skill data
 */
  public class SkillLogicData : IData
  {
    public enum Passivity
    {
      NOT_PASSIVE_SKILL = 0,  // 主动
      PASSIVE_SKILL = 1,      // 被动
    }

    public enum TargSelectType
    {
      SELECT_TYPE_SELECT = 1,    // 选择目标
      SELECT_TYPE_POSITION = 2,  // 选择地点
      SELECT_TYPE_NONE = 3,      // 无需选择
      SELECT_TYPE_MOVE_DIR = 4,  // 选择移动方向
    }

    public enum TargType
    {
      ANY = 0,      // 任意目标
      PARTNER = 1,  // 友军
      ENEMY = 2,    // 敌军
      SELF = 3,     // 自己
    }

    public enum RangeType
    {
      kNone = 0,     // 没有施法指示
      kSkillRange,   // 无方向技能范围指示
      kAOERange,     // 无方向AOE技能范围指示
      kDirectionAOERange, // 带方向的范围指示
    }
    
    public enum AreaType
    {
      None = 0,      // 无
      CIRCLE  = 1,   // 圆，以目标点为圆心
      RECT    = 2,   // 方
      SELFCIRCLE = 3, // 圆，以自己为圆心
    }

    // basic attributes
    public int SkillId;                           // 技能ID
    public string SkillDataFile;                  // 技能逻辑描述文件
    public string SkillIcon;                      // 技能图标
    public int ActivateLevel;                     // 激活的最小等级
    public string SkillDescription;               // 技能名称
    public string SkillTrueDescription;           // 技能描述
    public int SkillLevel;                        // 技能等级
    public float SkillCoefficient;                // 技能系数
    public Passivity SkillPassivity;              // 0-主动，1-被动
    public TargSelectType TargetSelectType;       // 1-选择目标，2-选择地点，3-无需选择
    public TargType TargetType;                   // 0-对任意目标有效，1-对友军使用，2-对敌人使用，3-对自己使用

    public float CoolDownTime;                    // 冷却时间，单位：秒
    public int CostHp;                            // 释放技能需要消耗的HP
    public int CostEnergy;                        // 释放技能需要消耗的Energy
    public int CostEnergyCore;                    // 释放技能需要消耗的能量豆
    public int CostItemId;                        // 释放技能需要消耗的物品id

    public RangeType RangeIndicatorType;          // 施法指示类型
    public string SkillRangeAsset;                // 技能指示资源
    public string AOERangeAsset;                  // AOE范围指示资源
    public float SkillAssetStdRange;              // 技能资源标准大小
    public float AOEAssetStdRange;                // AOE范围指示资源标准大小
    public float SkillRange;                      // 技能施法范围
    public float AOERange;                        // 技能AOE范围

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      SkillId = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      SkillDataFile = DBCUtil.ExtractString(node, "LogicDataFile", "", true);
      SkillIcon = DBCUtil.ExtractString(node, "Icon", "", true);
      SkillDescription = DBCUtil.ExtractString(node, "Description", "", true);
      SkillTrueDescription = DBCUtil.ExtractString(node, "TrueDescription", "", true);
      ActivateLevel = DBCUtil.ExtractNumeric<int>(node, "ActivateLevel", 0, true);
      SkillPassivity = (Passivity)DBCUtil.ExtractNumeric<int>(node, "Passivity", 0, true);
      SkillCoefficient = DBCUtil.ExtractNumeric<float>(node, "SkillCoefficient", 0, true);
      TargetType = (TargType)DBCUtil.ExtractNumeric<int>(node, "TargetType", 0, true);
      TargetSelectType = (TargSelectType)DBCUtil.ExtractNumeric<int>(node, "TargetSelectType", 0, true);
      CoolDownTime = DBCUtil.ExtractNumeric<float>(node, "CD", 0, true);
      CostHp = DBCUtil.ExtractNumeric<int>(node, "CostHp", 0, false);
      CostEnergy = DBCUtil.ExtractNumeric<int>(node, "CostEnergy", 0, false);
      CostEnergyCore = DBCUtil.ExtractNumeric<int>(node, "CostEnergyCore", 0, false);
      CostItemId = DBCUtil.ExtractNumeric<int>(node, "CostItemId", 0, false);
      RangeIndicatorType = (RangeType)DBCUtil.ExtractNumeric<int>(node, "RangeIndicatorType", 0, false);
      SkillRangeAsset = DBCUtil.ExtractString(node, "SkillRangeAsset", "", false);
      AOERangeAsset = DBCUtil.ExtractString(node, "AOERangeAsset", "", false);
      SkillAssetStdRange = DBCUtil.ExtractNumeric<float>(node, "SkillAssetStdRange", 0, false);
      AOEAssetStdRange = DBCUtil.ExtractNumeric<float>(node, "AOEAssetStdRange", 0, false);
      SkillRange = DBCUtil.ExtractNumeric<float>(node, "SkillRange", 0, false);
      AOERange = DBCUtil.ExtractNumeric<float>(node, "AOERange", 0, false);

      return true;
    }

    /**
     * @brief
     *   get skill ID
     *
     * @return 
     */
    public int GetId()
    {
      return SkillId;
    }

    /**
     *  @brief
     *    dump all data
     */
    public void Dump()
    {
      LogSystem.Debug("Id={0}", SkillId);
      LogSystem.Debug("Icon={0}", SkillIcon);
      LogSystem.Debug("Name={0}", SkillDescription);
      LogSystem.Debug("Level={0}", SkillLevel);
      LogSystem.Debug("Passivity={0}", SkillPassivity);
      LogSystem.Debug("TargetType={0}", TargetType);
      LogSystem.Debug("TargetSelectType={0}", TargetSelectType);
      LogSystem.Debug("Cooldown={0}", CoolDownTime);
      LogSystem.Debug("CostHp={0}", CostHp);
      LogSystem.Debug("CostEnergy={0}", CostEnergy);
      LogSystem.Debug("CostEnergyCore={0}", CostEnergyCore);
      LogSystem.Debug("CostItemId={0}", CostItemId);
    }
  }

  /**
 * @brief 效果数据
 */
  public class ImpactLogicData : IData
  {
    public int ImpactId;                  // 效果ID
    public int ImpactLogicId;             // 效果逻辑ID
    public string ImpactDescription;      // 效果说明
    public int ImpactType;                // 效果类型，1-持续效果  2-瞬发效果
    public int MaxRank;                   // 效果可以叠加的最大层数
    public string Effect;                 // 效果发动时候目标的特效
    public int ActionId;                  // 效果发动时目标产生的动作
    public int ImpactTime;                // 效果持续时间，单位：毫秒 （瞬发效果无用）
    public int BuffDataId;                // 效果发作的间歇时间(瞬发效果无用)
    public bool IsDeadDisappear;          // 死后是否可以保留
    public bool CanBeCancel;              // 是否可以被玩家自己手动取消（右键点击）
    public bool IsAbsorbHurt;             // 伤害能否被吸收
    public bool IsReflect;                // 效果能否被反射
    public bool IsDamageDisappear;        // 受到伤害是否会消失
    public bool IsFightDisappear;         // 进入战斗状态是否会消失
    public bool IsShootDisappear;         // 射击时是否消失
    public bool IsSkillDisappear;         // 释放技能时是否消失

    // 扩展数据项
    public int ParamNum = 0;
    public List<string> ExtraParams = new List<string>();
    
    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      ImpactId = DBCUtil.ExtractNumeric<int>(node, "Id", -1, true);
      ImpactLogicId = DBCUtil.ExtractNumeric<int>(node, "LogicId", -1, true);
      ImpactDescription = DBCUtil.ExtractString(node, "Description", "", false);
      ImpactType = DBCUtil.ExtractNumeric<int>(node, "ImpactType", -1, true);
      ImpactTime = DBCUtil.ExtractNumeric<int>(node, "ImpactTime", -1, true);
      BuffDataId = DBCUtil.ExtractNumeric<int>(node, "BuffDataId", -1, false);
      MaxRank = DBCUtil.ExtractNumeric<int>(node, "MaxRank", -1, false);
      Effect = DBCUtil.ExtractString(node, "Effect", "", false);
      ActionId = DBCUtil.ExtractNumeric<int>(node, "ActionId", -1, false);
      IsDeadDisappear = DBCUtil.ExtractBool(node, "IsDeadDisappear", false, false);
      CanBeCancel = DBCUtil.ExtractBool(node, "CanBeCancel", false, false);
      IsAbsorbHurt = DBCUtil.ExtractBool(node, "IsAbsorbHurt", false, false);
      IsReflect = DBCUtil.ExtractBool(node, "IsReflect", false, false);
      IsDamageDisappear = DBCUtil.ExtractBool(node, "IsDamageDisappear", false, false);
      IsFightDisappear = DBCUtil.ExtractBool(node, "IsFightDisappear", false, false);
      IsShootDisappear = DBCUtil.ExtractBool(node, "IsShootDisappear", false, false);
      IsSkillDisappear = DBCUtil.ExtractBool(node, "IsSkillDisappear", false, false);

      ParamNum = DBCUtil.ExtractNumeric<int>(node, "ParamNum", 0, false);
      ExtraParams.Clear();
      if (ParamNum > 0) {
        for (int i = 0; i < ParamNum; ++i) {
          string key = "Param" + i.ToString();
          ExtraParams.Insert(i, DBCUtil.ExtractString(node, key, "", false)); 
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
      return ImpactId;
    }
  }
    
  /**
 * @brief 效果数据
 */
  public class SoundLogicData : IData
  {
    public int SoundId;                   // 声音ID
    public string SoundDescription;      // 声音说明

    // 武器声音资源
    public const int skillSoundCount = 6;
    public System.Collections.Generic.List<string> m_SoundList = new System.Collections.Generic.List<string>(skillSoundCount);

    // 扩展数据项
    public int ParamNum = 0;
    public List<string> ExtraParams = new List<string>();
    
    public SoundLogicData()
    {
      m_SoundList.Clear();
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
      SoundId = DBCUtil.ExtractNumeric<int>(node, "Id", -1, true);
      SoundDescription = DBCUtil.ExtractString(node, "Description", "", false);
   
      for (int i = 0; i < skillSoundCount; ++i) {
        string NodeName = "Sound" + i.ToString();
        m_SoundList.Add(DBCUtil.ExtractString(node, NodeName, "", false));
      }

      ParamNum = DBCUtil.ExtractNumeric<int>(node, "ParamNum", 0, false);
      ExtraParams.Clear();
      if (ParamNum > 0) {
        for (int i = 0; i < ParamNum; ++i) {
          string key = "Param" + i.ToString();
          ExtraParams.Insert(i, DBCUtil.ExtractString(node, key, "", false)); 
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
      return SoundId;
    }
  }
}
