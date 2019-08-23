using System;
using System.Collections.Generic;
using ScriptRuntime;

namespace DashFire
{
  public class WormLair
  {
    public enum PhaseCode : int
    {
      Init,
      Monitor,
      Active,
      Done,
    }

    #region properties
    public int LairDataId { get; private set; }
    public long GenerateIntervalMilliseconds { get; private set; }
    public int WormCount { get { return worm_dataids_.Count; } }
    public Vector3 MonitorCentre { get; private set; }
    public float MonitorRadius { get; private set; }
    public int LairId { get; private set; }

    public int Phase { get; set; }
    public long ElapsedMilliseconds { get; set; }
    public List<int> ActiveWorms { get; set; }

    public bool Active
    {
      get { return PhaseCode.Active == (PhaseCode)Phase; }
      set
      {
        if (!Active)
          Phase = (int)PhaseCode.Active;
      }
    }
    #endregion properties

    public WormLair(int lair_dataid, long gim, Vector3? monitor_centre, float? monitor_radius, int[] worm_dataids)
    {
      LairDataId = lair_dataid;
      GenerateIntervalMilliseconds = gim;
      MonitorCentre = monitor_centre ?? default(Vector3);
      MonitorRadius = monitor_radius ?? 0;
      worm_dataids_ = new List<int>(worm_dataids);
      Init();
    }

    public WormLair(string[] p)
    {
      // lair-dataid | gim | monitor-centre | monitor-radius | worm-dataid [worm-dataid ]*
      System.Diagnostics.Debug.Assert(p.Length == 5);
      int offset = 0;
      LairDataId = Int32.Parse(p[offset++]);
      GenerateIntervalMilliseconds = Int32.Parse(p[offset++]);
      ParseMonitorParam(p, ref offset);
      ParseWorms(p, ref offset);
      Init();
    }

    public WormLair(string p) : this(p.Split(':')) { }

    public int NextWormDataId()
    {
      int dataid = worm_dataids_[worm_index_];
      if (++worm_index_ >= worm_dataids_.Count)
        worm_index_ = 0;
      return dataid;
    }

    public NpcInfo Lair(SceneLogicInfo info)
    {
      return info.NpcManager.GetNpcInfo(LairId);
    }

    public void SetLair(NpcInfo lair)
    {
      LairId = null != lair ? lair.GetId() : -1;
    }

    private void ParseWorms(string[] p, ref int offset)
    {
      worm_dataids_ = Converter.ConvertStringList(p[offset++]).ConvertAll<int>(s => Int32.Parse(s));
    }

    private void ParseMonitorParam(string[] p, ref int offset)
    {
      MonitorCentre = p[offset] != "null" ? Converter.ConvertVector3D(p[offset]) : default(Vector3);
      ++offset;
      MonitorRadius = p[offset] != "null" ? float.Parse(p[offset]) : 0;
      ++offset;
    }

    private void Init()
    {
      ElapsedMilliseconds = GenerateIntervalMilliseconds; // create the first worm at the instant of activation
      ActiveWorms = new List<int>();
      Phase = (int)PhaseCode.Init;
      LairId = -1;
    }

    private List<int> worm_dataids_;
    private int worm_index_ = 0;
  }
}