using System;
using System.Collections.Generic;
using System.Text;

namespace DashFire
{
  /**
   * @brief
   *   weapon data
   */
  public class WeaponLogicData : IData
  {
    public enum WeaponType
    {
      WT_MINIGUN               = 0,             
      WT_SMALLGUN              = 1,
      WT_BIGGUN                = 2,
    }
    
    // 武器Id
    public int m_WeaponId;     
    // 武器名字
    public string m_WeaponName;        
    // 资源名字
    public string m_ResourceName;  
    // 武器的类型
    public int m_WeaponType;
    // 武器对应装备数据
    public int m_EquipmentId;

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_WeaponId = DBCUtil.ExtractNumeric<int>(node, "Id", -1, true);
      m_WeaponName = DBCUtil.ExtractString(node, "Name", "", true);
      m_ResourceName = DBCUtil.ExtractString(node, "ResourceName", "", false);
      m_WeaponType = DBCUtil.ExtractNumeric<int>(node, "WeaponType", -1, false);
      m_EquipmentId = DBCUtil.ExtractNumeric<int>(node, "EquipmentId", -1, false);
      return true;
    }

    /**
     * @brief 获取数据ID
     *
     * @return 
     */
    public int GetId()
    {
      return m_WeaponId;
    }
  }
}
