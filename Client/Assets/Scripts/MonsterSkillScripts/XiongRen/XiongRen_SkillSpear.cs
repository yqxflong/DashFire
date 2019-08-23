using UnityEngine;
using System.Collections;

public class XiongRen_SkillSpear : MonoBehaviour {

  public float m_ThrowSpeed = 20;
  public Transform m_Parent;
  public float m_FlyTime;
  public float m_FlyStartTime;
  public Quaternion m_LocalRotation;
  public GameObject m_Sender;
  public ImpactInfo m_ImpactInfo;

  public bool m_IsFly = false;
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
    if (m_IsFly) {
      this.transform.position += -1 * this.transform.forward * m_ThrowSpeed * Time.deltaTime;
      if (Time.time > m_FlyStartTime + m_FlyTime) {
        StopFly();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (null != player) {
          ImpactSystem.Instance.SendImpact(m_Sender, player, m_ImpactInfo);
        }
      }
    }
	}

  private void StopFly() {
    m_IsFly = false;
    this.transform.parent = m_Parent;
    this.transform.localPosition = Vector3.zero;
    this.transform.localRotation = m_LocalRotation;
  }
}
