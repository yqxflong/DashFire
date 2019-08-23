using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewSwordManSkillController : SkillController {
  private SkillManager m_SkillManager;

  public NewSwordManSkillController(SkillManager skillmanager, List<SkillScript> skills)
  {
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<string>("ge_cast_skill", "game", OnPushStrSkill);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<bool>("ge_attack", "game", OnAttack);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_unlock_skill", "skill", base.UnlockSkill);
    m_SkillManager = skillmanager;
    Init(skills);
  }

  public void Init(List<SkillScript> skills) {
    foreach(SkillScript ss in skills) {
      if (IsCategoryContain(ss.SkillId)) {
        continue;
      }
      SkillNode first_node = InitNodeByScript(skills, ss);
      m_SkillCategoryDict[ss.Category] = first_node;
    }
    foreach(int id in m_UnlockedSkills) {
      SkillNode node = GetSkillNodeById(id);
      if (node != null) {
        node.IsLocked = false;
      }
    }
  }

  public override void OnTick() {
    UpdateAfterSkillAutoAttack();
    base.OnTick();
  }

  public void OnPushStrSkill(string categoryStr) {
    SkillCategory category = GetSkillCategoryFromStr(categoryStr);
    if (category != SkillCategory.kNone) {
      base.PushSkill(category, Vector3.zero);
    }
  }

  public void OnAttack(bool isAttack) {
    if (isAttack) {
      StartAttack();
    } else {
      StopAttack();
    }
  }

  public override void StartAttack(Vector3 targetpos) {
    if (targetpos == Vector3.zero) {
      base.StartAttack(targetpos);
      return;
    }
    GameObject owner = m_SkillManager.GetOwner();
    Vector3 xz_vector = targetpos - owner.transform.position;
    xz_vector.y = 0;
    float distance = xz_vector.magnitude;
    SkillInputData data = GetRealSkillInputData(SkillCategory.kAttack);
    SkillInputData flash_data = GetRealSkillInputData(SkillCategory.kFlashAttack);
    if (data != null && distance > data.castRange && flash_data != null && distance <= flash_data.castRange) {
      CancelBreakSkillTask();
      SetFaceDir(targetpos);
      SkillNode node = AddCategorySkillNode(SkillCategory.kFlashAttack);
      if (node != null) {
        node.TargetPos = targetpos;
      }
      m_IsAttacking = true;
    } else {
      base.StartAttack(targetpos);
    }
  }

  public override void ShowSkillTip(SkillCategory category, Vector3 targetpos)
  {
    m_SkillManager.ShowSkillTip(targetpos);
  }

  public override void HideSkillTip(SkillCategory category)
  {
    m_SkillManager.HideSkillTip();
  }

  public override void BeginSkillCD(SkillNode node) {
    if (node == null) {
      return;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return;
    }
    skill.BeginCD();
  }

  public override float GetSkillCD(SkillNode node) {
    if (node == null) {
      return 0;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return skill.m_CD;
  }

  public override float GetWaitInputTime(SkillNode node) {
    if (node == null) {
      return 0;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return node.StartTime + skill.NextInputTime;
  }

  public override float GetLockInputTime(SkillNode node) {
    if (node == null) {
      return 0;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return node.StartTime + skill.LockInputTime;
  }

  public override bool IsSkillCanBreak(SkillNode node, SkillCategory category = SkillCategory.kNone) {
    return IsSkillCanBreakByType(node, category);
  }

  public override bool IsSkillCanBreakByImpact(SkillNode node) {
    if (node == null) {
      return true;
    }
    SkillScript ss = m_SkillManager.GetSkillById(node.SkillId);
    if (ss == null) {
      return true;
    }
    return ss.IsCanBreakByImpact();
  }

  public override bool IsSkillCanStart(SkillNode node) {
    if (node == null) {
      return false;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return false;
    }
    return skill.CanStart();
  }

  public override bool IsSkillInCD(SkillNode node) {
    if (node == null) {
      return false;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return false;
    }
    return skill.IsInCD();
  }

  public override void BeginCurSkillCD() {
    if (m_CurSkillNode != null && !m_CurSkillNode.IsCDChecked) {
      base.BeginSkillCategoryCD(m_CurSkillNode.Category);
      m_CurSkillNode.IsCDChecked = true;
    }
  }

  public override void SetFaceDir(Vector3 targetpos) {
    m_SkillManager.SetFacePos(targetpos);
  }
  
  public override SkillInputData GetSkillInputData(SkillCategory category) {
    if (category == SkillCategory.kAttack || category == SkillCategory.kNone) {
      category = SkillCategory.kFlashAttack;
    }
    return GetRealSkillInputData(category);
  }

  public override bool StartSkill(SkillNode node)
  {
    if (node.TargetPos != Vector3.zero) {
      m_SkillManager.SetFacePos(node.TargetPos);
    }
    if (ReceiveInput.IsMoving) {
      m_SkillManager.SetFaceDir(ReceiveInput.CurDirection);
      AddBreakSkillTask();
    }
    if (m_SkillManager.StartSkillById(node.SkillId)) {
      if (node.Category == SkillCategory.kEx) {
        m_IsAttacking = false;
      }
      return true;
    }
    return false;
  }

  // child should override this function
  public override bool StopSkill(SkillNode node) {
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss != null && ss.IsActive()) {
      ss.StopSkill();
    }
    return true;
  }

  public override void ForceStopCurSkill() {
    SkillScript ss = m_SkillManager.GetCurPlaySkill();
    if (ss == null) {
      return;
    }
    ss.ForceStopSkill();
  }

  private SkillInputData GetRealSkillInputData(SkillCategory category) {
    SkillNode head = null;
    if (m_SkillCategoryDict.TryGetValue(category, out head)) {
      SkillScript ss = m_SkillManager.GetSkillById(head.SkillId);
      if (ss != null) {
        SkillInputData data = new SkillInputData();
        data.castRange = ss.m_SkillCastRange;
        data.targetChooseRange = ss.m_TargetChooseRange;
        return data;
      }
    }
    return null;
  }

  private SkillNode InitNodeByScript(List<SkillScript> skills, SkillScript ss) {
    SkillNode first = new SkillNode();
    first.SkillId = ss.SkillId;
    first.Category = ss.Category;
    SkillScript nextSkillScript = GetSkillById(skills, ss.NextSkillId);
    if (nextSkillScript != null) {
      first.NextSkillNode = InitNodeByScript(skills, nextSkillScript);
    }
    SkillScript qSkillScript = GetSkillById(skills, ss.QSkillId);
    if (qSkillScript != null) {
      first.SkillQ = InitNodeByScript(skills, qSkillScript);
    }
    SkillScript eSkillScript = GetSkillById(skills, ss.ESkillId);
    if (eSkillScript != null) {
      first.SkillE = InitNodeByScript(skills, eSkillScript);
    }
    return first;
  }

  private SkillScript GetSkillById(List<SkillScript> skills, int id) {
    foreach(SkillScript ss in skills) {
      if (ss.SkillId == id) {
        return ss;
      }
    }
    return null;
  }

  private bool IsCategoryContain(int skillid) {
    foreach(SkillNode head in m_SkillCategoryDict.Values) {
      if (FindSkillNodeInChildren(head, skillid) != null) {
        return true;
      }
    }
    return false;
  }

  private void UpdateAfterSkillAutoAttack() {
    if (m_WaiteSkillBuffer.Count > 0) {
      return;
    }
    if (m_CurSkillNode != null && IsSkillCanBreakByType(m_CurSkillNode, SkillCategory.kAttack)) {
      if (Time.time > GetWaitInputTime(m_CurSkillNode)) {
        return;
      }
      SkillScript ss = m_SkillManager.GetSkillById(m_CurSkillNode.SkillId);
      if (ss != null && ss.m_IsAttackAfterSkill && m_WaiteSkillBuffer.Count == 0) {
        StartAttack();
      }
    }
  }

  private bool IsSkillCanBreakByType(SkillNode node, SkillCategory category = SkillCategory.kNone) {
    if (node == null) {
      return true;
    }
    SkillScript skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null || !skill.IsActive()) {
      return true;
    }
    if (category == SkillCategory.kNone) {
      return skill.CanStopBackSwing();
    }
    if (node.Category == SkillCategory.kAttack && category == SkillCategory.kAttack) {
      return skill.CanStopBackSwing();
    }
    if (node.Category != SkillCategory.kAttack && category == SkillCategory.kAttack) {
      return skill.CanStop();
    }
    return skill.CanStop();
  }

  private SkillCategory GetSkillCategoryFromStr(string categoryStr) {
    if (categoryStr.Equals("SkillA")) {
      return SkillCategory.kSkillA;
    }
    if (categoryStr.Equals("SkillB")) {
      return SkillCategory.kSkillB;
    }
    if (categoryStr.Equals("SkillC")) {
      return SkillCategory.kSkillC;
    }
    if (categoryStr.Equals("SkillD")) {
      return SkillCategory.kSkillD;
    }
    if (categoryStr.Equals("SkillEx")) {
      return SkillCategory.kEx;
    }
    return SkillCategory.kNone;
  }
}
