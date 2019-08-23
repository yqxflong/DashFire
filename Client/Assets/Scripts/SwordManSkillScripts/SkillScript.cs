using UnityEngine;
using System.Collections.Generic;
using DashFire;

public class SkillScript : MonoBehaviour {
  public int SkillId;
  public bool IsNeedOnGround = true;
  public float MaxHeightWithGround = 1;
  public float MinHeightWithGround = 3;
  public float LockInputTime;
  public float NextInputTime;
  public float ChargeTime;
  public float m_CastPoint = 0;
  public float m_ChannelingTime = 0;
  public float m_BackSwing = -1;
  public float m_CD = 0;
  public SkillCategory Category;
  public int NextSkillId = -1;
  public int QSkillId = -1;
  public int ESkillId = -1;
  public ImpactGroup[] m_ImpactGroups;
  public float m_CrossFadeTime = 0.05f;
  public float m_StartWeight = 0;
  public float m_ClampTime = 0;
  public bool m_IsControlMove = true;
  public bool m_IsAttackAfterSkill = false;
  public float m_SkillCastRange = 5;
  public float m_TargetChooseRange = 0.5f;
  public bool m_IsCanBreakByImpact = false;
  public AudioClip[] m_CollideSounds;
  public float m_MinCollideSoundInterval = 0.2f;
  public int m_CostImpact = 0;
  public int m_RageCast = 0;

  protected float m_ClampedTime = 0;
  protected bool m_IsSkillActive = false;
  protected string m_SkillName;
  protected Animation m_ObjAnimation;
  protected SkillManager m_SkillManager;
  protected SkillColliderManager m_ColliderManager;
  protected SkillMovement m_SkillMovement;
  protected float m_StartTime;
  protected float m_CDBeginTime = -1000;
  protected MyCharacterStatus m_Status;

  protected bool m_IsHaveCollideMoveCurve = false;
  protected string m_CollideMoveCurve = "";
  protected List<GameObject> m_SkillPlayingEffect = new List<GameObject>();
  protected GameObject m_MoveTarget = null;
  protected float m_ToTargetDistanceRatio = 0;
  protected float m_ToTargetConstDistance = 0;

  protected float m_LastCollideSoundTime = -1000;

  public GameObject MoveTarget {
    get { return m_MoveTarget; }
    set { m_MoveTarget = value; }
  }

  public float ToTargetDistanceRatio {
    get { return m_ToTargetDistanceRatio; }
    set { m_ToTargetDistanceRatio = value; }
  }

  public float ToTargetConstDistance {
    get { return m_ToTargetConstDistance; }
    set { m_ToTargetConstDistance = value; }
  }

  public bool IsActive()
  {
    return m_IsSkillActive;
  }

  public virtual void BeginCD() {
    m_CDBeginTime = Time.time;
  }

  public virtual bool IsInCD() {
    if (Time.time <= m_CDBeginTime + m_CD) {
      return true;
    }
    DashFire.SharedGameObjectInfo selfObjInfo = DashFire.LogicSystem.GetSharedGameObjectInfo(gameObject);
    if (selfObjInfo == null || selfObjInfo.Blood <= 0 || selfObjInfo.Rage < m_RageCast) {
      return true;
    }
    return false;
  }

  public virtual bool IsCanBreakByImpact() {
    if (!m_IsSkillActive) {
      return true;
    }
    return m_IsCanBreakByImpact;
  }

  public virtual bool CanStart() {
    if (IsInCD()) {
      return false;
    }
    bool ret = false;
    if (IsNeedOnGround) {
      if (m_SkillMovement.IsGrounded()) {
        ret = true;
      } else if (m_SkillMovement.GetHeightWithGround() <= MaxHeightWithGround){
        ret = true;
      } else {
        ret = false;
      }
    } else {
      ret = (m_SkillMovement.GetHeightWithGround() >= MinHeightWithGround);
    }
    if (ret) {
      DashFire.SharedGameObjectInfo selfObjInfo = DashFire.LogicSystem.GetSharedGameObjectInfo(gameObject);
      if (selfObjInfo != null && selfObjInfo.Blood > 0 && selfObjInfo.Rage >= m_RageCast) {
        return true;
      }
    }
    return false;
  }

  public void PlayCollideSound() {
    int sound_count = m_CollideSounds.Length;
    if (sound_count <= 0) {
      return;
    }
    if (m_LastCollideSoundTime + m_MinCollideSoundInterval <= Time.time) {
      m_LastCollideSoundTime = Time.time;
      int random_index = new System.Random().Next(sound_count);
      AudioClip clip = m_CollideSounds[random_index];
      m_SkillManager.PlaySound(clip);
    }
  }

  public virtual bool StartSkill() {
    if (!CanStart()) {
      return false;
    }
    ImpactSystem.Instance.StopAllImpacts(gameObject);
    LogicSystem.NotifyGfxAnimationStart(gameObject);
    if (m_IsControlMove) {
      LogicSystem.NotifyGfxMoveControlStart(gameObject);
    }
    m_Status = MyCharacterStatus.kSkilling;
    m_StartTime = Time.time;
    m_IsSkillActive = true;
    m_IsHaveCollideMoveCurve = false;
    m_ClampedTime = 0;
    if (m_ColliderManager != null) {
      m_ColliderManager.ClearColliders();
    }
    if (m_RageCast > 0) {
      DashFire.SharedGameObjectInfo selfObjInfo = DashFire.LogicSystem.GetSharedGameObjectInfo(gameObject);
      DashFire.LogicSystem.NotifyGfxHitTarget(gameObject, m_CostImpact, gameObject, 0);
    }
    return true;
  }

  public virtual bool CanStop()
  {
    if (!m_IsSkillActive) {
      return true;
    }
    if (CanDisrupt()) {
      return true;
    }
    if (CanStopBackSwing()) {
      return true;
    }
    return false;
  }

  public virtual bool CanDisrupt()
  {
    if (!m_IsSkillActive) {
      return false;
    }
    if (0 <= m_CastPoint && Time.time <= (m_StartTime + m_CastPoint)) {
      return true;
    } else if (m_ChannelingTime > 0 && Time.time <= (m_StartTime + m_ChannelingTime)) {
      return true;
    }
    return false;
  }

  public virtual bool CanStopBackSwing() {
    if (m_BackSwing >= 0 && Time.time >= (m_StartTime + m_BackSwing)) {
      return true;
    }
    return false;
  }

  public virtual void ForceStopSkill()
  {
    MoveTarget = null;
    m_SkillManager.ResetWeapons();
    m_Status = MyCharacterStatus.kIdle;
    AnimationClip anim = GetCurAnimationClip();
    if (anim != null) {
      m_ObjAnimation[anim.name].speed = 0;
    }
    if (m_SkillMovement != null) {
      m_SkillMovement.StopMove();
    }
    if (m_IsSkillActive && !CanStopBackSwing()) {
      StopSkillEffects();
      if (m_ColliderManager != null) {
        m_ColliderManager.StopSkillColliders(SkillId);
      }
    }
    m_IsSkillActive = false;
    LogicSystem.NotifyGfxAnimationFinish(gameObject);
    if (m_IsControlMove) {
      LogicSystem.NotifyGfxMoveControlFinish(gameObject);
    }
    m_SkillPlayingEffect.Clear();
  }

  public virtual void StopSkill()
  {
    if (CanStop()) {
      ForceStopSkill();
    }
  }

  public void AddSkillEffect(GameObject effect) {
    m_SkillPlayingEffect.Add(effect);
  }

  private void StopSkillEffects() {
    foreach (GameObject effect in m_SkillPlayingEffect) {
      if (effect != null) {
        effect.SetActive(false);
      }      
    }
    m_SkillPlayingEffect.Clear();
  }

  public virtual AnimationClip GetCurAnimationClip() {
    return null;
  }

  public virtual void Init() {
    m_SkillManager = gameObject.GetComponent<SkillManager>();
    m_ObjAnimation = gameObject.GetComponent<Animation>();
    m_SkillMovement = gameObject.GetComponent<SkillMovement>();
    m_ColliderManager = gameObject.GetComponent<SkillColliderManager>();
  }

  public virtual void UpdateSkill() {
    if (m_ObjAnimation == null) {
      return;
    }
    if (m_IsSkillActive) {
      Vector3 pos = gameObject.transform.position;
      LogicSystem.NotifyGfxUpdatePosition(gameObject, pos.x, pos.y, pos.z);
      if (CheckSkillOver()) {
        m_IsSkillActive = false;
        StopSkill();
      }
    }
    if (m_IsHaveCollideMoveCurve) {
      if (!string.IsNullOrEmpty(m_CollideMoveCurve) && m_ColliderManager.IsCollided()) {
        m_SkillMovement.StartCurveMove(m_CollideMoveCurve);
        m_IsHaveCollideMoveCurve = false;
      }
    }
  }

  public virtual void RegisterCollideCurveMove(string str_param) {
    m_IsHaveCollideMoveCurve = true;
    m_CollideMoveCurve = str_param;
  }

  protected virtual bool CheckSkillOver() {
    return true;
  }

  public ImpactGroup GetImpactGroupById(int id) {
    foreach (ImpactGroup ig in m_ImpactGroups) {
      if (ig.id == id) {
        return ig;
      }
    }
    return null;
  }
}

public enum BeHitState {
  kDefault,
  kStand,
  kStiffness,
  kLauncher,
  kKnockDown,
}

[System.Serializable]
public class ImpactGroup
{
  public int id;
  public StateImpact[] m_StateImpact;
}

[System.Serializable]
public class StateImpact
{
  public BeHitState m_State;
  public ImpactInfo[] m_ImpactInfos;
}