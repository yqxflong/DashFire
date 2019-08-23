using System;
using System.Collections;
using System.Collections.Generic;

namespace DashFire {
  public sealed class AiViewManager {
    public void Init()
    {
      AbstractUserStateLogic.OnUserStartAttack += this.UserAttack;
      AbstractUserStateLogic.OnUserSkill += this.UserSkill;
      AbstractNpcStateLogic.OnNpcSkill += this.NpcSkill;
      AbstractNpcStateLogic.OnNpcFaceClient += this.OnNpcFace;
      AbstractNpcStateLogic.OnNpcMeetEnemy += this.OnNpcMeetEnemy;
      AbstractUserStateLogic.OnSkillPursuit += this.OnSkillPursuit;
      AbstractNpcStateLogic.OnSetNpcIdleAnim += this.OnSetNpcIdleAnim;
    }

    private void OnSkillPursuit(UserInfo user)
    {
      GfxSystem.PublishGfxEvent("Ai_InputSkillPursuitCmd", "Input", user.GetId());
    }

    private void UserAttack(UserInfo user, float x, float y, float z)
    {
      GfxSystem.PublishGfxEvent("Ai_InputAttackCmd", "Input", user.GetId(), x, y, z);
    }

    private void UserSkill(UserInfo user)
    {
      GfxSystem.PublishGfxEvent("Ai_InputSkillCmd", "Input", user.GetId());
    }

    private void OnNpcFace(NpcInfo npc, float faceDirection) {
      ControlSystemOperation.AdjustCharacterFace(npc.GetId(), faceDirection);
    }
    private void NpcSkill(NpcInfo npc, int skillId, CharacterInfo target) {
      if(null != npc){
        CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
        if(null != view) { 
          SkillParam param = new SkillParam();
          param.SkillId = skillId;
          param.TargetId = target.GetId();
          GfxSystem.SendMessage(view.Actor, "MonsterStartSkill", param);
        }
      }
    }

    private void OnNpcMeetEnemy(NpcInfo npc, Animation_Type animType) {
      CharacterView view = EntityManager.Instance.GetCharacterViewById(npc.GetId());
      if (null != view) {
        GfxSystem.SendMessage(view.Actor, "OnMeetEnemy", null);
      }
      view.PlayAnimation(animType);
      view.PlayQueuedAnimation(Animation_Type.AT_Stand);
    }

    private void OnSetNpcIdleAnim(NpcInfo npc, List<int> anims) {
      NpcView view = EntityManager.Instance.GetNpcViewById(npc.GetId());
      if (null != view) {
        List<Animation_Type> idleAnims = new List<Animation_Type>();
        foreach (int animId in anims) {
          idleAnims.Add((Animation_Type)animId);
        }
        view.SetIdleAnim(idleAnims);
      }
    }

    public static AiViewManager Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static AiViewManager s_Instance = new AiViewManager();
  }
}
