using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class WeaponConfigProvider
  {
    #region Singleton
    private static WeaponConfigProvider s_instance_ = new WeaponConfigProvider();
    public static WeaponConfigProvider Instance
    {
      get { return s_instance_; }
    }
    #endregion
    
    DataTemplateMgr<WeaponLogicData> weaponLogicDataMgr;  // 武器数据容器
    
    private WeaponConfigProvider()
    {
      weaponLogicDataMgr = new DataTemplateMgr<WeaponLogicData>();
    }

    /**
     * @brief 读取数据
     *
     * @param node
     *
     * @return 
     */
    public bool CollectData(string file, string rootLabel)
    {
      bool result = false;
      result = weaponLogicDataMgr.CollectDataFromDBC(file, rootLabel);

      return result;
    }

    /**
     * @brief 提取数据
     *
     * @param node
     *
     * @return 
     */
    public IData ExtractData(int id)
    {
      IData result = null;
      result = weaponLogicDataMgr.GetDataById(id);

      return result;
    }
  }
}
