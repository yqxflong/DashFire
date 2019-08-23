using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SkillColliderManager : MonoBehaviour {
  public Vector3 m_HitMotion;

  private List<Collider> m_Colliders = new List<Collider>();
  private SkillManager m_SkillManager;
  private ImpactSystem m_ImpactSystem;
  private CharacterCamp m_CampInfo;
  private List<GameObject> m_SkillColliders  = new List<GameObject>();

	// Use this for initialization
	void Start () {
    m_SkillManager = gameObject.GetComponent<SkillManager>();
    SkillCollider[] skill_colliders = gameObject.GetComponentsInChildren<SkillCollider>();
    foreach (SkillCollider co in skill_colliders) {
      co.SetColliderManager(this);
    }
    m_ImpactSystem = new ImpactSystem();
    m_CampInfo = gameObject.GetComponent<CharacterCamp>();
	}

	// Update is called once per frame
	void Update () {
	}

  public bool AddCollideObj(Collider co)
  {
    if (!IsEnimy(co)) {
      return false;
    }
    if (m_Colliders.Contains(co)) {
      return false;
    }
    m_Colliders.Add(co);
    return true;
  }

  public void SendSkillImpactToObj(int skillid, int impactgroupid, GameObject obj) {
     if (m_SkillManager == null) {
      Debug.Log("skill manage is null!");
      return;
    }
    SkillScript skill_script = m_SkillManager.GetSkillById(skillid);
    if (skill_script == null) {
      return;
    }
    skill_script.PlayCollideSound();
    StateImpact impacts = FindImpactByState(skill_script, impactgroupid, GetBeHitState(obj));
    if (impacts == null) {
      return;
    }
    foreach(ImpactInfo imp_info in impacts.m_ImpactInfos) {
      ImpactInfo info = imp_info.Clone() as ImpactInfo;
      info.m_Velocity = gameObject.transform.TransformPoint(info.m_Velocity);
      info.m_Velocity = info.m_Velocity - gameObject.transform.position;
      info.m_Attacker = gameObject;
      m_ImpactSystem.SendImpact(gameObject, obj, info);
    }
  }

  public List<GameObject> FiltEnimy(List<GameObject> list) {
    List<GameObject> result = new List<GameObject>();
    foreach (GameObject obj in list) {
      if (IsEnimy(obj)) {
        result.Add(obj);
      }
    }
    return result;
  }

  public void RemoveCollider(Collider co)
  {
    if (!IsEnimy(co)) {
      return;
    }
    m_Colliders.Remove(co);
  }

  public StateImpact FindImpactByState(SkillScript skillscript, int impactgroupid, BeHitState state)
  {
    ImpactGroup ig = skillscript.GetImpactGroupById(impactgroupid);
    if (ig == null) {
      return null;
    }
    foreach (StateImpact stateinfo in ig.m_StateImpact) {
      if (state == stateinfo.m_State) {
        return stateinfo;
      }
    }
    if (state == BeHitState.kDefault) {
      return null;
    } else {
      return FindImpactByState(skillscript, impactgroupid, BeHitState.kDefault);
    }
  }

  public BeHitState GetBeHitState(GameObject obj)
  {
    if (obj == null) {
      return BeHitState.kDefault;
    }
    BaseImpact[] impacts = obj.GetComponentsInChildren<BaseImpact>();
    foreach(BaseImpact imp in impacts) {
      if (imp.IsAcitve) {
        switch (imp.GetImpactType()) {
        case ImpactType.Stiffness:
          return BeHitState.kStiffness;
        case ImpactType.HitFly:
          return BeHitState.kLauncher;
        case ImpactType.KnockDown:
          return BeHitState.kKnockDown;
        }
      }
    }
    return BeHitState.kDefault;
  }

  public List<Collider> GetColliders()
  {
    return m_Colliders;
  }

  public bool IsCollided()
  {
    return m_Colliders.Count > 0;
  }

  public void ClearColliders()
  {
    m_Colliders.Clear();
  }

  public void DamageArea(string str_params)
  {
    string[] params_list = str_params.Split(' ');
    if (params_list.Length < 4) {
      return;
    }
    Vector3 local_center;
    local_center.x = (float)System.Convert.ToDouble(params_list[0]);
    local_center.y = (float)System.Convert.ToDouble(params_list[1]);
    local_center.z = (float)System.Convert.ToDouble(params_list[2]);
    float radius = (float)System.Convert.ToDouble(params_list[3]);
    int impactgroupid = 1;
    if (params_list.Length >= 5) {
      impactgroupid = System.Convert.ToInt32(params_list[4]);
    }
    bool isClearTargetWhenFinish = false;
    if (params_list.Length >= 6) {
      isClearTargetWhenFinish = System.Convert.ToBoolean(params_list[5]);
    }

    Vector3 world_center = gameObject.transform.TransformPoint(local_center);
    Collider[] targets = Physics.OverlapSphere(world_center, radius);
    int skillid = -1;
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss != null) {
      skillid = ss.SkillId;
    }
    foreach(Collider co in targets) {
      if (AddCollideObj(co)) {
        SendSkillImpactToObj(skillid, impactgroupid, co.gameObject);
        if (isClearTargetWhenFinish) {
          m_Colliders.Remove(co);
        }
      }
    }
  }

  public void FindMoveTarget(string param_str) {
    string[] params_list = param_str.Split(' ');
    if (params_list.Length < 8) {
      return;
    }
    int index = 0;
    Vector3 center = new Vector3();
    center.x = (float)System.Convert.ToDouble(params_list[index++]);
    center.y = (float)System.Convert.ToDouble(params_list[index++]);
    center.z = (float)System.Convert.ToDouble(params_list[index++]);
    float radius = (float)System.Convert.ToDouble(params_list[index++]);
    float degree = (float)System.Convert.ToDouble(params_list[index++]);
    bool isChooseClosest = System.Convert.ToBoolean(params_list[index++]);
    float toTargetDistanceRatio = (float)System.Convert.ToDouble(params_list[index++]);
    float toTargetConstDistance = (float)System.Convert.ToDouble(params_list[index++]);

    Vector3 world_center = transform.TransformPoint(center);
    List<GameObject> allobj = TargetChooser.FindTargetInSector(world_center, radius,
                                                               transform.forward, world_center,
                                                               degree);
    GameObject target = null;
    if (isChooseClosest) {
      target = TargetChooser.GetNearestObj(transform.position, FiltEnimy(allobj));
    } else {
      target = TargetChooser.GetMostForwardObj(transform.position, transform.forward,
                                               FiltEnimy(allobj));
    }
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss != null) {
      ss.MoveTarget = target;
      ss.ToTargetDistanceRatio = toTargetDistanceRatio;
      ss.ToTargetConstDistance = toTargetConstDistance;
    }
  }

  public void DelSkillCollider(GameObject obj) {
    m_SkillColliders.Remove(obj);
  }

  public void StartNewCollider(string str_params) {
    string[] params_list = str_params.Split(' ');
    if (params_list.Length < 4) {
      return;
    }
    int index = 0;
    bool is_attatch = System.Convert.ToBoolean(params_list[index++]);
    float remain_time = (float)System.Convert.ToDouble(params_list[index++]);
    string bone_name = params_list[index++];
    string preferb_name = params_list[index++];
    Vector3 pos = new Vector3();
    if (params_list.Length >= index + 2) {
      pos.x = (float)System.Convert.ToDouble(params_list[index++]);
      pos.y = (float)System.Convert.ToDouble(params_list[index++]);
      pos.z = (float)System.Convert.ToDouble(params_list[index++]);
    }
    Vector3 rotate = new Vector3();
    if (params_list.Length >= index + 2) {
      rotate.x = (float)System.Convert.ToDouble(params_list[index++]);
      rotate.y = (float)System.Convert.ToDouble(params_list[index++]);
      rotate.z = (float)System.Convert.ToDouble(params_list[index++]);
    }
    bool is_collide_attatch = false;
    if (params_list.Length >= index + 1) {
      is_collide_attatch = System.Convert.ToBoolean(params_list[index++]);
    }

    bool is_del_when_leave = false;
    if (params_list.Length >= index + 1) {
      is_del_when_leave = System.Convert.ToBoolean(params_list[index++]);
    }

    int impactgroupid = 1;
    if (params_list.Length >= index + 1) {
      impactgroupid = System.Convert.ToInt32(params_list[index++]);
    }

    bool is_clear_collider_when_finish = false;
    if (params_list.Length >= index + 1) {
      is_clear_collider_when_finish = System.Convert.ToBoolean(params_list[index++]);
    }

    Transform parent = EffectManager.GetChildNodeByName(gameObject, bone_name);
    if (parent == null) {
      Debug.Log("not find bone " + bone_name);
      return;
    }
    Vector3 new_pos = parent.transform.TransformPoint(pos);
    GameObject obj = DashFire.ResourceSystem.NewObject(preferb_name, remain_time) as GameObject;
    if (obj == null) {
      Debug.Log("can't create obj by " + preferb_name);
      return;
    }
    obj.transform.position = new_pos;
    obj.transform.rotation = parent.transform.rotation;
    if (is_attatch) {
      obj.transform.parent = parent.transform;
    }
    m_SkillColliders.Add(obj);
    SkillCollider[] co_arr = obj.GetComponentsInChildren<SkillCollider>();
    SkillScript skill = m_SkillManager.GetCurPlaySkill();
    foreach(SkillCollider co in co_arr) {
      if (skill != null) {
        co.SetBelongSkillId(skill.SkillId);
      }
      co.SetImpactGroup(impactgroupid);
      co.SetColliderManager(this);
      co.SetIsDelWhenLeave(is_del_when_leave);
      co.SetIsAttatchWhenCollide(is_collide_attatch);
      co.SetIsClearColliderWhenFinish(is_clear_collider_when_finish);
    }
  }

  public void StartDamageCollider()
  {
    SkillCollider[] skill_colliders = gameObject.GetComponentsInChildren<SkillCollider>();
    foreach (SkillCollider co in skill_colliders) {
      co.gameObject.collider.enabled = true;
      co.gameObject.collider.isTrigger = true;
    }
    m_Colliders.Clear();
  }

  public void StopDamageCollider()
  {
    SkillCollider[] skill_colliders = gameObject.GetComponentsInChildren<SkillCollider>();
    foreach (SkillCollider co in skill_colliders) {
      co.enabled = false;
      if (co.gameObject != null && co.gameObject.collider != null) {
        co.gameObject.collider.enabled = false;
        co.gameObject.collider.isTrigger = false;
      }
    }
    m_Colliders.Clear();
  }

  public void StopSkillColliders(int skillid) {
    GameObject go = FindSkillColliderBySkillId(skillid);
    if (go != null) {
      Destroy(go);
    }
    StopDamageCollider();
  }

  public bool IsEnimy(Collider co)
  {
    return IsEnimy(co.gameObject);
  }

  public bool IsEnimy(GameObject go)
  {
    if (go == null) {
      return false;
    }
    CharacterCamp campinfo = go.GetComponent<CharacterCamp>();
    if (campinfo == null) {
      return false;
    }
    if (m_CampInfo == null) {
      return true;
    }
    if (m_CampInfo.m_CampId != campinfo.m_CampId) {
      return true;
    }
    return false;
  }

  private GameObject FindSkillColliderBySkillId(int skillid) {
    foreach (GameObject go in m_SkillColliders) {
      if (go == null) {
        continue;
      }
      SkillCollider[] skill_colliders = go.GetComponentsInChildren<SkillCollider>();
      foreach(SkillCollider sc in skill_colliders) {
        if (sc.GetBelongSkillId() == skillid) {
          return go;
        }
      }
    }
    return null;
  }
}
