using System;
using System.Collections.Generic;
using UnityEngine;

namespace DashFire
{
  /// <summary>
  /// 资源管理器，提供资源缓存重用机制。
  /// 
  /// todo:分包策略确定后需要修改为从分包里加载资源
  /// </summary>
  internal class ResourceManager
  {
    internal void PreloadResource(string res, int count)
    {
      UnityEngine.Object prefab = GetSharedResource(res);
      PreloadResource(prefab, count);
    }
    internal void PreloadResource(UnityEngine.Object prefab, int count)
    {
      if (null != prefab) {
        if (!m_PreloadResources.Contains(prefab.GetInstanceID()))
          m_PreloadResources.Add(prefab.GetInstanceID());
        for (int i = 0; i < count; ++i) {
          UnityEngine.Object obj = GameObject.Instantiate(prefab);
          AddToUnusedResources(prefab.GetInstanceID(), obj);
        }
      }
    }
    internal void PreloadSharedResource(string res)
    {
      UnityEngine.Object prefab = GetSharedResource(res);
      if (null != prefab) {
        if (!m_PreloadResources.Contains(prefab.GetInstanceID()))
          m_PreloadResources.Add(prefab.GetInstanceID());
      }
    }
    internal UnityEngine.Object NewObject(string res)
    {
      return NewObject(res, 0);
    }
    internal UnityEngine.Object NewObject(string res, float timeToRecycle)
    {
      UnityEngine.Object prefab = GetSharedResource(res);
      return NewObject(prefab, timeToRecycle);
    }
    internal UnityEngine.Object NewObject(UnityEngine.Object prefab)
    {
      return NewObject(prefab, 0);
    }
    internal UnityEngine.Object NewObject(UnityEngine.Object prefab, float timeToRecycle)
    {
      UnityEngine.Object obj = null;
      if (null != prefab) {
        float curTime = Time.time;
        float time = timeToRecycle;
        if (timeToRecycle > 0)
          time += curTime;
        int resId = prefab.GetInstanceID();
        obj = NewFromUnusedResources(resId);
        if (null == obj) {
          obj = GameObject.Instantiate(prefab);
        }
        if (null != obj) {
          AddToUsedResources(resId, obj, time);

          InitializeObject(obj);
        }
      }
      return obj;
    }
    internal bool RecycleObject(UnityEngine.Object obj)
    {
      bool ret = false;
      int resId = FindUsedResource(obj);
      if (resId != 0) {
        FinalizeObject(obj);
        RemoveFromUsedResources(resId, obj);
        AddToUnusedResources(resId, obj);
        ret = true;
      }
      return ret;
    }
    internal void Tick()
    {
      float curTime = Time.time;
      /*
      if (m_LastTickTime <= 0) {
        m_LastTickTime = curTime;
        return;
      }
      float delta = curTime - m_LastTickTime;
      if (delta < 0.1f) {
        return;
      }
      m_LastTickTime = curTime;
      */
      foreach (int key in m_UsedResources.Keys) {
        Dictionary<UnityEngine.Object, float> resources = m_UsedResources[key];
        foreach (KeyValuePair<UnityEngine.Object, float> pair in resources) {
          if (pair.Value > 0 && pair.Value < curTime) {
            m_WaitDeleteResourceEntrys.Add(pair.Key);
          }
        }
        foreach (UnityEngine.Object obj in m_WaitDeleteResourceEntrys) {
          FinalizeObject(obj);
          resources.Remove(obj);
          AddToUnusedResources(key, obj);
        }
        m_WaitDeleteResourceEntrys.Clear();
      }
    }
    internal UnityEngine.Object GetSharedResource(string res)
    {
      UnityEngine.Object obj = null;
      if (m_LoadedPrefabs.ContainsKey(res)) {
        obj = m_LoadedPrefabs[res];
      } else {
        UnityEngine.Debug.Log(string.Format("Resources.Load {0}", res));
        obj = Resources.Load(res);
        m_LoadedPrefabs.Add(res, obj);
      }
      return obj;
    }
    internal void CleanupResourcePool()
    {
      foreach (int key in m_UsedResources.Keys) {
        if (m_PreloadResources.Contains(key))
          continue;
        Dictionary<UnityEngine.Object, float> resources = m_UsedResources[key];
        resources.Clear();
      }

      foreach (int key in m_UnusedResources.Keys) {
        if (m_PreloadResources.Contains(key))
          continue;
        Queue<UnityEngine.Object> queue = m_UnusedResources[key];
        queue.Clear();
      }

      foreach (string key in m_LoadedPrefabs.Keys) {
        UnityEngine.Object obj = m_LoadedPrefabs[key];
        if (null != obj) {
          try {
            int instId = obj.GetInstanceID();
            if (!m_PreloadResources.Contains(instId)) {
              m_WaitDeleteLoadedPrefabEntrys.Add(key);
            }
          } catch(Exception ex) {
            m_WaitDeleteLoadedPrefabEntrys.Add(key);
            LogSystem.Error("Exception:{0} stack:{1}", ex.Message, ex.StackTrace);
          }
        } else {
          m_WaitDeleteLoadedPrefabEntrys.Add(key);
        }
      }
      foreach (string key in m_WaitDeleteLoadedPrefabEntrys) {
        m_LoadedPrefabs.Remove(key);
      }
      m_WaitDeleteLoadedPrefabEntrys.Clear();

      Resources.UnloadUnusedAssets();
    }

    private UnityEngine.Object NewFromUnusedResources(int res)
    {
      UnityEngine.Object obj = null;
      if (m_UnusedResources.ContainsKey(res)) {
        Queue<UnityEngine.Object> queue = m_UnusedResources[res];
        if (queue.Count > 0)
          obj = queue.Dequeue();
      }
      return obj;
    }
    private void AddToUnusedResources(int res, UnityEngine.Object obj)
    {
      if (m_UnusedResources.ContainsKey(res)) {
        Queue<UnityEngine.Object> queue = m_UnusedResources[res];
        queue.Enqueue(obj);
      } else {
        Queue<UnityEngine.Object> queue = new Queue<UnityEngine.Object>();
        queue.Enqueue(obj);
        m_UnusedResources.Add(res, queue);
      }
    }
    private int FindUsedResource(UnityEngine.Object obj)
    {
      foreach (int key in m_UsedResources.Keys) {
        Dictionary<UnityEngine.Object, float> resources = m_UsedResources[key];
        if (resources.ContainsKey(obj)) {
          return key;
        }
      }
      return 0;
    }
    private void AddToUsedResources(int resId, UnityEngine.Object obj, float recycleTime)
    {
      if (m_UsedResources.ContainsKey(resId)) {
        Dictionary<UnityEngine.Object, float> resources = m_UsedResources[resId];
        resources.Add(obj, recycleTime);
      } else {
        Dictionary<UnityEngine.Object, float> resources = new Dictionary<UnityEngine.Object,float>();
        resources.Add(obj, recycleTime);
        m_UsedResources.Add(resId, resources);
      }
    }
    private void RemoveFromUsedResources(int resId, UnityEngine.Object obj)
    {
      if (m_UsedResources.ContainsKey(resId)) {
        Dictionary<UnityEngine.Object, float> resources = m_UsedResources[resId];
        resources.Remove(obj);
      }
    }

    private void InitializeObject(UnityEngine.Object obj)
    {
      GameObject gameObj = obj as GameObject;
      if (null != gameObj) {
        if (!gameObj.activeSelf)
          gameObj.SetActive(true);
        /*ParticleSystem[] pss = gameObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          if (null != ps && ps.playOnAwake) {
            ps.Play();
          }
        }*/
        ParticleSystem ps = gameObj.GetComponent<ParticleSystem>();
        if (null != ps && ps.playOnAwake) {
          ps.Play();
        }
      }
    }
    private void FinalizeObject(UnityEngine.Object obj)
    {
      GameObject gameObj = obj as GameObject;
      if (null != gameObj) {
        ParticleSystem ps0 = gameObj.GetComponent<ParticleSystem>();
        if (null != ps0 && ps0.playOnAwake) {
          ps0.Stop();
        }
        ParticleSystem[] pss = gameObj.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in pss) {
          if (null != ps) {
            ps.Clear();
          }
        }
        if (null != gameObj.transform.parent)
          gameObj.transform.parent = null;
        if(gameObj.activeSelf)
          gameObj.SetActive(false);
      }
    }

    private HashSet<int> m_PreloadResources = new HashSet<int>();
    private Dictionary<string, UnityEngine.Object> m_LoadedPrefabs = new Dictionary<string, UnityEngine.Object>();
    private List<string> m_WaitDeleteLoadedPrefabEntrys = new List<string>();

    private Dictionary<int, Dictionary<UnityEngine.Object, float>> m_UsedResources = new Dictionary<int, Dictionary<UnityEngine.Object, float>>();
    private Dictionary<int, Queue<UnityEngine.Object>> m_UnusedResources = new Dictionary<int, Queue<UnityEngine.Object>>();
    private List<UnityEngine.Object> m_WaitDeleteResourceEntrys = new List<UnityEngine.Object>();
    private float m_LastTickTime = 0;
    
    public static ResourceManager Instance
    {
      get { return s_Instance; }
    }
    private static ResourceManager s_Instance = new ResourceManager();
  }
}
