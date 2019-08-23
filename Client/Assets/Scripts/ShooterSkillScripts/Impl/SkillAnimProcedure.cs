using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using DashFire;

[System.Serializable]
public class SkillAnimProcedureInfo {
  public float Duration = 0.2f;
  public string BoneName = "ef_righthand";
  public string AnimName = "";
  public Vector3 TargetPos = Vector3.zero;
  public SkillAnimProcedureInfo Clone() {
    SkillAnimProcedureInfo newData = new SkillAnimProcedureInfo();
    newData.Duration = Duration;
    newData.BoneName = BoneName;
    newData.AnimName = AnimName;
    newData.TargetPos = TargetPos;
    return newData;
  }
  public SkillAnimProcedureInfo() { }
  public SkillAnimProcedureInfo(string param) {
    string[] result = Script_Util.SplitParam(param, 3);
    if (result != null) {
      Duration = Convert.ToSingle(result[0]);
      BoneName = result[1];
      AnimName = result[2];
    }
  }
}
public class SkillAnimProcedure : MonoBehaviour {
  public float m_Elapsed = 0;
  private SkillAnimProcedureInfo m_Info = null;
  private Animation m_AnimCom = null;

  internal void LateUpdate() {
    if (CheckProcedureValid()) {
      m_Elapsed += Time.deltaTime;
      if (m_Elapsed < m_Info.Duration) {
        Transform targetTrans = LogicSystem.FindChildRecursive(this.gameObject.transform, m_Info.BoneName);
        if (null != targetTrans) {
          Vector3 targetDir = m_Info.TargetPos - targetTrans.position;
          targetTrans.forward = targetDir;
        }
      }
    } else {
      m_Elapsed = 0;
      m_Info = null;
    }
  }
  public void StartAnimProcedure(SkillAnimProcedureInfo info) {
    m_Info = info;
    m_Elapsed = 0;
  }
  private bool CheckProcedureValid() {
    if (m_Info == null || m_Info.Duration <= 0
      || string.IsNullOrEmpty(m_Info.BoneName) || string.IsNullOrEmpty(m_Info.AnimName)) {
      return false;
    }
    if (m_Elapsed >= m_Info.Duration) {
      return false;
    }
    if (m_AnimCom == null) {
      m_AnimCom = this.gameObject.GetComponent<Animation>();
    }
    if (m_AnimCom == null) {
      return false;
    }
    if (!(m_AnimCom.IsPlaying(m_Info.AnimName) && m_AnimCom[m_Info.AnimName].normalizedTime < 1.0f)) {
      return false;
    }

    return true;
  }
}
