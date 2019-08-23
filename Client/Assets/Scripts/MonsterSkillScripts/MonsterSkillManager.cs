using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DashFire;

public class MonsterSkillManager : MonoBehaviour {


	// Use this for initialization
	void Start () {
    RegisterSkillLogic();
	}
	
	// Update is called once per frame
	void Update () {
	}

  public void MonsterStartSkill(SkillParam param) {
    if (CanUseSkill()) {
      if (m_SkillLogics.ContainsKey(param.SkillId) && !m_SkillLogics[param.SkillId].IsCoolingDown()) {
        m_SkillLogics[param.SkillId].StartSkill();
      }
    }
  }

  public bool IsCastSkill() {
    MonsterBaseSkill[] skillLogics = gameObject.GetComponents<MonsterBaseSkill>();
    foreach (MonsterBaseSkill skillLogic in skillLogics) {
      if (skillLogic.IsCastSkill()) {
        return true;
      }
    }
    return false;
  }

  public bool CanUseSkill() {
   SharedGameObjectInfo info = LogicSystem.GetSharedGameObjectInfo(gameObject);
   if (null != info && info.Blood <= 0) 
     return false;
    if (IsCastSkill())
      return false;
    BaseImpact[] impacts = gameObject.GetComponents<BaseImpact>();
    foreach (BaseImpact impact in impacts) {
      if (impact.IsAcitve)
        return false;
    }
    return true;
  }

  private void RegisterSkillLogic() {
    MonsterBaseSkill[] skillLogics = gameObject.GetComponents<MonsterBaseSkill>();
    foreach (MonsterBaseSkill skillLogic in skillLogics) {
      if (!m_SkillLogics.ContainsKey(skillLogic.m_SkillId)) {
        m_SkillLogics.Add(skillLogic.m_SkillId, skillLogic);
      }
    }
  }

  private Dictionary<int, MonsterBaseSkill> m_SkillLogics = new Dictionary<int, MonsterBaseSkill>();
}
