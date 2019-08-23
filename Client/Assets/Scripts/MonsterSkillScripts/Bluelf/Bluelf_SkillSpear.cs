using UnityEngine;
using DashFire;
using System.Collections;

public class Bluelf_SkillSpear : MonoBehaviour {

  public float m_ThrowSpeed = 10;
  public float m_MaxLiveTime = 3.0f;
  public GameObject m_Sender;
  public ImpactInfo m_ImpactInfo;
  private bool m_HitCollider = false;
  public float m_MaxDepth = 1.5f;
  private float m_AfterHitColliderTime = 0.0f;

  private float m_BornTime;
	void Start () {
	}

  void OnEnable() {
    m_BornTime = Time.time;
    m_HitCollider = false;
  }
	
	// Update is called once per frame
	void Update () {
    if (!m_HitCollider) {
      this.transform.position += this.transform.forward * m_ThrowSpeed * Time.deltaTime;
    } else {
      m_AfterHitColliderTime -= Time.deltaTime;
      if (m_AfterHitColliderTime > 0.0f) {
        this.transform.position += this.transform.forward * m_ThrowSpeed * Time.deltaTime;
      }
    }
    if (Time.time > m_BornTime + m_MaxLiveTime) {
      ResourceSystem.RecycleObject(gameObject);
    }
	}

  void OnTriggerEnter(Collider other) {
    if (!m_HitCollider) {
      if (null != other.gameObject) {
        if (other.gameObject.CompareTag("Player")) {
          OnHitTarget(other.gameObject);
        } else if(other.gameObject.layer == LayerMask.NameToLayer("Terrains") ||
                  other.gameObject.layer == LayerMask.NameToLayer("Default")){
          m_HitCollider = true;
          m_AfterHitColliderTime = Random.Range(0.8f, m_MaxDepth) / m_ThrowSpeed;
        }
      }
    }
  }

  private void OnHitTarget(GameObject obj) {
    ImpactInfo impact = m_ImpactInfo.Clone() as ImpactInfo;
    impact.Attacker = m_Sender;
    impact.m_Velocity = this.transform.forward * impact.m_Velocity.z;
    ImpactSystem.Instance.SendImpact(m_Sender, obj, impact.Clone() as ImpactInfo);
    ResourceSystem.RecycleObject(gameObject);
  }

  public void SetSender(GameObject sender) {
    m_Sender = sender;
  }
}

