using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public static class TriggerImpl {

  public delegate SkillSoundInfo GetSkillSoundInfoEvent(int id);
  public static GetSkillSoundInfoEvent OnGetSkillSoundInfo = null;
  //////////////////////////////////////////////////////////////////////////
  // Weapon
  public static GameObject GetCurMainWeapon(GameObject target, MasterWeaponType masterType = MasterWeaponType.Master) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("ShooterSkillExecutor WeaponControl Script miss!");
      return null;
    }
    return control.GetCurMainWeapon(masterType);
  }
  public static GameObject GetWeaponByHand(GameObject target, WeaponHand hand) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("ShooterSkillExecutor WeaponControl Script miss!");
      return null;
    }
    return control.GetWeaponByHand(hand);
  }
  public static SkillWeaponInfo GetCurWeaponInfo(GameObject target, MasterWeaponType masterType = MasterWeaponType.Master) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("ShooterSkillExecutor WeaponControl Script miss!");
      return null;
    }
    return control.GetCurMainWeaponInfo(masterType);
  }
  public static int GetCurWeaponId(GameObject target, MasterWeaponType masterType = MasterWeaponType.Master) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("ShooterSkillExecutor GetCurWeaponId Script miss!");
      return -1;
    }
    return control.GetCurWeaponId(masterType);
  }
  public static SkillWeaponInfo GetWeaponInfoById(GameObject target, int id) {
    SkillSceneObjManager tSceneObjMgr = target.GetComponent<SkillSceneObjManager>();
    if (tSceneObjMgr == null) {
      LogicSystem.GfxLog("GetCurWeapon miss!");
      return null;
    }
    return tSceneObjMgr.TryGetSkillWeaponInfo(id);
  }
  public static void ChangeWeapon(GameObject target, SkillWeaponInfo param) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("ShooterSkillExecutor WeaponControl Script miss!");
    }
    control.ChangeWeapon(param);
  }
  public static void ChangeWeaponById(GameObject target, int id) {
    SkillWeaponInfo info = GetWeaponInfoById(target, id);
    if (info != null) {
      ChangeWeapon(target, info);
    }
  }
  public static void GetWeaponInfoByCategory(GameObject target, int id) {
    SkillWeaponInfo info = GetWeaponInfoById(target, id);
    if (info != null) {
      ChangeWeapon(target, info);
    }
  }
  public static float GetWeaponHeight(GameObject target, MasterWeaponType masterType = MasterWeaponType.Master) {
    GameObject curWeapon = GetCurMainWeapon(target, masterType);
    if (curWeapon != null) {
      return Mathf.Abs(target.transform.position.y - curWeapon.transform.position.y);
    }
    return 0.0f;
  }
  public static void SwitchNextWeapon(GameObject target) {
    WeaponControl control = target.GetComponent<WeaponControl>();
    if (control == null) {
      LogicSystem.GfxLog("SwitchWeapon WeaponControl Script miss!");
      return;
    }
    control.SwitchNextWeapon();
  }
  //////////////////////////////////////////////////////////////////////////
  // Anim
  public static bool PlayAnim(GameObject target, SkillAnimInfo info) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null || info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
      LogicSystem.GfxLog("PlayAnim animation component miss!");
      return false;
    }
    animCom[info.AnimName.name].speed = info.AnimSpeed;
    animCom[info.AnimName.name].wrapMode = info.AnimWrapMode;
    animCom[info.AnimName.name].layer = info.AnimLayer;
    animCom.CrossFade(info.AnimName.name, info.AnimCrossFadeTime);
    return true;
  }
  // Anim
  public static bool PlayAnims(GameObject target, params SkillAnimInfo[] infos) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null) {
      LogicSystem.GfxLog("PlayAnim animation component miss!");
      return false;
    }

    for (int i = 0; i < infos.Length; ++i) {
      SkillAnimInfo info = infos[i];
      if (info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
        continue;
      }
      animCom[info.AnimName.name].speed = info.AnimSpeed;
      animCom[info.AnimName.name].wrapMode = info.AnimWrapMode;
      animCom[info.AnimName.name].layer = info.AnimLayer;
      animCom.PlayQueued(info.AnimName.name, QueueMode.CompleteOthers);
    }

    return true;
  }
  public static bool StopAnim(GameObject target, SkillAnimInfo info) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null || info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
      LogicSystem.GfxLog("PlayAnim animation component miss!");
      return false;
    }
    animCom.Stop(info.AnimName.name);
    return true;
  }
  public static bool StopAllAnim(GameObject target) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null) {
      LogicSystem.GfxLog("PlayAnim animation component miss!");
      return false;
    }
    animCom.Stop();
    return true;
  }
  public static bool FadeOutAnim(GameObject target, SkillAnimInfo info) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null || info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
      LogicSystem.GfxLog("PlayAnim animation component miss!");
      return false;
    }
    animCom[info.AnimName.name].weight = 0.0f;
    return true;
  }
  public static float GetAnimTime(GameObject target, SkillAnimInfo info, float tweak = 0.0f) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null || info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
      LogicSystem.GfxLog("GetAnimTime animation component miss!");
      return 0;
    }
    float speed = Mathf.Max(info.AnimSpeed, 0.001f);
    return animCom[info.AnimName.name].length / speed - tweak;
  }

  public static bool IsAnimPlaying(GameObject target, SkillAnimInfo info) {
    Animation animCom = target.GetComponent<Animation>();
    if (animCom == null || info == null || info.AnimName == null || animCom[info.AnimName.name] == null) {
      LogicSystem.GfxLog("IsAnimPlaying animation component miss!");
      return false;
    }
    return animCom.IsPlaying(info.AnimName.name) && animCom[info.AnimName.name].normalizedTime < 1.0f;
  }
  //////////////////////////////////////////////////////////////////////////
  // Effect
  public static GameObject PlayEffect(SkillEffectInfo info) {
    GameObject tEffectObj = null;
    if (info.EffectAsset != null) {
      tEffectObj = Script_Util.CreateGameObjectByAsset(info.EffectAsset);
    } else {
      tEffectObj = Script_Util.CreateGameObjectByName(info.EffectName);
    }

    if (tEffectObj != null) {
      tEffectObj.transform.rotation = Quaternion.Euler(info.EffectRot);
      tEffectObj.transform.position = info.EffectPos;
      if (tEffectObj.particleSystem != null) {
        tEffectObj.particleSystem.Play(true);
      }
      if (OnGetSkillSoundInfo != null) {
        SkillSoundInfo sInfo = OnGetSkillSoundInfo(info.EffectSoundId);
        if (sInfo != null) {
          TriggerImpl.PlaySoundAtTarget(tEffectObj, sInfo);
        }
      }
      GameObject.Destroy(tEffectObj, Script_Util.ForceNotifyEffectTime(info.EffectLiftTime));
    } else {
      LogicSystem.GfxLog("PlayEffect create failed!");
    }
    return tEffectObj;
  }
  public static GameObject AttachEffect(SkillEffectInfo info) {
    GameObject tEffectObj = null;
    if (info.EffectAsset != null) {
      tEffectObj = Script_Util.CreateGameObjectByAsset(info.EffectAsset);
    } else {
      tEffectObj = Script_Util.CreateGameObjectByName(info.EffectName);
    }
    if (null != tEffectObj) {
      bool ret = Script_Util.AttachGameObject(info.EffectParent, tEffectObj.transform, Script_Util.ForceNotifyEffectBone(info.EffectParentBone));
      if (ret) {
        tEffectObj.transform.localPosition = info.EffectPos;
        tEffectObj.transform.localRotation = Quaternion.Euler(info.EffectRot);
      }
      if (OnGetSkillSoundInfo != null) {
        SkillSoundInfo sInfo = OnGetSkillSoundInfo(info.EffectSoundId);
        if (sInfo != null) {
          TriggerImpl.PlaySoundAtTarget(tEffectObj, sInfo);
        }
      }
      GameObject.Destroy(tEffectObj, Script_Util.ForceNotifyEffectTime(info.EffectLiftTime));
    } else {
      LogicSystem.GfxLog("AttachEffect effect create failed!");
    }
    return tEffectObj;
  }
  public static GameObject AttachEffectDirect(SkillEffectInfo info) {
    GameObject tEffectObj = null;
    if (info.EffectAsset != null) {
      tEffectObj = Script_Util.CreateGameObjectByAsset(info.EffectAsset);
    } else {
      tEffectObj = Script_Util.CreateGameObjectByName(info.EffectName);
    }
    if (null != tEffectObj) {
      bool ret = Script_Util.AttachGameObjectDirect(info.EffectParent, tEffectObj.transform);
      if (ret) {
        tEffectObj.transform.localPosition = info.EffectPos;
        tEffectObj.transform.localRotation = Quaternion.Euler(info.EffectRot);
      }
      if (OnGetSkillSoundInfo != null) {
        SkillSoundInfo sInfo = OnGetSkillSoundInfo(info.EffectSoundId);
        if (sInfo != null) {
          TriggerImpl.PlaySoundAtTarget(tEffectObj, sInfo);
        }
      }
      GameObject.Destroy(tEffectObj, Script_Util.ForceNotifyEffectTime(info.EffectLiftTime));
    } else {
      LogicSystem.GfxLog("AttachEffect effect create failed!");
    }
    return tEffectObj;
  }
  //////////////////////////////////////////////////////////////////////////
  // Sound
  public static void PlaySoundAtTarget(GameObject target, SkillSoundInfo info) {
    AudioSource source = target.GetComponent<AudioSource>();
    if (source == null) {
      source = target.AddComponent<AudioSource>();
    }
    AudioClip tSoundObj = null;
    if (info.SoundAsset != null) {
      tSoundObj = info.SoundAsset;
    } else {
      tSoundObj = Resources.Load(info.SoundName) as AudioClip;
    }
    if (tSoundObj != null) {
      source.clip = info.SoundAsset;
      source.volume = info.SoundVolume;
      source.pitch = info.SoundPitch;
      source.loop = info.SoundLoop;
      source.Play(info.SoundDelay);
    }
  }
  public static void PlaySoundAtPos(SkillSoundInfo info, Vector3 pos) {
    AudioClip tSoundObj = null;
    if (info.SoundAsset != null) {
      tSoundObj = info.SoundAsset;
    } else {
      tSoundObj = Resources.Load(info.SoundName) as AudioClip;
    }
    if (tSoundObj != null) {
      GameObject go = new GameObject("Audio:" + tSoundObj.name);
      go.transform.position = pos;
      AudioSource source = go.AddComponent<AudioSource>();
      source.clip = tSoundObj;
      source.volume = info.SoundVolume;
      source.pitch = info.SoundPitch;
      source.loop = info.SoundLoop;
      source.Play(info.SoundDelay);
      if (!info.SoundLoop) {
        GameObject.DestroyObject(go, info.SoundAsset.length);
      }
    }
  }
  //////////////////////////////////////////////////////////////////////////
  // Skill
  public static IShooterSkill GetSkillScripObjectById(GameObject target, int skillId) {
    Component[] components = target.GetComponents(typeof(IShooterSkill));
    if (components != null && components.Length > 0) {
      foreach (Component component in components) {
        if (component is IShooterSkill && ((IShooterSkill)component).GetSkillId() == skillId) {
          return component as IShooterSkill;
        }
      }
    }
    return null;
  }
  public static IShooterSkill GetCurSkill(GameObject target) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("GetCurSkill skill manager miss!");
      return null;
    }
    return mgr.GetCurPlaySkill();
  }
  public static void ChangeSkillByCategory(GameObject target, SkillCategory category, int skillId) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("GetCurSkill skill manager miss!");
      return;
    }
    mgr.ChangeSkillByCategory(category, skillId);
  }
  public static void ForceStartSkillById(GameObject target, SkillCategory category, Vector3 targetpos) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("GetCurSkill skill manager miss!");
      return;
    }
    ShooterControlHandler control = (ShooterControlHandler)(mgr.GetSkillController());
    if (control != null) {
      control.ForceStartSkillById(category, targetpos);
    }
  }
  //////////////////////////////////////////////////////////////////////////
  // Camp
  public static int GetCharacterCamp(GameObject target) {
    if (target != null) {
      Component campCom = target.GetComponent<CharacterCamp>();
      if (campCom != null) {
        return ((CharacterCamp)campCom).m_CampId;
      }
    }
    return 0;
  }
  //////////////////////////////////////////////////////////////////////////
  // Impact
  public static void ProcessImpact(GameObject attacker, GameObject sender,
    SkillImpactInfos impacts, int hitCount = 1) {
    bool isTargeted = false;
    // Find Targets
    Collider[] hitColliders = Physics.OverlapSphere(sender.transform.position,
      impacts.ImpactRadius, (int)SceneLayerType.Character);
    int index = 0;
    while (index < hitColliders.Length) {
      if (hitColliders[index] == null || hitColliders[index].gameObject == null) {
        index++;
        continue;
      }

      GameObject target = hitColliders[index].gameObject;
      if (target != null) {
        ImpactInfo curImpact = ExtractBestImpactInfo(target, impacts);
        if (curImpact != null) {
          Vector3 faceDir = target.transform.position - sender.transform.position;
          faceDir.y = 0;
          ImpactInfo m_ImpactInfo = curImpact.Clone() as ImpactInfo;
          m_ImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir)
            * m_ImpactInfo.m_Velocity;
          m_ImpactInfo.Attacker = sender;
          ImpactSystem.Instance.SendImpact(attacker, target, m_ImpactInfo, hitCount);
          TriggerImpl.RecordTarget(attacker, target);
          isTargeted = true;
        }
      }
      index++;
    }
    ProcessImpactLock(attacker, impacts, isTargeted);
  }
  public static void ProcessVerticalImpact(GameObject attacker, GameObject sender,
    SkillImpactInfos impacts, float verticalHeight = 10.0f, float verticalHeightStart = 0.0f,
    float SectorDegree = 360.0f, int hitCount = 1) {
    bool isTargeted = false;
    // Find Targets
    Ray ray = new Ray(
      sender.transform.position + new Vector3(0, verticalHeightStart - impacts.ImpactRadius, 0),
    Vector3.up);
    RaycastHit[] targetHitInfos = Physics.SphereCastAll(ray, impacts.ImpactRadius,
      verticalHeight, (int)SceneLayerType.Character);
    Script_Util.DrawPhysicsSphereCastLine(ray, verticalHeight, impacts.ImpactRadius, Color.red, 10.0f);
    int index = 0;
    while (index < targetHitInfos.Length) {
      if (targetHitInfos[index].collider == null || targetHitInfos[index].collider.gameObject == null) {
        index++;
        continue;
      }

      GameObject target = targetHitInfos[index].collider.gameObject;
      if (target != null) {
        Vector3 targetDir = target.transform.position - sender.transform.position;
        targetDir.y = 0;
        Vector3 senderDir = sender.transform.rotation * Vector3.forward;
        if (Mathf.Abs(Vector3.Angle(targetDir, senderDir)) > SectorDegree) {
          index++;
          continue;
        }

        ImpactInfo curImpact = ExtractBestImpactInfo(target, impacts);
        if (curImpact != null) {
          Vector3 faceDir = target.transform.position - sender.transform.position;
          faceDir.y = 0;
          ImpactInfo m_ImpactInfo = curImpact.Clone() as ImpactInfo;
          m_ImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir)
            * m_ImpactInfo.m_Velocity;
          m_ImpactInfo.Attacker = sender;
          ImpactSystem.Instance.SendImpact(attacker, target, m_ImpactInfo, hitCount);
          TriggerImpl.RecordTarget(attacker, target);
          isTargeted = true;
        }
      }
      index++;
    }
    ProcessImpactLock(attacker, impacts, isTargeted);
  }
  public static void ProcessImpactByBoxCollider(GameObject attacker, GameObject sender,
   SkillImpactInfos impacts, Vector3 center, Vector3 size, int hitCount = 1) {
    bool isTargeted = false;

    // Find Targets
    Bounds bounds = new Bounds(center, size);
    Collider[] hitColliders = Physics.OverlapSphere(sender.transform.position,
      impacts.ImpactRadius, (int)SceneLayerType.Character);
    int index = 0;
    while (index < hitColliders.Length) {
      if (hitColliders[index] == null || hitColliders[index].gameObject == null) {
        index++;
        continue;
      }

      GameObject target = hitColliders[index].gameObject;
      if (target != null) {
        if (bounds != null && !IsPointInBounds(sender, target, bounds)) {
          index++;
          continue;
        }

        ImpactInfo curImpact = ExtractBestImpactInfo(target, impacts);
        if (curImpact != null) {
          Vector3 faceDir = target.transform.position - sender.transform.position;
          faceDir.y = 0;
          ImpactInfo m_ImpactInfo = curImpact.Clone() as ImpactInfo;
          m_ImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir)
            * m_ImpactInfo.m_Velocity;
          m_ImpactInfo.Attacker = sender;
          ImpactSystem.Instance.SendImpact(attacker, target, m_ImpactInfo, hitCount);
          TriggerImpl.RecordTarget(attacker, target);
          isTargeted = true;
        }
      }
      index++;
    }
    ProcessImpactLock(attacker, impacts, isTargeted);
  }
  private static void ProcessImpactLock(GameObject attacker, SkillImpactInfos impacts, bool isTargeted) {
    SkillLockFrameInfo lockInfo = TriggerImpl.GetLockFrameInfoById(attacker, impacts.LockFrameInfoId);
    if (lockInfo != null) {
      lockInfo = lockInfo.Clone();
      if (lockInfo.NeedTarget) {
        if (isTargeted) {
          TriggerImpl.StartLockFrame(attacker, lockInfo);
        }
      } else {
        TriggerImpl.StartLockFrame(attacker, lockInfo);
      }
    }

    SkillMoveScaleInfo lockMoveInfo = TriggerImpl.GetMoveScaleInfoById(attacker, impacts.MoveScaleInfoId);
    if (lockMoveInfo != null) {
      lockMoveInfo = lockMoveInfo.Clone();
      if (lockInfo.NeedTarget) {
        if (isTargeted) {
          TriggerImpl.StartMoveScale(attacker, lockMoveInfo);
        }
      } else {
        lockMoveInfo = lockMoveInfo.Clone();
        TriggerImpl.StartMoveScale(attacker, lockMoveInfo);
      }
    }
  }
  private static bool IsPointInBounds(GameObject sender, GameObject target, Bounds bounds) {
    Vector3 pointOff = target.transform.position - sender.transform.position;
    Vector3 localPointOff = sender.transform.InverseTransformDirection(pointOff);
    return bounds.Contains(localPointOff);
  }
  public static ImpactInfo ExtractBestImpactInfo(GameObject target, SkillImpactInfos impacts) {
    TargetStateType stateType = GetTargetStateType(target);
    if (impacts == null || CheckTargetInImpactExcept(target, impacts)) {
      return null;
    }
    if (impacts.ImpactList != null && impacts.ImpactList.Count > 0) {
      foreach (SkillImpactInfoData data in impacts.ImpactList) {
        if (data != null && data.TargetStateType == stateType) {
          return data.ImpactInfo;
        }
      }
    }
    return impacts.ImpactDefault;
  }
  public static bool CheckTargetInImpactExcept(GameObject target, SkillImpactInfos impacts) {
    TargetStateType stateType = GetTargetStateType(target);
    if (impacts != null && impacts.ExceptTargetState != null && impacts.ExceptTargetState.Count > 0) {
      foreach (TargetStateType tState in impacts.ExceptTargetState) {
        if (stateType == tState) {
          return true;
        }
      }
    }
    return false;
  }
  public static TargetStateType GetTargetStateType(GameObject obj) {
    BaseImpact[] impacts = obj.GetComponentsInChildren<BaseImpact>();
    foreach (BaseImpact imp in impacts) {
      if (imp.IsAcitve) {
        switch (imp.GetImpactType()) {
          case ImpactType.Stiffness:
            return TargetStateType.STIFFNESS;
          case ImpactType.HitFly:
            return TargetStateType.HITFLY;
          case ImpactType.KnockDown:
            return TargetStateType.KNOKDOWN;
        }
      }
    }
    return TargetStateType.STAND;
  }
  //////////////////////////////////////////////////////////////////////////
  // Character
  public static void SetFacePos(GameObject target, Vector3 targetPos) {
    SetFacePos(target, targetPos, new MathRange());
  }
  public static void SetFacePos(GameObject target, Vector3 targetPos, MathRange YRotateRange) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("SetFacePos skill manager miss!");
      return;
    }
    mgr.SetFacePos(targetPos, YRotateRange);
  }
  public static void StartMoveById(GameObject target, int id) {
    SkillSceneObjManager m_SceneObjMgr = target.GetComponent<SkillSceneObjManager>();
    if (m_SceneObjMgr == null) {
      LogicSystem.GfxLog("StartMoveById miss!");
      return;
    }
    SkillMovementInfo moveInfo = m_SceneObjMgr.TryGetSkillMovementInfo(id);
    if (moveInfo != null) {
      StartMove(target, moveInfo);
    }
  }
  public static void StartMove(GameObject target, SkillMovementInfo info) {
    MovementControl mgr = target.GetComponent<MovementControl>();
    if (mgr == null) {
      LogicSystem.GfxLog("StartMove movement control miss!");
      return;
    }
    mgr.StartMove(info);
  }
  public static void StopMove(GameObject target) {
    MovementControl mgr = target.GetComponent<MovementControl>();
    if (mgr == null) {
      LogicSystem.GfxLog("StartMove movement control miss!");
      return;
    }
    mgr.StopMove();
  }
  public static void NotifyIngoreGravity(GameObject target, bool isIngore) {
    MovementControl mgr = target.GetComponent<MovementControl>();
    if (mgr == null) {
      LogicSystem.GfxLog("NotifyIngoreGravity control miss!");
      return;
    }
    mgr.NotifyIngoreGravity(isIngore);
  }
  public static SkillAnimInfo GetDefaultAnimInfo(GameObject target) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("GetDefaultAnimInfo control miss!");
      return null;
    }
    return mgr.DefaultAnimInfo;
  }
  public static void ShowGameObject(GameObject target, bool isShow) {
    if (target == null) {
      Debug.LogWarning("ShowGameObject Can not find.");
      return;
    }

    Renderer[] marr = target.GetComponents<Renderer>();
    foreach (Renderer m in marr) {
      if (isShow == false) {
        if (m_BackupMaterialColor.ContainsKey(m.GetInstanceID())) {
          m_BackupMaterialColor.Remove(m.GetInstanceID());
        }
        m_BackupMaterialColor.Add(m.GetInstanceID(), m.material.color);
        m.material.color = m_HideMaterialColor;
      } else {
        Vector4 m_MaterialColor = new Vector4(0, 0, 0, 0);
        if (m_BackupMaterialColor.ContainsKey(m.GetInstanceID())) {
          m_MaterialColor = m_BackupMaterialColor[m.GetInstanceID()];
        }
        m.material.color = m_MaterialColor;
      }
    }
    ParticleSystem[] particles = target.GetComponentsInChildren<ParticleSystem>(true);
    foreach (ParticleSystem m in particles) {
      m.enableEmission = isShow;
    }

    int childCount = target.transform.childCount;
    for (int index = 0; index < childCount; index++) {
      Transform childTrans = target.transform.GetChild(index);
      if (childTrans != null && childTrans.gameObject != null) {
        ShowGameObject(childTrans.gameObject, isShow);
      }
    }
  }
  //public static void ShowChangWeaponButton(GameObject target) {
  //  ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
  //  if (mgr == null) {
  //    LogicSystem.GfxLog("ShowChangWeaponButton control miss!");
  //    return;
  //  }
  //  mgr.ShowChangWeaponButton();
  //}
  //public static void HideChangWeaponButton(GameObject target) {
  //  ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
  //  if (mgr == null) {
  //    LogicSystem.GfxLog("HideChangWeaponButton control miss!");
  //    return;
  //  }
  //  mgr.HideChangWeaponButton();
  //}
  public static void ShowJoyStick(GameObject target) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("ShowJoyStick control miss!");
      return;
    }
    mgr.ShowJoyStick();
  }
  public static void HideJoyStick(GameObject target) {
    ShooterSkillManager mgr = target.GetComponent<ShooterSkillManager>();
    if (mgr == null) {
      LogicSystem.GfxLog("HideJoyStick control miss!");
      return;
    }
    mgr.HideJoyStick();
  }

  private static Dictionary<int, Vector4> m_BackupMaterialColor = new Dictionary<int, Vector4>();
  private static Vector4 m_HideMaterialColor = new Vector4(1, 1, 1, 0);

  //////////////////////////////////////////////////////////////////////////
  // Target
  public static void StartRecordTarget(GameObject target) {
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("StartRecordTarget control miss!");
      return;
    }
    control.StartRecordTarget();
  }
  public static void FinishRecordTarget(GameObject target) {
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("FinishRecordTarget control miss!");
      return;
    }
    control.FinishRecordTarget();
  }
  public static void RecordTarget(GameObject target, GameObject target2) {
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("RecordTarget control miss!");
      return;
    }

    if (TriggerImpl.GetCharacterCamp(target2) != 0) {
      control.RecordTarget(target2);
    }
  }
  public static GameObject GetCurTarget(GameObject target) {
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("GetCurTarget control miss!");
      return null;
    }
    return control.GetCurTarget();
  }
  public static bool GetCurTargetPos(GameObject target, out Vector3 targetPos) {
    targetPos = Vector3.zero;
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("GetCurTargetPos control miss!");
      return false;
    }
    return control.GetCurTargetPos(out targetPos);
  }
  public static void ResetTarget(GameObject target) {
    SkillTargetControl control = target.GetComponent<SkillTargetControl>();
    if (control == null) {
      LogicSystem.GfxLog("ResetTarget control miss!");
      return;
    }
    control.ResetTarget();
  }
  public static SkillLockFrameInfo GetLockFrameInfoById(GameObject target, int id) {
    SkillSceneObjManager tSceneObjMgr = target.GetComponent<SkillSceneObjManager>();
    if (tSceneObjMgr == null) {
      LogicSystem.GfxLog("SkillSceneObjManager miss!");
      return null;
    }
    return tSceneObjMgr.TryGetSkillLockFrameInfo(id);
  }
  public static SkillMoveScaleInfo GetMoveScaleInfoById(GameObject target, int id) {
    SkillSceneObjManager tSceneObjMgr = target.GetComponent<SkillSceneObjManager>();
    if (tSceneObjMgr == null) {
      LogicSystem.GfxLog("SkillSceneObjManager miss!");
      return null;
    }
    return tSceneObjMgr.TryGetSkillMoveScaleInfo(id);
  }
  public static void StartLockFrame(GameObject target, SkillLockFrameInfo info) {
    SkillLockFrame tLockFrame = target.gameObject.GetComponent<SkillLockFrame>();
    if (tLockFrame == null) {
      LogicSystem.GfxLog("Trigger_LockFrame component miss!");
      return;
    }
    tLockFrame.StartLockFrame(info);
  }
  public static void StartMoveScale(GameObject target, SkillMoveScaleInfo info) {
    MovementControl tLockFrame = target.gameObject.GetComponent<MovementControl>();
    if (tLockFrame == null) {
      LogicSystem.GfxLog("StartMoveScale component miss!");
      return;
    }
    tLockFrame.StartMoveScale(info);
  }
}
