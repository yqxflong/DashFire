using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public enum SearchLightStatus
  {
    kPatrol,
    kFollow,
    kExFollow,
    kReturn,
  }

  public delegate void TriggerCallback(SceneLogicInfo logic_info, SearchLightLogicInfo search_light_info, int unit_id);
  public delegate void PathChangeHandler(SceneLogicInfo logic_info, Vector3 cur_pos, ISpaceObject target_obj, Vector3 target_pos, float speed);

  public class SearchLightLogicInfo
  {
    public uint SceneObjID;
    public string Model;
    public Vector3 BirthPoint;
    public Vector3 CurPos
    {
      set { m_CurPos = value; }
      get { return m_CurPos; }
    }
    public float FaceDir;
    public float SearchRadius;
    public List<Vector3> PatrolPath;
    public float PatrolSpeed;
    public float FollowSpeed;
    public long ExFollowTimeMs;
    public long TriggerInterval;
    public bool IsCreated;
    public float MaxFollowRange { set { m_MaxFollowRange = value; } }
    public List<int> NpcList;
    public long CreateNpcInterval;
    public Vector3 NpcBirthPoint;
    public TriggerCallback OnCreateNpc;
    public SceneLogicInfo LogicInfo;
    public ISpaceObject FollowObject { get { return m_CurFollowObj; } }
    public PathChangeHandler EventPathChange;

    public SearchLightLogicInfo()
    {
      OnCreateNpc = null;
      IsCreated = false;
      PatrolPath = new List<Vector3>();
      m_CurPathPointIndex = 0;
      m_IsReverse = false;
      m_Status = SearchLightStatus.kPatrol;
      m_NextTriggerTime = 0;
    }

    public void SetMoveInfo(Vector3 cur_pos, ISpaceObject target, Vector3 target_pos, float speed)
    {
      m_CurPos = cur_pos;
      m_CurFollowObj = target;
      m_CurTargetPos = target_pos;
      m_CurSpeed = speed;
      if (m_IsReturned && m_CurFollowObj != null) {
        m_BeginFollowPos = m_CurPos;
        m_IsReturned = false;
      }
      if (m_CurFollowObj == null && !m_IsReturned && Geometry.IsSamePoint(m_BeginFollowPos, cur_pos)) {
        m_IsReturned = true;
      }
    }

    public void ClientTick(long deltaTime)
    {
      if (m_CurFollowObj != null) {
        if (Geometry.IsSamePoint(m_CurPos, m_CurFollowObj.GetPosition())) { return; }
        Vector3 new_pos = Move(m_CurPos, m_CurFollowObj.GetPosition(), m_CurSpeed, deltaTime);
        if (Geometry.DistanceSquare(new_pos, m_BeginFollowPos) <= m_MaxFollowRange * m_MaxFollowRange) {
          m_CurPos = new_pos;
        }
      } else {
        if (Geometry.IsSamePoint(m_CurPos, m_CurTargetPos)) { return; }
        m_CurPos = Move(m_CurPos, m_CurTargetPos, m_CurSpeed, deltaTime);
      }
    }

    public void Tick(DashFireSpatial.ISpatialSystem spatial_system, long deltaTime)
    {
      UpdateStatus(spatial_system);
      switch (m_Status) {
        case SearchLightStatus.kPatrol:
          Vector3 target_pos = GetCurTargetPos();
          CheckPathChange(null, target_pos, PatrolSpeed);
          m_CurPos = Move(m_CurPos, target_pos, PatrolSpeed, deltaTime);
          break;
        case SearchLightStatus.kFollow:
        case SearchLightStatus.kExFollow:
          CheckPathChange(m_CurFollowObj, new Vector3(), FollowSpeed);
          Follow(deltaTime);
          break;
        case SearchLightStatus.kReturn:
          CheckPathChange(null, m_BeginFollowPos, FollowSpeed);
          Back(deltaTime);
          break;
      }
      if (m_IsTriggered) {
        CreateNpc();
      }
    }

    public void CheckPathChange(ISpaceObject target, Vector3 target_pos, float speed)
    {
      if (target != null) {
        if (m_LastFollowObj == null || target.GetID() != m_LastFollowObj.GetID()) {
          m_LastFollowObj = target;
          if (EventPathChange != null) {
            EventPathChange(LogicInfo, m_CurPos, target, target_pos, speed);
          }
          //LogSystem.Debug("---search light: path change follow obj {0}", target.GetID());
        }
        return;
      }
      if (m_LastFollowObj != null || !Geometry.IsSamePoint(target_pos, m_LastTargetPos)) {
        m_LastTargetPos = target_pos;
        m_LastFollowObj = null;
        if (EventPathChange != null) {
          EventPathChange(LogicInfo, m_CurPos, target, target_pos, speed);
        }
        //LogSystem.Debug("---search light: path change target pos {0}", target_pos);
      }
    }

    private void UpdateStatus(DashFireSpatial.ISpatialSystem spatial_system)
    {
      List<ISpaceObject> in_objs = spatial_system.GetObjectInCircle(m_CurPos, SearchRadius);
      switch (m_Status) {
        case SearchLightStatus.kPatrol:
        case SearchLightStatus.kReturn:
          ISpaceObject target = GetFirstUser(in_objs);
          if (target != null) {
            BeginFollow(target);
          }
          break;
        case SearchLightStatus.kFollow:
          if (!IsContainObj(in_objs, m_CurFollowObj)) {
            ISpaceObject new_target = GetFirstUser(in_objs);
            if (new_target != null) {
              BeginFollow(new_target);
            } else {
              BeginExFollow();
            }
          }
          break;
        case SearchLightStatus.kExFollow:
          if (TimeUtility.GetServerMilliseconds() > m_ExFollowTime + ExFollowTimeMs) {
            BeginReturn();
          }
          break;
      }
    }

    private void CreateNpc()
    {
      if (!m_IsTriggered) { return; }
      if (m_NextNpcIndex >= NpcList.Count) {
        m_IsTriggered = false;
        return;
      }
      long now = TimeUtility.GetServerMilliseconds();
      if (now >= m_NextNpcCreateTime) {
        int npc_unit_id = NpcList[m_NextNpcIndex];
        m_NextNpcIndex++;
        m_NextNpcCreateTime = now + CreateNpcInterval;
        if (OnCreateNpc != null) {
          OnCreateNpc(LogicInfo, this, npc_unit_id);
        }
      }
    }

    private void OnTrigger()
    {
      m_IsTriggered = true;
      m_NextNpcIndex = 0;
      m_NextNpcCreateTime = TimeUtility.GetServerMilliseconds();
    }

    private void BeginFollow(DashFireSpatial.ISpaceObject targetObj)
    {
      if (m_Status == SearchLightStatus.kPatrol) {
        m_BeginFollowPos = m_CurPos;
      }
      m_CurFollowObj = targetObj;
      if (m_NextTriggerTime == 0) {
        m_NextTriggerTime = TimeUtility.GetServerMilliseconds() + TriggerInterval;
        OnTrigger();
      }
      m_Status = SearchLightStatus.kFollow;
    }

    private void BeginReturn()
    {
      m_Status = SearchLightStatus.kReturn;
    }

    private void BeginExFollow()
    {
      m_Status = SearchLightStatus.kExFollow;
      m_ExFollowTime = TimeUtility.GetServerMilliseconds();
    }

    private bool IsContainObj(List<ISpaceObject> obj_list, ISpaceObject target)
    {
      foreach (ISpaceObject obj in obj_list) {
        if (obj.GetObjType() == SpatialObjType.kUser && obj.GetID() == target.GetID()) {
          return true;
        }
      }
      return false;
    }

    private ISpaceObject GetFirstUser(List<ISpaceObject> obj_list)
    {
      foreach (ISpaceObject obj in obj_list) {
        if (obj.GetObjType() == SpatialObjType.kUser) {
          return obj;
        }
      }
      return null;
    }

    private void Follow(long deltaTime)
    {
      long now = TimeUtility.GetServerMilliseconds();
      if (now >= m_NextTriggerTime) {
        m_NextTriggerTime = now + TriggerInterval;
        OnTrigger();
      }
      if (Geometry.IsSamePoint(m_CurPos, m_CurFollowObj.GetPosition())) { return; }
      Vector3 new_pos = Move(m_CurPos, m_CurFollowObj.GetPosition(), FollowSpeed, deltaTime);
      if (Geometry.DistanceSquare(new_pos, m_BeginFollowPos) <= m_MaxFollowRange*m_MaxFollowRange) {
        m_CurPos = new_pos;
      }
    }

    private void Back(long deltaTime)
    {
      m_CurPos = Move(m_CurPos, m_BeginFollowPos, FollowSpeed, deltaTime);
      if (Geometry.IsSamePoint(m_CurPos, m_BeginFollowPos)) {
        m_Status = SearchLightStatus.kPatrol;
        return; 
      }
    }

    private Vector3 Move(Vector3 cur_pos, Vector3 end_pos, float speed, long deltaTime)
    {
      Vector3 result = cur_pos;
      Vector3 move_vect = end_pos - cur_pos;
      float move_dir = Geometry.GetDirFromVector(move_vect);
      float distance = (speed * deltaTime) / 1000;
      float len = move_vect.Length();
      if (Math.Abs(len) <= Math.Abs(distance)) {
        result = end_pos;
      } else {
        result.Z = (float)(cur_pos.Z + distance * Math.Cos(move_dir));
        result.X = (float)(cur_pos.X + distance * Math.Sin(move_dir));
      }
      return result;
    }

    private Vector3 GetCurTargetPos()
    {
      Vector3 cur_target_pos = PatrolPath[m_CurPathPointIndex];
      if (Math.Abs(cur_target_pos.X - m_CurPos.X) <= 0.0001 &&
          Math.Abs(cur_target_pos.Z - m_CurPos.Z) <= 0.0001) {
        if (!m_IsReverse) {
          m_CurPathPointIndex++;
          if (m_CurPathPointIndex >= PatrolPath.Count) {
            m_CurPathPointIndex = PatrolPath.Count - 2;
            m_IsReverse = true;
          }
        } else {
          m_CurPathPointIndex--;
          if (m_CurPathPointIndex < 0) {
            m_CurPathPointIndex = 1;
            m_IsReverse = false;
          }
        }
        return PatrolPath[m_CurPathPointIndex];
      } else {
        return cur_target_pos;
      }
    }

    //private attributes
    private long m_ExFollowTime;
    private bool m_IsReturned = true;
    private Vector3 m_LastTargetPos;
    private Vector3 m_CurTargetPos;
    private float m_CurSpeed;
    private ISpaceObject m_LastFollowObj = null;
    private Vector3 m_CurPos;
    private long m_NextTriggerTime;
    private bool m_IsTriggered = false;
    private int m_NextNpcIndex = 0;
    private long m_NextNpcCreateTime = 0;

    private SearchLightStatus m_Status;
    private int m_CurPathPointIndex;
    private bool m_IsReverse;

    // follow and return attribute
    private DashFireSpatial.ISpaceObject m_CurFollowObj;
    private Vector3 m_BeginFollowPos;
    private float m_MaxFollowRange;
  }
}
