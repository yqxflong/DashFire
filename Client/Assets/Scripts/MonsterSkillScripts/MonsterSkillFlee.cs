using UnityEngine;
using System.Collections;
using DashFire;

[RequireComponent(typeof(CharacterController))]
public class MonsterSkillFlee : MonsterBaseSkill {

  public float m_SafeDis = 5.0f;
  public float m_MinAngle = 10.0f;
  public float m_FleeSpeed = 2.0f;
  public float m_MaxFleeTime = 6.0f;
  public AnimationClip m_FleeAnim;
  public AnimationClip m_IdleAnim;
  private CharacterController m_MoveController;
  private GameObject m_Player;
  public float m_Gravity = -9.8f;
  private float m_YSpeed = 0.0f;
	// Use this for initialization
	void Start () {
    m_AnimationPlayer = gameObject.GetComponentInChildren<AnimationManager>();
    if (null == m_AnimationPlayer) {
      m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
      if (null != m_AnimationPlayer) {
        Debug.LogError("Skill must have AnimationManager");
      }
    }
    m_MoveController = gameObject.GetComponent<CharacterController>();
	}

  public override void StartSkill() {
    GeneralStartSkill();
    m_YSpeed = 0.0f;
    m_Player = GameObject.FindGameObjectWithTag("Player");
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if (null != m_Player) {
        if (Vector3.Distance(this.transform.position, m_Player.transform.position) < m_SafeDis + 1.0f) {
          Quaternion rotation = Quaternion.LookRotation(GetFleeDir());
          if (m_MinAngle < Mathf.Abs(Quaternion.Angle(this.transform.rotation, rotation))) {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, rotation, 0.2f);
          } else {
            if(!m_AnimationPlayer.IsPlaying(m_FleeAnim.name))
              m_AnimationPlayer.Play(m_FleeAnim.name);
            if (m_MoveController.isGrounded) {
              m_YSpeed = 0.0f;
            } else {
              m_YSpeed += Time.deltaTime * m_Gravity;
            }
            m_MoveController.Move((this.transform.forward * m_FleeSpeed + new Vector3(0, m_YSpeed, 0)) * Time.deltaTime);
          }
        }else if(Vector3.Distance(this.transform.position, m_Player.transform.position) > m_SafeDis){
          Quaternion faceRotation = Quaternion.LookRotation(GetFleeDir() * -1);
          if (m_MinAngle < Mathf.Abs(Quaternion.Angle(this.transform.rotation, faceRotation))) {
            this.transform.rotation = Quaternion.Lerp(this.transform.rotation, faceRotation, 0.2f);
          } else {
            StopSkill();
          }
        }
      LogicSystem.NotifyGfxUpdatePosition(gameObject, this.transform.position.x, this.transform.position.y, this.transform.position.z, 0, this.transform.rotation.eulerAngles.y * Mathf.PI / 180.0f, 0);
      } else {
        StopSkill();
      }
      if (Time.time > m_LastTriggerTime + m_MaxFleeTime) {
        StopSkill();
      }
    }
	}

  public override void StopSkill() {
    GeneralStopSkill();
    if (null != m_IdleAnim) {
      m_AnimationPlayer.Play(m_IdleAnim.name);
    }
  }

  private Vector3 GetFleeDir() {
    Vector3 dir = this.transform.position - m_Player.transform.position;
    dir.y = 0;
    dir.Normalize();
    return dir;
  }
}
