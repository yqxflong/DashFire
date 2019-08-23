using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public enum MissionStatus
  {
    kNotStarted,
    kExecuting,
    kFinished,
    kFailed,
  }

  public class MissionManager
  {
    public MissionManager() { }
    public void AddMission(Mission mission)
    {
      if (!missions_.ContainsKey(mission.GetID())) {
        missions_.Add(mission.GetID(), mission); 
      }
    }
    public Mission GetMission(int id)
    {
      if (missions_.ContainsKey(id)) {
        return missions_[id];
      }
      return null;
    }

    public void Reset()
    {
      missions_.Clear();
    }

    public void SetMissionStatus(int mission_id, MissionStatus status)
    {
      if (missions_.ContainsKey(mission_id)) {
        missions_[mission_id].SetStatus(status);
      }
    }

    private MyDictionary<int, Mission> missions_ = new MyDictionary<int, Mission>();
  }

  public class Mission
  {
    public Mission(int id, string title)
    {
      mission_id_ = id;
      title_ = title;
      status_ = MissionStatus.kNotStarted;
    }

    public void AddGoal(Goal goal)
    {
      if (!mission_goals_.ContainsKey(goal.GetID())) {
        mission_goals_.Add(goal.GetID(), goal);
      }
    }

    public Goal GetGoal(int id)
    {
      if (mission_goals_.ContainsKey(id)) {
        return mission_goals_[id];
      }
      return null;
    }

    // setter------------------
    public void SetStatus(MissionStatus status) { status_ = status; }

    // getter------------------
    public int GetID() { return mission_id_; }
    public string GetTitle() { return title_; }
    public MissionStatus GetStatus() { return status_; }
    public MyDictionary<int, Goal> GetMissionGoals() { return mission_goals_; }

    //private attribtes--------------------------------------
    private int mission_id_;
    private MissionStatus status_;
    private string title_;
    private MyDictionary<int, Goal> mission_goals_ = new MyDictionary<int, Goal>();
  }

  public class Goal
  {
    public Goal(int id, string describe, int total_number, int cur_number)
    {
      goal_id_ = id;
      goal_title_ = describe;
      total_number_ = total_number;
      cur_number_ = cur_number;
    }

    // setter-----------------
    public void SetCurProgress(int cur_number) { cur_number_ = cur_number; }

    // getter-----------------
    public int GetID() { return goal_id_; }
    public string GetGoalTitle() { return goal_title_; }
    public int GetCurProgress() { return cur_number_; }
    public int GetTotalNumber() { return total_number_; }

    // private attributes-------------------------------------
    private int goal_id_ = 0;
    private string goal_title_ = "";
    private int total_number_ = 0;
    private int cur_number_ = 0;
  }
}
