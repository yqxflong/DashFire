using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class ItemDataInfo
  {
    public int ItemId
    {
      get
      {
        int id = 0;
        if (null != m_ItemConfig)
          id = m_ItemConfig.m_ItemId;
        return id;
      }
    }
    public int ItemNum
    {
      get { return m_ItemNum; }
      set { m_ItemNum = value; }
    }
    public int UseLogicId
    {
      get
      {
        int id = 0;
        if (null != ItemConfig)
          id = ItemConfig.m_UseLogicId;
        return id;
      }
    }
    public ItemConfig ItemConfig
    {
      get { return m_ItemConfig; }
      set
      {
        m_ItemConfig = value;
      }
    }
    public ItemDataInfo()
    {
    }

    public float GetAddHpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddHpMax(refVal,refLevel);
      }
      return ret;
    }
    public float GetAddNpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddNpMax(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddEpMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig)
      {
        ret += m_ItemConfig.m_AttrData.GetAddEpMax(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddCrgMax(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddCrgMax(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddAd(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddAd(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddDp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddDp(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddRps(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddRps(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddCrg(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddCrg(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddCht(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddCht(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddRange(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddRange(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddDps(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddDps(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddDamRange(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddDamRange(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddCri(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddCri(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddPow(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddPow(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddAndp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddAndp(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddHpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddHpRecover(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddNpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddNpRecover(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddEpRecover(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig)
      {
        ret += m_ItemConfig.m_AttrData.GetAddEpRecover(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddAp(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddAp(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddTay(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddTay(refVal, refLevel);
      }
      return ret;
    }
    public float GetAddSpd(float refVal, int refLevel)
    {
      float ret = 0;
      if (null != m_ItemConfig) {
        ret += m_ItemConfig.m_AttrData.GetAddSpd(refVal, refLevel);
      }
      return ret;
    }

    private int m_ItemNum = 0;
    private ItemConfig m_ItemConfig = null;
  }
}
