﻿using System;
using System.Collections;
using System.Linq;
using System.Text;

namespace DashFire
{
  /// <remarks>
  /// 这种方式主要用于数据层面不好抽象的场合，如果从业务角度能够对数据定义进行抽象，就尽量不要用这种动态的聚合数据的方式而尽量使用
  /// 静态类型的关联。
  /// 这个类仅用于管理若干不同类型的数据，如之前C++里用union管理不同数据的情形，使用限制：
  /// 1、这个集合里每种类型的数据只能有一个实例。
  /// 2、由于使用运行时类型信息及非强类型集合，运行效率较低。
  /// </remarks>
  public sealed class TypedDataCollection
  {
    public void AddData<T>(T data)
    {
      Type t = typeof(T);
      if (null != data) {
        if (m_AiDatas.Contains(t)) {
          m_AiDatas[t] = data;
        } else {
          m_AiDatas.Add(t, data);
        }
      }
    }
    public void RemoveData<T>()
    {
      Type t=typeof(T);
      if (m_AiDatas.Contains(t)) {
        m_AiDatas.Remove(t);
      }
    }
    public void Clear()
    {
      m_AiDatas.Clear();
    }
    public T GetData<T>()
    {
      T ret = default(T);
      Type t = typeof(T);
      if (m_AiDatas.Contains(t)) {
        ret = (T)m_AiDatas[t];
      }
      return ret;
    }
    private Hashtable m_AiDatas = new Hashtable();
  }
}
