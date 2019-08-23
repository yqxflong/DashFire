using UnityEngine;
using System.Collections;
using DashFire;

[RequireComponent(typeof(Animator))]
public class ClimbMonster : MonoBehaviour {

  public Transform m_MatchTransform;
  public Transform m_LeftHandTransform;
  public float m_ClimbStopDis = 6.0f;
  public float m_ClimbSpeed = 2.0f;
  public int m_NpcId = 3;

  // Use this for initialization
  void Start() {
  }

  void Enable() {

  }
  // Update is called once per frame
  void Update() {
    Animator animator = gameObject.GetComponent<Animator>();
    if (null != animator) {
    if (!HasArrive() && m_MatchTransform.position.y - this.transform.position.y > m_ClimbStopDis) {
      animator.SetBool("StartClimp", true);
      Vector3 direction =  m_MatchTransform.position - this.transform.position;
      direction.Normalize();
      RaycastHit hit = new RaycastHit();
      bool ret = Physics.Raycast(this.transform.position, this.transform.forward, out hit, 5.0f);
      if (ret) {
        this.transform.position = hit.point - this.transform.forward * 0.1f;
        this.transform.position += m_ClimbSpeed * Time.deltaTime * direction;
      } else {
        Debug.LogError("There is no wall in 5m");
      }
    } else if(!animator.GetBool("ClimbEnd")){
      animator.SetBool("StartClimp", false);
      animator.SetBool("ClimpEnd", true);
			animator.MatchTarget( m_MatchTransform.position, m_MatchTransform.rotation, AvatarTarget.LeftHand, new MatchTargetWeightMask(new Vector3(1, 1, 1), 0), 0.07f, 0.38f);
    }
  }
}
  public void OnMonsterEnterScene() {
    Animator animator = gameObject.GetComponent<Animator>();
    float directionAngle = Quaternion.ToEulerAngles(animator.bodyRotation).y;
    Debug.LogError(this.transform.eulerAngles);
    LogicSystem.PublishLogicEvent("ge_create_npc", "npc", m_NpcId, this.transform.position.x,this.transform.position.y, this.transform.position.z, directionAngle);
    GameObject.Destroy(gameObject);
  }

  private bool HasArrive() {
    Animator animator = gameObject.GetComponent<Animator>();
    return animator.GetBool("ClimpEnd") || animator.GetBool("TurnStand");
  }

  public void SetMatchTransform(Transform t) {
    m_MatchTransform = t;
  }

  public void SetDirection(Vector3 d) {
  }
}
