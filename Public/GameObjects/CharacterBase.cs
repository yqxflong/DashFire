using System;
using System.Collections.Generic;
using DashFireSpatial;
using ScriptRuntime;
using DashFire.Debug;

namespace DashFire
{
  public delegate void DamageDelegation(int receiver, int caster, bool isShootDamage, bool isCritical, int hpDamage, int npDamage);
  public delegate void OnBeginAttackEvent();

  public interface IShootTarget
  {
    uint GetActorID();
  }

  //！！！注意，不要在这里添加属于CharacterProperty管理的内容，只能加人物状态数据，不能加属性数据，属性由属性计算得到
  public enum PropertyIndex
  {
    IDX_HP = 0,
    IDX_MP,
    IDX_STATE,
  }

  public enum HilightType
  {
    kNone,
    kBurnType,
    kFrozenType,
    kShineType,
  }

  public enum CharacterState_Type
  {
    CST_FixedPosition = 1 << 0,  // 定身，不能移动
    CST_Silence       = 1 << 1,  // 沉默，不能释放技能
    CST_Invincible    = 1 << 2,  // 无敌
    CST_BODY          = 1 << 3,  // ？
    CST_Sleep         = 1 << 4,  // 昏迷，不能移动，不能攻击，不能放技能
    CST_DamageAbsorb  = 1 << 5,  // 伤害吸收，在计算伤害时需要考虑
    CST_Hidden        = 1 << 6,  // 隐身的
    CST_Opened        = 1 << 7,  // 用于npc带物品的，表明已经捡过了
    CST_Disarm        = 1 << 8,  // 缴械，不能开枪
  }

  /**
   * @brief 角色基类
   */
  public class CharacterInfo : IShootTarget
  {
    public struct AttackerInfo
    {
      public int m_AttackerType;
      public long m_AttackTime;
      public int m_HpDamage;
      public int m_NpDamage;
    }
    
    // implement ISpaceObject interface
    public class SpaceObjectImpl : ISpaceObject
    {
      public uint GetID() { return (uint)m_CharacterInfo.GetId(); }
      public SpatialObjType GetObjType() { return m_ObjType; }
      public Vector3 GetPosition()
      {
        Vector3 v = m_CharacterInfo.GetMovementStateInfo().GetPosition3D();
        return v;
      }
      public float GetRadius() { return m_CharacterInfo.GetRadius(); }
      public Vector3 GetVelocity()
      {
        Vector3 ret;
        if (!m_CharacterInfo.GetMovementStateInfo().IsMoving) {
          ret = new Vector3();
        } else {
          ret = m_CharacterInfo.GetMovementStateInfo().GetMoveDir3D() * (float)(m_CharacterInfo.GetActualProperty().MoveSpeed * m_CharacterInfo.VelocityCoefficient);
        }
        return ret;
      }
      public bool IsAvoidable()
      {
        bool ret = true;
        if (SpatialObjType.kNPC == m_ObjType) {
          NpcInfo npc = m_CharacterInfo.CastNpcInfo();
          if (null != npc) {
            ret = (npc.NpcType != (int)NpcTypeEnum.Skill && 
              npc.NpcType != (int)NpcTypeEnum.AutoPickItem);
          }
        }
        return ret;
      }
      public Shape GetCollideShape()
      {
        return m_CharacterInfo.Shape;
      }
      public List<ISpaceObject> GetCollideObjects() { return m_CollideObjects; }
      public void OnCollideObject(ISpaceObject obj) {
        m_CollideObjects.Add(obj); 
      }
      public void OnDepartObject(ISpaceObject obj) {
        m_CollideObjects.Remove(obj); 
      }
      public object RealObject
      {
        get
        {
          return m_CharacterInfo;
        }
      }
      public SpaceObjectImpl(CharacterInfo info, SpatialObjType objType)
      {
        m_CharacterInfo = info;
        m_ObjType = objType;
      }

      private CharacterInfo m_CharacterInfo = null;
      private SpatialObjType m_ObjType = SpatialObjType.kNPC;
      private List<ISpaceObject> m_CollideObjects = new List<ISpaceObject>(); // 与当前物体碰撞的物体
    }
    public DashFireSpatial.CellPos SightCell
    {
      get { return m_SightCell; }
      set { m_SightCell = value; }
    }
    public bool CurBlueCanSeeMe
    {
      get { return m_CurBlueCanSeeMe; }
      set { m_CurBlueCanSeeMe = value; }
    }
    public bool LastBlueCanSeeMe
    {
      get { return m_LastBlueCanSeeMe; }
    }
    public bool CurRedCanSeeMe
    {
      get { return m_CurRedCanSeeMe; }
      set { m_CurRedCanSeeMe = value; }
    }
    public bool LastRedCanSeeMe
    {
      get { return m_LastRedCanSeeMe; }
    }
    public void PrepareUpdateSight()
    {
      m_LastBlueCanSeeMe = m_CurBlueCanSeeMe;
      m_CurBlueCanSeeMe = false;
      m_LastRedCanSeeMe = m_CurRedCanSeeMe;
      m_CurRedCanSeeMe = false;
    }
    public bool IsNpc
    {
      get
      {
        return m_CastNpcInfo != null;
      }
    }
    public bool IsUser
    {
      get
      {
        return m_CastUserInfo != null;
      }
    }
    public NpcInfo CastNpcInfo()
    {
      return m_CastNpcInfo;
    }
    public UserInfo CastUserInfo()
    {
      return m_CastUserInfo;
    }

    /**
     * @brief 构造函数
     *
     * @param id
     *
     * @return 
     */
    public CharacterInfo(int id)
    {
      m_Id = id;
      m_UnitId = 0;
      m_LinkId = 0;
      m_AIEnable = true;
      m_BaseProperty = new CharacterProperty();
      m_ActualProperty = new CharacterProperty();
      m_BarrageId = 0;
      
      m_ComeOutOver = true;
      //m_TargetId = Defines.INVALID_ID;
      m_ReleaseTime = 0;
      IsFlying = false;
      m_MousePos = ScriptRuntime.Vector3.Zero;
    }

    /**
     * @brief 获取id
     *
     * @return 
     */
    public int GetId()
    {
      return m_Id;
    }
    
    /**
     * @brief 单位id
     *
     * @return 
     */
    public int GetUnitId()
    {
      return m_UnitId;
    }

    /**
     * @brief 设置单位id
     *
     * @return 
     */
    public void SetUnitId(int id)
    {
      m_UnitId = id;
    }

    public int GetLinkId()
    {
      return m_LinkId;
    }

    public void SetLinkId(int id)
    {
      m_LinkId = id;
    }

    /**
     * @brief 设置名字
     *
     * @param name
     *
     * @return 
     */
    public void SetName(string name)
    {
      m_Name = name;
    }
    
    /**
     * @brief 获取名字
     *
     * @return 
     */
    public string GetName()
    {
      return m_Name;
    }

    public void SetBeAttack(bool bAttack)
    {
      m_BeAttack = bAttack;
    }

    public bool GetBeAttack ()
    {
      return m_BeAttack;
    }

    public bool IsInShelter
    {
      get {return m_IsInShelter;}
      set {m_IsInShelter = value;}
    }

    public void SetCurEnemyInfo(int arg)
    {
      m_CurEnemyId = arg;
    }

    public int GetCurEnemyId()
    {
      return m_CurEnemyId;
    }

    public long ReleaseTime
    {
      get { return m_ReleaseTime; }
      set { m_ReleaseTime = value; }
    }

    public long DeadTime
    {
      get
      {
        return m_DeadTime;
      }
      set
      {
        m_DeadTime = value;
      }
    }

    /**
     * @brief 获取经验
     *
     * @return 
     */
    public int GetExp()
    {
      return m_Exp;
    }

    /**
     * @brief 设置经验值
     *
     * @param money
     *
     * @return 
     */
    public void SetExp(int exp)
    {
      m_Exp = exp;
    }

    /**
     * @brief 获取等级
     *
     * @return 
     */
    public int GetLevel()
    {
      return m_Level;
    }

    /**
     * @brief 设置等级
     *
     * @param money
     *
     * @return 
     */
    public void SetLevel(int level)
    {
      m_Level = level;
      m_LevelChanged = true;
    }

    public int GetTotalSkillPoint()
    {
      return GetLevel() + 100;
    }

    public int GetMoney()
    {
      return (int)m_Money;
    }

    public int GetTotalMoney()
    {
      return (int)m_TotalMoney;
    }
    
    public float Money
    {
      get { return m_Money; }
      set { m_Money = value; }
    }
    
    public float TotalMoney
    {
      get { return m_TotalMoney; }
      set { m_TotalMoney = value; }
    }
    
    /**
     * @brief 
     */
    public int Hp
    {
      get { return m_Hp; }
    }

    /**
     * @brief 
     */
    public int Rage
    {
      get { return m_Rage; }
    }

    /**
     * @brief 
     */
    public int Energy
    {
      get { return m_Energy; }
    }

    public bool LogicDead {
      get { return m_LogicDead; }
      set { m_LogicDead = value; }
    }

    /**
     * 攻击范围系数（在0~1之间） 
     */
    public float AttackRangeCoefficient
    {
      get { return m_AttackRangeCoefficient; }
      set { m_AttackRangeCoefficient = value; }
    }

    /**
     * 对象的速度系数（在0~1之间）
     */
    public float VelocityCoefficient
    {
      get { return m_VelocityCoefficient; }
      set { m_VelocityCoefficient = value; }
    }

    /**
     * @brief 视野范围
     */
    public float ViewRange
    {
      get { return m_ViewRange; }
      set { m_ViewRange = value; }
    }

    public float GohomeRange
    {
      get { return m_GohomeRange; }
      set { m_GohomeRange = value; }
    }

    public int HeadUiPos
    {
      get { return m_HeadUiPos; }
      set { m_HeadUiPos = value; }
    }

    public int CostType
    {
      get { return m_CostType; }
      set { m_CostType = value; }
    }

    public float ShootBuffLifeTime
    {
      get { return m_ShootBuffLifeTime; }
      set { m_ShootBuffLifeTime = value; }
    }

    public bool IsMoving { set; get; }
    public bool IsFlying { set; get; }
    public Vector3 FlyStartPos { set; get; }

    // configer muzzle pos
    public ScriptRuntime.Vector3 GetMuzzlePos ()
    {
      return DashFireSpatial.Shape.TransformToWorldPos(GetMovementStateInfo().GetPosition3D(), m_MuzzlePos, GetMovementStateInfo().FaceDirCosAngle, GetMovementStateInfo().FaceDirSinAngle);
    }

    public ScriptRuntime.Vector3 GetSecondMuzzlePos()
    {
      return DashFireSpatial.Shape.TransformToWorldPos(GetMovementStateInfo().GetPosition3D(), m_SecondMuzzlePos, GetMovementStateInfo().FaceDirCosAngle, GetMovementStateInfo().FaceDirSinAngle);
    }

    // 从脚本取到的枪口挂接点，只用于客户端表现
    public ScriptRuntime.Vector3 GetScriptMuzzlePos ()
    {
      return m_ScriptMuzzlePos;
    }

    public ScriptRuntime.Vector3 GetScriptSecondMuzzlePos()
    {
      return m_ScriptSecondMuzzlePos;
    }

    public ScriptRuntime.Vector3 GetScriptBodyHeadFirePos ()
    {
      return m_ScriptBodyHeadFirePos;
    }

    public bool IsHaveScriptBodyCenter() { return m_IsHaveScriptBodyCenter; }

    public ScriptRuntime.Vector3 GetScriptBodyCenterPos()
    {
      if (m_IsHaveScriptBodyCenter) {
        return m_ScriptBodyCenterPos;
      } else {
        return GetMovementStateInfo().GetPosition3D();
      }
    }

    public void SetScriptBodyCenterPos(Vector3 pos)
    {
      m_ScriptBodyCenterPos = pos;
      m_IsHaveScriptBodyCenter = true;
    }

    public ScriptRuntime.Vector3 GetMousePos()
    {
        return m_MousePos;
    }

    public void SetMousePos(Vector3 pos)
    {
      m_MousePos = pos;
    }
    
    public void SetScriptBodyHeadFirePos(Vector3 pos)
    {
      m_ScriptBodyHeadFirePos = pos;
    }

    public float GetHitCheckRadius ()
    {//这个应该提供配表
      return 0.9f;
    }

    /**
     * @brief 获取模型名
     *
     * @return 
     */
    public string GetModel()
    {
      return m_Model;
    }

    /**
     * @brief 获取死亡/爆破资源
     * 
     * @return 
     */
    public string DeathModel
    {
      get {return m_DeathModel;}
      set { m_DeathModel = value;}
    }

    public string DeathEffect
    {
      get {return m_DeathEffect;}
      set { m_DeathEffect = value;}
    }

    public string DeathSound
    {
      get { return m_DeathSound; }
      set { m_DeathSound = value; }
    }

    /**
     * @brief 设置模型名
     *
     * @return 
     */
    public void SetModel(string model)
    {
      m_Model = model;
    }

    public void SetMuzzlePos(float x, float y, float z)
    {
      if (Geometry.IsInvalid(x) || Geometry.IsInvalid(y) || Geometry.IsInvalid(z)) {
        LogSystem.Debug("SetMuzzlePos:{0},{1},{2}, is invalid", x, y, z);
        Helper.LogCallStack();
      }
      m_ScriptMuzzlePos.X = x;
      m_ScriptMuzzlePos.Y = y;
      m_ScriptMuzzlePos.Z = z;
    }

    public void SetSecondMuzzlePos(float x, float y, float z)
    {
      if (Geometry.IsInvalid(x) || Geometry.IsInvalid(y) || Geometry.IsInvalid(z)) {
        LogSystem.Debug("SetSecondMuzzlePos:{0},{1},{2}, is invalid", x, y, z);
        Helper.LogCallStack();
      }
      m_ScriptSecondMuzzlePos.X = x;
      m_ScriptSecondMuzzlePos.Y = y;
      m_ScriptSecondMuzzlePos.Z = z;
    }
    
    /**
     * @brief 设置头部炮弹发射点
     *
     * @return 
     */
    public void SetBodyHeadFirePos (float x, float y, float z)
    {
      m_ScriptBodyHeadFirePos.X = x;
      m_ScriptBodyHeadFirePos.Y = y;
      m_ScriptBodyHeadFirePos.Z = z;
    }

    /**
     * @brief 获取ai列表
     *
     * @return 
     */
    public bool GetAIEnable()
    {
      return m_AIEnable;
    }

    /**
     * @brief 设置ai列表
     *
     * @return 
     */
    public void SetAIEnable(bool enable)
    {
      m_AIEnable = enable;
      if (IsUser) {
       //LogSystem.Debug("SetAIEnable:{0} user:{1} heroid:{2} name:{3}", enable, GetId(), GetLinkId(), GetName());
      }
    }

    /**
     * @brief 获取动作id
     *
     * @return 
     */
    public List<int> GetActionList()
    {
      return m_ActionList;
    }

    public Data_ActionConfig GetCurActionConfig()
    {
      int weapon_type = -1;
      if (GetShootStateInfo().GetCurWeaponInfo() != null) {
        weapon_type = GetShootStateInfo().GetCurWeaponInfo().WeaponType;
        //LogSystem.Debug("---npc control: weapon type={0}", weapon_type);
      }
      //LogSystem.Debug("---npc control: action list{0}={1}", GetActionList().Count, GetActionList());
      return ActionConfigProvider.Instance.GetCharacterCurActionConfig(GetActionList(), weapon_type);
    }

    /**
     * @brief 设置动作id
     *
     * @return 
     */
    public void SetActionList(List<int> id_list)
    {
      m_ActionList.Clear();
      m_ActionList.AddRange(id_list);
    }

    /**
     * @brief 获取弹幕id
     *
     * @return 
     */
    public int GetBarrageId()
    {
      return m_BarrageId;
    }

    /**
     * @brief 设置弹幕id
     *
     * @return 
     */
    public void SetBarrageId(int id)
    {
      m_BarrageId = id;
    }

    /**
     * @brief 阵营ID	
     */
    public int GetCampId()
    {
      if (null != m_ControllerObject) {
        return m_ControllerObject.GetCampId();
      }
      return m_CampId;
    }

    /**
     * @brief 阵营ID	
     */
    public void SetCampId(int val)
    {
      if (null != m_ControllerObject) {
        m_ControllerObject.SetCampId(val);
      }
      m_CampId = val;
    }

    public int KillerId
    {
      get { return m_KillerId; }
      set { m_KillerId = value; }
    }

    public long LastAttackedTime
    {
      get { return m_LastAttackedTime; }
      set { m_LastAttackedTime = value; }
    }

    public long LastAttackTime
    {
      get { return m_LastAttackTime; }
      set { m_LastAttackTime = value; }
    }
    public bool IsBlaze
    {
      get { return m_isBlaze; }
      set { m_isBlaze = value; }
    }

    public bool IsHurtComa
    {
      get { return m_IsHurtComa; }
      set { m_IsHurtComa = value; }
    }

    public MyDictionary<int, AttackerInfo> AttackerInfos
    {
      get { return m_AttackerInfos; }
    }
    /// <summary>
    /// 避让半径（1=1格，2=3格，3=5格，...）
    /// </summary>
    public int AvoidanceRadius
    {
      get { return m_AvoidanceRadius; }
      set { m_AvoidanceRadius = value; }
    }
    /// <summary>
    /// 掩体ID（使用掩体场景逻辑ID）
    /// </summary>
    public int BlindageId
    {
      get { return m_BlindageId; }
      set { m_BlindageId = value; }
    }
    /// <summary>
    /// 对象所处的掩体区域，如果不在掩体中，则为null
    /// </summary>
    public Vector3[] Blindage
    {
      get { return m_Blindage; }
      set { m_Blindage = value; }
    }
    public long BlindageLeftTime
    {
      get { return m_BlindageLeftTime; }
      set { m_BlindageLeftTime = value; }
    }

    public OnBeginAttackEvent  OnBeginAttack;

    public Shape Shape
    {
      get { return m_Shape; }
      set { m_Shape = value; }
    }

    public float ShootTimer
    {
      get { return m_ShootTimer; }
      set { m_ShootTimer = value; }
    }

    public float GetRadius()
    {
      float radius = 0;
      if (null != m_Shape)
        radius = m_Shape.GetRadius();
      return radius;
    }

    public bool IsHaveStateFlag(CharacterState_Type type)
    {
      return (m_StateFlag & ((int)type)) != 0;
    }

    public void SetStateFlag(Operate_Type opType, CharacterState_Type mask)
    {
      if (opType == Operate_Type.OT_AddBit) {
        m_StateFlag |= (int)mask;
      } else if (opType == Operate_Type.OT_RemoveBit) {
        m_StateFlag &= ~((int)mask);
      }
    }

    public int StateFlag
    {
      get { return m_StateFlag; }
      set { m_StateFlag = value; }
    }
    
    public float EnergyCoreNum
    {
      get { return m_EnergyCoreNum; }
      set
      {
        m_EnergyCoreNum = value > 0 ? value : 0;
        m_EnergyCoreChanged = true;
      }
    }
    public bool EnergyCoreChanged
    {
      get { return m_EnergyCoreChanged; }
      set { m_EnergyCoreChanged = value; }
    }

    public bool IsNoGunRun
    {
      get { return (m_LastAnimTimeMs + m_NoGunRunEnterTimeMs) <= TimeUtility.GetServerMilliseconds(); }
    }

    public long NoGunRunEnterTimeMs
    {
      get { return m_NoGunRunEnterTimeMs; }
      set { m_NoGunRunEnterTimeMs = value; }
    }

    public long LastAnimTime
    {
      get { return m_LastAnimTimeMs; }
      set { m_LastAnimTimeMs = value; }
    }


    /**
     * @brief 预估移动时间
     *
     * @param pos
     *
     * @return 
     */
    public float PredictMoveDuration (ScriptRuntime.Vector2 pos)
    {
      float distance = ScriptRuntime.Vector2.Distance(GetMovementStateInfo().GetPosition2D(), pos);
      float duration = distance / GetActualProperty().MoveSpeed;
      //duration += GameDefines.C_PredictMoveDurationRefix;

      return duration;
    }

    /**
     * @brief 基础属性值
     */
    public CharacterProperty GetBaseProperty()
    {
      return m_BaseProperty;
    }

    /**
     * @brief 当前属性值
     */
    public CharacterProperty GetActualProperty()
    {
      return m_ActualProperty;
    }

    /**
     * @brief 获取是否出场结束状态
     *
     */
    public bool GetComeOutOver()
    {
      return m_ComeOutOver;
    }

    /**
     * @brief 设置出场结束状态
     *
     */
    public void SetComeOutOver(bool ComeOutOver)
    {
      m_ComeOutOver = ComeOutOver;
    }

    public bool IsDead()
    {
      return Hp <= 0;
    }
    
    //临时加的，不要调用
    public uint GetActorID(){
      return 0;
    }
    
    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetHp(Operate_Type opType, int tVal)
    {
      m_Hp = (int)CharacterProperty.UpdateAttr(m_Hp, m_ActualProperty.HpMax, opType, tVal);
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetRage(Operate_Type opType, int tVal)
    {
      int result = (int)CharacterProperty.UpdateAttr(m_Rage, m_ActualProperty.RageMax, opType, tVal);
      if (result > m_ActualProperty.RageMax) {
        result = m_ActualProperty.RageMax;
      } else if (result < 0){
        result = 0;
      }
      m_Rage = result;
    }

    /**
     * @brief 角色属性修改
     *
     * @param optype 操作类型
     * @param val 值
     *
     */
    public void SetEnergy(Operate_Type opType, int tVal)
    {
      m_Energy = (int)CharacterProperty.UpdateAttr(m_Energy, m_ActualProperty.EnergyMax, opType, tVal);
    }

    public void SetEnergyCore(Operate_Type opType, float tVal)
    {
      EnergyCoreNum = CharacterProperty.UpdateAttr(m_EnergyCoreNum, m_ActualProperty.EnergyCoreMax, opType, tVal);
    }

    public void ResetAttackerInfo()
    {
      KillerId = 0;
      AttackerInfos.Clear();
      m_LastAttackedTime = 0;
      m_LastAttackTime = 0;
    }

    public void SetAttackerInfo(int attackId, int attackerType, bool isKiller, bool isShootDamage, bool isCritical, int hpDamage, int npDamage)
    {
      if (isKiller)
        KillerId = attackId;
      long curTime=TimeUtility.GetServerMilliseconds();
      LastAttackedTime = curTime;
      if (!AttackerInfos.ContainsKey(attackId)) {
        AttackerInfos.Add(attackId, new AttackerInfo { m_AttackerType = attackerType, m_AttackTime = curTime, m_HpDamage = hpDamage, m_NpDamage = npDamage });
      } else {
        AttackerInfo info = AttackerInfos[attackId];
        info.m_AttackTime = curTime;
        info.m_HpDamage += hpDamage;
        info.m_NpDamage += npDamage;
        AttackerInfos[attackId] = info;
      }
      if (IsNpc) {
        NpcManager.FireDamageEvent(GetId(), attackId, isShootDamage, isCritical, hpDamage, npDamage);
      } else {
        UserManager.FireDamageEvent(GetId(), attackId, isShootDamage, isCritical, hpDamage, npDamage);
      }
    }
    public void SetAttackTime()
    {
      LastAttackTime = TimeUtility.GetServerMilliseconds();
    }
    public void CalcBaseAttr()
    {
      float aMoveSpeed = GetBaseProperty().MoveSpeed;
      int aHpMax = GetBaseProperty().HpMax;
      int aEnergyMax = GetBaseProperty().EnergyMax;
      int aEnergyCoreMax = GetBaseProperty().EnergyCoreMax;
      int aCrgMax = GetBaseProperty().CrgMax;
      float aHpRecover = GetBaseProperty().HpRecover;
      float aEnergyRecover = GetBaseProperty().EnergyRecover;
      float aEnergyCoreRecover = GetBaseProperty().EnergyCoreRecover;
      int aAttackBase = GetBaseProperty().AttackBase;
      int aDefenceBase = GetBaseProperty().DefenceBase;
      float aCritical = GetBaseProperty().Critical;
      float aCriticalPow = GetBaseProperty().CriticalPow;
      float aArmorPenetration = GetBaseProperty().ArmorPenetration;
      float aEnergyIntensity = GetBaseProperty().EnergyIntensity;
      float aEnergyArmor = GetBaseProperty().EnergyArmor;

      float aAttackRange = GetBaseProperty().AttackRange;
      float aRps = GetBaseProperty().Rps;
      int aCrg = (int)GetBaseProperty().Crg;
      float aCht = GetBaseProperty().Cht;
      float aWdps = GetBaseProperty().Wdps;
      float aDamRandom = GetBaseProperty().DamRandom;

      if (null != m_LevelupConfig) {
        int rate = (m_Level > 0 ? m_Level - 1 : 0);
        aMoveSpeed += m_LevelupConfig.m_AttrData.GetAddSpd(0, 0) * rate;
        aHpMax += (int)(m_LevelupConfig.m_AttrData.GetAddHpMax(0, 0) * rate);
        aEnergyMax += (int)(m_LevelupConfig.m_AttrData.GetAddNpMax(0, 0) * rate);
        aEnergyCoreMax += (int)(m_LevelupConfig.m_AttrData.GetAddEpMax(0, 0) * rate);
        aCrgMax += (int)(m_LevelupConfig.m_AttrData.GetAddCrgMax(0, 0) * rate);
        aHpRecover += m_LevelupConfig.m_AttrData.GetAddHpRecover(0, 0) * rate;
        aEnergyRecover += m_LevelupConfig.m_AttrData.GetAddNpRecover(0, 0) * rate;
        aEnergyCoreRecover += m_LevelupConfig.m_AttrData.GetAddEpRecover(0, 0) * rate;
        aAttackBase += (int)(m_LevelupConfig.m_AttrData.GetAddAd(0, 0) * rate);
        aDefenceBase += (int)(m_LevelupConfig.m_AttrData.GetAddDp(0, 0) * rate);
        aCritical += m_LevelupConfig.m_AttrData.GetAddCri(0, 0) * rate;
        aCriticalPow += m_LevelupConfig.m_AttrData.GetAddPow(0, 0) * rate;
        aArmorPenetration += m_LevelupConfig.m_AttrData.GetAddAndp(0, 0) * rate;
        aEnergyIntensity += m_LevelupConfig.m_AttrData.GetAddAp(0, 0) * rate;
        aEnergyArmor += m_LevelupConfig.m_AttrData.GetAddTay(0, 0) * rate;

        aAttackRange += m_LevelupConfig.m_AttrData.GetAddRange(0, 0) * rate;
        aRps += m_LevelupConfig.m_AttrData.GetAddRps(0, 0) * rate;
        aCrg += (int)(m_LevelupConfig.m_AttrData.GetAddCrg(0, 0) * rate);
        aCht += m_LevelupConfig.m_AttrData.GetAddCht(0, 0) * rate;
        aWdps += m_LevelupConfig.m_AttrData.GetAddDps(0, 0) * rate;
        aDamRandom += m_LevelupConfig.m_AttrData.GetAddDamRange(0, 0) * rate;
      }

      GetActualProperty().SetMoveSpeed(Operate_Type.OT_Absolute, aMoveSpeed);
      GetActualProperty().SetHpMax(Operate_Type.OT_Absolute, aHpMax);
      GetActualProperty().SetRageMax(Operate_Type.OT_Absolute, GetBaseProperty().RageMax);

      GetActualProperty().SetEnergyMax(Operate_Type.OT_Absolute, aEnergyMax);
      GetActualProperty().SetEnergyCoreMax(Operate_Type.OT_Absolute, aEnergyCoreMax);
      GetActualProperty().SetCrgMax(Operate_Type.OT_Absolute, aCrgMax);
      GetActualProperty().SetHpRecover(Operate_Type.OT_Absolute, aHpRecover);
      GetActualProperty().SetEnergyRecover(Operate_Type.OT_Absolute, aEnergyRecover);
      GetActualProperty().SetEnergyCoreRecover(Operate_Type.OT_Absolute, aEnergyCoreRecover);
      GetActualProperty().SetAttackBase(Operate_Type.OT_Absolute, aAttackBase);
      GetActualProperty().SetDefenceBase(Operate_Type.OT_Absolute, aDefenceBase);
      GetActualProperty().SetCritical(Operate_Type.OT_Absolute, aCritical);
      GetActualProperty().SetCriticalPow(Operate_Type.OT_Absolute, aCriticalPow);
      GetActualProperty().SetArmorPenetration(Operate_Type.OT_Absolute, aArmorPenetration);
      GetActualProperty().SetEnergyIntensity(Operate_Type.OT_Absolute, aEnergyIntensity);
      GetActualProperty().SetEnergyArmor(Operate_Type.OT_Absolute, aEnergyArmor);

      GetActualProperty().SetAttackRange(Operate_Type.OT_Absolute, aAttackRange);
      GetActualProperty().SetRps(Operate_Type.OT_Absolute, aRps);
      GetActualProperty().SetCrg(Operate_Type.OT_Absolute, aCrg);
      GetActualProperty().SetCht(Operate_Type.OT_Absolute, aCht);
      GetActualProperty().SetWdps(Operate_Type.OT_Absolute, aWdps);
      GetActualProperty().SetDamRange(Operate_Type.OT_Absolute, aDamRandom);
    }

    public void TransformShape()
    {
      if(null!=m_Shape){
        m_Shape.Transform(GetMovementStateInfo().GetPosition3D(), GetMovementStateInfo().FaceDirCosAngle, GetMovementStateInfo().FaceDirSinAngle);
      }
    }

    public bool LevelChanged
    {
      get { return m_LevelChanged; }
      set { m_LevelChanged = value; }
    }

    /*
     * 一个对象控制另一个对象时，对控制对象的操作（武器、装备、技能）将转移到受控制对象（数据与表现）。
     * 1、位置在移动模块里让受控对象跟随控制对象（开始控制时将控制对象移动到受控对象位置）。
     * 2、逻辑上二个对象仍然是分离的，仅仅是控制的目标发生转移（为了保证体系上一致，这种转移在控制层处理【目前对应于客户端输入、客户端与服务器消息处理】）。
     * 3、只影响交互式操作的状态数据不转移，仍然影响控制对象而不是影响受控对象(这里需要仔细考虑是否逻辑概念)。
     * 4、玩家对控制对象的操作所基于的逻辑判断由各操作所需的信息决定（即如果操作依赖武器装备与技能则使用受控制对象的数据，否则用控制对象的数据）。
     * 5、表现上，受控对象成为显示主体，控制对象根据配置挂接到受控对象上或隐藏。
     * 6、战斗骑乘特殊，此时控制对象的操作不转移到受控对象，仅仅是位置跟随与表现上挂接（如果在骑乘时不允许战斗，则可以用没有战斗配置的机甲实现）。
     */
    public WeaponStateInfo GetRealControlledShootStateInfo()
    {
      if (null != m_ControlledObject && !m_ControlledObject.m_IsHorse)
        return m_ControlledObject.GetRealControlledShootStateInfo();
      return m_ShootStateInfo;
    }
    public SkillStateInfo GetRealControlledSkillStateInfo()
    {
      if (null != m_ControlledObject && !m_ControlledObject.m_IsHorse)
        return m_ControlledObject.GetRealControlledSkillStateInfo();
      return m_SkillStateInfo;
    }
    public EquipmentStateInfo GetRealControlledEquipmentStateInfo()
    {
      if (null != m_ControlledObject && !m_ControlledObject.m_IsHorse)
        return m_ControlledObject.GetRealControlledEquipmentStateInfo();
      return m_EquipmentStateInfo;
    }
    public CharacterInfo GetRealControlledObject()
    {
      if (null != m_ControlledObject && !m_ControlledObject.m_IsHorse)
        return m_ControlledObject.GetRealControlledObject();
      return this;
    }
    public CharacterInfo ControllerObject
    {
      get { return m_ControllerObject; }
      set { m_ControllerObject = value; }
    }
    public CharacterInfo ControlledObject
    {
      get { return m_ControlledObject; }
      set { m_ControlledObject = value; }
    }
    public bool CanControl
    {
      get { return m_IsMecha || m_IsHorse; }
    }
    public bool IsTask
    {
      get
      {
        return m_IsTask;
      }
    }
    public bool IsPvpTower
    {
      get
      {
        return m_IsPvpTower;
      }
    }
    public bool IsControlMecha
    {
      get
      {
        bool ret = false;
        if (null != m_ControlledObject && m_ControlledObject.m_IsMecha) {
          ret = true;
        }
        return ret;
      }
    }
    public bool IsControlHorse
    {
      get
      {
        bool ret = false;
        if (null != m_ControlledObject && m_ControlledObject.m_IsHorse) {
          ret = true;
        }
        return ret;
      }
    }

    public MovementStateInfo GetMovementStateInfo()
    {
      return m_MovementStateInfo;
    }
    public WeaponStateInfo GetShootStateInfo()
    {
      return m_ShootStateInfo;
    }
    public SkillStateInfo GetSkillStateInfo()
    {
      return m_SkillStateInfo;
    }
    public EquipmentStateInfo GetEquipmentStateInfo()
    {
      return m_EquipmentStateInfo;
    }
    public DashFireSpatial.ISpaceObject SpaceObject
    {
      get { return m_SpaceObject; }
    }
    public SceneContextInfo SceneContext
    {
      get { return m_SceneContext; }
      set { m_SceneContext = value; }
    }
    public ISpatialSystem SpatialSystem
    {
      get
      {
        ISpatialSystem sys = null;
        if (null != m_SceneContext) {
          sys = m_SceneContext.SpatialSystem;
        }
        return sys;
      }
    }
    public SceneLogicInfoManager SceneLogicInfoManager
    {
      get
      {
        SceneLogicInfoManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.SceneLogicInfoManager;
        }
        return mgr;
      }
    }
    public NpcManager NpcManager
    {
      get
      {
        NpcManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.NpcManager;
        }
        return mgr;
      }
    }
    public UserManager UserManager
    {
      get
      {
        UserManager mgr = null;
        if (null != m_SceneContext) {
          mgr = m_SceneContext.UserManager;
        }
        return mgr;
      }
    }
    public BlackBoard BlackBoard
    {
      get
      {
        BlackBoard blackBoard = null;
        if (null != m_SceneContext) {
          blackBoard = m_SceneContext.BlackBoard;
        }
        return blackBoard;
      }
    }

    public HilightType GetHilightType() { return m_HilightType; }
    public void SetHilightType(HilightType hilight)
    {
      m_HilightType = hilight;
    }

    public Vector4 GetHilightColor() { return m_HilightColor; }
    public bool IsHilightColorShowed { set { m_IsColorShowed = value; } get { return m_IsColorShowed; } }
    public void SetHilightColor(Vector4 color)
    {
      m_HilightColor = color;
      IsHilightColorShowed = false;
    }
    public void ResetColor()
    {
      m_HilightColor = m_OrigColor;
      IsHilightColorShowed = false;
    }

    public void MakeHitRecover(int recoverTimeMs, float revoverActionSpeed)
    {
      m_HitRecoverTimeMs = recoverTimeMs;
      m_HitRecoverActionSpeed = revoverActionSpeed;
      m_IsHitRecovering = true;
    }

    public void EndHitRecover()
    {
      m_HitRecoverTimeMs = 0;
      m_IsHitRecovering = false;
    }

    public void SetCanShoot(bool can_shoot) { m_CanShoot = can_shoot; }
    public bool CanShoot()
    {
      if (!m_CanShoot) {
        return false;
      }
      if (IsDead()) {
        return false;
      }
      if (IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return false;
      }
      if (IsHaveStateFlag(CharacterState_Type.CST_Disarm)) {
        return false;
      }
      if (null != m_ControlledObject) {
        return m_ControlledObject.CanShoot();
      }
      if (GetSkillStateInfo().IsSkillActivated()) {
        UserInfo user = this.CastUserInfo();
        if (null != user && user.GetAiStateInfo().AiLogic <= 0) {
          LogSystem.Debug("Character {0} CanShoot return false because IsSkillActivated", GetId());
        }
        return false;
      }
      return true;
    }

    public void SetCanUseSkill(bool can_use_skill) { m_CanUseSkill = can_use_skill; }
    public bool CanUseSkill()
    {
      if (!m_CanUseSkill) {
        return false;
      }
      if (IsDead()) {
        return false;
      }
      if (IsHaveStateFlag(CharacterState_Type.CST_Sleep)) {
        return false;
      }
      if (IsHaveStateFlag(CharacterState_Type.CST_Silence)) {
        return false;
      }
      if (GetSkillStateInfo().IsSkillActivated()) {
        return false;
      }
      if (null != m_ControlledObject) {
        return m_ControlledObject.CanUseSkill();
      }
      return true;
    }

    public bool IsHitRecovering() { return m_IsHitRecovering; }
    public int GetHitRecoverTimeMs() { return m_HitRecoverTimeMs; }
    public float HitRecoverActionSpeed
    {
      get { return m_HitRecoverActionSpeed; }
      set { m_HitRecoverActionSpeed = value; }
    }

    public void AddPressedKey(string key)
    {
      if (!m_PressedKeys.Contains(key.ToUpper())) {
        m_PressedKeys.Add(key.ToUpper());
      }
    }

    public void RemovePressKey(string key)
    {
      m_PressedKeys.Remove(key.ToUpper());
    }

    public bool IsKeyPressed(string key)
    {
      return m_PressedKeys.Contains(key.ToUpper());
    }

    protected void ResetCharacterInfo()
    {
      SetAIEnable(true);
      DeadTime = 0;
      
      IsMoving = false;
      m_CampId = 0;

      m_Blindage = null;
      m_BlindageId = 0;
      m_BlindageLeftTime = 0;
      
      m_ComeOutOver = false;
      m_BeAttack = false;

      m_CurEnemyId = 0;
      m_IsHitRecovering = false;
      IsFlying = false;

      m_ControllerObject = null;
      m_ControlledObject = null;

      m_Money = 0;
      m_TotalMoney = 0;
      m_ActionList.Clear();

      m_Exp = 0;
      m_Level = 0;
      m_Hp = 0;
      m_Energy = 0;
      m_EnergyCoreNum = 0;
      m_AttackRangeCoefficient = 1;
      m_VelocityCoefficient = 1;
      m_StateFlag = 0;
      OnBeginAttack = null;
      m_LogicDead = false;

      m_IsMecha = false;
      m_IsHorse = false;
      m_IsTask = false;
      m_IsPvpTower = false;

      m_isBlaze = false;
      m_IsHurtComa = false;
      m_CanShoot = true;
      m_CanUseSkill = true;
      m_KillerId = 0;
      m_ShootTimer = 0;

      m_CurBlueCanSeeMe = false;
      m_CurRedCanSeeMe = false;
      m_LastBlueCanSeeMe = false;
      m_LastRedCanSeeMe = false;

      m_NoGunRunEnterTimeMs = 5000;

      SetHp(Operate_Type.OT_Absolute, GetActualProperty().HpMax);
      SetRage(Operate_Type.OT_Absolute, 0);
      SetEnergy(Operate_Type.OT_Absolute, GetActualProperty().EnergyMax);

      ResetAttackerInfo();

      GetMovementStateInfo().Reset();
      GetShootStateInfo().Reset();
      GetSkillStateInfo().Reset();
      GetEquipmentStateInfo().Reset();
    }

    protected int m_Id = 0;
    /**
     * @brief 单位ID，在地图中赋予的id，用于剧情查找
     */
    protected int m_UnitId = 0;
    protected int m_LinkId = 0;
    protected string m_Name = "";
    protected float m_Money = 0;      //当前金钱数
    protected float m_TotalMoney = 0;   //累计金钱数

    protected int m_Exp = 0;
    protected int m_Level = 1;
    private bool m_LevelChanged = false;
    private int m_Hp = 0;
    private int m_Rage = 0;
    private int m_Energy = 0;
    private float m_AttackRangeCoefficient = 1;
    private float m_VelocityCoefficient = 1;
    private float m_ViewRange = 0;
    private float m_GohomeRange = 0;
    private float m_EnergyCoreNum = 0;
    private bool m_EnergyCoreChanged = false;
    private bool m_LogicDead = false;

    protected string m_Model = "";
    protected string m_DeathModel = "";
    protected string m_DeathEffect = "";
    protected string m_DeathSound = "";

    protected bool m_AIEnable = true;
    protected List<int> m_ActionList = new List<int>();
    protected int m_BarrageId = 0;
    private int m_CampId = 0;
    protected bool m_isBlaze = false;
    protected bool m_IsHurtComa = false;

    protected long m_NoGunRunEnterTimeMs = 2000;
    protected long m_LastAnimTimeMs = 0;

    private bool m_CanShoot = true;
    private bool m_CanUseSkill = true;

    private float m_ShootTimer = 0;
    private int m_KillerId = 0;
    /************************************************************************/
    /* 助攻列表                                                             */
    /************************************************************************/
    private MyDictionary<int, AttackerInfo> m_AttackerInfos = new MyDictionary<int, AttackerInfo>();
    private long m_LastAttackedTime = 0;
    private long m_LastAttackTime = 0;
    //掩体信息
    private int m_BlindageId = 0;
    private Vector3[] m_Blindage = null;
    private long m_BlindageLeftTime = 0;

    private Shape m_Shape = null;
    private int m_AvoidanceRadius = 1;
    private bool m_ComeOutOver = false;
    protected bool m_BeAttack = false;
    protected bool m_IsInShelter = false;
    private int m_StateFlag = 0;

    protected ScriptRuntime.Vector3 m_ScriptMuzzlePos = new ScriptRuntime.Vector3();
    protected ScriptRuntime.Vector3 m_ScriptSecondMuzzlePos = new ScriptRuntime.Vector3();
    protected ScriptRuntime.Vector3 m_MuzzlePos = new ScriptRuntime.Vector3();
    protected ScriptRuntime.Vector3 m_SecondMuzzlePos = new ScriptRuntime.Vector3();
    protected bool m_IsHaveScriptBodyCenter = false;
    protected ScriptRuntime.Vector3 m_ScriptBodyCenterPos = new ScriptRuntime.Vector3();
    protected ScriptRuntime.Vector3 m_ScriptBodyHeadFirePos = new ScriptRuntime.Vector3();
    protected ScriptRuntime.Vector3 m_MousePos = new ScriptRuntime.Vector3();
    private long m_ReleaseTime = 0;  //尸体存在时间
    private long m_DeadTime = 0;
    private int m_HeadUiPos = 110;
    private int m_CostType = 0;
    private float m_ShootBuffLifeTime = 1.0f;
    /**
     * @brief 基础属性值
     */
    protected CharacterProperty m_BaseProperty;
    /**
     * @brief 当前属性值
     */
    protected CharacterProperty m_ActualProperty;

    private MovementStateInfo m_MovementStateInfo = new MovementStateInfo();
    private WeaponStateInfo m_ShootStateInfo = new WeaponStateInfo();
    private SkillStateInfo m_SkillStateInfo = new SkillStateInfo();
    private EquipmentStateInfo m_EquipmentStateInfo = new EquipmentStateInfo();
    private SceneContextInfo m_SceneContext = null;
    protected ISpaceObject m_SpaceObject = null;
    protected LevelupConfig m_LevelupConfig = null;
    
    protected int m_CurEnemyId = 0;
    
    private HilightType m_HilightType = HilightType.kNone;
    private Vector4 m_HilightColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    private Vector4 m_OrigColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
    private bool m_IsColorShowed = true;

    private int m_HitRecoverTimeMs = 0;     // 硬直时间
    private float m_HitRecoverActionSpeed = 1.0f; // 硬直期间内的动作速度
    private bool m_IsHitRecovering = false; // 是否硬直

    private List<string> m_PressedKeys = new List<string>();
    private CellPos m_SightCell;
    private bool m_CurBlueCanSeeMe = false;
    private bool m_LastBlueCanSeeMe = false;
    private bool m_CurRedCanSeeMe = false;
    private bool m_LastRedCanSeeMe = false;
    
    private CharacterInfo m_ControllerObject = null;
    private CharacterInfo m_ControlledObject = null;

    protected bool m_IsMecha = false;
    protected bool m_IsHorse = false;
    protected bool m_IsTask = false;
    protected bool m_IsPvpTower = false;

    protected NpcInfo m_CastNpcInfo = null;
    protected UserInfo m_CastUserInfo = null;

    //阵营可为Friendly、Hostile、Blue、Red
    //Friendly 全部友好
    //Hostile 全部敌对
    //Blue 与Hostile与Red敌对
    //Red与Hostile与Blue敌对
    public static CharacterRelation GetRelation(CharacterInfo pObj_A, CharacterInfo pObj_B)
    {
      if (pObj_A == null || pObj_B == null) {
        return CharacterRelation.RELATION_ENEMY;
      }

      if (pObj_A == pObj_B) {
        return CharacterRelation.RELATION_FRIEND;
      }

      int campA = pObj_A.GetCampId();
      int campB = pObj_B.GetCampId();
      return GetRelation(campA, campB);
    }
    public static CharacterRelation GetRelation(int campA, int campB)
    {
      CharacterRelation relation = CharacterRelation.RELATION_INVALID;
      if ((int)CampIdEnum.Unkown != campA && (int)CampIdEnum.Unkown != campB) {
        if (campA == campB)
          relation = CharacterRelation.RELATION_FRIEND;
        else if (campA == (int)CampIdEnum.Friendly || campB == (int)CampIdEnum.Friendly)
          relation = CharacterRelation.RELATION_FRIEND;
        else if (campA == (int)CampIdEnum.Hostile || campB == (int)CampIdEnum.Hostile)
          relation = CharacterRelation.RELATION_ENEMY;
        else
          relation = CharacterRelation.RELATION_ENEMY;
      }
      return relation;
    }
    public static void ControlObject(CharacterInfo controller, CharacterInfo controlled)
    {
      if (null != controller && null != controlled) {
        ReleaseControlObject(controller, controlled);
        controller.ControlledObject = controlled;
        controlled.ControllerObject = controller;
      }
    }
    public static void ReleaseControlObject(CharacterInfo controller, CharacterInfo controlled)
    {
      ReleaseControlledObject(controller);
      ReleaseControllerObject(controlled);
    }
    public static void ReleaseControlledObject(CharacterInfo controller)
    {
      if (null != controller) {
        CharacterInfo controlled = controller.ControlledObject;
        controller.ControlledObject = null;
        if (null != controlled)
          controlled.ControllerObject = null;
      }
    }
    public static void ReleaseControllerObject(CharacterInfo controlled)
    {
      if (null != controlled) {
        CharacterInfo controller = controlled.ControllerObject;
        controlled.ControllerObject = null;
        if (null != controller)
          controller.ControlledObject = null;
      }
    }
    public static bool CanSee(CharacterInfo source, CharacterInfo target)
    {
      bool ret = false;
      if (null != source && null != target) {
        Vector3 pos1 = source.GetMovementStateInfo().GetPosition3D();
        Vector3 pos2 = target.GetMovementStateInfo().GetPosition3D();
        float distSqr = DashFire.Geometry.DistanceSquare(pos1, pos2);
        return CanSee(source, target, distSqr, pos1, pos2);
      }
      return ret;
    }
    public static bool CanSee(CharacterInfo source, CharacterInfo target, float distSqr, Vector3 pos1, Vector3 pos2)
    {
      bool ret = false;
      if (null != source && null != target) {
        //一、先判断距离
        if (distSqr < source.ViewRange * source.ViewRange) {
          //二、再判断逻辑
          //后面修改的同学注意下：
          //1、我们目前的object层是数据接口层，是不需要使用多态的。概念变化的可能性比功能变化的可能性要小很多，所以我们将多态机制应用到Logic里。
          //2、逻辑上的影响可能是对象buff或类型产生，如果判断逻辑比较复杂，采用结构化编程的风格拆分成多个函数即可。
          //3、另一个不建议用多态理由是这个函数的调用频率会很高。
          if (source.GetCampId() == target.GetCampId() ||
            (!target.IsHaveStateFlag(CharacterState_Type.CST_Hidden))) {//隐身状态判断（未考虑反隐）
            //判断掩体（草丛）,目标不在草丛或者与源在同一草丛
            if (null == target.Blindage || source.BlindageId == target.BlindageId) {
              //三、最后判断空间关系
              ret = source.SpatialSystem.CanSee(pos1, pos2);
              if (ret && source.IsUser && target.IsNpc) {
                int row1, col1, row2, col2;
                target.SpatialSystem.GetCellMapView(1).GetCell(pos1, out row1, out col1);
                target.SpatialSystem.GetCellMapView(1).GetCell(pos2, out row2, out col2);
                byte status1 = target.SpatialSystem.GetCellStatus(row1, col1);
                byte status2 = target.SpatialSystem.GetCellStatus(row2, col2);
                byte lvl1 = BlockType.GetBlockLevel(status1);
                byte lvl2 = BlockType.GetBlockLevel(status2);
                if (lvl1 < lvl2) {
                  LogSystem.Debug("user {0}({1},{2}:{3}) can see user {4}({5},{6}:{7})", source.GetLinkId(), row1, col1, status1, target.GetLinkId(), row2, col2, status2);
                  ret = source.SpatialSystem.CanSee(pos1, pos2);
                }
              }
            }
          }
        }
      }
      return ret;
    }
  }
}
