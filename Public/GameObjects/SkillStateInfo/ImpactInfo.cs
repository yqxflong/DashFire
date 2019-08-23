/**
 * @file ImpactInfo.cs
 * @brief 效果动态数据结构类似于buff(持续性效果)
 *           
 *
 * @author liu bing
 * @version 1.0.0
 * @date 2013-01-30
 */

using System;
using System.Collections.Generic;
using ScriptRuntime;

namespace DashFire
{
  public enum ImpactType
  {
    BUFF = 1,    // continuous impact
    INSTANT = 2, // instant impact
  }

  public class ImpactInfo
  {
    public int m_ImpactId = -1;           // 效果ID
    public int m_ImpactType = -1;         // 效果类型
    public int m_SkillId = -1;            // 技能ID
    public long m_StartTime;              // 效果开始生效起始时间
    public int m_ImpactDuration = 0;      // 效果持续时间，即，总的持续时间。
    public int m_BuffDataId;              // 持续性效果对应的BuffDataId
    public int m_ImpactSenderId = -1;     // 效果触发者的Id
    public int m_ImpactSenderType = -1;   // 效果出发者的类型：npc or user
    public bool m_IsActivated = false;    // 效果是否激活
    public float m_factor = 1.0f;           // 效果数值放大
    public int m_ImpactWrapCnt = 1;
    public Vector3 m_ImpactSourcePos = new Vector3();
    public bool m_IsItemImpact = false;
    public bool m_IsMarkToRemove = false;
    public bool m_HasEffectApplyed = false;
    public uint effectId = 0;
    
    // 存放动作播放过程中产生的上下文数据
    public TypedDataCollection LogicDatas = new TypedDataCollection();
    public ImpactLogicData ConfigData = null;
    public object CustomData = null;

    public void RefixCharacterProperty(CharacterInfo entity)
    {
       BuffRefixProperty.RefixCharacterProperty(entity, m_BuffDataId, m_factor);
    }
    public void ClearLogicDatas()
    {
      LogicDatas.Clear();
    }
    public void AddImpactWrapCnt()
    {
      ++m_ImpactWrapCnt;
    }
    public void AddImpactWrapCnt(int count) {
      m_ImpactWrapCnt += count;
    }
  }

}
