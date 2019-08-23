using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SkillNode {
  public int SkillId;
  public SkillCategory Category;
  public SkillNode SkillQ = null;
  public SkillNode SkillE = null;
  public SkillNode NextSkillNode = null;
  public float StartTime;
  public Vector3 TargetPos;
  public bool IsLocked = false;
  public bool IsCDChecked = false;
}

public delegate void SkillQECanInputHandler(float remaintime, List<SkillNode> skills);
public delegate void SkillStartHandler();
public class SkillController : SkillControllerInterface {
  public static List<int> m_UnlockedSkills = new List<int>();
  protected Dictionary<SkillCategory, SkillNode> m_SkillCategoryDict = new Dictionary<SkillCategory, SkillNode>();
  protected bool m_IsAttacking = false;
  protected List<SkillNode> m_WaiteSkillBuffer = new List<SkillNode>();
  protected SkillNode m_LastSkillNode = null;
  protected SkillNode m_CurSkillNode = null;
  protected SkillQECanInputHandler m_SkillQECanInputHandler = null;
  protected SkillStartHandler m_SkillStartHandler = null;
  protected bool m_IsHaveBreakSkillTask = false;

  public virtual void Init() {
  }

  public virtual void OnTick() {
    UpdateAttacking();
    DealBreakSkillTask();
    UpdateSkillNodeCD();
    if (m_WaiteSkillBuffer.Count <= 0) {
      return;
    }
    SkillNode node = m_WaiteSkillBuffer[m_WaiteSkillBuffer.Count -1];
    if (node == null) {
      m_WaiteSkillBuffer.Remove(node);
      return;
    }
    SkillNode nextNode = null;
    if ((node.Category == SkillCategory.kSkillQ || node.Category == SkillCategory.kSkillE)
        && m_WaiteSkillBuffer.Count >= 2) {
      SkillNode lastone = m_WaiteSkillBuffer[m_WaiteSkillBuffer.Count -2];
      if (node.Category == SkillCategory.kSkillQ && lastone.SkillQ != null
          && lastone.SkillQ.SkillId == node.SkillId) {
        nextNode = node;
        node = lastone;
      } else if (node.Category == SkillCategory.kSkillE && lastone.SkillE != null
                 && lastone.SkillE.SkillId == node.SkillId) {
        nextNode = node;
        node = lastone;
      }
    }
    if (m_CurSkillNode == null || IsSkillCanBreak(m_CurSkillNode, node.Category)) {
      if (!IsSkillCanStart(node)) {
        return;
      }
      if (m_CurSkillNode != null) {
        StopSkill(m_CurSkillNode);
      }
      if (StartSkill(node)) {
        OnSkillStart(node);
        if (nextNode != null) {
          m_WaiteSkillBuffer.Add(nextNode);
        }
        PostSkillStart(node);
      }
    }
  }

  public void UnlockSkill(int skillid) {
    SkillNode node = GetSkillNodeById(skillid);
    if (node != null) {
      string name = GetCategoryName(node.Category);
      if (!string.IsNullOrEmpty(name)) {
        DashFire.LogicSystem.EventChannelForGfx.Publish("ge_unlock_skill", "ui", name);
      }
      node.IsLocked = false;
      m_UnlockedSkills.Add(skillid);
    }
  }

  public virtual void RegisterSkillQECanInputHandler(SkillQECanInputHandler handler) {
    m_SkillQECanInputHandler = handler;
  }

  public virtual void RegisterSkillStartHandler(SkillStartHandler handler)
  {
    m_SkillStartHandler = handler;
  }

  public virtual void PostSkillStart(SkillNode node) {
  }

  public virtual void AddBreakSkillTask() {
    m_IsHaveBreakSkillTask = true;
  }

  public virtual SkillInputData GetSkillInputData(SkillCategory category) {
    return null;
  }

  public void CancelBreakSkillTask() {
    m_IsHaveBreakSkillTask = false;
  }

  public virtual void BeginSkillCD(SkillNode node) {
  }

  public virtual void SetFaceDir(Vector3 targetpos) {
  }

  public virtual void StartAttack() {
    CancelBreakSkillTask();
    SkillNode node = AddAttackNode();
    if (node != null) {
      node.TargetPos = Vector3.zero;
    }
    m_IsAttacking = true;
  }

  public virtual void StartAttack(Vector3 targetpos) {
    CancelBreakSkillTask();
    SetFaceDir(targetpos);
    SkillNode node = AddAttackNode();
    if (node != null) {
      node.TargetPos = targetpos;
    } else {
      Debug.Log("--sa: have't create attack node! ");
    }
    m_IsAttacking = true;
  }

  protected SkillNode AddAttackNode() {
    SkillNode node = null;
    if (CanInput()) {
      node = AddCategorySkillNode(SkillCategory.kAttack);
    }
    return node;
  }

  public virtual void StopAttack() {
    m_IsAttacking = false;
  }

  // child should override this function
  public virtual void ShowSkillTip(SkillCategory category, Vector3 targetpos) {
  }

  // child should override this function
  public virtual void HideSkillTip(SkillCategory category) {
  }

  public virtual void PushSkill(SkillCategory category, Vector3 targetpos) {
    CancelBreakSkillTask();
    if (CanInput()) {
      if (IsCategorySkillInCD(category)) {
        HideSkillTip(category);
        DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
        return;
      }
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
        HideSkillTip(category);
        if (category != SkillCategory.kSkillQ && category != SkillCategory.kSkillE) {
          DashFire.LogicSystem.EventChannelForGfx.Publish("ge_skill_false", "ui");
        }
      }
    }
  }

  public SkillNode GetHead(SkillCategory category) {
    SkillNode target = null;
    m_SkillCategoryDict.TryGetValue(category, out target);
    return target;
  }

  public virtual List<SkillNode> QuerySkillQE(SkillCategory category, int times) {
    List<SkillNode> result = new List<SkillNode>();
    SkillNode node = QuerySkillNode(category, times);
    if (node != null) {
      if (node.SkillQ != null) {
        result.Add(node.SkillQ);
      }
      if (node.SkillE != null) {
        result.Add(node.SkillE);
      }
    }
    return result;
  }

  public SkillNode QuerySkillNode(SkillCategory category, int times) {
    if (times <= 0) {
      times = 1;
    }
    int index = 1;
    SkillNode head = null;
    if (!m_SkillCategoryDict.TryGetValue(category, out head)) {
      return null;
    }
    SkillNode result = head;
    while (index < times) {
      result = result.NextSkillNode;
      if (result == null) {
        result = head;
      }
      index++;
    }
    return result;
  }

  // child should override this function
  public virtual float GetWaitInputTime(SkillNode node) {
    if (node == null) {
      return 0;
    }
    return node.StartTime + 5;
  }

  // child should override this function
  public virtual float GetLockInputTime(SkillNode node) {
    return 0;
  }

  public virtual float GetSkillCD(SkillNode node) {
    return 3;
  }

  public bool IsCurSkillCanBreakByImpact() {
    return IsSkillCanBreakByImpact(m_CurSkillNode);
  }

  public virtual bool IsSkillCanBreakByImpact(SkillNode node) {
    return IsSkillCanBreak(node);
  }
  // child should override this function
  public virtual bool IsSkillCanBreak(SkillNode node, SkillCategory nextcategory = SkillCategory.kNone) {
    if (m_CurSkillNode == null) {
      return true;
    }
    return false;
  }

  // child should override this function
  public virtual bool IsSkillCanStart(SkillNode node) {
    return true;
  }

  public virtual bool IsCategorySkillInCD(SkillCategory category) {
    return IsSkillInCD(GetHead(category));
  }

  public virtual bool IsSkillInCD(SkillNode node) {
    return false;
  }

  // child should override this function
  public virtual bool StartSkill(SkillNode node) {
    return true;
  }

  // child should override this function
  public virtual bool StopSkill(SkillNode node) {
    return true;
  }

  public virtual void ForceStopCurSkill() {
  }

  protected SkillNode AddCategorySkillNode(SkillCategory category) {
    switch(category) {
    case SkillCategory.kSkillQ:
    case SkillCategory.kSkillE:
      return AddQESkillNode(category);
    default:
      return AddNextBasicSkill(category);
    }
  }

  protected SkillNode GetSkillNodeById(int skillid) {
    SkillNode result = null;
    foreach(SkillNode head in m_SkillCategoryDict.Values) {
      result = FindSkillNodeInChildren(head, skillid);
      if (result != null) {
        return result;
      }
    }
    return result;
  }

  protected SkillNode FindSkillNodeInChildren(SkillNode head, int target_id) {
    if (head.SkillId == target_id) {
      return head;
    }
    if (head.NextSkillNode != null) {
      SkillNode node = FindSkillNodeInChildren(head.NextSkillNode, target_id);
      if (node != null) {
        return node;
      }
    }
    if (head.SkillQ != null) {
      SkillNode node = FindSkillNodeInChildren(head.SkillQ, target_id);
      if (node != null) {
        return node;
      }
    }
    if (head.SkillE != null) {
      SkillNode node = FindSkillNodeInChildren(head.SkillE, target_id);
      if (node != null) {
        return node;
      }
    }
    return null;
  }

  private SkillNode AddNextBasicSkill(SkillCategory category) {
    float now = Time.time;
    if (m_CurSkillNode != null && m_CurSkillNode.Category == category) {
      if (m_CurSkillNode.NextSkillNode != null && !m_CurSkillNode.NextSkillNode.IsLocked &&
          now < GetWaitInputTime(m_CurSkillNode)) {
        m_WaiteSkillBuffer.Add(m_CurSkillNode.NextSkillNode);
        return m_CurSkillNode.NextSkillNode;
      }
    }
    SkillNode firstNode = null;
    if (m_SkillCategoryDict.TryGetValue(category, out firstNode)) {
      if (!firstNode.IsLocked) {
        m_WaiteSkillBuffer.Add(firstNode);
        return firstNode;
      }
    }
    return null;
  }

  private SkillNode AddQESkillNode(SkillCategory category) {
    float now = Time.time;
    if (m_CurSkillNode == null) {
      return null;
    }
    SkillNode parent = m_CurSkillNode;
    bool isHaveWaiteNode = false;
    if (m_WaiteSkillBuffer.Count > 0) {
      parent = m_WaiteSkillBuffer[m_WaiteSkillBuffer.Count -1];
      isHaveWaiteNode = true;
    }
    if (parent == null) {
      return null;
    }
    if (isHaveWaiteNode || now < GetWaitInputTime(m_CurSkillNode)) {
      SkillNode target = null;
      if (category == SkillCategory.kSkillQ) {
        target = parent.SkillQ;
      }
      if (category == SkillCategory.kSkillE) {
        target = parent.SkillE;
      }
      if (target != null && !target.IsLocked) {
        m_WaiteSkillBuffer.Add(target);
        return target;
      }
    }
    return null;
  }

  protected bool CanInput() {
    float now = Time.time;
    if (now < GetLockInputTime(m_CurSkillNode)) {
      return false;
    }
    return true;
  }

  private void OnSkillStart(SkillNode node) {
    HideSkillTip(SkillCategory.kNone);
    m_LastSkillNode = m_CurSkillNode;
    m_CurSkillNode = node;
    m_CurSkillNode.StartTime = Time.time;
    m_CurSkillNode.IsCDChecked = false;

    m_WaiteSkillBuffer.RemoveAt(m_WaiteSkillBuffer.Count - 1);
    List<SkillNode> new_buffer_element = new List<SkillNode>();
    new_buffer_element.AddRange(m_WaiteSkillBuffer);
    m_WaiteSkillBuffer.Clear();

    if (m_CurSkillNode.NextSkillNode != null) {
      while (new_buffer_element.Count >= 1) {
        SkillNode last = new_buffer_element[new_buffer_element.Count-1];
        if (m_CurSkillNode != null && last != null &&
            last.Category == m_CurSkillNode.Category && last.Category != SkillCategory.kAttack) {
          PushSkill(last.Category, Vector3.zero);
          new_buffer_element.RemoveAt(new_buffer_element.Count - 1);
        } else {
          break;
        }
      }
    }

    string categoryname = GetCategoryName(m_CurSkillNode.Category);
    if (m_CurSkillNode.NextSkillNode != null && !string.IsNullOrEmpty(categoryname)) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_cast_skill", "ui", categoryname);
    }
    if (m_LastSkillNode != null && m_LastSkillNode.Category != SkillCategory.kAttack &&
        m_LastSkillNode.Category != m_CurSkillNode.Category) {
      if (!m_LastSkillNode.IsCDChecked) {
        BeginSkillCategoryCD(m_LastSkillNode.Category);
        m_LastSkillNode.IsCDChecked = true;
      }
    }

    if (null != m_SkillStartHandler) {
      m_SkillStartHandler();
    }
  }

  private void UpdateSkillNodeCD() {
    if (m_CurSkillNode != null && !m_CurSkillNode.IsCDChecked) {
      if (m_CurSkillNode.NextSkillNode == null) {
        BeginSkillCategoryCD(m_CurSkillNode.Category);
        m_CurSkillNode.IsCDChecked = true;
      } else if (GetWaitInputTime(m_CurSkillNode) < Time.time) {
        BeginSkillCategoryCD(m_CurSkillNode.Category);
        m_CurSkillNode.IsCDChecked = true;
      }
    }
  }

  public virtual void BeginCurSkillCD() {
  }

  protected void BeginSkillCategoryCD(SkillCategory category) {
    SkillNode head = null;
    if (m_SkillCategoryDict.TryGetValue(category, out head)) {
      DashFire.LogicSystem.EventChannelForGfx.Publish("ge_cast_skill_cd", "ui",
                                                      GetCategoryName(head.Category),
                                                      GetSkillCD(head));
      BeginSkillCD(head);
    }
  }

  private string GetCategoryName(SkillCategory category) {
    switch (category) {
    case SkillCategory.kSkillA:
      return "SkillA";
    case SkillCategory.kSkillB:
      return "SkillB";
    case SkillCategory.kSkillC:
      return "SkillC";
    case SkillCategory.kSkillD:
      return "SkillD";
    default:
      return "";
    }
  }

  private void UpdateAttacking() {
    if (m_IsAttacking) {
      if (m_WaiteSkillBuffer.Count <= 0 && CanInput() && IsSkillCanBreak(m_CurSkillNode, SkillCategory.kAttack)) {
        CancelBreakSkillTask();
        SkillNode nextAttackNode = AddAttackNode();
        if (nextAttackNode != null) {
          nextAttackNode.TargetPos = Vector3.zero;
        }
      }
    }
  }

  protected virtual void DealBreakSkillTask() {
    if (m_IsHaveBreakSkillTask) {
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
