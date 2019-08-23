using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class ApplyGravity : MonoBehaviour {

  public float m_Gravity = -10.0f;
  public float m_TriggerInterval = 0.5f;
  private float m_verticalSpeed = 0.0f;
  private CharacterController m_MoveController;
  private BaseSkillManager m_SkillManager;
  private float m_LastTriggerTime;
	// Use this for initialization
	void Start () {
    m_MoveController = gameObject.GetComponent<CharacterController>();
    m_SkillManager = gameObject.GetComponent<BaseSkillManager>();
    m_verticalSpeed = 0.0f;
    m_LastTriggerTime = Time.time;
	}
	
	// Update is called once per frame
	void Update () {
    if (Time.time > m_LastTriggerTime + m_TriggerInterval + Random.Range(0.0f, 0.1f)) {
      m_LastTriggerTime = Time.time;
      if (ImpactType.None == ImpactSystem.Instance.GetImpactState(gameObject) && !IsUsingSkill()) {
        if (m_MoveController.isGrounded) {
          m_verticalSpeed = 0.0f;
        } else {
          m_verticalSpeed += m_Gravity * Time.deltaTime;
          m_MoveController.Move(new Vector3(0, m_verticalSpeed * Time.deltaTime, 0));
        }
      }
    }
	}

  private bool IsUsingSkill() {
    if (m_SkillManager != null && m_SkillManager.IsUsingSkill()) {
      return true;
    }
    return false;
  }
}
