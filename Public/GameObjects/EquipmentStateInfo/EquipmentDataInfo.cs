using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class EquipmentDataInfo
  {
    public EquipmentConfig EquipmentConfig
    {
      get { return m_EquipmentConfig; }
      set { m_EquipmentConfig = value; }
    }
    public List<EquipmentLevelupConfig> EquipmentLevelupConfig
    {
      get { return m_EquipmentLevelupConfig; }
    }

    public float GetAddHpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddHpMax(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddHpMax(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddNpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddNpMax(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddNpMax(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddEpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig)
      {
        ret += m_EquipmentConfig.m_AttrData.GetAddEpMax(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig)
      {
        if (null != cfg)
        {
          ret += cfg.m_AttrData.GetAddEpMax(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddCrgMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddCrgMax(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddCrgMax(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddAd(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddAd(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddAd(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddDp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddDp(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddDp(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddRps(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddRps(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddRps(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddCrg(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddCrg(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddCrg(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddCht(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddCht(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddCht(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddRange(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddRange(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddRange(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddDps(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddDps(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddDps(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddDamRange(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddDamRange(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddDamRange(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddCri(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddCri(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddCri(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddPow(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddPow(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddPow(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddAndp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddAndp(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddAndp(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddHpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddHpRecover(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddHpRecover(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddNpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddNpRecover(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddNpRecover(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddEpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig)
      {
        ret += m_EquipmentConfig.m_AttrData.GetAddEpRecover(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig)
      {
        if (null != cfg)
        {
          ret += cfg.m_AttrData.GetAddEpRecover(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddAp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddAp(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddAp(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddTay(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddTay(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddTay(refVal, refLevel);
        }
      }
      return ret;
    }
    public float GetAddSpd(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_EquipmentConfig) {
        ret += m_EquipmentConfig.m_AttrData.GetAddSpd(refVal, refLevel);
      }
      foreach (EquipmentLevelupConfig cfg in m_EquipmentLevelupConfig) {
        if (null != cfg) {
          ret += cfg.m_AttrData.GetAddSpd(refVal, refLevel);
        }
      }
      return ret;
    }

    private EquipmentConfig m_EquipmentConfig = null;
    private List<EquipmentLevelupConfig> m_EquipmentLevelupConfig = new List<EquipmentLevelupConfig>();
  }
  public class EquipmentInfo
  {
    public const int c_MaxWeaponNum = 6;
    public const int c_MaxPendantNum = 3;
    public EquipmentDataInfo[] Weapons
    {
      get { return m_Weapons; }
    }
    public EquipmentDataInfo BodyArmor
    {
      get { return m_BodyArmor; }
      set { m_BodyArmor = value; }
    }
    public EquipmentDataInfo[] Pendant
    {
      get { return m_Pendant; }
    }
    public void Reset()
    {
      for (int ix = 0; ix < c_MaxWeaponNum; ++ix) {
        m_Weapons[ix] = null;
      }
      m_BodyArmor = null;
      for (int ix = 0; ix < c_MaxPendantNum; ++ix) {
        m_Pendant[ix] = null;
      }
    }

    private EquipmentDataInfo[] m_Weapons = new EquipmentDataInfo[c_MaxWeaponNum];
    private EquipmentDataInfo m_BodyArmor = null;
    private EquipmentDataInfo[] m_Pendant = new EquipmentDataInfo[c_MaxPendantNum];
  }
}
