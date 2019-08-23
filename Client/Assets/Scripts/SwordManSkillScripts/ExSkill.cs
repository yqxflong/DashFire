using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ExSkillState {
  kStart,
  kHolding,
  kEnd,
}

public class DamageObjectInfo {
  public GameObject obj;
  public float remainDamageCD;
}

public class ExSkill : SkillScript{
  public AnimationClip m_StartAnim;
  public AnimationClip m_HoldingAnim;
  public AnimationClip m_EndAnim;
  public float m_HoldingTime = 1;
  public float m_AnimSpeed = 1;
  public int m_HoldingImpactId = 1;
  public float m_MinImpactInterval = 0.3f;
  public int m_InvincibleImpact;

  private ExSkillState m_ExSkillState;
  private float m_BeginHoldTime;
  private List<DamageObjectInfo> m_DamagedObjects = new List<DamageObjectInfo>();
  private List<DamageObjectInfo> m_DelBuffer = new List<DamageObjectInfo>();

  // Use this for initialization
  void Start () {
    base.Init();
    TouchManager.OnFingerEvent += OnSkillFingerEvent;
    m_Status = MyCharacterStatus.kIdle;
  }

  void Update()
  {
    if (!m_IsSkillActive) {
      return;
    }
    base.UpdateSkill();
    if (m_ExSkillState == ExSkillState.kStart) {
      if (m_ObjAnimation[m_StartAnim.name].normalizedTime >= 1) {
        PlayHoldingAnim();
        JoyStickInputProvider.JoyStickEnable = false;
        TouchManager.GestureEnable = false;
      }
    }
    if (m_ExSkillState == ExSkillState.kHolding) {
      if (Time.time >= (m_BeginHoldTime + m_HoldingTime)) {
        PlayEndAnim();
      }
    }
    UpdateDamagedObjects();
  }

  public override bool StartSkill()
  {
    if (!base.StartSkill()) {
      return false;
    }
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_ex_skill", "ui", "SwordMan", true);
    DashFire.LogicSystem.NotifyGfxHitTarget(gameObject, m_InvincibleImpact, gameObject, 0);
    PlayStartAnim();
    return true;
  }

  public override AnimationClip GetCurAnimationClip() {
    switch (m_ExSkillState) {
    case ExSkillState.kStart:
      return m_StartAnim;
    case ExSkillState.kHolding:
      return m_HoldingAnim;
    case ExSkillState.kEnd:
      return m_EndAnim;
    }
    return null;
  }

  public override void ForceStopSkill() {
    base.ForceStopSkill();
    DashFire.LogicSystem.EventChannelForGfx.Publish("ge_ex_skill", "ui", "SwordMan", false);
    TouchManager.GestureEnable = true;
    JoyStickInputProvider.JoyStickEnable = true;
    m_DamagedObjects.Clear();
  }

  protected override bool CheckSkillOver()
  {
    if (m_Status != MyCharacterStatus.kSkilling) {
      return true;
    }
    if (m_ExSkillState != ExSkillState.kEnd) {
      return false;
    }
    if (m_ObjAnimation[m_EndAnim.name].normalizedTime >= 1) {
      m_ClampedTime += Time.deltaTime;
      if (m_ClampedTime > m_ClampTime) {
        return true;
      }
    }
    return false;
  }

  private void PlayStartAnim() {
    m_ObjAnimation[m_StartAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_StartAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_StartAnim.name].wrapMode = WrapMode.ClampForever;
    if (m_CrossFadeTime == 0) {
      m_ObjAnimation.Play(m_StartAnim.name);
    } else {
      m_ObjAnimation[m_StartAnim.name].weight = m_StartWeight;
      m_ObjAnimation.CrossFade(m_StartAnim.name, m_CrossFadeTime);
    }
    m_ExSkillState = ExSkillState.kStart;
  }

  private void PlayHoldingAnim() {
    m_ObjAnimation[m_HoldingAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_HoldingAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_HoldingAnim.name].wrapMode = WrapMode.Loop;
    m_ObjAnimation.Play(m_HoldingAnim.name);
    m_ExSkillState = ExSkillState.kHolding;
    m_BeginHoldTime = Time.time;
  }

  private void PlayEndAnim() {
    JoyStickInputProvider.JoyStickEnable = true;
    m_ObjAnimation[m_EndAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_EndAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_EndAnim.name].wrapMode = WrapMode.ClampForever;
    m_ObjAnimation.Play(m_EndAnim.name);
    m_ExSkillState = ExSkillState.kEnd;
  }

  private void OnSkillFingerEvent(FingerEvent finger) {
    if (!IsActive() || m_ExSkillState != ExSkillState.kHolding) {
      return;
    }
    if ("OnFingerDown" == finger.Name) {
      Vector3 targetPos = finger.GetTouchToWorldPoint();
      List<GameObject> objs = finger.GetRayObjectsByLayerName("Character");
      DamageObjects(objs);
    }
    if ("OnFingerMove" == finger.Name) {
      Vector3 targetPos = finger.GetTouchToWorldPoint();
      List<GameObject> objs = finger.GetRayObjectsByLayerName("Character");
      DamageObjects(objs);
    }
  }

  private void UpdateDamagedObjects() {
    foreach (DamageObjectInfo damageinfo in m_DamagedObjects) {
      damageinfo.remainDamageCD -= Time.deltaTime;
      if (damageinfo.remainDamageCD <= 0) {
        m_DelBuffer.Add(damageinfo);
      }
    }
    foreach (DamageObjectInfo damageinfo in m_DelBuffer) {
      m_DamagedObjects.Remove(damageinfo);
    }
    m_DelBuffer.Clear();
  }

  private void DamageObjects(List<GameObject> objes)
  {
    if (objes == null) {
      return;
    }
    List<GameObject> enimys = m_ColliderManager.FiltEnimy(objes);
    foreach (GameObject obj in enimys) {
      if (IsObjectCanDamage(obj)) {
        m_ColliderManager.SendSkillImpactToObj(SkillId, m_HoldingImpactId, obj);
        DamageObjectInfo info = new DamageObjectInfo();
        info.obj = obj;
        info.remainDamageCD = m_MinImpactInterval;
        m_DamagedObjects.Add(info);
      }
    }
  }

  private bool IsObjectCanDamage(GameObject obj) {
    foreach (DamageObjectInfo damageinfo in m_DamagedObjects) {
      if (damageinfo.obj == obj) {
        return false;
      }
    }
    return true;
  }
}

