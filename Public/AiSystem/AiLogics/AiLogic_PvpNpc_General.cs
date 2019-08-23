using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  class AiLogic_PvpNpc_General : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Patrol, this.PatrolHandler);
      SetStateHandler((int)AiStateId.Pursuit, this.PursuitHandler);
      SetStateHandler((int)AiStateId.Combat, this.CombatHandler);
      SetStateHandler((int)AiStateId.GoHome, this.GoHomeHandler);
    }

    protected override void OnStateLogicInit(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      AiData_PvpNpc_General data = GetAiData(npc);
      if (null != data) {
        info.Time = 0;
        npc.GetMovementStateInfo().IsMoving = false;
        info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
        info.Target = 0;
      }
    }
    public CharacterInfo GetInterestestTargetHelper(CharacterInfo srcObj, CharacterRelation relation)
    {
      CharacterInfo interestTarget = null;
      List<AiTargetType> priorityList = new List<AiTargetType>();
      priorityList.Add(AiTargetType.SOLDIER);
      priorityList.Add(AiTargetType.TOWER);
      priorityList.Add(AiTargetType.USER);
      for (int index = 0; index < priorityList.Count; ++index) {
        interestTarget = AiLogicUtility.GetInterestestTargetHelper(srcObj, relation, priorityList[index]);
        if (null != interestTarget) {
          break;
        }
      }
      return interestTarget;
    }
    private void IdleHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      AiData_PvpNpc_General data = GetAiData(npc);
      if (null != data) {
        npc.GetMovementStateInfo().IsMoving = false;
        if (data.PatrolPath.HavePathPoint) {
          info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
          info.Time = 0;
          data.FoundPath.Clear();
          ChangeToState(npc, (int)AiStateId.Patrol);
        } else {
          ChangeToState(npc, (int)AiStateId.Combat);
        }
      }
    }
    private void PatrolHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        info.Time = 0;
        AiData_PvpNpc_General data = GetAiData(npc);
        if (null != data) {
          CharacterInfo interestestTarget = GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
          if (null != interestestTarget) {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            info.Time = 0;
            info.Target = interestestTarget.GetId();
            data.FoundPath.Clear();
            ChangeToState(npc, (int)AiStateId.Pursuit);
          } else {
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            if (!data.PatrolPath.IsReached(srcPos)) {
              PathToTargetWithoutObstacle(npc, data.FoundPath, data.PatrolPath.CurPathPoint,100);
            } else {
              data.PatrolPath.UseNextPathPoint();
              data.FoundPath.Clear();
              if (!data.PatrolPath.HavePathPoint) {
                info.Time = 0;
                ChangeToState(npc, (int)AiStateId.Idle);
              }
            }
            info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
          }
        }
      }
    }
    private void PursuitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      //ai配置参数
      long maxPursuitTime = int.Parse(info.AiParam[0]);
      long minPursuitTime = int.Parse(info.AiParam[1]);
      //
      bool goHome = false;
      info.Time += deltaTime;
      CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
      AiData_PvpNpc_General data = GetAiData(npc);
      if (null != target && info.Time <= maxPursuitTime && null != data) {
        if (info.Time >= minPursuitTime) {
          //超过最小追击时间，尝试换攻击目标
          CharacterInfo interestestTarget = GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
          if (null!=interestestTarget && interestestTarget != target) {
            info.Time = 0;
            info.Target = interestestTarget.GetId();
            data.FoundPath.Clear();
            target = interestestTarget;
          }
        }
        float dist = (float)(npc.GetActualProperty().AttackRange * npc.AttackRangeCoefficient);
        float distGoHome = (float)npc.GohomeRange;
        Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
        ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
        float powDist = Geometry.DistanceSquare(srcPos, targetPos);
        float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
        if (powDist <= dist * dist && npc.SpatialSystem.CanShoot(npc.SpaceObject, target.GetMovementStateInfo().GetPosition3D())) {
          npc.GetMovementStateInfo().IsMoving = false;
          info.Time = 0;
          ChangeToState(npc, (int)AiStateId.Combat);
          NotifyNpcMove(npc);
        } else if (powDistToHome < distGoHome * distGoHome) {
          if (AiLogicUtility.GetWalkablePosition(target, npc, ref targetPos)) {
            PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos,deltaTime);
          } else {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
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
        info.HomePos = GetHomePosition(npc, data);
        data.FoundPath.Clear();
        ChangeToState(npc, (int)AiStateId.GoHome);
      }
    }
    private void CombatHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        AiData_PvpNpc_General data = GetAiData(npc);
        if (null != data) {
          data.Time += info.Time;
          data.ThinkingTime += info.Time;
          //ai配置参数  小兵思考时间
          long thinkingTime = int.Parse(info.AiParam[2]);
          //大于思考时间 重新选择新的目标
          if (data.ThinkingTime > thinkingTime) {
            CharacterInfo interestestTarget = GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY); 
            if (null != interestestTarget &&info.Target != interestestTarget.GetId()) {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              info.Time = 0;
              data.Time = 0;
              data.ThinkingTime = 0;
              info.Target = interestestTarget.GetId();
              return;
            }
          }
          info.Time = 0;
          bool changeTarget = false;
          CharacterInfo target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
          if (null != target) {
            float dist = (float)(npc.GetActualProperty().AttackRange * npc.AttackRangeCoefficient);
            float distGoHome = (float)npc.GohomeRange;
            ScriptRuntime.Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            float powDist = Geometry.DistanceSquare(srcPos, targetPos);
            float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
            if (powDist < dist * dist && npc.SpatialSystem.CanShoot(npc.SpaceObject, target.GetMovementStateInfo().GetPosition3D())) {
              float rps = npc.GetActualProperty().Rps;
              if (rps > 0.001f && data.Time > 1000 / rps) {
                data.Time = 0;
                float dir = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
                npc.GetMovementStateInfo().SetFaceDir(dir);
                npc.GetMovementStateInfo().SetMoveDir(dir);
                if (npc.CanShoot()) {
                  aiCmdDispatcher.NpcFace(npc, this);
                }
              }
            } else if (powDistToHome < distGoHome * distGoHome) {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              info.Time = 0;
              data.FoundPath.Clear();
              ChangeToState(npc, (int)AiStateId.Pursuit);
            } else {
              changeTarget = true;
            }
          } else {
            changeTarget = true;
          }
          if (changeTarget) {
            data.FoundPath.Clear();
            target = GetInterestestTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
            if (null != target) {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              info.Time = 0;
              info.Target = target.GetId();
              ChangeToState(npc, (int)AiStateId.Pursuit);
            } else {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              info.Time = 0;
              info.HomePos = GetHomePosition(npc, data);
              ChangeToState(npc, (int)AiStateId.GoHome);
            }
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
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        info.Time = 0;
        AiData_PvpNpc_General data = GetAiData(npc);
        if (null != data) {
          Vector3 targetPos = info.HomePos;
          ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
          float powDistToHome = Geometry.DistanceSquare(srcPos, info.HomePos);
          if (powDistToHome <= 1) {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            info.Time = 0;
            data.FoundPath.Clear();
            ChangeToState(npc, (int)AiStateId.Patrol);
          } else {
            PathToTargetWithoutObstacle(npc, data.FoundPath, targetPos, 100);
            /*
            float angle = MathUtil.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            npc.GetMovementStateInfo().SetFaceDir(angle);
            npc.GetMovementStateInfo().SetMoveDir(angle);
            npc.GetMovementStateInfo().TargetPosition = targetPos;
            npc.GetMovementStateInfo().IsMoving = true;
            */
          }
        }
      }
    }

    private void PathToTargetWithoutObstacle(NpcInfo npc, AiPathData data, Vector3 pathTargetPos, long deltaTime) {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      if (null != data) {
        data.UpdateTime += deltaTime;
        ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
        float dir = npc.GetMovementStateInfo().GetMoveDir();
        bool findObstacle = false;
        bool havePathPoint = data.HavePathPoint;
        if (havePathPoint) {//沿路点列表移动的逻辑
          Vector3 targetPos = data.CurPathPoint;
          if (!data.IsReached(srcPos)) {//向指定路点移动（避让移动过程）
            float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            Vector3 prefVelocity = (float)npc.GetActualProperty().MoveSpeed*new Vector3((float)Math.Sin(angle),0,(float)Math.Cos(angle));
            Vector3 v = new Vector3(targetPos.X-srcPos.X,0,targetPos.Z-srcPos.Z);
            v.Normalize();
            Vector3 velocity = npc.SpaceObject.GetVelocity();
            float speedSquare = (float)npc.GetActualProperty().MoveSpeed * (float)npc.GetActualProperty().MoveSpeed;
            long stTime = TimeUtility.GetElapsedTimeUs();
            Vector3 newVelocity = npc.SpatialSystem.ComputeVelocity(npc.SpaceObject, v, (float)deltaTime / 1000, (float)npc.GetActualProperty().MoveSpeed, (float)npc.GetRadius(), data.IsUsingAvoidanceVelocity);
            long endTime = TimeUtility.GetElapsedTimeUs();
            long calcTime = endTime - stTime;
            if (calcTime > 10000) {
              LogSystem.Warn("*** pvp ComputeVelocity consume {0} us,npc:{1} velocity:{2} newVelocity:{3} deltaTime:{4} speed:{5} pos:{6}", calcTime, npc.GetId(), velocity.ToString(), newVelocity.ToString(), deltaTime, npc.GetActualProperty().MoveSpeed, npc.GetMovementStateInfo().GetPosition3D().ToString());
              for (LinkedListNode<UserInfo> node = npc.UserManager.Users.FirstValue; null != node; node = node.Next) {
                UserInfo userInfo = node.Value;
                if (null != userInfo) {
                  LogSystem.Warn("===>User:{0} Pos:{1}", userInfo.GetId(), userInfo.GetMovementStateInfo().GetPosition3D().ToString());
                }
              }
              for (LinkedListNode<NpcInfo> node = npc.NpcManager.Npcs.FirstValue; null != node; node = node.Next) {
                NpcInfo npcInfo = node.Value;
                if (null != npcInfo) {
                  LogSystem.Warn("===>Npc:{0} Pos:{1}", npcInfo.GetId(), npcInfo.GetMovementStateInfo().GetPosition3D().ToString());
                }
              }
            }
            if(findObstacle){//当前移动方向遇到阻挡，停止移动，触发寻路
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else if (!npc.GetMovementStateInfo().IsMoving && velocity.LengthSquared() > speedSquare * 0.25f) {//正常移动过程，继续移动
              velocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + velocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } 
          } else {//改变路点或结束沿路点移动
            data.UseNextPathPoint();
            if(data.HavePathPoint){
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().SetMoveDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              data.Clear();
            }
          }
        }
        if (!havePathPoint || findObstacle) {//获得路点过程（寻路）
          data.Clear();
          Vector3 targetPos = pathTargetPos;
          bool canGo = true;
          if (canGo) {
            List<Vector3> posList = null;
            bool canPass = npc.SpatialSystem.CanPass(npc.SpaceObject, targetPos);
            if (canPass) {
              posList = new List<Vector3>();
              posList.Add(srcPos);
              posList.Add(targetPos);
            } else {
              long stTime = TimeUtility.GetElapsedTimeUs();
              posList = npc.SpatialSystem.FindPath(srcPos, targetPos, npc.AvoidanceRadius);
              long endTime = TimeUtility.GetElapsedTimeUs();
              long calcTime = endTime - stTime;
              if (calcTime > 10000) {
                LogSystem.Warn("*** pvp FindPath consume {0} us,npc:{1} from:{2} to:{3} radius:{4} pos:{5}", calcTime, npc.GetId(), srcPos.ToString(), targetPos.ToString(), npc.AvoidanceRadius, npc.GetMovementStateInfo().GetPosition3D().ToString());
              }
            }
            if (posList.Count >= 2) {
              data.SetPathPoints(posList[0], posList, 1);
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().SetMoveDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            }
          } else {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            data.IsUsingAvoidanceVelocity = false;
          }
        }
      }
    }
    private void PathToTarget(NpcInfo npc, AiPathData data, Vector3 pathTargetPos, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      if (null != data) {
        data.UpdateTime += deltaTime;
        ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
        float dir = npc.GetMovementStateInfo().GetMoveDir();
        bool findObstacle = false;
        bool havePathPoint = data.HavePathPoint;
        if (havePathPoint) {//沿路点列表移动的逻辑
          Vector3 targetPos = data.CurPathPoint;
          if (!data.IsReached(srcPos)) {//向指定路点移动（避让移动过程）
            float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            Vector3 prefVelocity = (float)npc.GetActualProperty().MoveSpeed*new Vector3((float)Math.Sin(angle),0,(float)Math.Cos(angle));
            Vector3 v = new Vector3(targetPos.X-srcPos.X,0,targetPos.Z-srcPos.Z);
            v.Normalize();
            Vector3 velocity = npc.SpaceObject.GetVelocity();
            float speedSquare = (float)npc.GetActualProperty().MoveSpeed * (float)npc.GetActualProperty().MoveSpeed;
            long stTime = TimeUtility.GetElapsedTimeUs();
            Vector3 newVelocity = npc.SpatialSystem.ComputeVelocity(npc.SpaceObject, v, (float)deltaTime / 1000, (float)npc.GetActualProperty().MoveSpeed, (float)npc.GetRadius(), data.IsUsingAvoidanceVelocity);
            findObstacle = !AiLogicUtility.IsWalkable(npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius), srcPos, newVelocity);
            long endTime = TimeUtility.GetElapsedTimeUs();
            long calcTime = endTime - stTime;
            if (calcTime > 10000) {
              LogSystem.Warn("*** pvp ComputeVelocity consume {0} us,npc:{1} velocity:{2} newVelocity:{3} deltaTime:{4} speed:{5} pos:{6}", calcTime, npc.GetId(), velocity.ToString(), newVelocity.ToString(), deltaTime, npc.GetActualProperty().MoveSpeed, npc.GetMovementStateInfo().GetPosition3D().ToString());
              for (LinkedListNode<UserInfo> node = npc.UserManager.Users.FirstValue; null != node; node = node.Next) {
                UserInfo userInfo = node.Value;
                if (null != userInfo) {
                  LogSystem.Warn("===>User:{0} Pos:{1}", userInfo.GetId(), userInfo.GetMovementStateInfo().GetPosition3D().ToString());
                }
              }
              for (LinkedListNode<NpcInfo> node = npc.NpcManager.Npcs.FirstValue; null != node; node = node.Next) {
                NpcInfo npcInfo = node.Value;
                if (null != npcInfo) {
                  LogSystem.Warn("===>Npc:{0} Pos:{1}", npcInfo.GetId(), npcInfo.GetMovementStateInfo().GetPosition3D().ToString());
                }
              }
            }
            if(findObstacle){//当前移动方向遇到阻挡，停止移动，触发寻路
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else if (data.UpdateTime > 1000) {//避让速度改变每秒一次（表现上更像人类一些）
              data.UpdateTime = 0;

              float newV = newVelocity.Length();
              npc.VelocityCoefficient = newV / npc.GetActualProperty().MoveSpeed;
              float newAngle = Geometry.GetYAngle(new Vector2(0, 0), new Vector2(newVelocity.X, newVelocity.Z));
              npc.GetMovementStateInfo().SetFaceDir(newAngle);
              npc.GetMovementStateInfo().SetMoveDir(newAngle);
              newVelocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + newVelocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = true;
            } else if (Geometry.DistanceSquare(velocity, newVelocity) >= 9.0f) {//没有到速度改变周期，但避让方向需要大幅调整
              if (Geometry.Dot(newVelocity, prefVelocity) > 0) {//如果是调整为与目标方向一致，则进行调整
                float newAngle = Geometry.GetYAngle(new Vector2(0, 0), new Vector2(newVelocity.X, newVelocity.Z));
                npc.GetMovementStateInfo().SetFaceDir(newAngle);
                npc.GetMovementStateInfo().SetMoveDir(newAngle);
                newVelocity.Normalize();
                npc.GetMovementStateInfo().TargetPosition = srcPos + newVelocity * Geometry.Distance(srcPos, targetPos);
                npc.GetMovementStateInfo().IsMoving = true;
                NotifyNpcMove(npc);
                data.IsUsingAvoidanceVelocity = true;
              } else {//如果调整为远离目标方向，则停止
                npc.GetMovementStateInfo().IsMoving = false;
                NotifyNpcMove(npc);
                data.IsUsingAvoidanceVelocity = false;
              }
            } else if (!npc.GetMovementStateInfo().IsMoving && velocity.LengthSquared() > speedSquare * 0.25f) {//正常移动过程，继续移动
              velocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + velocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } 
          } else {//改变路点或结束沿路点移动
            data.UseNextPathPoint();
            if(data.HavePathPoint){
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().SetMoveDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              data.Clear();
            }
          }
        }
        if (!havePathPoint || findObstacle) {//获得路点过程（寻路）
          data.Clear();
          Vector3 targetPos = pathTargetPos;
          bool canGo = true;
          if (!npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius).CanPass(targetPos)) {
            if (!AiLogicUtility.GetWalkablePosition(npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius), targetPos, srcPos, ref targetPos))
              canGo = false;
          }
          if (canGo) {
            List<Vector3> posList = null;
            bool canPass = npc.SpatialSystem.CanPass(npc.SpaceObject, targetPos);
            if (canPass) {
              posList = new List<Vector3>();
              posList.Add(srcPos);
              posList.Add(targetPos);
            } else {
              long stTime = TimeUtility.GetElapsedTimeUs();
              posList = npc.SpatialSystem.FindPath(srcPos, targetPos, npc.AvoidanceRadius);
              long endTime = TimeUtility.GetElapsedTimeUs();
              long calcTime = endTime - stTime;
              if (calcTime > 10000) {
                LogSystem.Warn("*** pvp FindPath consume {0} us,npc:{1} from:{2} to:{3} radius:{4} pos:{5}", calcTime, npc.GetId(), srcPos.ToString(), targetPos.ToString(), npc.AvoidanceRadius, npc.GetMovementStateInfo().GetPosition3D().ToString());
              }
            }
            if (posList.Count >= 2) {
              data.SetPathPoints(posList[0], posList, 1);
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().SetMoveDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              npc.GetMovementStateInfo().IsMoving = false;
              NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            }
          } else {
            npc.GetMovementStateInfo().IsMoving = false;
            NotifyNpcMove(npc);
            data.IsUsingAvoidanceVelocity = false;
          }
        }
      }
    }
    private Vector3 GetHomePosition(NpcInfo npc,AiData_PvpNpc_General data)
    {
      Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
      Vector3 pt = new Vector3();
      if (data.PatrolPath.GetNearstPoint(npc.SpatialSystem, npc.AvoidanceRadius, srcPos, ref pt)) {
        if (!npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius).CanPass(pt)) {
          if (!AiLogicUtility.GetWalkablePosition(npc.SpatialSystem.GetCellMapView(npc.AvoidanceRadius), pt, srcPos, ref pt)) {
            pt = srcPos;
          }
        }
      }
      return pt;
    }
    private AiData_PvpNpc_General GetAiData(NpcInfo npc)
    {
      AiData_PvpNpc_General data = npc.GetAiStateInfo().AiDatas.GetData<AiData_PvpNpc_General>();
      if (null == data) {
        data = new AiData_PvpNpc_General();
        npc.GetAiStateInfo().AiDatas.AddData(data);
      }
      return data;
    }
  }
}
