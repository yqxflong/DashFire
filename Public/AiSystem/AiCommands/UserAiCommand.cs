using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class UserFaceCommand : AbstractUserAiCommand<UserFaceCommand>
  {
    public override bool Execute(long deltaTime)
    {
      if (0 == m_Count) {
        Logic.NotifyUserFace(User);
        ++m_Count;
        return false;
      } else {
        return true;
      }
    }
    public void Init()
    {
      m_Count = 0;
    }

    private long m_Count = 0;
  }
  public class UserSkillCommand : AbstractUserAiCommand<UserSkillCommand>
  {
    public override bool Execute(long deltaTime)
    {
      CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(User, m_TargetId);
      if (null != target) {
        Logic.ServerUserSkill(User, m_SkillId, target, m_TargetPos, m_TargetAngle, m_ItemId);
      }
      return true;
    }
    public void Init(int skillId, int targetId, ScriptRuntime.Vector3 targetPos, float targetAngle, int itemId)
    {
      m_SkillId = skillId;
      m_TargetId = targetId;
      m_TargetPos = targetPos;
      m_TargetAngle = targetAngle;
      m_ItemId = itemId;
    }
    
    private int m_SkillId = 0;
    private int m_TargetId = 0;
    private ScriptRuntime.Vector3 m_TargetPos;
    private float m_TargetAngle;
    private int m_ItemId = 0;
  }
}
