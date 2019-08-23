using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;
using DashFire.Network;

namespace DashFire
{
  public class MoveController : AbstractController<MoveController>
  {
    public override void Adjust()
    {
      long curTime = TimeUtility.GetServerMilliseconds();
      if (m_LastAdjustTime + 100 < curTime) {
        m_LastAdjustTime = curTime;

        UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
        if (null != myself) {
          float moveDir = myself.GetMovementStateInfo().GetMoveDir();
          Vector2 pos = myself.GetMovementStateInfo().GetPosition2D();
          if (!myself.GetMovementStateInfo().IsMoving) {//玩家已经停止移动，控制停止
            m_IsTerminated = true;
            NetworkSystem.Instance.SyncPlayerMoveStop();

            LogSystem.Debug("MoveController finish because stop move, adjust MoveDir:{0}", moveDir);
          } else if (!Geometry.IsSameDouble(WorldSystem.Instance.InputMoveDir, m_InputDir)) {//玩家输入的移动方向改变，控制停止
            m_IsTerminated = true;

            LogSystem.Debug("MoveController finish because change move dir, adjust MoveDir:{0}", moveDir);
          } else if (CanGo(myself, pos.X, pos.Y, m_InputDir, c_ForecastDistance * 3, false)) {//如果输入移动方向可以行走，则恢复移动方向，并停止控制
            m_DirOffset = 0;
            myself.GetMovementStateInfo().SetMoveDir(m_InputDir);
            DoMove();
            m_IsTerminated = true;

            LogSystem.Debug("MoveController finish because restore move dir, adjust MoveDir:{0}", moveDir);
          }
        } else {
          m_IsTerminated = true;

          LogSystem.Debug("MoveController finish because can't find player");
        }
        if (m_IsTerminated) {
          WorldSystem.Instance.MoveDirAdjustCount = 0;
        }
      }
    }

    public void Init(int id, float inputDir, float offsetDir)
    {
      m_Id = id;
      m_InputDir = inputDir;
      m_DirOffset = offsetDir;
      m_LastAdjustTime = 0;

      DoMove();
    }

    private void DoMove()
    {      
      UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
      if (null != myself) {
        float moveDir = myself.GetMovementStateInfo().GetMoveDir();
        myself.GetMovementStateInfo().IsMoveMeetObstacle = false;
        float newDir = CalcDir(moveDir, m_DirOffset);
        myself.GetMovementStateInfo().SetMoveDir(newDir);
        NetworkSystem.Instance.SyncPlayerMoveStart((float)newDir);

        LogSystem.Debug("MoveController adjust MoveDir:{0}+{1}", moveDir, m_DirOffset);
      }
    }

    private float m_InputDir = 0;
    private float m_DirOffset = 0;
    private long m_LastAdjustTime = 0;
    
    public static bool CanControl(CharacterInfo obj, out float offsetDir)
    {
      bool ret = false;
      offsetDir = 0;
      if (null != obj) {
        float moveDir = obj.GetMovementStateInfo().GetMoveDir();
        Vector2 pos = obj.GetMovementStateInfo().GetPosition2D();
        int lastAdjust = WorldSystem.Instance.LastMoveDirAdjust;

        float dir = CalcDir(moveDir, c_PI / 4);
        if (lastAdjust >= 0 && CanGo(obj, pos.X, pos.Y, dir)) {
          offsetDir = c_PI / 4;
          LogSystem.Debug("MoveController CanGo:{0}+{1}, last adjust:{2}", moveDir, offsetDir, lastAdjust); 
          lastAdjust = 1;
          ret = true;
        } else {
          LogSystem.Debug("MoveController try CanGo failed:{0}->{1}, last adjust:{2}", moveDir, dir, lastAdjust); 
          dir = CalcDir(moveDir, -c_PI / 4);
          if (lastAdjust <= 0 && CanGo(obj, pos.X, pos.Y, dir)) {
            offsetDir = -c_PI / 4;
            LogSystem.Debug("MoveController CanGo:{0}+{1}, last adjust:{2}", moveDir, offsetDir, lastAdjust); 
            lastAdjust = -1;
            ret = true;
          } else {
            LogSystem.Debug("MoveController try CanGo failed:{0}->{1}, last adjust:{2}", moveDir, dir, lastAdjust); 
            dir = CalcDir(moveDir, c_PI / 2);
            if (lastAdjust >= 0 && CanGo(obj, pos.X, pos.Y, dir)) {
              offsetDir = c_PI / 2;
              LogSystem.Debug("MoveController CanGo:{0}+{1}, last adjust:{2}", moveDir, offsetDir, lastAdjust); 
              lastAdjust = 1;
              ret = true;
            } else {
              LogSystem.Debug("MoveController try CanGo failed:{0}->{1}, last adjust:{2}", moveDir, dir, lastAdjust);   
              dir = CalcDir(moveDir, - c_PI / 2);
              if (lastAdjust <= 0 && CanGo(obj, pos.X, pos.Y, dir)) {
                offsetDir = -c_PI / 2;
                LogSystem.Debug("MoveController CanGo:{0}+{1}, last adjust:{2}", moveDir, offsetDir, lastAdjust); 
                lastAdjust = -1;
                ret = true;
              } else {
                LogSystem.Debug("MoveController try CanGo failed:{0}->{1}, last adjust:{2}", moveDir, dir, lastAdjust); 
              }
            }
          }
        }
        if (ret) {
          WorldSystem.Instance.LastMoveDirAdjust = lastAdjust;
        } else {
          LogSystem.Debug("MoveController Can't adjust for {0}", moveDir);
        }
      }
      return ret;
    }

    private static bool CanGo(CharacterInfo obj, float x, float y, float dir)
    {
      return CanGo(obj, x, y, dir, c_ForecastDistance, true);
    }

    private static bool CanGo(CharacterInfo obj, float x, float y, float dir, float forecastDistance, bool logging)
    {
      float cosV = (float)Math.Cos(dir);
      float sinV = (float)Math.Sin(dir);
      float tryY = (float)(y + forecastDistance * cosV);
      float tryX = (float)(x + forecastDistance * sinV);
      Vector3 spos = obj.GetMovementStateInfo().GetPosition3D();
      Vector3 dpos = new Vector3(tryX, 0, tryY);
      bool ret = obj.SpatialSystem.CanPass(spos, dpos);
      if (logging) {
        int srow, scol, drow, dcol;
        obj.SpatialSystem.GetCellMapView(1).GetCell(spos, out srow, out scol);
        obj.SpatialSystem.GetCellMapView(1).GetCell(dpos, out drow, out dcol);
        if (ret) {
          LogSystem.Debug("MoveController CanGo return true, ({0},{1})->({2},{3}), dir:{4}", srow, scol, drow, dcol, dir);
        } else {
          LogSystem.Debug("MoveController CanGo return false, ({0},{1})->({2},{3}), dir:{4}", srow, scol, drow, dcol, dir);
        }
      }
      return ret;
    }

    private static float CalcDir(float baseDir, float offsetDir)
    {
      return (baseDir + c_2PI + offsetDir) % c_2PI;
    }

    private const float c_ForecastDistance = 0.75f;
    private const float c_PI = (float)Math.PI;
    private const float c_2PI = (float)Math.PI * 2;
  }
}
