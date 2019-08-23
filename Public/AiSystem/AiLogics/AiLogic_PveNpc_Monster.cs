using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  class AiLogic_PveNpc_Monster : AbstractNpcStateLogic
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
      bool toIdle = false;
      AiData_PveNpc_Monster data = GetAiData(npc);
      if (null != data) {
        CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
        if (null != target) {
          float dist = (float)npc.GetActualProperty().AttackRange;
          float distGoHome = (float)npc.GohomeRange;
          Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);
          float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDist < dist * dist) {//进入攻击状态
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            info.Time = 0;
            data.Time = 0;
            ChangeToState(npc, (int)AiStateId.Combat);
          } else if (powDistToHome < distGoHome * distGoHome) {//追击
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
            toIdle = true;
          }
        } else {
          toIdle = true;
        }
        if (toIdle) {
          npc.GetMovementStateInfo().IsMoving = false;
          NotifyNpcMove(npc);
          info.Time = 0;
          ChangeToState(npc, (int)AiStateId.Idle);
        }
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if(info.Time>100){
        AiData_PveNpc_Monster data = GetAiData(npc);
        if (null != data) {
          data.Time += info.Time;
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
            float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            npc.GetMovementStateInfo().SetFaceDir(dir);
            if (powDist < data.ShootDistance * data.ShootDistance) {
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
              }
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              goHome = true;
            } else if (powDist < dist * dist /*&& SkillSystem.Instance.CanStartSkill(npc, data.Skill)*/) {
              ServerNpcSkill(npc, data.Skill, target, targetPos, dir);

              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              goHome = true;
            } else if (powDistToHome < distGoHome * distGoHome) {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
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
        }
      }
    }
    private void GoHomeHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
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
    private AiData_PveNpc_Monster GetAiData(NpcInfo npc)
    {
      AiData_PveNpc_Monster data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PveNpc_Monster>();
      if (null == data) {
        NpcAiStateInfo info = npc.GetAiStateInfo();
        data = new AiData_PveNpc_Monster();
        SkillInfo skillInfo = npc.GetSkillStateInfo().GetSkillInfoByIndex(0);
        if (null != skillInfo)
          data.Skill = skillInfo.SkillId;
        data.ShootDistance = int.Parse(info.AiParam[0]);
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
  }
}
