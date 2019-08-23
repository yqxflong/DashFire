using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class ImpactThunderStroke : BaseImpact {
  // 雷击效果
  public int m_StrokeCount = 1;
  public float m_StrokeInterval = 0.5f;

  public AnimationClip m_StrokeAnim;

  private float m_LastStrokeTime = 0;
  public override ImpactType GetImpactType() {
    return base.GetImpactType();
  }

  void Update() {
    if (m_IsActive) {
      GeneralTickImpact(Time.deltaTime);
      if (Time.time > m_LastStrokeTime + m_ImpactInfo.m_ThunderStrokeConfig.m_StrokeInterval) {
        m_AnimationPlayer.Play(m_StrokeAnim.name);
        m_ImpactInfo.m_ThunderStrokeConfig.m_StrokeCount--;
        m_LastStrokeTime = Time.time;
      }
      if (m_ImpactInfo.m_ThunderStrokeConfig.m_StrokeCount <= 0) {
        StopImpact();
      }
    }
  }

  public void StartImpact(ImpactInfo impactInfo) {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactThunderStroke::StartImpact -- impactInfo is null");
    }
    GeneralStartImpact(impactInfo);
    m_AnimationPlayer.SetAnimationState(m_StrokeAnim.name, 10, 1.0f, 1.0f, AnimationBlendMode.Additive);
    m_AnimationPlayer.Play(m_StrokeAnim.name);
    m_LastStrokeTime = Time.time;
    m_ImpactInfo.m_ThunderStrokeConfig.m_StrokeCount--;
  }

  public void StopImpact() {
    GeneralStopImpact();
  }
}
