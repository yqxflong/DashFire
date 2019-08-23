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
public class SceneObject_AttractBulletInfo : SceneObject_BaseInfo {
  // Effect
  public SkillEffectInfo BulletHitEffectInfo;
  public SkillEffectInfo BulletEndEffectInfo;
  // Move
  public Vector3 MoveStartPos;
  public Vector3 MoveDir;
  public float MoveSpeed;
  public float MoveDistanceMax;
  public float MoveImpactRadius = 0.7f;
  public float ImpactDuration = 0;
  public float ImpactInterval = 0.3f;

  public float ImpactAttractDuration = 3.0f;
  public float ImpactAttractRange = 2.0f;
  public float ImpactAttractOffPerSecond = 0.2f;
  public float ImpactAttractOffMin = 0.3f;
  public float ImpactAttractingImpactInterval = 0.3f;

  // Impact
  public SkillImpactInfos ImpactInfos = null;
  public SkillImpactInfos ImpactEndInfos = null;
  public SkillImpactInfos ImpactEndFromRoleInfos = null;
  public Vector3 ImpactSrcPos;

  // Target
  public GameObject Target;
  public SceneObject_AttractBulletInfo Clone() {
    SceneObject_AttractBulletInfo newData = new SceneObject_AttractBulletInfo();
    newData.Id = Id;
    newData.Attacker = Attacker;
    newData.SceneObjInfo = SceneObjInfo.Clone();
    newData.BulletHitEffectInfo = BulletHitEffectInfo.Clone();
    newData.BulletEndEffectInfo = BulletEndEffectInfo.Clone();
    newData.MoveStartPos = MoveStartPos;
    newData.MoveDir = MoveDir;
    newData.MoveSpeed = MoveSpeed;
    newData.MoveDistanceMax = MoveDistanceMax;
    newData.MoveImpactRadius = MoveImpactRadius;
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
    newData.ImpactSrcPos = ImpactSrcPos;

    newData.Target = Target;
    return newData;
  }
}
public class SceneObjLogic_AttractBullet : MonoBehaviour, ISceneObjectLogic {
  private SceneObject_AttractBulletInfo m_BulletInfo = null;

  private bool m_IsMoving;
  private float m_MoveDuration;
  private float m_MoveElapsed;

  private bool m_IsAttractImpacting;
  private bool m_IsStartImpact;
  private bool m_IsImpacting;
  private float m_ImpactDuration;
  private float m_AttractImpactElapsed;
  private float m_ImpactElapsed;
  private float m_ImpactProcessElapsed;
  private float m_AttractImpactProcessElapsed;

  private Vector3 m_HitPos;
  private Vector3 m_HitNormal;
  private bool m_IsHit;
  private RaycastHit m_MoveHitInfo;
  private List<GameObject> m_TargetAttractInfos;
  private List<GameObject> m_TargetImpactInfos;
  private List<GameObject> m_TargetEndImpactInfos;
  private List<GameObject> m_TargetEndFromRoleImpactInfos;

  public void Active(SceneObject_BaseInfo info) {
    m_BulletInfo = (SceneObject_AttractBulletInfo)info;
    gameObject.transform.position = m_BulletInfo.MoveStartPos;
    gameObject.transform.rotation = Quaternion.LookRotation(m_BulletInfo.MoveDir);
    StartMove();
  }
  public GameObject GetAttacker() {
    return m_BulletInfo.Attacker;
  }
  private void StartMove() {
    m_MoveDuration = 0;
    m_MoveElapsed = 0;
    m_IsMoving = true;

    Vector3 rayDirRevert = m_BulletInfo.MoveDir * (-1);
    Vector3 impactScrPos = m_BulletInfo.ImpactSrcPos + rayDirRevert * m_BulletInfo.MoveImpactRadius;
    Ray ray = new Ray(impactScrPos, m_BulletInfo.MoveDir);
    RaycastHit[] targetHitInfos = Physics.SphereCastAll(ray, m_BulletInfo.MoveImpactRadius,
      m_BulletInfo.MoveDistanceMax);
    Script_Util.DrawPhysicsSphereCastLine(ray, m_BulletInfo.MoveDistanceMax, m_BulletInfo.MoveImpactRadius, Color.red, 10.0f);
    if (targetHitInfos != null && targetHitInfos.Length > 0) {
      Script_Util.SortRaycastHit(ref targetHitInfos);
      m_IsHit = true;
      m_MoveHitInfo = targetHitInfos[0];
      m_MoveDuration = m_MoveHitInfo.distance / m_BulletInfo.MoveSpeed;
      m_HitPos = m_MoveHitInfo.point;
    } else {
      m_IsHit = false;
      m_MoveDuration = m_BulletInfo.MoveDistanceMax / m_BulletInfo.MoveSpeed;
      m_HitPos = Script_Util.CaculateRelativePos(m_BulletInfo.MoveStartPos,
        m_BulletInfo.MoveDir, m_BulletInfo.MoveDistanceMax);
    }
  }
  private void StopMove() {
    m_MoveDuration = 0;
    m_MoveElapsed = 0;
    m_IsMoving = false;

    Vector3 contactPointNormal = m_BulletInfo.MoveDir * (-1);
    m_BulletInfo.BulletHitEffectInfo.EffectPos += m_HitPos;
    m_BulletInfo.BulletHitEffectInfo.EffectRot += Quaternion.identity.eulerAngles;
    TriggerImpl.PlayEffect(m_BulletInfo.BulletHitEffectInfo);

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
      m_MoveElapsed += Time.deltaTime;
      if (m_MoveElapsed < m_MoveDuration) {
        Vector3 curPos = Script_Util.CaculateFlyPos(m_BulletInfo.MoveStartPos,
          m_BulletInfo.MoveDir, m_BulletInfo.MoveSpeed, m_MoveElapsed);
        gameObject.transform.position = curPos;
      } else {
        StopMove();
      }
    }
  }

  internal void Update() {
    ProcessMove();
    ProcessImpact();
  }
  private void StartImpact() {
    m_IsStartImpact = true;

    m_AttractImpactElapsed = 0;
    m_ImpactProcessElapsed = 0;
    m_ImpactElapsed = 0;
    m_AttractImpactProcessElapsed = 0;

    m_IsAttractImpacting = (m_BulletInfo.ImpactAttractDuration > 0);
    m_IsImpacting = (m_BulletInfo.ImpactDuration > 0);

    m_TargetAttractInfos = FindTarget(m_HitPos,
      m_BulletInfo.ImpactAttractRange, (int)(SceneLayerType.Character));
    m_TargetImpactInfos = FindTarget(m_HitPos,
      m_BulletInfo.ImpactInfos.ImpactRadius, (int)(SceneLayerType.Character));
    m_TargetEndImpactInfos = FindTarget(m_HitPos,
      m_BulletInfo.ImpactEndInfos.ImpactRadius, (int)(SceneLayerType.Character));
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

    Vector3 contactPointNormal = m_BulletInfo.MoveDir * (-1);
    m_BulletInfo.BulletEndEffectInfo.EffectPos += m_HitPos;
    m_BulletInfo.BulletEndEffectInfo.EffectRot += Quaternion.identity.eulerAngles;
    TriggerImpl.PlayEffect(m_BulletInfo.BulletEndEffectInfo);

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
  private void ProcessSkillImpact(GameObject attacker, List<GameObject> targets,
    Vector3 pos, SkillImpactInfos impactinfo) {
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
    if (targets == null || targets.Count <= 0) {
      return;
    }
    
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
