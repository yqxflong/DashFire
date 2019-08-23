using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  /// 
  /// @说明：
  ///   普通技能Npc AI逻辑
  ///   Npc只拥有一个技能，无普通攻击
  ///   
  ///   1、出生在配置点，进入Idle状态
  ///   2、当有敌人进入可视距离，进入追踪状态
  ///   3、追踪进入射程，根据RPS设定的频率，释放固有技能（SkillList[0]，其余技能配置无效）
  /// 
  public class AiLogic_PveNpc_OneSkill : AbstractNpcStateLogic
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
        CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
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
            ChangeToState(npc, (int)AiStateId.Combat);
            NotifyNpcMove(npc);
          } else if (powDistToHome < distGoHome * distGoHome) {
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
      float rps = npc.GetActualProperty().Rps;
      if (rps > 0.001f && info.Time > 1000 / rps) {
        info.Time = 0;
        AiData_PveNpc_General data = GetAiData(npc);
        if (null != data) {
          bool goHome = false;
          CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
          if (null != target) {
            float dist = (float)npc.GetActualProperty().AttackRange;
            float distGoHome = (float)npc.GohomeRange;
            ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDist = Geometry.DistanceSquare(srcPos, targetPos);
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
            if (powDist < dist * dist) {
              float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetFaceDir(dir);
              npc.GetMovementStateInfo().SetMoveDir(dir);
              if (npc.CanShoot() && !npc.GetSkillStateInfo().IsSkillActivated()) {
                Data_NpcConfig npcConfig = NpcConfigProvider.Instance.GetNpcConfigById(npc.GetLinkId());
                if (null == npcConfig) return;
                if (npcConfig.m_SkillList.Count >= 1) {
                  ServerNpcSkill(npc, npcConfig.m_SkillList[0], target, targetPos, dir);
                }
              }
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
      //临时屏蔽掉普通pve小兵的gohome
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
    private AiData_PveNpc_General GetAiData(NpcInfo npc)
    {
      AiData_PveNpc_General data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PveNpc_General>();
      if (null == data) {
        data = new AiData_PveNpc_General();
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
  }
}
