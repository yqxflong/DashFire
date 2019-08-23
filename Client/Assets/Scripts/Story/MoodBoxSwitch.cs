using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MoodBoxSwitch : MonoBehaviour {

  public List<GameObject> m_DoorsToOpen = new List<GameObject>();
  public List<GameObject> m_DoorsToClose = new List<GameObject>();
  public bool m_TriggerOnce = true;

  private bool m_HasTriggered = false;

  void Start() {
    foreach (GameObject obj in m_DoorsToClose) {
      ParticleSystem[] pss = obj.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in pss) {
        ps.Stop();
      }
    }
  }

  void OnTriggerEnter(Collider co) {
    if (m_HasTriggered && m_TriggerOnce) {
    } else {
      if (co.gameObject.CompareTag("Player")) {
        OpenDoors(m_DoorsToOpen);
        CloseDoors(m_DoorsToClose);
        m_HasTriggered = true;
      }
    }
  }

  private void CloseDoors(List<GameObject> doorsToClose) {
    foreach (GameObject obj in doorsToClose) {
      BoxCollider co = obj.GetComponent<BoxCollider>();
      if (null != co) {
        if (co.isTrigger) {
          co.isTrigger = false;
        }
      } else {
        Debug.LogError("Door to be closed does not have a BoxCollider");
      }
      ParticleSystem[] pss = obj.GetComponentsInChildren<ParticleSystem>();
      foreach (ParticleSystem ps in pss) {
        ps.Play();
      }
    }
  }

  private void OpenDoors(List<GameObject> doorsToOpen) {
    foreach (GameObject obj in doorsToOpen) {
      BoxCollider co = obj.GetComponent<BoxCollider>();
      if (null != co) {
        if (!co.isTrigger) {
          co.isTrigger = true;
        }
      } else {
        Debug.LogError("Door to be opened does not have a BoxCollider");
      }
    }
  }
}
