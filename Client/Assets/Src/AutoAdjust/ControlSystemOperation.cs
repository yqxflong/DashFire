using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ScriptRuntime;

namespace DashFire
{
  public sealed class ControlSystemOperation
  {
    public static void AdjustUserPosition(int id, float x, float y, float z, float time, float dir)
    {
      CharacterInfo info = WorldSystem.Instance.GetCharacterById(id);
      if (null != info) {
        MovementStateInfo msi=info.GetMovementStateInfo();
        if(time<1000) {
          Vector3 pos = msi.GetPosition3D();
          float speed = info.GetActualProperty().MoveSpeed;
          float distance = (speed * time) / 1000;
          float len = pos.Length();
          float nz = (float)(z + distance * Math.Cos(dir));
          float nx = (float)(x + distance * Math.Sin(dir));

          int ctrlId = ControllerIdCalculator.Calc(ControllerType.Position, id);
          PositionController ctrl = ControlSystem.Instance.PositionControllerPool.Alloc();
          if (null != ctrl) {
            ctrl.Init(ctrlId, id, nx - pos.X, 0, nz - pos.Z, 1000);
            ControlSystem.Instance.AddController(ctrl);
          }
          LogSystem.Debug("PositionController start, dx:{0} dz:{1} time:{2}", nx - pos.X, nz - pos.Z, 500);
        } else {
          msi.SetPosition2D(x, z);

          LogSystem.Debug("PositionController just move to pos, x:{0} z:{1}", x, z);
        }
      }
    }
    public static void AdjustCharacterFace(int id, float faceDir)
    {
      CharacterInfo info = WorldSystem.Instance.GetCharacterById(id);
      if (null != info) {
        int ctrlId = ControllerIdCalculator.Calc(ControllerType.Face, id);
        FaceController ctrl = ControlSystem.Instance.FaceControllerPool.Alloc();
        if (null != ctrl) {
          ctrl.Init(ctrlId, id, faceDir);
          ControlSystem.Instance.AddController(ctrl);
        }
      }
    }
    public static void AdjustUserMove()
    {
      UserInfo myself = WorldSystem.Instance.GetPlayerSelf();
      if (null != myself) {
        float dir;
        if (MoveController.CanControl(myself, out dir)) {
          int ctrlId = ControllerIdCalculator.Calc(ControllerType.Move, 0);
          bool canAdjust=true;
          if(ControlSystem.Instance.ExistController(ctrlId)){
            ++WorldSystem.Instance.MoveDirAdjustCount;
            if (WorldSystem.Instance.MoveDirAdjustCount > 8)//最多可重复调节8次
              canAdjust = false;
          }
          if (canAdjust) {
            MoveController ctrl = ControlSystem.Instance.MoveControllerPool.Alloc();
            if (null != ctrl) {
              ctrl.Init(ctrlId, WorldSystem.Instance.InputMoveDir, dir);
              ControlSystem.Instance.AddController(ctrl);
            }

            LogSystem.Debug("MoveController start, offset dir:{0}", dir);
          } else {
            LogSystem.Debug("MoveController cancel because aleady a controller exist");
          }
        } else {
          LogSystem.Debug("MoveController can't control, no suitable dir");
        }
      }
    }
  }
}
