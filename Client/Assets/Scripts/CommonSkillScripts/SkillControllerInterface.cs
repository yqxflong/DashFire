using UnityEngine;
using System.Collections.Generic;

public enum SkillCategory {
  kNone,
  kAttack,
  kSkillA,
  kSkillB,
  kSkillC,
  kSkillD,
  kSkillQ,
  kSkillE,
  kRoll,
  kHold,
  kEx,
  kFlashAttack,
}

public class SkillInputData {
  public float castRange;
  public float targetChooseRange;
}

public interface SkillControllerInterface {
  void StartAttack();
  void StartAttack(Vector3 targetpos);
  void StopAttack();
  void ShowSkillTip(SkillCategory category, Vector3 targetpos);
  void HideSkillTip(SkillCategory category);
  void PushSkill(SkillCategory category, Vector3 targetpos);
  void RegisterSkillQECanInputHandler(SkillQECanInputHandler handler);
  void RegisterSkillStartHandler(SkillStartHandler handler);
  void AddBreakSkillTask();
  SkillInputData GetSkillInputData(SkillCategory category);
  bool IsCurSkillCanBreakByImpact();
  bool IsCategorySkillInCD(SkillCategory category);
  void ForceStopCurSkill();
  List<SkillNode> QuerySkillQE(SkillCategory category, int times);
}
