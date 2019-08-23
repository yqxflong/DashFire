using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class SkillSceneObjManager : MonoBehaviour {
  public List<SkillEffectInfo> EffectInfos = new List<SkillEffectInfo>();
  public List<SkillSoundInfo> SoundInfos = new List<SkillSoundInfo>();
  public List<SkillImpactInfos> ImpactInfos = new List<SkillImpactInfos>();
  public List<SkillMovementInfo> MovementInfos = new List<SkillMovementInfo>();
  public List<SkillWeaponInfo> WeaponInfos = new List<SkillWeaponInfo>();
  public List<SkillLockFrameInfo> LockFrameInfos = new List<SkillLockFrameInfo>();
  public List<SkillMoveScaleInfo> MoveScaleInfos = new List<SkillMoveScaleInfo>();

  public List<SceneObject_LinearBulletInfo> LinearBulletInfos = new List<SceneObject_LinearBulletInfo>();
  public List<SceneObject_TerminalBulletInfo> TerminalBulletInfos = new List<SceneObject_TerminalBulletInfo>();
  public List<SceneObject_MissleInfo> MissleInfos = new List<SceneObject_MissleInfo>();
  public List<SceneObject_AttractBulletInfo> AttractBulletInfos = new List<SceneObject_AttractBulletInfo>();

  internal void Start() {
  }
  public SkillEffectInfo TryGetSkillEffectInfo(int id) {
    return EffectInfos.Find(delegate(SkillEffectInfo info) { return info.Id == id; });
  }
  public SkillSoundInfo TryGetSkillSoundInfo(int id) {
    return SoundInfos.Find(delegate(SkillSoundInfo info) { return info.Id == id; });
  }
  public SkillImpactInfos TryGetSkillImpactInfo(int id) {
    return ImpactInfos.Find(delegate(SkillImpactInfos info) { return info.Id == id; });
  }
  public SkillMovementInfo TryGetSkillMovementInfo(int id) {
    return MovementInfos.Find(delegate(SkillMovementInfo info) { return info.Id == id; });
  }
  public SkillWeaponInfo TryGetSkillWeaponInfo(int id) {
    return WeaponInfos.Find(delegate(SkillWeaponInfo info) { return info.Id == id; });
  }
  public SkillLockFrameInfo TryGetSkillLockFrameInfo(int id) {
    return LockFrameInfos.Find(delegate(SkillLockFrameInfo info) { return info.Id == id; });
  }
  public SkillMoveScaleInfo TryGetSkillMoveScaleInfo(int id) {
    return MoveScaleInfos.Find(delegate(SkillMoveScaleInfo info) { return info.Id == id; });
  }
  public SceneObject_LinearBulletInfo TryGetLinearBulletInfo(int id) {
    return LinearBulletInfos.Find(delegate(SceneObject_LinearBulletInfo info) { return info.Id == id; });
  }
  public SceneObject_TerminalBulletInfo TryGetTerminalBulletInfo(int id) {
    return TerminalBulletInfos.Find(delegate(SceneObject_TerminalBulletInfo info) { return info.Id == id; });
  }
  public SceneObject_MissleInfo TryGetMissleInfo(int id) {
    return MissleInfos.Find(delegate(SceneObject_MissleInfo info) { return info.Id == id; });
  }
  public SceneObject_AttractBulletInfo TryGetAttractBulletInfo(int id) {
    return AttractBulletInfos.Find(delegate(SceneObject_AttractBulletInfo info) { return info.Id == id; });
  }
}
