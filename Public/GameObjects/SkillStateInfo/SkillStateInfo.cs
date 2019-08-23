using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class SkillStateInfo
  {
    private List<SkillInfo> m_SkillList;   // 技能容器
    private SkillInfo m_CurSkillInfo;      // 当前所释放的点技能
    private List<ImpactInfo> m_ImpactList; // 效果容器
    private bool m_BuffChanged;            // BUFF状态是否改变
    private bool m_IsInCombat;             // 表示当前是否在战斗中

    public SkillStateInfo()
    {
      m_SkillList = new List<SkillInfo>();
      m_CurSkillInfo = null;
      m_ImpactList = new List<ImpactInfo>();
    }

    public void Reset()
    {
      m_SkillList.Clear();
      m_ImpactList.Clear();
      m_CurSkillInfo = null;
      m_BuffChanged = false;
      m_IsInCombat = false;
    }

    public bool IsSkillActivated()
    {
      return (null == m_CurSkillInfo) ? false : m_CurSkillInfo.IsSkillActivated;
    }

    public void AddSkill(int index, SkillInfo info)
    {
      if (m_SkillList.Count == index)
      {
        m_SkillList.Insert(index, info);
      }
      else if (m_SkillList.Count > index)
      {
        m_SkillList[index] = info;
      }
    }
    public void AddSkill(SkillInfo info)
    {
      m_SkillList.Add(info);
    }

    ///
    /// SkillLevel
    ///
    public int GetSkillLevel(int skillIndex)
    {
      if (m_SkillList.Count > skillIndex)
      {
        return m_SkillList[skillIndex].SkillLevel;
      }
      return 0;
    }

    public int GetTotalSkillLevel()
    {
      int totalLevel = 0;
      foreach (SkillInfo info in m_SkillList)
      {
        if (info != null)
          totalLevel += info.SkillLevel;
      }
      return totalLevel;
    }

    ///
    /// SkillInfo
    ///
    public SkillInfo GetCurSkillInfo()
    {
      return m_CurSkillInfo;
    }

    public SkillInfo GetSkillInfoById(int skillId)
    {
      return m_SkillList.Find(
          delegate(SkillInfo info)
          {
            if (info == null) return false;
            return info.SkillId == skillId;
          }
          );
    }

    public SkillInfo GetSkillInfoByIndex(int skillIndex)
    {
      if (m_SkillList.Count > skillIndex)
      {
        return m_SkillList[skillIndex];
      }

      return null;
    }

    public List<SkillInfo> GetAllSkill()
    {
      return m_SkillList;
    }

    public void SetCurSkillInfo(int skillId)
    {
      SkillInfo skillInfo = m_SkillList.Find(
          delegate(SkillInfo info)
          {
            if (info == null) return false;
            return info.SkillId == skillId;
          }
          );
      if (null != skillInfo) m_CurSkillInfo = skillInfo;
    }

    public void RemoveSkill(int skillId)
    {
      SkillInfo oriSkill = GetSkillInfoById(skillId);
      if (oriSkill != null)
      {
        m_SkillList.Remove(oriSkill);
      }
    }

    public void RemoveAllSkill()
    {
      m_SkillList.Clear();
    }

    ///
    /// ImpactInfo
    ///
    public bool BuffChanged
    {
      get { return m_BuffChanged; }
      set { m_BuffChanged = value; }
    }

    public void AddImpact(ImpactInfo info)
    {
      ImpactInfo oriImpact = GetImpactInfoById(info.m_ImpactId);
      if (oriImpact == null)
      {
        m_ImpactList.Add(info);
      }
      else
      {
        m_ImpactList.Remove(oriImpact);
        m_ImpactList.Add(info);
      }
      if ((int)ImpactType.BUFF == info.m_ImpactType)
      {
        m_BuffChanged = true;
      }
    }

    public ImpactInfo GetImpactInfoById(int impactId)
    {
      return m_ImpactList.Find(
          delegate(ImpactInfo info)
          {
            return info.m_ImpactId == impactId;
          }
          );
    }

    public List<ImpactInfo> GetAllImpact()
    {
      return m_ImpactList;
    }

    public void RemoveImpact(int impactId)
    {
      ImpactInfo oriImpact = GetImpactInfoById(impactId);
      if (oriImpact != null)
      {
        if ((int)ImpactType.BUFF == oriImpact.m_ImpactType)
        {
          m_BuffChanged = true;
        }
        m_ImpactList.Remove(oriImpact);
      }
    }

    public void RemoveAllImpact()
    {
      m_ImpactList.Clear();
      m_BuffChanged = true;
    }
        
    /// 战斗状态
    public void SetCombatState(bool value)
    {
      if (m_IsInCombat == value)
        return;
      else
      {
        m_IsInCombat = value;
      }
    }
    public bool GetCombatState()
    {
      return m_IsInCombat;
    }
  }
}
