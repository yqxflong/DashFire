﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  /// <summary>
  /// 我们频繁用到增加与删除少，但根据id查询对象与遍历对象都很多的情形。这个类提供对这种应用场景具有较好性能的容器实现。
  /// </summary>
  /// <typeparam name="KeyT"></typeparam>
  /// <typeparam name="ValueT"></typeparam>
  /// <remarks>
  /// 频繁使用时需要注意foreach语句的开销，本类不期望用于foreach语句遍历(保留是为了兼容现有代码，逐渐会替换掉)。另，本类不提供id遍历。
  /// </remarks>
  public sealed class LinkedListDictionary<TKey,TValue>
  {
    public bool Contains(TKey id)
    {
      return m_LinkNodeDictionary.ContainsKey(id);
    }
    ///这里不考虑重复，外界调用时保证（性能考虑）
    public void Add(TKey id, TValue obj)
    {
      LinkedListNode<TValue> linkNode = m_Objects.AddLast(obj);
      if (null != linkNode) {
        m_LinkNodeDictionary.Add(id, linkNode);
      }
    }
    public void Remove(TKey id)
    {
      if (m_LinkNodeDictionary.ContainsKey(id)) {
        LinkedListNode<TValue> linkNode = m_LinkNodeDictionary[id];
        m_LinkNodeDictionary.Remove(id);
        try { m_Objects.Remove(linkNode); } catch (Exception) { }
      }
    }
    public void Clear()
    {
      m_LinkNodeDictionary.Clear();
      m_Objects.Clear();
    }
    public bool TryGetValue(TKey id, out TValue value)
    {
      LinkedListNode<TValue> linkNode;
      bool ret = m_LinkNodeDictionary.TryGetValue(id, out linkNode);
      if (ret) {
        value = linkNode.Value;
      } else {
        value = default(TValue);
      }
      return ret;
    }
    public void VisitValues(MyAction<TValue> visitor)
    {
      for (LinkedListNode<TValue> linkNode = m_Objects.First; null != linkNode; linkNode = linkNode.Next) {
        visitor(linkNode.Value);
      }
    }
    public void VisitValues(MyFunc<TValue, bool> visitor)
    {
      for (LinkedListNode<TValue> linkNode = m_Objects.First; null != linkNode; linkNode = linkNode.Next) {
        if(!visitor(linkNode.Value))
          break;
      }
    }
    public TValue FindValue(MyFunc<TValue, bool> visitor)
    {
      for (LinkedListNode<TValue> linkNode = m_Objects.First; null != linkNode; linkNode = linkNode.Next) {
        if (visitor(linkNode.Value)) {
          return linkNode.Value;
        }
      }
      return default(TValue);
    }
    public int Count
    {
      get
      {
        return m_LinkNodeDictionary.Count;
      }
    }
    public TValue this[TKey id]
    {
      get
      {
        TValue ret;
        if (Contains(id)) {
          LinkedListNode<TValue> linkNode = m_LinkNodeDictionary[id];
          ret = linkNode.Value;
        } else {
          ret = default(TValue);
        }
        return ret;
      }
      set
      {
        if (Contains(id)) {
          LinkedListNode<TValue> linkNode = m_LinkNodeDictionary[id];
          linkNode.Value = value;
        } else {
          Add(id, value);
        }
      }
    }
    public LinkedListNode<TValue> FirstValue
    {
      get
      {
        return m_Objects.First;
      }
    }
    public LinkedListNode<TValue> LastValue
    {
      get
      {
        return m_Objects.Last;
      }
    }
    public void CopyValuesTo(TValue[] array,int index)
    {
      m_Objects.CopyTo(array, index);
    }
    /*
    [Obsolete("请不要使用这个特性，目前提供仅为了兼容现有代码。请使用FirstValue/LastValue特性来遍历值链表，或用VisitValues或FindValue方法遍历或查找值。")]
    public IEnumerable<TValue> Values
    {
      get
      {
        return m_Objects;
      }
    }*/

    private MyDictionary<TKey, LinkedListNode<TValue>> m_LinkNodeDictionary = new MyDictionary<TKey, LinkedListNode<TValue>>();
    private LinkedList<TValue> m_Objects = new LinkedList<TValue>();
  }
}
