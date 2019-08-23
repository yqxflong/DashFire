using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollideAttatchInfo {
  public Vector3 RelativePos;
  public GameObject TargetObj;
  public bool IsCanMove = true;
}

public class EffectObjectInfo {
  public GameObject TargetObj;
  public float StartEffectTime;
  public float NextEffectTime;
}

public class SkillCollider : MonoBehaviour {
  public bool m_IsEffectMoreTimes = false;
  public float m_FirstInterval = -1;
  public float m_EffectInterval = -1;
  private int m_BelongSkillId;
  private int m_ImpactGroupId;
  private bool m_IsDelWhenLeave = false;
  private bool m_IsAttatchWhenCollide = false;
  private bool m_IsClearColliderWhenFinish = false;
  private SkillColliderManager m_ColliderManager;
  private List<CollideAttatchInfo> m_AttatchedObjects = new List<CollideAttatchInfo>();
  private List<EffectObjectInfo> m_MoreTimesEffectObjects = new List<EffectObjectInfo>();
  private List<Collider> m_CurEffectColliders = new List<Collider>();

	// Use this for initialization
	void Start () {
	}

	// Update is called once per frame
	void Update () {
    if (!enabled) {
      return;
    }
    UpdateMoreEffectObjects();
    UpdateAttatchedObjects();
  }

  void OnTriggerEnter(Collider co)
  {
    if (m_ColliderManager != null) {
      if (m_ColliderManager.AddCollideObj(co)) {
        m_ColliderManager.SendSkillImpactToObj(m_BelongSkillId, m_ImpactGroupId, co.gameObject);
        if (m_IsAttatchWhenCollide) {
          AddAttachObject(co.gameObject);
        }
        if (m_IsEffectMoreTimes) {
          AddMoreTimeEffectObject(co.gameObject);
        }
        if (m_IsClearColliderWhenFinish) {
          m_CurEffectColliders.Add(co);
        }
      }
    }
  }

  void OnTriggerExit(Collider co)
  {
    if (m_IsDelWhenLeave) {
      if (m_ColliderManager != null) {
        m_ColliderManager.RemoveCollider(co);
      }
    }
    if (m_IsEffectMoreTimes) {
      RemoveMoreTimeEffectObject(co.gameObject);
    }
  }

  void OnDisable()
  {
    m_ColliderManager.DelSkillCollider(gameObject);
    foreach (CollideAttatchInfo ca in m_AttatchedObjects) {
      if (ca.TargetObj == null) {
        continue;
      }
      DashFire.LogicSystem.NotifyGfxMoveControlFinish(ca.TargetObj);
    }
    foreach(Collider co in m_CurEffectColliders) {
      m_ColliderManager.RemoveCollider(co);
    }
    m_CurEffectColliders.Clear();
    m_AttatchedObjects.Clear();
    m_MoreTimesEffectObjects.Clear();
  }

  public void AddAttachObject(GameObject obj)
  {
    CollideAttatchInfo ca = new CollideAttatchInfo();
    ca.TargetObj = obj;
    ca.RelativePos = gameObject.transform.InverseTransformPoint(obj.transform.position);
    m_AttatchedObjects.Add(ca);
    DashFire.LogicSystem.NotifyGfxMoveControlStart(obj);
  }

  public void AddMoreTimeEffectObject(GameObject obj) {
    if (FindEffectInfoByObj(obj) != null) {
      return;
    }
    EffectObjectInfo effectinfo = new EffectObjectInfo();
    effectinfo.TargetObj = obj;
    effectinfo.StartEffectTime = Time.time;
    effectinfo.NextEffectTime = Time.time + m_FirstInterval;
    m_MoreTimesEffectObjects.Add(effectinfo);
  }

  public void RemoveMoreTimeEffectObject(GameObject obj) {
    EffectObjectInfo ei = FindEffectInfoByObj(obj);
    if (ei != null) {
      m_MoreTimesEffectObjects.Remove(ei);
    }
  }

  public EffectObjectInfo FindEffectInfoByObj(GameObject obj)
  {
    foreach (EffectObjectInfo ei in m_MoreTimesEffectObjects) {
      if (ei.TargetObj == obj) {
        return ei;
      }
    }
    return null;
  }

  public void SetBelongSkillId(int skillid) {
    m_BelongSkillId = skillid;
  }

  public void SetImpactGroup(int impactgroupid) {
    m_ImpactGroupId = impactgroupid;
  }

  public int GetBelongSkillId() {
    return m_BelongSkillId;
  }

  public void SetColliderManager(SkillColliderManager mamager)
  {
    m_ColliderManager = mamager;
  }

  public void SetIsDelWhenLeave(bool isdel)
  {
    m_IsDelWhenLeave = isdel;
  }

  public void SetIsAttatchWhenCollide(bool isattatch) {
    m_IsAttatchWhenCollide = isattatch;
  }

  public void SetIsClearColliderWhenFinish(bool isdel) {
    m_IsClearColliderWhenFinish = isdel;
  }

  //---------------------------------------------------------------------------
  private void UpdateMoreEffectObjects() {
    float now = Time.time;
    foreach (EffectObjectInfo ei in m_MoreTimesEffectObjects) {
      if (ei.NextEffectTime <= now) {
        m_ColliderManager.SendSkillImpactToObj(m_BelongSkillId, m_ImpactGroupId, ei.TargetObj);
        ei.NextEffectTime += m_EffectInterval;
      }
    }
  }

  private void UpdateAttatchedObjects() {
    if (!m_IsAttatchWhenCollide) {
      return;
    }
    foreach (CollideAttatchInfo ca in m_AttatchedObjects) {
      if (!ca.IsCanMove) {
        continue;
      }
      if (ca.TargetObj == null || !ca.TargetObj.activeSelf) {
        continue;
      }
      Vector3 new_pos = gameObject.transform.TransformPoint(ca.RelativePos);
      Vector3 old_pos = ca.TargetObj.transform.position;
      new_pos.y = old_pos.y;
      Vector3 motion = new_pos - old_pos;
      CharacterController cc = ca.TargetObj.GetComponent<CharacterController>();
      if (cc != null) {
        CollisionFlags flag = cc.Move(motion);
        if ((flag & CollisionFlags.CollidedSides) > 0) {
          ca.IsCanMove = false;
        }
      }
      DashFire.LogicSystem.NotifyGfxUpdatePosition(ca.TargetObj, new_pos.x, new_pos.y, new_pos.z);
    }
  }
}
