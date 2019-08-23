using System;
using System.Collections.Generic;

namespace DashFire
{
  public enum WeaponMode
  {
    LOGIC_ID_START               = 10000,               // 武器形态Id基数
    LOGIC_ID_COMMON              = LOGIC_ID_START + 1,  // 普通状态
    LOGIC_ID_SKILL               = LOGIC_ID_START + 2,  // 技能状态
    LOGIC_ID_CONTINUATION        = LOGIC_ID_START + 3,  // 连发状态
  }
  
  public class WeaponInfo
  {
    //player info
    public int WeaponId;
    public bool IsShoot;
    
    public string WeaponName;
    public string ResourceName;
    public float CurCritical;
    public int EquipmentId;
    public int WeaponType;

    public WeaponLogicData ConfigData;

    // 初始化数据信息
    public WeaponInfo(int weaponId) 
    {
      WeaponId = weaponId;
      WeaponType = -1;
      IsShoot = false;

      //table value
      WeaponName = "";
      ResourceName = "";
      CurCritical = 0;
      EquipmentId = 1;

      ConfigData = (WeaponLogicData)WeaponConfigProvider.Instance.ExtractData(weaponId);
      if (null != ConfigData) {
        WeaponName = ConfigData.m_WeaponName;
        ResourceName = ConfigData.m_ResourceName;
        EquipmentId = ConfigData.m_EquipmentId;
        WeaponType = ConfigData.m_WeaponType;
      } else {
      }
    }

    private MyAction<WeaponInfo> mStatusCB;
    public void OnAddedToShootStateInfo(MyAction<WeaponInfo> cb)
    {
      mStatusCB = cb;
    }
  }
}
