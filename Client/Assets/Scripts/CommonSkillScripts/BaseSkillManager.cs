using UnityEngine;
using System.Collections;

public class BaseSkillManager : MonoBehaviour {
  public virtual SkillControllerInterface GetSkillController() {
    return null;
  }

  public virtual bool IsUsingSkill() {
    return false;
  }

  public virtual bool IsControledMove() {
    return false;
  }
}
