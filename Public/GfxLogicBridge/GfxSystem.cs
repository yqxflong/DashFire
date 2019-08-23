using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public sealed partial class GfxSystem
  {
    //引擎线程调用的方法，不要在逻辑线程调用
    public static void Init()
    {
      s_Instance.InitImpl();
    }
    public static void Release()
    {
      s_Instance.ReleaseImpl();
    }
    public static void Tick()
    {
      s_Instance.TickImpl();
    }
    //注册异步处理接口（这个是渲染线程向逻辑线程返回信息的底层机制：逻辑线程向渲染线程注册处理，渲染线程完成请求后将此处理发回逻辑线程执行）
    public static void SetLogicInvoker(IActionQueue processor)
    {
      s_Instance.SetLogicInvokerImpl(processor);
    }
    public static void SetLogicLogCallback(MyAction<string, object[]> callback)
    {
      s_Instance.SetLogicLogCallbackImpl(callback);
    }
    public static void SetGameLogicNotification(IGameLogicNotification notification)
    {
      s_Instance.SetGameLogicNotificationImpl(notification);
    }
    //输入状态，允许跨线程读取
    public static float GetMouseX()
    {
      return s_Instance.GetMouseXImpl();
    }
    public static float GetMouseY()
    {
      return s_Instance.GetMouseYImpl();
    }
    public static float GetMouseZ()
    {
      return s_Instance.GetMouseZImpl();
    }
    public static float GetMouseRayPointX()
    {
      return s_Instance.GetMouseRayPointXImpl();
    }
    public static float GetMouseRayPointY()
    {
      return s_Instance.GetMouseRayPointYImpl();
    }
    public static float GetMouseRayPointZ()
    {
      return s_Instance.GetMouseRayPointZImpl();
    }
    public static float GetTouchRayPointX()
    {
      return s_Instance.GetTouchRayPointXImpl();
    }
    public static float GetTouchRayPointY()
    {
      return s_Instance.GetTouchRayPointYImpl();
    }
    public static float GetTouchRayPointZ()
    {
      return s_Instance.GetTouchRayPointZImpl();
    }
    public static bool IsButtonPressed(Mouse.Code c)
    {
      return s_Instance.IsButtonPressedImpl(c);
    }
    public static bool IsKeyPressed(Keyboard.Code c)
    {
      return s_Instance.IsKeyPressedImpl(c);
    }
    //指出需要查询状态与处理事件的键列表（仅在初始化时调用【第一次tick前】，一般是初始化时，可多次设置不同的键）
    public static void ListenKeyPressState(params Keyboard.Code[] keys)
    {
      s_Instance.ListenKeyPressStateImpl(keys);
    }
    //事件注册,事件处理会通过IActionQueue在游戏逻辑线程处理（仅在初始化时调用【第一次tick前】)
    public static void ListenKeyboardEvent(Keyboard.Code c, MyAction<int, int> handler)
    {
      s_Instance.ListenKeyboardEventImpl(c, handler);
    }
    public static void ListenMouseEvent(Mouse.Code c, MyAction<int, int> handler)
    {
      s_Instance.ListenMouseEventImpl(c, handler);
    }
    public static void ListenTouchEvent(TouchEvent c, MyAction<int, GestureArgs> handler)
    {
      s_Instance.ListenTouchEventImpl(c, handler);
    }
    //供逻辑线程调用的异步命令
    public static void LoadScene(string sceneName, MyAction onFinish)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.LoadSceneImpl, sceneName, onFinish);
    }
    public static void MarkPlayerSelf(int id)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.MarkPlayerSelfImpl, id);
    }
    public static void CreateGameObject(int id, string resource, SharedGameObjectInfo info)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.CreateGameObjectImpl, id, resource, info);
    }
    public static void CreateGameObject(int id, string resource, float x, float y, float z, float rx, float ry, float rz)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.CreateGameObjectImpl, id, resource, x, y, z, rx, ry, rz);
    }
    public static void CreateGameObjectForAttach(int id, string resource)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.CreateGameObjectForAttachImpl, id, resource);
    }
    public static void DestroyGameObject(int id)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.DestroyGameObjectImpl, id);
    }
    public static object SyncLock
    {
      get
      {
        return s_Instance.m_SyncLock;
      }
    }
    public static void UpdateGameObjectLocalPosition(int id, float x, float y, float z)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.UpdateGameObjectLocalPositionImpl, id, x, y, z);
    }
    public static void UpdateGameObjectLocalRotate(int id, float rx, float ry, float rz)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.UpdateGameObjectLocalRotateImpl, id, rx, ry, rz);
    }
    public static void UpdateGameObjectLocalScale(int id, float sx, float sy, float sz)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.UpdateGameObjectLocalScaleImpl, id, sx, sy, sz);
    }
    public static void AttachGameObject(int id, int parentId)
    {
      AttachGameObject(id, parentId, 0, 0, 0, 0, 0, 0);
    }
    public static void AttachGameObject(int id, int parentId, float x, float y, float z, float rx, float ry, float rz)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.AttachGameObjectImpl, id, parentId, x, y, z, rx, ry, rz);
    }
    public static void AttachGameObject(int id, int parentId, string path)
    {
      AttachGameObject(id, parentId, path, 0, 0, 0, 0, 0, 0);
    }
    public static void AttachGameObject(int id, int parentId, string path, float x, float y, float z, float rx, float ry, float rz)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.AttachGameObjectImpl, id, parentId, path, x, y, z, rx, ry, rz);
    }
    public static void DetachGameObject(int id)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.DetachGameObjectImpl, id);
    }
    public static void SetGameObjectVisible(int id, bool visible)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetGameObjectVisibleImpl, id, visible);
    }
    public static void PlayAnimation(int id)
    {
      PlayAnimation(id, false);
    }
    public static void PlayAnimation(int id, bool isStopAll)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.PlayAnimationImpl, id, isStopAll);
    }
    public static void PlayAnimation(int id, string animationName)
    {
      PlayAnimation(id, animationName, false);
    }
    public static void PlayAnimation(int id, string animationName, bool isStopAll)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.PlayAnimationImpl, id, animationName, isStopAll);
    }
    public static void StopAnimation(int id, string animationName)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.StopAnimationImpl, id, animationName);
    }
    public static void StopAnimation(int id)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.StopAnimationImpl, id);
    }
    public static void BlendAnimation(int id, string animationName)
    {
      BlendAnimation(id, animationName, 1, 0.3f);
    }
    public static void BlendAnimation(int id, string animationName, float weight)
    {
      BlendAnimation(id, animationName, weight, 0.3f);
    }
    public static void BlendAnimation(int id, string animationName, float weight, float fadeLength)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.BlendAnimationImpl, id, animationName, weight, fadeLength);
    }
    public static void CrossFadeAnimation(int id, string animationName)
    {
      CrossFadeAnimation(id, animationName, 0.3f, false);
    }
    public static void CrossFadeAnimation(int id, string animationName, float fadeLength)
    {
      CrossFadeAnimation(id, animationName, fadeLength, false);
    }
    public static void CrossFadeAnimation(int id, string animationName, float fadeLength, bool isStopAll)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.CrossFadeAnimationImpl, id, animationName, fadeLength, isStopAll);
    }
    public static void PlayQueuedAnimation(int id, string animationName)
    {
      PlayQueuedAnimation(id, animationName, false, false);
    }
    public static void PlayQueuedAnimation(int id, string animationName, bool isPlayNow)
    {
      PlayQueuedAnimation(id, animationName, isPlayNow, false);
    }
    public static void PlayQueuedAnimation(int id, string animationName, bool isPlayNow, bool isStopAll)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.PlayQueuedAnimationImpl, id, animationName, isPlayNow, isStopAll);
    }
    public static void CrossFadeQueuedAnimation(int id, string animationName)
    {
      CrossFadeQueuedAnimation(id, animationName, 0.3f, false, false);
    }
    public static void CrossFadeQueuedAnimation(int id, string animationName, float fadeLength)
    {
      CrossFadeQueuedAnimation(id, animationName, fadeLength, false, false);
    }
    public static void CrossFadeQueuedAnimation(int id, string animationName, float fadeLength, bool isPlayNow)
    {
      CrossFadeQueuedAnimation(id, animationName, fadeLength, isPlayNow, false);
    }
    public static void CrossFadeQueuedAnimation(int id, string animationName, float fadeLength, bool isPlayNow, bool isStopAll)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.CrossFadeQueuedAnimationImpl, id, animationName, fadeLength, isPlayNow, isStopAll);
    }
    public static void RewindAnimation(int id, string animationName)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.RewindAnimationImpl, id, animationName);
    }
    public static void RewindAnimation(int id)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.RewindAnimationImpl, id);
    }
    public static void SetAnimationSpeed(int id, string animationName, float speed)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetAnimationSpeedImpl, id, animationName, speed);
    }
    public static void SetAnimationSpeedByTime(int id, string animationName, float time)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetAnimationSpeedByTimeImpl, id, animationName, time);
    }
    public static void SetAnimationWeight(int id, string animationName, float weight)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetAnimationWeightImpl, id, animationName, weight);
    }
    public static void SetAnimationLayer(int id, string animationName, int layer)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetAnimationLayerImpl, id, animationName, layer);
    }
    public static void SetAnimationBlendMode(int id, string animationName, int blendMode)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SetAnimationBlendModeImpl, id, animationName, blendMode);
    }
    public static void AddMixingTransformAnimation(int id, string animationName, string path)
    {
      AddMixingTransformAnimation(id, animationName, path, true);
    }
    public static void AddMixingTransformAnimation(int id, string animationName, string path, bool recursive)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.AddMixingTransformAnimationImpl, id, animationName, path, recursive);
    }
    public static void RemoveMixingTransformAnimation(int id, string animationName, string path)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.RemoveMixingTransformAnimationImpl, id, animationName, path);
    }
    //日志
    public static void GfxLog(string format, params object[] args)
    {
      string msg = string.Format(format, args);
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.GfxLogImpl, msg);
    }
    //逻辑层与unity3d脚本交互函数
    public static void QueueGfxAction(MyAction action)
    {
      s_Instance.m_GfxInvoker.QueueAction(action);
    }
    public static void PublishGfxEvent(string evt, string group, params object[] args)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.PublishGfxEventImpl, evt, group, args);
    }
    public static PublishSubscribeSystem EventChannelForLogic
    {
      get { return s_Instance.m_EventChannelForLogic; }
    }
    public static void SendMessage(string objname, string msg, object arg)
    {
      SendMessage(objname, msg, arg, false);
    }
    public static void SendMessage(string objname, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageImpl, objname, msg, arg, needReceiver);
    }
    public static void BroadcastMessage(string objname, string msg, object arg)
    {
      BroadcastMessage(objname, msg, arg, false);
    }
    public static void BroadcastMessage(string objname, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.BroadcastMessageImpl, objname, msg, arg, needReceiver);
    }
    public static void SendMessageUpwards(string objname, string msg, object arg)
    {
      SendMessageUpwards(objname, msg, arg, false);
    }
    public static void SendMessageUpwards(string objname, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageUpwardsImpl, objname, msg, arg, needReceiver);
    }
    public static void SendMessage(int objid, string msg, object arg)
    {
      SendMessage(objid, msg, arg, false);
    }
    public static void SendMessage(int objid, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageByIdImpl, objid, msg, arg, needReceiver);
    }
    public static void BroadcastMessage(int objid, string msg, object arg)
    {
      BroadcastMessage(objid, msg, arg, false);
    }
    public static void BroadcastMessage(int objid, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.BroadcastMessageByIdImpl, objid, msg, arg, needReceiver);
    }
    public static void SendMessageUpwards(int objid, string msg, object arg)
    {
      SendMessageUpwards(objid, msg, arg, false);
    }
    public static void SendMessageUpwards(int objid, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageUpwardsByIdImpl, objid, msg, arg, needReceiver);
    }
    public static void SendMessageWithTag(string objtag, string msg, object arg)
    {
      SendMessageWithTag(objtag, msg, arg, false);
    }
    public static void SendMessageWithTag(string objtag, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageWithTagImpl, objtag, msg, arg, needReceiver);
    }
    public static void BroadcastMessageWithTag(string objtag, string msg, object arg)
    {
      BroadcastMessageWithTag(objtag, msg, arg, false);
    }
    public static void BroadcastMessageWithTag(string objtag, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.BroadcastMessageWithTagImpl, objtag, msg, arg, needReceiver);
    }
    public static void SendMessageUpwardsWithTag(string objtag, string msg, object arg)
    {
      SendMessageUpwardsWithTag(objtag, msg, arg, false);
    }
    public static void SendMessageUpwardsWithTag(string objtag, string msg, object arg, bool needReceiver)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.SendMessageUpwardsWithTagImpl, objtag, msg, arg, needReceiver);
    }
    public static void DrawCube(float x, float y, float z, bool attachTerrain)
    {
      s_Instance.m_GfxInvoker.QueueAction(s_Instance.DrawCubeImpl, x, y, z, attachTerrain);
    }

    internal static GfxSystem Instance
    {
      get
      {
        return s_Instance;
      }
    }
    private static GfxSystem s_Instance = new GfxSystem();
  }
}
