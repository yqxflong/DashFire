using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class PvpHeadquartersLogicInfo
  {
    public Vector3 m_Center;
    public float m_RadiusOfTrigger = 0;
    public int m_LostNpc = 0;
    public int m_CampId = 0;
    public int m_HpRecover = 0;
    public int m_NpRecover = 0;
    public int[] m_UpTowers = new int[3];
    public int[] m_MiddleTowers = new int[5];
    public int[] m_BottomTowers = new int[3];
    public List<int> m_UserObjs = new List<int>();
  }
}
