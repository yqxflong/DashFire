using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class PvpBarracksLogicInfo
  {
    public int[] m_Npcs = null;
    public float m_TriggerInterval = 0;
    public int m_Pos = 0;//0--蓝上 1--蓝中 2--蓝下 3--红上 4--红中 5--红下
    public Vector3[] m_PatrolPoints = null;
    public List<int> m_NpcObjs = new List<int>();
    public int m_SuperNpc = 0;
    public int m_EngineerNpc = 0;
    public float m_LastTriggerTime = 0;
    public int m_BatchCount = 0;
    public Queue<int> m_AddNpcQueue = new Queue<int>();
  }
}
