using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DashFire
{
  public sealed partial class GfxSystem
  {
    private class GameObjectInfo
    {
      public GameObject ObjectInstance;
      public SharedGameObjectInfo ObjectInfo;

      public GameObjectInfo(GameObject o, SharedGameObjectInfo i)
      {
        ObjectInstance = o;
        ObjectInfo = i;
      }
    }
    //初始化阶段调用的函数
    private void InitImpl()
    {
    }
    private void TickImpl()
    {
      long curTime = TimeUtility.GetLocalMilliseconds();
      if (m_LastLogTime + 10000 < curTime) {
        m_LastLogTime = curTime;

        CallGfxLog(string.Format("GfxSystem.Tick actionNum:{0}", m_GfxInvoker.CurActionNum));
      }
      m_GfxInvoker.HandleActions(4096);
      HandleInput();
      HandleSync();
      HandleLoadingProgress();
      ResourceManager.Instance.Tick();
    }
    private void ReleaseImpl()
    {

    }
    private void SetLogicInvokerImpl(IActionQueue processor)
    {
      m_LogicInvoker = processor;
    }
    private void SetLogicLogCallbackImpl(MyAction<string, object[]> callback)
    {
      m_LogicLogCallback = callback;
    }
    private void SetGameLogicNotificationImpl(IGameLogicNotification notification)
    {
      m_GameLogicNotification = notification;
    }
    //Gfx线程执行的函数，供游戏逻辑线程异步调用
    private void LoadSceneImpl(string name, MyAction onFinish)
    {
      m_TargetScene = name;
      if (null == m_LoadingBarAsyncOperation) {
        m_LoadingBarAsyncOperation = Application.LoadLevelAsync(m_LoadingBarScene);
        m_LevelLoadedCallback = onFinish;

        EventChannelForGfx.Publish("ge_loading_start", "ui");
      }
    }
    private void MarkPlayerSelfImpl(int id)
    {
      GameObjectInfo info = GetGameObjectInfo(id);
      if (null != info) {
        m_PlayerSelf = info;
        if (null != info.ObjectInstance) {
          int layer = LayerMask.NameToLayer("Player");
          if (layer >= 0) {
            info.ObjectInstance.layer = layer;
          }
        }
      }
    }
    private void CreateGameObjectImpl(int id, string resource, SharedGameObjectInfo info)
    {
      if (null != info) {
        try {
          Vector3 pos = new Vector3(info.X, info.Y, info.Z);
          if (!info.IsFloat && null!=Terrain.activeTerrain)
            pos.y = Terrain.activeTerrain.SampleHeight(pos);
          Quaternion q = Quaternion.Euler(0, RadianToDegree(info.FaceDir), 0);
          GameObject obj = ResourceManager.Instance.NewObject(resource) as GameObject;
          if (null != obj) {
            if (null != obj.transform) {
              obj.transform.position = pos;
              obj.transform.localRotation = q;
              obj.transform.localScale = new Vector3(info.Sx, info.Sy, info.Sz);
            }
            RememberGameObject(id, obj, info);
            obj.SetActive(true);
          } else {
            CallGfxLog("CreateGameObject {0} can't load resource", resource);
          }
        } catch (Exception ex) {
          CallGfxLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
        }
      }
    }
    private void CreateGameObjectImpl(int id, string resource, float x, float y, float z, float rx, float ry, float rz)
    {
      try {
          Vector3 pos = new Vector3(x, y, z);
          Quaternion q = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
          GameObject obj = ResourceManager.Instance.NewObject(resource) as GameObject;
          if (null != obj) {
            obj.transform.position = pos;
            obj.transform.localRotation = q;
            RememberGameObject(id, obj);
            obj.SetActive(true);
          } else {
            CallGfxLog("CreateGameObject {0} can't load resource", resource);
          }
      } catch (Exception ex) {
        CallGfxLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
      }
    }
    private void CreateGameObjectForAttachImpl(int id, string resource)
    {
      try {
          GameObject obj = ResourceManager.Instance.NewObject(resource) as GameObject;
          if (null != obj) {
            RememberGameObject(id, obj);
            obj.SetActive(true);
          } else {
            CallGfxLog("CreateGameObject {0} can't load resource", resource);
          }
      } catch (Exception ex) {
        CallGfxLog("CreateGameObject {0} throw exception:{1}\n{2}", resource, ex.Message, ex.StackTrace);
      }
    }
    private void DestroyGameObjectImpl(int id)
    {
      try {
        GameObject obj = GetGameObject(id);
        if (null != obj) {
          ForgetGameObject(id, obj);
          obj.SetActive(false);
          if (!ResourceManager.Instance.RecycleObject(obj)) {
            GameObject.Destroy(obj);
          }
        }
      } catch (Exception ex) {
        CallGfxLog(string.Format("DestroyGameObject:{0} failed:{1}\n{2}", id, ex.Message, ex.StackTrace));
      }
    }
    private void UpdateGameObjectLocalPositionImpl(int id, float x, float y, float z)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj) {
        obj.transform.localPosition = new Vector3(x, y, z);
      }
    }
    private void UpdateGameObjectLocalRotateImpl(int id, float rx, float ry, float rz)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj) {
        obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
      }
    }
    private void UpdateGameObjectLocalScaleImpl(int id, float sx, float sy, float sz)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj) {
        obj.transform.localScale = new Vector3(sx, sy, sz);
      }
    }
    private void AttachGameObjectImpl(int id, int parentId, float x, float y, float z, float rx, float ry, float rz)
    {
      GameObject obj = GetGameObject(id);
      GameObject parent = GetGameObject(parentId);
      if (null != obj && null != obj.transform && null != parent && null != parent.transform) {
        obj.transform.parent = parent.transform;
        obj.transform.localPosition = new Vector3(x, y, z);
        obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
      }
    }
    private void AttachGameObjectImpl(int id, int parentId, string path, float x, float y, float z, float rx, float ry, float rz)
    {
      GameObject obj = GetGameObject(id);
      GameObject parent = GetGameObject(parentId);
      if (null != obj && null != obj.transform && null != parent && null != parent.transform) {
        Transform t = FindChildRecursive(parent.transform, path);
        if (null != t) {
          obj.transform.parent = t;
          obj.transform.localPosition = new Vector3(x, y, z);
          obj.transform.localRotation = Quaternion.Euler(RadianToDegree(rx), RadianToDegree(ry), RadianToDegree(rz));
        } else {
          CallLogicLog("Obj {0} AttachGameObject {1} can't find bone {2}", id, parentId, path);
        }
      }
    }
    private void DetachGameObjectImpl(int id)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.transform) {
        obj.transform.parent = null;
      }
    }
    private void SetGameObjectVisibleImpl(int id, bool visible)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.renderer) {
        obj.renderer.enabled = visible;
      }
    }
    private void PlayAnimationImpl(int id, bool isStopAll)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          obj.animation.Play(isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
        } catch {
        }
      }
    }
    private void PlayAnimationImpl(int id, string animationName, bool isStopAll)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName]) {
            obj.animation.Play(animationName, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
            //CallLogicLog("Obj {0} PlayerAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} PlayerAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void StopAnimationImpl(int id, string animationName)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName]) {
            obj.animation.Stop(animationName);
            //CallLogicLog("Obj {0} StopAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} StopAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void StopAnimationImpl(int id)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          obj.animation.Stop();
        } catch {
        }
      }
    }
    private void BlendAnimationImpl(int id, string animationName, float weight, float fadeLength)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            obj.animation.Blend(animationName, weight, fadeLength);
            //CallLogicLog("Obj {0} BlendAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} BlendAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void CrossFadeAnimationImpl(int id, string animationName, float fadeLength, bool isStopAll)
    {
      GameObject obj = GetGameObject(id);
      SharedGameObjectInfo obj_info = GetSharedGameObjectInfo(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName] && obj_info != null && !obj_info.IsGfxAnimation) {
            obj.animation.CrossFade(animationName, fadeLength, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
            //CallLogicLog("Obj {0} CrossFadeAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} CrossFadeAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void PlayQueuedAnimationImpl(int id, string animationName, bool isPlayNow, bool isStopAll)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName]) {
            obj.animation.PlayQueued(animationName, isPlayNow ? QueueMode.PlayNow : QueueMode.CompleteOthers, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
            //CallLogicLog("Obj {0} PlayQueuedAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} PlayQueuedAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void CrossFadeQueuedAnimationImpl(int id, string animationName, float fadeLength, bool isPlayNow, bool isStopAll)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName]) {
            obj.animation.CrossFadeQueued(animationName, fadeLength, isPlayNow ? QueueMode.PlayNow : QueueMode.CompleteOthers, isStopAll ? PlayMode.StopAll : PlayMode.StopSameLayer);
            //CallLogicLog("Obj {0} CrossFadeQueuedAnimation {1} clipcount {2}", id, animationName, obj.animation.GetClipCount());
          } else {
            CallLogicLog("Obj {0} CrossFadeQueuedAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void RewindAnimationImpl(int id, string animationName)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          if (null != obj.animation[animationName]) {
            obj.animation.Rewind(animationName);
          } else {
            CallLogicLog("Obj {0} RewindAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void RewindAnimationImpl(int id)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          obj.animation.Rewind();
        } catch {
        }
      }
    }
    private void SetAnimationSpeedImpl(int id, string animationName, float speed)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            state.speed = speed;
          } else {
            CallLogicLog("Obj {0} SetAnimationSpeed {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void SetAnimationSpeedByTimeImpl(int id, string animationName, float time)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            state.speed = state.length / state.time;
          } else {
            CallLogicLog("Obj {0} SetAnimationSpeedByTime {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void SetAnimationWeightImpl(int id, string animationName, float weight)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            state.weight = weight;
          } else {
            CallLogicLog("Obj {0} SetAnimationWeight {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void SetAnimationLayerImpl(int id, string animationName, int layer)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            state.layer = layer;
          } else {
            CallLogicLog("Obj {0} SetAnimationLayer {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void SetAnimationBlendModeImpl(int id, string animationName, int blendMode)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            state.blendMode = (AnimationBlendMode)blendMode;
          } else {
            CallLogicLog("Obj {0} SetAnimationBlendMode {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void AddMixingTransformAnimationImpl(int id, string animationName, string path, bool recursive)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation && null != obj.transform) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            Transform t = obj.transform.Find(path);
            if (null != t) {
              state.AddMixingTransform(t, recursive);
            } else {
              CallLogicLog("Obj {0} AddMixingTransformAnimation {1} Can't find bone {2}", id, animationName, path);
            }
          } else {
            CallLogicLog("Obj {0} AddMixingTransformAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void RemoveMixingTransformAnimationImpl(int id, string animationName, string path)
    {
      GameObject obj = GetGameObject(id);
      if (null != obj && null != obj.animation && null != obj.transform) {
        try {
          AnimationState state = obj.animation[animationName];
          if (null != state) {
            Transform t = obj.transform.Find(path);
            if (null != t) {
              state.RemoveMixingTransform(t);
            } else {
              CallLogicLog("Obj {0} RemoveMixingTransformAnimation {1} Can't find bone {2}", id, animationName, path);
            }
          } else {
            CallLogicLog("Obj {0} RemoveMixingTransformAnimation {1} AnimationState is null, clipcount {2}", id, animationName, obj.animation.GetClipCount());
          }
        } catch {
        }
      }
    }
    private void GfxLogImpl(string msg)
    {
      SendMessageImpl("GfxGameRoot", "LogToConsole", msg, false);
      //UnityEngine.Debug.Log(msg);
    }
    private void PublishGfxEventImpl(string evt, string group, object[] args)
    {
      m_EventChannelForGfx.Publish(evt, group, args);
    }
    private void SendMessageImpl(string objname, string msg, object arg, bool needReceiver)
    {
      GameObject obj = GameObject.Find(objname);
      if (null != obj) {
        try {
          obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void SendMessageByIdImpl(int objid, string msg, object arg, bool needReceiver) {
      GameObject obj = GetGameObject(objid);
      if (null != obj) {
        try {
          obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void BroadcastMessageImpl(string objname, string msg, object arg, bool needReceiver)
    {
      GameObject obj = GameObject.Find(objname);
      if (null != obj) {
        try {
          obj.BroadcastMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void BroadcastMessageByIdImpl(int objid, string msg, object arg, bool needReceiver)
    {
      GameObject obj = GetGameObject(objid);
      if (null != obj) {
        try {
          obj.BroadcastMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void SendMessageUpwardsImpl(string objname, string msg, object arg, bool needReceiver)
    {
      GameObject obj = GameObject.Find(objname);
      if (null != obj) {
        try {
          obj.SendMessageUpwards(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void SendMessageUpwardsByIdImpl(int objid, string msg, object arg, bool needReceiver)
    {
      GameObject obj = GetGameObject(objid);
      if (null != obj) {
        try {
          obj.SendMessageUpwards(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
        } catch {

        }
      }
    }
    private void SendMessageWithTagImpl(string objtag, string msg, object arg, bool needReceiver)
    {
      GameObject[] objs = GameObject.FindGameObjectsWithTag(objtag);
      if (null != objs) {
        foreach (GameObject obj in objs) {
          try {
            obj.SendMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
          } catch {
          }
        }
      }
    }
    private void BroadcastMessageWithTagImpl(string objtag, string msg, object arg, bool needReceiver)
    {
      GameObject[] objs = GameObject.FindGameObjectsWithTag(objtag);
      if (null != objs) {
        foreach (GameObject obj in objs) {
          try {
            obj.BroadcastMessage(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
          } catch {
          }
        }
      }
    }
    private void SendMessageUpwardsWithTagImpl(string objtag, string msg, object arg, bool needReceiver)
    {
      GameObject[] objs = GameObject.FindGameObjectsWithTag(objtag);
      if (null != objs) {
        foreach (GameObject obj in objs) {
          try {
            obj.SendMessageUpwards(msg, arg, needReceiver ? SendMessageOptions.RequireReceiver : SendMessageOptions.DontRequireReceiver);
          } catch {
          }
        }
      }
    }
    private void DrawCubeImpl(float x, float y, float z, bool attachTerrain)
    {
      if (attachTerrain && null!=Terrain.activeTerrain) {
        y = Terrain.activeTerrain.SampleHeight(new Vector3(x, 0, z)) + 0.5f;
      }
      GameObject obj = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
      obj.transform.position = new Vector3(x, y, z);
      obj.SetActive(true);

      //CallGfxLog("DrawCube:{0} {1} {2} {3}", x, y, z, attachTerrain);
    }
    //游戏逻辑层执行的函数，供Gfx线程异步调用
    private void PublishLogicEventImpl(string evt, string group, object[] args)
    {
      m_EventChannelForLogic.Publish(evt, group, args);
    }

    //Gfx线程执行的函数，对游戏逻辑线程的异步调用由这里发起
    internal void SetLoadingBarScene(string name)
    {
      m_LoadingBarScene = name;
    }
    internal GameObject GetGameObject(int id)
    {
      GameObject ret = null;
      if (m_GameObjects.ContainsKey(id))
        ret = m_GameObjects[id].ObjectInstance;
      return ret;
    }
    internal SharedGameObjectInfo GetSharedGameObjectInfo(int id)
    {
      SharedGameObjectInfo ret = null;
      if (m_GameObjects.ContainsKey(id))
        ret = m_GameObjects[id].ObjectInfo;
      return ret;
    }
    internal SharedGameObjectInfo GetSharedGameObjectInfo(GameObject obj)
    {
      int id = GetGameObjectId(obj);
      return GetSharedGameObjectInfo(id);
    }
    internal bool ExistGameObject(GameObject obj)
    {
      int id = GetGameObjectId(obj);
      return id > 0;
    }
    internal GameObject PlayerSelf
    {
      get
      {
        if (null != m_PlayerSelf)
          return m_PlayerSelf.ObjectInstance;
        else
          return null;
      }
    }
    internal SharedGameObjectInfo PlayerSelfInfo
    {
      get
      {
        if (null != m_PlayerSelf)
          return m_PlayerSelf.ObjectInfo;
        else
          return null;
      }
    }
    internal void CallLogicLog(string format, params object[] args)
    {
      if (null != m_LogicInvoker && null != m_LogicLogCallback) {
        m_LogicInvoker.QueueAction(() => { m_LogicLogCallback(format, args); });
      }
    }
    internal void CallGfxLog(string format, params object[] args)
    {
      string msg = string.Format(format, args);
      GfxLogImpl(msg);
    }
    internal float RadianToDegree(float dir)
    {
      return (float)(dir * 180 / Math.PI);
    }
    internal float GetLoadingProgress()
    {
      if (null == m_LoadingBarAsyncOperation && null == m_LoadingLevelAsyncOperation)
        return 1;
      else if (null == m_LoadingLevelAsyncOperation)
        return 0;
      else
        return m_LoadingLevelAsyncOperation.progress;
    }
    internal Transform FindChildRecursive(Transform parent, string bonePath)
    {
      Transform t = parent.Find(bonePath);
      if (null != t) {
        return t;
      } else {
        int ct = parent.childCount;
        for (int i = 0; i < ct; ++i) {
          t = FindChildRecursive(parent.GetChild(i), bonePath);
          if (null != t) {
            return t;
          }
        }
      }
      return null;
    }
    internal void QueueLogicAction(MyAction action)
    {
      if (null != m_LogicInvoker) {
        m_LogicInvoker.QueueAction(action);
      }
    }
    internal void PublishLogicEvent(string evt, string group, object[] args)
    {
      if (null != m_LogicInvoker) {
        m_LogicInvoker.QueueAction(PublishLogicEventImpl, evt, group, args);
      }
    }
    internal PublishSubscribeSystem EventChannelForGfx
    {
      get { return m_EventChannelForGfx; }
    }
    internal IGameLogicNotification GameLogicNotification
    {
      get { return m_GameLogicNotification; }
    }
    internal void VisitGameObject(MyAction<GameObject,SharedGameObjectInfo> visitor)
    {
      if (Monitor.TryEnter(m_SyncLock)) {
        try {
          foreach (GameObjectInfo info in m_GameObjects.Values) {
            if (null != info.ObjectInstance) {
              visitor(info.ObjectInstance, info.ObjectInfo);
            }
          }
        } finally {
          Monitor.Exit(m_SyncLock);
        }
      }
    }

    private void HandleSync()
    {
      if (Monitor.TryEnter(m_SyncLock)) {
        try {
          foreach (GameObjectInfo info in m_GameObjects.Values) {
            if (null != info.ObjectInstance && null != info.ObjectInfo) {
              if (info.ObjectInfo.DataChangedByLogic) {
                Vector3 pos = new Vector3(info.ObjectInfo.X, info.ObjectInfo.Y, info.ObjectInfo.Z);
                if (!info.ObjectInfo.IsFloat && null != Terrain.activeTerrain)
                  pos.y = Terrain.activeTerrain.SampleHeight(pos);
                GameObject obj = info.ObjectInstance;
                Vector3 old = obj.transform.position;
                if (!info.ObjectInfo.IsFloat)
                  pos.y = old.y;
                CharacterController ctrl = obj.GetComponent<CharacterController>();
                if (null != ctrl) {
                  if (ctrl.isGrounded) {
                    info.ObjectInfo.VerticlaSpeed = 0.0f;
                  } else {
                    info.ObjectInfo.VerticlaSpeed += -9.8f * Time.deltaTime;
                  }
                  ctrl.Move(pos - old + new Vector3(0, info.ObjectInfo.VerticlaSpeed, 0));
                } else {
                  info.ObjectInstance.transform.position = pos;
                }
                info.ObjectInstance.transform.rotation = Quaternion.Euler(0, RadianToDegree(info.ObjectInfo.FaceDir), 0);

                info.ObjectInfo.DataChangedByLogic = false;
              } else {
                if (!info.ObjectInfo.IsGfxMoveControl) {
                  if (info.ObjectInfo.IsLogicMoving) {
                    GameObject obj = info.ObjectInstance;
                    Vector3 old = obj.transform.position;
                    Vector3 pos;
                    float distance = info.ObjectInfo.MoveSpeed * Time.deltaTime;
                    if (distance * distance < info.ObjectInfo.MoveTargetDistanceSqr) {
                      float dz = distance * info.ObjectInfo.MoveCos;
                      float dx = distance * info.ObjectInfo.MoveSin;

                      CharacterController ctrl = obj.GetComponent<CharacterController>();
                      if (null != ctrl) {
                        if (ctrl.isGrounded) {
                          info.ObjectInfo.VerticlaSpeed = 0.0f;
                        } else {
                          info.ObjectInfo.VerticlaSpeed += -9.8f * Time.deltaTime;
                        }
                        ctrl.Move(new Vector3(dx, 0, dz) + new Vector3(0, info.ObjectInfo.VerticlaSpeed, 0));
                        pos = obj.transform.position;
                      } else {
                        pos = old + new Vector3(dx, 0, dz);
                        if (!info.ObjectInfo.IsFloat && null != Terrain.activeTerrain)
                          pos.y = Terrain.activeTerrain.SampleHeight(pos);
                        info.ObjectInstance.transform.position = pos;
                      }

                      info.ObjectInfo.X = pos.x;
                      info.ObjectInfo.Y = pos.y;
                      info.ObjectInfo.Z = pos.z;
                      info.ObjectInfo.DataChangedByGfx = true;
                    }
                  }
                  info.ObjectInstance.transform.rotation = Quaternion.Euler(RadianToDegree(0), RadianToDegree(info.ObjectInfo.FaceDir), RadianToDegree(0));
                }
              }
            }
          }
        } finally {
          Monitor.Exit(m_SyncLock);
        }
      }
    }
    private void HandleLoadingProgress()
    {
      //先等待loading bar加载完成,发起对目标场景的加载
      if (null != m_LoadingBarAsyncOperation) {
        if (m_LoadingBarAsyncOperation.isDone) {
          m_LoadingBarAsyncOperation = null;

          ResourceManager.Instance.CleanupResourcePool();
          m_LoadingLevelAsyncOperation = Application.LoadLevelAsync(m_TargetScene);
        }
      } else if (null != m_LoadingLevelAsyncOperation && null!=m_LevelLoadedCallback) {//再等待目标场景加载
        if (m_LoadingLevelAsyncOperation.isDone) {
          m_LoadingLevelAsyncOperation = null;
          m_LevelLoadedCallback();
          m_LevelLoadedCallback = null;

          Resources.UnloadUnusedAssets();
        }
      }
    }
    private GameObjectInfo GetGameObjectInfo(int id)
    {
      GameObjectInfo ret = null;
      if (m_GameObjects.ContainsKey(id))
        ret = m_GameObjects[id];
      return ret;
    }
    private int GetGameObjectId(GameObject obj)
    {
      int ret = 0;
      if (m_GameObjectIds.ContainsKey(obj)) {
        ret = m_GameObjectIds[obj];
      }
      return ret;
    }
    private void RememberGameObject(int id, GameObject obj)
    {
      RememberGameObject(id, obj, null);
    }
    private void RememberGameObject(int id, GameObject obj, SharedGameObjectInfo info)
    {
      if (m_GameObjects.ContainsKey(id)) {
        GameObject oldObj = m_GameObjects[id].ObjectInstance;
        oldObj.SetActive(false);
        m_GameObjectIds.Remove(oldObj);
        GameObject.Destroy(oldObj);
        m_GameObjects[id] = new GameObjectInfo(obj, info);
      } else {
        m_GameObjects.Add(id, new GameObjectInfo(obj, info));
      }
      m_GameObjectIds.Add(obj, id);
    }
    private void ForgetGameObject(int id, GameObject obj)
    {
      m_GameObjects.Remove(id);
      m_GameObjectIds.Remove(obj);
    }

    private object m_SyncLock = new object();
    private MyDictionary<int, GameObjectInfo> m_GameObjects = new MyDictionary<int, GameObjectInfo>();
    private MyDictionary<GameObject, int> m_GameObjectIds = new MyDictionary<GameObject, int>();
    private MyAction<string, object[]> m_LogicLogCallback;

    private IActionQueue m_LogicInvoker;
    private AsyncActionProcessor m_GfxInvoker = new AsyncActionProcessor();
    private PublishSubscribeSystem m_EventChannelForLogic = new PublishSubscribeSystem();
    private PublishSubscribeSystem m_EventChannelForGfx = new PublishSubscribeSystem();

    private UnityEngine.AsyncOperation m_LoadingBarAsyncOperation = null;
    private UnityEngine.AsyncOperation m_LoadingLevelAsyncOperation = null;
    private MyAction m_LevelLoadedCallback = null;

    private IGameLogicNotification m_GameLogicNotification = null;
    private GameObjectInfo m_PlayerSelf = null;

    private string m_LoadingBarScene = "";
    private string m_TargetScene = "";

    private long m_LastLogTime = 0;
  }
}
