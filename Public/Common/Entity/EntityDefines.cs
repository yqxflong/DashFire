
/**
 * @file EntityDefines.cs
 * @brief 角色定义常量
 *
 * @author lixiaojiang
 * @version 0
 * @date 2012-11-14
 */

using System;
using System.Collections.Generic;
//using System.Diagnostics;

namespace DashFire
{
  /**
   * @brief 角色类型
   */
  public enum Entity_Type 
  {

    // 角色
    ET_Character,

      // 装备
      ET_Equipment,

      // 效果
      ET_Effect,
  }

  /**
   * @brief 角色类型
   */
  public enum Character_Type
  {

    // 玩家
    ET_Player,

      // npc
      ET_Npc,
  }

  /**
   * @brief 装备类型
   */
  public enum Equipment_Type
  {
    // 武器
    ET_Weapon,

      // 防具
      ET_Armor,
  }

  /**
   * @brief 效果类型
   */
  public enum Effect_Type
  {
    // 子弹
    ET_Bullet,

      // 加血包
      ET_HpBag,

      // 能量包
      ET_EnergyBat,

      // 机关
      ET_Trigger,
  }

  /**
   * @brief 数值操作类型
   */
  public enum Operate_Type
  {
    // 设置绝对值，直接设置当前值
    OT_Absolute,

    // 设置相对值，即在当前基础上的增加值，可以为负数
    OT_Relative,

    // 设置相对当前值的百分比
    OT_PercentCurrent,

    // 设置相对最大值百分比，[!!!此类操作必须要求改值存在最大值，比如HP，ENERGY, MOVESPEED, SHOOTSPEED]
    OT_PercentMax,

    // 设置位
    OT_AddBit,

    // 取消位
    OT_RemoveBit,
  }

  /**
   * @brief 属性类型
   */
  public enum Property_Type
  {
    PT_Base,
      PT_Actual,
  }

  /**
   * @brief 角色属性类型
   */
  public enum CharacterProperty_Type
  {
    /**
     * @brief 生命值
     */
    EPT_Hp,

      /**
       * @brief 能量值
       */
      EPT_Energy,

      /**
       * @brief 移动速度
       */
      EPT_MoveSpeed,

      /**
       * @brief 射击速度
       */
      EPT_ShootSpeed,

      /**
       * @brief 最大生命值
       */
      EPT_HpMax,

      /**
       * @brief 最大能量值
       */
      EPT_EnergyMax,

      /**
       * @brief 生命值回复速度
       */
      EPT_HpRecover,

      /**
       * @brief 能量值回复速度
       */
      EPT_EnergyRecover,

      /**
       * @brief 基础攻击力
       */
      EPT_AttackBase,

      /**
       * @brief 基础防御力
       */
      EPT_DefenceBase,

      /**
       * @brief 最大移动速度
       */
      EPT_MoveSpeedMax,

      /**
       * @brief 最大射击速度
       */
      EPT_ShootSpeedMax,

      /**
       * @brief 暴击率
       */
      EPT_Critical,

      /**
       * @brief 暴击伤害
       */
      EPT_CriticalDmg,

      /**
       * @brief 装备防护
       */
      EPT_ArmorPenetration,

      /**
       * @brief 视野范围
       */
      EPT_ViewRange,
      /**
       * @brief 攻击范围
       */
      EPT_AttackRange,

      /**
       * @brief 韧性
       */
      EPT_Tenacity,

      /**
       * @brief 状态标记
       */
      EPT_StateFlag,
  }

  /**
   * @brief 武器属性类型
   */
  public enum WeaponProperty_Type
  {
    /**
     * @brief 射程
     */
    WPT_Range,

      /**
       * @brief 装填速度
       */
      WPT_Reload,

      /**
       * @brief 充能速度
       */
      WPT_Charge,

      /**
       * @brief 充能数量
       */
      WPT_ChargeNum,

      /**
       * @brief 弹夹容量
       */
      WPT_MaxAmmo,

      /**
       * @brief 射速
       */
      WPT_ShootSpeed,

      /**
       * @brief 基础攻击力
       */
      WPT_AttackBase,

      /**
       * @brief 弹幕ID
       */
      WPT_BarrageID,

      /**
       * @brief 装填标志位
       */
      WPT_ReloadFlag,

      /**
       * @brief 当前装弹数
       */
      WPT_Ammus,
  }

  /**
   * @brief 防具属性类型
   */
  public enum ArmorProperty_Type
  {

    /**
     * @brief 韧性
     */
    APT_Tenacity,

      /**
       * @brief 移动速度
       */
      APT_MoveSpeed,

      /**
       * @brief 防御力
       */
      APT_Defence,

      /**
       * @brief 生命值
       */
      APT_Hp,

      /**
       * @brief 生命值回复速度
       */
      APT_HpRecover,
  }

  // 关系
  public enum Input_Type
  {
    IT_Move,
      IT_Rotate,
      IT_Fire,
      IT_All,
  }

  public enum RelMoveDir
  {
    Forward = 1,
    Backward = 2,
    Leftward = 4,
    Rightward = 8,
  }

  public enum CampIdEnum : int
  {
    Unkown = 0,
    Friendly,
    Hostile,
    Blue,
    Red,
  }

  // 关系
  public enum CharacterRelation : int
  {
    RELATION_INVALID = -1,
    RELATION_ENEMY,				// 敌对
    RELATION_FRIEND,			// 友好
    RELATION_NUMBERS
  };
}

