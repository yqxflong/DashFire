using System;
using UnityEngine;

namespace DashFire
{
  public static class LogicSystem
  {
    public static void SetLoadingBarScene(string name)
    {
      GfxSystem.Instance.SetLoadingBarScene(name);
    }
    public static GameObject GetGameObject(int id)
    {
      return GfxSystem.Instance.GetGameObject(id);
    }
    public static SharedGameObjectInfo GetSharedGameObjectInfo(int id)
    {
      return GfxSystem.Instance.GetSharedGameObjectInfo(id);
    }
    public static SharedGameObjectInfo GetSharedGameObjectInfo(GameObject obj)
    {
      return GfxSystem.Instance.GetSharedGameObjectInfo(obj);
    }
    public static bool ExistGameObject(GameObject obj)
    {
      return GfxSystem.Instance.ExistGameObject(obj);
    }
    public static void VisitGameObject(MyAction<GameObject, SharedGameObjectInfo> visitor)
    {
      GfxSystem.Instance.VisitGameObject(visitor);
    }
    public static GameObject PlayerSelf
    {
      get { return GfxSystem.Instance.PlayerSelf; }
    }
    public static SharedGameObjectInfo PlayerSelfInfo
    {
      get { return GfxSystem.Instance.PlayerSelfInfo; }
    }
    public static bool IsLastHitUi
    {
      get { return GfxSystem.Instance.IsLastHitUi; }
      set { GfxSystem.Instance.IsLastHitUi = value; }
    }
    public static void LogicLog(string format, params object[] args)
    {
	    GfxSystem.Instance.CallLogicLog(format,args);
    }
    public static void GfxLog(string format, params object[] args)
    {
      GfxSystem.Instance.CallGfxLog(string.Format(format, args));
    }
    public static float RadianToDegree(float dir)
    {
      return GfxSystem.Instance.RadianToDegree(dir);
    }
    public static float GetLoadingProgress()
    {
      return GfxSystem.Instance.GetLoadingProgress();
    }
    public static Transform FindChildRecursive(Transform parent, string bonePath)
    {
      return GfxSystem.Instance.FindChildRecursive(parent, bonePath);
    }
    public static void FireGestureEvent(GestureArgs args)
    {
      GfxSystem.Instance.OnGesture(args);
    }
    public static void QueueLogicAction(MyAction action)
    {
      GfxSystem.Instance.QueueLogicAction(action);
    }
    public static void PublishLogicEvent(string evt, string group, params object[] args)
    {
      GfxSystem.Instance.PublishLogicEvent(evt, group, args);
    }
    public static PublishSubscribeSystem EventChannelForGfx
    {
      get
      {
        return GfxSystem.Instance.EventChannelForGfx;
      }
    }

    public static void NotifyGfxAnimationStart(GameObject obj)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.IsGfxAnimation = true;

        //GfxLog("NotifyGfxAnimationStart:{0}", info.m_LogicObjectId);
      } else {
        GfxLog("NotifyGfxAnimationStart:{0}, can't find object !", obj.name);
      }
    }
    public static void NotifyGfxAnimationFinish(GameObject obj)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.IsGfxAnimation = false;

        //GfxLog("NotifyGfxAnimationFinish:{0}", info.m_LogicObjectId);
      } else {
        GfxLog("NotifyGfxAnimationFinish:{0}, can't find object !", obj.name);
      }
    }
    public static void NotifyGfxMoveControlStart(GameObject obj)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.IsGfxMoveControl = true;

        //GfxLog("NotifyGfxMoveControlStart:{0}", info.m_LogicObjectId);
      } else {
        GfxLog("NotifyGfxMoveControlStart:{0}, can't find object !", obj.name);
      }
    }
    public static void NotifyGfxMoveControlFinish(GameObject obj)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.IsGfxMoveControl = false;

        //GfxLog("NotifyGfxMoveControlFinish:{0}", info.m_LogicObjectId);
      } else {
        GfxLog("NotifyGfxMoveControlFinish:{0}, can't find object !", obj.name);
      }
    }
    public static void NotifyGfxUpdatePosition(GameObject obj, float x, float y, float z)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.X = x;
        info.Y = y;
        info.Z = z;
        info.DataChangedByGfx = true;
      } else {
        GfxLog("NotifyGfxUpdatePosition:{0} {1} {2} {3}, can't find object !", obj.name, x, y, z);
      }
    }
    public static void NotifyGfxUpdatePosition(GameObject obj, float x, float y, float z, float rx, float ry, float rz)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        info.X = x;
        info.Y = y;
        info.Z = z;
        info.FaceDir = ry;
        info.DataChangedByGfx = true;
      } else {
        GfxLog("NotifyGfxUpdatePosition:{0} {1} {2} {3} {4} {5} {6}, can't find object !", obj.name, x, y, z, rx, ry, rz);
      }
    }
    public static void NotifyGfxHitTarget(GameObject src, int impactId, GameObject target, int hitCount)
    {
      SharedGameObjectInfo srcInfo = GfxSystem.Instance.GetSharedGameObjectInfo(src);
      SharedGameObjectInfo targetInfo = GfxSystem.Instance.GetSharedGameObjectInfo(target);
      if (null != srcInfo && null != targetInfo) {
        if (null != GfxSystem.Instance.GameLogicNotification) {
          QueueLogicAction(() => { GfxSystem.Instance.GameLogicNotification.OnGfxHitTarget(srcInfo.m_LogicObjectId, impactId, targetInfo.m_LogicObjectId, hitCount); });
        }
      } else {
        GfxLog("NotifyGfxHitTarget:{0} {1} {2} {3}, can't find object !", src.name, impactId, target.name, hitCount);
      }
    }
    public static void NotifyGfxMoveMeetObstacle(GameObject obj)
    {
      SharedGameObjectInfo info = GfxSystem.Instance.GetSharedGameObjectInfo(obj);
      if (null != info) {
        if (null != GfxSystem.Instance.GameLogicNotification) {
          QueueLogicAction(() => { GfxSystem.Instance.GameLogicNotification.OnGfxMoveMeetObstacle(info.m_LogicObjectId); });
        }
      }
    }
  }
}
