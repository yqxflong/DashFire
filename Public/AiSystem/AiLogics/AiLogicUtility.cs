using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DashFire.Debug;
using ScriptRuntime;
using DashFireSpatial;

namespace DashFire
{
  public enum AiTargetType : int
  {
    USER = 0,
    NPC,
    ALL,
    TOWER,
    SOLDIER,
  }
  public sealed class AiLogicUtility
  {
    public const int c_MaxViewRange = 30;
    public const int c_MaxViewRangeSqr = c_MaxViewRange * c_MaxViewRange;
    public static CharacterInfo GetNearstTargetHelper(CharacterInfo srcObj, CharacterRelation relation)
    {
      return GetNearstTargetHelper(srcObj, relation, AiTargetType.ALL);
    }
    public static CharacterInfo GetNearstTargetHelper(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type)
    {
      CharacterInfo nearstTarget = null;
      DashFireSpatial.ISpatialSystem spatialSys = srcObj.SpatialSystem;
      if (null != spatialSys) {
        ScriptRuntime.Vector3 srcPos = srcObj.GetMovementStateInfo().GetPosition3D();
        float minPowDist = 999999;
        spatialSys.VisitObjectInCircle(srcPos, srcObj.ViewRange, (float distSqr, ISpaceObject obj) => {
          StepCalcNearstTarget(srcObj, relation, type, distSqr, obj, ref minPowDist, ref nearstTarget);
        });
      }
      return nearstTarget;
    }

    public static CharacterInfo GetInterestestTargetHelper(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type)
    {
      CharacterInfo interestestTarget = null;
      DashFireSpatial.ISpatialSystem spatialSys = srcObj.SpatialSystem;
      if (null != spatialSys) {
        ScriptRuntime.Vector3 srcPos = srcObj.GetMovementStateInfo().GetPosition3D();
        float minPowDist = 999999;
        spatialSys.VisitObjectInCircle(srcPos, srcObj.ViewRange, (float distSqr, ISpaceObject obj) => {
          GetInterestestTarget(srcObj, relation, type, distSqr, obj, ref minPowDist, ref interestestTarget);
        });
      }
      return interestestTarget;
    }

    public static CharacterInfo GetRandomTargetHelper(CharacterInfo srcObj, CharacterRelation relation)
    {
      return GetNearstTargetHelper(srcObj, relation, AiTargetType.ALL);
    }
    public static CharacterInfo GetRandomTargetHelper(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type)
    {
      CharacterInfo target = null;
      DashFireSpatial.ISpatialSystem spatialSys = srcObj.SpatialSystem;
      if (null != spatialSys) {
        List<DashFireSpatial.ISpaceObject> objs = spatialSys.GetObjectInCircle(srcObj.GetMovementStateInfo().GetPosition3D(), srcObj.ViewRange, (distSqr, obj) => IsWantedTargetForRandomTarget(srcObj, relation, type, obj));
        int index = Helper.Random.Next(objs.Count);
        if (index >= 0 && index < objs.Count) {
          target = objs[index].RealObject as CharacterInfo;
        }
      }
      return target;
    }
    public static CharacterInfo GetLivingCharacterInfoHelper(CharacterInfo srcObj, int id)
    {
      CharacterInfo target = srcObj.NpcManager.GetNpcInfo(id);
      if (null == target) {
        target = srcObj.UserManager.GetUserInfo(id);
      }
      if (null != target) {
        if (target.IsDead())
          target = null;
      }
      return target;
    }
    public static CharacterInfo GetSeeingLivingCharacterInfoHelper(CharacterInfo srcObj, int id)
    {
      CharacterInfo target = srcObj.NpcManager.GetNpcInfo(id);
      if (null == target) {
        target = srcObj.UserManager.GetUserInfo(id);
      }
      if (null != target) {
        if (target.IsHaveStateFlag(CharacterState_Type.CST_Hidden))
          target = null;
        else if (target.IsDead())
          target = null;
        else if (!CanSee(srcObj, target))
          target = null;
      }
      return target;
    }

    private static void GetInterestestTarget(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type, float powDist, ISpaceObject obj, ref float minPowDist, ref CharacterInfo interestestTarget)
    {
      if (type == AiTargetType.USER && obj.GetObjType() != SpatialObjType.kUser) return;
      if (type == AiTargetType.TOWER && obj.GetObjType() != SpatialObjType.kNPC) return;
      if (type == AiTargetType.SOLDIER && obj.GetObjType() != SpatialObjType.kNPC) return;
      CharacterInfo target = GetSeeingLivingCharacterInfoHelper(srcObj, (int)obj.GetID());
      if (null != target && !target.IsDead()) {
        if (target.IsControlMecha) {
          return;
        }
        NpcInfo npcTarget = target.CastNpcInfo();
        if (null != npcTarget && npcTarget.NpcType == (int)NpcTypeEnum.Skill) {
          return;
        }
        if (type == AiTargetType.SOLDIER && npcTarget.IsPvpTower)
          return;
        if (type == AiTargetType.TOWER && !npcTarget.IsPvpTower)
          return;

        if (relation == CharacterInfo.GetRelation(srcObj, target)) {
          if (powDist < minPowDist) {
            if (powDist > c_MaxViewRangeSqr || CanSee(srcObj, target)) {
              interestestTarget = target;
              minPowDist = powDist;
            }
          }
        }
      }
    }

    private static void StepCalcNearstTarget(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type, float powDist, ISpaceObject obj, ref float minPowDist, ref CharacterInfo nearstTarget)
    {
      if (type == AiTargetType.USER && obj.GetObjType() != SpatialObjType.kUser) return;
      if (type == AiTargetType.NPC && obj.GetObjType() != SpatialObjType.kNPC) return;
      CharacterInfo target = GetSeeingLivingCharacterInfoHelper(srcObj, (int)obj.GetID());
      if (null != target && !target.IsDead()) {
        if (target.IsControlMecha) {
          return;
        }
        NpcInfo npcTarget = target.CastNpcInfo();
        if (null != npcTarget && npcTarget.NpcType == (int)NpcTypeEnum.Skill) {
          return;
        }
        if (relation == CharacterInfo.GetRelation(srcObj, target)) {
          if (powDist < minPowDist) {
            if (powDist > c_MaxViewRangeSqr || CanSee(srcObj, target)) {
              nearstTarget = target;
              minPowDist = powDist;
            }
          }
        }
      }
    }
    private static bool IsWantedTargetForRandomTarget(CharacterInfo srcObj, CharacterRelation relation, AiTargetType type, ISpaceObject obj)
    {
      if (type == AiTargetType.USER && obj.GetObjType() != SpatialObjType.kUser) return false;
      if (type == AiTargetType.NPC && obj.GetObjType() != SpatialObjType.kNPC) return false;
      CharacterInfo target = GetSeeingLivingCharacterInfoHelper(srcObj, (int)obj.GetID());
      if (null != target && !target.IsDead()) {
        if (target.IsControlMecha) {
          return false;
        }
        NpcInfo npcTarget = target.CastNpcInfo();
        if (null != npcTarget && npcTarget.NpcType == (int)NpcTypeEnum.Skill) {
          return false;
        }
        if (relation == CharacterInfo.GetRelation(srcObj, target)) {
          if(CanSee(srcObj, target)) {
            return true;
          }
        }
      }
      return false;
    }
    private static bool CanSee(CharacterInfo src, CharacterInfo target)
    {
      int srcCampId = src.GetCampId();
      int targetCampId = target.GetCampId();
      if (srcCampId == targetCampId)
        return true;
      else if (srcCampId == (int)CampIdEnum.Hostile || targetCampId == (int)CampIdEnum.Hostile) {
        return CharacterInfo.CanSee(src, target);
      } else {
        bool isBlue = (targetCampId == (int)CampIdEnum.Blue);
        if (isBlue && target.CurRedCanSeeMe || !isBlue && target.CurBlueCanSeeMe)
          return true;
        else
          return false;
      }
    }

    public static bool GetWalkablePosition(CharacterInfo target, CharacterInfo src, ref Vector3 pos)
    {
      Vector3 srcPos = src.GetMovementStateInfo().GetPosition3D();
      Vector3 targetPos = target.GetMovementStateInfo().GetPosition3D();
      ICellMapView view = target.SpatialSystem.GetCellMapView(src.AvoidanceRadius);
      return GetWalkablePosition(view, targetPos, srcPos, ref pos);
    }
    public static bool GetWalkablePosition(ICellMapView view, Vector3 targetPos, Vector3 srcPos, ref Vector3 pos)
    {
      bool ret=false;
      const int c_MaxCheckCells = 3;
      int row = 0;
      int col = 0;
      view.GetCell(targetPos, out row, out col);
      float radian = Geometry.GetYAngle(new Vector2(targetPos.X, targetPos.Z), new Vector2(srcPos.X, srcPos.Z));
      if (radian >= Math.PI / 4 && radian < Math.PI * 3 / 4) {//右边
        for (int ci = 1; ci <= c_MaxCheckCells; ++ci) {
          for (int ri = 0; ri <= c_MaxCheckCells; ++ri) {
            int row_ = row + ri;
            int col_ = col + ci;
            if (view.IsCellValid(row_, col_)) {
              if (view.CanPass(row_,col_)) {
                pos = view.GetCellCenter(row_, col_);
                ret = true;
                goto exit;
              }
            }
            if (ri > 0) {
              row_ = row - ri;
              if (view.IsCellValid(row_, col_)) {
                if (view.CanPass(row_, col_)) {
                  pos = view.GetCellCenter(row_, col_);
                  ret = true;
                  goto exit;
                }
              }
            }
          }
        }
      } else if (radian >= Math.PI * 3 / 4 && radian < Math.PI * 5 / 4) {//上边
        for (int ri = 1; ri <= c_MaxCheckCells; ++ri) {
          for (int ci = 0; ci <= c_MaxCheckCells; ++ci) {
            int row_ = row - ri;
            int col_ = col + ci;
            if (view.IsCellValid(row_, col_)) {
              if (view.CanPass(row_, col_)) {
                pos = view.GetCellCenter(row_, col_);
                ret = true;
                goto exit;
              }
            }
            if (ci > 0) {
              col_ = col - ci;
              if (view.IsCellValid(row_, col_)) {
                if (view.CanPass(row_, col_)) {
                  pos = view.GetCellCenter(row_, col_);
                  ret = true;
                  goto exit;
                }
              }
            }
          }
        }
      } else if (radian >= Math.PI * 5 / 4 && radian < Math.PI * 7 / 4) {//左边
        for (int ci = 1; ci <= c_MaxCheckCells; ++ci) {
          for (int ri = 0; ri <= c_MaxCheckCells; ++ri) {
            int row_ = row + ri;
            int col_ = col - ci;
            if (view.IsCellValid(row_, col_)) {
              if (view.CanPass(row_, col_)) {
                pos = view.GetCellCenter(row_, col_);
                ret = true;
                goto exit;
              }
            }
            if (ri > 0) {
              row_ = row - ri;
              if (view.IsCellValid(row_, col_)) {
                if (view.CanPass(row_, col_)) {
                  pos = view.GetCellCenter(row_, col_);
                  ret = true;
                  goto exit;
                }
              }
            }
          }
        }
      } else {//下边
        for (int ri = 1; ri <= c_MaxCheckCells; ++ri) {
          for (int ci = 0; ci <= c_MaxCheckCells; ++ci) {
            int row_ = row + ri;
            int col_ = col + ci;
            if (view.IsCellValid(row_, col_)) {
              if (view.CanPass(row_, col_)) {
                pos = view.GetCellCenter(row_, col_);
                ret = true;
                goto exit;
              }
            }
            if (ci > 0) {
              col_ = col - ci;
              if (view.IsCellValid(row_, col_)) {
                if (view.CanPass(row_, col_)) {
                  pos = view.GetCellCenter(row_, col_);
                  ret = true;
                  goto exit;
                }
              }
            }
          }
        }
      }
    exit:
      return ret;
    }
    public static bool IsWalkable(ICellMapView view, Vector3 srcPos, Vector3 dir)
    {
      int row = 0;
      int col = 0;
      view.GetCell(srcPos, out row, out col);
      float r = view.RadiusLength * 2;
      Vector3 targetPos = view.GetCellCenter(row, col);
      dir.Normalize();
      targetPos.X += r * dir.X;
      targetPos.Z += r * dir.Z;
      int oRow = 0;
      int oCol = 0;
      view.GetCell(targetPos, out oRow, out oCol);
      bool ret = true;
      if (oRow != row || oCol != col) {
        ret = view.CanPass(oRow, oCol);
      }
      return ret;
    }

    public static void DoMoveCommandState(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      //执行状态处理
      AiData_ForMoveCommand data = GetAiDataForMoveCommand(npc);
      if (null == data) return;

      if (!data.IsFinish) {
        if (WayPointArrived(npc, data))
          MoveToNext(npc, data);
      }

      //判断是否状态结束并执行相应处理
      if (null != data && data.IsFinish) {
        npc.GetMovementStateInfo().IsMoving = false;
        npc.GetAiStateInfo().PopState();
      }
    }
    private static AiData_ForMoveCommand GetAiDataForMoveCommand(NpcInfo npc)
    {
      AiData_ForMoveCommand data = npc.GetAiStateInfo().AiDatas.GetData<AiData_ForMoveCommand>();
      return data;
    }
    private static void MoveToNext(NpcInfo npc, AiData_ForMoveCommand data)
    {
      if (++data.Index >= data.WayPoints.Count)
      {
        data.IsFinish = true;
        return;
      }

      var move_info = npc.GetMovementStateInfo();
      Vector3 from = move_info.GetPosition3D();
      Vector3 to = data.WayPoints[data.Index];
      float move_dir = MoveDirection(from, to);

      float now = TimeUtility.GetServerMilliseconds();
      float distance = Geometry.Distance(from, to);
      float speed = npc.GetActualProperty().MoveSpeed;
      data.EstimateFinishTime = now + 1000 * (distance / speed);
      DLog._("ai_move", "[{0}]: now({1}), from({2}), to({3}), distance({4}), speed({5}), move_time({6}), estimate({7})",
             npc.GetId(), now, from.ToString(), to.ToString(), distance, speed, 1000 * (distance / speed), data.EstimateFinishTime);

      move_info.IsMoving = true;
      move_info.SetMoveDir(move_dir);
      move_info.SetFaceDir(move_dir);
    }
    private static bool WayPointArrived(NpcInfo npc, AiData_ForMoveCommand data)
    {
      if (TimeUtility.GetServerMilliseconds() >= data.EstimateFinishTime)
      { 
        var move_info = npc.GetMovementStateInfo();
        Vector3 to = data.WayPoints[data.Index];
        Vector3 now = move_info.GetPosition3D();
        float distance = Geometry.Distance(now, to);
        DLog._("ai_move", "[{0}]: closest distance({1}) ", npc.GetId(), distance);
        return true;
      }
      return false;
    }
    private static float MoveDirection(Vector3 from, Vector3 to)
    {
      return (float)Math.Atan2(to.X - from.X, to.Z - from.Z);    
    }

    public static void DoPatrolCommandState(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime, AbstractNpcStateLogic logic)
    {
      if (npc.IsDead())
        return;
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 100) {
        long intervalTime = info.Time;
        info.Time = 0;
        CharacterInfo target = null;
        if (info.IsExternalTarget) {
          target = AiLogicUtility.GetSeeingLivingCharacterInfoHelper(npc, info.Target);
          if (null == target) {
            target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
            if (null != target)
              info.Target = target.GetId();
          }
        } else {
          target = AiLogicUtility.GetNearstTargetHelper(npc, CharacterRelation.RELATION_ENEMY);
          if (null != target)
            info.Target = target.GetId();
        }
        if (null != target) {
          logic.NotifyNpcTargetChange(npc);
          npc.GetMovementStateInfo().IsMoving = false;
          logic.NotifyNpcMove(npc);
          info.Time = 0;
          AiData_ForPatrolCommand data = GetAiDataForPatrolCommand(npc);
          if(null!=data)
            data.FoundPath.Clear();
          logic.ChangeToState(npc, (int)AiStateId.Pursuit);
        } else {
          AiData_ForPatrolCommand data = GetAiDataForPatrolCommand(npc);
          if (null != data) {
            ScriptRuntime.Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
            if (data.PatrolPath.HavePathPoint && !data.PatrolPath.IsReached(srcPos)) {
              PathToTargetWithoutObstacle(npc, data.FoundPath, data.PatrolPath.CurPathPoint, intervalTime, true, logic);
            } else {
              data.PatrolPath.UseNextPathPoint();
              data.FoundPath.Clear();
              if (!data.PatrolPath.HavePathPoint) {
                if (data.IsLoopPatrol) {
                  data.PatrolPath.Restart();
                } else {
                  info.Time = 0;
                  logic.ChangeToState(npc, (int)AiStateId.Idle);
                }
              }
            }
            info.HomePos = npc.GetMovementStateInfo().GetPosition3D();
          } else {
            info.Time = 0;
            logic.ChangeToState(npc, (int)AiStateId.Idle);
          }
        }
      }
    }
    public static void PathToTargetWithoutObstacle(NpcInfo npc, AiPathData data, Vector3 pathTargetPos, long deltaTime, bool faceIsMoveFir, AbstractNpcStateLogic logic) {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
      if (null != data) {
        data.Clear();
        data.UpdateTime += deltaTime;
        Vector3 targetPos = pathTargetPos;
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
            LogSystem.Warn("*** pve FindPath consume {0} us,npc:{1} from:{2} to:{3} radius:{4} pos:{5}", calcTime, npc.GetId(), srcPos.ToString(), targetPos.ToString(), npc.AvoidanceRadius, npc.GetMovementStateInfo().GetPosition3D().ToString());
          }
        }
        if (posList.Count >= 2) {
          data.SetPathPoints(posList[0], posList, 1);
        } else {
          npc.GetMovementStateInfo().IsMoving = false;
          logic.NotifyNpcMove(npc);
          data.IsUsingAvoidanceVelocity = false;
        }
        bool havePathPoint = data.HavePathPoint;
        if (havePathPoint) {//沿路点列表移动的逻辑
          targetPos = data.CurPathPoint;
          if (!data.IsReached(srcPos)) {//向指定路点移动（避让移动过程）
            float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
            Vector3 prefVelocity = (float)npc.GetActualProperty().MoveSpeed * new Vector3((float)Math.Sin(angle), 0, (float)Math.Cos(angle));
            Vector3 v = new Vector3(targetPos.X - srcPos.X, 0, targetPos.Z - srcPos.Z);
            v.Normalize();
            Vector3 velocity = npc.SpaceObject.GetVelocity();
            float speedSquare = (float)npc.GetActualProperty().MoveSpeed * (float)npc.GetActualProperty().MoveSpeed;
            long stTime = TimeUtility.GetElapsedTimeUs();
            Vector3 newVelocity = npc.SpatialSystem.ComputeVelocity(npc.SpaceObject, v, (float)deltaTime / 1000, (float)npc.GetActualProperty().MoveSpeed, (float)npc.GetRadius(), data.IsUsingAvoidanceVelocity);
            long endTime = TimeUtility.GetElapsedTimeUs();
            long calcTime = endTime - stTime;
            if (calcTime > 10000) {
              LogSystem.Warn("*** pve ComputeVelocity consume {0} us,npc:{1} velocity:{2} newVelocity:{3} deltaTime:{4} speed:{5} pos:{6}", calcTime, npc.GetId(), velocity.ToString(), newVelocity.ToString(), deltaTime, npc.GetActualProperty().MoveSpeed, npc.GetMovementStateInfo().GetPosition3D().ToString());
            }
            if (data.UpdateTime > 500) {
              data.UpdateTime = 0;
              float newAngle = Geometry.GetYAngle(new Vector2(0, 0), new Vector2(newVelocity.X, newVelocity.Z));
              npc.GetMovementStateInfo().SetMoveDir(newAngle);
              if (faceIsMoveFir)
                logic.NotifyNpcFace(npc, newAngle);
              newVelocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + newVelocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
            } else {
              data.UpdateTime += deltaTime;
            }
          } else {//改变路点或结束沿路点移动
            data.UseNextPathPoint();
            if (data.HavePathPoint) {
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetMoveDir(angle);
              if (faceIsMoveFir)
                logic.NotifyNpcFace(npc, angle);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              npc.GetMovementStateInfo().IsMoving = false;
              data.Clear();
            }
          }
        }
      }
    }
    public static void PathToTarget(NpcInfo npc, AiPathData data, Vector3 pathTargetPos, long deltaTime, bool faceIsMoveFir, AbstractNpcStateLogic logic)
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
              LogSystem.Warn("*** pve ComputeVelocity consume {0} us,npc:{1} velocity:{2} newVelocity:{3} deltaTime:{4} speed:{5} pos:{6}", calcTime, npc.GetId(), velocity.ToString(), newVelocity.ToString(), deltaTime, npc.GetActualProperty().MoveSpeed, npc.GetMovementStateInfo().GetPosition3D().ToString());
            }
            if (Geometry.DistanceSquare(newVelocity, new Vector3()) <= speedSquare*0.25f) {//避让计算的移动速度变小（说明没有更好的保持原速的选择，停止）
              npc.GetMovementStateInfo().IsMoving=false;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else if(findObstacle){//当前移动方向遇到阻挡，停止移动，触发寻路
              npc.GetMovementStateInfo().IsMoving = false;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else if (data.UpdateTime > 1000) {//避让速度改变每秒一次（表现上更像人类一些）
              data.UpdateTime = 0;

              float newAngle = Geometry.GetYAngle(new Vector2(0, 0), new Vector2(newVelocity.X, newVelocity.Z));
              npc.GetMovementStateInfo().SetMoveDir(newAngle);
              if (faceIsMoveFir)
                npc.GetMovementStateInfo().SetFaceDir(newAngle);
              newVelocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + newVelocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = true;
            } else if (Geometry.DistanceSquare(velocity, newVelocity) > 9.0f) {//没有到速度改变周期，但避让方向需要大幅调整
              if (Geometry.Dot(newVelocity, prefVelocity) > 0) {//如果是调整为与目标方向一致，则进行调整
                float newAngle = Geometry.GetYAngle(new Vector2(0, 0), new Vector2(newVelocity.X, newVelocity.Z));
                npc.GetMovementStateInfo().SetMoveDir(newAngle);
                if (faceIsMoveFir)
                  npc.GetMovementStateInfo().SetFaceDir(newAngle);
                newVelocity.Normalize();
                npc.GetMovementStateInfo().TargetPosition = srcPos + newVelocity * Geometry.Distance(srcPos, targetPos);
                npc.GetMovementStateInfo().IsMoving = true;
                logic.NotifyNpcMove(npc);
                data.IsUsingAvoidanceVelocity = true;
              } else {//如果调整为远离目标方向，则停止
                npc.GetMovementStateInfo().IsMoving = false;
                logic.NotifyNpcMove(npc);
                data.IsUsingAvoidanceVelocity = false;
              }
            } else if (!npc.GetMovementStateInfo().IsMoving && velocity.LengthSquared() > speedSquare * 0.25f) {//正常移动过程，继续移动
              velocity.Normalize();
              npc.GetMovementStateInfo().TargetPosition = srcPos + velocity * Geometry.Distance(srcPos, targetPos);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } 
          } else {//改变路点或结束沿路点移动
            data.UseNextPathPoint();
            if (data.HavePathPoint) {
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetMoveDir(angle);
              if (faceIsMoveFir)
                npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
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
                LogSystem.Warn("*** pve FindPath consume {0} us,npc:{1} from:{2} to:{3} radius:{4} pos:{5}", calcTime, npc.GetId(), srcPos.ToString(), targetPos.ToString(), npc.AvoidanceRadius, npc.GetMovementStateInfo().GetPosition3D().ToString());
              }
            }
            if (posList.Count >= 2) {
              data.SetPathPoints(posList[0], posList, 1);
              targetPos = data.CurPathPoint;
              npc.GetMovementStateInfo().TargetPosition = targetPos;
              float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
              npc.GetMovementStateInfo().SetMoveDir(angle);
              if (faceIsMoveFir)
                npc.GetMovementStateInfo().SetFaceDir(angle);
              npc.GetMovementStateInfo().IsMoving = true;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            } else {
              npc.GetMovementStateInfo().IsMoving = false;
              logic.NotifyNpcMove(npc);
              data.IsUsingAvoidanceVelocity = false;
            }
          } else {
            npc.GetMovementStateInfo().IsMoving = false;
            logic.NotifyNpcMove(npc);
            data.IsUsingAvoidanceVelocity = false;
          }
        }
      }
    }
    private static AiData_ForPatrolCommand GetAiDataForPatrolCommand(NpcInfo npc)
    {
      AiData_ForPatrolCommand data = npc.GetAiStateInfo().AiDatas.GetData<AiData_ForPatrolCommand>();
      return data;
    }
  }
  class AiLogic_MovableNpc_Client : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Move, this.MoveHandler);
      SetStateHandler((int)AiStateId.Wait, this.WaitHandler);
      SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
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
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
    private void MoveHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (npc.IsDead()) {
        npc.GetMovementStateInfo().IsMoving = false;
        ChangeToState(npc, (int)AiStateId.Wait);
        return;
      }
      NpcAiStateInfo info = npc.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 10) {
        info.Time = 0;
        Vector3 srcPos = npc.GetMovementStateInfo().GetPosition3D();
        Vector3 startPos = info.HomePos;
        Vector3 targetPos = npc.GetMovementStateInfo().TargetPosition;
        if (!IsReached(srcPos, targetPos)) {
          float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
          npc.GetMovementStateInfo().SetMoveDir(angle);
          npc.GetMovementStateInfo().IsMoving = true;
        }
      }
    }
    private void WaitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      AiLogicUtility.DoMoveCommandState(npc, aiCmdDispatcher, deltaTime);
    }
    private bool IsReached(Vector3 src, Vector3 target)
    {
      bool ret = false;
      float powDist = Geometry.DistanceSquare(src,target);
      if (powDist <= 0.25f) {
        ret = true;
      }
      return ret;
    }
  }
  class AiLogic_ImmovableNpc_Client : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Move, this.MoveHandler);
      SetStateHandler((int)AiStateId.Wait, this.WaitHandler);
      SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
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
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
    private void MoveHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
    private void WaitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
  }
  class AiLogic_FixedNpc_Client : AbstractNpcStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Move, this.MoveHandler);
      SetStateHandler((int)AiStateId.Wait, this.WaitHandler);
      SetStateHandler((int)AiStateId.MoveCommand, this.MoveCommandHandler);
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
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
    private void MoveHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
    private void WaitHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      npc.GetMovementStateInfo().IsMoving = false;
    }
    private void MoveCommandHandler(NpcInfo npc, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      NpcAiStateInfo info = npc.GetAiStateInfo();
      npc.GetMovementStateInfo().IsMoving = false;
      ChangeToState(npc, (int)AiStateId.Wait);
    }
  }
  class AiLogic_User_Client : AbstractUserStateLogic
  {
    protected override void OnInitStateHandlers()
    {
      SetStateHandler((int)AiStateId.Idle, this.IdleHandler);
      SetStateHandler((int)AiStateId.Move, this.MoveHandler);
      SetStateHandler((int)AiStateId.Wait, this.WaitHandler);
    }

    protected override void OnStateLogicInit(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      UserAiStateInfo info = user.GetAiStateInfo();
      info.Time = 0;
      info.Target = 0;
    }

    private void IdleHandler(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      UserAiStateInfo info = user.GetAiStateInfo();
      user.GetMovementStateInfo().IsMoving = false;
      ChangeToState(user, (int)AiStateId.Wait);
    }
    private void MoveHandler(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
      if (user.IsDead()) {
        user.GetMovementStateInfo().IsMoving = false;
        ChangeToState(user, (int)AiStateId.Wait);
        return;
      }
      UserAiStateInfo info = user.GetAiStateInfo();
      info.Time += deltaTime;
      if (info.Time > 10) {
        info.Time = 0;
        Vector3 srcPos = user.GetMovementStateInfo().GetPosition3D();
        Vector3 targetPos = user.GetMovementStateInfo().TargetPosition;
        if (!IsReached(srcPos, targetPos)) {
          float angle = Geometry.GetYAngle(new Vector2(srcPos.X, srcPos.Z), new Vector2(targetPos.X, targetPos.Z));
          user.GetMovementStateInfo().SetMoveDir(angle);
          user.GetMovementStateInfo().IsMoving = true;
        }
      }
    }
    private void WaitHandler(UserInfo user, AiCommandDispatcher aiCmdDispatcher, long deltaTime)
    {
    }
    private bool IsReached(Vector3 src, Vector3 target)
    {
      bool ret = false;
      float powDist = Geometry.DistanceSquare(src, target);
      if (powDist <= 0.25f) {
        ret = true;
      }
      return ret;
    }
  }
}
