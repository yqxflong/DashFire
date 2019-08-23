using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  class AiLogic_PveNpc_Monster_CloseCombat : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Pursuit, this.PursuitHandler);
      SetStateHandler((int)AiStateId.Combat, this.CombatHandler);
      SetStateHandler((int)AiStateId.GoHome, this.GoHomeHandler);
      SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
      SetStateHandler((int)AiStateId.PatrolCommand, this.PatrolCommandHandler);
    }

    protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time = 0;
      npc.GetMovementStateInfo().IsMoving = false;
      info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
      info.Target = 0;
    }

    private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        info.Time = 0;
        npc.GetMovementStateInfo().IsMoving = false;
        ChangeToState(npc, (int)AiStateId.PatrolCommand);
      }
    }
    private void PursuitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead() || npc.GetSkillStateInfo().IsSkillActivated())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      bool goHome = false;
      AiData_PveNpc_Monster_CloseCombat data = GetAiData(npc);
      if (null != data) {
        CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
        if (null != target) {
          float dist = (float)npc.GetActualProperty().AttackRange;
          float distGoHome = (float)npc.GohomeRange;
          Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);
          float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDist < dist * dist) {//进入移动攻击状态
            if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
              ServerNpcStopImpact(npc, data.PreAttackImpact);
            }
            info.Time = 0;
            data.FoundPath.Clear();
            data.Time = (long)(1000.0f / npc.GetActualProperty().Rps);
            ChangeToState(npc, (int)AiStateId.Combat);
          } else if (powDist < data.PreAttackDistance * data.PreAttackDistance) { //预热状态
            if (null == npc.GetSkillStateInfo().GetImpactInfoById(data.FastMoveImpact)) {
              ServerNpcImpact(npc, data.FastMoveImpact, npc);
            }
            if (null == npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
              ServerNpcImpact(npc, data.PreAttackImpact, npc);
            }
            info.Time += deltaTime;
            if (info.Time > 100) {
              info.Time = 0;
              if (AiLogicUtility.GetWalkablePosition(target, npc, ref targetPos)) {
                AiLogicUtility.PathToTarget(npc, data.FoundPath, targetPos, 100, true, this);
              } else {
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
              }
            }
          } else if (powDistToHome < distGoHome * distGoHome) {//追击
            if (data.TestFlag != 3) {
              data.TestFlag = 3;
            }
            if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.FastMoveImpact)) {
              ServerNpcStopImpact(npc, data.FastMoveImpact);
            }
            if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
              ServerNpcStopImpact(npc, data.PreAttackImpact);
            }
            info.Time += deltaTime;
            if (info.Time > 100) {
              info.Time = 0;
              if (AiLogicUtility.GetWalkablePosition(target, npc, ref targetPos)) {
                AiLogicUtility.PathToTarget(npc, data.FoundPath, targetPos, 100, true, this);
              } else {
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
              }
            }
          } else {
            goHome = true;
          }
        } else {
          goHome = true;
        }
        if (goHome) {
          npc.GetMovementStateInfo().IsMoving = false;
          NotifyNpcMove(npc);

          info.Time = 0;
          ChangeToState(npc, (int)AiStateId.GoHome);
        }
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        AiData_PveNpc_Monster_CloseCombat data = GetAiData(npc);
        if (null != data) {
          data.Time += deltaTime;
          info.Time = 0;
          bool goHome = false;
          CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
          if (null != target) {
            float dist = (float)npc.GetActualProperty().AttackRange;
            float distGoHome = (float)npc.GohomeRange;
            ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDist = Geometry.DistanceSquare(srcPos, targetPos);
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
            if (powDist < data.StandShootDistance * data.StandShootDistance) {
              if (npc.GetMovementStateInfo().IsMoving) {
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
              }
              data.FoundPath.Clear();
              if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
                ServerNpcStopImpact(npc, data.PreAttackImpact);
              }
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
              }
            } else if (powDist < dist * dist) {
              if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
                ServerNpcStopImpact(npc, data.PreAttackImpact);
              }
              if (AiLogicUtility.GetWalkablePosition(target, npc, ref targetPos)) {
                AiLogicUtility.PathToTarget(npc, data.FoundPath, targetPos, 100, true, this);
              } else {
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
              }
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
              }
            } else if (powDistToHome < distGoHome * distGoHome) {
              if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
                ServerNpcStopImpact(npc, data.PreAttackImpact);
              }
              info.Time = 0;
              data.FoundPath.Clear();
              ChangeToState(npc, (int)AiStateId.Pursuit);
            } else {
              goHome = true;
            }
          } else {
            goHome = true;
          }
          if (goHome) {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);

            info.Time = 0;
            ChangeToState(npc, (int)AiStateId.GoHome);
          }
        } else {
          info.Time = 0;
        }
      }
    }

    private void GoHomeHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      AiData_PveNpc_Monster_CloseCombat data = GetAiData(npc);
      if (null != data) {
        if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.FastMoveImpact)) {
          ServerNpcStopImpact(npc, data.FastMoveImpact);
        }
        if (null != npc.GetSkillStateInfo().GetImpactInfoById(data.PreAttackImpact)) {
          ServerNpcStopImpact(npc, data.PreAttackImpact);
        }
      }
      ChangeToState(npc, (int)AiStateId.Idle);
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoMoveCommandState(npc, aiCmdDispatcher, deltaTime);
    }
    private void PatrolCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoPatrolCommandState(npc, aiCmdDispatcher, deltaTime, this);
    }
    private AiData_PveNpc_Monster_CloseCombat GetAiData(NpcInfo npc)
    {
      AiData_PveNpc_Monster_CloseCombat data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PveNpc_Monster_CloseCombat>();
      if (null == data) {
        NpcAiStateInfo info = npc.GetAiStateInfo();
        data = new AiData_PveNpc_Monster_CloseCombat();
          data.FastMoveImpact = int.Parse(info.AiParam[0]);
          data.PreAttackImpact = int.Parse(info.AiParam[1]);
          data.PreAttackDistance = int.Parse(info.AiParam[2]);
          data.StandShootDistance = int.Parse(info.AiParam[3]);
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
  }
}
