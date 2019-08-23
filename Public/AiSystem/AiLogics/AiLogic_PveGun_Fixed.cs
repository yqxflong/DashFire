using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public class AiLogic_PveGun_Fixed : AbstractNpcStateLogic
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
      info.Time += deltaTime;
      if (info.Time > 100) {
        info.Time = 0;
        CharacterInfo target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
        if (null != target) {
          npc.GetMovementStateInfo().IsMoving = false;
          info.Time = 0;
          info.Target = target.GetId();
          ChangeToState(npc, (int)AiStateId.Combat);
          NotifyNpcTargetChange(npc);
        } else {
        }
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      float rps = npc.GetActualProperty().Rps;
      if (rps > 0.001f && info.Time > 1000 / rps) {
        info.Time = 0;
        bool toIdle = false;
        CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
        if (null != target && npc.SpatialSystem.CanShoot(npc.SpaceObject, target.GetMovementStateInfo().GetPosition3D())) {
          ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);
          float dist = (float)npc.GetActualProperty().AttackRange;
          float distView = (float)npc.ViewRange;
          if (powDist < dist * dist) {
            npc.GetMovementStateInfo().IsMoving = false;
          } else if (powDist < distView * distView) {
            npc.GetMovementStateInfo().IsMoving = false;
          } else {
            toIdle = true;
          }
        } else {
          toIdle = true;
        }
        if (toIdle) {
          info.Time = 0;
          npc.GetMovementStateInfo().IsMoving = false;
          ChangeToState(npc, (int)AiStateId.Idle);
        }
      }
    }
  }
}
