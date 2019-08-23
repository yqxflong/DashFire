using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

/// <summary>
/// 快速型子弹
///   子弹有快速飞行轨迹，飞行轨迹由逻辑计算获得（避免物理子弹穿透的问题），
///   击中的目标点由射线计算获得
///   
/// </summary>
[System.Serializable]
public class SceneObject_MissleInfo : SceneObject_BaseInfo {
  // Effect
  public SkillEffectInfo MissleHitEffectInfo;
  public SkillEffectInfo MissleEndEffectInfo;

  // Move
  public Vector3 MoveStartPos;
  public Vector3 MoveTargetPos;
  public Vector3 MoveVelocity = new Vector3(0, -3.0f, 9.0f);
  public Vector3 MoveAccerelate = new Vector3(0, 0, 2.0f);
  public float MoveRotateVelocity = 1.5f;
  public float MoveRotateDistance = 8.0f;
  public Vector3 Gravity = new Vector3(0, -10.0f, 0);

  // Random
  public Vector3 MissleStartPositionOff = new Vector3(0, 0, 0);
  public float MissleStartPositionRandomOff = 0.0f;
  public float MissleTargetPositionRandomOff = 0.0f;

  // Attract
  public float ImpactDuration = 0;
  public float ImpactInterval = 0.3f;

  public float ImpactAttractDuration = 0.0f;
  public float ImpactAttractRange = 2.0f;
  public float ImpactAttractOffPerSecond = 1.0f;
  public float ImpactAttractOffMin = 0.3f;
  public float ImpactAttractingImpactInterval = 0.3f;

  // Impact
  public SkillImpactInfos ImpactInfos = null;
  public SkillImpactInfos ImpactEndInfos = null;
  public SkillImpactInfos ImpactEndFromRoleInfos = null;

  // Target
  public GameObject Target;
  public SceneObject_MissleInfo Clone() {
    SceneObject_MissleInfo newData = new SceneObject_MissleInfo();
    newData.Id = Id;
    newData.Attacker = Attacker;
    newData.SceneObjInfo = SceneObjInfo.Clone();
    newData.MissleHitEffectInfo = MissleHitEffectInfo.Clone();
    newData.MissleEndEffectInfo = MissleEndEffectInfo.Clone();
    newData.MoveStartPos = MoveStartPos;
    newData.MoveTargetPos = MoveTargetPos;
    newData.MoveVelocity = MoveVelocity;
    newData.MoveAccerelate = MoveAccerelate;
    newData.MoveRotateVelocity = MoveRotateVelocity;
    newData.MoveRotateDistance = MoveRotateDistance;
    newData.MissleStartPositionOff = MissleStartPositionOff;
    newData.MissleStartPositionRandomOff = MissleStartPositionRandomOff;
    newData.MissleTargetPositionRandomOff = MissleTargetPositionRandomOff;

    newData.ImpactDuration = ImpactDuration;
    newData.ImpactInterval = ImpactInterval;

    newData.ImpactAttractDuration = ImpactAttractDuration;
    newData.ImpactAttractRange = ImpactAttractRange;
    newData.ImpactAttractOffPerSecond = ImpactAttractOffPerSecond;
    newData.ImpactAttractOffMin = ImpactAttractOffMin;
    newData.ImpactAttractingImpactInterval = ImpactAttractingImpactInterval;

    newData.ImpactInfos = ImpactInfos.Clone();
    newData.ImpactEndInfos = ImpactEndInfos.Clone();
    newData.ImpactEndFromRoleInfos = ImpactEndFromRoleInfos.Clone();
    newData.Target = Target;
    return newData;
  }
}
public class SceneObjLogic_Missle : MonoBehaviour, ISceneObjectLogic {
  private SceneObject_MissleInfo m_BulletInfo = null;

  private bool m_IsMoving;
  private Vector3 m_MoveVelocity;
  private Vector3 m_HitPos;

  private bool m_IsAttractImpacting;
  private bool m_IsStartImpact;
  private bool m_IsImpacting;
  private float m_ImpactDuration;
  private float m_AttractImpactElapsed;
  private float m_ImpactElapsed;
  private float m_ImpactProcessElapsed;
  private float m_AttractImpactProcessElapsed;

  private List<GameObject> m_TargetAttractInfos;
  private List<GameObject> m_TargetImpactInfos;
  private List<GameObject> m_TargetEndImpactInfos;
  private List<GameObject> m_TargetEndFromRoleImpactInfos;

  public void Active(SceneObject_BaseInfo info) {
    m_BulletInfo = (SceneObject_MissleInfo)info;
    Quaternion missleRot = Quaternion.LookRotation(m_BulletInfo.MoveVelocity);
    m_BulletInfo.MoveStartPos += missleRot * (m_BulletInfo.MissleStartPositionOff
      + GetRandomPosOff(m_BulletInfo.MissleStartPositionRandomOff));
    m_BulletInfo.MoveTargetPos += missleRot * GetRandomPosOff(m_BulletInfo.MissleTargetPositionRandomOff);

    gameObject.transform.position = m_BulletInfo.MoveStartPos;
    gameObject.transform.rotation = missleRot;
    StartMove();
  }
  public GameObject GetAttacker() {
    return m_BulletInfo.Attacker;
  }
  private void StartMove() {
    m_MoveVelocity = m_BulletInfo.MoveVelocity;
    m_IsMoving = true;
  }
  private void StopMove() {
    m_IsMoving = false;

    m_BulletInfo.MissleHitEffectInfo.EffectPos += gameObject.transform.position;
    m_BulletInfo.MissleHitEffectInfo.EffectRot += Quaternion.identity.eulerAngles;
    TriggerImpl.PlayEffect(m_BulletInfo.MissleHitEffectInfo);

    ForceStopParticles(this.gameObject);

    StartImpact();
  }
  private void ForceStopParticles(GameObject target) {
    if (target.particleSystem != null) {
      GameObject.DestroyObject(target.particleSystem);
    }

    int childCount = target.transform.childCount;
    for (int index = 0; index < childCount; index++) {
      Transform childTrans = target.transform.GetChild(index);
      if (childTrans != null && childTrans.gameObject != null) {
        ForceStopParticles(childTrans.gameObject);
      }
    }
  }

  private void ProcessMove() {
    if (m_IsMoving) {
      float delta = Time.deltaTime;
      Vector3 posOff = m_MoveVelocity * delta;
      gameObject.transform.position += posOff;
      Vector3 flyTargetV = (m_BulletInfo.MoveTargetPos - gameObject.transform.position);
      float distanceToTarget = flyTargetV.magnitude;
      if (distanceToTarget <= m_BulletInfo.MoveRotateDistance) {
        Vector3 flyTargetDir = flyTargetV.normalized;
        float step = m_BulletInfo.MoveRotateVelocity * Time.deltaTime;
        m_MoveVelocity = Vector3.RotateTowards(m_MoveVelocity, flyTargetDir, step, 0.0f);
        m_MoveVelocity = m_MoveVelocity + delta * m_BulletInfo.Gravity;
      }
    }
  }
  internal void Update() {
    ProcessMove();
    ProcessImpact();
  }
  void OnTriggerEnter(Collider other) {
    int maskValue = ((int)(1 << other.gameObject.layer)) & ((int)(SceneLayerType.SceneStatic));
    if (maskValue != 0) {
      m_HitPos = this.gameObject.transform.position;
      StopMove();
    }
  }
  private Vector3 GetRandomPosOff(float off) {
    float xOff = Random.Range(-off, off);
    float yOff = Random.Range(-off, off);
    float zOff = Random.Range(-off, off);
    return new Vector3(xOff, yOff, zOff);
  }
  private void StartImpact() {
    m_IsStartImpact = true;

    m_AttractImpactElapsed = 0;
    m_ImpactProcessElapsed = 0;
    m_ImpactElapsed = 0;
    m_AttractImpactProcessElapsed = 0;

    m_IsAttractImpacting = (m_BulletInfo.ImpactAttractDuration > 0);
    m_IsImpacting = (m_BulletInfo.ImpactDuration > 0);

    m_TargetAttractInfos = FindTarget(m_HitPos, m_BulletInfo.ImpactAttractRange, (int)(SceneLayerType.Character));
    m_TargetImpactInfos = FindTarget(m_HitPos, m_BulletInfo.ImpactInfos.ImpactRadius, (int)(SceneLayerType.Character));
    m_TargetEndImpactInfos = FindTarget(m_HitPos, m_BulletInfo.ImpactEndInfos.ImpactRadius, (int)(SceneLayerType.Character));
    m_TargetEndFromRoleImpactInfos = FindTarget(m_HitPos, 
      m_BulletInfo.ImpactEndFromRoleInfos.ImpactRadius, (int)(SceneLayerType.Character));
    foreach (GameObject target in m_TargetAttractInfos) {
      if (target != null) {
        LogicSystem.NotifyGfxMoveControlStart(target);
      }
    }
  }
  private void StopImpact() {
    m_IsStartImpact = false;

    m_AttractImpactElapsed = 0;
    m_ImpactProcessElapsed = 0;
    m_ImpactElapsed = 0;
    m_AttractImpactProcessElapsed = 0;

    m_IsImpacting = false;
    m_IsAttractImpacting = false;

    m_BulletInfo.MissleEndEffectInfo.EffectPos += m_HitPos;
    m_BulletInfo.MissleEndEffectInfo.EffectRot += Quaternion.identity.eulerAngles;
    TriggerImpl.PlayEffect(m_BulletInfo.MissleEndEffectInfo);

    foreach (GameObject target in m_TargetAttractInfos) {
      if (target != null) {
        LogicSystem.NotifyGfxMoveControlFinish(target);
      }
    }
    ProcessSkillImpact(m_BulletInfo.Attacker, m_TargetEndImpactInfos, m_HitPos, m_BulletInfo.ImpactEndInfos);
    ProcessSkillImpactFromSender(m_BulletInfo.Attacker, m_BulletInfo.Attacker,
      m_TargetEndFromRoleImpactInfos, m_BulletInfo.ImpactEndFromRoleInfos);

    GameObject.DestroyObject(this.gameObject);
  }
  private void ProcessImpact() {
    if (!m_IsStartImpact)
      return;

    if (m_IsImpacting) {
      m_ImpactElapsed += Time.deltaTime;
      m_ImpactProcessElapsed += Time.deltaTime;
      if (m_BulletInfo.ImpactDuration > 0
        && m_ImpactElapsed < m_BulletInfo.ImpactDuration) {
        if (m_ImpactProcessElapsed >= m_BulletInfo.ImpactInterval) {
          ProcessSkillImpact(m_BulletInfo.Attacker, m_TargetImpactInfos, m_HitPos, m_BulletInfo.ImpactInfos);
          m_ImpactProcessElapsed = 0;
        }
      } else {
        m_IsImpacting = false;
      }
    }

    if (m_IsAttractImpacting) {
      m_AttractImpactElapsed += Time.deltaTime;
      m_AttractImpactProcessElapsed += Time.deltaTime;
      if (m_BulletInfo.ImpactAttractDuration > 0 && m_AttractImpactElapsed < m_BulletInfo.ImpactAttractDuration) {
        if (m_AttractImpactProcessElapsed >= m_BulletInfo.ImpactAttractingImpactInterval) {
          float attractOff = m_BulletInfo.ImpactAttractOffPerSecond * m_AttractImpactProcessElapsed;
          ProcessAttractImpact(m_BulletInfo.Attacker, m_TargetAttractInfos,
            m_HitPos, attractOff, m_BulletInfo.ImpactAttractOffMin);
          m_AttractImpactProcessElapsed = 0;
        }
      } else {
        m_IsAttractImpacting = false;
      }
    }
    if (!m_IsAttractImpacting && !m_IsImpacting) {
      StopImpact();
    }
  }

  private List<GameObject> FindTarget(Vector3 pos, float range, int mask) {
    List<GameObject> targetInfos = new List<GameObject>();
    Collider[] hitColliders = Physics.OverlapSphere(pos, range, mask);
    int index = 0;
    while (index < hitColliders.Length) {
      if (hitColliders[index] == null || hitColliders[index].gameObject == null) {
        index++;
        continue;
      }

      GameObject target = hitColliders[index].gameObject;
      if (target != null) {
        if (Script_Util.IsChildGameObject(GetAttacker(), target.GetInstanceID())) {
          index++;
          continue;
        }
        targetInfos.Add(target);
      }
      index++;
    }
    return targetInfos;
  }
  private void ProcessSkillImpact(GameObject attacker, List<GameObject> targets, Vector3 pos, SkillImpactInfos impactinfo) {
    if (targets == null || targets.Count <= 0 || impactinfo == null) {
      return;
    }

    foreach (GameObject target in targets) {
      if (target != null) {
        Vector3 faceDir = target.transform.position - pos;
        faceDir.y = 0;
        ImpactInfo curImpact = TriggerImpl.ExtractBestImpactInfo(target, impactinfo);
        if (curImpact != null) {
          ImpactInfo tImpactInfo = curImpact.Clone() as ImpactInfo;
          tImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir)
            * tImpactInfo.m_Velocity;
          tImpactInfo.Attacker = this.gameObject;
          ImpactSystem.Instance.SendImpact(attacker, target, tImpactInfo, 1);
          TriggerImpl.RecordTarget(GetAttacker(), target);
        }
      }
    }
  }
  private void ProcessSkillImpactFromSender(GameObject attacker, GameObject sender,
    List<GameObject> targets, SkillImpactInfos impactinfo) {
    if (targets == null || targets.Count <= 0 || impactinfo == null) {
      return;
    }

    foreach (GameObject target in targets) {
      if (target != null) {
        Vector3 faceDir = target.transform.position - sender.transform.position;
        faceDir.y = 0;
        ImpactInfo curImpact = TriggerImpl.ExtractBestImpactInfo(target, impactinfo);
        if (curImpact != null) {
          ImpactInfo tImpactInfo = curImpact.Clone() as ImpactInfo;
          tImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir)
            * tImpactInfo.m_Velocity;
          tImpactInfo.Attacker = sender;
          ImpactSystem.Instance.SendImpact(attacker, target, tImpactInfo, 1);
          TriggerImpl.RecordTarget(GetAttacker(), target);
        }
      }
    }
  }
  private void ProcessAttractImpact(GameObject attacker, List<GameObject> targets, Vector3 pos, float attractOff, float disMin) {
    foreach (GameObject target in targets) {
      if (target != null) {
        Vector3 attractDir = pos - target.transform.position;
        float distance = attractDir.magnitude;
        if (distance >= disMin) {
          Vector3 posOff = attractDir.normalized * attractOff;
          Vector3 targetPos = target.transform.position + posOff;
          Component controller = target.GetComponent<CharacterController>();
          if (controller != null) {
            ((CharacterController)controller).Move(posOff);
          } else {
            target.transform.position += posOff;
          }
          LogicSystem.NotifyGfxUpdatePosition(target, targetPos.x, targetPos.y, targetPos.z);
          TriggerImpl.RecordTarget(GetAttacker(), target);
        }
      }
    }
  }
}
