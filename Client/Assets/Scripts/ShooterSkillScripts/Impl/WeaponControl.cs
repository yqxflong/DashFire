using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public enum MasterWeaponType {
  Master = 0,
  SubMaster = 1,
}
[System.Serializable]
public class SwitchSkillWeaponInfoPair {
  public int MasterWeaponId = -1;
  public int SubMasterWeaponId = -1;
}
public class WeaponControl : MonoBehaviour {
  int m_CurWeaponId = -1;
  int m_CurSubWeaponId = -1;
  Dictionary<WeaponHand, GameObject> m_Weapons = new Dictionary<WeaponHand, GameObject>();
  private int m_CurWeaponIndex = 0;
  public List<SwitchSkillWeaponInfoPair> ShootSwitchWeaponIds = new List<SwitchSkillWeaponInfoPair>();

  //Config
  public string BoneLeftHand = "ef_lefthand";
  public string BoneRightHand = "ef_righthand";
  public int DefaultWeaponId = -1;

  void Start() {
    if (DefaultWeaponId >= 0) {
      ChangeDefaultWeapon();
    }
  }
  public void ChangeWeapon(SkillWeaponInfo info) {
    if (info.IsCurWeapon) {
#if SHOOTER_LOG
      Debug.Log("CurWeapon:" + info.Id);
#endif
      m_CurWeaponId = info.Id;
    }
    else if (info.IsCurSubWeapon) {
#if SHOOTER_LOG
      Debug.Log("CurWeapon:" + info.Id);
#endif
      m_CurSubWeaponId = info.Id;
    }

    WeaponCmd operate = info.Operate;
    if (WeaponCmd.Hold == operate) {
      DropWeapon(info.HoldType);
      HoldWeapon(info.HoldType, info);
    } else if (WeaponCmd.HoldOnly == operate) {
      DropWeapon(WeaponHand.LeftHand);
      DropWeapon(WeaponHand.RightHand);
      HoldWeapon(info.HoldType, info);
    } else if (WeaponCmd.Drop == operate) {
      DropWeapon(info.HoldType);
    } else if (WeaponCmd.DropAll == operate) {
      DropWeapon(WeaponHand.LeftHand);
      DropWeapon(WeaponHand.RightHand);
    } else if (WeaponCmd.HoldDefault == operate) {
      SkillWeaponInfo defaltInfo = TriggerImpl.GetWeaponInfoById(this.gameObject, DefaultWeaponId);
      if (defaltInfo != null && defaltInfo.Operate != WeaponCmd.HoldDefault) {
        ChangeWeapon(defaltInfo);
      }
    } else if (WeaponCmd.HoldMain == operate) {
      SkillWeaponInfo masterWeaponInfo = TriggerImpl.GetCurWeaponInfo(this.gameObject, MasterWeaponType.Master);
      SkillWeaponInfo subMasterWeaponInfo = TriggerImpl.GetCurWeaponInfo(this.gameObject, MasterWeaponType.SubMaster);
      if (masterWeaponInfo != null && masterWeaponInfo.Operate != WeaponCmd.HoldMain) {
        TriggerImpl.ChangeWeapon(this.gameObject, masterWeaponInfo);
      }
      if (subMasterWeaponInfo != null && masterWeaponInfo.Operate != WeaponCmd.HoldMain) {
        TriggerImpl.ChangeWeapon(this.gameObject, subMasterWeaponInfo);
      }
    }
  }
  public void ChangeDefaultWeapon() {
    SkillWeaponInfo info = new SkillWeaponInfo();
    info.Operate = WeaponCmd.HoldDefault;
    ChangeWeapon(info);
  }
  public GameObject GetWeaponByHand(WeaponHand type) {
    if (m_Weapons.ContainsKey(type)) {
      return m_Weapons[type];
    }
    return null;
  }
  public SkillWeaponInfo GetCurMainWeaponInfo(MasterWeaponType masterType = MasterWeaponType.Master) {
    int weaponId = GetCurWeaponId(masterType);
    SkillWeaponInfo info = TriggerImpl.GetWeaponInfoById(this.gameObject, weaponId);
    return info;
  }
  public int GetCurWeaponId(MasterWeaponType masterType = MasterWeaponType.Master) {
    return (masterType == MasterWeaponType.Master) ? m_CurWeaponId : m_CurSubWeaponId;
  }
  public GameObject GetCurMainWeapon(MasterWeaponType masterType = MasterWeaponType.Master) {
    SkillWeaponInfo info = GetCurMainWeaponInfo(masterType);
    if (info == null) {
      return null;
    }
    return GetWeaponByHand(info.HoldType);
  }
  private void HoldWeapon(WeaponHand type, SkillWeaponInfo info) {
    GameObject newWeapon = CreateNewWeapon(info);
    if (newWeapon != null) {
#if SHOOTER_LOG
      Debug.Log("HoldWeapon:" + newWeapon.name);
#endif
      m_Weapons.Add(info.HoldType, newWeapon);
      Script_Util.AttachGameObject(this.gameObject, newWeapon, GetBoneName(info.HoldType));
    }
  }
  private void DropWeapon(WeaponHand type) {
    if (m_Weapons.ContainsKey(type)) {
      GameObject weapon = m_Weapons[type];
      Script_Util.DetachGameObject(weapon);
      GameObject.DestroyObject(weapon);
      m_Weapons.Remove(type);
    }

    // Make Sure Remove All Children
    Transform t = LogicSystem.FindChildRecursive(this.gameObject.transform, GetBoneName(type));
    if (t != null) {
      int childCount = t.childCount;
      for (int index = 0; index < childCount; index++) {
        Transform child = t.GetChild(index);
        child.parent = null;
        GameObject.Destroy(child.gameObject);
      }
      t.DetachChildren();
    }
  }
  private GameObject CreateNewWeapon(SkillWeaponInfo info) {
    GameObject tWeaponObj = null;
    if (info.WeaponAsset != null) {
      tWeaponObj = Script_Util.CreateGameObjectByAsset(info.WeaponAsset);
    } else {
      tWeaponObj = Script_Util.CreateGameObjectByName(info.WeaponName);
    }
    return tWeaponObj;
  }
  private string GetBoneName(WeaponHand type) {
    switch (type) {
      case WeaponHand.LeftHand: {
          return BoneLeftHand;
        }
      case WeaponHand.RightHand: {
          return BoneRightHand;
        }
    }
    return "";
  }
  public void SwitchNextWeapon() {
    if (ShootSwitchWeaponIds != null && ShootSwitchWeaponIds.Count > 0) {
      m_CurWeaponIndex = (m_CurWeaponIndex + 1) % ShootSwitchWeaponIds.Count;
      SwitchSkillWeaponInfoPair weaponIdPair = ShootSwitchWeaponIds[m_CurWeaponIndex];
      if (weaponIdPair != null) {
        TriggerImpl.ChangeWeaponById(this.gameObject, weaponIdPair.MasterWeaponId);
        TriggerImpl.ChangeWeaponById(this.gameObject, weaponIdPair.SubMasterWeaponId);
      }
    }
  }
}
