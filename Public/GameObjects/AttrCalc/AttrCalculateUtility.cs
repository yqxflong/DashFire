using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class AttrCalculateUtility
  {
    public static void ResetBaseProperty(CharacterInfo obj)
    {
      obj.CalcBaseAttr();
    }
    public static void RefixAttrByEquipment(CharacterInfo obj)
    {
      //装备不会有百分比属性加成，所以不需要考虑当前值
      //计算武器影响
      int index = obj.GetShootStateInfo().CurrentWeaponIndex;
      if (index >= 0 && index < EquipmentInfo.c_MaxWeaponNum) {
        EquipmentDataInfo info = obj.GetEquipmentStateInfo().EquipmentInfo.Weapons[index];
        if (null != info) {
          obj.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Relative, info.GetAddSpd(0,obj.GetLevel()));
          obj.GetActualProperty().SetHpMax(Operate_Type.OT_Relative, (int)info.GetAddHpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyMax(Operate_Type.OT_Relative, (int)info.GetAddNpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Relative, (int)info.GetAddEpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrgMax(Operate_Type.OT_Relative, (int)info.GetAddCrgMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpRecover(Operate_Type.OT_Relative, info.GetAddHpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyRecover(Operate_Type.OT_Relative, info.GetAddNpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Relative, info.GetAddEpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetAttackBase(Operate_Type.OT_Relative, (int)info.GetAddAd(0, obj.GetLevel()));
          obj.GetActualProperty().SetDefenceBase(Operate_Type.OT_Relative, (int)info.GetAddDp(0, obj.GetLevel()));
          obj.GetActualProperty().SetCritical(Operate_Type.OT_Relative, info.GetAddCri(0, obj.GetLevel()));
          obj.GetActualProperty().SetCriticalPow(Operate_Type.OT_Relative, info.GetAddPow(0, obj.GetLevel()));
          obj.GetActualProperty().SetArmorPenetration(Operate_Type.OT_Relative, info.GetAddAndp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Relative, info.GetAddAp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyArmor(Operate_Type.OT_Relative, info.GetAddTay(0, obj.GetLevel()));

          obj.GetActualProperty().SetAttackRange(Operate_Type.OT_Relative, info.GetAddRange(0, obj.GetLevel()));
          obj.GetActualProperty().SetRps(Operate_Type.OT_Relative, info.GetAddRps(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrg(Operate_Type.OT_Relative, (int)info.GetAddCrg(0, obj.GetLevel()));
          obj.GetActualProperty().SetCht(Operate_Type.OT_Relative, info.GetAddCht(0, obj.GetLevel()));
          obj.GetActualProperty().SetWdps(Operate_Type.OT_Relative, info.GetAddDps(0, obj.GetLevel()));
          obj.GetActualProperty().SetDamRange(Operate_Type.OT_Relative, info.GetAddDamRange(0, obj.GetLevel()));
        }
      }
      //计算防弹衣与挂件的影响
      {
        EquipmentDataInfo info = obj.GetEquipmentStateInfo().EquipmentInfo.BodyArmor;
        if (null != info) {
          obj.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Relative, info.GetAddSpd(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpMax(Operate_Type.OT_Relative, (int)info.GetAddHpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyMax(Operate_Type.OT_Relative, (int)info.GetAddNpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Relative, (int)info.GetAddEpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrgMax(Operate_Type.OT_Relative, (int)info.GetAddCrgMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpRecover(Operate_Type.OT_Relative, (int)info.GetAddHpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyRecover(Operate_Type.OT_Relative, info.GetAddNpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Relative, info.GetAddEpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetAttackBase(Operate_Type.OT_Relative, (int)info.GetAddAd(0, obj.GetLevel()));
          obj.GetActualProperty().SetDefenceBase(Operate_Type.OT_Relative, (int)info.GetAddDp(0, obj.GetLevel()));
          obj.GetActualProperty().SetCritical(Operate_Type.OT_Relative, info.GetAddCri(0, obj.GetLevel()));
          obj.GetActualProperty().SetCriticalPow(Operate_Type.OT_Relative, info.GetAddPow(0, obj.GetLevel()));
          obj.GetActualProperty().SetArmorPenetration(Operate_Type.OT_Relative, info.GetAddAndp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Relative, info.GetAddAp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyArmor(Operate_Type.OT_Relative, info.GetAddTay(0, obj.GetLevel()));

          obj.GetActualProperty().SetAttackRange(Operate_Type.OT_Relative, info.GetAddRange(0, obj.GetLevel()));
          obj.GetActualProperty().SetRps(Operate_Type.OT_Relative, info.GetAddRps(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrg(Operate_Type.OT_Relative, (int)info.GetAddCrg(0, obj.GetLevel()));
          obj.GetActualProperty().SetCht(Operate_Type.OT_Relative, info.GetAddCht(0, obj.GetLevel()));
          obj.GetActualProperty().SetWdps(Operate_Type.OT_Relative, info.GetAddDps(0, obj.GetLevel()));
          obj.GetActualProperty().SetDamRange(Operate_Type.OT_Relative, info.GetAddDamRange(0, obj.GetLevel()));
        }
      }
      foreach (EquipmentDataInfo info in obj.GetEquipmentStateInfo().EquipmentInfo.Pendant) {
        if (null != info) {
          obj.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Relative, info.GetAddSpd(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpMax(Operate_Type.OT_Relative, (int)info.GetAddHpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyMax(Operate_Type.OT_Relative, (int)info.GetAddNpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Relative, (int)info.GetAddEpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrgMax(Operate_Type.OT_Relative, (int)info.GetAddCrgMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpRecover(Operate_Type.OT_Relative, (int)info.GetAddHpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyRecover(Operate_Type.OT_Relative, info.GetAddNpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Relative, info.GetAddEpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetAttackBase(Operate_Type.OT_Relative, (int)info.GetAddAd(0, obj.GetLevel()));
          obj.GetActualProperty().SetDefenceBase(Operate_Type.OT_Relative, (int)info.GetAddDp(0, obj.GetLevel()));
          obj.GetActualProperty().SetCritical(Operate_Type.OT_Relative, info.GetAddCri(0, obj.GetLevel()));
          obj.GetActualProperty().SetCriticalPow(Operate_Type.OT_Relative, info.GetAddPow(0, obj.GetLevel()));
          obj.GetActualProperty().SetArmorPenetration(Operate_Type.OT_Relative, info.GetAddAndp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Relative, info.GetAddAp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyArmor(Operate_Type.OT_Relative, info.GetAddTay(0, obj.GetLevel()));

          obj.GetActualProperty().SetAttackRange(Operate_Type.OT_Relative, info.GetAddRange(0, obj.GetLevel()));
          obj.GetActualProperty().SetRps(Operate_Type.OT_Relative, info.GetAddRps(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrg(Operate_Type.OT_Relative, (int)info.GetAddCrg(0, obj.GetLevel()));
          obj.GetActualProperty().SetCht(Operate_Type.OT_Relative, info.GetAddCht(0, obj.GetLevel()));
          obj.GetActualProperty().SetWdps(Operate_Type.OT_Relative, info.GetAddDps(0, obj.GetLevel()));
          obj.GetActualProperty().SetDamRange(Operate_Type.OT_Relative, info.GetAddDamRange(0, obj.GetLevel()));
        }
      }
      //计算pvp物品的影响
      for (int ix = 0; ix < EquipmentStateInfo.c_PackageCapacity; ++ix) {
        ItemDataInfo info = obj.GetEquipmentStateInfo().GetItemData(ix);
        if (null != info) {
          obj.GetActualProperty().SetMoveSpeed(Operate_Type.OT_Relative, info.GetAddSpd(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpMax(Operate_Type.OT_Relative, (int)info.GetAddHpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyMax(Operate_Type.OT_Relative, (int)info.GetAddNpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Relative, (int)info.GetAddEpMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrgMax(Operate_Type.OT_Relative, (int)info.GetAddCrgMax(0, obj.GetLevel()));
          obj.GetActualProperty().SetHpRecover(Operate_Type.OT_Relative, info.GetAddHpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyRecover(Operate_Type.OT_Relative, info.GetAddNpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Relative, info.GetAddEpRecover(0, obj.GetLevel()));
          obj.GetActualProperty().SetAttackBase(Operate_Type.OT_Relative, (int)info.GetAddAd(0, obj.GetLevel()));
          obj.GetActualProperty().SetDefenceBase(Operate_Type.OT_Relative, (int)info.GetAddDp(0, obj.GetLevel()));
          obj.GetActualProperty().SetCritical(Operate_Type.OT_Relative, info.GetAddCri(0, obj.GetLevel()));
          obj.GetActualProperty().SetCriticalPow(Operate_Type.OT_Relative, info.GetAddPow(0, obj.GetLevel()));
          obj.GetActualProperty().SetArmorPenetration(Operate_Type.OT_Relative, info.GetAddAndp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Relative, info.GetAddAp(0, obj.GetLevel()));
          obj.GetActualProperty().SetEnergyArmor(Operate_Type.OT_Relative, info.GetAddTay(0, obj.GetLevel()));

          obj.GetActualProperty().SetAttackRange(Operate_Type.OT_Relative, info.GetAddRange(0, obj.GetLevel()));
          obj.GetActualProperty().SetRps(Operate_Type.OT_Relative, info.GetAddRps(0, obj.GetLevel()));
          obj.GetActualProperty().SetCrg(Operate_Type.OT_Relative, (int)info.GetAddCrg(0, obj.GetLevel()));
          obj.GetActualProperty().SetCht(Operate_Type.OT_Relative, info.GetAddCht(0, obj.GetLevel()));
          obj.GetActualProperty().SetWdps(Operate_Type.OT_Relative, info.GetAddDps(0, obj.GetLevel()));
          obj.GetActualProperty().SetDamRange(Operate_Type.OT_Relative, info.GetAddDamRange(0, obj.GetLevel()));
        }
      }
    }
    public static void RefixAttrByImpact(CharacterInfo obj)
    {
      List<ImpactInfo> impacts = obj.GetSkillStateInfo().GetAllImpact();
      foreach (ImpactInfo impact in impacts) {
        impact.RefixCharacterProperty(obj);
      }
    }
  }
}
