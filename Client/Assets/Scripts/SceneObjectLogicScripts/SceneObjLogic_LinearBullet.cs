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
public class SceneObject_LinearBulletInfo : SceneObject_BaseInfo {
  // Effect
  public SkillEffectInfo BulletHitEffectInfo;
  public SkillEffectInfo BulletEndEffectInfo;
  // Move
  public Vector3 MoveStartPos;
  public Vector3 MoveDir;
  public float MoveSpeed;
  public float MoveDistanceMax;

  // Impact
  public SkillImpactInfos ImpactInfos = null;
  public List<TargetStateType> ExceptTargetState = new List<TargetStateType>();
  public Vector3 ImpactSrcPos;

  // Target
  public GameObject Target;
  public SceneObject_LinearBulletInfo Clone() {
    SceneObject_LinearBulletInfo newData = new SceneObject_LinearBulletInfo();
    newData.Id = Id;
    newData.Attacker = Attacker;
    newData.SceneObjInfo = SceneObjInfo.Clone();
    newData.BulletHitEffectInfo = BulletHitEffectInfo.Clone();
    newData.BulletEndEffectInfo = BulletEndEffectInfo.Clone();
    newData.MoveStartPos = MoveStartPos;
    newData.MoveDir = MoveDir;
    newData.MoveSpeed = MoveSpeed;
    newData.MoveDistanceMax = MoveDistanceMax;
    newData.ImpactInfos = ImpactInfos.Clone();
    newData.ExceptTargetState = new List<TargetStateType>(ExceptTargetState);
    newData.ImpactSrcPos = ImpactSrcPos;
    newData.Target = Target;
    return newData;
  }
}
public class SceneObjLogic_LinearBullet : MonoBehaviour, ISceneObjectLogic {
  private SceneObject_LinearBulletInfo m_BulletInfo = null;

  private bool m_IsMoving;
  private float m_MoveDuration;
  private float m_MoveElapsed;

  private Vector3 m_HitPos;
  private Vector3 m_HitNormal;
  private bool m_IsHit;
  private RaycastHit m_MoveHitInfo;
  private List<RaycastHit> m_TargetHitInfos;

  public void Active(SceneObject_BaseInfo info) {
    m_BulletInfo = (SceneObject_LinearBulletInfo)info;
    gameObject.transform.position = m_BulletInfo.MoveStartPos;
    gameObject.transform.rotation = Quaternion.LookRotation(m_BulletInfo.MoveDir);
    StartMove();
    ProcessImpact();
  }
  public GameObject GetAttacker() {
    return m_BulletInfo.Attacker;
  }
  private void StartMove() {
    m_MoveDuration = 0;
    m_MoveElapsed = 0;
    m_IsMoving = true;

    Ray ray = new Ray(m_BulletInfo.MoveStartPos, m_BulletInfo.MoveDir);
    if (Physics.Raycast(ray, out m_MoveHitInfo, m_BulletInfo.MoveDistanceMax, (int)SceneLayerType.SceneStatic)) {
      m_IsHit = true;
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
    if (m_IsHit) {
      m_BulletInfo.BulletHitEffectInfo.EffectPos += m_HitPos;
      m_BulletInfo.BulletHitEffectInfo.EffectRot = Quaternion.LookRotation(contactPointNormal).eulerAngles;
      TriggerImpl.PlayEffect(m_BulletInfo.BulletHitEffectInfo);
    } else {
      m_BulletInfo.BulletEndEffectInfo.EffectPos += m_HitPos;
      m_BulletInfo.BulletEndEffectInfo.EffectRot += Quaternion.LookRotation(contactPointNormal).eulerAngles;
      TriggerImpl.PlayEffect(m_BulletInfo.BulletEndEffectInfo);
    }
    GameObject.Destroy(gameObject);
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
  private void ProcessImpact() {
    if (m_TargetHitInfos != null) {
      m_TargetHitInfos.Clear();
    }

    Vector3 rayDirRevert = m_BulletInfo.MoveDir * (-1);
    Vector3 impactScrPos = m_BulletInfo.ImpactSrcPos + rayDirRevert * m_BulletInfo.ImpactInfos.ImpactRadius;
    Ray ray = new Ray(impactScrPos, m_BulletInfo.MoveDir);
    float distance = (m_HitPos - m_BulletInfo.ImpactSrcPos).magnitude;
    RaycastHit[] targetHitInfos = Physics.SphereCastAll(ray, m_BulletInfo.ImpactInfos.ImpactRadius,
      distance, (int)SceneLayerType.Character);
    Script_Util.DrawPhysicsSphereCastLine(ray, distance, m_BulletInfo.ImpactInfos.ImpactRadius, Color.red, 10.0f);
    if (targetHitInfos != null && targetHitInfos.Length > 0) {
      m_TargetHitInfos = new List<RaycastHit>(targetHitInfos);
      m_TargetHitInfos = CheckTarget(m_TargetHitInfos, m_BulletInfo.ImpactInfos);
      foreach (RaycastHit hitInfo in m_TargetHitInfos) {
        GameObject target = hitInfo.transform.gameObject;
        if (target != null) {
          ImpactInfo tImpactInfo = TriggerImpl.ExtractBestImpactInfo(target, m_BulletInfo.ImpactInfos);
          if (tImpactInfo != null) {
            tImpactInfo = tImpactInfo.Clone() as ImpactInfo;
            Script_Util.AdjustImpactEffectDataPos(target, GetAttacker(), tImpactInfo, hitInfo.point, m_BulletInfo.MoveDir);
            Vector3 faceDir = m_BulletInfo.MoveDir;
            faceDir.y = 0;
            tImpactInfo.m_Velocity = Quaternion.LookRotation(faceDir) * tImpactInfo.m_Velocity;
            tImpactInfo.Attacker = GetAttacker();
            ImpactSystem.Instance.SendImpact(GetAttacker(), target, tImpactInfo);
            TriggerImpl.RecordTarget(GetAttacker(), target);
          }
        }
      }
    }
  }
  internal void Update() {
    ProcessMove();
  }
  private List<RaycastHit> CheckTarget(List<RaycastHit> hitInfos, SkillImpactInfos impact) {
    List<RaycastHit> retInfos = new List<RaycastHit>();
    foreach (RaycastHit hitinfo in hitInfos) {
      GameObject target = hitinfo.transform.gameObject;
      if (Script_Util.IsChildGameObject(GetAttacker(), target.GetInstanceID())) {
        continue;
      }
      if (TriggerImpl.CheckTargetInImpactExcept(target, impact)) {
        continue;
      }
      retInfos.Add(hitinfo);
    }
    return retInfos;
  }
}
