using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class IceHoleLogicInfo
  {
    public Vector3 m_Center;
    public float m_RadiusOfTrigger = 0;
    public float m_RadiusOfDamage = 0;
    public float m_RadiusOfObstacle = 0;
    public int m_Damage = 0;
    public float m_Duration = 0;
    public string m_IceHoleName = "";
    public long m_Time = 0;
    public int m_TriggerEntityId = -1;
    public bool m_IsTriggered = false;
    public bool m_IsExploded = false;
    public List<int> m_Objs = new List<int>();

    public int m_BarrageId = 0;
    public string m_LandMarkEffect = "";
    public string m_BlastEffect = "";
    public string m_BlastSoundEffect = "";
    public float m_BarrageStartDistance = 0.0f;
    public ScriptRuntime.Vector3 m_ShootPos;
    public float m_ShootDir = 0.0f;
  }
}
