using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DashFire
{
  public class AsyncActionProcessor : IActionQueue
  {
    public int CurActionNum
    {
      get
      {
        return m_Actions.Count;
      }
    }

    public void QueueAction(MyAction action)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(action);
      }
    }

    public void QueueAction<T1>(MyAction<T1> action, T1 t1)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1); });
      }
    }

    public void QueueAction<T1, T2>(MyAction<T1, T2> action, T1 t1, T2 t2)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2); });
      }
    }

    public void QueueAction<T1, T2, T3>(MyAction<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3); });
      }
    }

    public void QueueAction<T1, T2, T3, T4>(MyAction<T1, T2, T3, T4> action, T1 t1, T2 t2, T3 t3, T4 t4)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5>(MyAction<T1, T2, T3, T4, T5> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6>(MyAction<T1, T2, T3, T4, T5, T6> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7>(MyAction<T1, T2, T3, T4, T5, T6, T7> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15); });
      }
    }

    public void QueueAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16)
    {
      lock (m_Lock) {
        m_Actions.Enqueue(() => { action(t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16); });
      }
    }

    public void HandleActions(int maxCount)
    {
      try {
        for (int i = 0; i < maxCount; ++i) {
          if (m_Actions.Count > 0) {
            MyAction action = null;
            lock (m_Lock) {
              action = m_Actions.Dequeue();
            }
            if (null != action)
              action();
          } else {
            break;
          }
        }
      } catch (Exception ex) {
        LogSystem.Error("AsyncActionProcessor.HandleActions throw exception:{0}\n{1}", ex.Message, ex.StackTrace);
      }
    }

    private object m_Lock = new object();
    private Queue<MyAction> m_Actions = new Queue<MyAction>();
  }
}
