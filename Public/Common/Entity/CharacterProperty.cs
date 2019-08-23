/**
 * @file CharacterProperty.cs
 * @brief 角色属性类
 *          用于技能系统中的属性变化
 *
 * @author lixiaojiang
 * @version 0
 * @date 2012-11-14
 */

using System;
using System.Collections.Generic;

namespace DashFire
{
  /**
   * @brief 角色属性类
   */
  public sealed class CharacterProperty
  {
    /**
     * @brief 构造函数
     *
     * @param owner
     *
     * @return 
     */
    public CharacterProperty()
    {}

    /**
     * @brief 移动速度
     */
    public float MoveSpeed
    {
      get { return m_MoveSpeed; }
    }
    
    /**
     * @brief 最大血量
     */
    public int HpMax
    {
      get { return m_HpMax;}
    }

    /**
     * @brief 最大血量
     */
    public int RageMax
    {
      get { return m_RageMax; }
    }

    /**
     * @brief 最大能量
     */
    public int EnergyMax
    {
      get { return m_EnergyMax;}
    }

    /**
     * @brief 最大能量豆
     */
    public int EnergyCoreMax
    {
      get { return m_EnergyCoreMax;}
    }

    /**
     * @brief 最大子弹数
     */
    public int CrgMax
    {
      get { return m_CrgMax; }
    }
    /**
     * @brief 生命值回复速度
     */
    public float HpRecover
    {
      get { return m_HpRecover;}
    }

    /**
     * @brief 能量值回复速度
     */
    public float EnergyRecover
    {
      get { return m_EnergyRecover;}
    }

    /**
     * @brief 能量豆回复速度
     */
    public float EnergyCoreRecover
    {
      get { return m_EnergyCoreRecover;}
    }
    /**
     * @brief 基础攻击力
     */
    public int AttackBase
    {
      get { return m_AttackBase;}
    }

    /**
     * @brief 基础防御力
     */
    public int DefenceBase
    {
      get { return m_DefenceBase;}
    }
    
    /**
     * @brief 暴击率
     */
    public float Critical
    {
      get { return m_Critical; }
    }

    /**
     * @brief 暴击倍率
     */
    public float CriticalPow
    {
      get { return m_CriticalPow; }
    }

    /**
     * @brief 护甲穿透
     */
    public float ArmorPenetration
    {
      get { return m_ArmorPenetration; }
    }

    /**
     * @brief 能量强度
     */
    public float EnergyIntensity
    {
      get { return m_EnergyIntensity; }
    }

    /**
     * @brief 能量护甲
     */
    public float EnergyArmor
    {
      get { return m_EnergyArmor; }
    }

    /**
     * @brief 攻击范围
     */
    public float AttackRange
    {
      get { return m_AttackRange; }
    }

    public float Rps
    {
      get { return m_Rps; }
    }
    public int Crg
    {
      get { return m_Crg; }
    }
    public float Cht
    {
      get { return m_Cht; }
    }
    public float Wdps
    {
      get { return m_Wdps; }
    }
    public float DamRandom
    {
      get { return m_DamRandom; }
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetMoveSpeed(Operate_Type opType, float tVal)
    {
      m_MoveSpeed = UpdateAttr(m_MoveSpeed, m_MoveSpeed, opType, tVal);
    }
    
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetHpMax(Operate_Type opType, int tVal)
    {
      m_HpMax = (int)UpdateAttr(m_HpMax, m_HpMax, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetRageMax(Operate_Type opType, int tVal)
    {
      m_RageMax = (int)UpdateAttr(m_RageMax, m_RageMax, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyMax(Operate_Type opType, int tVal)
    {
      m_EnergyMax = (int)UpdateAttr(m_EnergyMax, m_EnergyMax, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyCoreMax(Operate_Type opType, int tVal)
    {
      m_EnergyCoreMax = (int)UpdateAttr(m_EnergyCoreMax, m_EnergyCoreMax, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetCrgMax(Operate_Type opType, int tVal)
    {
      m_CrgMax = (int)UpdateAttr(m_CrgMax, m_CrgMax, opType, tVal);
    }
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetHpRecover(Operate_Type opType, float tVal)
    {
      m_HpRecover = UpdateAttr(m_HpRecover, opType, tVal);
    }
    
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyRecover(Operate_Type opType, float tVal)
    {
      m_EnergyRecover = UpdateAttr(m_EnergyRecover, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyCoreRecover(Operate_Type opType, float tVal)
    {
      m_EnergyCoreRecover = UpdateAttr(m_EnergyCoreRecover, opType, tVal);
    }
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetAttackBase(Operate_Type opType, int tVal)
    {
      m_AttackBase = (int)UpdateAttr(m_AttackBase, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetDefenceBase(Operate_Type opType, int tVal)
    {
      m_DefenceBase = (int)UpdateAttr(m_DefenceBase, opType, tVal);
    }
        
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetCritical(Operate_Type opType, float tVal)
    {
      m_Critical = UpdateAttr(m_Critical, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetCriticalPow(Operate_Type opType, float tVal)
    {
      m_CriticalPow = UpdateAttr(m_CriticalPow, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetArmorPenetration(Operate_Type opType, float tVal)
    {
      m_ArmorPenetration = UpdateAttr(m_ArmorPenetration, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyIntensity(Operate_Type opType, float tVal)
    {
      m_EnergyIntensity = UpdateAttr(m_EnergyIntensity, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergyArmor(Operate_Type opType, float tVal)
    {
      m_EnergyArmor = UpdateAttr(m_EnergyArmor, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetAttackRange(Operate_Type opType, float tVal)
    {
      m_AttackRange = UpdateAttr(m_AttackRange, opType, tVal);
    }
    /**
     * @brief 射速
     */
    public void SetRps(Operate_Type opType, float tVal)
    {
      m_Rps = UpdateAttr(m_Rps, opType, tVal);
    }

    /**
     * @brief 弹容量
     */
    public void SetCrg(Operate_Type opType, int tVal)
    {
      m_Crg = (int)UpdateAttr(m_Crg, opType, tVal);
    }

    /**
     * @brief 换弹甲时间
     */
    public void SetCht(Operate_Type opType, float tVal)
    {
      m_Cht = UpdateAttr(m_Cht, opType, tVal);
    }

    /**
     * @brief 武器dps
     */
    public void SetWdps(Operate_Type opType, float tVal)
    {
      m_Wdps = UpdateAttr(m_Wdps, opType, tVal);
    }

    /**
     * @brief 武器伤害浮动
     */
    public void SetDamRange(Operate_Type opType, float tVal)
    {
      m_DamRandom = UpdateAttr(m_DamRandom, opType, tVal);
    }


    public static float UpdateAttr(float val, float maxVal, Operate_Type opType, float tVal)
    {
      float ret = val;
      if (opType == Operate_Type.OT_PercentMax) {       
        float t = maxVal * (tVal/100.0f);
        ret = t;
      } else {
        ret = UpdateAttr(val, opType, tVal);
      }
      return ret;
    }

    public static float UpdateAttr(float val, Operate_Type opType, float tVal)
    {
      float ret = val;
      if (opType == Operate_Type.OT_Absolute) {
        ret = tVal;
      } else if (opType == Operate_Type.OT_Relative) {
        float t = (ret + tVal);
        if (t < 0) {
          t = 0;
        }
        ret = t;
      } else if (opType == Operate_Type.OT_PercentCurrent) {
        float t = (ret * (tVal / 100.0f));
        ret = t;
      }
      return ret;
    }

    /**
     * @brief 移动速度
     */
    private float m_MoveSpeed;
    /**
     * @brief 最大生命值
     */
    private int m_HpMax;
    /**
     * @brief 最大怒气值
     */
    private int m_RageMax;
    /**
     * @brief 最大能量值
     */
    private int m_EnergyMax;
    /**
     * @brief 最大能量豆
     */
    private int m_EnergyCoreMax;
    /**
     * @brief 最大子弹数
     */
    private int m_CrgMax;
    /**
     * @brief 生命值回复速度
     */
    private float m_HpRecover;
    /**
     * @brief 能量值回复速度
     */
    private float m_EnergyRecover;
    /**
     * @brief 能量豆回复速度
     */
    private float m_EnergyCoreRecover;
    /**
     * @brief 基础攻击力
     */
    private int m_AttackBase;
    /**
     * @brief 基础防御力
     */
    private int m_DefenceBase;
    /**
     * @brief 暴击率
     */
    private float m_Critical;
    /**
     * @brief 暴击倍率
     */
    private float m_CriticalPow;
    /**
     * @brief 护甲穿透
     */
    private float m_ArmorPenetration;
    /**
     * @brief 能量强度
     */
    private float m_EnergyIntensity;
    /**
     * @brief 能量护甲
     */
    private float m_EnergyArmor;
    /**
     * @brief 攻击范围
     */
    private float m_AttackRange;
    /**
     * @brief 射速
     */
    private float m_Rps;
    /**
     * @brief 弹容量
     */
    private int m_Crg;
    /**
     * @brief 换弹甲时间
     */
    private float m_Cht;
    /**
     * @brief 武器dps
     */
    private float m_Wdps;
    /**
     * @brief 武器伤害浮动
     */
    private float m_DamRandom;    
  }
}
