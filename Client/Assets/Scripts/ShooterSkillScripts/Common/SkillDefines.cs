using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Animation
/// </summary>
[System.Serializable]
public class SkillAnimInfo {
  public bool IsValid = true;
  public AnimationClip AnimName = null;
  public float AnimSpeed = 1.0f;
  public float AnimCrossFadeTime = 0.05f;
  public int AnimLayer = 10;
  public float AnimWeight = 1.0f;
  public WrapMode AnimWrapMode = WrapMode.Once;
  public SkillAnimInfo Clone() {
    SkillAnimInfo newData = new SkillAnimInfo();
    newData.IsValid = IsValid;
    newData.AnimName = AnimName;
    newData.AnimSpeed = AnimSpeed;
    newData.AnimCrossFadeTime = AnimCrossFadeTime;
    newData.AnimLayer = AnimLayer;
    newData.AnimWeight = AnimWeight;
    newData.AnimWrapMode = AnimWrapMode;
    return newData;
  }
}
/// <summary>
/// Effect
/// Hero_FX/2_Shooter/6_Hero_QiangShou_HuoGuang_01,1.0,0 0 0,0 0 0,ef_weapon01
/// </summary>
[System.Serializable]
public class SkillEffectInfo {
  public int Id = 0;
  public bool IsValid = true;
  public string EffectName = null;
  public GameObject EffectAsset = null;
  public float EffectLiftTime = 1.0f;
  public Vector3 EffectPos = Vector3.zero;
  public Vector3 EffectRot = Vector3.zero;
  public string EffectParentBone = null;
  public Transform EffectParent = null;
  public int EffectSoundId = -1;
  public SkillEffectInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 1);
    if (result != null) {
      Id = Convert.ToInt32(result[0]);
      if (result.Length >= 2) {
        EffectName = result[1];
      }
      if (result.Length >= 3) {
        EffectLiftTime = Convert.ToSingle(result[2]);
      }
      if (result.Length >= 4) {
        EffectPos = Script_Util.ToVector3(result[3]);
      }
      if (result.Length >= 5) {
        EffectRot = Script_Util.ToVector3(result[4]);
      }
      if (result.Length >= 6) {
        EffectParentBone = result[5];
      }
      if (result.Length >= 7) {
        EffectSoundId = Convert.ToInt32(result[6]);
      }
    } else {
      IsValid = false;
    }
  }
  public SkillEffectInfo() { }
  public SkillEffectInfo Clone() {
    SkillEffectInfo newData = new SkillEffectInfo();
    newData.Id = Id;
    newData.IsValid = IsValid;
    newData.EffectName = EffectName;
    newData.EffectAsset = EffectAsset;
    newData.EffectLiftTime = EffectLiftTime;
    newData.EffectPos = EffectPos;
    newData.EffectRot = EffectRot;
    newData.EffectParentBone = EffectParentBone;
    newData.EffectParent = EffectParent;
    newData.EffectSoundId = EffectSoundId;
    return newData;
  }
}
/// <summary>
/// Sound
/// </summary>
[System.Serializable]
public class SkillSoundInfo {
  public int Id = 0;
  public string SoundName = null;
  public AudioClip SoundAsset = null;
  public float SoundVolume = 1.0f;
  public float SoundPitch = 1.0f;
  public bool SoundLoop = false;
  public ulong SoundDelay = 0;
}
/// <summary>
/// Weapon
/// </summary>
public enum WeaponCmd {
  Hold,
  Drop,
  DropAll,
  HoldDefault,
  HoldOnly,
  HoldMain,
}
public enum WeaponHand {
  LeftHand,
  RightHand,
}
[System.Serializable]
public class SkillWeaponInfo {
  public bool IsValid = true;
  public int Id = 0;
  public string WeaponName;
  public GameObject WeaponAsset;
  public WeaponCmd Operate;
  public WeaponHand HoldType;
  public bool IsCurWeapon = true;
  public bool IsCurSubWeapon = false;
  public string SpriteName = "";
  public SkillWeaponInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 6);
    if (result != null) {
      Id = Convert.ToInt32(result[0]);
      WeaponName = result[1];
      Operate = (WeaponCmd)Convert.ToInt32(result[2]);
      HoldType = (WeaponHand)Convert.ToInt32(result[3]);
      IsCurWeapon = Convert.ToBoolean(result[4]);
      IsCurSubWeapon = Convert.ToBoolean(result[5]);
      if (result.Length > 6) {
        SpriteName = result[6];
      }
    } else {
      IsValid = false;
    }
  }
  public SkillWeaponInfo() { }
  public SkillWeaponInfo Clone() {
    SkillWeaponInfo newData = new SkillWeaponInfo();
    newData.IsValid = IsValid;
    newData.Id = Id;
    newData.WeaponName = WeaponName;
    newData.WeaponAsset = WeaponAsset;
    newData.Operate = Operate;
    newData.HoldType = HoldType;
    newData.IsCurWeapon = IsCurWeapon;
    newData.IsCurSubWeapon = IsCurSubWeapon;
    newData.SpriteName = SpriteName;
    return newData;
  }
}
/// <summary>
/// Impact
/// </summary>
[System.Serializable]
public class SkillImpactInfoData {
  public TargetStateType TargetStateType = TargetStateType.STAND;
  public ImpactInfo ImpactInfo = null;
  public SkillImpactInfoData Clone() {
    SkillImpactInfoData newData = new SkillImpactInfoData();
    newData.TargetStateType = TargetStateType;
    newData.ImpactInfo = (ImpactInfo)ImpactInfo.Clone();
    return newData;
  }
}
[System.Serializable]
public class SkillImpactInfos {
  public int Id = 0;
  public float ImpactRadius = 0.7f;
  public ImpactInfo ImpactDefault = null;
  public int LockFrameInfoId = 0;
  public int MoveScaleInfoId = 0;
  public List<TargetStateType> ExceptTargetState = new List<TargetStateType>();
  public List<SkillImpactInfoData> ImpactList = new List<SkillImpactInfoData>();
  public SkillImpactInfos Clone() {
    SkillImpactInfos newData = new SkillImpactInfos();
    newData.Id = Id;
    newData.ImpactRadius = ImpactRadius;
    newData.LockFrameInfoId = LockFrameInfoId;
    newData.MoveScaleInfoId = MoveScaleInfoId;
    newData.ImpactDefault = (ImpactInfo)ImpactDefault.Clone();
    newData.ExceptTargetState = new List<TargetStateType>(ExceptTargetState);
    newData.ImpactList = new List<SkillImpactInfoData>();
    foreach (SkillImpactInfoData info in ImpactList) {
      newData.ImpactList.Add(info.Clone());
    }
    return newData;
  }
}

/// <summary>
/// SceneObj
/// </summary>
[System.Serializable]
public class SkillSceneObjInfo {
  public bool IsValid;
  public int Id = 0;
  public List<string> paramOther = new List<string>();
  public SkillSceneObjInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 1);
    if (result != null) {
      Id = Convert.ToInt32(result[0]);
      if (result.Length > 1) {
        for (int index = 1; index < result.Length; index++) {
          paramOther.Add(result[index]);
        }
      }
    } else {
      IsValid = false;
    }
  }
  public SkillSceneObjInfo() { }
  public string ExtractString(int index) {
    if (index >= 0 && index < paramOther.Count) {
      return paramOther[index];
    }
    return "";
  }
  public bool ExtractBool(int index) {
    string ret = ExtractString(index);
    if (string.IsNullOrEmpty(ret)) {
      return false;
    }
    if (ret.Trim().ToLower() == "true" || ret.Trim().ToLower() == "1") {
      return true;
    }
    if (ret.Trim().ToLower() == "false" || ret.Trim().ToLower() == "0") {
      return false;
    }
    return false;
  }
  public T ExtractNumeric<T>(int index, T defualtVal) {
    T result = defualtVal;
    try {
      string ret = ExtractString(index);
      if (!string.IsNullOrEmpty(ret)) {
        result = (T)Convert.ChangeType(ret, typeof(T));
      }
    } catch (System.Exception ex) {
      string errorInfo = string.Format("ExtractNumeric index:{0} ex:{1}", index, ex);
      Debug.Log(errorInfo);
    }
    return result;
  }
  public Vector3 ExtractVector3(int index) {
    string ret = ExtractString(index);
    if (!string.IsNullOrEmpty(ret)) {
      return Script_Util.ToVector3(ret);
    }
    return Vector3.zero;
  }
  public SkillSceneObjInfo Clone() {
    SkillSceneObjInfo newData = new SkillSceneObjInfo();
    newData.Id = Id;
    newData.IsValid = IsValid;
    newData.paramOther = new List<string>();
    foreach (string info in paramOther) {
      newData.paramOther.Add(info);
    }
    return newData;
  }
}

public enum SceneLayerType {
  Ground = 1 << 0,
  TransparentFX = 1 << 1,
  IngoreRaycast = 1 << 2,
  Reserved1 = 1 << 3,
  Water = 1 << 4,
  Reserved2 = 1 << 5,
  Reserved3 = 1 << 6,
  Reserved4 = 1 << 7,
  Character = 1 << 8,
  SkillCall = 1 << 9,
  Dead = 1 << 10,
  SkillHit = 1 << 11,
  AirWall = 1 << 12,
  SceneObj = 1 << 13,
  SceneObjEffect = 1 << 14,
  Player = 1 << 15,
  Terrains = 1 << 16,

  //Combine
  SceneStatic = Ground | Terrains,
}

[System.Serializable]
public class MathRange {
  public float From = 0;
  public float To = 0;
  public MathRange() {
  }
  public float Clip(float val) {
    Nomalize();
    val = Mathf.Max(From, val);
    val = Mathf.Min(To, val);
    return val;
  }
  private void Nomalize() {
    From = Mathf.Min(From, To);
    To = Mathf.Max(From, To);
  }
}
[System.Serializable]
public class SkillLockFrameInfo {
  public bool IsValid = true;
  public int Id = 0;
  public string AnimName = "";
  public AnimationClip AnimAsset = null;
  public float Duration = 0.0f;
  public float AnimSpeed = 1.0f;
  public float AnimSpeedReset = 1.0f;
  public bool AnimUseCurve = false;
  public AnimationCurve AnimSpeedCurve = null;
  public bool NeedTarget = false;
  public SkillLockFrameInfo() { }
  public SkillLockFrameInfo Clone() {
    SkillLockFrameInfo newData = new SkillLockFrameInfo();
    newData.IsValid = IsValid;
    newData.Id = Id;
    newData.AnimName = AnimName;
    newData.AnimAsset = AnimAsset;
    newData.Duration = Duration;
    newData.AnimSpeed = AnimSpeed;
    newData.AnimSpeedReset = AnimSpeedReset;
    newData.AnimUseCurve = AnimUseCurve;
    newData.AnimSpeedCurve = AnimSpeedCurve;
    newData.NeedTarget = NeedTarget;
    return newData;
  }
}
[System.Serializable]
public class SkillMoveScaleInfo {
  public bool IsValid = true;
  public int Id = 0;
  public float Duration = 0.0f;
  public float MoveScale = 1.0f;
  public bool NeedTarget = false;
  public SkillMoveScaleInfo() { }
  public SkillMoveScaleInfo Clone() {
    SkillMoveScaleInfo newData = new SkillMoveScaleInfo();
    newData.IsValid = IsValid;
    newData.Id = Id;
    newData.Duration = Duration;
    newData.MoveScale = MoveScale;
    newData.NeedTarget = NeedTarget;
    return newData;
  }
}
