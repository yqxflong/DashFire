using System;
using System.Collections.Generic;
using ScriptRuntime;

namespace DashFire
{
  public class ShipDeckFightInfo
  {
    #region phase code definition
    public enum PhaseCode : int
    {
      Init,          // parse config files, add lair logic
      MonitorRegion, // monitor a region for players to be found
      Active,        // active lairs and boss logic
      MonitorLairs,  // monitor the lairs to see when are they die out
      Done,
    }
    #endregion phase code definition

    #region properties
    public List<WormLair> LairInfos { get; private set; }
    public Vector3 MonitorCentre { get; private set; }
    public float MonitorRadius { get; private set; }
    public int Phase { get; set; }
    public ShipDeckBossShowInfo BossOnShipDeck { get; set; }
    #endregion properties

    /// <summary>
    /// format: monitor-centre | monitorradius | boss-dataid:boss-skillid | N-lairs [| lairinfo ]
    /// NOTE: lairinfo is a string containing all information to create a worm-lair
    /// which is separated by semicolon.
    /// </summary>
    /// <param name="p"></param>
    public ShipDeckFightInfo(string[] p)
    {
      Phase = (int)PhaseCode.Init;
      LairInfos = new List<WormLair>();
      System.Diagnostics.Debug.Assert(p.Length > 4);
      int offset = 0;
      MonitorCentre = Converter.ConvertVector3D(p[offset++]);
      MonitorRadius = float.Parse(p[offset++]);
      BossOnShipDeck = new ShipDeckBossShowInfo(p[offset++]);
      int count  = Int32.Parse(p[offset++]);
      for (int i = 0; i < count; ++i)
        LairInfos.Add(new WormLair(p[offset++]));
    }
  }
}