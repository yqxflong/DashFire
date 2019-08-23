using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  class AiLogic_PveNpc_General : AbstractNpcStateLogic
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
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      bool goHome = false;
      AiData_PveNpc_General data = GetAiData(npc);
      if (null != data) {
        CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
        if (null != target) {
          float dist = (float)npc.GetActualProperty().AttackRange;
          float distGoHome = (float)npc.GohomeRange;
          Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);
          float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDist < dist * dist) {
            npc.GetMovementStateInfo().IsMoving = false;
            info.Time = 0;
            data.Time = 0;
            ChangeToState(npc, (int)AiStateId.Combat);
            NotifyNpcMove(npc);
          } else if (true) { // 目标存活的情况下屏蔽掉gohome。
            info.Time += deltaTime;
            if (info.Time > m_IntervalTime) {
              info.Time = 0;
              AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, m_IntervalTime, true, this);
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
          data.Time = 0;
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
      if (info.Time > m_IntervalTime) {
        AiData_PveNpc_General data = GetAiData(npc);
        if (null != data) {
          data.Time += info.Time;
          info.Time = 0;
          bool goHome = false;
          CharacterInfo target = AiLogicUtility.GetLivingCharacterInfoHelper(npc, info.Target);
          //CharacterInfo target = AiLogicUtility.GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY, AiTargetType.USER);
          if (null != target) {
            float dist = (float)npc.GetActualProperty().AttackRange;
            float distGoHome = (float)npc.GohomeRange;
            ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDist = Geometry.DistanceSquare(srcPos, targetPos);
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
            if (powDist < dist * dist) {
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
                float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
                NotifyNpcFace(npc, dir);
                //npc.GetMovementStateInfo().SetFaceDir(dir);
                npc.GetMovementStateInfo().SetMoveDir(dir);
                if (null != OnNpcSkill) {
                  OnNpcSkill(npc, 10001, target);
                }
              }
            } else if (true) {
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
        } else {
          info.Time = 0;
        }
      }
    }
    private void GoHomeHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      //临时屏蔽掉普通pve小兵的gohome
      if (npc.IsDead())
        return;
      ChangeToState(npc, (int)AiStateId.Idle);
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > m_IntervalTime) {
        info.Time = 0;
        AiData_PveNpc_General data = GetAiData(npc);
        if (null != data) {
          Vector3 targetPos = info.HomePos;
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDistToHome <= 1) {
            npc.GetMovementStateInfo().IsMoving = false;
            info.Time = 0;
            ChangeToState(npc, (int)AiStateId.Idle);
          } else {
            AiLogicUtility.PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, 100, true, this);
          }
        }
      }
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoMoveCommandState(npc, aiCmdDispatcher, deltaTime);
    }
    private void PatrolCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoPatrolCommandState(npc, aiCmdDispatcher, deltaTime, this);
    }
    private AiData_PveNpc_General GetAiData(NpcInfo npc)
    {
      AiData_PveNpc_General data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PveNpc_General>();
      if (null == data) {
        data = new AiData_PveNpc_General();
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
    private const long m_IntervalTime = 100;
  }
}
