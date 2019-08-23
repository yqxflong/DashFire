using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class AiData_ForMoveCommand
  {
    public List<Vector3> WayPoints { get; set; }
    public int Index { get; set; }
    public bool IsFinish { get; set; }
    public float EstimateFinishTime { get; set; }

    public AiData_ForMoveCommand(List<Vector3> way_points)
    {
      WayPoints = way_points;
      Index = 0;
      EstimateFinishTime = 0;
      IsFinish = false;
    }
  }
  public class AiData_ForPatrolCommand
  {
    public bool IsLoopPatrol
    {
      get { return m_IsLoopPatrol; }
      set { m_IsLoopPatrol = value; }
    }
    public DashFire.AiPathData PatrolPath
    {
      get { return m_PatrolPath; }
    }
    public DashFire.AiPathData FoundPath
    {
      get { return m_FoundPath; }
    }

    private AiPathData m_PatrolPath = new AiPathData();
    private AiPathData m_FoundPath = new AiPathData();
    private bool m_IsLoopPatrol = false;
  }
  public class AiData_PveNpc_Trap
  {
    public bool IsTriggered
    {
      get { return m_IsTriggered; }
      set { m_IsTriggered = value; }
    }
    public float RadiusOfTrigger
    {
      get { return m_RadiusOfTrigger; }
      set { m_RadiusOfTrigger = value; }
    }
    public float RadiusOfDamage
    {
      get { return m_RadiusOfDamage; }
      set { m_RadiusOfDamage = value; }
    }
    public int DamageCount
    {
      get { return m_DamageCount; }
      set { m_DamageCount = value; }
    }
    public int ImpactToMyself
    {
      get { return m_ImpactToMyself; }
      set { m_ImpactToMyself = value; }
    }
    public int Impact1ToTarget
    {
      get { return m_Impact1ToTarget; }
      set { m_Impact1ToTarget = value; }
    }
    public int Impact2ToTarget
    {
      get { return m_Impact2ToTarget; }
      set { m_Impact2ToTarget = value; }
    }
    public int HideImpact
    {
      get { return m_HideImpact; }
      set { m_HideImpact = value; }
    }

    private bool m_IsTriggered = false;
    private float m_RadiusOfTrigger = 2;
    private float m_RadiusOfDamage = 4;
    private int m_DamageCount = 1;
    private int m_ImpactToMyself = 10504;
    private int m_Impact1ToTarget = 10502;
    private int m_Impact2ToTarget = 10503;
    private int m_HideImpact = 10514;
  }
  public class AiData_PveNpc_General
  {
    public long Time
    {
      get { return m_Time; }
      set { m_Time = value; }
    }
    public long FindPathTime
    {
      get { return m_FindPathTime; }
      set { m_FindPathTime = value; }
    }
    public DashFire.AiPathData FoundPath
    {
      get { return m_FoundPath; }
    }

    private long m_Time = 0;
    private long m_FindPathTime = 0;
    private AiPathData m_FoundPath = new AiPathData();
  }
  public class AiData_PveNpc_Monster : AiData_PveNpc_General
  {
    public int Skill
    {
      get { return m_Skill; }
      set { m_Skill = value; }
    }
    public int ShootDistance
    {
      get { return m_ShootDistance; }
      set { m_ShootDistance = value; }
    }

    private int m_Skill = 10165;//飞扑
    private int m_ShootDistance = 5;
  }
  public class AiData_PveNpc_Monster_CloseCombat : AiData_PveNpc_General
  {
    public int FastMoveImpact
    {
      get { return m_Impact1; }
      set { m_Impact1 = value; }
    }
    public int PreAttackImpact
    {
      get { return m_Impact2; }
      set { m_Impact2 = value; }
    }
    public int PreAttackDistance
    {
      get { return m_PreAttackDistance; }
      set { m_PreAttackDistance = value; }
    }
    public int StandShootDistance
    {
      get { return m_StandShootDistance; }
      set { m_StandShootDistance = value; }
    }

    public int TestFlag { set; get; }

    private int m_Impact1 = 20020;//战斗移动，加速
    private int m_Impact2 = 20022;//预热
    private int m_PreAttackDistance = 15;
    private int m_StandShootDistance = 3;
  }
  public class AiData_PveNpc_RadioactiveMan : AiData_PveNpc_General
  {
    public long LeftAdjustMoveTime
    {
      get { return m_LeftAdjustMoveTime; }
      set { m_LeftAdjustMoveTime = value; }
    }
    public ScriptRuntime.Vector3 AdjustMoveTarget
    {
      get { return m_AdjustMoveTarget; }
      set { m_AdjustMoveTarget = value; }
    }
    public int Skill
    {
      get { return m_Skill; }
      set { m_Skill = value; }
    }
    public int Impact
    {
      get { return m_Impact; }
      set { m_Impact = value; }
    }
    public int MinDistanceSquareToSkill
    {
      get { return m_MinDistanceSquareToSkill; }
      set { m_MinDistanceSquareToSkill = value; }
    }
    public int MinHpToImpact
    {
      get { return m_MinHpToImpact; }
      set { m_MinHpToImpact = value; }
    }
    public int MoveProbability
    {
      get { return m_MoveProbability; }
      set { m_MoveProbability = value; }
    }

    private long m_LeftAdjustMoveTime = 0;
    private Vector3 m_AdjustMoveTarget = new Vector3();
    private int m_Skill = 10167;//近身击退
    private int m_Impact = 52;//暴怒
    private int m_MinDistanceSquareToSkill = 4;
    private int m_MinHpToImpact = 10;
    private int m_MoveProbability = 60;
  }
  public class AiData_PveNpc_Soldier : AiData_PveNpc_General
  {
    public int Impact
    {
      get { return m_Impact; }
      set { m_Impact = value; }
    }
    public int StandShootDistance
    {
      get { return m_StandShootDistance; }
      set { m_StandShootDistance = value; }
    }

    private int m_Impact = 20021;//战斗移动，减速
    private int m_StandShootDistance = 8;
  }
  public class AiData_PveBoss_UcaCommander : AiData_PveNpc_General
  {
    public const int c_FarDistance = 20;
    public const int c_NearDistance = 10;
    public int Skill
    {
      get { return m_Skill; }
      set { m_Skill = value; }
    }
    public int MaxCount
    {
      get { return m_MaxCount; }
      set { m_MaxCount = value; }
    }
    public int CurCount
    {
      get { return m_CurCount; }
      set { m_CurCount = value; }
    }

    private int m_Skill = 0;
    private int m_MaxCount = 0;
    private int m_CurCount = 0;
  }
  public class AiData_PvpNpc_General
  {
    public long Time
    {
      get { return m_Time; }
      set { m_Time = value; }
    }
    public long ThinkingTime
    {
      get { return m_ThinkingTime; }
      set { m_ThinkingTime = value; }
    }
    public DashFire.AiPathData PatrolPath
    {
      get { return m_PatrolPath; }
    }
    public DashFire.AiPathData FoundPath
    {
      get { return m_FoundPath; }
    }

    private long m_ThinkingTime = 0;
    private long m_Time = 0;
    private AiPathData m_PatrolPath = new AiPathData();
    private AiPathData m_FoundPath = new AiPathData();
  }

  public class AiData_Demo_Melee : AiData_PveNpc_General {

    public long WalkTime {
      get { return m_WalkTime; }
      set { m_WalkTime = value; }
    }

    public long WaitTime {
      get { return m_WaitTime; }
      set { m_WaitTime = value; }
    }

    public bool HasMeetEnemy {
      get { return m_HasMeetEnemy; }
      set { m_HasMeetEnemy = value; }
    }

    public int SkillToCast {
      get { return m_SkillToCast; }
      set { m_SkillToCast = value; }
    }

    public bool HasPatrolData {
      get { return m_HasPatrolData; }
      set { m_HasPatrolData = value; }
    }

    public Animation_Type MeetEnemyAnim {
      get { return m_MeetEnemyAnim; }
      set { m_MeetEnemyAnim = value; }
    }

    private bool m_HasMeetEnemy = false;
    private Animation_Type m_MeetEnemyAnim = Animation_Type.AT_None;
    private long m_WalkTime = 0;
    private long m_WaitTime = 0;
    private int m_SkillToCast = 0;
    private bool m_HasPatrolData = false;

  }
}
