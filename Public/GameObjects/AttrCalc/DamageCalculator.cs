using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed class DamageCalculator
  {
    public static int CalcShootDamage(CharacterInfo sender, CharacterInfo receiver, out bool isCritical)
    {
      isCritical = false;
      if(null == sender || null == receiver){
        return 0;        
      }
      /* 需要检查目标是否免疫 */
      bool IsUnmatched = (receiver.IsHaveStateFlag(CharacterState_Type.CST_Invincible) || receiver.IsControlMecha);
      if(IsUnmatched){
      } else {
        float rps = (float)sender.GetActualProperty().Rps;
        float wdps = (float)sender.GetActualProperty().Wdps;
        float ad = (float)sender.GetActualProperty().AttackBase;
        float dp = (float)receiver.GetActualProperty().DefenceBase;
        float andp = (float)sender.GetActualProperty().ArmorPenetration;
        float cri = (float)sender.GetActualProperty().Critical;
        float pow = (float)sender.GetActualProperty().CriticalPow;
        float damrandom = (float)sender.GetActualProperty().DamRandom;

        float shootATK = wdps / rps + ad / (1.0f * rps);
        float shootEDP = dp - andp;
        float shootDAM = shootATK * 1.0f * (1 - shootEDP / (Math.Abs(shootEDP) + 100.0f));
        // 伤害浮动
        float shootDAMMIM = (float)(shootDAM * (1.0 - damrandom / 100.0));
        float shootDAMMAX = (float)(shootDAM * (1.0 + damrandom / 100.0));
        int shootDAMSEC = (int)(shootDAMMAX - shootDAMMIM);
        if (shootDAMSEC>0)
          shootDAM = (float)Helper.Random.Next(shootDAMSEC) + shootDAMMIM;
        // 暴击
        float shootCRIRATE = (float)(cri / 100.0);
        float shootCRIRATE_C = CriticalConfigProvider.Instance.GetC(shootCRIRATE);
        sender.GetShootStateInfo().GetCurWeaponInfo().CurCritical += shootCRIRATE_C;
        float _random = (float)Helper.Random.Next(1000);
        if (_random < sender.GetShootStateInfo().GetCurWeaponInfo().CurCritical * 1000.0 
            || sender.GetShootStateInfo().GetCurWeaponInfo().CurCritical >= 1) {
          float _CRIRATE = (float)(cri / 100.0);
          float _CRIRATE_C = CriticalConfigProvider.Instance.GetC(_CRIRATE);
          sender.GetShootStateInfo().GetCurWeaponInfo().CurCritical = 0;
          float shootCRIDAM = shootDAM * (1.0f + pow);
          shootDAM = shootCRIDAM;
          isCritical = true;
        }
        /* 是否有伤害吸收效果 */
        if(receiver.IsHaveStateFlag(CharacterState_Type.CST_DamageAbsorb)){}
        /* 是否有伤害反弹效果 */

        return (int)shootDAM;
      }
      return 0;
    }
    public static int CalcImpactDamage(CharacterInfo sender, CharacterInfo receiver, int skillId, int damageFromConfig)
    {
      if (null == sender || -1 == skillId) return damageFromConfig;

      SkillLogicData skillLogicData = (SkillLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_SKILL, skillId);
      if (null == skillLogicData) return 0;
      float skillCoefficient = skillLogicData.SkillCoefficient;
      int realDamage = (int)(damageFromConfig + sender.GetActualProperty().EnergyIntensity * skillCoefficient);
      float tay = receiver.GetActualProperty().EnergyArmor;
      realDamage = realDamage - (int)(realDamage * tay / (tay + 100.00));
      // TODO：伤害吸收修正
      // TODO: 伤害反弹修正
      return realDamage;
    }
  }
}
