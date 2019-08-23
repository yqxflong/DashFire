using System;
using System.Collections.Generic;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public enum NpcTypeEnum
  {
    Normal = 0,
    Skill,
    Mecha,
    Horse,
    Task,
    PvpTower,
    AutoPickItem,
    InteractiveNpc,
    MachineGun,
    Fort,
    BigBoss,
    LittleBoss,
  }

  public enum NpcDeadType
  {
    None = 0,
    Animation,
    Explode,
    Ragdoll,
  }

  public class NpcInfo : CharacterInfo
  {
    public int NpcType
    {
      get { return m_NpcType; }
    }
    public float Scale
    {
      get { return m_Scale; }
    }
    public bool IsRangeNpc
    {
      get { return m_IsRangeNpc; }
    }
    public bool IsShine
    {
      get { return m_IsShine; }
    }
    public bool CanMove
    {
      get { return m_CanMove; }
    }
    public bool CanRotate
    {
      get { return m_CanRotate; }
    }
    public NpcDeadType DeadType
    {
      get { return m_DeadType; }
    }
    public string BombEffect
    {
      get { return m_BombEffect; }
      set { m_BombEffect = value; }
    }
    public bool IsAttachControler
    {
      get { return m_IsAttachControler; }
      set { m_IsAttachControler = true; }
    }
    public string AttachNodeName
    {
      get { return m_AttachNodeName; }
    }
    public bool UseBomb
    {
      get { return m_UseBomb; }
      set { m_UseBomb = value; }
    }
    public int DropCount
    {
      get { return m_DropCount; }
    }
    public int DropExp
    {
      get { return m_DropExp; }
    }
    public int DropMoney
    {
      get { return m_DropMoney; }
    }
    public int[] DropProbabilities
    {
      get { return m_DropProbabilities; }
    }
    public int[] DropNpcs
    {
      get { return m_DropNpcs; }
    }
    public int ValidDropNum
    {
      get { return m_ValidDropNum; }
    }
    public int TotalDropProbability
    {
      get { return m_TotalDropProbability; }
    }
    public int OwnerId
    {
      get { return m_OwnerId; }
      set { m_OwnerId = value; }
    }
    public int CreatorId
    {
      get { return m_CreatorId; }
      set { m_CreatorId = value; }
    }

    public int BornAnimTimeMs
    {
      get { return m_BornAnimTimeMs; }
    }

    public string GetBornEffect() { return m_BornEffect; }

    public long BornTime
    {
      set { m_BornTime = value; }
      get { return m_BornTime; }
    }

    public bool IsBorning { set; get; }
    
    public bool NeedDelete
    {
      get { return m_NeedDelete; }
      set { m_NeedDelete = value; }
    }

    public NpcInfo(int id)
      : base(id)
    {
      m_SpaceObject = new SpaceObjectImpl(this, SpatialObjType.kNPC);
      m_CastNpcInfo = this;
    }

    public void InitId(int id)
    {
      m_Id = id;
    }

    public void Reset()
    {
      m_NeedDelete = false;

      ResetCharacterInfo();
      GetAiStateInfo().Reset();
      GetAiStateInfo().AiDatas.Clear();
    }

    public void LoadData(Data_Unit unit)
    {
      SetUnitId(unit.m_Id);
      SetCampId(unit.m_CampId);
      GetAiStateInfo().AiLogic = unit.m_AiLogic;
      for (int i = 0; i < Data_Unit.c_MaxAiParamNum; ++i) {
        GetAiStateInfo().AiParam[i] = unit.m_AiParam[i];
      }
      GetMovementStateInfo().SetPosition(unit.m_Pos);
      GetMovementStateInfo().SetFaceDir(unit.m_RotAngle);
      LoadData(unit.m_LinkId);
    }

    public void LoadData(int resId)
    {
      SetLinkId(resId);
      m_LevelupConfig = NpcConfigProvider.Instance.GetNpcLevelupConfigById(resId);
      Data_NpcConfig npcCfg = NpcConfigProvider.Instance.GetNpcConfigById(resId);
      if (null != npcCfg) {
        m_NpcType = npcCfg.m_NpcType;
        switch (m_NpcType) {
          case (int)NpcTypeEnum.Mecha:
            m_IsMecha = true;
            break;
          case (int)NpcTypeEnum.Horse:
            m_IsHorse = true;
            break;
          case (int)NpcTypeEnum.Task:
            m_IsTask = true;
            break;
          case (int)NpcTypeEnum.PvpTower:
            m_IsPvpTower = true;
            break;
        }
        m_IsRangeNpc = npcCfg.m_IsRange;
        m_IsShine = npcCfg.m_IsShine;
        m_CanMove = npcCfg.m_CanMove;
        m_CanRotate = npcCfg.m_CanRotate;
        m_DeadType = (NpcDeadType)npcCfg.m_DeadType;
        m_DropCount = npcCfg.m_DropCount;
        m_DropExp = npcCfg.m_DropExp;
        m_DropMoney = npcCfg.m_DropMoney;
        m_DropProbabilities = npcCfg.m_DropProbabilities;
        m_DropNpcs = npcCfg.m_DropNpcs;
        m_Scale = npcCfg.m_Scale;

        m_IsHurtComa = npcCfg.m_IsHurtComa;
        m_isBlaze = npcCfg.m_isBlaze;
        m_BornAnimTimeMs = npcCfg.m_BornTimeMs;
        m_BornEffect = npcCfg.m_BornEffect;
        m_IsAttachControler = npcCfg.m_IsAttachControler;
        m_AttachNodeName = npcCfg.m_AttachNodeName;
        m_MuzzlePos = npcCfg.m_GunEndRelativePos;
        m_SecondMuzzlePos = npcCfg.m_GunEndRelativePos;
        m_SecondMuzzlePos.X = -npcCfg.m_GunEndRelativePos.X;

        SetName(npcCfg.m_Name);
        SetLevel(npcCfg.m_Level);
        SetModel(npcCfg.m_Model);
        DeathModel = npcCfg.m_DeathModel;
        DeathEffect = npcCfg.m_DeathEffect;
        DeathSound = npcCfg.m_DeathSound;
        SetActionList(npcCfg.m_ActionList);
        SetBarrageId(npcCfg.m_Barrage);
                
        AvoidanceRadius = npcCfg.m_AvoidanceRadius;
        if (null != npcCfg.m_Shape)
          Shape = (Shape)npcCfg.m_Shape.Clone();
        else
          Shape = new Circle(new Vector3(0, 0, 0), 1);

        ViewRange = npcCfg.m_ViewRange;
        GohomeRange = npcCfg.m_GohomeRange;
        ReleaseTime = npcCfg.m_ReleaseTime;
        HeadUiPos = npcCfg.m_HeadUiPos;

        int hp = (int)npcCfg.m_AttrData.GetAddHpMax(0, npcCfg.m_Level);
        int energy = (int)npcCfg.m_AttrData.GetAddNpMax(0, npcCfg.m_Level);
        float moveSpeed = npcCfg.m_AttrData.GetAddSpd(0, npcCfg.m_Level);
        int hpMax = (int)npcCfg.m_AttrData.GetAddHpMax(0, npcCfg.m_Level);
        int energyMax = (int)npcCfg.m_AttrData.GetAddNpMax(0, npcCfg.m_Level);
        int energyCoreMax = (int)npcCfg.m_AttrData.GetAddEpMax(0, npcCfg.m_Level);
        int crgMax = (int)npcCfg.m_AttrData.GetAddCrgMax(0, npcCfg.m_Level);
        float hpRecover = npcCfg.m_AttrData.GetAddHpRecover(0, npcCfg.m_Level);
        float energyRecover = npcCfg.m_AttrData.GetAddNpRecover(0, npcCfg.m_Level);
        float energyCoreRecover = npcCfg.m_AttrData.GetAddEpRecover(0, npcCfg.m_Level);
        int attackBase = (int)npcCfg.m_AttrData.GetAddAd(0, npcCfg.m_Level);
        int defenceBase = (int)npcCfg.m_AttrData.GetAddDp(0, npcCfg.m_Level);
        float critical = npcCfg.m_AttrData.GetAddCri(0, npcCfg.m_Level);
        float criticalPow = npcCfg.m_AttrData.GetAddPow(0, npcCfg.m_Level);
        float armorPenetration = npcCfg.m_AttrData.GetAddAndp(0, npcCfg.m_Level);
        float energyIntensity = npcCfg.m_AttrData.GetAddAp(0, npcCfg.m_Level);
        float energyArmor = npcCfg.m_AttrData.GetAddTay(0, npcCfg.m_Level);
        float attackRange = npcCfg.m_AttrData.GetAddRange(0, npcCfg.m_Level);
        float aRps = npcCfg.m_AttrData.GetAddRps(0, npcCfg.m_Level);
        int aCrg = (int)npcCfg.m_AttrData.GetAddCrg(0, npcCfg.m_Level);
        float aCht = npcCfg.m_AttrData.GetAddCht(0, npcCfg.m_Level);
        float aWdps = npcCfg.m_AttrData.GetAddDps(0, npcCfg.m_Level);
        float aDamRandom = npcCfg.m_AttrData.GetAddDamRange(0, npcCfg.m_Level);

        GetBaseProperty().SetMoveSpeed(Operate_Type.OT_Absolute, moveSpeed);
        GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, hpMax);
        GetBaseProperty().SetEnergyMax(Operate_Type.OT_Absolute, energyMax);
        GetBaseProperty().SetEnergyCoreMax(Operate_Type.OT_Absolute, energyCoreMax);
        GetBaseProperty().SetCrgMax(Operate_Type.OT_Absolute, crgMax);
        GetBaseProperty().SetHpRecover(Operate_Type.OT_Absolute, hpRecover);
        GetBaseProperty().SetEnergyRecover(Operate_Type.OT_Absolute, energyRecover);
        GetBaseProperty().SetEnergyCoreRecover(Operate_Type.OT_Absolute, energyCoreRecover);
        GetBaseProperty().SetAttackBase(Operate_Type.OT_Absolute, attackBase);
        GetBaseProperty().SetDefenceBase(Operate_Type.OT_Absolute, defenceBase);
        GetBaseProperty().SetCritical(Operate_Type.OT_Absolute, critical);
        GetBaseProperty().SetCriticalPow(Operate_Type.OT_Absolute, criticalPow);
        GetBaseProperty().SetArmorPenetration(Operate_Type.OT_Absolute, armorPenetration);
        GetBaseProperty().SetEnergyIntensity(Operate_Type.OT_Absolute, energyIntensity);
        GetBaseProperty().SetEnergyArmor(Operate_Type.OT_Absolute, energyArmor);
        GetBaseProperty().SetAttackRange(Operate_Type.OT_Absolute, attackRange);
        GetBaseProperty().SetRps(Operate_Type.OT_Absolute, aRps);
        GetBaseProperty().SetCrg(Operate_Type.OT_Absolute, aCrg);
        GetBaseProperty().SetCht(Operate_Type.OT_Absolute, aCht);
        GetBaseProperty().SetWdps(Operate_Type.OT_Absolute, aWdps);
        GetBaseProperty().SetDamRange(Operate_Type.OT_Absolute, aDamRandom);
        
        // 技能数据
        for (int i=0; i<npcCfg.m_SkillList.Count; i++) {
          SkillInfo skillInfo = new SkillInfo(npcCfg.m_SkillList[i]);
          GetSkillStateInfo().AddSkill(skillInfo);
        }

        for (int i = 0; i < npcCfg.m_WeaponList.Count; ++i) {
          WeaponInfo weaponInfo = new WeaponInfo(npcCfg.m_WeaponList[i]);
          WeaponLogicData weaponData = weaponInfo.ConfigData;
          if (null != weaponData) {
            GetShootStateInfo().AddWeapon(0, weaponInfo);
            if (null == GetShootStateInfo().GetCurWeaponInfo()) {
              GetShootStateInfo().SetCurWeaponInfo(weaponInfo.WeaponId);
              GetShootStateInfo().CurrentWeaponIndex = i;
            }
            //todo:先按武器表里的武器配置上，后续需要从db里读取升级数据并初始化升级配置数据
            EquipmentDataInfo equipDataInfo = new EquipmentDataInfo();
            equipDataInfo.EquipmentConfig = EquipmentConfigProvider.Instance.GetEquipmentConfigById(weaponData.m_EquipmentId);
            GetEquipmentStateInfo().EquipmentInfo.Weapons[i] = equipDataInfo;
          }
        }

        NpcAttrCalculator.Calc(this);
        CalcValidDropNumAndTotalDropProbability();
        SetHp(Operate_Type.OT_Absolute, GetActualProperty().HpMax);
        SetEnergy(Operate_Type.OT_Absolute, GetActualProperty().EnergyMax);
        for (int i = 0; i < npcCfg.m_WeaponList.Count; ++i) {
          WeaponInfo weaponInfo = GetShootStateInfo().GetWeaponInfoById(npcCfg.m_WeaponList[i]);
          WeaponLogicData weaponData = weaponInfo.ConfigData;
          if (null != weaponData) {
            //暴击数值
            float CRIRATE_ = (float)(GetActualProperty().Critical / 480.0);
            float CRIRATE_C_ = CriticalConfigProvider.Instance.GetC(CRIRATE_);
            weaponInfo.CurCritical = CRIRATE_C_;
          }
        }
      }
    }

    public NpcAiStateInfo GetAiStateInfo()
    {
      return m_AiStateInfo;
    }

    private void CalcValidDropNumAndTotalDropProbability()
    {
      if (null != m_DropProbabilities && null != m_DropNpcs) {
        int ct = m_DropProbabilities.Length;
        if (ct > m_DropNpcs.Length)
          ct = m_DropNpcs.Length;
        m_ValidDropNum = ct;
        m_TotalDropProbability = 0;
        for (int i = 0; i < ct; ++i) {
          m_TotalDropProbability += m_DropProbabilities[i];
        }
      } else {
        m_ValidDropNum = 0;
        m_TotalDropProbability = 0;
      }
    }

    private int m_NpcType = 0;
    private float m_Scale = 1.0f;
    private bool m_IsRangeNpc = false;
    private bool m_IsShine = false;
    private bool m_CanMove = true;
    private bool m_CanRotate = true;
    private NpcDeadType m_DeadType = NpcDeadType.Animation;
    private string m_BombEffect = "";
    //Note:目前所有能爆炸的东西死亡时都使用爆炸，不区分来源, by lixiaojiang
    private bool m_UseBomb = false;

    private bool m_NeedDelete = false;

    private int m_DropCount = 0;
    private int m_DropExp = 0;
    private int m_DropMoney = 0;
    private int[] m_DropProbabilities = null;
    private int[] m_DropNpcs = null;
    private int m_ValidDropNum = 0;
    private int m_TotalDropProbability = 0;
    private int m_OwnerId = 0;
    private int m_CreatorId = 0;

    private long m_BornTime = 0;
    private int m_BornAnimTimeMs = 0;
    private string m_BornEffect = "";
    private bool m_IsAttachControler = false;
    private string m_AttachNodeName = "";
    private NpcAiStateInfo m_AiStateInfo = new NpcAiStateInfo();
  }
}
