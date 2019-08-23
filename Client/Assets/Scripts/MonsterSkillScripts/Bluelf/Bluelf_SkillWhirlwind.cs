using UnityEngine;
using DashFire;
using System.Collections;

public class Bluelf_SkillWhirlwind : MonsterBaseSkill {

  public AnimationClip m_StartAnim;
  public AnimationClip m_RotateAnim;
  public AnimationClip m_EndAnim;
  public AnimationClip m_DeadAnim;
  public GameObject m_WarningEffect;
  public GameObject m_RotateEffect;
  public GameObject m_BurnEffect;
  public string m_RotateEffectBone = "ef_root";

  public float m_MaxDis = 6.0f;
  public float m_Speed = 3.0f;
  public float m_ImpactTriggerInterval = 0.2f;
  public ImpactInfo m_ImpactInfo;
  public float m_DamageAngle = 180.0f;
  public float m_DamageRange = 2.0f;
  private CharacterController m_MoveController;
  private int m_LayerMask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("AirWall");
  private Vector3 m_Direction;
  private Vector3 m_TargetPos;
  private float m_MovingTime;
  private float m_LastImpactTime;
  private bool m_HasRotateEffect;
	// Use this for initialization
	void Start () {
    m_AnimationPlayer = gameObject.GetComponent<AnimationManager>();
    if (null == m_AnimationPlayer) {
      Debug.LogError("Skill must have AnimationManager");
    }
    m_MoveController = gameObject.GetComponent<CharacterController>();
	}

  public override void StartSkill() {
    GeneralStartSkill();
    m_HasRotateEffect = false;
    GameObject player = LogicSystem.PlayerSelf;
    if (null != player) {
      m_Direction = player.transform.position - this.transform.position;
      m_Direction.y = 0;
      m_Direction.Normalize();
      RaycastHit hit;
      Vector3 pos = this.transform.position;
      pos.y += 0.5f;
      if (Physics.Raycast(pos, m_Direction, out hit, m_MaxDis, m_LayerMask)){
        m_TargetPos = hit.point;
      } else {
        m_TargetPos = this.transform.position + m_Direction * m_MaxDis;
      }
      m_MovingTime = Vector3.Distance(this.transform.position, m_TargetPos) / m_Speed;
      m_LastImpactTime = 0.0f;
      m_AnimationPlayer.Play(m_StartAnim);
      SetEndure(gameObject, true);
      if (null != m_WarningEffect) {
        GameObject warnEffect = ResourceSystem.NewObject(m_WarningEffect, m_AnimationPlayer.AnimationLenth(m_StartAnim)) as GameObject;
        if (null != warnEffect) {
          warnEffect.transform.position = this.transform.position + new Vector3(0.0f, 0.1f, 0.0f);
          warnEffect.transform.rotation = Quaternion.LookRotation(m_Direction);
          m_EffectList.Add(warnEffect);
        }
      }
      if (null != m_BurnEffect) {
        float duration = m_AnimationPlayer.AnimationLenth(m_StartAnim) + m_AnimationPlayer.AnimationLenth(m_EndAnim) + m_MovingTime;
        GameObject burnEffect = ResourceSystem.NewObject(m_BurnEffect, duration) as GameObject;
        if (null != burnEffect) {
          Transform parent = LogicSystem.FindChildRecursive(this.transform, "Bip001 Spine1");
          if (null != parent) {
            burnEffect.transform.parent = parent;
            burnEffect.transform.localPosition = Vector3.zero;
            burnEffect.transform.localRotation = Quaternion.identity;
            m_EffectList.Add(burnEffect);
          }
        }
      }
    } else {
      StopSkill();
    }
  }
	// Update is called once per frame
	void Update () {
    if (IsActive) {
      if(Time.time < m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_StartAnim)){
      }else if (Time.time < m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_StartAnim) + m_MovingTime) {
        if (!m_AnimationPlayer.IsPlaying(m_RotateAnim)) {
          m_AnimationPlayer.Play(m_RotateAnim);
        }
        if (!m_HasRotateEffect) {
          m_HasRotateEffect = true;
          if (null != m_RotateEffect) {
            GameObject rotateEffect = ResourceSystem.NewObject(m_RotateEffect, m_MovingTime) as GameObject;
            if (null != rotateEffect) {
              Transform parent = LogicSystem.FindChildRecursive(this.transform, m_RotateEffectBone);
              if (null != parent) {
                rotateEffect.transform.parent = parent;
                rotateEffect.transform.localPosition = Vector3.zero;
                rotateEffect.transform.localRotation = Quaternion.identity;
                m_EffectList.Add(rotateEffect);
              }
            }
          }
        }
        Vector3 motion = m_Direction * Time.deltaTime * m_Speed;
        if (!m_MoveController.isGrounded) {
          motion.y += -9.8f * Time.deltaTime;
        }
        m_MoveController.Move(m_Direction * Time.deltaTime * m_Speed);
        LogicSystem.NotifyGfxUpdatePosition(gameObject, this.transform.position.x, this.transform.position.y, this.transform.position.z, 0, this.transform.rotation.eulerAngles.y * Mathf.PI / 180f, 0);
        HandleDamage();
      } else if (Time.time < m_LastTriggerTime + m_AnimationPlayer.AnimationLenth(m_StartAnim) + m_MovingTime + m_AnimationPlayer.AnimationLenth(m_EndAnim)) {
        if (!m_AnimationPlayer.IsPlaying(m_EndAnim)) {
          SetEndure(gameObject, false);
          m_AnimationPlayer.Play(m_EndAnim);
        }
      } else {
        StopSkill();
      }
      if (IsLogicDead()) {
        NotifyNpcDead(gameObject);
        StopSkill();
      }
    }
	}

  private void HandleDamage() {
    if (Time.time > m_LastImpactTime + m_ImpactTriggerInterval) {
      m_LastImpactTime = Time.time;
      foreach (GameObject obj in GetObjInSector(this.transform.position, m_DamageRange, m_DamageAngle)) {
        if (obj.CompareTag("Player")) {
          ImpactInfo impact = m_ImpactInfo.Clone() as ImpactInfo;
          impact.Attacker = gameObject;
          impact.m_Velocity = impact.m_Velocity.z * m_Direction;
          ImpactSystem.Instance.SendImpact(gameObject, obj, impact);
        }
      }
    }
  }
  public override void StopSkill() {
    SetEndure(gameObject, false);
    GeneralStopSkill();
    if (IsLogicDead()) {
      m_AnimationPlayer.Play(m_DeadAnim);
    }
  }
}
