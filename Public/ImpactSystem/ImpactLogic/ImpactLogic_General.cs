using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire{
  class ImpactLogic_General : IImpactLogic {

    public void Tick(CharacterInfo character, int impactId) {
      ImpactInfo impactInfo = character.GetSkillStateInfo().GetImpactInfoById(impactId);
      if (null != impactInfo) {
        if (impactInfo.m_IsActivated) {
          float damageDelayTime = float.Parse(impactInfo.ConfigData.ExtraParams[0]);
          if (TimeUtility.GetServerMilliseconds() > impactInfo.m_StartTime + damageDelayTime * 1000 && !impactInfo.m_HasEffectApplyed) {
            int damage = int.Parse(impactInfo.ConfigData.ExtraParams[1]);
            if (!character.IsHaveStateFlag(CharacterState_Type.CST_Invincible)) {
              ApplyDamage(impactInfo.m_ImpactSenderId, character, damage);
            }
            impactInfo.m_HasEffectApplyed = true;
            if (impactInfo.ConfigData.ExtraParams.Count >= 3) {
              int rage = int.Parse(impactInfo.ConfigData.ExtraParams[2]);
              if (character.IsUser) {
                character.SetRage(Operate_Type.OT_Relative, rage);
              } else {
                CharacterInfo user = character.SceneContext.GetCharacterInfoById(impactInfo.m_ImpactSenderId);
                if (user != null && user.IsUser) {
                  user.SetRage(Operate_Type.OT_Relative, rage);
                }
              }
            }
          }
          if (TimeUtility.GetServerMilliseconds() > impactInfo.m_StartTime + impactInfo.m_ImpactDuration) {
            impactInfo.m_IsActivated = false;
          }
        }
      }
    }

    private void ApplyDamage(int senderId, CharacterInfo character, int damage) {
      if (null != character && !character.IsDead()) {
        bool isCritical = false;
        bool isKiller = false;
        if (Helper.Random.Next() <= 30) {
          isCritical = true;
          damage =(int)(damage * 1.5f);
        }
        character.SetHp(Operate_Type.OT_Relative, damage);
        if (character.Hp <= 0) {
          isKiller = true;
        }
        character.SetAttackerInfo(senderId, 0, isKiller, !isCritical, isCritical, damage, 0);
      }
    }
  }
}
