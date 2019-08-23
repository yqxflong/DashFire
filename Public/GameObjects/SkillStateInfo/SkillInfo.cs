using System;
using System.Collections.Generic;

namespace DashFire
{
  public class SkillInfo
  {
    public int SkillId;                // 技能Id
    public int SkillLevel;             // 技能等级
    public bool IsSkillActivated;      // 是否正在释放技能
    public bool IsItemSkill;
    public bool IsMarkToRemove;
    public SkillLogicData ConfigData = null;

    public SkillInfo (int skillId, int skillLevel = 0)
    {
      SkillId = skillId;
      SkillLevel = skillLevel;
      IsSkillActivated = false;
      IsItemSkill = false;
      IsMarkToRemove = false;

      ConfigData = SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId) as SkillLogicData;
    }
    
    public virtual void Reset()
    {
      IsSkillActivated = false;
      IsItemSkill = false;
      IsMarkToRemove = false;
    }

    public virtual bool IsNull()
    {
      return false;
    }
  }

  public class NullSkillInfo : SkillInfo
  {
    public NullSkillInfo():base(-1)
    {
      SkillLevel = 0;
    }
    public override bool IsNull()
    {
      return true;
    }
  }
}
