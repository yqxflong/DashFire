using System;

namespace DashFire
{
  public class MyTaskDispatcher
  {
    public void DispatchAction (MyAction action)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action);
    }

    public void DispatchAction<T1> (MyAction<T1> action, T1 t1)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1);
    }

    public void DispatchAction<T1,T2> (MyAction<T1,T2> action, T1 t1,T2 t2)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2);
    }

    public void DispatchAction<T1,T2,T3> (MyAction<T1,T2,T3> action, T1 t1,T2 t2,T3 t3)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3);
    }

    public void DispatchAction<T1,T2,T3,T4> (MyAction<T1,T2,T3,T4> action, T1 t1,T2 t2,T3 t3,T4 t4)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4);
    }

    public void DispatchAction<T1,T2,T3,T4,T5> (MyAction<T1,T2,T3,T4,T5> action, T1 t1,T2 t2,T3 t3,T4 t4,T5 t5)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5);
    }

    public void DispatchAction<T1,T2,T3,T4,T5,T6> (MyAction<T1,T2,T3,T4,T5,T6> action, T1 t1,T2 t2,T3 t3,T4 t4,T5 t5,T6 t6)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6);
    }

    public void DispatchAction<T1,T2,T3,T4,T5,T6,T7> (MyAction<T1,T2,T3,T4,T5,T6,T7> action, T1 t1,T2 t2,T3 t3,T4 t4,T5 t5,T6 t6,T7 t7)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7);
    }

    public void DispatchAction<T1,T2,T3,T4,T5,T6,T7,T8> (MyAction<T1,T2,T3,T4,T5,T6,T7,T8> action, T1 t1,T2 t2,T3 t3,T4 t4,T5 t5,T6 t6,T7 t7,T8 t8)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15);
    }

    public void DispatchAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(MyAction<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> action, T1 t1, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9, T10 t10, T11 t11, T12 t12, T13 t13, T14 t14, T15 t15, T16 t16)
    {
      MyThread thread = GetIdlestTaskThread();
      thread.QueueAction(action, t1, t2, t3, t4, t5, t6, t7, t8, t9, t10, t11, t12, t13, t14, t15, t16);
    }

    public MyTaskDispatcher()
    {
      InitTaskThreads(m_ThreadNum);
    }
    public MyTaskDispatcher(int threadNum)
    {
      if (threadNum < 1)
        threadNum = 1;
      m_ThreadNum = threadNum;
      InitTaskThreads(m_ThreadNum);
    }
    ~MyTaskDispatcher()
    {
      StopTaskThreads();
    }
    private void InitTaskThreads(int threadNum)
    {
      m_Threads = new MyThread[threadNum];
      for (int i = 0; i < threadNum; ++i) {
        MyThread thread = new MyThread();
        m_Threads[i] = thread;
        thread.Start();
      }
    }
    private void StopTaskThreads()
    {
      for (int i = 0; i < m_ThreadNum; ++i) {
        m_Threads[i].Stop();
      }
    }
    private MyThread GetIdlestTaskThread()
    {
      int index = 0;
      int count = int.MaxValue;
      for (int i = 0; i < m_ThreadNum; ++i) {
        int num=m_Threads[i].CurActionNum;
        if (count > num) {
          count = num;
          index = i;
        }
      }
      return m_Threads[index];
    }

    private MyThread[] m_Threads;
    private int m_ThreadNum = c_DefaultThreadNum;

    private const int c_DefaultThreadNum = 8;
  }
}

