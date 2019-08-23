using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShooterControlHandler : SkillController {
  private ShooterSkillManager m_SkillManager;
  List<IShooterSkill> m_Skills;

  public ShooterControlHandler(ShooterSkillManager skillmanager, List<IShooterSkill> skills) {
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<string>("ge_cast_skill", "game", OnPushStrSkill);
    DashFire.LogicSystem.EventChannelForGfx.Subscribe<int>("ge_unlock_skill", "skill", base.UnlockSkill);
    m_SkillManager = skillmanager;
    Init(skills);
  }

  public void Init(List<IShooterSkill> skills) {
    m_Skills = skills;
    foreach (IShooterSkill ss in skills) {
      if (IsCategoryContain(ss.GetSkillId()) || !ss.IsDefaultCategory()) {
        continue;
      }
      SkillNode first_node = InitNodeByScript(skills, ss);
      m_SkillCategoryDict[ss.GetCategory()] = first_node;
    }
  }
  public override void OnTick() {
    UpdateAfterSkillAutoAttack();
    base.OnTick();
  }

  private void UpdateAfterSkillAutoAttack() {
    if (m_WaiteSkillBuffer.Count > 0) {
      return;
    }

    if (m_CurSkillNode != null) {
      if (Time.time > GetWaitInputTime(m_CurSkillNode)) {
        return;
      }
      IShooterSkill ss = m_SkillManager.GetSkillById(m_CurSkillNode.SkillId);
      SkillNode attackNode = m_SkillCategoryDict[SkillCategory.kAttack];
      if (ss != null && ss.IsAttackAfterSkill()
        && attackNode != null && IsSkillCanBreak(m_CurSkillNode, attackNode.Category) 
        && m_WaiteSkillBuffer.Count == 0) {
        Vector3 targetPos;
        if (TriggerImpl.GetCurTargetPos(m_SkillManager.GetOwner(), out targetPos)) {
          SkillInputData data = GetSkillInputData(SkillCategory.kAttack);
          if (data != null && Vector3.Magnitude(targetPos - m_SkillManager.GetOwner().transform.position) <= data.castRange) {
            StartAttack(targetPos);
          }
        }
      }
    }
  }

  public void OnPushStrSkill(string categoryStr) {
    SkillCategory category = GetSkillCategoryFromStr(categoryStr);
    if (category != SkillCategory.kNone) {
      base.PushSkill(category, Vector3.zero);
    }
  }

  public override void ShowSkillTip(SkillCategory category, Vector3 targetpos) {
    m_SkillManager.ShowSkillTip(targetpos);
  }

  public override void HideSkillTip(SkillCategory category) {
    m_SkillManager.CancelSkillTip();
  }

  public override void BeginSkillCD(SkillNode node) {
    if (node == null) {
      return;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return;
    }
    skill.BeginCD();
  }

  public override float GetSkillCD(SkillNode node) {
    if (node == null) {
      return 0;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return skill.GetCD();
  }

  public override void StartAttack(Vector3 targetpos) {
    //IShooterSkill curSkill = m_SkillManager.GetCurPlaySkill();
    //if (curSkill != null && curSkill.IsExecuting() && !curSkill.CanBreak() && curSkill.GetCategory() != SkillCategory.kAttack) {
    //  return;
    //}
    //m_WaiteSkillBuffer.Clear();

    CancelBreakSkillTask();
    SetFaceDir(targetpos);
#if SHOOTER_LOG
    Debug.Log("--sa: start pos attack " + targetpos);
#endif
    //TODO:避免重复操作
    UpdateSkillByCategory(SkillCategory.kAttack, m_Skills);

    SkillNode node = AddAttackNode();
    if (node != null) {
      node.TargetPos = targetpos;
    } else {
      Debug.Log("--sa: have't create attack node! ");
    }
    m_IsAttacking = true;
  }
  public override void PushSkill(SkillCategory category, Vector3 targetpos) {
    CancelBreakSkillTask();
    if (CanInput()) {
      if (IsCategorySkillInCD(category)) {
        HideSkillTip(category);
        DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
        return;
      }

      //TODO:避免重复操作
      int categoryId = (int)category;
      UpdateSkillByCategory(category, m_Skills);

      SkillNode target_node = AddCategorySkillNode(category);
      if (target_node != null) {
        target_node.TargetPos = targetpos;
        if (SkillCategory.kSkillQ != category && SkillCategory.kSkillE != category) {
          List<SkillNode> qeSkills = new List<SkillNode>();
          if (target_node.SkillQ != null) {
            qeSkills.Add(target_node.SkillQ);
          }
          if (target_node.SkillE != null) {
            qeSkills.Add(target_node.SkillE);
          }
          if (m_SkillQECanInputHandler != null) {
            m_SkillQECanInputHandler(GetWaitInputTime(m_CurSkillNode), qeSkills);
          }
        }
      } else {
        DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
      }
    }
  }
  public override float GetWaitInputTime(SkillNode node) {
    if (node == null) {
      return 0;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return node.StartTime + skill.GetNextInputTime();
  }

  public override float GetLockInputTime(SkillNode node) {
    if (node == null) {
      return 0;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return 0;
    }
    return node.StartTime + skill.GetLockInputTime();
  }

  public override bool IsSkillCanBreak(SkillNode node, SkillCategory category) {
    if (m_CurSkillNode == null) {
      return true;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null || !skill.IsExecuting()) {
      return true;
    }
    if (category != SkillCategory.kNone) {
      if (m_CurSkillNode.Category == SkillCategory.kAttack
        && category == SkillCategory.kAttack
        && m_CurSkillNode.NextSkillNode != null) {
        return false;
      } else if (m_CurSkillNode.Category != SkillCategory.kAttack
        && category == SkillCategory.kAttack) {
        return skill.CanBreak();
      } else {
        return skill.CanBreakBySkill();
      }
    }
    return skill.CanBreak();
  }

  public override bool IsSkillCanStart(SkillNode node) {
    if (node == null) {
      return false;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return false;
    }
    return skill.CanStart();
  }

  public override bool IsSkillInCD(SkillNode node) {
    if (node == null) {
      return false;
    }
    IShooterSkill skill = m_SkillManager.GetSkillById(node.SkillId);
    if (skill == null) {
      return false;
    }
    return skill.IsInCD();
  }

  public override void SetFaceDir(Vector3 targetpos) {
    m_SkillManager.SetFacePos(targetpos);
  }

  public override SkillInputData GetSkillInputData(SkillCategory category) {
    SkillNode head = null;
    if (m_SkillCategoryDict.TryGetValue(category, out head)) {
      IShooterSkill ss = m_SkillManager.GetSkillById(head.SkillId);
      if (ss != null) {
        SkillInputData data = new SkillInputData();
        data.castRange = ss.GetSkillCastRange();
        data.targetChooseRange = ss.GetTargetChooseRange();
        return data;
      }
    }
    return null;
  }

  public override bool StartSkill(SkillNode node) {
#if SHOOTER_LOG
    Debug.Log("--nc: start skill " + node.SkillId);
#endif
    if (m_SkillManager.StartSkillById(node.SkillId, node.TargetPos)) {
      //if (node.TargetPos != Vector3.zero) {
      //  m_SkillManager.SetFacePos(node.TargetPos);
      //}
      return true;
    }
    return false;
  }

  // child should override this function
  public override bool StopSkill(SkillNode node) {
    IShooterSkill ss = m_SkillManager.GetCurPlaySkill();
    if (ss != null) {
      ss.StopSkill();
    }
    return true;
  }

  private SkillNode InitNodeByScript(List<IShooterSkill> skills, IShooterSkill ss) {
    SkillNode first = new SkillNode();
    first.SkillId = ss.GetSkillId();
    first.Category = ss.GetCategory();
    //FIXME:临时解锁全部技能，by lixiaojiang
    first.IsLocked = false;
    IShooterSkill nextSkillScript = GetSkillById(skills, ss.GetNextSkillId());
    if (nextSkillScript != null) {
      first.NextSkillNode = InitNodeByScript(skills, nextSkillScript);
    }
    IShooterSkill qSkillScript = GetSkillById(skills, ss.GetQSkillId());
    if (qSkillScript != null) {
      first.SkillQ = InitNodeByScript(skills, qSkillScript);
    }
    IShooterSkill eSkillScript = GetSkillById(skills, ss.GetESkillId());
    if (eSkillScript != null) {
      first.SkillE = InitNodeByScript(skills, eSkillScript);
    }
    return first;
  }

  private IShooterSkill GetSkillById(List<IShooterSkill> skills, int id) {
    foreach (IShooterSkill ss in skills) {
      if (ss.GetSkillId() == id && ss.GetSkillId() != (int)ShooterSkillId.SkillLogic_None) {
        return ss;
      }
    }
    return null;
  }

  private bool IsCategoryContain(int skillid) {
    foreach (SkillNode head in m_SkillCategoryDict.Values) {
      if (FindSkillNodeInChildren(head, skillid)) {
        return true;
      }
    }
    return false;
  }

  private bool FindSkillNodeInChildren(SkillNode head, int target_id) {
    if (head.SkillId == target_id) {
      return true;
    }
    if (head.NextSkillNode != null) {
      if (FindSkillNodeInChildren(head.NextSkillNode, target_id)) {
        return true;
      }
    }
    if (head.SkillQ != null) {
      if (FindSkillNodeInChildren(head.SkillQ, target_id)) {
        return true;
      }
    }
    if (head.SkillE != null) {
      if (FindSkillNodeInChildren(head.SkillE, target_id)) {
        return true;
      }
    }
    return false;
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
    if (categoryStr.Equals("SkillHold")) {
      return SkillCategory.kHold;
    }
    if (categoryStr.Equals("SkillEx")) {
      return SkillCategory.kEx;
    }
    return SkillCategory.kNone;
  }

  public void ChangeSkillByCategory(SkillCategory category, List<IShooterSkill> skills, int skillId) {
    IShooterSkill targetSkill = GetSkillById(skills, skillId);
    if (targetSkill == null) {
      return;
    }
    if (IsCategoryContain(targetSkill.GetSkillId())) {
      m_SkillCategoryDict.Remove(targetSkill.GetCategory());
    }
    SkillNode first_node = InitNodeByScript(skills, targetSkill);
    m_SkillCategoryDict[targetSkill.GetCategory()] = first_node;
  }
  public void UpdateNextSkillByCategory(IShooterSkill categorySkill, List<IShooterSkill> skills, int curWeaponId) {
    if (categorySkill != null) {
      //Update next skill
      List<ShooterSkillId> nextSkillIdList = categorySkill.GetNextSkillIdList();
      if (nextSkillIdList != null && nextSkillIdList.Count > 0) {
        foreach (ShooterSkillId skillId in nextSkillIdList) {
          IShooterSkill tSkill = GetSkillById(skills, (int)skillId);
          if (tSkill != null && tSkill.GetWeaponId() == curWeaponId) {
            categorySkill.SetNextSkillId((int)skillId);
            break;
          }
        }
      }
    }
  }
  public void UpdateSkillByCategory(SkillCategory category, List<IShooterSkill> skills) {
    //Update Category
    int curWeaponId = TriggerImpl.GetCurWeaponId(m_SkillManager.GetOwner());
    if (curWeaponId >= 0) {
      List<IShooterSkill> allCategorySkills = GetSkillsByCategory(category, skills);
      List<IShooterSkill> categorySkills = GetSkillsByWeaponId(curWeaponId, allCategorySkills);
      IShooterSkill categorySkill = null;
      if (categorySkills.Count > 0) {
        categorySkill = categorySkills[0];
      } else {
        SkillNode firstNode = null;
        if (m_SkillCategoryDict.TryGetValue(category, out firstNode)) {
          categorySkill = GetSkillById(skills, firstNode.SkillId);
        }
      }
      if (categorySkill != null) {
        UpdateNextSkillByCategory(categorySkill, skills, curWeaponId);
        ChangeSkillByCategory(category, skills, categorySkill.GetSkillId());
      }
    }
  }
  public void ResetSkillByCategory(SkillCategory category, List<IShooterSkill> skills) {
    //Update Category
    int curWeaponId = TriggerImpl.GetCurWeaponId(m_SkillManager.GetOwner());
    if (curWeaponId >= 0) {
      List<IShooterSkill> allCategorySkills = GetSkillsByCategory(category, skills);
      List<IShooterSkill> categorySkills = GetSkillsByWeaponId(curWeaponId, allCategorySkills);
      IShooterSkill categorySkill = null;
      if (categorySkills.Count > 0) {
        categorySkill = categorySkills[0];
      } else {
        SkillNode firstNode = null;
        if (m_SkillCategoryDict.TryGetValue(category, out firstNode)) {
          categorySkill = GetSkillById(skills, firstNode.SkillId);
        }
      }
      if (categorySkill != null) {
        UpdateNextSkillByCategory(categorySkill, skills, curWeaponId);
        ChangeSkillByCategory(category, skills, categorySkill.GetSkillId());
      }
    }
  }
  private List<IShooterSkill> GetSkillsByCategory(SkillCategory category, List<IShooterSkill> skills) {
    List<IShooterSkill> retSkill = new List<IShooterSkill>();
    foreach (IShooterSkill skill in skills) {
      if (skill.GetCategory() == category) {
        retSkill.Add(skill);
      }
    }
    return retSkill;
  }
  private List<IShooterSkill> GetSkillsByWeaponId(int weaponId, List<IShooterSkill> skills) {
    List<IShooterSkill> retSkill = new List<IShooterSkill>();
    foreach (IShooterSkill skill in skills) {
      if (skill.GetWeaponId() == weaponId) {
        retSkill.Add(skill);
      }
    }
    return retSkill;
  }
  public void ForceStartSkillById(SkillCategory category, Vector3 targetpos) {
    if (m_CurSkillNode != null) {
      StopSkill(m_CurSkillNode);
    }
    CancelBreakSkillTask();
    UpdateSkillByCategory(category, m_Skills);

    SkillNode target_node = AddCategorySkillNode(category);
    if (target_node != null) {
      target_node.TargetPos = targetpos;
    } else {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
    }
  }
  protected override void DealBreakSkillTask() {
    if (m_IsHaveBreakSkillTask) {
      TriggerImpl.ResetTarget(m_SkillManager.GetOwner());
      if (m_CurSkillNode == null || IsSkillCanBreak(m_CurSkillNode)) {
        if (m_CurSkillNode != null) {
          StopSkill(m_CurSkillNode);
        }
        m_IsHaveBreakSkillTask = false;
        m_IsAttacking = false;
        if (m_WaiteSkillBuffer.Count > 0) {
          HideSkillTip(SkillCategory.kNone);
        }
        m_WaiteSkillBuffer.Clear();
      }
    }
  }
}
