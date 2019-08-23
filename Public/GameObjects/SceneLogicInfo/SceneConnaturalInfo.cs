using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class MedicineCabParams
  {
    public uint ObjId;
    public int EffectId;
    public bool IsCreated = false;
    public long SleepTime = 60000;
    public long CurTime = 0;
    public bool IsSleeping = false;
  }

  public class LaserParams
  {
    public uint MainObjId;
    public uint OtherObjId;
    public uint LowerLaserId;
    public uint MidLaserId;
    public uint UpLaserId;
    public long ChargeTime = 0;
    public long FireTime = 0;
    public long OpenTime = 0;
    public long CloseTime = 0;
    public int Damage = 0;
    public bool IsCreated = false;
    public int LaserState = 1; // 0-Charge, 1-Fire
    public List<LaserTargetInfo> LaserTargets = new List<LaserTargetInfo>();
  }

  public class LaserTargetInfo
  {
    public CharacterInfo TargetEntity;
    public long LastDamageTime;
  }
}
