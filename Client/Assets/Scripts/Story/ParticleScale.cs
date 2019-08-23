using UnityEngine;
using System.Collections;

public class ParticleScale : MonoBehaviour {

  public float m_Scale = 1;
  private float m_LastScale = 1;

  void OnDrawGizmos() {
    if (m_LastScale != m_Scale) {
      if (m_Scale <= 0) {
        return;
      }
      ScaleParticleEffect(m_Scale, m_LastScale);
      m_LastScale = m_Scale;
    }
  }

  private void ScaleParticleEffect(float newScale, float oldScale = 1) {
    ParticleSystem[] pss = gameObject.GetComponentsInChildren<ParticleSystem>();
    foreach (ParticleSystem ps in pss) {
      ps.startSpeed = ps.startSpeed / oldScale * newScale;
      ps.startSize = ps.startSize / oldScale * newScale;
    }
  }
  void OnDrawGizmosSelected() {
    if (m_LastScale != m_Scale) {
      if (m_Scale <= 0) {
        return;
      }
      ScaleParticleEffect(m_Scale, m_LastScale);
      m_LastScale = m_Scale;
    }
  }
}
