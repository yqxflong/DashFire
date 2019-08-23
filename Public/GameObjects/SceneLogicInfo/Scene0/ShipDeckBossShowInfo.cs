using System;
using System.Collections.Generic;

namespace DashFire
{
  public class ShipDeckBossShowInfo
  {
    public enum PhaseCode : int
    {
      OnStage,
      ShootingEveryone,
      FlyingHome,
      OffStage,
      Done,
    }

    public int Phase { get; set; }
    public int BossId { get; private set; }
    public int BossDataId { get; private set; }
    public int BossSkillId { get; private set; }
    public ScriptRuntime.Vector3 BossPos { get; private set; }
    public long LastSkillTime { get; set; }
    public float SkillInterval { get; private set; }

    public ShipDeckBossShowInfo(string pstr)
    {
      string[] p = pstr.Split(':');
      BossDataId = Int32.Parse(p[0]);
      BossSkillId = Int32.Parse(p[1]);
      string[] v = p[2].Split(' ');
      if (3 == v.Length) {
        BossPos = new ScriptRuntime.Vector3(
          float.Parse(v[0]),
          float.Parse(v[1]),
          float.Parse(v[2]));
      }
      SkillInterval = float.Parse(p[3]);
      LastSkillTime = 0;

      BossId = -1;
      Phase = (int)PhaseCode.OnStage;
    }

    public void Terminate()
    {
      Phase = (int)PhaseCode.OffStage;
    }

    public NpcInfo Boss(SceneLogicInfo info)
    {
      return info.NpcManager.GetNpcInfo(BossId);
    }

    public void SetBoss(NpcInfo boss)
    {
      BossId = boss.GetId();
    }
  }
}