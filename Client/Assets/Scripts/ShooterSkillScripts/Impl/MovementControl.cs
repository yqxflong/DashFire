using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public delegate void CurveKeyEvent(Vector3 curVelocity, float elapsed);
public class CurveKeyInfo {
  public float m_TimePointPercent = 0.0f;
  public CurveKeyEvent m_KeyEvent = null;
  public bool m_IsExecuted = false;
}

public delegate void EventMovePos(Vector3 posTarget, Vector3 posOff);
public delegate void EventHitGround(Vector3 groundPos, float disWithGround);

[System.Serializable]
public class NumericMovementInfo {
  public float Speed;
  public float Accelerate;
  public NumericMovementInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 2);
    if (result != null) {
      Speed = Convert.ToSingle(result[0]);
      Accelerate = Convert.ToSingle(result[1]);
    }
  }
  public NumericMovementInfo() { }
  public NumericMovementInfo Clone() {
    NumericMovementInfo newData = new NumericMovementInfo();
    newData.Speed = Speed;
    newData.Accelerate = Accelerate;
    return newData;
  }
}

[System.Serializable]
public class SkillMovementInfo {
  public bool IsValid = true;
  public int Id = 0;
  public float MoveTimeMax = float.MaxValue;
  public NumericMovementInfo ForwardInfo = null;
  public NumericMovementInfo UpwardInfo = null;
  public NumericMovementInfo SidewardInfo = null;
  public Vector3 Gravity = new Vector3(0, -9.8f, 0);
  public bool IsAppendVelocity = false;
  public SkillMovementInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 1);
    if (result != null) {
      Id = Convert.ToInt32(result[0]);
      MoveTimeMax = Convert.ToSingle(result[1]);
      ForwardInfo = new NumericMovementInfo(result[2]);
      UpwardInfo = new NumericMovementInfo(result[3]);
      SidewardInfo = new NumericMovementInfo(result[4]);
      Gravity = Script_Util.ToVector3(result[5]);
      IsAppendVelocity = Convert.ToBoolean(result[6]);
    } else {
      IsValid = false;
    }
  }
  public SkillMovementInfo() { }
  public SkillMovementInfo Clone() {
    SkillMovementInfo newData = new SkillMovementInfo();
    newData.IsValid = IsValid;
    newData.Id = Id;
    newData.MoveTimeMax = MoveTimeMax;
    newData.ForwardInfo = ForwardInfo.Clone();
    newData.UpwardInfo = UpwardInfo.Clone();
    newData.SidewardInfo = SidewardInfo.Clone();
    newData.Gravity = Gravity;
    newData.IsAppendVelocity = IsAppendVelocity;
    return newData;
  }
}

public class SkillMovementCom {
  private List<CurveKeyInfo> m_CurveKeyList = new List<CurveKeyInfo>();
  private float m_MoveElapsed = 0;
  private float m_Percent = 0;
  private Vector3 m_Velocity = Vector3.zero;
  private bool m_IsMoving = false;
  private GameObject m_Target = null;

  // Config
  public SkillMovementInfo m_MovementInfo = null;
  public EventMovePos m_OnMovePos = null;
  public EventHitGround m_OnHitGround = null;

  public SkillMovementCom() {
  }
  public float MoveElapsed { get { return m_MoveElapsed; } }
  public float Percent { get { return m_Percent; } }
  public Vector3 Velocity { get { return m_Velocity; } }
  public GameObject Target { get { return m_Target; } }

  public void StartMove(GameObject target, SkillMovementInfo info) {
    Reset();
    m_Target = target;
    m_MovementInfo = info;
    m_IsMoving = true;
    if (!info.IsAppendVelocity) {
      float forwardV = CaculateNumericVelocity(m_MovementInfo.ForwardInfo, m_MovementInfo.ForwardInfo.Speed, 0);
      float upwardV = CaculateNumericVelocity(m_MovementInfo.UpwardInfo, m_MovementInfo.UpwardInfo.Speed, 0);
      float sidewardV = CaculateNumericVelocity(m_MovementInfo.SidewardInfo, m_MovementInfo.SidewardInfo.Speed, 0);
      m_Velocity = new Vector3(sidewardV, upwardV, forwardV);
    }
  }
  public void StopMove() {
    Reset();
    m_Velocity = Vector3.zero;
  }
  public void Reset() {
    m_Target = null;
    m_IsMoving = false;
    m_MoveElapsed = 0;
    m_Percent = 0;
    for (int index = 0; index < m_CurveKeyList.Count; index++) {
      CurveKeyInfo info = m_CurveKeyList[index];
      if (info != null) {
        info.m_IsExecuted = false;
      }
    }
  }
  public void RegisterKeyEvent(float timePercent, CurveKeyEvent kEvent) {
    CurveKeyInfo info = new CurveKeyInfo();
    info.m_TimePointPercent = timePercent;
    info.m_KeyEvent = kEvent;
    info.m_IsExecuted = false;
    m_CurveKeyList.Add(info);
  }
  public void ExecuteMove(float delta) {
    if (!m_IsMoving || m_Target == null || m_MovementInfo.MoveTimeMax <= 0) { return; }
    m_MoveElapsed += delta;
    m_Percent = m_MoveElapsed / m_MovementInfo.MoveTimeMax;

    float forwardV = CaculateNumericVelocity(m_MovementInfo.ForwardInfo, m_Velocity.z, delta);
    float upwardV = CaculateNumericVelocity(m_MovementInfo.UpwardInfo, m_Velocity.y, delta);
    float sidewardV = CaculateNumericVelocity(m_MovementInfo.SidewardInfo, m_Velocity.x, delta);
    m_Velocity = new Vector3(sidewardV, upwardV, forwardV);

    ExecuteMoveInternal(delta, m_MovementInfo.Gravity);
    TriggerEvent(m_Percent);
  }
  private void ExecuteMoveInternal(float delta, Vector3 gravity) {
    m_Velocity += delta * gravity;
    Vector3 tVelocity = m_Target.transform.rotation * m_Velocity;
    Vector3 posOff = tVelocity * delta;
    Vector3 pos = m_Target.transform.position + posOff;
    Component controller = m_Target.GetComponent<CharacterController>();
    if (controller != null) {
      ((CharacterController)controller).Move(posOff);
    } else {
      m_Target.transform.position += posOff;
    }
    if (m_OnMovePos != null) {
      m_OnMovePos(pos, posOff);
    }
    float disWithGround = Script_Util.GetHeightWithGround(m_Target.transform);
    if (disWithGround <= 0 && m_OnHitGround != null) {
      m_OnHitGround(m_Target.transform.position, disWithGround);
    }
  }
  private void TriggerEvent(float percent) {
    for (int index = 0; index < m_CurveKeyList.Count; index++) {
      CurveKeyInfo info = m_CurveKeyList[index];
      if (info != null && !info.m_IsExecuted && info.m_KeyEvent != null
        && percent >= info.m_TimePointPercent) {
        info.m_IsExecuted = true;
        info.m_KeyEvent(m_Velocity, m_MoveElapsed);
      }
    }
  }
  private float CaculateNumericVelocity(NumericMovementInfo info, float curSpeed, float delta) {
    return curSpeed + info.Accelerate * delta;
  }
}

public class MovementControl : MonoBehaviour {
  private SkillMovementCom m_MovementCom = null;
  private bool m_IsGravityControl = false;
  private bool m_IsGravityOn = false;
  private bool m_IsIngoreGravity = false;
  private Vector3 m_Vertical = Vector3.zero;
  private SkillMoveScaleInfo m_MoveScaleInfo = null;
  private float m_MoveScaleElapsed = 0;

  public SkillAnimInfo GravityDropDownAnimInfo = null;
  public Vector3 Gravity = new Vector3(0, -10.0f, 0);
  public float GroundTweak = 1.0f;
  public bool m_IsAutoGravityOn = true;

  internal void Start() {
    m_MovementCom = new SkillMovementCom();
    m_MovementCom.m_OnMovePos = OnMovePos;
  }
  internal void Update() {
    float delta = Time.deltaTime;
    if (m_MoveScaleInfo != null && m_MoveScaleInfo.Duration >= 0) {
      m_MoveScaleElapsed += delta;
      if (m_MoveScaleElapsed <= m_MoveScaleInfo.Duration) {
        delta *= m_MoveScaleInfo.MoveScale;
      } else {
        m_MoveScaleElapsed = 0;
        m_MoveScaleInfo = null;
      }
    }
    m_MovementCom.ExecuteMove(delta);

    if (!m_IsIngoreGravity) {
      if (IsAutoGravity()) {
        SetGravityOn(true);
      } else {
        SetGravityOn(false);
      }
    }

    if (m_IsGravityOn) {
      ApplyGravity(this.gameObject, Time.deltaTime, GravityDropDownAnimInfo);
    }
  }
  public void StartMove(SkillMovementInfo info) {
    LogicSystem.NotifyGfxMoveControlStart(gameObject);
    m_MovementCom.Reset();
    m_MovementCom.StartMove(this.gameObject, info);
  }
  public void StopMove() {
    m_MovementCom.StopMove();
    LogicSystem.NotifyGfxMoveControlFinish(gameObject);
  }
  public void StartMoveScale(SkillMoveScaleInfo info) {
    m_MoveScaleElapsed = 0;
    m_MoveScaleInfo = info;
  }
  private bool IsAutoGravity() {
    return !m_IsIngoreGravity && m_IsAutoGravityOn;
  }
  public void NotifyIngoreGravity(bool isIngore) {
    m_IsIngoreGravity = isIngore;
    if (isIngore) {
      SetGravityOn(false);
    }
  }
  private void SetGravityOn(bool isOn) {
    //确保重力控制正常终结
    if (m_IsGravityOn != isOn) {
      if (m_IsGravityOn == true) {
        EndControlGravity();
      }
    }
    m_IsGravityOn = isOn;
  }
  private void StartControlGravity() {
    if (!m_IsGravityControl) {
      LogicSystem.NotifyGfxAnimationStart(this.gameObject);
      LogicSystem.NotifyGfxMoveControlStart(this.gameObject);
      if (GravityDropDownAnimInfo != null) {
        TriggerImpl.PlayAnim(this.gameObject, GravityDropDownAnimInfo);
      }
      m_Vertical = Vector3.zero;
      m_IsGravityControl = true;
    }
  }
  private void EndControlGravity() {
    m_Vertical = Vector3.zero;
    if (m_IsGravityControl) {
      if (GravityDropDownAnimInfo != null) {
        TriggerImpl.StopAnim(this.gameObject, GravityDropDownAnimInfo);
      }
      LogicSystem.NotifyGfxAnimationFinish(this.gameObject);
      LogicSystem.NotifyGfxMoveControlFinish(this.gameObject);
      m_IsGravityControl = false;
    }
  }
  private void OnMovePos(Vector3 posTarget, Vector3 posOff) {
    LogicSystem.NotifyGfxUpdatePosition(this.gameObject, posTarget.x, posTarget.y, posTarget.z);
  }
  private void ApplyGravity(GameObject target, float delta, SkillAnimInfo animInfo) {
    if (target == null) { return; }
    if (!Script_Util.IsOnGround(target, GroundTweak)) {
      StartControlGravity();
      m_Vertical += Gravity * delta;
      Vector3 posOff = m_Vertical * delta;
      Vector3 pos = target.transform.position + posOff;
      Component controller = target.GetComponent<CharacterController>();
      if (controller != null) {
        ((CharacterController)controller).Move(posOff);
      } else {
        target.transform.position += posOff;
      }
      LogicSystem.NotifyGfxUpdatePosition(target, pos.x, pos.y, pos.z);
    } else {
      EndControlGravity();
    }
  }
}
