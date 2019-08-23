using UnityEngine;
using System.Collections;
using DashFire;

[RequireComponent(typeof(BoxCollider))]
public class StroyHandler : MonoBehaviour {

  public int m_UnitIdFrom;
  public int m_UnitIdTo;
  public int m_Batch = 1;
  public GameObject m_DistroyEffect;
  private BoxCollider m_BoxCollider;
  private int m_AliveUnitCount;
	// Use this for initialization
	void Start () {
    m_AliveUnitCount = 0;
    m_BoxCollider = gameObject.GetComponent<BoxCollider>();
    if (null != m_DistroyEffect) {
      ParticleSystem[] pss = m_DistroyEffect.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in pss) {
        ps.Stop();
      }
    }
	}
	
	// Update is called once per frame
	void Update () {
	}

  public void StartStory() {
    m_BoxCollider.isTrigger = true;
    DisableParticals();
    LogicSystem.PublishLogicEvent("ge_create_npc_by_story", "npc", m_UnitIdFrom, m_UnitIdTo);
    m_AliveUnitCount = m_UnitIdTo - m_UnitIdFrom + 1;
    m_Batch -= 1;
    Debug.Log("Start Story, there are " + m_AliveUnitCount + " to be kill.");
  }

  public void OnNpcDead(int unitId) {
    if (unitId >= m_UnitIdFrom && unitId <= m_UnitIdTo) {
      m_AliveUnitCount--;
    }
    if (0 == m_AliveUnitCount && 0 < m_Batch) {
      LogicSystem.PublishLogicEvent("ge_create_npc_by_story", "npc", m_UnitIdFrom, m_UnitIdTo);
      m_AliveUnitCount = m_UnitIdTo - m_UnitIdFrom + 1;
      m_Batch -= 1;
    }
  }

  public int GetAliveUnitsCount() {
    return m_AliveUnitCount;
  }

  public void EndStory() {
    Debug.Log("Stroy end, there are(is) " + m_AliveUnitCount + " alive");
  }

  private void DisableParticals() {
    ParticleSystem[] pss = gameObject.GetComponentsInChildren<ParticleSystem>();
    foreach (ParticleSystem ps in pss) {
      ps.Stop();
    }
    if (null != m_DistroyEffect) {
      ParticleSystem[] deadPss = m_DistroyEffect.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in deadPss) {
        ps.Play();
      }
    }
  }
}
