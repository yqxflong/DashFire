using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  class AiLogic_PveNpc_Trap : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Combat, this.CombatHandler);
    }

    protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
      info.Time = 0;
      info.Target = 0;
      npc.GetMovementStateInfo().IsMoving = false;
    }

    private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      AiData_PveNpc_Trap data = GetAiData(npc);
      if (null != data) {
        info.Time += deltaTime;
        if (info.Time > 100) {
          info.Time = 0;
          if (data.HideImpact > 0) {
            ImpactInfo impactInfo = npc.GetSkillStateInfo().GetImpactInfoById(data.HideImpact);
            if (null == impactInfo) {
              ServerNpcImpact(npc, data.HideImpact, npc);
            }
          }
          IList<ISpaceObject> objs = npc.SpatialSystem.GetObjectInCircle(npc.GetMovementStateInfo().GetPosition3D(), data.RadiusOfTrigger);
          foreach (ISpaceObject obj in objs) {
            if (obj.GetObjType() == SpatialObjType.kNPC || obj.GetObjType() == SpatialObjType.kUser) {
              CharacterInfo charInfo = obj.RealObject as CharacterInfo;
              if (null != charInfo) {
                if (CharacterInfo.GetRelation(npc, charInfo) == CharacterRelation.RELATION_ENEMY) {
                  info.Time = 0;
                  info.Target = charInfo.GetId();
                  ChangeToState(npc,(int)AiStateId.Combat);
                  NotifyNpcTargetChange(npc);
                  break;
                }
              }
            }
          }
        }
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      AiData_PveNpc_Trap data = GetAiData(npc);
      if (null != data && !data.IsTriggered) {
        CharacterInfo source = npc.SceneContext.GetCharacterInfoById(npc.CreatorId);
        if (null != source) {
          if (data.ImpactToMyself > 0)
            ServerNpcImpact(source, data.ImpactToMyself, npc);
          CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
          if (null != target) {
            if(data.Impact1ToTarget>0)
              ServerNpcImpact(source, data.Impact1ToTarget, target);
            if(data.Impact2ToTarget>0)
              ServerNpcImpact(source, data.Impact2ToTarget, target);
          }
          int ct = 1;
          IList<ISpaceObject> objs = npc.SpatialSystem.GetObjectInCircle(npc.GetMovementStateInfo().GetPosition3D(), data.RadiusOfTrigger);
          foreach (ISpaceObject obj in objs) {
            if (obj.GetObjType() == SpatialObjType.kNPC || obj.GetObjType() == SpatialObjType.kUser) {
              CharacterInfo charInfo = obj.RealObject as CharacterInfo;
              if (null != charInfo) {
                if (CharacterInfo.GetRelation(npc, charInfo) == CharacterRelation.RELATION_ENEMY) {
                  if (ct < data.DamageCount) {
                    if(data.Impact1ToTarget>0)
                      ServerNpcImpact(source, data.Impact1ToTarget, charInfo);
                    if(data.Impact2ToTarget>0)
                      ServerNpcImpact(source, data.Impact2ToTarget, charInfo);
                    ++ct;
                  }
                }
              }
            }
          }
        }
        data.IsTriggered = true;
      }
    }
    private AiData_PveNpc_Trap GetAiData(NpcInfo npc)
    {
      AiData_PveNpc_Trap data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PveNpc_Trap>();
      if (null == data) {
        NpcAiStateInfo info = npc.GetAiStateInfo();
        data = new AiData_PveNpc_Trap();
        data.RadiusOfTrigger = int.Parse(info.AiParam[0]);
        data.RadiusOfDamage = int.Parse(info.AiParam[1]);
        data.DamageCount = int.Parse(info.AiParam[2]);
        data.ImpactToMyself = int.Parse(info.AiParam[3]);
        data.Impact1ToTarget = int.Parse(info.AiParam[4]);
        data.Impact2ToTarget = int.Parse(info.AiParam[5]);
        data.HideImpact = int.Parse(info.AiParam[6]);
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
  }
}
