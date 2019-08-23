using UnityEngine;
using System.Collections;

public class ImpactComa : BaseImpact {

  public AnimationClip m_ComaAnim;
  public void StartImpact(ImpactInfo impactInfo) {
    if (null == impactInfo) {
      Debug.LogWarning("ImpactComa::StartImpact -- impactInfo is null");
    }
    m_AnimationPlayer.CrossFade(m_ComaAnim.name);
    GeneralStartImpact(impactInfo);
  }
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    if (IsAcitve) {
      TickAnimation(Time.deltaTime);
      TickEffect(Time.deltaTime);
      TickMovement(Time.deltaTime);
      if (Time.time > m_StartTime + 3) {
        StopImpact();
      }
    }
	}

  public override void StopImpact() {
    GeneralStopImpact();
  }
}