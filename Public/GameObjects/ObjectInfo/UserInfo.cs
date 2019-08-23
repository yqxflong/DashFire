using System;
using System.Collections.Generic;
//using System.Diagnostics;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public class UserInfo : CharacterInfo
  {
    public delegate ImpactInfo SendImpactToSelfDelegation(int impactId);
    public delegate void StopMyImpactDelegation(int impactId);

    public object CustomData
    {
      get
      {
        return m_CustomData;
      }
      set
      {
        m_CustomData = value;
      }
    }

    public float Scale
    {
      get { return m_Scale; }
    }

    public long ReviveTime
    {
      get
      {
        return m_ReviveTime;
      }
      set
      {
        m_ReviveTime = value;
      }
    }

    public Vector3 RevivePoint
    {
      get { return m_RevivePoint; }
      set { m_RevivePoint = value; }
    }

    public int MultiHitCount
    {
      get { return m_MultiHitCount; }
      set { m_MultiHitCount = value; }
    }

    public long LastHitTime
    {
      get { return m_LastHitTime; }
      set { m_LastHitTime = value; }
    }

    public int[] AiEquipment
    {
      get { return m_AiEquipment; }
    }
    public int[] AiAttackSkill
    {
      get { return m_AiAttackSkill; }
    }
    public int[] AiMoveSkill
    {
      get { return m_AiMoveSkill; }
    }
    public int[] AiControlSkill
    {
      get { return m_AiControlSkill; }
    }
    public int[] AiSelfAssitSkill
    {
      get { return m_AiSelfAssitSkill; }
    }
    public int[] AiTeamAssitSkill
    {
      get { return m_AiTeamAssitSkill; }
    }

    public UserInfo(int id)
      : base(id)
    {
      m_SpaceObject = new SpaceObjectImpl(this, SpatialObjType.kUser);
      m_CastUserInfo = this;
    }

    public void InitId(int id)
    {
      m_Id = id;
    }

    public void Reset()
    {
      ResetCharacterInfo();

      GetLobbyItemBag().Reset();
      GetAiStateInfo().Reset();
      GetCombatStatisticInfo().Reset();

      m_MultiHitCount = 0;
      m_LastHitTime = 0;
    }
    
    public void LoadData(int resId)
    {
      SetLinkId(resId);
      m_LevelupConfig = PlayerConfigProvider.Instance.GetPlayerLevelupConfigById(resId);
      Data_PlayerConfig playerData = PlayerConfigProvider.Instance.GetPlayerConfigById(resId);
      if (null != playerData) {
        SetName(playerData.m_Name);
        SetModel(playerData.m_Model);
        SetActionList(playerData.m_ActionList);

        m_AiEquipment = playerData.m_AiEquipment;
        m_AiAttackSkill = playerData.m_AiAttackSkill;
        m_AiMoveSkill = playerData.m_AiMoveSkill;
        m_AiControlSkill = playerData.m_AiControlSkill;
        m_AiSelfAssitSkill = playerData.m_AiSelfAssitSkill;
        m_AiTeamAssitSkill = playerData.m_AiTeamAssitSkill;
        ///
        GetAiStateInfo().AiLogic = playerData.m_AiLogic;

        m_Scale = playerData.m_Scale;
        AvoidanceRadius = playerData.m_AvoidanceRadius;
        Shape = new Circle(new Vector3(0, 0, 0), playerData.m_Radius);

        ViewRange = playerData.m_ViewRange;
        ReleaseTime = playerData.m_ReleaseTime;
        HeadUiPos = playerData.m_HeadUiPos;
        CostType = playerData.m_CostType;
        ShootBuffLifeTime = playerData.m_ShootBuffLifeTime;
        NoGunRunEnterTimeMs = playerData.m_NoGunRunEnterTimeMs;

        int hp = (int)playerData.m_AttrData.GetAddHpMax(0, 0);
        int energy = (int)playerData.m_AttrData.GetAddNpMax(0, 0);
        int energyCore = (int)playerData.m_AttrData.GetAddEpMax(0, 0);
        float moveSpeed = playerData.m_AttrData.GetAddSpd(0, 0);
        int hpMax = (int)playerData.m_AttrData.GetAddHpMax(0, 0);
        int energyMax = (int)playerData.m_AttrData.GetAddNpMax(0, 0);
        int energyCoreMax = (int)playerData.m_AttrData.GetAddEpMax(0, 0);
        int crgMax = (int)playerData.m_AttrData.GetAddCrgMax(0, 0);
        float hpRecover = playerData.m_AttrData.GetAddHpRecover(0, 0);
        float energyRecover = playerData.m_AttrData.GetAddNpRecover(0, 0);
        float energyCoreRecover = playerData.m_AttrData.GetAddEpRecover(0, 0);
        int attackBase = (int)playerData.m_AttrData.GetAddAd(0, 0);
        int defenceBase = (int)playerData.m_AttrData.GetAddDp(0, 0);
        float critical = playerData.m_AttrData.GetAddCri(0, 0);
        float criticalPow = playerData.m_AttrData.GetAddPow(0, 0);
        float armorPenetration = playerData.m_AttrData.GetAddAndp(0, 0);
        float energyIntensity = playerData.m_AttrData.GetAddAp(0, 0);
        float energyArmor = playerData.m_AttrData.GetAddTay(0, 0);
        float attackRange = playerData.m_AttrData.GetAddRange(0, 0);

        m_MuzzlePos = playerData.m_GunEndRelativePos; 

        GetBaseProperty().SetMoveSpeed(Operate_Type.OT_Absolute, moveSpeed);
        GetBaseProperty().SetHpMax(Operate_Type.OT_Absolute, hpMax);
        GetBaseProperty().SetRageMax(Operate_Type.OT_Absolute, (int)playerData.m_AttrData.GetAddRageMax(0, 0));
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

        // 技能数据
        for (int i = 0; i < 4; i++)
        {
          GetSkillStateInfo().AddSkill(i, new SkillInfo(i+1));
        }
        GetSkillStateInfo().AddSkill(4, new SkillInfo(playerData.m_RollSkill));

        // 武器数据
        for (int i = 0; i < playerData.m_WeaponList.Count; ++i) {
          WeaponInfo weaponInfo = new WeaponInfo(playerData.m_WeaponList[i]);
          WeaponLogicData weaponData = weaponInfo.ConfigData;
          if (null != weaponData) {
            GetShootStateInfo().AddWeapon(i, weaponInfo);
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

        //装备数据
        for (int i = 0; i < ShopEquipmentsId.Length; ++i)
        {
            ShopEquipmentsId[i] = -1;
        }
        UserAttrCalculator.Calc(this);
        SetHp(Operate_Type.OT_Absolute, GetActualProperty().HpMax);
        SetRage(Operate_Type.OT_Absolute, 0);
        SetEnergy(Operate_Type.OT_Absolute, GetActualProperty().EnergyMax);
        SetEnergyCore(Operate_Type.OT_Absolute, GetActualProperty().EnergyCoreMax);
        for (int i = 0; i < playerData.m_WeaponList.Count; ++i) {
          WeaponInfo weaponInfo = GetShootStateInfo().GetWeaponInfoById(playerData.m_WeaponList[i]);
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

    public void RefreshItemSkills()
    {
      //用于客户端刷新物品技能，新加/删除buff由服务端别发消息。
      RefreshItemSkills(null,null);
    }

    public void RefreshItemSkills(SendImpactToSelfDelegation sendImpactToSelf,StopMyImpactDelegation stopImpact)
    {
      //标记所有物品带的技能与buff
      foreach (SkillInfo info in GetSkillStateInfo().GetAllSkill()) {
        if (info.IsItemSkill) {
          info.IsMarkToRemove = true;
        }
      }
      foreach (ImpactInfo info in GetSkillStateInfo().GetAllImpact()) {
        if (info.m_IsItemImpact) {
          info.m_IsMarkToRemove = true;
        }
      }
      //刷新物品带的技能与buff
      EquipmentStateInfo equipInfo = GetEquipmentStateInfo();
      for (int ix = 0; ix < EquipmentStateInfo.c_PackageCapacity; ++ix) {
        ItemDataInfo itemInfo = equipInfo.GetItemData(ix);
        if (null != itemInfo && itemInfo.ItemNum == 1 && null != itemInfo.ItemConfig) {
          ItemConfig cfg = itemInfo.ItemConfig;
          if (null != cfg.m_AddSkillOnEquiping) {
            foreach (int id in cfg.m_AddSkillOnEquiping) {
              SkillInfo skillInfo = GetSkillStateInfo().GetSkillInfoById(id);
              if (null == skillInfo) {
                skillInfo = new SkillInfo(id);
                skillInfo.IsItemSkill = true;
                skillInfo.IsMarkToRemove = false;
                GetSkillStateInfo().AddSkill(skillInfo);
              } else {
                skillInfo.IsMarkToRemove = false;
              }
            }
          }
          if (null != cfg.m_AddBuffOnEquiping && null!=sendImpactToSelf) {
            //此分支为服务器端处理，参数为加impact的回调，这个回调里包括加impact并发消息给客户端（现在ImpactSystem是这样实现的）
            foreach (int id in cfg.m_AddBuffOnEquiping) {
              ImpactInfo impactInfo = GetSkillStateInfo().GetImpactInfoById(id);
              if (null == impactInfo) {
                impactInfo = sendImpactToSelf(id);
                if (null != impactInfo) {
                  impactInfo.m_IsItemImpact = true;
                  impactInfo.m_IsMarkToRemove = false;
                }
              } else {
                impactInfo.m_IsMarkToRemove = false;
              }
            }
          }
        }
      }
      //移除不再有效的技能与buff
      List<int> removeSkills = new List<int>();
      foreach (SkillInfo info in GetSkillStateInfo().GetAllSkill()) {
        if (info.IsItemSkill && info.IsMarkToRemove) {
          removeSkills.Add(info.SkillId);
        }
      }
      foreach (int id in removeSkills) {
        GetSkillStateInfo().RemoveSkill(id);
      }
      removeSkills.Clear();

      List<int> removeImpacts = new List<int>();
      foreach (ImpactInfo info in GetSkillStateInfo().GetAllImpact()) {
        if (info.m_IsItemImpact && info.m_IsMarkToRemove) {
          removeImpacts.Add(info.m_ImpactId);
        }
      }
      foreach (int id in removeImpacts) {
        if (null != stopImpact)
          stopImpact(id);
      }
      removeImpacts.Clear();
    }

    public LobbyItemBag GetLobbyItemBag()
    {
      return m_LobbyItemBag;
    }
    public UserAiStateInfo GetAiStateInfo()
    {
      return m_AiStateInfo;
    }
    public CombatStatisticInfo GetCombatStatisticInfo()
    {
      return m_CombatStatisticInfo;
    }

    public string GetNickName() 
    {
      return m_NickName;
    }
    public void SetNickName(string nickname)
    {
      m_NickName = nickname;
    }

    public int[] ShopEquipmentsId
    {
        get
        {
            return m_ShopEquipmentsId;
        }
    }
    public void SetShopEquipmentsId(int index, int value)
    {
        if (index >= ShopEquipmentsId.Length)
            return;
        ShopEquipmentsId[index] = value;
    }
    
    private float m_Scale = 1.0f;
    private string m_NickName = "";
    private object m_CustomData;
    private long m_ReviveTime = 0;
    private Vector3 m_RevivePoint;
    private int m_MultiHitCount = 0;
    private long m_LastHitTime = 0;
    private int[] m_ShopEquipmentsId = new int[20];
    private int[] m_AiEquipment = null;
    private int[] m_AiAttackSkill = null;
    private int[] m_AiMoveSkill = null;
    private int[] m_AiControlSkill = null;
    private int[] m_AiSelfAssitSkill = null;
    private int[] m_AiTeamAssitSkill = null;

    private LobbyItemBag m_LobbyItemBag = new LobbyItemBag();
    private UserAiStateInfo m_AiStateInfo = new UserAiStateInfo();
    private CombatStatisticInfo m_CombatStatisticInfo = new CombatStatisticInfo();
  }
}
