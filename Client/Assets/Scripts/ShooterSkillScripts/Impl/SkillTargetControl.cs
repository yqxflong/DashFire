using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class SkillTargetControl : MonoBehaviour {
  public GameObject m_CurTarget = null;
  public List<GameObject> m_CurTargetList = new List<GameObject>();
  internal void Start() {
    m_CurTarget = null;
  }
  internal void Update() {
    //m_CurTarget = null;
  }
  internal void OnDestroy() {
    m_CurTarget = null;
  }
  public void StartRecordTarget() {
    m_CurTargetList.Clear();
  }
  public void FinishRecordTarget() {
    ResetTarget();
    float bestDistance = float.MaxValue;
    GameObject bestTarget = null;
    foreach (GameObject target in m_CurTargetList) {
      float curDis = Vector3.SqrMagnitude(target.transform.position - this.gameObject.transform.position);
      if (curDis < bestDistance) {
        bestDistance = curDis;
        bestTarget = target;
      }
    }
    if (bestTarget != null) {
      m_CurTarget = bestTarget;
    }
  }
  public void RecordTarget(GameObject target) {
    //m_CurTargetList.Add(target);
    m_CurTarget = target;
  }
  public GameObject GetCurTarget() {
    return m_CurTarget;
  }
  public bool GetCurTargetPos(out Vector3 targetPos) {
    targetPos = Vector3.zero;
    if (m_CurTarget != null) {
      targetPos = m_CurTarget.transform.position;
      return true;
    }
    return false;
  }
  public void ResetTarget() {
    m_CurTarget = null;
  }
}
