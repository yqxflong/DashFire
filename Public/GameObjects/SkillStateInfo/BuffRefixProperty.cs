using System;

namespace DashFire
{
  /// 
  /// @brief
  ///   根据配置修正角色的属性值
  ///   配置文件为 FilePathDefine.cs中C_BuffConfig所对应的文件
  ///
  class BuffRefixProperty
  {
    public static void RefixCharacterProperty(CharacterInfo entity, int buffId, float factor)
    {
      BuffConfig buffConfig = BuffConfigProvider.Instance.GetDataById(buffId);
      if (null == buffConfig) return;

      float aMoveSpeed = entity.GetActualProperty().MoveSpeed;
      int aHpMax = entity.GetActualProperty().HpMax;
      int aEnergyMax = entity.GetActualProperty().EnergyMax;
      int aEnergyCoreMax = entity.GetActualProperty().EnergyCoreMax;
      int aCrgMax = entity.GetActualProperty().CrgMax;
      float aHpRecover = entity.GetActualProperty().HpRecover;
      float aEnergyRecover = entity.GetActualProperty().EnergyRecover;
      float aEnergyCoreRecover = entity.GetActualProperty().EnergyCoreRecover;
      int aAttackBase = entity.GetActualProperty().AttackBase;
      int aDefenceBase = entity.GetActualProperty().DefenceBase;
      float aCritical = entity.GetActualProperty().Critical;
      float aCriticalPow = entity.GetActualProperty().CriticalPow;
      float aArmorPenetration = entity.GetActualProperty().ArmorPenetration;
      float aEnergyIntensity = entity.GetActualProperty().EnergyIntensity;
      float aEnergyArmor = entity.GetActualProperty().EnergyArmor;
      float aAttackRange = entity.GetActualProperty().AttackRange;

      float aRps = entity.GetActualProperty().Rps;
      int aCrg = entity.GetActualProperty().Crg;
      float aCht = entity.GetActualProperty().Cht;
      float aWdps = entity.GetActualProperty().Wdps;
      float aDamRandom = entity.GetActualProperty().DamRandom;

      aMoveSpeed += AddFactor(buffConfig.m_AttrData.GetAddSpd(aMoveSpeed, entity.GetLevel()), factor);
      aHpMax += (int)AddFactor(buffConfig.m_AttrData.GetAddHpMax(aHpMax, entity.GetLevel()), factor);
      aEnergyMax += (int)AddFactor(buffConfig.m_AttrData.GetAddNpMax(aEnergyMax, entity.GetLevel()), factor);
      aEnergyCoreMax += (int)AddFactor(buffConfig.m_AttrData.GetAddEpMax(aEnergyCoreMax, entity.GetLevel()), factor);
      aCrgMax += (int)AddFactor(buffConfig.m_AttrData.GetAddCrgMax(aCrgMax, entity.GetLevel()), factor);
      aHpRecover += AddFactor(buffConfig.m_AttrData.GetAddHpRecover(aHpRecover, entity.GetLevel()), factor);
      aEnergyRecover += AddFactor(buffConfig.m_AttrData.GetAddNpRecover(aEnergyRecover, entity.GetLevel()), factor);
      aEnergyCoreRecover += AddFactor(buffConfig.m_AttrData.GetAddEpRecover(aEnergyCoreRecover, entity.GetLevel()), factor);
      aAttackBase += (int)AddFactor(buffConfig.m_AttrData.GetAddAd(aAttackBase, entity.GetLevel()), factor);
      aDefenceBase += (int)AddFactor(buffConfig.m_AttrData.GetAddDp(aDefenceBase, entity.GetLevel()), factor);
      aCritical += AddFactor(buffConfig.m_AttrData.GetAddCri(aCritical, entity.GetLevel()), factor);
      aCriticalPow += AddFactor(buffConfig.m_AttrData.GetAddPow(aCriticalPow, entity.GetLevel()), factor);
      aArmorPenetration += AddFactor(buffConfig.m_AttrData.GetAddAndp(aArmorPenetration, entity.GetLevel()), factor);
      aEnergyIntensity += AddFactor(buffConfig.m_AttrData.GetAddAp(aEnergyIntensity, entity.GetLevel()), factor);
      aEnergyArmor += AddFactor(buffConfig.m_AttrData.GetAddTay(aEnergyArmor, entity.GetLevel()), factor);

      aAttackRange += AddFactor(buffConfig.m_AttrData.GetAddRange(aAttackRange, entity.GetLevel()), factor);
      aRps += AddFactor(buffConfig.m_AttrData.GetAddRps(aRps, entity.GetLevel()), factor);
      aCrg += (int)AddFactor(buffConfig.m_AttrData.GetAddCrg(aCrg, entity.GetLevel()), factor);
      aCht += AddFactor(buffConfig.m_AttrData.GetAddCht(aCht, entity.GetLevel()), factor);
      aWdps += AddFactor(buffConfig.m_AttrData.GetAddDps(aWdps, entity.GetLevel()), factor);
      aDamRandom += AddFactor(buffConfig.m_AttrData.GetAddDamRange(aDamRandom, entity.GetLevel()), factor);

      entity.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, aMoveSpeed);
      entity.GetActualProperty().SetHpMax(Operate_Type.OT_Absolute, aHpMax);
      entity.GetActualProperty().SetEnergyMax(Operate_Type.OT_Absolute, aEnergyMax);
      entity.GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Absolute, aEnergyCoreMax);
      entity.GetActualProperty().SetCrgMax(Operate_Type.OT_Absolute, aCrgMax);
      entity.GetActualProperty().SetHpRecover(Operate_Type.OT_Absolute, aHpRecover);
      entity.GetActualProperty().SetEnergyRecover(Operate_Type.OT_Absolute, aEnergyRecover);
      entity.GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Absolute, aEnergyCoreRecover);
      entity.GetActualProperty().SetAttackBase(Operate_Type.OT_Absolute, aAttackBase);
      entity.GetActualProperty().SetDefenceBase(Operate_Type.OT_Absolute, aDefenceBase);
      entity.GetActualProperty().SetCritical(Operate_Type.OT_Absolute, aCritical);
      entity.GetActualProperty().SetCriticalPow(Operate_Type.OT_Absolute, aCriticalPow);
      entity.GetActualProperty().SetArmorPenetration(Operate_Type.OT_Absolute, aArmorPenetration);
      entity.GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Absolute, aEnergyIntensity);
      entity.GetActualProperty().SetEnergyArmor(Operate_Type.OT_Absolute, aEnergyArmor);

      entity.GetActualProperty().SetAttackRange(Operate_Type.OT_Absolute, aAttackRange);
      entity.GetActualProperty().SetRps(Operate_Type.OT_Absolute, aRps);
      entity.GetActualProperty().SetCrg(Operate_Type.OT_Absolute, aCrg);
      entity.GetActualProperty().SetCht(Operate_Type.OT_Absolute, aCht);
      entity.GetActualProperty().SetWdps(Operate_Type.OT_Absolute, aWdps);
      entity.GetActualProperty().SetDamRange(Operate_Type.OT_Absolute, aDamRandom);
    }
    private static int AddFactor(int input, float factor)
    {
        return (int)(factor * input);
    }
    private static float AddFactor(float input, float factor)
    {
        return factor * input;
    }
  }
}
