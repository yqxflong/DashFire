﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public interface IPoolAllocatedObject<T> where T : IPoolAllocatedObject<T>, new()
  {
    void InitPool(ObjectPool<T> pool);
    T Downcast();
  }
  public class ObjectPool<T> where T : IPoolAllocatedObject<T>, new()
  {
    public void Init(int initPoolSize)
    {
      for (int i = 0; i < initPoolSize; ++i) {
        T t = new T();
        t.InitPool(this);
        m_UnusedObjects.Enqueue(t);
      }
    }
    public T Alloc()
    {
      if (m_UnusedObjects.Count > 0)
        return m_UnusedObjects.Dequeue();
      else {
        T t = new T();
        t.InitPool(this);
        return t;
      }
    }
    public void Recycle(IPoolAllocatedObject<T> t)
    {
      if (null != t) {
        m_UnusedObjects.Enqueue(t.Downcast());
      }
    }
    private Queue<T> m_UnusedObjects = new Queue<T>();
  }
}
