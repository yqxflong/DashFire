using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{  
  public class WeaponStateInfo
  {
    public WeaponStateInfo()
    {
      m_WeaponChanged = false;
      m_CurrentWeaponIndex = 0;
      m_NextWeaponIndexOffset = 0;
      m_WeaponList = new List<WeaponInfo>();
      m_CurWeaponInfo = null;
      LeftChangeWeaponTime = 0.0f;
    }

    public void Reset()
    {
      m_WeaponChanged = false;
      m_CurrentWeaponIndex = 0;
      m_NextWeaponIndexOffset = 0;
      m_CurWeaponInfo = null;
      m_WeaponList.Clear();
      LeftChangeWeaponTime = 0.0f;
    }

    public bool IsChangingWeapon()
    {
      return LeftChangeWeaponTime > 0;
    }

    //add
    public void RemoveAllWeapon()
    {
      m_WeaponList.Clear();
    }
    
    public void RemoveWeapon(int weaponId)
    {
      int idx = m_WeaponList.FindIndex(info => info.WeaponId == weaponId);
      if (-1 != idx)
        m_WeaponList.RemoveAt(idx);
    }
    
    public WeaponInfo GetCurWeaponInfo()
    {
      return m_CurWeaponInfo;
    }

    public WeaponInfo GetWeaponInfoById(int weaponId)
    {
      return m_WeaponList.Find(
        delegate(WeaponInfo info) {
          return info.WeaponId == weaponId;
        }
      );
    }
    
    public void SetCurWeaponInfo(int weaponId, bool forceSet = false)
    {
      WeaponInfo weaponInfo = m_WeaponList.Find(
        delegate(WeaponInfo info)
        {
          return info.WeaponId == weaponId;
        }
      );
      if (null != weaponInfo)
      {
        m_CurWeaponInfo = weaponInfo;
        if (null != m_WeaponStatusCB)
          m_WeaponStatusCB(m_CurWeaponInfo);
      } else if (forceSet) {
        // 强制换枪，不需要本身拥有枪
        WeaponInfo info = new WeaponInfo(weaponId);
        WeaponLogicData weaponData = info.ConfigData;
        if (null != weaponData) {
          m_CurWeaponInfo = info;
          if (null != m_WeaponStatusCB) {
            m_WeaponStatusCB(m_CurWeaponInfo);
          }
        }
      }
    }
    
    public List<WeaponInfo> GetAllWeapon()
    {
      return m_WeaponList;
    }
    
    public void AddWeapon(int index, WeaponInfo info)
    {
      int idx = m_WeaponList.FindIndex(item => info.WeaponId == item.WeaponId);
      m_WeaponList.Insert(idx == -1 ? index : idx, info);
      info.OnAddedToShootStateInfo(wi => { if (null != m_WeaponStatusCB) m_WeaponStatusCB(wi); });
    }
  
    public int CurrentWeaponIndex
    {
      get {
        return m_CurrentWeaponIndex;
      }  
      set {
        m_CurrentWeaponIndex = value;
      }
    }
    
    public int NextWeaponIndexOffset
    {
      get {
        return m_NextWeaponIndexOffset;
      }  
      set {
        m_NextWeaponIndexOffset = value;
      }
    }
    
    public bool WeaponChanged
    {
      get {
        return m_WeaponChanged;
      }  
      set {
        m_WeaponChanged = value;
      }
    }

    public float LeftChangeWeaponTime
    {
      get {
        return m_LeftChangeWeaponTime;
      }  
      set {
        m_LeftChangeWeaponTime = value;
      }
    }

    protected bool m_WeaponChanged;
    protected int m_CurrentWeaponIndex;
    protected int m_NextWeaponIndexOffset;
    protected List<WeaponInfo> m_WeaponList;  // 武器容器
    protected WeaponInfo m_CurWeaponInfo;     // 当前武器
    protected float m_LeftChangeWeaponTime;

    public delegate void WeaponStatusCallback(WeaponInfo wi);
    private WeaponStatusCallback m_WeaponStatusCB;
    
    public static void AddSubWeapon(CharacterInfo entity, int weaponId)
    {
      if (null == entity) return;
      WeaponInfo wpnInfo = entity.GetShootStateInfo().GetAllWeapon().Find(
        delegate(WeaponInfo info) {
          return info.WeaponId == weaponId;
        }
      );
      if (null != wpnInfo) {
        return;
      }
      WeaponInfo weaponInfo = new WeaponInfo(weaponId);
      WeaponLogicData weaponData = weaponInfo.ConfigData;
      if (null != weaponData) {
        //暴击数值
        float CRIRATE_ = (float)(entity.GetActualProperty().Critical / 480.0);
        float CRIRATE_C_ = CriticalConfigProvider.Instance.GetC(CRIRATE_);
        weaponInfo.CurCritical = CRIRATE_C_;

        int existWeaponCount = entity.GetShootStateInfo().GetAllWeapon().Count;
        entity.GetShootStateInfo().AddWeapon(existWeaponCount, weaponInfo);
        //todo:先按武器表里的武器配置上，后续需要从db里读取升级数据并初始化升级配置数据
        EquipmentDataInfo equipDataInfo = new EquipmentDataInfo();
        equipDataInfo.EquipmentConfig = EquipmentConfigProvider.Instance.GetEquipmentConfigById(weaponData.m_EquipmentId);
        entity.GetEquipmentStateInfo().EquipmentInfo.Weapons[existWeaponCount] = equipDataInfo;
      }
    }
  }
}
