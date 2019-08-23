using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public class AiLogic_PvpNpc_Tower : AbstractNpcStateLogic
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
        CheckImpact(npc);
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
    //确定新的攻击目标：小兵优先
    private CharacterInfo GetNewTarget(NpcInfo npc)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      CharacterInfo npcTarget = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY, AiTargetType.NPC);
      if (null != npcTarget) {
        return npcTarget;
      } else {
        CharacterInfo userTarget = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY, AiTargetType.USER);
        if (null != userTarget) {
          return userTarget;
        }
      }
      return null;
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
        CheckImpact(npc);
        bool toIdle = false;
        //修改确定攻击目标的逻辑       
        CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);       
        if (null != target && npc.SpatialSystem.CanShoot(npc.SpaceObject, target.GetMovementStateInfo().GetPosition3D())) {
          ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDist = Geometry.DistanceSquare(srcPos, targetPos);   //npc与目标距离
          float dist = (float)npc.GetActualProperty().AttackRange;      //攻击范围
          //float distView = (float)npc.ViewRange;                      //视野范围
          if (powDist < dist * dist) {
            //在攻击范围内
            float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            npc.GetMovementStateInfo().IsMoving = false;
            npc.GetMovementStateInfo().SetFaceDir(dir);
            npc.GetMovementStateInfo().SetMoveDir(dir);
            if (npc.CanShoot()) {
              aiCmdDispatcher.NpcFace(npc, this);
            }
          } else {
            //如果原目标已经超出了攻击范围，防御塔应当重新进行探测，而不是跳转到Idle状态
            CharacterInfo newTarget = GetNewTarget(npc);
            if (null != newTarget) {
              npc.GetMovementStateInfo().IsMoving = false;
              info.Time = 0;
              info.Target = newTarget.GetId();
            } else {              
              toIdle = true;
            }
          }
        } else {
          //如果原目标死亡或不可攻击，防御塔应当首先重新进行探测，而不是跳转到Idle状态
          //若找到新目标，则进行攻击
          //若没有心目标。则进入Idle状态
          CharacterInfo newTarget = GetNewTarget(npc);
          if (null != newTarget) {
            npc.GetMovementStateInfo().IsMoving = false;
            info.Time = 0;
            info.Target = newTarget.GetId();
          } else {
            toIdle = true;
          }
        }
        if (toIdle) {
          info.Time = 0;
          npc.GetMovementStateInfo().IsMoving = false;
          ChangeToState(npc, (int)AiStateId.Idle);
        }
      }
    }
    private void CheckImpact(NpcInfo npc)
    {
      DashFireSpatial.ISpatialSystem spatialSys = npc.SpatialSystem;
      if (null != spatialSys) {
        bool existNpcInAttackRange = false;
        ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
        spatialSys.VisitObjectInCircle(srcPos, npc.GetActualProperty().AttackRange, (float distSqr, ISpaceObject obj) => {
          if (obj.GetObjType() == SpatialObjType.kNPC && (int)obj.GetID() != npc.GetId()) {
            NpcInfo npcObj = obj.RealObject as NpcInfo;
            if (null != npcObj && npcObj.NpcType != (int)NpcTypeEnum.PvpTower) {
              existNpcInAttackRange = true;
              return false;
            }
          }
          return true;
        });
        if (existNpcInAttackRange) {
          ImpactInfo impactInfo = npc.GetSkillStateInfo().GetImpactInfoById(c_ImpactForTower);
          if (null == impactInfo) {
            ServerNpcImpact(npc, c_ImpactForTower, npc);
          }
        }
      }
    }
    private const int c_ImpactForTower = 78;
  }
}
