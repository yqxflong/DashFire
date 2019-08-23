using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum FireDragState {
  kDragTarget,
  kDragTargetEnd,
  kDragRunning,
  kJumpBreaking,
}

public class FireDrag : SkillScript{
  public AnimationClip m_DragTargetAnim;
  public float m_LockTargetTime;
  public TargetChoosePriority m_TargetChoosePriority = TargetChoosePriority.kChooseMostForward;
  public float m_SectorRadius = 3;
  public float m_SectorDegree = 60;
  public string m_ToggleNodeName;
  public string m_TargetNodeName;
  public ImpactInfo m_DragImpactInfo;

  public AnimationClip m_DragRunAnim;
  public float m_DragRunTime;
  public bool m_IsDragRunCanControl;

  public AnimationClip m_JumpBreakAnim;
  public float m_AnimSpeed = 1.0f;

  private GameObject m_Target = null;
  private FireDragState m_DragState;
  private float m_BeginDragRunTime;
  private bool m_IsTryFindTarget = false;

  // Use this for initialization
  void Start () {
    base.Init();
    m_Status = MyCharacterStatus.kIdle;
    ReceiveInput.OnJoystickMove += OnDirectionChange;
  }

  void OnDestroy() {
    ReceiveInput.OnJoystickMove -= OnDirectionChange;
  }

  void Update()
  {
    if (!m_IsSkillActive) {
      return;
    }
    base.UpdateSkill();
    if (m_DragState == FireDragState.kDragTarget) {
      if (m_ObjAnimation[m_DragTargetAnim.name].normalizedTime >= 1) {
        if (m_Target == null) {
          ForceStopSkill();
          return;
        } else {
          PlayDragRunAnim();
        }
      }
    }
    if (m_DragState == FireDragState.kDragRunning) {
      if (Time.time >= (m_BeginDragRunTime + m_DragRunTime)) {
        PlayJumpBreakAnim();
      }
    }
    if (m_Target != null) {
      if (m_IsSkillActive) {
        m_Target.transform.localPosition = Vector3.zero;
        m_Target.transform.localRotation = Quaternion.identity;
      }
      Vector3 pos = m_Target.transform.position;
      DashFire.LogicSystem.NotifyGfxUpdatePosition(m_Target, pos.x, pos.y, pos.z);
    }
    if (!m_IsTryFindTarget && Time.time >= (m_StartTime + m_LockTargetTime)) {
      FindTarget();
      m_IsTryFindTarget = true;
    }
  }

  public override bool StartSkill()
  {
    if (!base.StartSkill()) {
      return false;
    }
    m_IsTryFindTarget = false;
    PlayDragTargetAnim();
    return true;
  }

  public bool FindTarget() {
    List<GameObject> allobj = TargetChooser.FindTargetInSector(transform.position, m_SectorRadius,
                                                               transform.forward, transform.position,
                                                               m_SectorDegree);
    List<GameObject> unendure_objs = FiltUnEndureObj(allobj);
    if (m_TargetChoosePriority == TargetChoosePriority.kChooseMostForward) {
      m_Target = TargetChooser.GetMostForwardObj(transform.position, transform.forward,
                                                 m_ColliderManager.FiltEnimy(unendure_objs));
    } else {
      m_Target = TargetChooser.GetNearestObj(transform.position, m_ColliderManager.FiltEnimy(unendure_objs));
    }
    if (m_Target == null) {
      return false;
    } else {
      m_SkillMovement.SetFacePos(m_Target.transform.position);
      ControlTarget(m_Target);
      if (!SkillManager.AttatchNodeToNode(gameObject, m_ToggleNodeName, m_Target, m_TargetNodeName)) {
        m_Target = null;
        return false;
      }
      Vector3 pos = m_Target.transform.position;
      DashFire.LogicSystem.NotifyGfxUpdatePosition(m_Target, pos.x, pos.y, pos.z);
    }
    return true;
  }

  private List<GameObject> FiltUnEndureObj(List<GameObject> list)
  {
    List<GameObject> result = new List<GameObject>();
    foreach (GameObject obj in list) {
      if (obj == null) {
        continue;
      }
      CharacterCamp cc = obj.GetComponent<CharacterCamp>();
      if (cc == null || cc.m_IsEndure) {
        continue;
      }
      DashFire.SharedGameObjectInfo selfObjInfo = DashFire.LogicSystem.GetSharedGameObjectInfo(obj);
      if (selfObjInfo == null || selfObjInfo.Blood <= 0) {
        continue;
      }
      result.Add(obj);
    }
    return result;
  }

  private void ControlTarget(GameObject obj)
  {
    ImpactInfo ii = m_DragImpactInfo.Clone() as ImpactInfo;
    ii.Attacker = gameObject;
    ImpactSystem.Instance.SendImpact(obj, ii);
  }

  public override void ForceStopSkill()
  {
    base.ForceStopSkill();
    if (m_Target != null) {
      ImpactSystem.Instance.StopImpact(m_Target, ImpactType.Grab);
      m_Target.transform.parent = null;
      m_Target.transform.rotation = Quaternion.identity;
      m_Target.transform.position = SkillMovement.GetGroundPos(m_Target.transform.position);
      m_Target = null;
    }
  }

  public override AnimationClip GetCurAnimationClip() {
    switch (m_DragState) {
    case FireDragState.kDragTarget:
      return m_DragTargetAnim;
    case FireDragState.kDragRunning:
      return m_DragRunAnim;
    case FireDragState.kJumpBreaking:
      return m_JumpBreakAnim;
    }
    return null;
  }


  protected override bool CheckSkillOver()
  {
    if (m_Status != MyCharacterStatus.kSkilling) {
      return true;
    }
    if (m_DragState != FireDragState.kJumpBreaking) {
      return false;
    }
    if (m_ObjAnimation[m_JumpBreakAnim.name].normalizedTime >= 1) {
      m_ClampedTime += Time.deltaTime;
      if (m_ClampedTime > m_ClampTime) {
        return true;
      }
    }
    return false;
  }

  private void PlayDragTargetAnim() {
    m_ObjAnimation[m_DragTargetAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_DragTargetAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_DragTargetAnim.name].wrapMode = WrapMode.ClampForever;
    if (m_CrossFadeTime == 0) {
      m_ObjAnimation.Play(m_DragTargetAnim.name);
    } else {
      m_ObjAnimation[m_DragTargetAnim.name].weight = m_StartWeight;
      m_ObjAnimation.CrossFade(m_DragTargetAnim.name, m_CrossFadeTime);
    }
    m_DragState = FireDragState.kDragTarget;
  }

  private void PlayDragRunAnim() {
    m_ObjAnimation[m_DragRunAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_DragRunAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_DragRunAnim.name].wrapMode = WrapMode.Loop;
    m_ObjAnimation.Play(m_DragRunAnim.name);
    m_DragState = FireDragState.kDragRunning;
    m_BeginDragRunTime = Time.time;
  }

  private void PlayJumpBreakAnim() {
    m_ObjAnimation[m_JumpBreakAnim.name].normalizedTime = 0;
    m_ObjAnimation[m_JumpBreakAnim.name].speed = m_AnimSpeed;
    m_ObjAnimation[m_JumpBreakAnim.name].wrapMode = WrapMode.ClampForever;
    m_ObjAnimation.Play(m_JumpBreakAnim.name);
    m_DragState = FireDragState.kJumpBreaking;
  }

  private void OnDirectionChange(float direction) {
    if (!IsActive() && m_DragState != FireDragState.kDragRunning) {
      return;
    }
    Vector3 rotate = new Vector3(0, direction * 180 / Mathf.PI, 0);
    gameObject.transform.eulerAngles = rotate;
    DashFire.LogicSystem.NotifyGfxUpdatePosition(gameObject, transform.position.x, transform.position.y,
                                        transform.position.z, 0, direction, 0);
  }
}

