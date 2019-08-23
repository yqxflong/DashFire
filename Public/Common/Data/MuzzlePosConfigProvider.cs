using System;
using System.Collections.Generic;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  [Serializable]
  public class MuzzlePosConfigData : IData
  {
    public int m_Id = 0;
    public int m_UsagerId = 0;
    public int m_WeaponId = 0;
    public Vector3 m_MuzzlePos = new Vector3(0, 0, 0);

    public bool CollectDataFromDBC(DBC_Row node)
    {
      m_Id = DBCUtil.ExtractNumeric<int>(node, "Id", 0, true);
      m_UsagerId = DBCUtil.ExtractNumeric<int>(node, "UsagerId", 0, true);
      m_WeaponId = DBCUtil.ExtractNumeric<int>(node, "WeaponId", 0, true);
      string pos = DBCUtil.ExtractString(node, "MuzzlePos", "0 0 0", true);
      m_MuzzlePos = Converter.ConvertVector3D(pos);
      return true;
    }

    public int GetId()
    {
      return m_Id;
    }
  }
  public class MuzzlePosConfigProvider
  {
    public Vector3 GetPlayerMuzzlePos(int linkId, int weaponId)
    {
      return GetMuzzlePosHelper(m_PlayerMuzzlePos, linkId, weaponId);
    }
    public Vector3 GetNpcMuzzlePos(int linkId, int weaponId)
    {
      return GetMuzzlePosHelper(m_NpcMuzzlePos, linkId, weaponId);
    }

    public void Load(string playerMuzzleConfigFile, string npcMuzzleConfigFile)
    {
      m_PlayerMuzzleConfigMgr.CollectDataFromDBC(playerMuzzleConfigFile, "MuzzleConfig");
      m_NpcMuzzleConfigMgr.CollectDataFromDBC(npcMuzzleConfigFile, "MuzzleConfig");

      BuildMuzzlePosData(m_PlayerMuzzleConfigMgr, m_PlayerMuzzlePos);
      BuildMuzzlePosData(m_NpcMuzzleConfigMgr, m_NpcMuzzlePos);
    }

    private void BuildMuzzlePosData(DataTemplateMgr<MuzzlePosConfigData> configData,MyDictionary<int, MyDictionary<int, Vector3>> posData)
    {
      foreach (MuzzlePosConfigData cfg in configData.GetData().Values) {
        MyDictionary<int, Vector3> poses = null;
        if (!posData.ContainsKey(cfg.m_UsagerId)) {
          poses=new MyDictionary<int,Vector3>();
          posData.Add(cfg.m_UsagerId, poses);
        } else {
          poses = posData[cfg.m_UsagerId];
        }
        if (null != poses) {
          if (!poses.ContainsKey(cfg.m_WeaponId)) {
            poses.Add(cfg.m_WeaponId, cfg.m_MuzzlePos);
          } else {
            poses[cfg.m_WeaponId] = cfg.m_MuzzlePos;
          }
        }
      }
    }

    private static Vector3 GetMuzzlePosHelper(MyDictionary<int, MyDictionary<int, Vector3>> muzzlePosData, int linkId, int weaponId)
    {
      Vector3 ret = new Vector3();
      if (muzzlePosData.ContainsKey(linkId)) {
        MyDictionary<int, Vector3> poses = muzzlePosData[linkId];
        if (poses.ContainsKey(weaponId)) {
          ret = poses[weaponId];
        }
      }
      return ret;
    }

    private DataTemplateMgr<MuzzlePosConfigData> m_PlayerMuzzleConfigMgr = new DataTemplateMgr<MuzzlePosConfigData>();
    private DataTemplateMgr<MuzzlePosConfigData> m_NpcMuzzleConfigMgr = new DataTemplateMgr<MuzzlePosConfigData>();

    private MyDictionary<int, MyDictionary<int, Vector3>> m_PlayerMuzzlePos = new MyDictionary<int, MyDictionary<int, Vector3>>();
    private MyDictionary<int, MyDictionary<int, Vector3>> m_NpcMuzzlePos = new MyDictionary<int, MyDictionary<int, Vector3>>();

    public static MuzzlePosConfigProvider Instance
    {
      get { return s_Instance; }
    }
    private static MuzzlePosConfigProvider s_Instance = new MuzzlePosConfigProvider();
  }
}
