using System;
using System.Collections.Generic;

namespace DashFire
{
  public sealed class ImpactViewManager
  {
    public void Init()
    {
      //添加view实例到m_Views列表中
    }

    private List<object> m_Views = new List<object>();

    public static ImpactViewManager Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static ImpactViewManager s_Instance = new ImpactViewManager();
  }
}
