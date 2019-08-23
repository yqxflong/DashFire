using System;
using System.Collections.Generic;

namespace DashFire
{
  public sealed class ImpactSystem
  {
    public void Tick(CharacterInfo obj)
    {
      List<ImpactInfo> impactInfos = obj.GetSkillStateInfo().GetAllImpact();
      int ct = impactInfos.Count;
      for (int i = ct - 1; i >= 0; --i) {
        ImpactInfo info = impactInfos[i];
        IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(info.ConfigData.ImpactLogicId);
        if (info.m_IsActivated) {
          if (null != logic) {
            logic.Tick(obj, info.m_ImpactId);
          }
        } else {
          obj.GetSkillStateInfo().RemoveImpact(info.m_ImpactId);
        }
      }
    }
    public void SendImpactToCharacter(CharacterInfo sender, int impactId, int targetId)
    {
      //LogSystem.Debug("character {0} send impact {1} to character {2}", sender.GetId(), impactId, targetId);
      if (null != sender) {
        CharacterInfo target = sender.SceneContext.GetCharacterInfoById(targetId);
        if (null != target) {
          ImpactLogicData impactLogicData = (ImpactLogicData)SkillConfigProvider.Instance.ExtractData(SkillConfigType.SCT_IMPACT, impactId);
          if (null != impactLogicData) {
            IImpactLogic logic = ImpactLogicManager.Instance.GetImpactLogic(impactLogicData.ImpactLogicId);
            if (null != logic) {
              ImpactInfo impactInfo = new ImpactInfo();
              impactInfo.m_IsActivated = true;
              impactInfo.m_ImpactId = impactLogicData.ImpactId;
              impactInfo.ConfigData = impactLogicData;
              impactInfo.m_StartTime = TimeUtility.GetServerMilliseconds();
              impactInfo.m_ImpactDuration = impactLogicData.ImpactTime;
              impactInfo.m_ImpactSenderId = sender.GetId();
              impactInfo.m_HasEffectApplyed = false;
              target.GetSkillStateInfo().AddImpact(impactInfo);
            }
          }
        }
      }
    }

    public static ImpactSystem Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static ImpactSystem s_Instance = new ImpactSystem();
  }
}
