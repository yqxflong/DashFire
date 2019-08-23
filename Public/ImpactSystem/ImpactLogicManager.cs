using System;
using System.Collections.Generic;

namespace DashFire
{
  public sealed class ImpactLogicManager
  {
    public enum ImpactLogicId {
      ImpactLogic_General = 1,
      ImpactLogic_Invincible = 2,
    }

    public IImpactLogic GetImpactLogic(int id)
    {
      IImpactLogic logic = null;
      if (m_ImpactLogics.ContainsKey(id))
        logic = m_ImpactLogics[id];
      return logic;
    }

    private ImpactLogicManager(){
      m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_General, new ImpactLogic_General());
      m_ImpactLogics.Add((int)ImpactLogicId.ImpactLogic_Invincible, new ImpactLogic_Invincible());
    }

    public static ImpactLogicManager Instance
    {
      get { return s_Instance; }
    }

    private Dictionary<int, IImpactLogic> m_ImpactLogics = new Dictionary<int, IImpactLogic>();
    private static ImpactLogicManager s_Instance = new ImpactLogicManager();
  }
}
