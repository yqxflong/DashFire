using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class SkillTriggerInfo
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int ImpactId = -1;
    public SkillLogicData.TargType TargetType;
    public CharacterInfo Target;
    public ScriptRuntime.Vector3 TriggerPos = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 TargetPos = new ScriptRuntime.Vector3();
    public float TargetAngle = 0;
  }

  public class SkillGrenadeParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 TargetPosL = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 TargetPosR = new ScriptRuntime.Vector3();
    public string BulletModel;
    public ScriptRuntime.Vector3 BulletRelativePos;
    public string BlastWaitEffect;
    public string BlastEffect;
    public string BlastSound;
    public float FlatSpeed;
    public float AccelerateY;
    public float BlastRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public float BlastWaitTime;
    public bool IsGrenadeCreated = false;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;

    public List<uint> BulletObjIdList = new List<uint>();
    public bool IsBeginBlastWait = false;
    public bool IsBlast = false;
    public ScriptRuntime.Vector3 StartPos;
  }
  
  public class SkillAirAttackParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public string SignalModel;
    public float SignalFlatSpeed;
    public float SignalAccelerateY;

    public string PlaneModel;
    public string PlaneSound;
    public float FlyAngle;
    public string MarkModel;

    public const int MarkNum = 8;
    public List<ScriptRuntime.Vector3> MarkPosList = new List<ScriptRuntime.Vector3>();
    public float[] MarkBlastTime = new float[MarkNum];
    public bool[] IsMarkBlast = new bool[MarkNum];

    public string BlastEffect;
    public string BlastSound;
    public float BlastRadius = 0;
    public float BlastPace;

    public int ObjectType; // 信号弹-0，飞机-1
    public float ElapsedTime;
    public float LifeTime;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;

    public bool IsSignalCreated = false;
    public uint ObjectId;
  }

  public class SkillBossMissileParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public CharacterInfo TargetEntity = null;
    public string MissileModel;
    public string MissileEffect;
    public string BlastSound;
    public float FlatSpeed;
    public float AccelerateY;
    public float BlastRadius = 0;
    public string MarkModel;
    public float ElapsedTime;
    public float LifeTime;
    public bool IsMissileCreated = false;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    public const int MaxBlastTimes = 4;
    public int BlastTimes = 0;
    public const int MarkNum = 6;
    public int MarkType; // 0 - 直线, 1 - 三角
    public const float BlastInterval = 0.6f;
    public List<ScriptRuntime.Vector3> MarkPosList = new List<ScriptRuntime.Vector3>();
    public float MarkBlastTime;
    public bool IsMarkBlast;
    
    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;

    public uint GrenadeId;
  }

  public class SkillLightningShotParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int BarrageType;
    public float ShootAngle;
    public ScriptRuntime.Vector3 ShootPos;
    public string LightningEffect;
    public string SoundEffect;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public bool IsShot;
    public int ValidNum;

    public int FirstBulletId = -1;
    public float FirstBulletBlastRadius = 0;
    public int FirstBulletExtraImpactId = 0;
    public int FirstBulletExtraImpactTargType;
    public string FirstBulletBlastEffect = "";
    public bool FirstBulletDestroy = false;
  }

  public class SkillPierceShotParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int BarrageId = -1;
    public ScriptRuntime.Vector3 ShootPos = new ScriptRuntime.Vector3();
    public float ShootAngle = 0;
    public bool IsShotCreated = false;
    public long ElapsedTime = 0;
    public float MaxBarrageTime = 3.0f;
    public SkillLogicData.TargType TargetType;
    public int ImpactId = -1;
    public string SoundEffect = "";
  }
  
  public class WpnSourBulletParams
  {
    public int TriggerEntityId = -1;
    public int WeaponId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public string BulletModel;
    public string BlastEffect;
    public string BlastSound;
    public float FlyAngle;
    public float FlatSpeed;
    public float AccelerateY;
    public float BlastRadius = 0;
    public long ElapsedTime;
    public long LifeTime;
    public bool IsSourBulletCreated = false;

    public uint SourBulletId;
  }
  
  public class SkillSourBulletParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public string BulletModel;
    public string BulletEffect;
    public string FireSound;
    public string BlastSound;
    public string ActiveBlastSound;
    public float FlatSpeed;
    public float AccelerateY;
    public float BlastRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public int ValidNum;
    public bool IsSourBulletCreated = false;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    public uint SourBulletId;
  }
  
  public class SkillSourPathParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public ScriptRuntime.Vector3 EffectPos;
    public string SourPathModel;
    public string SourPathEffect;
    public string SourPathSound;
    public float PathRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public bool IsSourPathCreated = false;
    public int ImpactId;
    public int BlastImpactId;
    public int BlastImpactTargType;
    public int HurtImpactId;
    public int HurtImpactTargType;
    public bool IsBlast;
    public int ValidNum;
    public float TriggerBlastRadius = 0;
    public float TriggerBlastInterval;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;
    public float TBlastTime = 0;
    public float TCount = 0;

    public uint SourPathId;
  }
  
  public class SkillFlyBombParams
  {
    public int TriggerEntityId = -1;
    public int BarrageId = -1;
    public int SkillId = -1;
    public float AttackRadius = 0;
    public long ElapsedTime = 0;
    public long LastBarrageTime = 0;
    public float LifeTime = 0;
    public float BarrageInterval = 0;
    public int TargetIndex = 0;
    public int ValidNum = 0;
    public int ImpactId;
    public bool IsFlyBombCreated = false;
    public SkillLogicData.TargType TargetType;
    public List<int> EnemyList = new List<int>();
    public string HeadEffect;
    public string HurtEffect;
    public float MaxBarrageTime = 0;
    public bool IsWaitBarrageFinish = false;
    public bool HasFirstShoot = false;
    public string BulletEffect = "";
    public int DgeAreaType = 0;
    public int HeadEffectFastID = 0;
  }

  public struct Parampeer
  {
    public int barrage;
    public int buff;
    public string skeleton;
  }
  public class SkillConditionBarrageParams
  {
    public int TriggerEntityId = -1;
    public int BarrageId = -1;
    public int SkillId = -1;
    public float AttackRadius = 0;
    public long ElapsedTime = 0;
    public long LastBarrageTime = 0;
    public float LifeTime = 0;
    public float BarrageInterval = 0;
    public ScriptRuntime.Vector3 TargetPos;
    public int ValidNum = 0;
    public int ImpactId;
    public bool IsBulletCreated = false;
    public SkillLogicData.TargType TargetType;
    public List<int> EnemyList = new List<int>();
    public string MuzzleEffect;
    public string HurtEffect;
    public string BlastEffect;
    public int MaxBarrageCount = 0;
    public int CurBarrageNum = 0;
    public bool HasFirstShoot = false;
    public int DgeAreaType = 0;
    public List<Parampeer> paramarg = new List<Parampeer>(); 
  }

  public class SkillFireBombParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 TargetPos;
    public string BulletModel;
    public string BulletEffect;
    public string BlastSound;
    public float FlatSpeed;
    public float FlyAngle;
    public float AccelerateY;
    public float BlastRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public bool IsFireBombCreated = false;
    public int ImpactId;
    public int ValidNum;
    public int HarmImpactId;
    public int HarmImpactTargType;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    public uint FireBombId;
  }

  public class SkillHuoxiyiShieldParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int NpcLinkId = -1;
    public ScriptRuntime.Vector3 BornPos;
    public float FaceDir = 0;
    public int CampId = -1;
    public long ElapsedTime = 0;
    public float LifeTime = 0;
    public bool IsCreated = false;
    public int NpcId = 0;
  }
  
  public class SkillPublicSummonParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int NpcLinkId = -1;
    public ScriptRuntime.Vector3 BornPos;
    public float FaceDir = 0;
    public int CampId = -1;
    public long ElapsedTime = 0;
    public float LifeTime = 0;
    public bool IsCreated = false;
    public int NpcId = 0;
    public int AiLogicNum = 0;
    public int[] AiLogicParam = new int[8];
  }

  //飞行参数
  public class SkillFlyParams
  {
    public ScriptRuntime.Vector3 StartPos = new ScriptRuntime.Vector3();  //起点坐标
    public ScriptRuntime.Vector3 EndPos = new ScriptRuntime.Vector3();    //终点坐标
    public float VX = 0;
    public float VY = 0;
    public float VZ = 0;
    public float AX = 0;
    public float AY = 0;
    public float AZ = 0;
    public long StartTime = 0;  //启动时间
    public float FlyTime = 0;   //飞行时间
    public float FlyAngle = 0; //飞行角度
  } 

  public class SkillBarrageParams
  {    
    //场景逻辑参数
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int BarrageId = -1;
    public float ShootAngle = 0;
    public bool IsShotCreated = false;
    public long ElapsedTime = 0;
    public float MaxBarrageTime = 4.0f;
    public SkillLogicData.TargType TargetType = SkillLogicData.TargType.ENEMY;
    public int ImpactId = 7;
    public ScriptRuntime.Vector3 BombPos = new ScriptRuntime.Vector3();
  }

  public class SkillShootBarrageParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public int ImpactId = -1;
    public SkillLogicData.TargType TargetType;
    public CharacterInfo Target;
    public ScriptRuntime.Vector3 TriggerPos = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 TargetPos = new ScriptRuntime.Vector3();
    public float TargetAngle = 0;


    public bool IsShotCreated = false;
    public int BarrageType = -1;
    public float ShootAngle = 0;
    public bool IsSetTargetPos = false;

    public float MaxBarrageTime = 5.0f;
    public long ElapsedTime = 0;
    public string LandMarkEffect = "";
    public string CollideEffect = "";
    public string CollideSound = "";
    public int ImpactSrcPosType = 0;
  }

  public class SimulateBarrageParams
  {
    // 配置数据
    public string BulletModel;
    public ScriptRuntime.Vector3 StartRelativePos = new ScriptRuntime.Vector3();
    public string BlastWaitEffect = "";
    public float BlastWaitTime = 0;
    public string BlastEffect = "";
    public string BlastSound = "";
    public float BlastRadius = 0;
    public float FlatSpeed = 0;
    public float FlatAccelerate = 0;
    public float AccelerateY = 0;
    public bool IsFocusTarget = false;
    // 运行时数据
    public ScriptRuntime.Vector3 StartPos = new ScriptRuntime.Vector3();
    public long ElapsedTime = 0;
    public float FlyTime = 0;
    public bool IsBulletCreated = false;
    public bool IsBlastWait = false;
    public bool IsBlast = false;
    public uint BulletObjId = 0;
    public uint WaitObjId = 0;
    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;
  }

  public class SkillSummonParams
  {
    public List<int> unitList = new List<int>();    //待召唤的NPC的ID列表，从配置文件中获取，固定数据 
    public List<int> summonList = new List<int>();  //待召唤列表：unitList中当前死亡或不存在的UnitID
    public List<int> existList = new List<int>();   //当前列表：unitList中当前存活的UnitID    
    public ScriptRuntime.Vector3[] positionList = new ScriptRuntime.Vector3[8]; //召唤点位置坐标列表，固定数据 
    public bool[] positionFlagList = new bool[8];   //位置点标识位：true当前位置可创建NPC，false当前位置不可创建NPC     
    public long beginTime = 0;          //技能开始时间    
    public int count = 0;               //计数器
  }
  
  public class SkillSnipeBombParams
  {
    public int TargetIndex = -1;
    public bool TriggerSight = false;
    public long CurKeepTime = 0;
    public long Time = 0;
    public uint lineFastId = 0;
  }
  public class SkillUCABossMissileParams
  {
    // 配置数据
    public string BulletModel;
    public ScriptRuntime.Vector3 StartRelativePos = new ScriptRuntime.Vector3();
    public string BlastEffect = "";
    public string BlastSound = "";
    public float BlastRadius = 0;
    public float FlatSpeed = 0;
    public float FlatAccelerate = 0;
    public float VerticalAccelerate = 0;
    public float VerticalSpeed = 0;
    public float MaxAngle = 0;    // 转弯弧度
    public float StartAngle = 0;   // 初始水平角度
    public float ChaseTime = 0;
    public string TargetSign = "";
    // 运行时数据
    public ScriptRuntime.Vector3 StartPos = new ScriptRuntime.Vector3();
    public long ElapsedTime = 0;
    public bool IsBulletCreated = false;
    public bool IsBlast = false;
    public uint BulletObjId = 0;
    public float direction = 0;
    public ScriptRuntime.Vector3 oldPos = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 newPos = new ScriptRuntime.Vector3();
    public ScriptRuntime.Vector3 endPos = new ScriptRuntime.Vector3();
    public float pitchAngle = 0;
    public bool IsStopChase = false;
    public bool IsTracking = false;
    public uint TargetSignId = 0;
    public float TargetY = 0;
    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;
  }
  public class UCABossLandmineParams
  {
    // 配置数据
    public string BulletModel;
    public ScriptRuntime.Vector3 StartRelativePos = new ScriptRuntime.Vector3();
    public string PrepareEffect = "";
    public float PrepareTime = 0;
    public string BlastWaitEffect = "";
    public float BlastWaitTime = 0;
    public string BlastEffect = "";
    public string BlastSound = "";
    public float BlastRadius = 0;
    public float FlatSpeed = 0;
    public float FlatAccelerate = 0;
    public float AccelerateY = 0;
    public bool IsFocusTarget = false;
    // 运行时数据
    public ScriptRuntime.Vector3 StartPos = new ScriptRuntime.Vector3();
    public long ElapsedTime = 0;
    public float FlyTime = 0;
    public bool IsBulletCreated = false;
    public bool IsBulletPrepare = false;
    public bool IsBulletPrepareEnd = false;
    public bool IsBlastWait = false;
    public bool IsBlast = false;
    public uint BulletObjId = 0;
    public uint PrepareObjId = 0;
    public uint WaitObjId = 0;
    //Physic data
    public float PhyForce = 15.0f;
    public float DampingRate = 0.8f;
    public float Range = 8.0f;
    public float UpwardAdjust = 5.0f;
  }

  public class UCABossRotationParams
  {
    public bool IsMoving = false;
    public float MovingStartTime = 0;
  }

  public class SkillGunnerAttackParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 FirePos;
    public ScriptRuntime.Vector3 TargetPos;
    public string BulletModel;
    public string HitEffect;
    public float FlatSpeed;
    public float FlyAngle;
    public float MaxRangle;
    public float BlastRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    public bool IsCreated = false;
    public uint GunnerAttackId;
  }

  public class SkillGunnerArcAttackParams
  {
    public int TriggerEntityId = -1;
    public int SkillId = -1;
    public ScriptRuntime.Vector3 FirePos;
    public ScriptRuntime.Vector3 TargetPos;
    public string BulletModel;
    public string HitEffect;
    public float HSpeed;
    public float HASpeed;
    public float VASpeed;
    public float FlyAngle;
    public float BlastRadius = 0;
    public float ElapsedTime;
    public float LifeTime;
    public int ImpactId;
    public SkillLogicData.TargType TargetType;
    public int DgeAreaType = 0;

    public bool IsCreated = false;
    public uint GunnerArcAttackId;
  }
}
