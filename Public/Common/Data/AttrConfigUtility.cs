using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  /**
   * 属性表格填表规则：
   * 1、0～100000000，表示此值为直接添加到属性上的值，用整数表示2位精度浮点数，实际值为表格中的值/100（如9999表示99.99，这个值直接加到对应属性值）
   * 2、100000000~200000000，表示此值为百分比加成，用整数表示2位精度浮点百分数值，实际值为表格中的值/10000（如:9999表示99.99%，实际值为0.9999，这个值将乘以对应属性后再加到对应属性值）
   * 3、200000000~，表示此值为按角色等级加成系数，用整数表示2位精度浮点值，实际值为表格中的值/100（如9999表示99.99，这个值将乘以角色等级后再加到对应属性值）
   * 4、规则1～3中对应范围值的负值表示与1～3规则相同，只不过最后会从对应属性值扣除相应加成数
   */
  [Serializable]
  public class AttrDataConfig
  {
    public const int c_MaxAbsoluteValue = 100000000;
    public const int c_MaxPercentValue = 200000000;
    public const float c_Rate = 100.0f;
    public enum ValueType : int
    {
      AbsoluteValue = 0,
      PercentValue,
      LevelRateValue,
    }

    public void CollectDataFromDBC(DBC_Row node)
    {
      m_AddHpMax = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddHpMax", 0, false),out m_HpMaxType);
      m_AddRageMax = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddRageMax", 0, false), out m_HpMaxType);
      m_AddNpMax = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddNpMax", 0, false), out m_NpMaxType);
      m_AddEpMax = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddEpMax", 0, false), out m_EpMaxType);
      m_AddCrgMax = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddCrgMax", 0, false), out m_CrgMaxType);
      m_AddAd = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddAd", 0, false), out m_AdType);
      m_AddDp = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddDp", 0, false), out m_DpType);
      m_AddRps = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddRps", 0, false), out m_RpsType);
      m_AddCrg = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddCrg", 0, false), out m_CrgType);
      m_AddCht = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddCht", 0, false), out m_ChtType);
      m_AddRange = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddRange", 0, false), out m_RangeType);
      m_AddDps = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddDps", 0, false), out m_DpsType);
      m_AddDamRange = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddDamRange", 0, false), out m_DamRangeType);
      m_AddCri = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddCri", 0, false), out m_CriType);
      m_AddPow = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddPow", 0, false), out m_PowType);
      m_AddAndp = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddAndp", 0, false), out m_AndpType);
      m_AddHpRecover = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddHpRecover", 0, false), out m_HpRecoverType);
      m_AddNpRecover = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddNpRecover", 0, false), out m_NpRecoverType);
      m_AddEpRecover = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddEpRecover", 0, false), out m_EpRecoverType);
      m_AddAp = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddAp", 0, false), out m_ApType);
      m_AddTay = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddTay", 0, false), out m_TayType);
      m_AddSpd = CalcRealValue(DBCUtil.ExtractNumeric<int>(node, "AddSpd", 0, false), out m_SpdType);
    }

    public float GetAddHpMax(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddHpMax, m_HpMaxType);
    }
    public float GetAddRageMax(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddRageMax, m_HpMaxType);
    }
    public float GetAddNpMax(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddNpMax, m_NpMaxType);
    }
    public float GetAddEpMax(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddEpMax, m_EpMaxType);
    }
    public float GetAddCrgMax(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddCrgMax, m_CrgMaxType);
    }
    public float GetAddAd(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddAd, m_AdType);
    }
    public float GetAddDp(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddDp, m_DpType);
    }
    public float GetAddRps(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddRps, m_RpsType);
    }
    public float GetAddCrg(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddCrg, m_CrgType);
    }
    public float GetAddCht(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddCht, m_ChtType);
    }
    public float GetAddRange(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddRange, m_RangeType);
    }
    public float GetAddDps(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddDps, m_DpsType);
    }
    public float GetAddDamRange(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddDamRange, m_DamRangeType);
    }
    public float GetAddCri(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddCri, m_CriType);
    }
    public float GetAddPow(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddPow, m_PowType);
    }
    public float GetAddAndp(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddAndp, m_AndpType);
    }
    public float GetAddHpRecover(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddHpRecover, m_HpRecoverType);
    }
    public float GetAddNpRecover(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddNpRecover, m_NpRecoverType);
    }
    public float GetAddEpRecover(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddEpRecover, m_EpRecoverType);
    }
    public float GetAddAp(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddAp, m_ApType);
    }
    public float GetAddTay(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddTay, m_TayType);
    }
    public float GetAddSpd(float refVal, int refLevel)
    {
      return CalcAddedAttrValue(refVal, refLevel, m_AddSpd, m_SpdType);
    }

    private float CalcRealValue(int tableValue,out int type)
    {
      float retVal = 0;
      int val=tableValue;
      bool isNegative=false;
      if(tableValue<0){
        isNegative=true;
        val = -val;
      }
      if (val < c_MaxAbsoluteValue) {
        retVal = val / c_Rate;
        type = (int)ValueType.AbsoluteValue;
      } else if (val < c_MaxPercentValue) {
        retVal = (val - c_MaxAbsoluteValue) / c_Rate / 100;
        type = (int)ValueType.PercentValue;
      } else {
        retVal = (val - c_MaxPercentValue) / c_Rate;
        type = (int)ValueType.LevelRateValue;
      }
      if(isNegative)
        retVal = -retVal;
      return retVal;
    }

    private float CalcAddedAttrValue(float refVal, int refLevel, float addVal, int type)
    {
      float retVal = 0;
      switch (type) {
        case (int)ValueType.AbsoluteValue:
          retVal = addVal;
          break;
        case (int)ValueType.PercentValue:
          retVal = refVal * addVal;
          break;
        case (int)ValueType.LevelRateValue:
          retVal = refLevel * addVal;
          break;
      }
      return retVal;
    }

    private float m_AddHpMax = 0;
    private float m_AddRageMax = 0;
    private int m_HpMaxType = 0;
    private float m_AddNpMax = 0;
    private int m_NpMaxType = 0;
    private float m_AddEpMax = 0;
    private int m_EpMaxType = 0;
    private float m_AddCrgMax = 0;
    private int m_CrgMaxType = 0;
    private float m_AddAd = 0;
    private int m_AdType = 0;
    private float m_AddDp = 0;
    private int m_DpType = 0;
    private float m_AddRps = 0;
    private int m_RpsType = 0;
    private float m_AddCrg = 0;
    private int m_CrgType = 0;
    private float m_AddCht = 0;
    private int m_ChtType = 0;
    private float m_AddRange = 0;
    private int m_RangeType = 0;
    private float m_AddDps = 0;
    private int m_DpsType = 0;
    private float m_AddDamRange = 0;
    private int m_DamRangeType = 0;
    private float m_AddCri = 0;
    private int m_CriType = 0;
    private float m_AddPow = 0;
    private int m_PowType = 0;
    private float m_AddAndp = 0;
    private int m_AndpType = 0;
    private float m_AddHpRecover = 0;
    private int m_HpRecoverType = 0;
    private float m_AddNpRecover = 0;
    private int m_NpRecoverType = 0;
    private float m_AddEpRecover = 0;
    private int m_EpRecoverType = 0;
    private float m_AddAp = 0;
    private int m_ApType = 0;
    private float m_AddTay = 0;
    private int m_TayType = 0;
    private float m_AddSpd = 0;
    private int m_SpdType = 0;
  }

  public class LevelupConfig : IData
  {
    public int m_Id = 0;
    public string m_Description = "";
    public AttrDataConfig m_AttrData = new AttrDataConfig();
    
    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      m_Description = DBCUtil.ExtractString(node, "Description", "", true);
      m_AttrData.CollectDataFromDBC(node);
      return true;
    }

    public int GetId()
    {
      return m_Id;
    }
  }
}
