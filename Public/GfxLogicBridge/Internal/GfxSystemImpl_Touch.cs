using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace DashFire
{
  public sealed partial class GfxSystem
  {
    private float GetTouchXImpl()
    {
      return m_CurTouchPos.x;
    }
    private float GetTouchYImpl()
    {
      return m_CurTouchPos.y;
    }
    private float GetTouchRayPointXImpl()
    {
      return m_TouchRayPoint.x;
    }
    private float GetTouchRayPointYImpl()
    {
      return m_TouchRayPoint.y;
    }
    private float GetTouchRayPointZImpl()
    {
      return m_TouchRayPoint.z;
    }
    private void ListenTouchEventImpl(TouchEvent c, MyAction<int, GestureArgs> handler)
    {
      if (m_TouchHandlers.ContainsKey((int)c)) {
        m_TouchHandlers[(int)c] = handler;
      } else {
        m_TouchHandlers.Add((int)c, handler);
      }
    }
    private void Fire(int c, GestureArgs e)
    {
      if (null != m_LogicInvoker && m_TouchHandlers.ContainsKey(c)) {
        MyAction<int, GestureArgs> handler = m_TouchHandlers[c];
        m_LogicInvoker.QueueAction(() => { handler(c, e); });
      }
    }

    internal void OnGesture(GestureArgs e)
    {
      if (null != e) {
        m_LastTouchPos = m_CurTouchPos;
        m_CurTouchPos = new Vector3(e.positionX, e.positionY, 0);
        m_TouchRayPoint = new Vector3(e.gamePosX, e.gamePosY, e.gamePosZ);
      }
      Fire((int)TouchEvent.Cesture, e);
    }

    private Vector3 m_LastTouchPos;
    private Vector3 m_CurTouchPos;
    private Vector3 m_TouchRayPoint;

    private MyDictionary<int, MyAction<int, GestureArgs>> m_TouchHandlers = new MyDictionary<int, MyAction<int, GestureArgs>>();
  }
}
