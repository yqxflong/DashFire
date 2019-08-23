using UnityEngine;
using System.Collections;

public class ImpactGrab : BaseImpact {

  public AnimationClip m_GrabedAnim;
  public AnimationClip m_GrabEndAnim;
  public AnimationClip m_DeadAnim;
  public float m_GrabTime = 0.0f;

  public override ImpactType GetImpactType() {
    return ImpactType.Grab;
  }
  public void StartImpact(ImpactInfo impactInfo) {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactComa::StartImpact -- impactInfo is null");
    }
    GeneralStartImpact(impactInfo);
    if (m_ImpactInfo.m_GrabConfig.m_UseCustom) {
      //m_GrabTime = m_ImpactInfo.m_GrabConfig.m_GrabTime;
      //m_GrabEndAnim = m_ImpactInfo.m_GrabConfig.m_GrabEndAnim;
    }
    if (null != m_GrabedAnim) {
      m_AnimationPlayer.CrossFade(m_GrabedAnim.name);
    }
  }

	// Update is called once per frame
	void Update () {
    if (IsAcitve) {
      TickAnimation(Time.deltaTime);
      TickEffect(Time.deltaTime);
      TickMovement(Time.deltaTime);
      if (Time.time > m_StartTime + m_GrabTime) {
        StopImpact();
      }
    }
	}

  public override void StopImpact() {
    GeneralStopImpact();
    if (null != m_GrabedAnim) {
      m_AnimationPlayer.StopAnimation(m_GrabedAnim.name);
    }
    if (IsLogicDead()) {
      NotifyNpcDead(gameObject);
      if (null != m_DeadAnim) {
        m_AnimationPlayer.CrossFade(m_DeadAnim.name);
      }
    } else {
      if (null != m_GrabEndAnim) {
        m_AnimationPlayer.CrossFade(m_GrabEndAnim.name);
      }
    }
  }
}
