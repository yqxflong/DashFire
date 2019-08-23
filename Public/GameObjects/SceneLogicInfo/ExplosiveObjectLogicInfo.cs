using System;

namespace DashFire
{
  public class ExplosiveObjectLogicInfo
  {
    public enum PhaseCode : int
    {
      Init,    // config setup and npc locating
      Monitor, // monitor the npc obj and emit a when it's dead
      Done,
    }

    public int ExplosiveDataId { get; private set; }
    public string ExplodeEvent { get; private set; }
    public int Phase { get; set; }

    public ExplosiveObjectLogicInfo(string[] p)
    {
      System.Diagnostics.Debug.Assert(p.Length == 2);
      ExplosiveDataId = int.Parse(p[0]);
      ExplodeEvent = p[1];
      Phase = (int)PhaseCode.Init;
    }

    public NpcInfo ExplodeNpc(SceneLogicInfo info)
    {
      return info.NpcManager.GetNpcInfo(npc_id_);
    }

    public void SetExplodeNpc(NpcInfo npc)
    {
      npc_id_ = npc.GetId();
    }

    private int npc_id_ = -1;
  }
}