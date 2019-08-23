using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  /// <summary>
  /// 这里放客户端与服务器存在差异的变量值，供各公共模块使用（如果是各模块所需的逻辑数据，则不要放在这里，独立写读表器）。
  /// </summary>
  public class GlobalVariables
  {
    public bool IsClient
    {
      get
      {
        return m_IsClient;
      }
      set
      {
        m_IsClient = value;
      }
    }
    public bool IsDebug
    {
      get { return m_IsDebug; }
      set { m_IsDebug = value; }
    }
    public bool IsFullClient
    {
      get { return m_IsFullClient; }
      set { m_IsFullClient = value; }
    }
    public bool IsMobile
    {
      get { return m_IsMobile; }
      set { m_IsMobile = value; }
    }

    private bool m_IsClient = false;
    private bool m_IsDebug = false;
    private bool m_IsFullClient = true;
    private bool m_IsMobile = false;
    
    public static GlobalVariables Instance
    {
      get { return s_Instance; }
    }
    private static GlobalVariables s_Instance = new GlobalVariables();
    
    public static int GetUnitIdByCampId(int campid)
    {
      if (campid == (int)CampIdEnum.Blue)
        return 20001;
      else
        return 20002;
    }
  }
}
